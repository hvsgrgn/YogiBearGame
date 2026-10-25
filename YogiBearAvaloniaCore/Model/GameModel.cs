using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YogiBearAvaloniaCore.Persistence;

namespace YogiBearAvaloniaCore.Model; 
public class GameModel : IDisposable // ezt az adattagkent felvett timer miatt kell implementalni
{

    #region Members

    private GameBoard _gameBoard;           //a model tartja szamon a jatekterepet
    private TimerAggregation _timer;        // peldanyositjuk a custom timert
    private int _gameTime;                // itt taroljuk az eltelt idot
    private int _collectedBasketCount = 0;        // ezek a jatekosnak visszajelzesre es a winconditionhoz kellenek
    private int _allBasketCount;
    private YogiBear _yogiBear;                 // maci laci peldany
    private List<Ranger> _rangers;              // vadorok listaja

    #endregion

    public Fillers this[int x, int y] => _gameBoard[x, y];

    public GameModel(GameBoard board, List<Ranger> rangers, int allBasketCount)
    {
        _gameBoard = board;
        _rangers = rangers;
        _allBasketCount = allBasketCount;
        _yogiBear = new YogiBear();
        _timer = new TimerAggregation();
        _timer.Interval = Config.TIMER_INTERVAL;
        _timer.Elapsed += OnTimerTick;
        _timer.Start();
    }

    #region Properties
    public int CollectedBasketCount => _collectedBasketCount;
    public int AllBasketCount => _allBasketCount;
    public int GameBoardSize => _gameBoard.Size;
    public int GameTime => _gameTime;

    public bool IsTimerRunning => _timer.Enabled;

    #endregion

    #region Methods for game mechanic
    //ha meg nincs win/lose condition teljesitve, egyebkent megfelelo esemenyt valt ki
    private void CheckGameOver()
    {
        if (_timer.Enabled == false) return;

        bool hasAnyRangerCaughtYogi = false;
        foreach (Ranger ranger in _rangers)
        {
            if (ranger.LookForBear(_yogiBear.CoordsX, _yogiBear.CoordsY))
            {
                hasAnyRangerCaughtYogi = true;
                break;
            }
        }
        if (!hasAnyRangerCaughtYogi && _collectedBasketCount == _allBasketCount)
        {
            _timer.Stop();
            GameWon?.Invoke(this, EventArgs.Empty);
        }
        else if (hasAnyRangerCaughtYogi)
        {
            _timer.Stop();
            GameLost?.Invoke(this, EventArgs.Empty);
        }
    }

    //helper metodus ahhoz, hogy meg a mozgatas elott eldontsuk, jo lesz-e
    public bool isValidMove(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _gameBoard.Size || y >= _gameBoard.Size || (_gameBoard[x, y] != Fillers.None && _gameBoard[x, y] != Fillers.Basket))
            return false;
        return true;
    }

    private void MoveRangers()
    {
        foreach (var ranger in _rangers)
        {
            int nextX = ranger.CoordsX + ranger.DirectionX;
            int nextY = ranger.CoordsY + ranger.DirectionY;

            // megnezzuk, hogy szabad-e es palyan van-e a kivant pozicio
            if (!isValidMove(nextX, nextY))
                ranger.Turn();
            else
                ranger.Move();
        }
    }
    public void MoveYogiBear(int deltaX, int deltaY)
    {
        int newX = _yogiBear.CoordsX + deltaX;
        int newY = _yogiBear.CoordsY + deltaY;


        // Itt is validalni kell a potencialis uj koordinatat
        if (isValidMove(newX, newY))
        {
            _yogiBear.MoveTo(newX, newY);

            // Ha kosaras mezore leptunk
            if (_gameBoard[newX, newY] == Fillers.Basket)
            {
                _collectedBasketCount++;
                _gameBoard.SetField(newX, newY, Fillers.None); // Akkor felvesszuk es kiszedjuk a gridbol
            }

            GameAdvanced?.Invoke(this, EventArgs.Empty);
            CheckGameOver();
        }
    }
    #endregion

    #region Methods for helping display
    public bool IsYogiAt(int x, int y)
    {
        return _yogiBear.CoordsX == x && _yogiBear.CoordsY == y;
    }

    public bool IsRangerAt(int x, int y)
    {
        foreach (var ranger in _rangers)
        {
            if (ranger.CoordsX == x && ranger.CoordsY == y)
                return true;
        }
        return false;
    }

    #endregion

    #region Methods that are timer related
    public void PauseGame()
    {
        CheckGameOver();
        _timer.Stop();
    }

    public void ResumeGame()
    {
        CheckGameOver();
        _timer.Start();
    }

    public void NewGame()
    {
        _timer.Stop();
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
    #endregion

    #region Event Handlers

    public event EventHandler? GameAdvanced;
    public event EventHandler? GameWon;
    public event EventHandler? GameLost;

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _gameTime++;
        MoveRangers();
        GameAdvanced?.Invoke(this, EventArgs.Empty);
        CheckGameOver();
        // minden ticknel: osszegyujtott kosarszamot es kiirt idot frissitjuk, rangert leptetjuk, megnezzuk, nincs-e vege a jateknak
    }
    #endregion
}

