using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using SharedArea;
using SharedArea.Wrappers;

namespace ApiGateway
{
    public class KafkaTransport : Answerable
    {
        private static readonly KafkaTransport Instance = new KafkaTransport();
        private static Dictionary<string, string> _config;
        private static readonly Dictionary<string, object> PendingQuestions = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> ReceivedAnswers = new Dictionary<string, object>();
        
        public KafkaTransport()
        {
            if (_config == null)
            {
                _config = new Dictionary<string, string>()
                {
                    {"bootstrap.servers", Variables.PairedPeerAddress},
                    {"username", SharedArea.GlobalVariables.KafkaUsername},
                    {"password", SharedArea.GlobalVariables.KafkaPassword},
                    {"security.protocol", "SASL_PLAINTEXT"},
                    {"sasl.mechanism", "PLAIN"}
                };
            }
        }
        
        public void NotifyPairedPeer<T>(T message)
        {
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(_config).Build())
                p.ProduceAsync(message.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>()
                    {
                        Message = message,
                        SrcClusterCode = Variables.SelfClusterCode,
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = Variables.SelfClusterCode,
                        DestPeerCode = Variables.SelfPeerCode
                    }
                });
        }

        public async Task<object> AskPairedPeer<T>(T message)
        {
            var questionId = Guid.NewGuid().ToString();
            var question = new object();
            PendingQuestions.Add(questionId, question);
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(_config).Build())
                await p.ProduceAsync(message.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>()
                    {
                        Message = message,
                        SrcClusterCode = Variables.SelfClusterCode,
                        SrcPeerCode = Variables.SelfPeerCode,
                        DestClusterCode = Variables.SelfClusterCode,
                        DestPeerCode = Variables.SelfPeerCode,
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
            using (var p = new ProducerBuilder<Null, MessageWrapper<object>>(_config).Build())
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
    }
}