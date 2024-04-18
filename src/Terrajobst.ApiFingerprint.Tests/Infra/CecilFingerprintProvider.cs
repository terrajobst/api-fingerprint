using Basic.Reference.Assemblies;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Terrajobst.ApiFingerprint.Tests.Infra;

public sealed class CecilFingerprintProvider : FingerprintProvider
{
    public override FingerprintResult[] GetFingerprints(byte[] assemblyBytes)
    {
        var parameters = new ReaderParameters {
            AssemblyResolver = Resolver.Instance 
        };

        var result = new HashSet<FingerprintResult>();
        var stream = new MemoryStream(assemblyBytes);
        var assembly = AssemblyDefinition.ReadAssembly(stream, parameters);

        foreach (var module in assembly.Modules)
        foreach (var type in module.Types)
            WalkType(type, result);

        return result.ToArray();
    }

    private void WalkType(TypeDefinition type, HashSet<FingerprintResult> result)
    {
        AddFingerprint(type, result);

        foreach (var nestedType in type.NestedTypes)
            WalkType(nestedType, result);
        
        foreach (var method in type.Methods)
            AddFingerprint(method, result);

        foreach (var field in type.Fields)
            AddFingerprint(field, result);

        foreach (var property in type.Properties)
            AddFingerprint(property, result);

        foreach (var @event in type.Events)
            AddFingerprint(@event, result);
    }

    private static void AddFingerprint(IMemberDefinition member, HashSet<FingerprintResult> result)
    {
        var fingerprint = member.GetApiFingerprint();
        if (fingerprint is not null)
        {
            var docId = DocCommentId.GetDocCommentId(member);
            var fingerprintResult = new FingerprintResult(docId, fingerprint.Value);
            result.Add(fingerprintResult);
        }
    }

    private sealed class Resolver : IAssemblyResolver
    {
        public static Resolver Instance { get; } = new();

        private Resolver()
        {
        }
        
        public void Dispose()
        {
        }

        public AssemblyDefinition? Resolve(AssemblyNameReference name)
        {
            var reference = Net70.ReferenceInfos.All
                .Where(r => string.Equals(Path.GetFileNameWithoutExtension(r.FileName), name.Name, StringComparison.OrdinalIgnoreCase))
                .Cast<Net70.ReferenceInfo?>()
                .FirstOrDefault();
            
            if (reference is null)
                return null;

            using var stream = new MemoryStream(reference.Value.ImageBytes);
            return AssemblyDefinition.ReadAssembly(stream);
        }

        public AssemblyDefinition? Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return Resolve(name);
        }
    }
}