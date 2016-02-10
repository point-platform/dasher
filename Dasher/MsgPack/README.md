# src/Dasher/MsgPack/

Code specific the MessagePack format is grouped together in this folder. Note that all types in this folder are in the `Dasher` namespace (not `Dasher.MsgPack`).

Primary public types are:

* `Packer` and `UnsafePacker` used to write values to a stream
* `Unpacker` used to read values from a stream
* `Format` and `FormatFamily` enums denoting the types of values according to the MsgPack standard
* `MsgPackConstants` constants used internally within the library to encode and decode MsgPack data
