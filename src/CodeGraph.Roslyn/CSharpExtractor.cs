using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGraph.Roslyn;

public sealed class CSharpExtractor(string filePath, string source)
{
    public ExtractionResult Extract()
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(source, path: filePath);
            var root = tree.GetRoot();
            var walker = new CSharpWalker(filePath);
            walker.Visit(root);
            return walker.GetResult();
        }
        catch (Exception ex)
        {
            return new ExtractionResult
            {
                Errors = [new ExtractionError { Message = ex.Message }]
            };
        }
    }
}

internal sealed class CSharpWalker : CSharpSyntaxWalker
{
    private readonly string _filePath;
    private readonly List<NodeEntry> _nodes = [];
    private readonly List<EdgeEntry> _edges = [];
    private readonly List<UnresolvedRef> _unresolvedRefs = [];

    // Namespace segments stack (e.g. ["Fixtures", "Inner"])
    private readonly List<string> _nsSegments = [];

    // Scope stack: (nodeId, qualifiedName, kind)
    private readonly Stack<(string id, string qualifiedName, string kind)> _scopeStack = new();

    // Current method context for call/instantiates edges
    private string? _currentMethodId;

    // Registry built as nodes are emitted (for resolution)
    private readonly Dictionary<string, string> _typesBySimpleName = new();          // "Dog" -> id
    private readonly Dictionary<string, string> _typesByQualifiedName = new();       // "Fixtures.Dog" -> id
    private readonly Dictionary<string, string> _typeKindBySimpleName = new();       // "IAnimal" -> "interface"
    private readonly Dictionary<string, List<string>> _methodIdsByName = new();      // "MakeSound" -> [ids]
    private readonly Dictionary<(string typeId, string name), string> _memberLookup = new(); // for override resolution

    // Overload counter to produce unique IDs
    private readonly Dictionary<string, int> _idCounter = new();

    public CSharpWalker(string filePath) : base(SyntaxWalkerDepth.Node)
    {
        _filePath = filePath;
    }

    public ExtractionResult GetResult() => new()
    {
        Nodes = _nodes,
        Edges = _edges,
        UnresolvedReferences = _unresolvedRefs,
    };

    // ── ID generation ───────────────────────────────────────────────────

    private string MakeId(string qualifiedName)
    {
        var key = $"{_filePath}::{qualifiedName}";
        if (!_idCounter.TryGetValue(key, out var count))
        {
            _idCounter[key] = 1;
            return key;
        }
        _idCounter[key] = count + 1;
        return $"{key}${count + 1}";
    }

    private string CurrentNamespace() =>
        _nsSegments.Count > 0 ? string.Join(".", _nsSegments) : "";

    private string QualifyName(string simpleName)
    {
        var ns = CurrentNamespace();
        var typeCtx = _scopeStack.Count > 0 ? _scopeStack.Peek().qualifiedName : null;
        if (typeCtx != null)
            return $"{typeCtx}.{simpleName}";
        if (!string.IsNullOrEmpty(ns))
            return $"{ns}.{simpleName}";
        return simpleName;
    }

    private string? CurrentParentId() =>
        _scopeStack.Count > 0 ? _scopeStack.Peek().id : null;

    // ── Helpers ──────────────────────────────────────────────────────────

    private static (int start, int end) GetLines(SyntaxNode node)
    {
        var span = node.GetLocation().GetLineSpan();
        return (span.StartLinePosition.Line + 1, span.EndLinePosition.Line + 1);
    }

    private static string? GetVisibility(SyntaxTokenList modifiers)
    {
        foreach (var m in modifiers)
        {
            if (m.IsKind(SyntaxKind.PublicKeyword)) return "public";
            if (m.IsKind(SyntaxKind.PrivateKeyword)) return "private";
            if (m.IsKind(SyntaxKind.ProtectedKeyword)) return "protected";
            if (m.IsKind(SyntaxKind.InternalKeyword)) return "internal";
        }
        return null;
    }

