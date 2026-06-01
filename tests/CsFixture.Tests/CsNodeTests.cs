using Xunit;

namespace CsFixture.Tests
{
    // Verifies that all expected node types are extracted from the C# fixture files.
    public class CsNodeTests
    {
        private const string Skip = "Set CODEGRAPH_ROSLYN_BIN to run extraction tests";
        private static bool Available => FixtureRunner.IsBinaryAvailable();

        // --- Animals.cs ---

        [SkippableFact]
        public void Animals_ExtractsIAnimalInterface()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var node = r.Node("interface", "IAnimal");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_IAnimal_HasFourMembers()
        {
            Xunit.Skip.If(!Available, Skip);
            var r      = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var iAnimal = r.Node("interface", "IAnimal");
            Assert.NotNull(iAnimal);
            var members = r.Nodes.FindAll(n => n.ParentId == iAnimal!.Id);
            Assert.Equal(4, members.Count);
        }

        [SkippableFact]
        public void Animals_ExtractsAnimalAbstractClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var node = r.Node("class", "Animal");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_Animal_HasPrivateReadonlyField()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var node = r.ChildNode("Animal", "field", "_name");
            Assert.NotNull(node);
            Assert.Equal("private", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_Animal_HasProtectedField()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var node = r.ChildNode("Animal", "field", "_sound");
            Assert.NotNull(node);
            Assert.Equal("protected", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_ExtractsDogClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasNode("class", "Dog"));
        }

        [SkippableFact]
        public void Animals_Dog_HasPublicMethodMakeSound()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var node = r.ChildNode("Dog", "method", "MakeSound");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_Dog_HasPrivateMethodLogDomestication()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            var node = r.ChildNode("Dog", "method", "LogDomestication");
            Assert.NotNull(node);
            Assert.Equal("private", node!.Visibility);
        }

        [SkippableFact]
        public void Animals_ExtractsCatAndShelterClasses()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasNode("class", "Cat"));
            Assert.True(r.HasNode("class", "AnimalShelter"));
        }

        // --- GeometryTypes.cs ---

        [SkippableFact]
        public void Geometry_ExtractsDirectionEnum()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            Assert.True(r.HasNode("enum", "Direction"));
        }

        [SkippableFact]
        public void Geometry_Direction_HasFourMembers()
        {
            Xunit.Skip.If(!Available, Skip);
            var r        = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var enumNode = r.Node("enum", "Direction");
            Assert.NotNull(enumNode);
            var members  = r.Nodes.FindAll(n => n.Kind == "enum_member" && n.ParentId == enumNode!.Id);
            Assert.Equal(4, members.Count);
        }

        [SkippableFact]
        public void Geometry_ExtractsPointStruct()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var node = r.Node("struct", "Point");
            Assert.NotNull(node);
            Assert.Equal("public", node!.Visibility);
        }

        [SkippableFact]
        public void Geometry_ExtractsRectangleRecordStruct()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            // record struct maps to NodeKind "struct"
            Assert.True(r.HasNode("struct", "Rectangle"),
                "record struct must be extracted with kind = struct");
        }

        [SkippableFact]
        public void Geometry_ExtractsPersonRecord()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            // record class maps to NodeKind "class"
            Assert.True(r.HasNode("class", "PersonRecord"),
                "record class must be extracted with kind = class");
        }

        [SkippableFact]
        public void Geometry_ExtractsGeometryUtilsStaticClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var node = r.Node("class", "GeometryUtils");
            Assert.NotNull(node);
            Assert.True(node!.IsStatic, "GeometryUtils must be marked isStatic = true");
        }

        [SkippableFact]
        public void Geometry_GeometryUtils_MidpointIsStatic()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GeometryTypes.cs");
            var node = r.ChildNode("GeometryUtils", "method", "Midpoint");
            Assert.NotNull(node);
            Assert.True(node!.IsStatic);
        }

        // --- GenericTypes.cs ---

        [SkippableFact]
        public void Generics_ExtractsIRepositoryInterface()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            var node = r.Node("interface", "IRepository");
            Assert.NotNull(node);
        }

        [SkippableFact]
        public void Generics_ExtractsRepositoryBaseClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            Assert.True(r.HasNode("class", "RepositoryBase"));
        }

        [SkippableFact]
        public void Generics_ExtractsCustomerRepositoryClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            Assert.True(r.HasNode("class", "CustomerRepository"));
        }

        [SkippableFact]
        public void Generics_ExtractsCustomerEntityClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            var node = r.Node("class", "Customer");
            Assert.NotNull(node);
        }

        [SkippableFact]
        public void Generics_CollectionExtensions_IsStaticClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            var node = r.Node("class", "CollectionExtensions");
            Assert.NotNull(node);
            Assert.True(node!.IsStatic);
        }

        // --- EventSystem.cs ---

        [SkippableFact]
        public void Events_ExtractsCounterAlarmLoggerClasses()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/EventSystem.cs");
            Assert.True(r.HasNode("class", "Counter"));
            Assert.True(r.HasNode("class", "Alarm"));
            Assert.True(r.HasNode("class", "Logger"));
        }

        // --- AsyncPatterns.cs ---

        [SkippableFact]
        public void Async_FetchAsync_IsMarkedAsync()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/AsyncPatterns.cs");
            var node = r.ChildNode("HttpDataFetcher", "method", "FetchAsync");
            Assert.NotNull(node);
            Assert.True(node!.IsAsync);
        }

        [SkippableFact]
        public void Async_FireAndForget_IsMarkedAsync()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/AsyncPatterns.cs");
            var node = r.ChildNode("UrlProcessor", "method", "FireAndForget");
            Assert.NotNull(node);
            Assert.True(node!.IsAsync, "async void method must have isAsync = true");
        }

        [SkippableFact]
        public void Async_GetLengthSync_IsNotMarkedAsync()
        {
            Xunit.Skip.If(!Available, Skip);
            var r    = FixtureRunner.RunOnFile("CsFixture/AsyncPatterns.cs");
            var node = r.ChildNode("UrlProcessor", "method", "GetLengthSync");
            Assert.NotNull(node);
            Assert.False(node!.IsAsync);
        }

        // --- ImportAliases.cs ---

        [SkippableFact]
        public void Imports_ExtractsAliasDemoAndNestedClasses()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/ImportAliases.cs");
            Assert.True(r.HasNode("class", "AliasDemo"));
            Assert.True(r.HasNode("class", "OuterClass"));
            Assert.True(r.HasNode("class", "InnerClass"));
        }

        [SkippableFact]
        public void Imports_InnerClassParentIsOuterClass()
        {
            Xunit.Skip.If(!Available, Skip);
            var r     = FixtureRunner.RunOnFile("CsFixture/ImportAliases.cs");
            var outer = r.Node("class", "OuterClass");
            var inner = r.Node("class", "InnerClass");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            Assert.Equal(outer!.Id, inner!.ParentId);
        }

        // --- Line numbers and ID sanity ---

        [SkippableFact]
        public void AllNodes_HaveValidLineNumbers()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            foreach (var node in r.Nodes)
            {
                Assert.True(node.StartLine >= 1,
                    $"Node {node.Kind}:{node.Name} StartLine < 1");
                Assert.True(node.EndLine >= node.StartLine,
                    $"Node {node.Kind}:{node.Name} EndLine < StartLine");
            }
        }

        [SkippableFact]
        public void Output_HasNoErrors()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.Empty(r.Errors);
        }
    }
}
