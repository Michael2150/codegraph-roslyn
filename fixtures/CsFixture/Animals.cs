using System.Collections.Generic;

namespace Fixtures
{
    // Covers: interface, method signatures, property signatures
    public interface IAnimal
    {
        string Name { get; }
        string Sound { get; }
        string Describe();
        void MakeSound();
    }

    // Covers: second interface on same type (multiple implements)
    public interface IDomesticated
    {
        string Owner { get; }
        void Domesticate(string ownerName);
    }

    // Covers: abstract class, interface implementation, fields, properties,
    //         virtual method, abstract method
    public abstract class Animal : IAnimal
    {
        private readonly string _name;
        protected string _sound;

        protected Animal(string name, string sound)
        {
            _name = name;
            _sound = sound;
        }

        public string Name => _name;
        public string Sound => _sound;

        public virtual string Describe() => $"I am {_name} and I say {_sound}";
        public abstract void MakeSound();
    }

    // Covers: : BaseClass, multiple interfaces, override, base call,
    //         object instantiation, cross-method call
    public class Dog : Animal, IDomesticated
    {
        private string _owner = string.Empty;

        public Dog(string name) : base(name, "Woof") { }

        public string Owner => _owner;

        public override void MakeSound() => Console.WriteLine(_sound);

        public override string Describe()
        {
            var baseDesc = base.Describe();
            return $"{baseDesc}, and I am a dog";
        }

        public void Domesticate(string ownerName)
        {
            _owner = ownerName;
            LogDomestication(ownerName);
        }

        private void LogDomestication(string ownerName)
        {
            Console.WriteLine($"{Name} domesticated by {ownerName}");
        }

        public void Fetch(string item)
        {
            Console.WriteLine($"{Name} fetches {item}");
        }
    }

    // Covers: single inheritance only
    public class Cat : Animal
    {
        public Cat(string name) : base(name, "Meow") { }
        public override void MakeSound() => Console.WriteLine(_sound);
    }

    // Covers: new List<>() instantiation, foreach call to interface method
    public class AnimalShelter
    {
        private readonly List<IAnimal> _animals = new List<IAnimal>();

        public void Add(IAnimal animal) => _animals.Add(animal);

        public void MakeAllSounds()
        {
            foreach (var animal in _animals)
                animal.MakeSound();
        }

        public IAnimal FindByName(string name)
        {
            var dog = new Dog(name);
            return dog;
        }

        public int Count() => _animals.Count;
    }
}
