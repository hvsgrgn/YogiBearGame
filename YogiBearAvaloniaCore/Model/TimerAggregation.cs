using System;
using System.Collections.Generic;
using System.Timers;

namespace YogiBearAvaloniaCore.Model {

    //Ez egy wrapper class, amitol konnyebb az timer hasznalata és biztosítja a megfelelo disposeolast.
    public class TimerAggregation : ITimer, IDisposable         //valojaban csak hozzaadunk egy kis pizazz-t az interface-ekhez
    {

        private readonly System.Timers.Timer _timer;  //beleteszunk egy timert

        public bool Enabled //basic getter-setter
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        public double Interval //basic getter-setter
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public event EventHandler? Elapsed;     //intervalonkent valtodik ki

        public TimerAggregation()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += (sender, e) =>
            {
                Elapsed?.Invoke(sender, e);
            };
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()       // eroforrasok felszabaditasara, de az automat ha mar nincs referencia a timerre
        {
            _timer.Dispose();
        }
    }
}


