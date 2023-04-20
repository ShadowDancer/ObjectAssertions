# Object Assertions Library

ObjectAssertions is a C# library that provides a code generator for writing robust and future-proof test cases by enabling compile-time checks for all properties of an object.

With ObjectAssertions, developers can easily generate assertions for all properties of their classes and structs by implementing the `IAssertsAllPropertiesOf` interface. The library is designed to provide compile-time safety by making it impossible to forget to test any newly added properties. If a new property is added to a class, the code won't compile until a corresponding assertion is added.

## Quickstart


Let's assume this is class you want to test

```csharp
public class MyClass
{
    public int MyInt { get; set; }
    public string MyString { get; set; }
    public bool MyBool { get; set; }
}
```


Add package:

```bash
dotnet add package ObjectAssertions.Abstractions
dotnet add package ObjectAssertions.Generator
```

Define assertions class:

```csharp

using ObjectAssertions.Abstractions;

public class MyClassAssertions : IAssertsAllPropertiesOf<MyClass>
{
}
```

Define test:

```csharp
[Fact]
public void TestMyClass()
{
    var myClass = new MyClass { MyInt = 5, MyString = "Hello", MyBool = true };
    var assertions = new MyClassAssertions(myClass)
    {
        MyInt = i => Assert.Equal(5, i),        
        MyString = s => Assert.Equal("Hello", s),
        MyBool = b => Assert.Equal(true, b),
    };

    assertions.Assert();
}
```