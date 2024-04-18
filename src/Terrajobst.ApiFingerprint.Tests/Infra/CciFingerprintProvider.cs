using Basic.Reference.Assemblies;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;

namespace Terrajobst.ApiFingerprint.Tests.Infra;

public sealed class CciFingerprintProvider : FingerprintProvider
{
    public override FingerprintResult[] GetFingerprints(byte[] assemblyBytes)
    {
        var result = new HashSet<FingerprintResult>();

        var host = new HostEnvironment();
        foreach (var reference in Net70.ReferenceInfos.All)
            AddToHost(reference, host);

        var assembly = AddToHost(host, assemblyBytes, "dummy");

        foreach (var type in assembly.GetAllTypes())
            WalkType(type, result);

        return result.ToArray();
    }

    private void WalkType(ITypeDefinition type, HashSet<FingerprintResult> result)
    {
        foreach (var member in type.Members)
            WalkMember(member, result);

        AddFingerprint(type, result);
    }

    private void WalkMember(ITypeDefinitionMember member, HashSet<FingerprintResult> result)
    {
        if (member is ITypeDefinition type)
        {
            WalkType(type, result);
            return;
        }

        AddFingerprint(member, result);
    }

    private static void AddFingerprint(IReference reference, HashSet<FingerprintResult> result)
    {
        var fingerprint = reference.GetApiFingerprint();
        if (fingerprint is not null)
        {
            var documentationId = reference.RefDocId();
            var fingerprintResult = new FingerprintResult(documentationId, fingerprint.Value);
            result.Add(fingerprintResult);
        }
    }

    private static void AddToHost(Net70.ReferenceInfo reference, HostEnvironment host)
    {
        var location = reference.FileName;
        var bytes = reference.ImageBytes;
        AddToHost(host, bytes, location);
    }

    private static IAssembly AddToHost(HostEnvironment host, byte[] bytes, string location)
    {
        var memoryStream = new MemoryStream(bytes);
        return host.LoadAssemblyFrom(location, memoryStream);
    }
}