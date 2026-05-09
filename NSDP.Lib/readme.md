# NetSend Decentralized Protocol

This spec defines the NSD Protocol

### Address format

An address looks similar to emails, however follows a stricter set of rules:

All characters of an address must be of ASCII format. The host defines rules such as the address max length

`example-user#nsdp.netsend.pw` is a valid user.
`example-user#255.211.9.92` is a valid user.

The host relay must have TXT DNS records set as such:

Host: `nsdp.netsend.pw`
Record:

```json
// NSDP
{
  "protocol": "NSDP",
  "hosts": [
    {
      "cidr": "255.211.9.92/32",
      "port": 4520,
      "mode": "S"
    },
    {
      "cname": "nsdp.netsend.pw",
      "port": 4520,
      "mode": "S+R"
    }
  ],
  "sender_keys": [
    {
      "alg": "ES384",
      "key": "base64 public key here"
    }
  ]
}
```

### Relay Protocol

A request must be made by the sender to the receiver to establish a secure connection,
TLS is required between communicators. Certificate validation is only performed for `cname` bindings.

When a user in host `nsdp.host-1.com` wants to talk to `nsdp.host-2.com`, host 1 must query the TXT record
from DNS and select an ingress containing the `mode` mentioning `R` (receive).

The sender must authenticate by signing a JWT with their key before being able to send a stream request.

To establish a unidirectional stream to the received, the requestor must call the appropriate RPC method.  
`var stream = InitializeSendStream(InitializeSendStreamRequest)`.

On the server end, this connection is stateful and contains the sender authentication and address validation logic.
As a user from host-1 sends messages to a user from host-2 the sending gRPC stream would be re-used to continuously send
messages.

If a user from host-2 specifies they have an encryption method, such as RSA or ECC, host-1 must get that data first
from host-2 via this RPC:

```
struct UserInfo {
    address: "someuser#nsdp.host-2.com",
    requiresEncryption: true,
    encryptionKeys: [
        {
            alg: "RSA4096",
            publicKey: "base64 key here"
        }
    ]
}

UserInfo userInfo = GetUserInfoRpc({
    userAddress: "someuser@nsdp.host-2.com"
});
```

If an encryption scheme is selected, the sender must encrypt all messages using one of the user keys.

```
struct NsdpMessagePayload {
    sender: "sender-user#nsdp.host-1.com",
    encryptionKeyHash: // byte array of SHA-512 hash,
    textContent: // byte array of encrypted or plain UTF-8 text,
    attachments: [
        fileName: "voice_note.mp3",
        encrypted: true,
        data: // byte array of data
    ]
}
```

