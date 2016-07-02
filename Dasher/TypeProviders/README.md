# src/Dasher/TypeProviders/

Dasher implements serialisation and deserialisation for certain types using type providers. Type providers implement a `Serialise` and a `Deserialise` method.

Type providers generate code capable of serialising and deserialising types at runtime.

Users may add their own type providers to the `DasherContext`. The following type providers are included in Dasher:

* `ComplexTypeProvider`
* `DateTimeOffsetProvider`
* `DateTimeProvider`
* `DecimalProvider`
* `EnumProvider`
* `GuidProvider`
* `IntPtrProvider`
* `MsgPackTypeProvider`
* `NullableValueProvider`
* `ReadOnlyDictionaryProvider`
* `ReadOnlyListProvider`
* `TimeSpanProvider`
* `TupleProvider`
* `UnionProvider`
* `VersionProvider`

Type providers may work together to serialise/deserialisa a given type. For example, `IReadOnlyList<decimal?>` uses three type providers: one for `IReadOnlyList<T>`, one for `Nullable<T>` and one for `System.Decimal`.