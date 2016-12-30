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
using System.Reflection.Emit;

namespace Dasher
{
    /// <summary>
    /// Code for throwing exceptions should not be emitted inline amongst straight-path code.
    /// This class gathers these blocks so they may be flushed after any non-exceptional code.
    /// Doing so reduces pressure on the CPU's i-cache.
    /// </summary>
    /// <remarks>
    /// Benchmarks before and after the introduction of this behaviour showed a 14.3% reduction in deserialisation time.
    /// </remarks>
    public sealed class ThrowBlockGatherer
    {
        private readonly List<Action> _blocks = new List<Action>();
        private readonly ILGenerator _ilg;

        internal ThrowBlockGatherer(ILGenerator ilg)
        {
            _ilg = ilg;
        }

        public void ThrowIfTrue    (Action throwBlock) => BranchToThrowBlock(OpCodes.Brtrue,  throwBlock);
        public void ThrowIfFalse   (Action throwBlock) => BranchToThrowBlock(OpCodes.Brfalse, throwBlock);
        public void ThrowIfEqual   (Action throwBlock) => BranchToThrowBlock(OpCodes.Beq,     throwBlock);
        public void ThrowIfNotEqual(Action throwBlock) => BranchToThrowBlock(OpCodes.Bne_Un,  throwBlock);
        public void Throw          (Action throwBlock) => BranchToThrowBlock(OpCodes.Br,      throwBlock);

        public void BranchToThrowBlock(OpCode branchOpCode, Action throwBlock)
        {
            if (branchOpCode.FlowControl != FlowControl.Branch && branchOpCode.FlowControl != FlowControl.Cond_Branch)
                throw new ArgumentException("Must be a branch opcode.", nameof(branchOpCode));

            var label = _ilg.DefineLabel();

            _ilg.Emit(branchOpCode, label);

            _blocks.Add(() =>
            {
                _ilg.MarkLabel(label);
                throwBlock();
            });
        }

        internal void Flush() => _blocks.ForEach(b => b());
    }
}