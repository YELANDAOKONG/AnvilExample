using Anvil.Instructions;

namespace AnvilExample;

public sealed class BasicBlock
{
    internal BasicBlock(
        string id,
        int startOffset,
        int endOffset,
        IReadOnlyList<Instruction> instructions)
    {
        Id = id;
        StartOffset = startOffset;
        EndOffset = endOffset;
        Instructions = instructions;
    }

    public string Id { get; }
    public int StartOffset { get; }
    public int EndOffset { get; }
    public IReadOnlyList<Instruction> Instructions { get; }
    public bool IsReachable { get; internal set; }

    public override string ToString() => Id;
}
