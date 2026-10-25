using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearAvaloniaCore.Model; 
public class YogiBear
{
    private int _coordsX;
    private int _coordsY;

    public int CoordsX => _coordsX;
    public int CoordsY => _coordsY;

    public YogiBear()
    {
        this._coordsX = 0;
        this._coordsY = 0;
    }
    public void MoveTo(int newX, int newY)
    {
        _coordsX = newX;
        _coordsY = newY;
    }
}





