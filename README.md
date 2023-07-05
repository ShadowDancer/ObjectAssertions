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
        IgnoredProperty = ObjectAssertionsHelpers.Ignore<Foo>("Not in test scope"")
    };

    assertions.Assert();
}
```

All delegates passed to object initializer will be invoked within `.Assert()` call. 
You can use `ObjectAssertionsHelpers.Ignore<T>(string reason)` method to ignore certain assertions and express intent.

## Assertion library integrations

### FluentAssertions

Use `AssertionScope` class:

```
using (new AssertionScope())
{
    new MyClassAssertions(myClass)
    {
        MyInt = i => i.Should().Be(5),        
        MyString = s => s.Should().Be("Hello"),
        MyBool = b => Should().Be(true),
        IgnoredProperty = ObjectAssertionsHelpers.Ignore<Foo>("Not in test scope"")
    }.Assert();
}
```

### NUnit

Use `Assert.Multiple` method:

```
Assert.Multiple(() =>
{
    new MyClassAssertions(myClass)
    {
        MyInt = i => Assert.AreEqual(5, i),        
        MyString = s => Assert.AreEqual("Hello", s),
        MyBool = b => Assert.AreEqual(true, b),
        IgnoredProperty = ObjectAssertionsHelpers.Ignore<Foo>("Not in test scope"")
    }.Assert();
}
```

### Shoudly

Use ShouldSatisfyAllConditions extension method:

```
var assertions = new MyClassAssertions(myClass)
{
    MyInt = i => i.ShouldBe(5),        
    MyString = s => s.ShouldBe("Hello"),
    MyBool = b => ShouldBe(true),
    IgnoredProperty = ObjectAssertionsHelpers.Ignore<Foo>("Not in test scope"")
}.CollectAssertions();

myClass.ShouldSatisfyAllConditions(assertions);
```