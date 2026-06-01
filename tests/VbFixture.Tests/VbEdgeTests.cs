using Xunit;

namespace VbFixture.Tests
{
    // Verifies that all expected edges are extracted from the VB fixture files.
    public class VbEdgeTests
    {
        private const string Skip = "Set CODEGRAPH_ROSLYN_BIN to run extraction tests";
        private static bool Available => FixtureRunner.IsBinaryAvailable();

        // --- extends edges ---

        [SkippableFact]
        public void Animals_Dog_ExtendsAnimal()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("extends", "Dog", "Animal"),
                "Expected Dog -extends-> Animal");
        }

        [SkippableFact]
        public void Animals_Cat_ExtendsAnimal()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("extends", "Cat", "Animal"),
                "Expected Cat -extends-> Animal");
        }

        [SkippableFact]
        public void Events_ThresholdReachedEventArgs_ExtendsEventArgs()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            Assert.True(r.HasEdge("extends", "ThresholdReachedEventArgs", "EventArgs"),
                "Expected ThresholdReachedEventArgs -extends-> EventArgs");
        }

        // --- implements edges ---

        [SkippableFact]
        public void Animals_Animal_ImplementsIAnimal()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("implements", "Animal", "IAnimal"),
                "Expected Animal -implements-> IAnimal");
        }

        [SkippableFact]
        public void Animals_Dog_ImplementsIDomesticated()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("implements", "Dog", "IDomesticated"),
                "Expected Dog -implements-> IDomesticated");
        }

        [SkippableFact]
        public void Async_HttpDataFetcher_ImplementsIDataFetcher()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/AsyncPatterns.vb");
            Assert.True(r.HasEdge("implements", "HttpDataFetcher", "IDataFetcher"),
                "Expected HttpDataFetcher -implements-> IDataFetcher");
        }

        // --- overrides edges ---

        [SkippableFact]
        public void Animals_DogMakeSound_OverridesAnimalMakeSound()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("overrides", "MakeSound", "MakeSound"),
                "Expected Dog.MakeSound -overrides-> Animal.MakeSound");
        }

        [SkippableFact]
        public void Animals_DogDescribe_OverridesAnimalDescribe()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("overrides", "Describe", "Describe"),
                "Expected Dog.Describe -overrides-> Animal.Describe");
        }

        // --- calls edges (intra-file) ---

        [SkippableFact]
        public void Animals_Dog_Domesticate_CallsLogDomestication()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("calls", "Domesticate", "LogDomestication"),
                "Expected Domesticate -calls-> LogDomestication");
        }

        [SkippableFact]
        public void Events_Counter_Increment_CallsOnThresholdReached()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            Assert.True(r.HasEdge("calls", "Increment", "OnThresholdReached"),
                "Expected Increment -calls-> OnThresholdReached");
        }

        [SkippableFact]
        public void Events_Alarm_ThresholdHandler_CallsTriggerAlarm()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            Assert.True(r.HasEdge("calls", "Counter_ThresholdReached", "TriggerAlarm"),
                "Expected Counter_ThresholdReached -calls-> TriggerAlarm");
        }

        [SkippableFact]
        public void Partial_Process_CallsReadLines()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part2.vb");
            Assert.True(r.HasEdge("calls", "Process", "ReadLines"),
                "Expected Process -calls-> ReadLines");
        }

        [SkippableFact]
        public void Partial_Process_CallsTransform()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part2.vb");
            Assert.True(r.HasEdge("calls", "Process", "Transform"),
                "Expected Process -calls-> Transform");
        }

        [SkippableFact]
        public void Partial_Process_CallsWriteResults()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/PartialClass.Part2.vb");
            Assert.True(r.HasEdge("calls", "Process", "WriteResults"),
                "Expected Process -calls-> WriteResults");
        }

        // --- instantiates edges ---

        [SkippableFact]
        public void Animals_AnimalShelter_FindByName_InstantiatesDog()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            Assert.True(r.HasEdge("instantiates", "FindByName", "Dog"),
                "Expected FindByName -instantiates-> Dog");
        }

        [SkippableFact]
        public void Events_Counter_Increment_InstantiatesThresholdReachedEventArgs()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/EventSystem.vb");
            Assert.True(r.HasEdge("instantiates", "Increment", "ThresholdReachedEventArgs"),
                "Expected Increment -instantiates-> ThresholdReachedEventArgs");
        }

        // --- imports edges ---

        [SkippableFact]
        public void Imports_FileHasImportNodes()
        {
            Xunit.Skip.If(!Available, Skip);
            var r = FixtureRunner.RunOnFile("VbFixture/ImportAliases.vb");
            Assert.NotEmpty(r.Nodes.FindAll(n => n.Kind == "import"));
        }

        // --- contains edges ---

        [SkippableFact]
        public void Animals_AnimalClass_ContainsMakeSoundMethod()
        {
            Xunit.Skip.If(!Available, Skip);
            var r      = FixtureRunner.RunOnFile("VbFixture/Animals.vb");
            var animal = r.Node("class", "Animal");
            var method = r.ChildNode("Animal", "method", "Describe");
            Assert.NotNull(animal);
            Assert.NotNull(method);
            Assert.True(r.HasEdge("contains", "Animal", "Describe"),
                "Expected Animal -contains-> Describe");
        }
    }
}
