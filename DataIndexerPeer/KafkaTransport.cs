using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using SharedArea;
using SharedArea.Wrappers;

namespace DataIndexerPeer
{
    public class KafkaTransport : Answerable
    {
        private static readonly KafkaTransport Instance = new KafkaTransport();
        private static Dictionary<string, Dictionary<string, string>> _configs;
        private static readonly Dictionary<string, object> PendingQuestions = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> ReceivedAnswers = new Dictionary<string, object>();
        
        public KafkaTransport()
        {
            if (_configs == null)
            {
                _configs = new Dictionary<string, Dictionary<string, string>>();
                
                foreach (var address in SharedArea.GlobalVariables.SuperPeerAddresses)
                {
                    _configs.Add(address.Key, new Dictionary<string, string>()
                    {
                        {"bootstrap.servers", address.Value},
                        {"username", SharedArea.GlobalVariables.KafkaUsername},
                        {"password", SharedArea.GlobalVariables.KafkaPassword},
                        {"security.protocol", "SASL_PLAINTEXT"},
                        {"sasl.mechanism", "PLAIN"}
                    });
                }
            }
        }

        public async Task<object> AskSinglePeer<T>(string clusterCode, string peerCode, T message)
        {
            var questionId = Guid.NewGuid().ToString();
            var question = new object();
            PendingQuestions.Add(questionId, question);
            var conf = _configs[clusterCode];
            using (var p = new ProducerBuilder<Null, MessageWrapper<T>>(conf).Build())
                await p.ProduceAsync(message.GetType().FullName, new Message<Null, MessageWrapper<T>>()
                {
                    Value = new MessageWrapper<T>() {Message = message, DestClusterCode = clusterCode, DestPeerCode = peerCode, QuestionId = questionId}
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
            var conf = _configs[clusterCode];
            using (var p = new ProducerBuilder<Null, MessageWrapper<object>>(conf).Build())
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