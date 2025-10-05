using Microsoft.CodeAnalysis;
using ObjectAssertions.Abstractions;

namespace ObjectAssertions
{
    public static class Diagnostics
    {
        public static readonly DiagnosticDescriptor MissingAbstractions = new(
            "OBJASS0001",
            "Missing ObjectAssertions.Abstractions library",
            "Cannot resolve type. ObjectAssertions.Abstractions library must referenced.",
            "Object Assertions",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor NonPartialAssertions = new(
            "OBJASS0002",
            "Type must be partial",
            "{0} is not partial. Assertion classes must be partial in order to generate code.",
            "Object Assertions",
            DiagnosticSeverity.Error,
            true);


        public static readonly DiagnosticDescriptor AssertionsInNonPartialClass = new(
            "OBJASS0003",
            "Containing type must be partial",
            "{0} is nested type of {1} which is not partial. If assertion class is inside other type, containing types must be partial in order to generate code.",
            "Object Assertions",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor MultipleInterfaceDeclarations = new(
            "OBJASS0004",
            "Multiple interface declarations",
            "{0} can implement IAssertsAllPropertiesOf<TObject> only once",
            "Object Assertions",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor UnknownTypeName = new(
            "OBJASS0005",
            "Unsupported type",
            "Argument of IAssertsAllPropertiesOf<TObject> is not a regular type",
            "Object Assertions",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor AbstractClassesAreNotSupported = new(
            "OBJASS0006",
            "Unsupported type",
            "Argument of " + typeof(IAssertsAllPropertiesOf<>).Name + " in abstract class. Library does not support abstract classes, use concrete type instead.",
            "Object Assertions",
            DiagnosticSeverity.Error,
            true);
    }
}
