# src/Dasher/TypeProviders/

Dasher implements serialisation and deserialisation for certain types using type providers. Type providers implement a `Serialise` and a `Deserialise` method.

Type providers generate code capable of serialising and deserialising types at runtime.

Users may add their own type providers to the `DasherContext`. The following type providers are included in Dasher:

* `ComplexTypeProvider`
* `DateTimeProvider`
* `DecimalProvider`
* `EnumProvider`
* `GuidProvider`
* `IntPtrProvider`
* `NullableValueProvider`
* `TimeSpanProvider`
* `ReadOnlyListProvider`
* `VersionProvider`

Type providers may work together to serialise/deserialisa a given type. For example, `IReadOnlyList<decimal?>` uses three type providers: one for `IReadOnlyList<T>`, one for `Nullable<T>` and one for `System.Decimal`.