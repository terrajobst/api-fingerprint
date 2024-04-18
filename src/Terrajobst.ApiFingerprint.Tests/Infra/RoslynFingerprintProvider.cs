using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Terrajobst.ApiFingerprint.Tests.Infra;

public sealed class RoslynFingerprintProvider : FingerprintProvider
{
    public override FingerprintResult[] GetFingerprints(byte[] assemblyBytes)
    {
        var assemblyReference = MetadataReference.CreateFromImage(assemblyBytes);
        var compilation = CSharpCompilation.Create("dummy", references: [..Net70.References.All, assemblyReference]);
        var assembly = (IAssemblySymbol) compilation.GetAssemblyOrModuleSymbol(assemblyReference)!;
        return GetFingerprints(assembly);
    }

    private static FingerprintResult[] GetFingerprints(IAssemblySymbol assembly)
    {
        var result = new HashSet<FingerprintResult>();
        WalkAssembly(assembly, result);
        return result.ToArray();
    }

    private static void WalkAssembly(IAssemblySymbol assembly, HashSet<FingerprintResult> result)
    {
        foreach (var type in GetAllTypes(assembly))
            WalkType(type, result);
    }

    private static void WalkType(INamedTypeSymbol symbol, HashSet<FingerprintResult> result)
    {
        AddFingerprint(symbol, result);
        
        foreach (var member in symbol.GetMembers())
            WalkMember(member, result);
    }

    private static void WalkMember(ISymbol symbol, HashSet<FingerprintResult> result)
    {
        if (symbol is INamedTypeSymbol type)
        {
            WalkType(type, result);
            return;
        }
        
        AddFingerprint(symbol, result);
    }

    private static void AddFingerprint(ISymbol symbol, HashSet<FingerprintResult> result)
    {
        var fingerprintValue = symbol.GetApiFingerprint();
        if (fingerprintValue is not null)
        {
            var documentationCommentId = symbol.GetDocumentationCommentId() ?? string.Empty;
            var fingerprint = new FingerprintResult(documentationCommentId, fingerprintValue.Value);
            result.Add(fingerprint);
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(IAssemblySymbol symbol)
    {
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(symbol.GlobalNamespace);

        while (stack.Count > 0)
        {
            var ns = stack.Pop();
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol childNs)
                    stack.Push(childNs);
                else if (member is INamedTypeSymbol type)
                    yield return type;
            }
        }
    }
}