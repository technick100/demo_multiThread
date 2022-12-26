using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace Con_Prod_Console
{
    internal delegate void ClbckDelegate(string msg);

    internal class Producer
    {
        private readonly ChannelWriter<Tuple<string, ClbckDelegate>> _writer;
        private readonly int _identifier;
        private readonly int _delay;

        public Producer(ChannelWriter<Tuple<string, ClbckDelegate>> writer, int identifier, int delay)
        {
            _writer = writer;
            _identifier = identifier;
            _delay = delay;
        }

        private void RunThis(string msg)
        {
            // 콘솔출력 : 1024
            Console.WriteLine($"PRODUCER ({this._identifier}): Callback {msg}");
        }

        public async Task BeginProducing()
        {
            Console.WriteLine($"PRODUCER ({this._identifier}): Starting");

            for (var i = 0; i < 2; i++)
            {
                await Task.Delay(_delay); // simulate producer building/fetching some data

                var msg = $"PRODUCER_{this._identifier} - {DateTime.UtcNow:G}";

                Console.WriteLine($"PRODUCER ({this._identifier}): Creating {msg}");

                var clbck_func = new ClbckDelegate(RunThis);
                var input = new Tuple<string, ClbckDelegate>(msg, clbck_func);
                await _writer.WriteAsync(input);
            }

            Console.WriteLine($"PRODUCER ({this._identifier}): Completed");
        }
    }

    internal class Consumer
    {
        private readonly ChannelReader<Tuple<string, ClbckDelegate>> _reader;
        private readonly int _identifier;
        private readonly int _delay;

        public Consumer(ChannelReader<Tuple<string, ClbckDelegate>> reader, int identifier, int delay)
        {
            _reader = reader;
            _identifier = identifier;
            _delay = delay;
        }

        public async Task ConsumeData()
        {
            Console.WriteLine($"CONSUMER ({_identifier}): Starting");

            while (await _reader.WaitToReadAsync())
            {
                if (_reader.TryRead(out var item))
                {
                    await Task.Delay(_delay); // simulate processing time

                    Console.WriteLine($"CONSUMER ({_identifier}): Consuming {item.Item1}");
                    item.Item2(item.Item1);
                }
            }

            Console.WriteLine($"CONSUMER ({_identifier}): Completed");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            await StartProducerConsumer();

            Console.ReadLine();
        }

        public static async Task StartProducerConsumer()
        {
            var channel = Channel.CreateUnbounded<Tuple<string, ClbckDelegate>>(); 

            var consumer1 = new Consumer(channel.Reader, 1, 1500);
            Task consumerTask1 = consumer1.ConsumeData();   // begin consuming

            List<Task> lstProducerTasks = new List<Task>();
            for (var i = 0; i < 2; i++)
            {
                var producer = new Producer(channel.Writer, i, 2000);
                var producerTask = producer.BeginProducing();    // begin producing
                lstProducerTasks.Add(producerTask);
            }
            await Task.WhenAll(lstProducerTasks).ContinueWith(_ => channel.Writer.Complete());

            await consumerTask1;
        }
    }
}
