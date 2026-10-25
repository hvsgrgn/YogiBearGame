using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YogiBearAvaloniaCore.Model;

namespace YogiBear.Test;

    public class MockTimer : YogiBearAvaloniaCore.Model.ITimer, IDisposable
    {
        public bool Enabled { get; set; }
        public double Interval { get; set; }
        public event EventHandler? Elapsed;

        public void Start() => Enabled = true;
        public void Stop() => Enabled = false;

        public void RaiseElapsed() => Elapsed?.Invoke(this, EventArgs.Empty);

        public void Dispose() {
        
        }
}

