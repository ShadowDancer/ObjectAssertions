using Microsoft.CodeAnalysis;
using ObjectAssertions.Generator;

namespace ObjectAssertions
{
    [Generator]
    public class ObjectAssertionsSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            Debugger.Launch();
#endif
            new GenerationOrchestrator(context).Generate();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
