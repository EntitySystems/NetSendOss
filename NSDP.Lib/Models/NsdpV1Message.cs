using NSDP.Lib.Static;

namespace NSDP.Lib.Models;

public class NsdpV1Message
{
    public required string MessageVersion { get; init; } = ProtocolKeys.NsdpV1Message;

    public required string Sender { get; init; }
    public required string Receiver { get; set; }

    /// <summary>
    /// List of other receivers of a specific message, useful in group chats
    /// </summary>
    public required IReadOnlyList<string> OtherReceivers { get; init; }

    public required byte[]? EncryptionKeyHash { get; init; }
    public required NsdpV1MessageSymmetricKeyInfo? SymmetricKeyInfo { get; init; }

    /// <summary>
    /// Plain UTF8 or encrypted UTF8
    /// </summary>
    public required byte[]? TextContent { get; set; }

    public required IReadOnlyList<NsdpV1MessageAttachment> Attachments { get; init; }
}

public class NsdpV1MessageAttachment
{
    public required string? FileName { get; init; }
    public required string ContentType { get; init; }
    public required bool Encrypted { get; init; }
    public required Stream Data { get; init; }
}

public class NsdpV1MessageSymmetricKeyInfo
{
    public required byte[] EncryptedKey { get; init; }
    public required string Alg { get; init; }
}