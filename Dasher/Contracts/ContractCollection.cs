#region License
//
// Dasher
//
// Copyright 2015-2016 Drew Noakes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// More information about this project is available at:
//
//    https://github.com/drewnoakes/dasher
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Dasher.Contracts.Types;
using Dasher.Utils;

namespace Dasher.Contracts
{
    public sealed class ContractCollection
    {
        private readonly List<Contract> _contracts = new List<Contract>();

        internal IReadOnlyCollection<Contract> Contracts => _contracts;

        #region Contract resolution

        private bool AllowResolution { get; set; } = true;

        public IReadContract ResolveReadContract(string str)
        {
            IReadContract contract;
            if (!TryResolveReadContract(str, out contract))
                throw new Exception($"String \"{str}\" cannot be resolved as a read contract within this collection.");
            return contract;
        }

        public bool TryResolveReadContract(string str, out IReadContract readContract)
        {
            if (!AllowResolution)
                throw new InvalidOperationException("Cannot resolve contract at this stage. Use a bind action instead.");

            if (str.StartsWith("#"))
            {
                var id = str.Substring(1);
                readContract = Contracts.OfType<ByRefContract>().SingleOrDefault(s => s.Id == id) as IReadContract;
                return readContract != null;
            }

            if (str.StartsWith("{"))
            {
                var tokens = ContractMarkupExtension.Tokenize(str).ToList();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (tokens.Count)
                {
                    case 1:
                        if (tokens[0] == "empty")
                        {
                            readContract = Intern(new EmptyContract());
                            return true;
                        }
                        break;
                    case 2:
                        if (tokens[0] == "nullable")
                        {
                            IReadContract inner;
                            if (TryResolveReadContract(tokens[1], out inner))
                            {
                                readContract = Intern(new NullableReadContract(inner));
                                return true;
                            }
                        }
                        else if (tokens[0] == "list")
                        {
                            IReadContract itemContract;
                            if (TryResolveReadContract(tokens[1], out itemContract))
                            {
                                readContract = Intern(new ListReadContract(itemContract));
                                return true;
                            }
                        }
                        break;
                    case 3:
                        if (tokens[0] == "dictionary")
                        {
                            IReadContract keyContract;
                            IReadContract valueContract;
                            if (TryResolveReadContract(tokens[1], out keyContract) && TryResolveReadContract(tokens[2], out valueContract))
                            {
                                readContract = Intern(new DictionaryReadContract(keyContract, valueContract));
                                return true;
                            }
                        }
                        break;
                }

                if (tokens.Count != 0 && tokens[0] == "tuple")
                {
                    var itemContracts = new List<IReadContract>();
                    foreach (var token in tokens.Skip(1))
                    {
                        IReadContract itemContract;
                        if (!TryResolveReadContract(token, out itemContract))
                        {
                            readContract = null;
                            return false;
                        }
                        itemContracts.Add(itemContract);
                    }
                    readContract = Intern(new TupleReadContract(itemContracts));
                    return true;
                }

                readContract = null;
                return false;
            }

            readContract = Intern(new PrimitiveContract(str));
            return true;
        }

        public IWriteContract ResolveWriteContract(string str)
        {
            IWriteContract contract;
            if (!TryResolveWriteContract(str, out contract))
                throw new Exception($"String \"{str}\" cannot be resolved as a write contract within this collection.");
            return contract;
        }

        public bool TryResolveWriteContract(string str, out IWriteContract writeContract)
        {
            if (!AllowResolution)
                throw new InvalidOperationException("Cannot resolve contract at this stage. Use a bind action instead.");

            if (str.StartsWith("#"))
            {
                var id = str.Substring(1);
                writeContract = Contracts.OfType<ByRefContract>().SingleOrDefault(s => s.Id == id) as IWriteContract;
                return writeContract != null;
            }

            if (str.StartsWith("{"))
            {
                var tokens = ContractMarkupExtension.Tokenize(str).ToList();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (tokens.Count)
                {
                    case 1:
                        if (tokens[0] == "empty")
                        {
                            writeContract = Intern(new EmptyContract());
                            return true;
                        }
                        break;
                    case 2:
                        if (tokens[0] == "nullable")
                        {
                            IWriteContract inner;
                            if (TryResolveWriteContract(tokens[1], out inner))
                            {
                                writeContract = Intern(new NullableWriteContract(inner));
                                return true;
                            }
                        }
                        else if (tokens[0] == "list")
                        {
                            IWriteContract itemContract;
                            if (TryResolveWriteContract(tokens[1], out itemContract))
                            {
                                writeContract = Intern(new ListWriteContract(itemContract));
                                return true;
                            }
                        }
                        break;
                    case 3:
                        if (tokens[0] == "dictionary")
                        {
                            IWriteContract keyContract;
                            IWriteContract valueContract;
                            if (TryResolveWriteContract(tokens[1], out keyContract) && TryResolveWriteContract(tokens[2], out valueContract))
                            {
                                writeContract = Intern(new DictionaryWriteContract(keyContract, valueContract));
                                return true;
                            }
                        }
                        break;
                }

                if (tokens.Count != 0 && tokens[0] == "tuple")
                {
                    var itemContracts = new List<IWriteContract>();
                    foreach (var token in tokens.Skip(1))
                    {
                        IWriteContract itemContract;
                        if (!TryResolveWriteContract(token, out itemContract))
                        {
                            writeContract = null;
                            return false;
                        }
                        itemContracts.Add(itemContract);
                    }
                    writeContract = Intern(new TupleWriteContract(itemContracts));
                    return true;
                }

                writeContract = null;
                return false;
            }

            writeContract = Intern(new PrimitiveContract(str));
            return true;
        }

