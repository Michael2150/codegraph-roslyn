using System.Linq;
using Xunit;

namespace CsFixture.Tests
{
    // Tests for C#-specific patterns that differ from VB.NET or have no VB.NET equivalent.
    public class CsSpecificTests
    {
        private const string Skip = "Set CODEGRAPH_ROSLYN_BIN to run extraction tests";
        private static bool Available => FixtureRunner.IsBinaryAvailable();

        // --- record class vs record struct ---

        [SkippableFact]
        public void RecordClass_IsExtractedWithKindClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var node = r.Node("class", "PersonRecord");
            Assert.NotNull(node);
            Assert.False(r.HasNode("struct", "PersonRecord"),
                "record class must not be extracted as struct");
        }

        [SkippableFact]
        public void RecordStruct_IsExtractedWithKindStruct()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var node = r.Node("struct", "Rectangle");
            Assert.NotNull(node);
            Assert.False(r.HasNode("class", "Rectangle"),
                "record struct must not be extracted as class");
        }

        // --- static class ---

        [SkippableFact]
        public void StaticClass_IsMarkedStatic()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var node = r.Node("class", "GeometryUtils");
            Assert.NotNull(node);
            Assert.True(node!.IsStatic, "static class must have isStatic = true");
        }

        [SkippableFact]
        public void StaticClass_AllMethodsAreStatic()
        {
            Xunit.Skip.If(!Available, Skip);
            var r      = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var parent = r.Node("class", "GeometryUtils");
            Assert.NotNull(parent);
            var methods = r.Nodes.Where(n => n.Kind == "method" && n.ParentId == parent!.Id).ToList();
            Assert.NotEmpty(methods);
            Assert.All(methods, m => Assert.True(m.IsStatic,
                $"Static class method {m.Name} must have isStatic = true"));
        }

        // --- generic types (name extraction without type parameters) ---

        [SkippableFact]
        public void GenericInterface_NameDoesNotIncludeBrackets()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            // Name should be "IRepository", not "IRepository<T>"
            var node = r.Node("interface", "IRepository");
            Assert.NotNull(node);
            Assert.DoesNotContain("<", node!.Name);
        }

        [SkippableFact]
        public void GenericClass_NameDoesNotIncludeBrackets()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            var node = r.Node("class", "RepositoryBase");
            Assert.NotNull(node);
            Assert.DoesNotContain("<", node!.Name);
        }

        // --- extension methods ---

        [SkippableFact]
        public void ExtensionMethod_IsExtractedAsMethod()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            var node = r.ChildNode("CollectionExtensions", "method", "WhereNotNull");
            Assert.NotNull(node);
        }

        [SkippableFact]
        public void ExtensionMethod_IsMarkedStatic()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            var node = r.ChildNode("CollectionExtensions", "method", "WhereNotNull");
            Assert.NotNull(node);
            Assert.True(node!.IsStatic);
        }

        // --- using alias ---

        [SkippableFact]
        public void UsingAlias_ProducesImportNodeWithAliasOrTargetName()
        {
            Xunit.Skip.If(!Available, Skip);
            var r       = FixtureRunner.RunOnFile("CsFixture/ImportAliases.cs");
            var imports = r.Nodes.Where(n => n.Kind == "import").ToList();
            Assert.NotEmpty(imports);
            var hasCol  = imports.Any(n =>
                n.Name == "Col" || n.QualifiedName.Contains("Collections.Generic"));
            Assert.True(hasCol,
                "using alias 'Col = ...' must appear in import nodes");
        }

        // --- async void ---

        [SkippableFact]
        public void AsyncVoid_IsExtractedAsAsyncMethod()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/AsyncPatterns.cs");
            var node = r.ChildNode("UrlProcessor", "method", "FireAndForget");
            Assert.NotNull(node);
            Assert.True(node!.IsAsync, "async void must have isAsync = true");
        }

        // --- nested class in separate namespace ---

        [SkippableFact]
        public void NestedClass_BothNamespacesExtracted()
        {
            Xunit.Skip.If(!Available, Skip);
            var r          = FixtureRunner.RunOnFile("CsFixture/ImportAliases.cs");
            var namespaces = r.Nodes.Where(n => n.Kind == "namespace").ToList();
            Assert.True(namespaces.Count >= 2,
                "ImportAliases.cs declares two namespaces; both should be extracted");
        }

        // --- qualified name includes namespace ---

        [SkippableFact]
        public void QualifiedName_IncludesNamespace()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var dog  = r.Node("class", "Dog");
            Assert.NotNull(dog);
            Assert.Contains("Fixtures", dog!.QualifiedName);
        }

        // --- event subscription (+=) treated as reference ---

        [SkippableFact]
        public void EventSubscription_GeneratesEdgeFromConstructor()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/EventSystem.cs");
            var ctor = r.ChildNode("Alarm", "method", ".ctor");
            // Accept either ".ctor" or the class name as the constructor node name
            ctor ??= r.ChildNode("Alarm", "method", "Alarm");
            Assert.NotNull(ctor);
            // The += subscription should produce at least one outbound edge
            var hasOutbound = r.Edges.Any(e => e.FromId == ctor!.Id)
                           || r.UnresolvedReferences.Any(u => u.FromId == ctor!.Id);
            Assert.True(hasOutbound,
                "Constructor with += event subscription must produce at least one outbound edge");
        }

        // --- visibility on interface members ---

        [SkippableFact]
        public void InterfaceMembers_HavePublicOrNullVisibility()
        {
            Xunit.Skip.If(!Available, Skip);
            var r       = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var iAnimal = r.Node("interface", "IAnimal");
            Assert.NotNull(iAnimal);
            var members = r.Nodes.Where(n => n.ParentId == iAnimal!.Id).ToList();
            Assert.All(members, m =>
                Assert.True(m.Visibility is null or "public",
                    $"Interface member {m.Name} should have visibility null or public"));
        }
    }
}
