using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace WorkingWithProton.Entities
{
    //public struct TotalMessagesContent
    //{
    //    public int Code { get; set; }
    //    public string Error { get; set; }
    //    public JsonObject Details { get; set; }

    //    public IList<Folder> Counts { get; set; }

    //    public record Folder
    //    {
    //        public string? LabelID { get; set; }
    //        public long Total { get; set; }
    //        public long Unread { get; set; }
    //    }
    //}

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Class is instantiated via JsonSerializer")]
    internal class UsersResponse
    {
        [JsonInclude]
        public UserData? User { get; private set; }

        public struct UserData
        {
            [JsonInclude]
            [JsonPropertyName("ID")]
            public string? Id { get; private set; }

            [JsonInclude]
            public string? Name { get; private set; }

            [JsonInclude]
            public string? DisplayName { get; private set; }

            [JsonInclude]
            public string? Email { get; private set; }

            [JsonInclude]
            public IList<Key>? Keys { get; private set; }


            [JsonInclude]
            public int UsedSpace { get; private set; }

            [JsonInclude]
            public int MaxSpace { get; private set; }

            [JsonInclude]
            public int MaxUpload { get; private set; }


            [JsonInclude]
            public int Credit { get; private set; }

            [JsonInclude]
            public string? Currency { get; private set; }


            // Json has additional fields


            //[JsonInclude]
            //public int Type { get; private set; }

            //[JsonInclude]
            //public int CreateTime { get; private set; }

            //[JsonInclude]
            //public int Subscribed { get; private set; }

            //[JsonInclude]
            //public int Services { get; private set; }

            //[JsonInclude]
            //public int MnemonicStatus { get; private set; }

            //[JsonInclude]
            //public int Role { get; private set; }

            //[JsonInclude]
            //public int Private { get; private set; }

            //[JsonInclude]
            //public int Delinquent { get; private set; }

            //[JsonInclude]
            //public int ToMigrate { get; private set; }
        }
    }
}
