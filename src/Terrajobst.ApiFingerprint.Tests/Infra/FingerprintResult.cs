using System.Security.Cryptography;
using System.Text;

namespace Terrajobst.ApiFingerprint.Tests.Infra;

public readonly struct FingerprintResult : IEquatable<FingerprintResult>, IComparable<FingerprintResult>
{
    public FingerprintResult(string documentationId)
    {
        ThrowIfNull(documentationId);

        Value = GetValue(documentationId);
        DocumentationId = documentationId;
    }

    public FingerprintResult(string documentationId, Guid value)
    {
        ThrowIfNull(documentationId);

        Value = value;
        DocumentationId = documentationId;
    }

    public Guid Value { get; }

    public string DocumentationId { get; }

    private static Guid GetValue(string documentationId)
    {
        var utf8Bytes = Encoding.UTF8.GetBytes(documentationId);
        var hashBytes = MD5.HashData(utf8Bytes);
        return new Guid(hashBytes);
    }

    public override string ToString()
    {
        return $"{DocumentationId}: {Value}";
    }

    public bool Equals(FingerprintResult other)
    {
        return Value.Equals(other.Value) &&
               string.Equals(DocumentationId, other.DocumentationId, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is FingerprintResult other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, DocumentationId);
    }

    public static bool operator ==(FingerprintResult left, FingerprintResult right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FingerprintResult left, FingerprintResult right)
    {
        return !left.Equals(right);
    }

    public int CompareTo(FingerprintResult other)
    {
        return string.Compare(DocumentationId, other.DocumentationId, StringComparison.Ordinal);
    }
}