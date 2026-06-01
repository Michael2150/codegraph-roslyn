using System.Linq;
using Xunit;

namespace VbFixture.Tests
{
    // Tests for VB.NET-specific patterns that have no C# equivalent or behave differently.
    public class VbSpecificTests
    {
        private const string Skip = "Set CODEGRAPH_ROSLYN_BIN to run extraction tests";
        private static bool Available => FixtureRunner.IsBinaryAvailable();

        // --- Module ---

        [SkippableFact]
        public void Module_IsExtractedWithKindModule()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            // A VB Module must map to NodeKind "module", not "class"
            Assert.True(r.HasNode("module", "GeometryUtils"),
                "VB Module must be extracted with kind = module");
            Assert.False(r.HasNode("class", "GeometryUtils"),
                "VB Module must not be extracted as a class");
        }

        [SkippableFact]
        public void Module_AllMembersAreMarkedStatic()
        {
            Xunit.Skip.If(!Available, Skip);
            var r      = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            var module = r.Node("module", "GeometryUtils");
            Assert.NotNull(module);
            var members = r.Nodes.Where(n => n.ParentId == module!.Id).ToList();
            Assert.NotEmpty(members);
            Assert.All(members, m => Assert.True(m.IsStatic,
                $"Module member {m.Name} must have isStatic = true"));
        }

        // --- WithEvents ---

        [SkippableFact]
        public void WithEvents_FieldIsExtracted()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            // The WithEvents field _counter must appear as a field node on Alarm
            var field = r.ChildNode("Alarm", "field", "_counter");
            Assert.NotNull(field);
        }

        [SkippableFact]
        public void WithEvents_GeneratesReferencesEdge()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            // WithEvents + Handles wires the handler to the event source.
            // Expect a references edge from Counter_ThresholdReached to the event/field.
            var handlerNode = r.ChildNode("Alarm", "method", "Counter_ThresholdReached");
            Assert.NotNull(handlerNode);
            var hasRef = r.Edges.Any(e =>
                e.Kind == "references" && e.FromId == handlerNode!.Id)
                || r.UnresolvedReferences.Any(u =>
                u.Kind == "references" && u.FromId == handlerNode!.Id);
            Assert.True(hasRef,
                "Handles clause must generate a 'references' edge from the handler method");
        }

        // --- RaiseEvent ---

        [SkippableFact]
        public void RaiseEvent_GeneratesCallsOrReferencesEdge()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            var onMethod = r.ChildNode("Counter", "method", "OnThresholdReached");
            Assert.NotNull(onMethod);
            // RaiseEvent ThresholdReached should produce at least one outbound edge
            var hasOutbound = r.Edges.Any(e => e.FromId == onMethod!.Id)
                           || r.UnresolvedReferences.Any(u => u.FromId == onMethod!.Id);
            Assert.True(hasOutbound,
                "RaiseEvent must produce at least one outbound edge from OnThresholdReached");
        }

        // --- Partial Class ---

        [SkippableFact]
        public void PartialClass_BothPartsProduceDataProcessorNode()
        {
            Xunit.Skip.If(!Available, Skip);
            var r1 = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part1.vb");
            var r2 = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part2.vb");
            // Each file produces its own DataProcessor class node (per-file extraction)
            Assert.True(r1.HasNode("class", "DataProcessor"),
                "Part1 must contain a DataProcessor class node");
            Assert.True(r2.HasNode("class", "DataProcessor"),
                "Part2 must contain a DataProcessor class node");
        }

        [SkippableFact]
        public void PartialClass_Part1_ContainsConstructorAndProperties()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part1.vb");
            Assert.NotNull(r.ChildNode("DataProcessor", "method", "New"));
            Assert.NotNull(r.ChildNode("DataProcessor", "property", "InputPath"));
        }

        [SkippableFact]
        public void PartialClass_Part2_ContainsProcessingMethods()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part2.vb");
            Assert.NotNull(r.ChildNode("DataProcessor", "method", "Process"));
            Assert.NotNull(r.ChildNode("DataProcessor", "method", "ReadLines"));
            Assert.NotNull(r.ChildNode("DataProcessor", "method", "WriteResults"));
        }

        // --- Imports alias ---

        [SkippableFact]
        public void ImportAlias_ProducesImportNodeWithAliasName()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/ImportAliases.vb");
            var imports = r.Nodes.Where(n => n.Kind == "import").ToList();
            Assert.NotEmpty(imports);
            // At least one import should reference the alias name or target namespace
            var hasCol = imports.Any(n =>
                n.Name == "Col" || n.QualifiedName.Contains("Collections.Generic"));
            Assert.True(hasCol,
                "Import alias 'Col = System.Collections.Generic' must appear in import nodes");
        }

        // --- Async Sub (void async) ---

        [SkippableFact]
        public void AsyncSub_IsExtractedAsAsyncMethod()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/AsyncPatterns.vb");
            var node = r.ChildNode("UrlProcessor", "method", "FireAndForget");
            Assert.NotNull(node);
            // Async Sub has no Task return type but must still be marked isAsync
            Assert.True(node!.IsAsync,
                "Async Sub FireAndForget must have isAsync = true");
        }

        // --- Nested namespaces ---

        [SkippableFact]
        public void Namespaces_BothNamespacesAreExtracted()
        {
            Xunit.Skip.If(!Available, Skip);
            var r          = FixtureRunner.RunOnFile("VbFixture/ImportAliases.vb");
            var namespaces = r.Nodes.Where(n => n.Kind == "namespace").ToList();
            Assert.True(namespaces.Count >= 2,
                "ImportAliases.vb declares two namespaces; both should be extracted");
        }

        // --- Qualified names include namespace ---

        [SkippableFact]
        public void QualifiedName_IncludesNamespace()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var dog  = r.Node("class", "Dog");
            Assert.NotNull(dog);
            Assert.Contains("Fixtures", dog!.QualifiedName);
        }
    }
}
