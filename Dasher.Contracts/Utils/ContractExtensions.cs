using System;

namespace Dasher.Contracts.Utils
{
    public static class ContractExtensions
    {
        public static string ToReferenceString(this IWriteContract contract) => ToReferenceStringInternal(contract);

        public static string ToReferenceString(this IReadContract contract) => ToReferenceStringInternal(contract);

        private static string ToReferenceStringInternal(object contract)
        {
            var byRefContract = contract as ByRefContract;

            if (byRefContract != null)
            {
                if (string.IsNullOrWhiteSpace(byRefContract.Id))
                    throw new Exception("ByRefContract must have an ID to produce a reference string.");
                return '#' + byRefContract.Id;
            }

            return ((ByValueContract)contract).MarkupValue;
        }
    }
}