        #endregion

        /// <summary>
        /// Removes any unreachable contract given specified <paramref name="roots"/>.
        /// </summary>
        /// <param name="roots"></param>
        /// <returns>The number of contract removed, or zero if nothing was removed.</returns>
        public int GarbageCollect(IEnumerable<Contract> roots)
        {
            var explored = new HashSet<Contract>();
            var frontier = new Queue<Contract>(roots);

            while (frontier.Count != 0)
            {
                var item = frontier.Dequeue();
                if (explored.Contains(item))
                    continue;
                explored.Add(item);
                foreach (var child in item.Children)
                    frontier.Enqueue(child);
            }

            return _contracts.RemoveAll(s => !explored.Contains(s, ReferenceEqualityComparer.Default));
        }

        public void UpdateByRefIds()
        {
            var existingIds = new HashSet<string>(Contracts.OfType<ByRefContract>().Where(s => s.Id != null).Select(s => s.Id));

            // TODO revisit how IDs are assigned
            var i = 1;
            foreach (var contract in Contracts.OfType<ByRefContract>().Where(s => s.Id == null))
            {
                string id = $"Contract{i++}";
                while (existingIds.Contains(id))
                    id = $"Contract{i++}";
                contract.Id = id;
            }
        }

        #region To/From XML

        public XElement ToXml(string elementName = "Contracts")
        {
            return new XElement(elementName,
                Contracts.OfType<ByRefContract>().OrderBy(s => s.Id, NumericStringComparer.Default).Select(s => s.ToXml()));
        }

        public static ContractCollection FromXml(XElement element)
        {
            var bindActions = new List<Action>();

            var collection = new ContractCollection {AllowResolution = false};

            foreach (var el in element.Elements())
            {
                var id = el.Attribute("Id")?.Value;

                if (string.IsNullOrWhiteSpace(id))
                    throw new ContractParseException("Contract XML element must contain a non-empty \"Id\" attribute.");

                ByRefContract contract;
                switch (el.Name.LocalName)
                {
                    case "ComplexRead":
                        contract = new ComplexReadContract(el, collection.ResolveReadContract, bindActions);
                        break;
                    case "ComplexWrite":
                        contract = new ComplexWriteContract(el, collection.ResolveWriteContract, bindActions);
                        break;
                    case "UnionRead":
                        contract = new UnionReadContract(el, collection.ResolveReadContract, bindActions);
                        break;
                    case "UnionWrite":
                        contract = new UnionWriteContract(el, collection.ResolveWriteContract, bindActions);
                        break;
                    case "Enum":
                        contract = new EnumContract(el);
                        break;
                    default:
                        throw new ContractParseException($"Unsupported contract XML element with name \"{el.Name.LocalName}\".");
                }

                contract.Id = id;

                collection._contracts.Add(contract);
            }

            collection.AllowResolution = true;

            foreach (var bindAction in bindActions)
                bindAction();

            return collection;
        }

        #endregion

        public IWriteContract GetOrAddWriteContract(Type type)
        {
            if (type == typeof(Empty))
                return Intern(new EmptyContract());
            if (PrimitiveContract.CanProcess(type))
                return Intern(new PrimitiveContract(type));
            if (EnumContract.CanProcess(type))
                return Intern(new EnumContract(type));
            if (TupleWriteContract.CanProcess(type))
                return Intern(new TupleWriteContract(type, this));
            if (NullableWriteContract.CanProcess(type))
                return Intern(new NullableWriteContract(type, this));
            if (ListWriteContract.CanProcess(type))
                return Intern(new ListWriteContract(type, this));
            if (DictionaryWriteContract.CanProcess(type))
                return Intern(new DictionaryWriteContract(type, this));
            if (UnionWriteContract.CanProcess(type))
                return Intern(new UnionWriteContract(type, this));

            return Intern(new ComplexWriteContract(type, this));
        }

        public IReadContract GetOrAddReadContract(Type type)
        {
            if (type == typeof(Empty))
                return Intern(new EmptyContract());
            if (PrimitiveContract.CanProcess(type))
                return Intern(new PrimitiveContract(type));
            if (EnumContract.CanProcess(type))
                return Intern(new EnumContract(type));
            if (TupleReadContract.CanProcess(type))
                return Intern(new TupleReadContract(type, this));
            if (NullableReadContract.CanProcess(type))
                return Intern(new NullableReadContract(type, this));
            if (ListReadContract.CanProcess(type))
                return Intern(new ListReadContract(type, this));
            if (DictionaryReadContract.CanProcess(type))
                return Intern(new DictionaryReadContract(type, this));
            if (UnionReadContract.CanProcess(type))
                return Intern(new UnionReadContract(type, this));

            return Intern(new ComplexReadContract(type, this));
        }

        internal T Intern<T>(T contract) where T : Contract
        {
            Debug.Assert(contract is ByRefContract || contract is ByValueContract, "contract is ByRefContract || contract is ByValueContract");

            foreach (var existing in _contracts)
            {
                if (existing.Equals(contract))
                    return (T)existing;
            }

            _contracts.Add(contract);
            return contract;
        }

        internal T GetOrCreate<T>(T contract, Func<T> func) where T : Contract
        {
            Debug.Assert(contract is ByRefContract || contract is ByValueContract, "contract is ByRefContract || contract is ByValueContract");

            foreach (var existing in _contracts)
            {
                if (existing.Equals(contract))
                    return (T)existing;
            }

            var newContract = func();
            _contracts.Add(newContract);
            return newContract;
        }
    }
}
