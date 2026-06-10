using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace ObjectAssertions.Generator.Models
{
    internal readonly struct GenerationResult
    {
        public GenerationResult(string hintName, string? source, ImmutableArray<Diagnostic> diagnostics)
        {
            HintName = hintName;
            Source = source;
            Diagnostics = diagnostics;
        }

        public string HintName { get; }

        public string? Source { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}
