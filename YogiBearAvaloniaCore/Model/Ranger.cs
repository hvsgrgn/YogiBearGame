using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearAvaloniaCore.Model; 
public class Ranger
{
    #region Members

    private int _coordsX;
    private int _coordsY;
    private int _directionY;     //-1 vagy 1 erteket vehet fel, ennek hozzaadogatasaval mozog a ranger az X tengely menten
    private int _directionX;


    #endregion

    #region Properties
    public int CoordsX => _coordsX;
    public int CoordsY => _coordsY;

    public int DirectionX => _directionX;

    public int DirectionY => _directionY;
    #endregion


    public Ranger(int x, int y, int directionX, int directionY)
    {
        this._coordsX = x;
        this._coordsY = y;
        this._directionX = directionX;
        this._directionY = directionY;

        //megjegyzes: mivel egyenes menten setal, es ebbol nem ter ki, valamelyik irany konstans 0, a masik valtogat 1 es -1 kozt

    }

    #region Methods for moving and catching the bear
    public void Move()
    {
        _coordsX += _directionX;
        _coordsY += _directionY;
    }

    public void Turn()
    {
        _directionX *= -1;
        _directionY *= -1;
    }

    public Boolean LookForBear(int x, int y)
    {
        return (Math.Abs(_coordsX - x) <= 1 && Math.Abs(_coordsY - y) <= 1);
    }

    #endregion
}


