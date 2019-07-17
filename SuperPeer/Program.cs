using System.Collections.Generic;
using Bugsnag;
using SharedArea;
using SharedArea.Utils;

namespace SuperPeer
{
    public class Program
    {
        public static void Main(Config conf)
        {
            Variables.BugSnagToken = conf.BugSnagToken;
            Variables.SelfClusterCode = conf.SelfClusterCode;
            Variables.SelfPeerCode = conf.SelfPeerCode;
            Variables.SelfPeerAddress = conf.SelfPeerAddress;
            
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