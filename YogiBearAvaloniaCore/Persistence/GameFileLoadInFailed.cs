using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearAvaloniaCore.Persistence;

public class GameFileLoadInFailed : Exception
{
    public GameFileLoadInFailed() { }

    public GameFileLoadInFailed(string message) : base(message) { }
}
