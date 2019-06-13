namespace Demo.Example1
{
    using System;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    // Demonstrates a basic producer and consumer pattern that uses dataflow.
    class ProducerConsumerExample
    {
        // Initialize a counter to track the number of bytes that are processed.
        static void Produce(ITargetBlock<int> target)
        {
            for (var i = 0; i < 100; i++)
            {
                Log(i.ToString(), ConsoleColor.Yellow);

                // Post the result to the message block.
                target.Post(i);
            }

            // Set the target to the completed state to signal to the consumer
            // that no more data will be available.
            target.Complete();
        }

        // Demonstrates the consumption end of the producer and consumer pattern.
        static async Task<int> ConsumeAsync(IReceivableSourceBlock<int> source)
        {
            var sum = 0;

            // Read from the source buffer until the source buffer has no 
            // available output data.
            while (await source.OutputAvailableAsync())
            {
                // Needed if you have multiple consumers
                while (source.TryReceive(out var data))
                {
                    Log(data.ToString(), ConsoleColor.Green);

                    // Sum it up.
                    sum += data;
                }
            }

            return sum;
        }

        static void Main(string[] args)
        {
            // Create a BufferBlock<int> object. This object serves as the 
            // target block for the producer and the source block for the consumer.
            var buffer = new BufferBlock<int>();

            // Start the consumer. The Consume method runs asynchronously. 
            var consumer = ConsumeAsync(buffer);

            // Post source data to the dataflow block.
            Produce(buffer);

            // Wait for the consumer to process all data.
            consumer.Wait();

            // Print the sum to the console.
            Console.WriteLine("Sum is {0}.", consumer.Result);

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }
}