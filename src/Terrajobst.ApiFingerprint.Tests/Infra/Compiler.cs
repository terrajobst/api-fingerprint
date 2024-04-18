using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Terrajobst.ApiFingerprint.Tests.Infra;

internal static class Compiler
{
    public static byte[] Compile(string source)
    {
        return Compile(source, c => c);
    }

    public static byte[] Compile(string source, Func<CSharpCompilation, CSharpCompilation> modifyCompilation)
    {
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                                  optimizationLevel: OptimizationLevel.Release);
        var compilation = CSharpCompilation.Create("dummy",
                                                   [ CSharpSyntaxTree.ParseText(source) ],
                                                   Net70.References.All,
                                                   options);

        compilation = modifyCompilation(compilation);

        using var peStream = new MemoryStream();
        var result = compilation.Emit(peStream);
        if (!result.Success)
        {
            var diagnostics = string.Join(Environment.NewLine, result.Diagnostics);
            var message = $"Compilation has errors{Environment.NewLine}{diagnostics}";
            throw new Exception(message);
        }

        peStream.Position = 0;

        return peStream.ToArray();
    }
}