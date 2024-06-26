﻿namespace Terrajobst.ApiFingerprint.Tests.Infra;

public abstract class FingerprintProvider
{
    public abstract FingerprintResult[] GetFingerprints(byte[] assemblyBytes);

    public static IReadOnlyList<FingerprintProvider> All { get; } = GetAllProviders();

    private static IReadOnlyList<FingerprintProvider> GetAllProviders()
    {
        return typeof(FingerprintProvider).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(FingerprintProvider).IsAssignableFrom(t))
            .Select(t => (FingerprintProvider?)Activator.CreateInstance(t))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToArray();
    }

    public FingerprintResult[] GetFingerprintsExcludingAutoGenerated(byte[] assemblyBytes)
    {
        var result = GetFingerprints(assemblyBytes);
        return result.Where(f => !AutoGeneratedResults.Contains(f)).ToArray();
    }
    
    private static readonly HashSet<FingerprintResult> AutoGeneratedResults = [
        new FingerprintResult("T:<Module>"),
        new FingerprintResult("T:Microsoft.CodeAnalysis.EmbeddedAttribute"),
        new FingerprintResult("M:Microsoft.CodeAnalysis.EmbeddedAttribute.#ctor"),
        new FingerprintResult("T:System.Runtime.CompilerServices.RefSafetyRulesAttribute"),
        new FingerprintResult("M:System.Runtime.CompilerServices.RefSafetyRulesAttribute.#ctor(System.Int32)"),
        new FingerprintResult("F:System.Runtime.CompilerServices.RefSafetyRulesAttribute.Version")
    ];
}