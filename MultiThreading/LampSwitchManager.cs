namespace MultiThreading
{
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
}
