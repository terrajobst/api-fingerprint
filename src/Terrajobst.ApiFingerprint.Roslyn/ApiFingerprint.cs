using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.CodeAnalysis;

public static class ApiFingerprint
{
    public static Guid? GetApiFingerprint(this ISymbol symbol)
    {
        var documentationId = symbol.GetDocumentationCommentId();
        if (documentationId is null)
            return null;

        const int maxBytesOnStack = 256;

        var encoding = Encoding.UTF8;
        var maxByteCount = encoding.GetMaxByteCount(documentationId.Length);

        if (maxByteCount <= maxBytesOnStack)
        {
            var buffer = (Span<byte>)stackalloc byte[maxBytesOnStack];
            var written = encoding.GetBytes(documentationId, buffer);
            var utf8Bytes = buffer[..written];
            return Create(utf8Bytes);
        }
        else
        {
            var utf8Bytes = encoding.GetBytes(documentationId);
            return Create(utf8Bytes);
        }
        
        static Guid Create(ReadOnlySpan<byte> bytes)
        {
            var hashBytes = (Span<byte>)stackalloc byte[16];
            var written = MD5.HashData(bytes, hashBytes);
            Debug.Assert(written == hashBytes.Length);

            return new Guid(hashBytes);
        }
    }
}