using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SharedArea.Utils
{
    public static class Logger
    {
        private static BlockingCollection<KeyValuePair<string, string>> _queue = new BlockingCollection<KeyValuePair<string, string>>();
        private static bool _alive = true;

        public static void Log(string queue, string message)
        {
            _queue.Add(new KeyValuePair<string, string>(queue, message));
        }

        public static void Setup()
        {
            new Thread((() =>
            {
                while (_alive)
                {
                    var message = _queue.Take();
                    //File.AppendAllText(message.Key + ".txt", Environment.NewLine + message.Value + Environment.NewLine);
                    Console.WriteLine(message.Value);
                }
            })).Start();
        }
    }
}