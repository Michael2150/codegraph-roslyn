namespace CodeGraph.Roslyn;

public sealed class ExtractionResult
{
    public List<NodeEntry> Nodes { get; init; } = [];
    public List<EdgeEntry> Edges { get; init; } = [];
    public List<UnresolvedRef> UnresolvedReferences { get; init; } = [];
    public List<ExtractionError> Errors { get; init; } = [];
}

public sealed class NodeEntry
{
    public required string Id { get; init; }
    public required string Kind { get; init; }
    public required string Name { get; init; }
    public required string QualifiedName { get; init; }
    public required string FilePath { get; init; }
    public required int StartLine { get; init; }
    public required int EndLine { get; init; }
    public string? Visibility { get; init; }
    public bool IsStatic { get; init; }
    public bool IsAsync { get; init; }
    public string? ParentId { get; init; }
}

public sealed class EdgeEntry
{
    public required string Kind { get; init; }
    public required string FromId { get; init; }
    public required string ToId { get; init; }
    public string ToQualifiedName { get; init; } = "";
}

public sealed class UnresolvedRef
{
    public required string FromId { get; init; }
    public required string ToQualifiedName { get; init; }
    public required string Kind { get; init; }
}

public sealed class ExtractionError
{
    public required string Message { get; init; }
}
