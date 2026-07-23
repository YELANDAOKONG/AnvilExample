using Anvil.Instructions;

namespace AnvilExample;

public static class ControlFlowGraphBuilder
{
    public static ControlFlowGraph Build(MethodBody body, int codeLength)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (body.Instructions.Count == 0)
        {
            throw new ArgumentException(
                "A control-flow graph requires at least one instruction.",
                nameof(body));
        }

        if (codeLength <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(codeLength),
                codeLength,
                "Code length must be positive.");
        }

        body.ResolveLabels();

        var instructions = body.Instructions;
        var offsetToIndex = instructions
            .Select((instruction, index) => (
                Offset: GetInstructionOffset(instruction),
                Index: index))
            .ToDictionary(pair => pair.Offset, pair => pair.Index);
        var labelOffsets = BuildLabelOffsets(instructions);
        var leaders = new HashSet<int>
        {
            GetInstructionOffset(instructions[0])
        };

        for (var index = 0; index < instructions.Count; index++)
        {
            var instruction = instructions[index];
            AddControlFlowLeaders(instruction, leaders, labelOffsets);

            if (EndsBasicBlock(instruction) && index + 1 < instructions.Count)
            {
                leaders.Add(GetInstructionOffset(instructions[index + 1]));
            }
        }

        foreach (var block in body.TryCatchBlocks)
        {
            AddLeader(block.Start, leaders, labelOffsets, body, codeLength);
            AddLeader(block.End, leaders, labelOffsets, body, codeLength);
            AddLeader(block.Handler, leaders, labelOffsets, body, codeLength);
        }

        var blocks = BuildBlocks(
            instructions,
            leaders,
            offsetToIndex,
            codeLength);
        var blockByOffset = blocks
            .SelectMany(block => block.Instructions.Select(instruction => (
                Offset: GetInstructionOffset(instruction),
                Block: block)))
            .ToDictionary(pair => pair.Offset, pair => pair.Block);
        var edges = BuildNormalEdges(blocks, blockByOffset, labelOffsets);
        AddExceptionEdges(
            body,
            blocks,
            edges,
            blockByOffset,
            labelOffsets,
            codeLength);
        MarkReachableBlocks(blocks[0], edges);

        return new ControlFlowGraph(blocks, edges);
    }

    private static Dictionary<string, int> BuildLabelOffsets(
        IEnumerable<Instruction> instructions)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var instruction in instructions)
        {
            var offset = GetInstructionOffset(instruction);
            foreach (var label in instruction.Labels)
            {
                if (result.TryGetValue(label.Name, out var existingOffset)
                    && existingOffset != offset)
                {
                    throw new InvalidOperationException(
                        $"Label '{label.Name}' identifies both offset "
                        + $"{existingOffset} and {offset}.");
                }

                result[label.Name] = offset;
            }
        }

        return result;
    }

    private static void AddControlFlowLeaders(
        Instruction instruction,
        HashSet<int> leaders,
        IReadOnlyDictionary<string, int> labelOffsets)
    {
        switch (instruction)
        {
            case JumpInstruction jump:
                leaders.Add(GetLabelOffset(jump.Target, labelOffsets));
                break;

            case TableSwitchInstruction tableSwitch:
                leaders.Add(GetLabelOffset(tableSwitch.DefaultTarget, labelOffsets));
                foreach (var target in tableSwitch.Targets)
                {
                    leaders.Add(GetLabelOffset(target, labelOffsets));
                }

                break;

            case LookupSwitchInstruction lookupSwitch:
                leaders.Add(GetLabelOffset(lookupSwitch.DefaultTarget, labelOffsets));
                foreach (var (_, target) in lookupSwitch.Pairs)
                {
                    leaders.Add(GetLabelOffset(target, labelOffsets));
                }

                break;

            default:
                break;
        }
    }

    private static void AddLeader(
        Label label,
        HashSet<int> leaders,
        IReadOnlyDictionary<string, int> labelOffsets,
        MethodBody body,
        int codeLength)
    {
        var offset = GetLabelOffset(label, labelOffsets, body, codeLength);
        if (offset < codeLength)
        {
            leaders.Add(offset);
        }
    }

    private static IReadOnlyList<BasicBlock> BuildBlocks(
        IReadOnlyList<Instruction> instructions,
        IEnumerable<int> leaders,
        IReadOnlyDictionary<int, int> offsetToIndex,
        int codeLength)
    {
        var orderedLeaders = leaders
            .Where(offsetToIndex.ContainsKey)
            .Distinct()
            .Order()
            .ToArray();
        var blocks = new List<BasicBlock>(orderedLeaders.Length);

        for (var index = 0; index < orderedLeaders.Length; index++)
        {
            var startOffset = orderedLeaders[index];
            var startIndex = offsetToIndex[startOffset];
            var endOffset = index + 1 < orderedLeaders.Length
                ? orderedLeaders[index + 1]
                : codeLength;
            var endIndex = index + 1 < orderedLeaders.Length
                ? offsetToIndex[endOffset]
                : instructions.Count;
            var blockInstructions = instructions
                .Skip(startIndex)
                .Take(endIndex - startIndex)
                .ToList();

            blocks.Add(new BasicBlock(
                $"B{index}",
                startOffset,
                endOffset,
                blockInstructions));
        }

        return blocks;
    }

    private static List<ControlFlowEdge> BuildNormalEdges(
        IReadOnlyList<BasicBlock> blocks,
        IReadOnlyDictionary<int, BasicBlock> blockByOffset,
        IReadOnlyDictionary<string, int> labelOffsets)
    {
        var edges = new List<ControlFlowEdge>();

        for (var index = 0; index < blocks.Count; index++)
        {
            var block = blocks[index];
            var terminal = block.Instructions[^1];
            var fallThrough = index + 1 < blocks.Count ? blocks[index + 1] : null;

            switch (terminal)
            {
                case JumpInstruction jump:
                AddEdge(
                    edges,
                    block,
                    GetTargetBlock(jump.Target, blockByOffset, labelOffsets),
                    IsUnconditionalJump(jump.OpCode)
                        ? ControlFlowEdgeKind.UnconditionalBranch
                        : ControlFlowEdgeKind.ConditionalBranch,
                    IsUnconditionalJump(jump.OpCode) ? null : "condition true");

                if (!IsUnconditionalJump(jump.OpCode) && fallThrough != null)
                {
                    AddEdge(
                        edges,
                        block,
                        fallThrough,
                        ControlFlowEdgeKind.FallThrough,
                        "condition false");
                }

                break;

                case TableSwitchInstruction tableSwitch:
                    AddEdge(
                        edges,
                        block,
                        GetTargetBlock(
                            tableSwitch.DefaultTarget,
                            blockByOffset,
                            labelOffsets),
                        ControlFlowEdgeKind.SwitchDefault,
                        "default");
                    for (var targetIndex = 0;
                         targetIndex < tableSwitch.Targets.Count;
                         targetIndex++)
                    {
                        AddEdge(
                            edges,
                            block,
                            GetTargetBlock(
                                tableSwitch.Targets[targetIndex],
                                blockByOffset,
                                labelOffsets),
                            ControlFlowEdgeKind.SwitchCase,
                            $"case {tableSwitch.Low + targetIndex}");
                    }

                    break;

                case LookupSwitchInstruction lookupSwitch:
                    AddEdge(
                        edges,
                        block,
                        GetTargetBlock(
                            lookupSwitch.DefaultTarget,
                            blockByOffset,
                            labelOffsets),
                        ControlFlowEdgeKind.SwitchDefault,
                        "default");
                    foreach (var (key, target) in lookupSwitch.Pairs)
                    {
                        AddEdge(
                            edges,
                            block,
                            GetTargetBlock(target, blockByOffset, labelOffsets),
                            ControlFlowEdgeKind.SwitchCase,
                            $"case {key}");
                    }

                    break;

                default:
                    if (!IsExitInstruction(terminal.OpCode) && fallThrough != null)
                    {
                        AddEdge(
                            edges,
                            block,
                            fallThrough,
                            ControlFlowEdgeKind.FallThrough,
                            null);
                    }

                    break;
            }
        }

        return edges;
    }

    private static void AddExceptionEdges(
        MethodBody body,
        IReadOnlyList<BasicBlock> blocks,
        List<ControlFlowEdge> edges,
        IReadOnlyDictionary<int, BasicBlock> blockByOffset,
        IReadOnlyDictionary<string, int> labelOffsets,
        int codeLength)
    {
        foreach (var handler in body.TryCatchBlocks)
        {
            var startOffset = GetLabelOffset(
                handler.Start,
                labelOffsets,
                body,
                codeLength);
            var endOffset = GetLabelOffset(
                handler.End,
                labelOffsets,
                body,
                codeLength);
            var handlerBlock = GetTargetBlock(
                handler.Handler,
                blockByOffset,
                labelOffsets);

            foreach (var block in blocks.Where(block =>
                         block.StartOffset >= startOffset
                         && block.StartOffset < endOffset))
            {
                AddEdge(
                    edges,
                    block,
                    handlerBlock,
                    ControlFlowEdgeKind.Exception,
                    handler.CatchType ?? "finally");
            }
        }
    }

    private static void AddEdge(
        ICollection<ControlFlowEdge> edges,
        BasicBlock source,
        BasicBlock target,
        ControlFlowEdgeKind kind,
        string? detail)
    {
        if (edges.Any(edge =>
                edge.Source == source
                && edge.Target == target
                && edge.Kind == kind
                && edge.Detail == detail))
        {
            return;
        }

        edges.Add(new ControlFlowEdge(source, target, kind, detail));
    }

    private static void MarkReachableBlocks(
        BasicBlock entry,
        IReadOnlyList<ControlFlowEdge> edges)
    {
        var queue = new Queue<BasicBlock>();
        entry.IsReachable = true;
        queue.Enqueue(entry);

        while (queue.TryDequeue(out var block))
        {
            foreach (var target in edges
                         .Where(edge => edge.Source == block)
                         .Select(edge => edge.Target))
            {
                if (target.IsReachable)
                {
                    continue;
                }

                target.IsReachable = true;
                queue.Enqueue(target);
            }
        }
    }

    private static BasicBlock GetTargetBlock(
        Label label,
        IReadOnlyDictionary<int, BasicBlock> blockByOffset,
        IReadOnlyDictionary<string, int> labelOffsets)
    {
        var offset = GetLabelOffset(label, labelOffsets);
        if (blockByOffset.TryGetValue(offset, out var block))
        {
            return block;
        }

        throw new InvalidOperationException(
            $"No basic block starts at label '{label.Name}' (offset {offset}).");
    }

    private static int GetLabelOffset(
        Label label,
        IReadOnlyDictionary<string, int> labelOffsets)
    {
        if (labelOffsets.TryGetValue(label.Name, out var offset))
        {
            return offset;
        }

        throw new InvalidOperationException(
            $"Label '{label.Name}' does not identify an instruction.");
    }

    private static int GetLabelOffset(
        Label label,
        IReadOnlyDictionary<string, int> labelOffsets,
        MethodBody body,
        int codeLength)
    {
        if (labelOffsets.TryGetValue(label.Name, out var offset))
        {
            return offset;
        }

        if (body.EndLabels.Any(endLabel => endLabel.Name == label.Name))
        {
            return codeLength;
        }

        throw new InvalidOperationException(
            $"Label '{label.Name}' does not identify an instruction or method end.");
    }

    private static int GetInstructionOffset(Instruction instruction)
    {
        return instruction.Offset
            ?? throw new InvalidOperationException(
                $"Instruction {instruction.OpCode} has no resolved offset.");
    }

    private static bool EndsBasicBlock(Instruction instruction)
    {
        return instruction is JumpInstruction
            or TableSwitchInstruction
            or LookupSwitchInstruction
            || IsExitInstruction(instruction.OpCode);
    }

    private static bool IsUnconditionalJump(OperationCode opCode)
    {
        return opCode is OperationCode.GOTO
            or OperationCode.GOTO_W
            or OperationCode.JSR
            or OperationCode.JSR_W;
    }

    private static bool IsExitInstruction(OperationCode opCode)
    {
        return opCode is OperationCode.IRETURN
            or OperationCode.LRETURN
            or OperationCode.FRETURN
            or OperationCode.DRETURN
            or OperationCode.ARETURN
            or OperationCode.RETURN
            or OperationCode.ATHROW
            or OperationCode.RET;
    }
}
