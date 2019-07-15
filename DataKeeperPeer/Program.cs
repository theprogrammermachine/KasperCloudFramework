using System.Collections.Generic;
using Bugsnag;
using SharedArea.Utils;

namespace DataKeeperPeer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Setup();

            var configs = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>()
                {
                    { "bootstrap.servers" ,  Variables.SuperPeerAddress },
                    { "username" ,  SharedArea.GlobalVariables.KafkaUsername },
                    { "password" ,  SharedArea.GlobalVariables.KafkaPassword },
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

            Logger.Log("Info", $"Peer { Variables.SuperPeerAddress } loaded");
        }
    }
}