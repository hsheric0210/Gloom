# Message Protocol

|long: Length of Content (n)|GUID: OpCode|bytearray: Content|
|---:|---:|---:|
|8 bytes|16 bytes|n bytes|

Message will be encrypted/decrypted with AES-256.
Gloom is using '[Marshal.StructureToPtr](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.structuretoptr)' for fixed-length content,
[XmlSerializer](https://learn.microsoft.com/ko-kr/dotnet/api/system.xml.serialization.xmlserializer) for variable-length content.

## Content: Handshake

### Client Handshake
|string(255): Computor name|string(255): User name|long: Public key size (n)|bytearray: Public key|
|---:|---:|---:|---:|
|8 bytes|16 bytes|8 bytes|n bytes|

---

TODO: Public key size will be removed
>We can calculate the public key size by subtracting 24 from total content length

|string(255): Computor name|string(255): User name|bytearray: Public key|
|---:|---:|---:|---:|
|8 bytes|16 bytes|(content_length - 24) bytes|

### Server Handshake
|bytearray: Encrypted Secret|
|---:|
|(content_length) Encrypted Secret|

## Content: Key Logger

### Key Logger Setting

|int: Mode|long: SaveInterval|
|---:|---:|
|4 bytes|8 bytes|

## Key Logger Log Request

|int: Log Count|
|---:|
|4 bytes|

## Key Logger Log Response

|int: Log Index|long: Log date|bytearray: Compressed Log Files|
|---:|---:|---:|
|4 bytes|8 bytes|(content_length - 24) bytes|

Sent in-parallel for each log archive.

## Content: Clipboard Logger

### Clipboard Logger Setting

|int: Mode|long: SaveInterval|
|---:|---:|
|4 bytes|8 bytes|

## Clipboard Logger Log Request

|int: Log Count|
|---:|
|4 bytes|

## Clipboard Logger Log Response

|int: Log Index|long: Log date|bytearray: Compressed Log Files|
|---:|---:|---:|
|4 bytes|8 bytes|(content_length - 24) bytes|

Sent in-parallel for each log archive.
