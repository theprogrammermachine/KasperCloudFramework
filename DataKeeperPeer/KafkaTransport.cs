using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using SharedArea;
using SharedArea.Wrappers;

namespace DataKeeperPeer
{
    public class KafkaTransport : Answerable
    {
        private static readonly KafkaTransport Instance = new KafkaTransport();
        private static Dictionary<string, string> _superPeerConfig;
        private static Dictionary<string, string> _apiGatewayConfig;
        private static readonly Dictionary<string, object> PendingQuestions = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> ReceivedAnswers = new Dictionary<string, object>();
        
        public KafkaTransport()
        {
            if (_superPeerConfig == null)
            {
                _superPeerConfig = new Dictionary<string, string>()
                {
                    {"bootstrap.servers", Variables.SuperPeerAddress},
                    {"username", SharedArea.GlobalVariables.KafkaUsername},
                    {"password", SharedArea.GlobalVariables.KafkaPassword},
                    {"security.protocol", "SASL_PLAINTEXT"},
                    {"sasl.mechanism", "PLAIN"}
                };
                
                _apiGatewayConfig = new Dictionary<string, string>()
                {
                    {"bootstrap.servers", Variables.ApiGatewayAddress},
                    {"username", SharedArea.GlobalVariables.KafkaUsername},
                    {"password", SharedArea.GlobalVariables.KafkaPassword},
                    {"security.protocol", "SASL_PLAINTEXT"},
                    {"sasl.mechanism", "PLAIN"}
                };
            }

        }
        
        public void NotifyAllClusters<T>(T message)
        {
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(_superPeerConfig).Build())
                p.ProduceAsync(message.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>()
                    {
                        Message = message,
                        SrcClusterCode = Variables.SelfClusterCode,
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = "all",
                        DestPeerCode = "all"
                    }
                });
        }

        public void NotifySingleCluser<T>(string clusterCode, T message)
        {
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(_superPeerConfig).Build())
                p.ProduceAsync(message.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>()
                    {
                        Message = message,
                        SrcClusterCode = Variables.SelfClusterCode,
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = clusterCode,
                        DestPeerCode = "all"
                    }
                });
        }

        public void NotifySinglePeer<T>(string clusterCode, string peerCode, T message)
        {
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(_superPeerConfig).Build())
                p.ProduceAsync(message.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>()
                    {
                        Message = message,
                        SrcClusterCode = Variables.SelfClusterCode,
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = clusterCode,
                        DestPeerCode = peerCode
                    }
                });
        }

        public async Task<object> AskSinglePeer<T>(string clusterCode, string peerCode, T message)
        {
            var questionId = Guid.NewGuid().ToString();
            var question = new object();
            PendingQuestions.Add(questionId, question);
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(_superPeerConfig).Build())
                await p.ProduceAsync(message.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>()
                    {
                        Message = message,
                        SrcClusterCode = Variables.SelfClusterCode,
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = clusterCode,
                        DestPeerCode = peerCode,
                        QuestionId = questionId
                    }
                });
            lock (question)
            {
                Monitor.Wait(question);
            }
            var answer = ReceivedAnswers[questionId];
            ReceivedAnswers.Remove(questionId);
            return answer;
        }

        public void NotifyAnswerReceived(string questionId, object answer)
        {
            ReceivedAnswers.Add(questionId, answer);
            var question = PendingQuestions[questionId];
            lock (question)
            {
                Monitor.Pulse(question);
            }
        }

        public void SendAnswerToQuestionaire(string clusterCode, string peerCode, string questionId, object answer)
        {
            using (var p = new ProducerBuilder<Null, MessageWrapper<object>>(_superPeerConfig).Build())
                p.ProduceAsync(answer.GetType().FullName, new Message<Null, MessageWrapper<object>>()
                {
                    Value = new MessageWrapper<object>()
                    {
                        Message = answer, 
                        SrcClusterCode = Variables.SelfClusterCode, 
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = clusterCode,
                        DestPeerCode = peerCode
                    }
                });
        }

        public void PushNotifToApiGateway<T>(T notif)
        {
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(_apiGatewayConfig).Build())
                p.ProduceAsync(notif.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>()
                    {
                        Message = notif,
                        SrcClusterCode = Variables.SelfClusterCode,
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = Variables.SelfClusterCode,
                        DestPeerCode = Variables.SelfPeerCode
                    }
                });
        }
    }
}