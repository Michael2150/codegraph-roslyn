using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeGraph.Roslyn;

public sealed class VBNetExtractor(string filePath, string source)
{
    public ExtractionResult Extract()
    {
        try
        {
            var tree = VisualBasicSyntaxTree.ParseText(source, path: filePath);
            var root = tree.GetRoot();
            var walker = new VBNetWalker(filePath);
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

internal sealed class VBNetWalker : VisualBasicSyntaxWalker
{
    private readonly string _filePath;
    private readonly List<NodeEntry> _nodes = [];
    private readonly List<EdgeEntry> _edges = [];
    private readonly List<UnresolvedRef> _unresolvedRefs = [];

    private readonly List<string> _nsSegments = [];
    private readonly Stack<(string id, string qualifiedName, string kind, bool isModule)> _scopeStack = new();
    private string? _currentMethodId;

    private readonly Dictionary<string, string> _typesBySimpleName = new();
    private readonly Dictionary<string, string> _typeKindBySimpleName = new();
    private readonly Dictionary<string, List<string>> _methodIdsByName = new();
    private readonly Dictionary<string, int> _idCounter = new();

    public VBNetWalker(string filePath) : base(SyntaxWalkerDepth.Node)
    {
        _filePath = filePath;
    }

    public ExtractionResult GetResult() => new()
    {
        Nodes = _nodes,
        Edges = _edges,
        UnresolvedReferences = _unresolvedRefs,
    };

    // ── Helpers ──────────────────────────────────────────────────────────

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

    private string QualifyName(string simpleName)
    {
        if (_scopeStack.Count > 0)
            return $"{_scopeStack.Peek().qualifiedName}.{simpleName}";
        if (_nsSegments.Count > 0)
            return $"{string.Join(".", _nsSegments)}.{simpleName}";
        return simpleName;
    }

    private string? CurrentParentId() =>
        _scopeStack.Count > 0 ? _scopeStack.Peek().id : null;

    private bool InModule() =>
        _scopeStack.Count > 0 && _scopeStack.Peek().isModule;

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
            if (m.IsKind(SyntaxKind.FriendKeyword)) return "internal";
        }
        return null;
    }

    private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind) =>
        modifiers.Any(m => m.IsKind(kind));

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

        if (!_methodIdsByName.TryGetValue(name, out var list))
            _methodIdsByName[name] = list = [];
        list.Add(id);

        if (kind is "class" or "interface" or "struct" or "enum" or "module")
        {
            _typesBySimpleName[name] = id;
            _typeKindBySimpleName[name] = kind;
        }

        if (parentId != null)
            _edges.Add(new EdgeEntry { Kind = "contains", FromId = parentId, ToId = id });