    private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind) =>
        modifiers.Any(m => m.IsKind(kind));

    private static string GetSimpleTypeName(TypeSyntax type) => type switch
    {
        SimpleNameSyntax sn => sn.Identifier.Text,
        QualifiedNameSyntax qn => GetSimpleTypeName(qn.Right),
        _ => type.ToString()
    };

    // ── Node registration helpers ────────────────────────────────────────

    private NodeEntry EmitNode(string kind, string name, string qualifiedName, SyntaxNode syntax,
        string? visibility, bool isStatic, bool isAsync)
    {
        var (start, end) = GetLines(syntax);
        var id = MakeId(qualifiedName);
        var parentId = CurrentParentId();

        var node = new NodeEntry
        {
            Id = id,
            Kind = kind,
            Name = name,
            QualifiedName = qualifiedName,
            FilePath = _filePath,
            StartLine = start,
            EndLine = end,
            Visibility = visibility,
            IsStatic = isStatic,
            IsAsync = isAsync,
            ParentId = parentId,
        };
        _nodes.Add(node);

        // Register in name dictionaries
        if (!_methodIdsByName.TryGetValue(name, out var list))
            _methodIdsByName[name] = list = [];
        list.Add(id);

        if (kind is "class" or "interface" or "struct" or "enum" or "module")
        {
            _typesBySimpleName[name] = id;
            _typesByQualifiedName[qualifiedName] = id;
            _typeKindBySimpleName[name] = kind;
        }

        if (kind is "method" or "property" or "field" && parentId != null)
            _memberLookup[(parentId, name)] = id;

        // Emit contains edge from parent
        if (parentId != null)
            _edges.Add(new EdgeEntry { Kind = "contains", FromId = parentId, ToId = id });

        return node;
    }

    // ── Type declaration visitors ─────────────────────────────────────────

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var nsName = node.Name.ToString();
        _nsSegments.Add(nsName);
        var qualName = string.Join(".", _nsSegments);
        var ns = EmitNode("namespace", nsName, qualName, node, "public", false, false);
        _scopeStack.Push((ns.Id, qualName, "namespace"));
        base.VisitNamespaceDeclaration(node);
        _scopeStack.Pop();
        _nsSegments.RemoveAt(_nsSegments.Count - 1);
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        var nsName = node.Name.ToString();
        _nsSegments.Add(nsName);
        var qualName = string.Join(".", _nsSegments);
        var ns = EmitNode("namespace", nsName, qualName, node, "public", false, false);
        _scopeStack.Push((ns.Id, qualName, "namespace"));
        base.VisitFileScopedNamespaceDeclaration(node);
        _scopeStack.Pop();
        _nsSegments.RemoveAt(_nsSegments.Count - 1);
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        // Skip global using directives that are not at file level for clarity
        var alias = node.Alias?.Name.Identifier.Text;
        var target = node.Name?.ToString() ?? node.NamespaceOrType?.ToString() ?? "";
        var name = alias ?? (target.Contains('.') ? target.Split('.').Last() : target);
        var qualName = target;

        var (start, end) = GetLines(node);
        var id = MakeId($"import::{qualName}");
        _nodes.Add(new NodeEntry
        {
            Id = id,
            Kind = "import",
            Name = name,
            QualifiedName = qualName,
            FilePath = _filePath,
            StartLine = start,
            EndLine = end,
            Visibility = null,
            IsStatic = false,
            IsAsync = false,
            ParentId = CurrentParentId(),
        });
    }

    private void VisitTypeDeclaration(BaseTypeDeclarationSyntax node, string kind)
    {
        var name = node.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(node.Modifiers);
        var isStatic = HasModifier(node.Modifiers, SyntaxKind.StaticKeyword);

        var entry = EmitNode(kind, name, qualName, node, vis, isStatic, false);

        // Emit extends/implements edges from base list
        if (node is TypeDeclarationSyntax td && td.BaseList != null)
        {
            foreach (var baseType in td.BaseList.Types)
            {
                var baseName = GetSimpleTypeName(baseType.Type);
                EmitBaseEdge(entry.Id, baseName);
            }
        }

        _scopeStack.Push((entry.Id, qualName, kind));
        DefaultVisit(node);  // visit members
        _scopeStack.Pop();
    }

    private void EmitBaseEdge(string fromId, string baseName)
    {
        // Determine if extends or implements
        string edgeKind;
        if (_typeKindBySimpleName.TryGetValue(baseName, out var baseKind))
        {
            edgeKind = baseKind == "interface" ? "implements" : "extends";
        }
        else
        {
            // Heuristic: I-prefix + uppercase second char → interface
            edgeKind = (baseName.Length > 1 && baseName[0] == 'I' && char.IsUpper(baseName[1]))
                ? "implements" : "extends";
        }

        if (_typesBySimpleName.TryGetValue(baseName, out var toId))
        {
            _edges.Add(new EdgeEntry { Kind = edgeKind, FromId = fromId, ToId = toId, ToQualifiedName = baseName });
        }
        else
        {
            _unresolvedRefs.Add(new UnresolvedRef { Kind = edgeKind, FromId = fromId, ToQualifiedName = baseName });
        }
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node) =>
        VisitTypeDeclaration(node, "class");

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) =>
        VisitTypeDeclaration(node, "interface");

    public override void VisitStructDeclaration(StructDeclarationSyntax node) =>
        VisitTypeDeclaration(node, "struct");

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        // record class → "class", record struct → "struct"
        var kind = node.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) ? "struct" : "class";
        VisitTypeDeclaration(node, kind);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(node.Modifiers);
        var entry = EmitNode("enum", name, qualName, node, vis, false, false);

        _scopeStack.Push((entry.Id, qualName, "enum"));
        base.VisitEnumDeclaration(node);
        _scopeStack.Pop();
    }

    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var qualName = QualifyName(name);
        EmitNode("enum_member", name, qualName, node, null, true, false);
    }

    // ── Member declaration visitors ──────────────────────────────────────

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        var vis = GetVisibility(node.Modifiers);
        var isStatic = HasModifier(node.Modifiers, SyntaxKind.StaticKeyword);
        foreach (var v in node.Declaration.Variables)
        {
            var name = v.Identifier.Text;
            var qualName = QualifyName(name);
            EmitNode("field", name, qualName, node, vis, isStatic, false);
        }
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(node.Modifiers);
        var isStatic = HasModifier(node.Modifiers, SyntaxKind.StaticKeyword);
        // Don't recurse into accessor bodies for call tracking
        EmitNode("property", name, qualName, node, vis, isStatic, false);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(node.Modifiers);
        var isStatic = HasModifier(node.Modifiers, SyntaxKind.StaticKeyword);
        var isAsync = HasModifier(node.Modifiers, SyntaxKind.AsyncKeyword);
        var isOverride = HasModifier(node.Modifiers, SyntaxKind.OverrideKeyword);
        var parentInfo = _scopeStack.Count > 0 ? _scopeStack.Peek() : default;
        var isInModule = parentInfo.kind is "module";

        // Module members are implicitly static
        if (isInModule) isStatic = true;

        var entry = EmitNode("method", name, qualName, node, vis, isStatic, isAsync);

        // Emit overrides edge
        if (isOverride)
            EmitOverridesEdge(entry.Id, name);

        // Visit body for call/instantiates edges
        var savedMethod = _currentMethodId;
        _currentMethodId = entry.Id;
        if (node.Body != null) base.VisitBlock(node.Body);
        else if (node.ExpressionBody != null) Visit(node.ExpressionBody);
        _currentMethodId = savedMethod;
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var qualName = QualifyName(".ctor");
        var vis = GetVisibility(node.Modifiers);
        var entry = EmitNode("method", ".ctor", qualName, node, vis, false, false);

        var savedMethod = _currentMethodId;
        _currentMethodId = entry.Id;
        if (node.Body != null) base.VisitBlock(node.Body);
        else if (node.ExpressionBody != null) Visit(node.ExpressionBody);
        _currentMethodId = savedMethod;
    }

    // ── Override resolution ──────────────────────────────────────────────

    private void EmitOverridesEdge(string fromId, string methodName)
    {
        // Try to find the base class method in the same file
        if (_methodIdsByName.TryGetValue(methodName, out var candidates))
        {
            // Find one that is NOT under the same parent
            var currentParentId = CurrentParentId();
            var target = candidates.FirstOrDefault(id => {
                // Find the node
                var n = _nodes.Find(x => x.Id == id);
                return n != null && n.ParentId != currentParentId;
            });
            if (target != null)
            {
                _edges.Add(new EdgeEntry { Kind = "overrides", FromId = fromId, ToId = target, ToQualifiedName = methodName });
                return;
            }
        }
        // Fallback: emit unresolved ref
        _unresolvedRefs.Add(new UnresolvedRef { Kind = "overrides", FromId = fromId, ToQualifiedName = methodName });
    }

    // ── Expression visitors (for edges within method bodies) ─────────────

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (_currentMethodId != null)
        {
            var name = GetInvokedMethodName(node);
            if (name != null)
            {
                _unresolvedRefs.Add(new UnresolvedRef
                {
                    Kind = "calls",
                    FromId = _currentMethodId,
                    ToQualifiedName = name,
                });
            }
        }
        base.VisitInvocationExpression(node);
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        if (_currentMethodId != null)
        {
            var typeName = GetSimpleTypeName(node.Type);
            if (_typesBySimpleName.TryGetValue(typeName, out var toId))
            {
                _edges.Add(new EdgeEntry
                {
                    Kind = "instantiates",
                    FromId = _currentMethodId,
                    ToId = toId,
                    ToQualifiedName = typeName,
                });
            }
            else
            {
                _unresolvedRefs.Add(new UnresolvedRef
                {
                    Kind = "instantiates",
                    FromId = _currentMethodId,
                    ToQualifiedName = typeName,
                });
            }
        }
        base.VisitObjectCreationExpression(node);
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        // Detect += event subscription
        if (_currentMethodId != null && node.OperatorToken.IsKind(SyntaxKind.PlusEqualsToken))
        {
            if (node.Right is IdentifierNameSyntax id)
            {
                _unresolvedRefs.Add(new UnresolvedRef
                {
                    Kind = "references",
                    FromId = _currentMethodId,
                    ToQualifiedName = id.Identifier.Text,
                });
            }
        }
        base.VisitAssignmentExpression(node);
    }

    private static string? GetInvokedMethodName(InvocationExpressionSyntax node) =>
        node.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            ConditionalAccessExpressionSyntax ca when ca.WhenNotNull is MemberBindingExpressionSyntax mb =>
                mb.Name.Identifier.Text,
            _ => null
        };
}
