using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Cci.Extensions;

public static class ApiFingerprint
{
    public static Guid? GetApiFingerprint(this IReference reference)
    {
        var docId = reference.RefDocId();
        if (string.IsNullOrEmpty(docId))
            return null;

        const int maxBytesOnStack = 256;

        var encoding = Encoding.UTF8;
        var maxByteCount = encoding.GetMaxByteCount(docId.Length);

        if (maxByteCount <= maxBytesOnStack)
        {
            var buffer = (Span<byte>)stackalloc byte[maxBytesOnStack];
            var written = encoding.GetBytes(docId, buffer);
            var utf8Bytes = buffer[..written];
            return Create(utf8Bytes);
        }
        else
        {
            var utf8Bytes = encoding.GetBytes(docId);
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