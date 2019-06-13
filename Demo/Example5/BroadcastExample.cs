namespace Example5
{
    using System;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    class BroadcastExample
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

        private BroadcastBlock<int> broadcast;

        private TransformBlock<int, int> transformA;

        private TransformBlock<int, int> transformB;

        private ActionBlock<int> actionA;

        private ActionBlock<int> actionB;

        public Handler()
        {
            this.CreatePipeline();
        }

        private void CreatePipeline()
        {
            this.buffer = new BufferBlock<int>();

            this.broadcast = new BroadcastBlock<int>(null);

            this.transformA = new TransformBlock<int, int>(async i =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                    return i * 2;
                });
            this.transformB = new TransformBlock<int, int>(async i =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    return i * i;
                });

            this.actionA = new ActionBlock<int>(i => Log(i.ToString(), ConsoleColor.Green));
            this.actionB = new ActionBlock<int>(i => Log(i.ToString(), ConsoleColor.DarkGreen));

            this.buffer.LinkTo(this.broadcast, new DataflowLinkOptions() { PropagateCompletion = true });

            this.broadcast.LinkTo(this.transformA, new DataflowLinkOptions() { PropagateCompletion = true });
            this.broadcast.LinkTo(this.transformB, new DataflowLinkOptions() { PropagateCompletion = true });

            this.transformA.LinkTo(this.actionA, new DataflowLinkOptions() { PropagateCompletion = true });
            this.transformB.LinkTo(this.actionB, new DataflowLinkOptions() { PropagateCompletion = true });
        }

        public async Task HandleAsync()
        {
            for (var i = 0; i < 100; i++)
            {
                await this.buffer.SendAsync(i);
            }

            this.buffer.Complete();

            await Task.WhenAll(this.actionA.Completion, this.actionB.Completion);
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
