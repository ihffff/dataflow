namespace Demo.Example7
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    class FilteringExample
    {
        static void Main(string[] args)
        {
            var handler = new Handler();
            handler.HandleAsync().GetAwaiter().GetResult();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }

    class Handler
    {
        private BatchBlock<int> buffer;

        private TransformManyBlock<int[], int> transform;

        private ActionBlock<int> action;

        public Handler()
        {
            this.CreatePipeline();
        }

        private void CreatePipeline()
        {
            this.buffer = new BatchBlock<int>(5);
            this.transform = new TransformManyBlock<int[], int>(records => records.Where(x => x % 2 == 0));

            this.action = new ActionBlock<int>(i => Log(i.ToString(), ConsoleColor.Green));

            this.buffer.LinkTo(this.transform, new DataflowLinkOptions() { PropagateCompletion = true });
            this.transform.LinkTo(this.action, new DataflowLinkOptions() { PropagateCompletion = true });
        }

        public async Task HandleAsync()
        {
            for (var i = 0; i < 100; i++)
            {
                await this.buffer.SendAsync(i);
                Log(i.ToString(), ConsoleColor.Yellow);
            }

            this.buffer.Complete();

            await this.action.Completion;
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