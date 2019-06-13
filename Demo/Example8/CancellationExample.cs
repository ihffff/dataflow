namespace Demo.Example6
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    class CancellationExample
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

        private CancellationTokenSource cts;

        public Handler()
        {
            var cts = new CancellationTokenSource();

            this.CreatePipeline(cts);
        }

        private void CreatePipeline(CancellationTokenSource cts)
        {
            this.cts = cts;

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var blockOptions = new ExecutionDataflowBlockOptions { CancellationToken = cts.Token };

            this.buffer = new BufferBlock<int>(new GroupingDataflowBlockOptions() { CancellationToken = cts.Token });
            this.transform = new TransformBlock<int, int>(
                async i =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        return i * 2;
                    }, 
                blockOptions);
            this.action = new ActionBlock<int>(
                i => Log(i.ToString(), ConsoleColor.Green),
                blockOptions);

            this.buffer.LinkTo(this.transform, linkOptions);
            this.transform.LinkTo(this.action, linkOptions);
        }

        public async Task HandleAsync()
        {
            for (var i = 0; i < 100; i++)
            {
                await this.buffer.SendAsync(i);
            }

            this.buffer.Complete();

            await Task.WhenAny(
                this.action.Completion, 
                Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(x => this.cts.Cancel()));
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
