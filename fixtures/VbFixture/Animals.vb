Imports System.Collections.Generic

Namespace Fixtures

    ' Covers: interface, method signatures, property signatures
    Public Interface IAnimal
        ReadOnly Property Name As String
        ReadOnly Property Sound As String
        Function Describe() As String
        Sub MakeSound()
    End Interface

    ' Covers: second interface on same type (multiple implements)
    Public Interface IDomesticated
        ReadOnly Property Owner As String
        Sub Domesticate(ownerName As String)
    End Interface

    ' Covers: MustInherit class, Implements interface, fields, properties,
    '         Overridable method, MustOverride method
    Public MustInherit Class Animal
        Implements IAnimal

        Private _name As String
        Protected _sound As String

        Public Sub New(name As String, sound As String)
            _name = name
            _sound = sound
        End Sub

        Public ReadOnly Property Name As String Implements IAnimal.Name
            Get
                Return _name
            End Get
        End Property

        Public ReadOnly Property Sound As String Implements IAnimal.Sound
            Get
                Return _sound
            End Get
        End Property

        Public Overridable Function Describe() As String Implements IAnimal.Describe
            Return String.Format("I am {0} and I say {1}", _name, _sound)
        End Function

        Public MustOverride Sub MakeSound() Implements IAnimal.MakeSound

    End Class

    ' Covers: Inherits, multiple Implements, Overrides, MyBase call,
    '         object creation (instantiates edge), cross-method call
    Public Class Dog
        Inherits Animal
        Implements IDomesticated

        Private _owner As String

        Public Sub New(name As String)
            MyBase.New(name, "Woof")
        End Sub

        Public ReadOnly Property Owner As String Implements IDomesticated.Owner
            Get
                Return _owner
            End Get
        End Property

        Public Overrides Sub MakeSound()
            Console.WriteLine(_sound)
        End Sub

        Public Overrides Function Describe() As String
            Dim base As String = MyBase.Describe()
            Return base & ", and I am a dog"
        End Function

        Public Sub Domesticate(ownerName As String) Implements IDomesticated.Domesticate
            _owner = ownerName
            LogDomestication(ownerName)
        End Sub

        Private Sub LogDomestication(ownerName As String)
            Console.WriteLine(String.Format("{0} domesticated by {1}", Name, ownerName))
        End Sub

        Public Sub Fetch(item As String)
            Console.WriteLine(String.Format("{0} fetches {1}", Name, item))
        End Sub

    End Class

    ' Covers: Inherits only (no extra interfaces)
    Public Class Cat
        Inherits Animal

        Public Sub New(name As String)
            MyBase.New(name, "Meow")
        End Sub

        Public Overrides Sub MakeSound()
            Console.WriteLine(_sound)
        End Sub

    End Class

    ' Covers: New List(Of ...) instantiation, For Each loop call,
    '         calls to interface method via variable
    Public Class AnimalShelter
        Private _animals As New List(Of IAnimal)

        Public Sub Add(animal As IAnimal)
            _animals.Add(animal)
        End Sub

        Public Sub MakeAllSounds()
            For Each animal As IAnimal In _animals
                animal.MakeSound()
            Next
        End Sub

        Public Function FindByName(name As String) As IAnimal
            Dim dog As New Dog(name)
            Return dog
        End Function

        Public Function Count() As Integer
            Return _animals.Count
        End Function

    End Class

End Namespace
