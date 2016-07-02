# src/Dasher/

This folder contains the core Dasher source code.

Primary public types are:

* `Serialiser` and `Serialiser<T>`
* `Deserialiser` and `Deserialiser<T>`
* `DasherContext`
* `SerialisationException`
* `DeserialisationException`
* `UnexpectedFieldBehaviour`
* `Union<...>`

In addition, some internal types are used within the library:

* `SerialiserEmitter`
* `DeserialiserEmitter`
* `ILGeneratorExtensions`

See also the `TypeProviders` sub-namespace.