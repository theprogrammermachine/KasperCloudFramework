using System.Collections.Generic;
using Bugsnag;
using SharedArea.Utils;

namespace SuperPeer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Setup();

            var configs = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>()
                {
                    {"bootstrap.servers", Variables.SelfPeerAddress},
                    {"username", SharedArea.GlobalVariables.KafkaUsername},
                    {"password", SharedArea.GlobalVariables.KafkaPassword},
                    {"groupid", "test-consumer-group"},
                    {"security.protocol", "SASL_PLAINTEXT"},
                    {"sasl.mechanism", "PLAIN"}
                }
            };

            if (string.IsNullOrEmpty(Variables.BugSnagToken))
            {
                KafkaExtension.SetupConsumer<Consumer, KafkaTransport>(configs);
            }
            else
            {
                KafkaExtension.SetupConsumer<Consumer, KafkaTransport>(configs,
                    new Bugsnag.Client(new Configuration(Variables.BugSnagToken)));   
            }

            Logger.Log("Info", $"Peer { Variables.SelfPeerAddress } loaded");
        }
    }
}