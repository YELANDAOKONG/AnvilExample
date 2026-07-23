namespace AnvilExample;

public sealed class ControlFlowEdge
{
    internal ControlFlowEdge(
        BasicBlock source,
        BasicBlock target,
        ControlFlowEdgeKind kind,
        string? detail)
    {
        Source = source;
        Target = target;
        Kind = kind;
        Detail = detail;
    }

    public BasicBlock Source { get; }
    public BasicBlock Target { get; }
    public ControlFlowEdgeKind Kind { get; }
    public string? Detail { get; }

    public bool IsBackward =>
        Kind != ControlFlowEdgeKind.Exception
        && Target.StartOffset <= Source.StartOffset;
}
