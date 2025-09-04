using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Domain.Utilities;

public static class DeterministicGuid
{
    /// <summary>
    /// Derives an RFC-compliant GUID from the SHA-256 hash of the input.
    /// </summary>
    public static Guid Create(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var inputBytes = Encoding.UTF8.GetBytes(input);

        // 32-byte SHA-256 → use the first 16 bytes for GUID
        Span<byte> hashBytes = stackalloc byte[32];
        SHA256.HashData(inputBytes, hashBytes);

        Span<byte> guidBytes = stackalloc byte[16];
        hashBytes[..16].CopyTo(guidBytes);

        // Set version/variant bits to be RFC 4122 compliant (v4-like)
        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | (4 << 4)); // version = 4
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // variant = RFC 4122

        return new Guid(guidBytes);
    }

    /// <summary>
    /// Creates a deterministic GUID by joining parts (e.g., pageId:draftId) and hashing the composite string.
    /// </summary>
    public static Guid Create(params object[] parts)
    {
        // Note: null parts become empty strings to keep behavior deterministic
        return Create(string.Join(":", parts.Select(p => p?.ToString() ?? string.Empty)));
    }
}
