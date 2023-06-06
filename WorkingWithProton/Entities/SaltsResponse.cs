using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorkingWithProton.Entities
{
    internal class SaltsResponse
    {
        [JsonInclude]
        public IList<Salt>? KeySalts { get; private set; }
    }

    public struct Salt
    {
        [JsonInclude]
        [JsonPropertyName("ID")]
        public string? Id { get; private set; }

        [JsonInclude]
        public string? KeySalt { get; private set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
            });
        }
    }
}
