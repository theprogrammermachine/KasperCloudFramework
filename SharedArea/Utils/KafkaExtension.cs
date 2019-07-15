using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Confluent.Kafka;
using SharedArea.Wrappers;

namespace SharedArea.Utils
{
    public class KafkaExtension
    {
        public static void SetupConsumer<T, V>(IEnumerable<Dictionary<string, string>> configs, Bugsnag.Client bugsnag) where V : Answerable
        {
            var consumer = Activator.CreateInstance<T>();

            var transport = Activator.CreateInstance<V>();
            
            foreach (var config in configs)
            {
                foreach (var type in Assembly.Load("SharedArea").GetTypes().Where(t => 
                    t.Namespace.StartsWith("SharedArea.Commands")).ToArray())
                {
                    Task.Run(() =>
                    {
                        using (var c = new ConsumerBuilder<Ignore, MessageWrapper<object>>(config).Build())
                        {
                            c.Subscribe(type.FullName);
                            while (true)
                            {
                                try
                                {
                                    var cr = c.Consume();

                                    switch (cr.Value.MessageType)
                                    {
                                        case MessageWrapper<object>.WrapperType.Question:
                                        {
                                            var methodInfo = consumer.GetType().GetMethod("AnswerQuestion");
                                            var answer = methodInfo.Invoke(consumer, new [] { cr.Value.Message });
                                            transport.SendAnswerToQuestionaire(cr.Value.SrcClusterCode, cr.Value.SrcPeerCode, cr.Value.QuestionId, answer);
                                            break;
                                        }
                                        case MessageWrapper<object>.WrapperType.Answer:
                                            transport.NotifyAnswerReceived(cr.Value.QuestionId, cr.Value.Message);
                                            break;
                                        case MessageWrapper<object>.WrapperType.Notification:
                                        {
                                            var methodInfo = consumer.GetType().GetMethod("Consume");
                                            methodInfo.Invoke(consumer, new [] { cr.Value.Message });
                                            break;
                                        }
                                    }
                                }
                                catch (ConsumeException e)
                                {
                                    Logger.Log("Error", $"Error occured: {e.Error.Reason}");
                                    bugsnag?.Notify(e);
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("Error", $"Error occured: {e.Message}");
                                    bugsnag?.Notify(e);
                                }
                            }
                        }   
                    });   
                }
            }
        }

        public static void SetupConsumer<T, V>(IEnumerable<Dictionary<string, string>> configs) where V : Answerable
        {
            SetupConsumer<T, V>(configs, null);
        }
    }
}