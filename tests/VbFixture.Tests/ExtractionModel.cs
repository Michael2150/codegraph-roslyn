using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VbFixture.Tests
{
    // C# model that mirrors the JSON output contract of codegraph-roslyn.
    // Keep in sync with the JSON schema in docs/codegraph-vbnet-roslyn-handoff.md.

    public record ExtractionResult(
        [property: JsonPropertyName("nodes")]              List<ExtractionNode>  Nodes,
        [property: JsonPropertyName("edges")]              List<ExtractionEdge>  Edges,
        [property: JsonPropertyName("unresolvedReferences")] List<UnresolvedRef> UnresolvedReferences,
        [property: JsonPropertyName("errors")]             List<ExtractionError> Errors
    );

    public record ExtractionNode(
        [property: JsonPropertyName("id")]            string  Id,
        [property: JsonPropertyName("kind")]          string  Kind,
        [property: JsonPropertyName("name")]          string  Name,
        [property: JsonPropertyName("qualifiedName")] string  QualifiedName,
        [property: JsonPropertyName("filePath")]      string  FilePath,
        [property: JsonPropertyName("startLine")]     int     StartLine,
        [property: JsonPropertyName("endLine")]       int     EndLine,
        [property: JsonPropertyName("visibility")]    string? Visibility,
        [property: JsonPropertyName("isStatic")]      bool    IsStatic,
        [property: JsonPropertyName("isAsync")]       bool    IsAsync,
        [property: JsonPropertyName("parentId")]      string? ParentId
    );

    public record ExtractionEdge(
        [property: JsonPropertyName("kind")]            string  Kind,
        [property: JsonPropertyName("fromId")]          string  FromId,
        [property: JsonPropertyName("toId")]            string  ToId,
        [property: JsonPropertyName("toQualifiedName")] string? ToQualifiedName
    );

    public record UnresolvedRef(
        [property: JsonPropertyName("fromId")]          string  FromId,
        [property: JsonPropertyName("toQualifiedName")] string  ToQualifiedName,
        [property: JsonPropertyName("kind")]            string  Kind
    );

    public record ExtractionError(
        [property: JsonPropertyName("message")] string Message
    );
}
