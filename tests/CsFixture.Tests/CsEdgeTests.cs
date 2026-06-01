using Xunit;

namespace CsFixture.Tests
{
    // Verifies that all expected edges are extracted from the C# fixture files.
    public class CsEdgeTests
    {
        private const string Skip = "Set CODEGRAPH_ROSLYN_BIN to run extraction tests";
        private static bool Available => FixtureRunner.IsBinaryAvailable();

        // --- extends edges ---

        [SkippableFact]
        public void Animals_Dog_ExtendsAnimal()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("extends", "Dog", "Animal"),
                "Expected Dog -extends-> Animal");
        }

        [SkippableFact]
        public void Animals_Cat_ExtendsAnimal()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("extends", "Cat", "Animal"),
                "Expected Cat -extends-> Animal");
        }

        [SkippableFact]
        public void Events_ThresholdReachedEventArgs_ExtendsEventArgs()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/EventSystem.cs");
            Assert.True(r.HasEdge("extends", "ThresholdReachedEventArgs", "EventArgs"),
                "Expected ThresholdReachedEventArgs -extends-> EventArgs");
        }

        [SkippableFact]
        public void Generics_CustomerRepository_ExtendsRepositoryBase()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            Assert.True(r.HasEdge("extends", "CustomerRepository", "RepositoryBase"),
                "Expected CustomerRepository -extends-> RepositoryBase");
        }

        // --- implements edges ---

        [SkippableFact]
        public void Animals_Animal_ImplementsIAnimal()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("implements", "Animal", "IAnimal"),
                "Expected Animal -implements-> IAnimal");
        }

        [SkippableFact]
        public void Animals_Dog_ImplementsIDomesticated()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("implements", "Dog", "IDomesticated"),
                "Expected Dog -implements-> IDomesticated");
        }

        [SkippableFact]
        public void Async_HttpDataFetcher_ImplementsIDataFetcher()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/AsyncPatterns.cs");
            Assert.True(r.HasEdge("implements", "HttpDataFetcher", "IDataFetcher"),
                "Expected HttpDataFetcher -implements-> IDataFetcher");
        }

        [SkippableFact]
        public void Generics_RepositoryBase_ImplementsIRepository()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            Assert.True(r.HasEdge("implements", "RepositoryBase", "IRepository"),
                "Expected RepositoryBase -implements-> IRepository");
        }

        // --- overrides edges ---

        [SkippableFact]
        public void Animals_DogMakeSound_OverridesAnimalMakeSound()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("overrides", "MakeSound", "MakeSound"),
                "Expected Dog.MakeSound -overrides-> Animal.MakeSound");
        }

        [SkippableFact]
        public void Animals_DogDescribe_OverridesAnimalDescribe()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("overrides", "Describe", "Describe"),
                "Expected Dog.Describe -overrides-> Animal.Describe");
        }

        [SkippableFact]
        public void Generics_CustomerRepository_Save_OverridesRepositoryBase_Save()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            Assert.True(r.HasEdge("overrides", "Save", "Save"),
                "Expected CustomerRepository.Save -overrides-> RepositoryBase.Save");
        }

        // --- calls edges ---

        [SkippableFact]
        public void Animals_Dog_Domesticate_CallsLogDomestication()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("calls", "Domesticate", "LogDomestication"),
                "Expected Domesticate -calls-> LogDomestication");
        }

        [SkippableFact]
        public void Events_Alarm_Handler_CallsTriggerAlarm()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/EventSystem.cs");
            Assert.True(r.HasEdge("calls", "Counter_ThresholdReached", "TriggerAlarm"),
                "Expected Counter_ThresholdReached -calls-> TriggerAlarm");
        }

        [SkippableFact]
        public void Generics_CustomerRepository_Save_CallsValidateName()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/GenericTypes.cs");
            Assert.True(r.HasEdge("calls", "Save", "ValidateName"),
                "Expected Save -calls-> ValidateName");
        }

        [SkippableFact]
        public void Imports_OuterClass_Run_CallsCreateInner()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/ImportAliases.cs");
            Assert.True(r.HasEdge("calls", "Run", "CreateInner"),
                "Expected Run -calls-> CreateInner");
        }

        // --- instantiates edges ---

        [SkippableFact]
        public void Animals_AnimalShelter_FindByName_InstantiatesDog()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("instantiates", "FindByName", "Dog"),
                "Expected FindByName -instantiates-> Dog");
        }

        [SkippableFact]
        public void Events_Counter_Increment_InstantiatesThresholdReachedEventArgs()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/EventSystem.cs");
            Assert.True(r.HasEdge("instantiates", "Increment", "ThresholdReachedEventArgs"),
                "Expected Increment -instantiates-> ThresholdReachedEventArgs");
        }

        // --- imports ---

        [SkippableFact]
        public void Imports_FileHasImportNodes()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/ImportAliases.cs");
            Assert.NotEmpty(r.Nodes.FindAll(n => n.Kind == "import"));
        }

        // --- contains edges ---

        [SkippableFact]
        public void Animals_DogClass_ContainsDescribeMethod()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("CsFixture/Animals.cs");
            Assert.True(r.HasEdge("contains", "Dog", "Describe"),
                "Expected Dog -contains-> Describe");
        }
    }
}
