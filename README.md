# API Fingerprinting

The purpose of this repository is to define a standardized format for generating
string-based identifier that uniquely identify a specific .NET API. It's
designed to distinguish between different overloads as well as handling of
generics.

This repo provides implementations for various metadata readers:

* [Roslyn (C#/VB compilers)][impl-roslyn]
* [Mono Cecil][impl-cecil]
* [Microsoft CCI][impl-cci]
* Reflection / MetadataLoadContext (TBD)

## The Problem

We have various static analysis tools that report information about .NET APIs.
We often want to correlate outputs from different tools but this is virtually
impossible because each tool differs in the way it represents APIs.

For example, let's say a tool reports about my usage of `List<Customer>.Add()`.
There are various ways this can be expressed:

* `List<T>.Add(T)`
* `List<Customer>.Add(Customer)`
* `System.Collections.Generic.List<Contoso.Models.Customer>(Contoso.Models.Customer)`
* `System.Collections.Generic.List``1(``0)`
* ...

There is a format that is designed to identity APIs uniquely by name
(ignoring assembly identity, which is generally what we want because different
frameworks put the same APIs in different assemblies). It's often referred to
as the documentation id, or doc id. It's the format the compilers use when
emitting the documentation XML files. It's documented [here][doc-id].

For the above example, the doc id would look like this:

* `M:System.Collections.Generic.List{Contoso.Models.Customer}(Contoso.Models.Customer)`

The general format is using namespace-qualified type names (omitting assembly
information) which makes them quite verbose. This is often no prohibitively
expensive when reporting large amounts of APIs. In some cases it also
complicates the data storage as most system put limits on how long a value can
be when generating and index (SQL Server has a 1700 byte limitation). Some of
the documentation ids generated for .NET platform APIs exceed that.

## The solution

Instead of using the documentation ID directly, we use a hash-derived GUID. That
ensures uniformity in size of the fingerprint while still practically being
unique.

This approach has been used for years by the .NET team for various assets, be that
the documentation platform or the [API Catalog].

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
> For example, each overload of `M` has its own fingerprint.
> ```C#
> class C {
>     void M(List<Customer> customers) { }
>     void M(List<int> customerIds) { }
> }
> ```

The API fingerprint for an extension method is the same as it is for the static
definition of the extension method. This is mostly relevant in cases of static
analysis tools like Roslyn which produce synthetic symbols for extension methods
that look like instance methods.

[doc-id]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d4-processing-the-documentation-file
[API Catalog]: https://apisof.net
[impl-roslyn]: src/Terrajobst.ApiFingerprint.Roslyn/ApiFingerprint.cs
[impl-cecil]: src/Terrajobst.ApiFingerprint.Cecil/ApiFingerprint.cs 
[impl-cci]: src/Terrajobst.ApiFingerprint.Cci/ApiFingerprint.cs
