namespace AnvilExample;

public sealed class ControlFlowGraph
{
    internal ControlFlowGraph(
        IReadOnlyList<BasicBlock> blocks,
        IReadOnlyList<ControlFlowEdge> edges)
    {
        Blocks = blocks;
        Edges = edges;
    }

    public IReadOnlyList<BasicBlock> Blocks { get; }
    public IReadOnlyList<ControlFlowEdge> Edges { get; }
    public BasicBlock Entry => Blocks[0];

    public IReadOnlyList<BasicBlock> ExitBlocks =>
        Blocks
            .Where(block => !Edges.Any(edge =>
                edge.Source == block
                && edge.Kind != ControlFlowEdgeKind.Exception))
            .ToList();
}
