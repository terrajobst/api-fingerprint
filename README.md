# API Fingerprinting

The purpose of this repository is to define a standardized format for generating
fingerprints that uniquely identify a specific .NET API. It's designed to
distinguish between overloads as well as handling of generics.

This repo provides implementations for various metadata readers:

* [Roslyn (C#/VB compilers)][impl-roslyn]
* [Mono Cecil][impl-cecil]
* [Microsoft CCI][impl-cci]
* Reflection / MetadataLoadContext (TBD)

## The Problem

We have various static analysis tools that report information about .NET APIs.
We often want to correlate outputs from different tools but this is virtually
impossible because each tool differs in the way it represents them.

For example, let's say a tool reports about the usage of
`List<Customer>.Add(Customer)`. There are various ways this can be expressed:

* `List<T>.Add(T)`
* `List<Customer>.Add(Customer)`
* `System.Collections.Generic.List<Contoso.Models.Customer>(Contoso.Models.Customer)`
* `System.Collections.Generic.List``1(``0)`
* ...

There is a format that is designed to identity APIs uniquely by name (ignoring
assembly identity, which is generally what we want because different frameworks
put the same APIs in different assemblies). It's often referred to as the
*documentation id*, or doc id. It's the format the compilers use when emitting
the XML documentation files. The format documented [here][doc-id].

For the above example, the doc id looks like this:

* `M:System.Collections.Generic.List{Contoso.Models.Customer}(Contoso.Models.Customer)`

The format uses namespace-qualified type names (omitting assembly information)
which makes them quite verbose. This is often prohibitively expensive when
reporting large amounts of APIs. In some cases it also complicates the data
storage as many systems put limits on how large a value can be when creating an
index over them (for example, SQL Server has a 1700 byte limitation). Some of
the documentation ids generated for .NET platform APIs exceed that.

## The solution

Instead of using the doc id directly, we use a hash-derived GUID (which is 16
bytes). That ensures uniformity in size of the fingerprint while still
practically being unique.

This approach has been used for many years by the .NET team for various assets,
including the documentation platform and [API Catalog].

## Format

An API fingerprint is the GUID derived from the MD5 hash of the UTF8 encoded
documentation id.

If the API is an instantiated generic type, an instantiated generic method, or a
member of an instantiated generic type, the fingerprint shall be computed for
its original (uninstantiated) generic form. For example, the fingerprint for
`List<Customer>` is the same as for `List<T>`).

> [!NOTE]
>
> Please note that this **doesn't mean** that types of method parameters should
> be reduced to their original form. A non-generic method that takes parameters
> that are instantiated generic types should be interpreted as-is, otherwise
> overloading based on different instantiations would be indistinguishable.
> 
> For example, each overload of `M` should have its own fingerprint:
>
> ```C#
> class C {
>     void M(List<Customer> customers) { }
>     void M(List<int> customerIds) { }
> }
> ```

The API fingerprint for an extension method is the same as it is for the static
definition of the extension method. This is mostly relevant in cases of static
analysis tools like Roslyn which produce synthetic symbols for extension methods
that look like instance methods. It's generally meaningless for tools that parse
IL as IL has no special syntax for calling extension methods.

[doc-id]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d4-processing-the-documentation-file
[API Catalog]: https://apisof.net
[impl-roslyn]: src/Terrajobst.ApiFingerprint.Roslyn/ApiFingerprint.cs
[impl-cecil]: src/Terrajobst.ApiFingerprint.Cecil/ApiFingerprint.cs 
[impl-cci]: src/Terrajobst.ApiFingerprint.Cci/ApiFingerprint.cs
