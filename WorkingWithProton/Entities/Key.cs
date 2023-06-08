using System.Text.Json.Serialization;

namespace WorkingWithProton.Entities
{
    public struct Key
    {
        [JsonInclude]
        [JsonPropertyName("ID")]
        public string? Id { get; private set; }

        [JsonInclude]
        public string? PrivateKey { get; private set; }

        [JsonInclude]
        public string? Token { get; private set; }

        [JsonInclude]
        public string? Signature { get; private set; }

        [JsonInclude]
        public int Primary { get; private set; }

        public bool IsPrimary => Primary != 0;

        [JsonInclude]
        public int Active { get; private set; }

        public bool IsActive => Active != 0;

        [JsonInclude]
        public int Flags { get; private set; } // KeyState


        // Json has additional fields

        [JsonInclude]
        public string? Fingerprint { get; private set; }

        [JsonInclude]
        public IList<string>? Fingerprints { get; private set; }

        [JsonInclude]
        public string? PublicKey { get; private set; }

        [JsonInclude]
        public int Version { get; private set; }

        [JsonInclude]
        public object? Activation { get; private set; } // null
    }
}
