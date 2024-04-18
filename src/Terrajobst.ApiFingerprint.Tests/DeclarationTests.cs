using Terrajobst.ApiFingerprint.Tests.Infra;

namespace Terrajobst.ApiFingerprint.Tests;

public class DeclarationTests
{
    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Type(FingerprintProvider provider)
    {
        var source =
            """
            namespace N;

            public class C
            {
            }
            """;

        Check(provider, source, [
            "T:N.C",
            "M:N.C.#ctor",
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Type_WithoutNamespace(FingerprintProvider provider)
    {
        var source =
            """
            public class C
            {
            }
            """;

        Check(provider, source, [
            "T:C",
            "M:C.#ctor",
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Type_Nested(FingerprintProvider provider)
    {
        var source =
            """
            using System.Collections.Generic;

            namespace N;

            public class C
            {
                public class D
                {
                }
            }
            """;

        Check(provider, source, [
            "T:N.C",
            "M:N.C.#ctor",
            "T:N.C.D",
            "M:N.C.D.#ctor",
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Type_NestedGeneric(FingerprintProvider provider)
    {
        var source =
            """
            using System.Collections.Generic;

            namespace N;

            public class C
            {
                public class D<T>
                {
                }
            }
            """;

        Check(provider, source, [
            "T:N.C",
            "M:N.C.#ctor",
            "T:N.C.D`1",
            "M:N.C.D`1.#ctor",
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_TypeGeneric_Nested(FingerprintProvider provider)
    {
        var source =
            """
            using System.Collections.Generic;

            namespace N;

            public class C<T>
            {
                public class D
                {
                }
            }
            """;

        Check(provider, source, [
            "T:N.C`1",
            "M:N.C`1.#ctor",
            "T:N.C`1.D",
            "M:N.C`1.D.#ctor",
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Method(FingerprintProvider provider)
    {
        var source =
            """
            namespace N;

            public class C
            {
                public void M(int x) { }
            }
            """;

        Check(provider, source, [
            "T:N.C",
            "M:N.C.#ctor",
            "M:N.C.M(System.Int32)"
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Method_GenericOverloads(FingerprintProvider provider)
    {
        var source =
            """
            using System.Collections.Generic;

            namespace N;

            public class C
            {
                public void M(List<int> items) { }
                public void M(List<C> items) { }
            }
            """;

        Check(provider, source, [
            "T:N.C",
            "M:N.C.#ctor",
            "M:N.C.M(System.Collections.Generic.List{System.Int32})",
            "M:N.C.M(System.Collections.Generic.List{N.C})"
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_TypeGeneric_NestedGeneric(FingerprintProvider provider)
    {
        var source =
            """
            using System.Collections.Generic;

            namespace N;

            public class C<T>
            {
                public class D<K>
                {
                }
            }
            """;

        Check(provider, source, [
            "T:N.C`1",
            "M:N.C`1.#ctor",
            "T:N.C`1.D`1",
            "M:N.C`1.D`1.#ctor",
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Field(FingerprintProvider provider)
    {
        var source =
            """
            using System.Collections.Generic;

            namespace N;

            public class C
            {
                public int F;
            }
            """;

        Check(provider, source, [
            "T:N.C",
            "M:N.C.#ctor",
            "F:N.C.F"
        ]);
    }

    [Theory]
    [MemberData(nameof(GetProviders))]
    public void Declaration_Property(FingerprintProvider provider)
    {
        var source =
            """
            using System.Collections.Generic;

            namespace N;

            public class C
            {
                public int P1 { get => 1; }
                public int P2 { set {} }
                public int P3 { get => 1; set {} }
            }
            """;

        Check(provider, source, [
            "T:N.C",
            "M:N.C.#ctor",
            "P:N.C.P1",
            "M:N.C.get_P1",
            "P:N.C.P2",
            "M:N.C.set_P2(System.Int32)",
            "P:N.C.P3",
            "M:N.C.get_P3",
            "M:N.C.set_P3(System.Int32)",
        ]);
    }

    public static IEnumerable<object[]> GetProviders()
    {
        return FingerprintProvider.All.Select(p => new object[] { p });
    }
    
    private static void Check(FingerprintProvider provider, string source, string[] expectedDocIds)
    {
        var assembly = Compiler.Compile(source);
        var expectedFingerprints = expectedDocIds.Select(d => new FingerprintResult(d)).Order().ToArray();
       
        var actualFingerprints = provider.GetFingerprintsExcludingAutoGenerated(assembly);
        Array.Sort(actualFingerprints);
        Assert.Equal(expectedFingerprints, actualFingerprints);
    }
}

