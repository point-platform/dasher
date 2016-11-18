![dasher logo](https://cdn.rawgit.com/drewnoakes/dasher/master/Resources/logo.svg)

[![Build status](https://ci.appveyor.com/api/projects/status/s8rusi6ir56tnvoj?svg=true)](https://ci.appveyor.com/project/drewnoakes/dasher)
[![Dasher NuGet version](https://img.shields.io/nuget/v/Dasher.svg)](https://www.nuget.org/packages/Dasher/)

Dasher is a fast, lightweight, cross-platform serialisation tool.

Use it to get data objects from one application to another, and to reliably control behaviour as schema evolve independently.

---

In inter-application communication, message schema evolve and problematic incompatibilities can arise.
In the worst case, these failures are silent and untracked.
Dasher gives you control over what happens as versions change, in a way that's very natural to C# developers.
You can be very strict about what you receive, or lenient.

# API

The core API is very simple:

```csharp
// serialise to stream
new Serialiser<Holiday>().Serialise(stream, christmas);

// deserialise from stream
var christmas = new Deserialiser<Holiday>().Deserialise(stream);
```

Dasher messages are regular CLR types (POCO), with no need for a base class or attributes. They can (and should) be immutable as well:

```csharp
public sealed class Holiday
{
	public string Name { get; }
	public DateTime Date { get; }

	public Holiday(string name, DateTime date)
	{
		Name = name;
		Date = date;
	}
}
```

# Supported types

Both serialiser and deserialiser support the core built-in types of `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`,
`ulong`, `float`, `double`, `decimal`, `string`, as well as `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, `IntPtr`,
`Version`, `Nullable<T>`, `IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`, `Tuple<...>` and enum types.

Dasher also provides and supports types:

- `Dasher.Union<...>` allows a value to be one of a fixed number of known types
- `Dasher.Empty` represents a value with no fields or state that may gracefully support addition of fields in future versions

More details on `Union` and `Empty` types can be found later in this document.

Types not listed so far are treated as _complex types_. Complex types have their properties serialised, and are deserialised
according to their constructor parameters.

If custom treatment for specific types is required, the `ITypeProvider` interface allows bespoke code to be emitted for
serialisation and deserialisation at runtime.

---

# Schema Evolution

Over time, the serialiser and/or deserialiser may need to modify the schema of the messages they exchange.

Dasher gives the receiver the final say in whether it will accept a given message or not.
Therefore, Dasher's `Deserialiser` class governs, through user configuration, how different messages are interpreted.

Let's look at some examples.

## Handing unexpected fields

The most common modifications to schema are the addition and removal of fields.
In this example, the serialised message contains a field that the deserialiser does not expect.

This may be because the serialiser added the field, or because the deserialiser removed it. In practice, both situations are the same.

Imagine a deserialiser for this class:

```csharp
public sealed class Reindeer
{
    public string Name { get; }
    public int Position { get; }

    public Reindeer(string name, int position)
    {
    	Name = name;
    	Position = position;
    }
}
```

Consider the payload to deserialise as:

```json
{ "Name": "Dasher", "Position": 1, "HasRedNose": false }
```

Clearly `HasRedNose` is an unexpected field. There are two possible behaviours: ignore the field, or throw an exception. Which occurs depends upon how the `Deserialiser` is instantiated.

```csharp
// return new Reindeer("Dasher", 1)
new Deserialiser<Reindeer>(UnexpectedFieldBehaviour.Ignore).Deserialise(...);

// throw new DeserialisationException("Unexpected field \"HasRedNose\".", typeof(Reindeer))
new Deserialiser<Reindeer>(UnexpectedFieldBehaviour.Throw).Deserialise(...);
```

When no `UnexpectedFieldBehaviour` is specified in the deserialiser's constructor, the default behaviour is to throw:

```csharp
// throws as above
new Deserialiser<Reindeer>().Deserialise(...);
```

## Handling missing fields

In the previous example we considered the case where the serialiser provided a field that was not expected by the deserialiser. Now we consider the opposite case, where the deserialiser requires a field that the serialiser has not provided.

This may be because the deserialiser added the field, or because the serialiser removed it. In practice, both situations can be treated in the same way.

### Missing a required field

```csharp
public sealed class Reindeer
{
    public string Name { get; }
    public int Position { get; }

    public Reindeer(string name, int position)
    {
    	Name = name;
    	Position = position;
    }
}
```

Consider the payload to deserialise as:

```json
{ "Name": "Dancer" }
```

The `Position` field is not specified, and the programmer who declared the `Reindeer` type requires such a field to be passed to the constructor.

In this case, attempts to deserialise the payload result in an exception:

```csharp
// throw new DeserialisationException("Missing required field \"Position\".", typeof(Reindeer))
new Deserialiser<Reindeer>().Deserialise(...);
```

### Missing an optional field

Often it's desirable to make a field optional by providing a default value to use when no value is specified. This enables backwards-compatible modifications to schema, allowing serialisers to be updated independently of deserialisers, at some later time.

```csharp
public sealed class Reindeer
{
    public string Name { get; }
    public int Position { get; }
    public bool HasRedNose { get; }

    public Reindeer(string name, int position, bool hasRedNose = false)
    {
    	Name = name;
    	Position = position;
    	HasRedNose = hasRedNose;
    }
}
```
Consider the payload to deserialise as:

```json
{ "Name": "Prancer", "Position": 3 }
```

No value is provided for `HasRedNose` in the message, however the constructor argument includes a default value of `false`. In such a case, Dasher uses the default value of a field when the field is omitted from the payload.

```csharp
// return new Reindeer("Prancer", 3, false)
new Deserialiser<Reindeer>(UnexpectedFieldBehaviour.Ignore).Deserialise(...);
```

---

# Union Types

Dasher is strict about the types it deals with. This allows great control over message schema versions, but sometime you don't know exactly which type you will need. Union types allow flexibility, but in a controlled fashion.

Dasher provides a `Union<T1,T2,...>` type. This allows a given field's type to be one of a set of known types.

Let's look at some examples. Firstly, construction of a union instance:

```csharp
// implicit conversion
Union<int, string> union1 = 1;
Union<int, string> union2 = "Hello";

// alternatively, explicit construction
var union1 = Union<int, string>.Create(1);
var union2 = Union<int, string>.Create("Hello");
```

There are a few ways to consume unions:

```csharp
// consuming a union
union1.Type   // int
union1.Value  // 1 (boxed as object)

// perform an action based on type
union1.Match(
  i => Console.WriteLine($"Union holds an int: {i}"),
  s => Console.WriteLine($"Union holds a string: {s}"));

// mapping based on type
var num = union1.Match(
  i => i * 2,
  s => s.Length);
```

The `Match` methods offer great performance as they won't box value types and don't involve any lookup based on type (beyond a single vtable lookup).

A class could have a property of type `Union<int, string>` and be successfully serialised and deserialised by Dasher. That property can contain either of these types.

This allows a controlled form of polymorphism (with base type requirement), and allows for heterogeneous lists/dictionaries. For example:

```csharp
IReadOnlyList<Union<AddItemRequest, RemoveItemRequest>> Requests { get; }
```

Unions are fully composable, so the above to could be inverted if you knew all items in the list would have the same type:

```csharp
Union<IReadOnlyList<AddItemRequest>, IReadOnlyList<RemoveItemRequest>> Requests { get; }
```

---

# Empty Type

`Empty` is useful in contracts when a value is not currently required, but may be one day in future.

Consider a message that indicates some kind of notification. The message itself might not contain any fields,
as it is enough to simply observe the empty message. However one day, you may wish to add one or more optional
fields and support backwards compatibility. In such a case, use `Empty` today and introduce a complex or union
type at some later point.

It is expected that `Empty` would be most useful as a top-level type (i.e. `Serialiser<Empty>`) or used in
conjunction with generic wrapper types (e.g. `Serialiser<MyEnvelope<Empty>>>`).

The `Dasher.Empty` CLR type itself cannot be instantiated or subclassed. At runtime, the value will be `null`.

Empty values may be deserialised to the following types:

- A complex type for which all constructor parameters have default values, where an instance is instantiated with those defaults
- A union type, where the resulting value is `null`
- A nullable nullable complex struct, where the resulting value is `null`

If a `Deserialiser` is constructed with option `UnexpectedFieldBehaviour.Ignore`, then non-empty values received for an `Empty` type
are discarded. However if `UnexpectedFieldBehaviour.Throw` is used (the default setting) then a `DeserialisationException` is raised.

---

# Encoding

You don't _need_ to know how Dasher encodes messages in order to use it successfully, but a deeper understanding is never a bad thing.
Thankfully it's not too complex.

On the wire, messages are encoded using [MsgPack](http://msgpack.org), which is an efficient binary serialisation format.
If you're familiar with JSON, you can think of it in much the same way.
MsgPack messages are quick to pack and unpack, and the resulting byte stream is very concise (much more so than JSON or XML for example).

Using our example from above, this object:

```csharp
new Holiday("Christmas", new DateTime(2015, 12, 25));
```

Would logically be encoded as (using JSON for readability):

```json
{ "Name": "Christmas", "Date": 635865984000000000 }
```

And physically encoded as (the actual byte stream):

```plain
82 A4 4E 61 6D 65 A9 43 68 72 69 73 74 6D 61 73 A4 44 61 74 65 D3 08 D3 0C BE 55 13 00 00
|  |___________/  |__________________________/  |___________/  |_______________________/
|  |              |                             |              |
|  \ "Name"       \ "Christmas"                 \ "Date"       \ 64-bit integer
|
\ map of two key/value pairs
```

MsgPack specifies the encoding of types `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `bool`, `string`, and `byte[]`.

Dasher encodes the following types using the following MsgPack formats:

* Array (other than `byte[]`) as array
* `DateTime` as 64-bit integer
* `DateTimeOffset` as two-element array of 64-bit integer (ticks) and 16-bit integer (offset)
* `Decimal` as string
* `Empty` as zero-sized map
* `Enum` as string
* `Guid` as byte array
* `IntPtr` as 64-bit integer
* `IReadOnlyDictionary<T, K>` as map from `T` to `K` (recur on this list)
* `IReadOnlyList<T>` as array of `T` (recor on this list)
* `Nullable<T>` as either null or `T` (recur on this list)
* `TimeSpan` as 64-bit integer
* `Tuple<>` as array of values (recur on this list)
* `Union<>` as two-element array: type identifier as string, value (recur on this list)
* `Version` as string
* Finally, if none of the above apply, `class` and `struct` as heterogeneous MsgPack map from name to value

---

# License

Copyright 2015-2016 Drew Noakes

> Licensed under the Apache License, Version 2.0 (the "License");
> you may not use this file except in compliance with the License.
> You may obtain a copy of the License at
>
>     http://www.apache.org/licenses/LICENSE-2.0
>
> Unless required by applicable law or agreed to in writing, software
> distributed under the License is distributed on an "AS IS" BASIS,
> WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
> See the License for the specific language governing permissions and
> limitations under the License.
