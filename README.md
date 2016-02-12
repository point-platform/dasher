![dasher logo](https://cdn.rawgit.com/drewnoakes/dasher/master/Resources/logo.svg)

[![Build status](https://ci.appveyor.com/api/projects/status/s8rusi6ir56tnvoj?svg=true)](https://ci.appveyor.com/project/drewnoakes/dasher)
[![Dasher NuGet version](https://img.shields.io/nuget/v/Dasher.svg)](https://www.nuget.org/packages/Dasher/)
[![Dasher download stats](https://img.shields.io/nuget/dt/Dasher.svg)](https://www.nuget.org/packages/Dasher/)

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

Both serialiser and deserialiser support the core built-in types of `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `string`, as well as `DateTime`, `TimeSpan`, `Guid`, `IntPtr`, `Version`, `Nullable<T>`, `IReadOnlyList<T>` and enum types.

Types may contain fields of further complex types, which are nested.

Custom types are supported automatically if their public, readable properties match the single constructor's argument list.

If custom treatment for specific types is required, the `ITypeProvider` interface allows bespoke code to be emitted for serialisation and deserialisation at runtime.

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

In the previous example we considered the case where the serialiser provided an extra field. Now we consider the deserialiser requires one that the serialiser has not provided.

This may be because the deserialiser added the field, or because the serialiser removed it. In practice, both situations are the same.

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
