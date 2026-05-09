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
{
  "protocol": "NSDP",
  "hosts": [
    "netsend.pw:4520",
    "255.211.9.92:4520",
    "255.211.9.0/24:4250"
  ],
  "sender_pk": {
    "alg": "ES384",
    "key": "base64 public key here"
  }
}
```