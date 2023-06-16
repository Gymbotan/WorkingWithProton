using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorkingWithProton.Entities
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Class is instantiated via JsonSerializer")]
    public class AddressesResponse
    {
        [JsonInclude]
        public IList<Address>? Addresses { get; private set; }

        public class Address
        {
            [JsonInclude]
            [JsonPropertyName("ID")]
            public string? Id { get; private set; }

            [JsonInclude]
            public string? Email { get; private set; }

            //[JsonInclude]
            //public bool Send { get; private set; }

            //[JsonInclude]
            //public bool Receive { get; private set; }

            [JsonInclude]
            public int Status { get; private set; } // AddressStatus

            [JsonInclude]
            public int Order { get; private set; }

            [JsonInclude]
            public string? DisplayName { get; private set; }

            [JsonInclude]
            public IList<Key>? Keys { get; private set; }


            // Json has additional fields

            //[JsonInclude]
            //[JsonPropertyName("DomainID")]
            //public string? DomainId { get; private set; }

            //[JsonInclude]
            //public int Type { get; private set; }

            [JsonInclude]
            public string? Signature { get; private set; }

            //[JsonInclude]
            //public int Priority { get; private set; }

            //[JsonInclude]
            //public int ConfirmationState { get; private set; }

            //[JsonInclude]
            //public int HasKeys { get; private set; }

            [JsonInclude]
            public SignedKey? SignedKeyList { get; private set; }
        }

        public struct SignedKey
        {
            //[JsonInclude]
            //public int MinEpochID { get; private set; }

            //[JsonInclude]
            //public int MaxEpochID { get; private set; }

            [JsonInclude]
            public int? ExpectedMinEpochID { get; private set; }

            [JsonInclude]
            public string? Data { get; private set; }

            [JsonInclude]
            public string? Signature { get; private set; }
        }
    }
}
