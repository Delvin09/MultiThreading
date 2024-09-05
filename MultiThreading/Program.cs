using System.Diagnostics;
using System.Text;
using System.Threading;

namespace MultiThreading
{
    abstract class TaskProcessorBase<T>
    {
        protected readonly T[] _array;

        protected readonly int _taskCount;
        protected readonly CancellationToken _token;

        public TaskProcessorBase(T[] array, int taskCount, CancellationToken token)
        {
            _array = array;
            _taskCount = taskCount;
            _token = token;
        }

        public virtual Task Run()
        {
            var tasks = new Task[_taskCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                var num = i;
                tasks[i] = Task.Run(() => Process(num), _token);
            }

            return Task.WhenAll(tasks);
        }

        protected abstract void Process(int num);
    }

    abstract class RandomTaskProcessor<T> : TaskProcessorBase<T>
    {
        protected readonly Random[] _randoms;

        protected RandomTaskProcessor(T[] array, int taskCount, CancellationToken token)
            : base(array, taskCount, token)
        {
            var random = new Random();
            _randoms = new Random[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                _randoms[i] = new Random(random.Next());
            }
        }
    }

    class RandomIntTaskProcessor : RandomTaskProcessor<int>
    {
        public RandomIntTaskProcessor(int[] array, int threadCount, CancellationToken token)
            : base(array, threadCount, token)
        {
        }

        protected override void Process(int num)
        {
            if (_token.IsCancellationRequested) return;

            var itemsByThread = _array.Length / _taskCount;
            var span =
                num == _taskCount - 1
                   ? _array[(num * itemsByThread)..]
                   : _array.AsSpan(num * itemsByThread, itemsByThread);

            var random = _randoms[num];

            for (int i = 0; i < span.Length; i++)
            {
                if (_token.IsCancellationRequested) return;

                span[i] = random.Next();
            }
        }
    }

    class RandomTaskWordProcessor : RandomTaskProcessor<string>
    {
        public RandomTaskWordProcessor(string[] array, int taskCount, CancellationToken token)
            : base(array, taskCount, token)
        {
        }

        protected override void Process(int num)
        {
            if (_token.IsCancellationRequested) return;

            var itemsByThread = _array.Length / _taskCount;
            var span =
                num == _taskCount - 1
                   ? _array[(num * itemsByThread)..]
                   : _array.AsSpan(num * itemsByThread, itemsByThread);

            var random = _randoms[num];

            for (int i = 0; i < span.Length; i++)
            {
                if (_token.IsCancellationRequested) return;

                var bytes = new byte[random.Next(2, 11)];
                random.NextBytes(bytes);
                span[i] = Encoding.ASCII.GetString(bytes);
            }
        }
    }

    class SumTaskProcessor : TaskProcessorBase<int>
    {
        private long[] _results;

        public SumTaskProcessor(int[] array, int taskCount, CancellationToken token)
            : base(array, taskCount, token)
        {
            _results = new long[_taskCount];
        }

        protected override void Process(int num)
        {
            if (_token.IsCancellationRequested) return;

            var itemsByThread = _array.Length / _taskCount;
            var span =
                num == _taskCount - 1
                   ? _array[(num * itemsByThread)..]
                   : _array.AsSpan(num * itemsByThread, itemsByThread);

            for (int i = 0; i < span.Length; i++)
            {
                if (_token.IsCancellationRequested) return;

                _results[num] += span[i];
            }
        }

        public override Task Run()
        {
            return base.Run().ContinueWith(t => _results.Sum(), _token);
        }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var cancelTask = Task.Run(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var k = Console.ReadKey();
                    if (k.Key == ConsoleKey.Escape)
                    {
                        cancellationTokenSource.Cancel();
                    }
                }
            });

            //Console.WriteLine("Run word generator!");
            //var words = new string[1_000_000];
            //var generator = new RandomTaskWordProcessor(words, 4, cancellationTokenSource.Token);
            //var wordGenTask = generator.Run();

            Console.WriteLine("Run int generator!");
            var arr = new int[100_000_000];
            var randomProc = new RandomIntTaskProcessor(arr, 6, cancellationTokenSource.Token);
            var intGenTask = randomProc.Run();

            await intGenTask;

            Console.WriteLine("Start parallel");

            //var listLock = new object();
            //var bag = new System.Collections.Concurrent.ConcurrentBag<long>();

            //Parallel.ForEach(arr
            //    , new ParallelOptions { CancellationToken = cancellationTokenSource.Token }
            //    , item =>
            //    {
            //        if (item % 2 == 0)
            //        {
            //            bag.Add(item);
            //        }
            //    }
            //);

            var bag = arr.AsParallel().Where(i => i % 2 == 0).ToList();

            Console.WriteLine("Start class");

            var sumProcessor = new SumTaskProcessor(arr, 4, cancellationTokenSource.Token);
            var sumTask = (Task<long>)sumProcessor.Run();

            await Task.WhenAll(sumTask/*, wordGenTask*/);

            Console.WriteLine(bag.Count);
            Console.WriteLine(sumTask.Result);
            Console.WriteLine("Done");


            //var t = GetLines();

            //Console.WriteLine("Do Somthing");

            //var lines = await t;


            /*var lamps = new Lamp[]
            {
                new Lamp(10, 1, ConsoleColor.Red, 1700, 300),
                new Lamp(20, 2, ConsoleColor.Green, 200, 1300),
                new Lamp(30, 2, ConsoleColor.Blue, 400, 500),
                new Lamp(40, 2, ConsoleColor.Yellow, 1000, 200),
                new Lamp(50, 2, ConsoleColor.White, 340, 470),
                new Lamp(60, 2, ConsoleColor.Cyan, 490, 1210),
                new Lamp(70, 2, ConsoleColor.Magenta, 670, 340),
                new Lamp(80, 2, ConsoleColor.DarkGray, 570, 1400),
            };

            var manager = new LampSwitchManager(lamps);
            manager.Run();*/
        }


        static async Task<string[]> GetLines()
        {
            var task = File.ReadAllLinesAsync("text.txt");

            Console.WriteLine("Start reading");

            var lines = await task;

            foreach (var item in lines)
            {
                Console.WriteLine("do something with lines");
            }

            return lines;
        }
    }
}
