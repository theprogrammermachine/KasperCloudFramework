using System.Collections.Generic;

namespace SharedArea
{
    public class GlobalVariables
    {
        public static readonly Dictionary<string, string> SuperPeerAddresses = new Dictionary<string, string>()
        {
            { "#guilan", "localhost:9093" }
        };
        public static string KafkaUsername = "admin", KafkaPassword = "admin-secret";
    }
}