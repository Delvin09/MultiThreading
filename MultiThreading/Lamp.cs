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
}