        return node;
    }

    private static string GetVbTypeName(TypeSyntax type) => type switch
    {
        SimpleNameSyntax sn => sn.Identifier.Text,
        QualifiedNameSyntax qn => GetVbTypeName(qn.Right),
        _ => type.ToString()
    };

    // ── Namespace ────────────────────────────────────────────────────────

    public override void VisitNamespaceBlock(NamespaceBlockSyntax node)
    {
        var nsName = node.NamespaceStatement.Name.ToString();
        _nsSegments.Add(nsName);
        var qualName = string.Join(".", _nsSegments);
        var ns = EmitNode("namespace", nsName, qualName, node, "public", false, false);
        _scopeStack.Push((ns.Id, qualName, "namespace", false));
        DefaultVisit(node);
        _scopeStack.Pop();
        _nsSegments.RemoveAt(_nsSegments.Count - 1);
    }

    // ── Imports ──────────────────────────────────────────────────────────

    public override void VisitImportsStatement(ImportsStatementSyntax node)
    {
        foreach (var clause in node.ImportsClauses)
        {
            if (clause is not SimpleImportsClauseSyntax simple) continue;
            string name, qualName;
            var target = simple.Name.ToString();
            qualName = target;
            if (simple.Alias != null)
            {
                // Imports Col = System.Collections.Generic
                name = simple.Alias.Identifier.ToString();
            }
            else
            {
                name = target.Contains('.') ? target.Split('.').Last() : target;
            }

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
    }

    // ── Type declarations ─────────────────────────────────────────────────

    public override void VisitClassBlock(ClassBlockSyntax node)
    {
        var stmt = node.ClassStatement;
        var name = stmt.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(stmt.Modifiers);
        var entry = EmitNode("class", name, qualName, node, vis, false, false);
        EmitInheritsImplements(entry.Id, node);

        _scopeStack.Push((entry.Id, qualName, "class", false));
        DefaultVisit(node);
        _scopeStack.Pop();
    }

    public override void VisitModuleBlock(ModuleBlockSyntax node)
    {
        var stmt = node.ModuleStatement;
        var name = stmt.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(stmt.Modifiers);
        var entry = EmitNode("module", name, qualName, node, vis ?? "public", true, false);

        _scopeStack.Push((entry.Id, qualName, "module", true));
        DefaultVisit(node);
        _scopeStack.Pop();
    }

    public override void VisitInterfaceBlock(InterfaceBlockSyntax node)
    {
        var stmt = node.InterfaceStatement;
        var name = stmt.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(stmt.Modifiers);
        var entry = EmitNode("interface", name, qualName, node, vis, false, false);

        _scopeStack.Push((entry.Id, qualName, "interface", false));
        DefaultVisit(node);
        _scopeStack.Pop();
    }

    public override void VisitStructureBlock(StructureBlockSyntax node)
    {
        var stmt = node.StructureStatement;
        var name = stmt.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(stmt.Modifiers);
        var entry = EmitNode("struct", name, qualName, node, vis, false, false);
        EmitInheritsImplements(entry.Id, node);

        _scopeStack.Push((entry.Id, qualName, "struct", false));
        DefaultVisit(node);
        _scopeStack.Pop();
    }

    public override void VisitEnumBlock(EnumBlockSyntax node)
    {
        var stmt = node.EnumStatement;
        var name = stmt.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(stmt.Modifiers);
        var entry = EmitNode("enum", name, qualName, node, vis, false, false);

        _scopeStack.Push((entry.Id, qualName, "enum", false));
        DefaultVisit(node);
        _scopeStack.Pop();
    }

    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        EmitNode("enum_member", name, QualifyName(name), node, null, true, false);
    }

    private void EmitInheritsImplements(string fromId, SyntaxNode typeBlock)
    {
        foreach (var child in typeBlock.ChildNodes())
        {
            if (child is InheritsStatementSyntax inherits)
            {
                foreach (var t in inherits.Types)
                    EmitBaseEdge(fromId, GetVbTypeName(t), isInterface: false);
            }
            else if (child is ImplementsStatementSyntax implements)
            {
                foreach (var t in implements.Types)
                {
                    var baseName = GetVbTypeName(t);
                    if (_typesBySimpleName.TryGetValue(baseName, out var toId))
                        _edges.Add(new EdgeEntry { Kind = "implements", FromId = fromId, ToId = toId, ToQualifiedName = baseName });
                    else
                        _unresolvedRefs.Add(new UnresolvedRef { Kind = "implements", FromId = fromId, ToQualifiedName = baseName });
                }
            }
        }
    }

    private void EmitBaseEdge(string fromId, string baseName, bool isInterface)
    {
        if (_typeKindBySimpleName.TryGetValue(baseName, out var baseKind))
        {
            var edgeKind = baseKind == "interface" ? "implements" : "extends";
            if (_typesBySimpleName.TryGetValue(baseName, out var toId))
                _edges.Add(new EdgeEntry { Kind = edgeKind, FromId = fromId, ToId = toId, ToQualifiedName = baseName });
            else
                _unresolvedRefs.Add(new UnresolvedRef { Kind = edgeKind, FromId = fromId, ToQualifiedName = baseName });
        }
        else
        {
            // Heuristic: I-prefix + uppercase → interface
            var edgeKind = (!isInterface && baseName.Length > 1 && baseName[0] == 'I' && char.IsUpper(baseName[1]))
                ? "implements" : "extends";
            _unresolvedRefs.Add(new UnresolvedRef { Kind = edgeKind, FromId = fromId, ToQualifiedName = baseName });
        }
    }

    // ── Method blocks ─────────────────────────────────────────────────────

    // Regular Sub / Function blocks (MethodBlockSyntax handles both)
    public override void VisitMethodBlock(MethodBlockSyntax node)
    {
        var stmt = node.SubOrFunctionStatement;
        var name = stmt.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(stmt.Modifiers);
        var isStatic = InModule() || HasModifier(stmt.Modifiers, SyntaxKind.SharedKeyword);
        var isAsync = HasModifier(stmt.Modifiers, SyntaxKind.AsyncKeyword);
        var isOverride = HasModifier(stmt.Modifiers, SyntaxKind.OverridesKeyword);

        var entry = EmitNode("method", name, qualName, node, vis, isStatic, isAsync);

        if (isOverride)
            EmitOverridesEdge(entry.Id, name);

        // Handles clause — on the statement header
        var handlesClause = stmt.HandlesClause;
        if (handlesClause != null)
        {
            foreach (var item in handlesClause.Events)
            {
                _unresolvedRefs.Add(new UnresolvedRef
                {
                    Kind = "references",
                    FromId = entry.Id,
                    ToQualifiedName = item.ToString(),
                });
            }
        }

        // Visit body statements only (don't call base to avoid re-visiting the header)
        var savedMethod = _currentMethodId;
        _currentMethodId = entry.Id;
        foreach (var s in node.Statements)
            Visit(s);
        _currentMethodId = savedMethod;
    }

    // Constructor (Sub New) block
    public override void VisitConstructorBlock(ConstructorBlockSyntax node)
    {
        var qualName = QualifyName("New");
        var vis = GetVisibility(node.SubNewStatement.Modifiers);
        var entry = EmitNode("method", "New", qualName, node, vis, false, false);

        var savedMethod = _currentMethodId;
        _currentMethodId = entry.Id;
        foreach (var s in node.Statements)
            Visit(s);
        _currentMethodId = savedMethod;
    }

    // Standalone method declarations (interface members / MustOverride)
    public override void VisitMethodStatement(MethodStatementSyntax node)
    {
        // Skip if it's the header of a MethodBlock (handled by VisitMethodBlock)
        if (node.Parent is MethodBlockSyntax) return;
        // Skip if it's a constructor header (handled by VisitConstructorBlock)
        if (node.Parent is ConstructorBlockSyntax) return;

        var name = node.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(node.Modifiers);
        var isStatic = InModule() || HasModifier(node.Modifiers, SyntaxKind.SharedKeyword);
        var isAsync = HasModifier(node.Modifiers, SyntaxKind.AsyncKeyword);
        EmitNode("method", name, qualName, node, vis, isStatic, isAsync);
    }

    private void EmitOverridesEdge(string fromId, string methodName)
    {
        if (_methodIdsByName.TryGetValue(methodName, out var candidates))
        {
            var currentParentId = CurrentParentId();
            var target = candidates.FirstOrDefault(id => {
                var n = _nodes.Find(x => x.Id == id);
                return n != null && n.ParentId != currentParentId;
            });
            if (target != null)
            {
                _edges.Add(new EdgeEntry { Kind = "overrides", FromId = fromId, ToId = target, ToQualifiedName = methodName });
                return;
            }
        }
        _unresolvedRefs.Add(new UnresolvedRef { Kind = "overrides", FromId = fromId, ToQualifiedName = methodName });
    }

    // ── Properties ───────────────────────────────────────────────────────

    public override void VisitPropertyBlock(PropertyBlockSyntax node)
    {
        var stmt = node.PropertyStatement;
        EmitPropertyStatement(stmt, node);
    }

    public override void VisitPropertyStatement(PropertyStatementSyntax node)
    {
        if (node.Parent is PropertyBlockSyntax) return; // handled by VisitPropertyBlock
        EmitPropertyStatement(node, node);
    }

    private void EmitPropertyStatement(PropertyStatementSyntax stmt, SyntaxNode syntaxNode)
    {
        var name = stmt.Identifier.Text;
        var qualName = QualifyName(name);
        var vis = GetVisibility(stmt.Modifiers);
        var isStatic = InModule() || HasModifier(stmt.Modifiers, SyntaxKind.SharedKeyword);
        EmitNode("property", name, qualName, syntaxNode, vis, isStatic, false);
    }

    // ── Fields ───────────────────────────────────────────────────────────

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        var vis = GetVisibility(node.Modifiers);
        var isStatic = InModule() || HasModifier(node.Modifiers, SyntaxKind.SharedKeyword);
        foreach (var declarator in node.Declarators)
        {
            foreach (var nameNode in declarator.Names)
            {
                var simpleName = nameNode.Identifier.Text;
                var qualName = QualifyName(simpleName);
                EmitNode("field", simpleName, qualName, node, vis, isStatic, false);
            }
        }
    }

    // ── Expression visitors ──────────────────────────────────────────────

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (_currentMethodId != null)
        {
            var name = GetVbInvokedMethodName(node);
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
            var typeName = GetVbTypeName(node.Type);
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
            else if (!string.IsNullOrEmpty(typeName))
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

    public override void VisitRaiseEventStatement(RaiseEventStatementSyntax node)
    {
        if (_currentMethodId != null)
        {
            _unresolvedRefs.Add(new UnresolvedRef
            {
                Kind = "calls",
                FromId = _currentMethodId,
                ToQualifiedName = node.Name.ToString(),
            });
        }
        base.VisitRaiseEventStatement(node);
    }

    private static string? GetVbInvokedMethodName(InvocationExpressionSyntax node) =>
        node.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            _ => null
        };
}
