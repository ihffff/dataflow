namespace Demo.Example3
{
    using System;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    // Demonstrates ActionBlock and TransformBlock usage
    class BoundedCapacityExample
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
        private BufferBlock<int> buffer;

        private TransformBlock<int, int> transform;

        private ActionBlock<int> action;

        public Handler()
        {
            this.CreatePipeline();
        }

        private void CreatePipeline()
        {
            this.buffer = new BufferBlock<int>(new DataflowBlockOptions() { BoundedCapacity = 5 });
            this.transform = new TransformBlock<int, int>(
                async i =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    return i * 2;
                }, 
                new ExecutionDataflowBlockOptions() { BoundedCapacity = 5 });
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
