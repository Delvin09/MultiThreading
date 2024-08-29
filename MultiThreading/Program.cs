




using System.Diagnostics;
using System.Threading;

namespace MultiThreading
{
    class Lamp
    {
        private readonly static object _lock = new object();

        private readonly int x;
        private readonly int y;
        private readonly ConsoleColor color;
        private readonly int timeOn;
        private readonly int timeOff;

        private bool isOn = false;
        private Thread _thread;

        public Lamp(int x, int y, ConsoleColor color, int timeOn, int timeOff)
        {
            this.x = x;
            this.y = y;
            this.color = color;
            this.timeOn = timeOn;
            this.timeOff = timeOff;
        }

        public void Toggle()
        {
            if (isOn)
            {
                isOn = false;
            }
            else
            {
                isOn = true;
            }

            Show();
        }

        private void Show()
        {
            lock (_lock)
            {
                Console.CursorVisible = false;
                Console.ResetColor();
                Console.SetCursorPosition(x, y);
                if (isOn) Console.BackgroundColor = color;
                Console.WriteLine(" ");
            }
        }

        public void Run()
        {
            _thread = new Thread(InnerRun)
            {
                IsBackground = true,
                Name = color.ToString() + " Thread"
            };

            _thread.Start();
        }

        private void InnerRun()
        {
            while (true)
            {
                if (isOn)
                {
                    Thread.Sleep(timeOn);
                    Toggle();
                }
                else
                {
                    Thread.Sleep(timeOff);
                    Toggle();
                }
            }
        }

        public void Wait()
        {
            _thread.Join();
        }
    }

    class LampSwitchManager
    {
        private readonly Lamp[] _lamps;

        public LampSwitchManager(Lamp[] lamps)
        {
            this._lamps = lamps;
        }

        public void Run()
        {
            foreach (var lamp in _lamps)
                lamp.Run();

            foreach (var lamp in _lamps)
                lamp.Wait();
        }
    }

    class RandomThreadProcessor
    {
        private readonly Random[] _randoms;
        private readonly Thread[] _threads;
        private readonly int[] _array;

        public RandomThreadProcessor(int[] array, int threadCount)
        {
            var random = new Random();
            _randoms = new Random[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                _randoms[i] = new Random(random.Next());
            }

            _threads = new Thread[threadCount];
            _array = array;
        }

        public void Run()
        {
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(Process);
            }

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Start(i);
            }

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Join();
            }
        }

        private void Process(object? threadNumber)
        {
            var itemsByThread = _array.Length / _threads.Length;
            var num = (int)threadNumber;

            var span =
                num == _threads.Length - 1
                   ? _array[(num * itemsByThread)..]
                   : _array.AsSpan(num * itemsByThread, itemsByThread);

            var random = _randoms[num];

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = random.Next();
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            //1. 1 - 0.6329866
            //2. 1 - 00:00:00.6115480
            //3. 1 - 00:00:00.6116033

            //2 - 00:00:00.3768524
            //2 - 00:00:00.3738134
            //2 - 00:00:00.3749054

            //4 - 00:00:00.2428593

            //8 - 00:00:00.1811266

            //16 - 00:00:00.1261678

            //32 - 00:00:00.1107846

            //64 - 00:00:00.1059993


            var sw = Stopwatch.StartNew();

            var arr = new int[100_000_000];
            var randomProc = new RandomThreadProcessor(arr, 1024);
            randomProc.Run();

            sw.Stop();
            Console.WriteLine(sw.Elapsed.ToString());


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
    }
}
