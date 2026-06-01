using Xunit;

namespace VbFixture.Tests
{
    // Verifies that all expected node types are extracted from the VB fixture files.
    public class VbNodeTests
    {
        private const string Skip = "Set CODEGRAPH_ROSLYN_BIN to run extraction tests";
        private static bool Available => FixtureRunner.IsBinaryAvailable();

        // --- Animals.vb ---

        [SkippableFact]
        public void Animals_ExtractsIAnimalInterface()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var node = r.Node("interface", "IAnimal");
            Assert.NotNull(node);
            Assert.Equal("public", node.Visibility);
        }

        [SkippableFact]
        public void Animals_IAnimal_HasFourMembers()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var iAnimal = r.Node("interface", "IAnimal");
            Assert.NotNull(iAnimal);
            var members = r.Nodes.FindAll(n => n.ParentId == iAnimal!.Id);
            Assert.Equal(4, members.Count);
        }

        [SkippableFact]
        public void Animals_ExtractsIDomesticatedInterface()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasNode("interface", "IDomesticated"));
        }

        [SkippableFact]
        public void Animals_ExtractsAnimalAbstractClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var node = r.Node("class", "Animal");
            Assert.NotNull(node);
            Assert.Equal("public", node.Visibility);
        }

        [SkippableFact]
        public void Animals_Animal_HasPrivateFieldName()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var node = r.ChildNode("Animal", "field", "_name");
            Assert.NotNull(node);
            Assert.Equal("private", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_Animal_HasProtectedFieldSound()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var node = r.ChildNode("Animal", "field", "_sound");
            Assert.NotNull(node);
            Assert.Equal("protected", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_Animal_HasPropertyName()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.NotNull(r.ChildNode("Animal", "property", "Name"));
        }

        [SkippableFact]
        public void Animals_ExtractsDogClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var node = r.Node("class", "Dog");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_Dog_HasPublicMethodMakeSound()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var node = r.ChildNode("Dog", "method", "MakeSound");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_Dog_HasPublicMethodFetch()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.NotNull(r.ChildNode("Dog", "method", "Fetch"));
        }

        [SkippableFact]
        public void Animals_Dog_HasPrivateMethodLogDomestication()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var node = r.ChildNode("Dog", "method", "LogDomestication");
            Assert.NotNull(node);
            Assert.Equal("private", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_ExtractsCatClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasNode("class", "Cat"));
        }

        [SkippableFact]
        public void Animals_ExtractsAnimalShelterClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasNode("class", "AnimalShelter"));
        }

        // --- GeometryTypes.vb ---

        [SkippableFact]
        public void Geometry_ExtractsDirectionEnum()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            Assert.True(r.HasNode("enum", "Direction"));
        }

        [SkippableFact]
        public void Geometry_Direction_HasFourMembers()
        {
            Xunit.Skip.If(!Available, Skip);
            var r         = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            var enumNode  = r.Node("enum", "Direction");
            Assert.NotNull(enumNode);
            var members   = r.Nodes.FindAll(n => n.Kind == "enum_member" && n.ParentId == enumNode!.Id);
            Assert.Equal(4, members.Count);
        }

        [SkippableFact]
        public void Geometry_ExtractsColourEnum()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            Assert.True(r.HasNode("enum", "Colour"));
        }

        [SkippableFact]
        public void Geometry_ExtractsPointStructure()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            var node = r.Node("struct", "Point");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Geometry_Point_HasMethodDistanceTo()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            Assert.NotNull(r.ChildNode("Point", "method", "DistanceTo"));
        }

        [SkippableFact]
        public void Geometry_ExtractsRectangleStructure()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            Assert.True(r.HasNode("struct", "Rectangle"));
        }

        [SkippableFact]
        public void Geometry_ExtractsGeometryUtilsModule()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            var node = r.Node("module", "GeometryUtils");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Geometry_GeometryUtils_HasMidpointMethod()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            Assert.NotNull(r.ChildNode("GeometryUtils", "method", "Midpoint"));
        }

        [SkippableFact]
        public void Geometry_GeometryUtils_MidpointIsStatic()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/GeometryTypes.vb");
            var node = r.ChildNode("GeometryUtils", "method", "Midpoint");
            Assert.NotNull(node);
            Assert.True(node!.IsStatic, "Module members must be marked isStatic = true");
        }

        // --- EventSystem.vb ---

        [SkippableFact]
        public void Events_ExtractsCounterClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            Assert.True(r.HasNode("class", "Counter"));
        }

        [SkippableFact]
        public void Events_ExtractsAlarmClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            Assert.True(r.HasNode("class", "Alarm"));
        }

        [SkippableFact]
        public void Events_Alarm_HasWithEventsField()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            var node = r.ChildNode("Alarm", "field", "_counter");
            Assert.NotNull(node);
        }

        // --- PartialClass files ---

        [SkippableFact]
        public void Partial_Part1_ExtractsDataProcessorClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part1.vb");
            Assert.True(r.HasNode("class", "DataProcessor"));
        }

        [SkippableFact]
        public void Partial_Part1_HasConstructor()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part1.vb");
            Assert.NotNull(r.ChildNode("DataProcessor", "method", "New"));
        }

        [SkippableFact]
        public void Partial_Part2_ExtractsDataProcessorClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part2.vb");
            Assert.True(r.HasNode("class", "DataProcessor"));
        }

        [SkippableFact]
        public void Partial_Part2_HasProcessMethod()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part2.vb");
            Assert.NotNull(r.ChildNode("DataProcessor", "method", "Process"));
        }

        // --- ImportAliases.vb ---

        [SkippableFact]
        public void Imports_ExtractsAliasDemoClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/ImportAliases.vb");
            Assert.True(r.HasNode("class", "AliasDemo"));
        }

        [SkippableFact]
        public void Imports_ExtractsOuterClassWithInnerClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/ImportAliases.vb");
            Assert.True(r.HasNode("class", "OuterClass"));
            Assert.True(r.HasNode("class", "InnerClass"));
        }

        [SkippableFact]
        public void Imports_InnerClassParentIsOuterClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r     = FixtureRunner.RunOnFile("VbFixture/ImportAliases.vb");
            var outer = r.Node("class", "OuterClass");
            var inner = r.Node("class", "InnerClass");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            Assert.Equal(outer!.Id, inner!.ParentId);
        }

        // --- AsyncPatterns.vb ---

        [SkippableFact]
        public void Async_ExtractsHttpDataFetcherClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/AsyncPatterns.vb");
            Assert.True(r.HasNode("class", "HttpDataFetcher"));
        }

        [SkippableFact]
        public void Async_FetchAsync_IsMarkedAsync()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/AsyncPatterns.vb");
            var node = r.ChildNode("HttpDataFetcher", "method", "FetchAsync");
            Assert.NotNull(node);
            Assert.True(node!.IsAsync, "FetchAsync must have isAsync = true");
        }

        [SkippableFact]
        public void Async_FireAndForget_IsMarkedAsync()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/AsyncPatterns.vb");
            var node = r.ChildNode("UrlProcessor", "method", "FireAndForget");
            Assert.NotNull(node);
            Assert.True(node!.IsAsync, "Async Sub must have isAsync = true");
        }

        [SkippableFact]
        public void Async_GetLengthSync_IsNotMarkedAsync()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("VbFixture/AsyncPatterns.vb");
            var node = r.ChildNode("UrlProcessor", "method", "GetLengthSync");
            Assert.NotNull(node);
            Assert.False(node!.IsAsync);
        }

        // --- Line numbers sanity ---

        [SkippableFact]
        public void AllNodes_HaveValidLineNumbers()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            foreach (var node in r.Nodes)
            {
                Assert.True(node.StartLine >= 1,
                    $"Node {node.Kind}:{node.Name} has StartLine < 1");
                Assert.True(node.EndLine >= node.StartLine,
                    $"Node {node.Kind}:{node.Name} has EndLine < StartLine");
            }
        }

        [SkippableFact]
        public void AllNodes_HaveNonEmptyIds()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            foreach (var node in r.Nodes)
                Assert.False(string.IsNullOrWhiteSpace(node.Id),
                    $"Node {node.Kind}:{node.Name} has empty Id");
        }

        [SkippableFact]
        public void Output_HasNoErrors()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.Empty(r.Errors);
        }
    }
}
