using System;
using System.IO;
using Avalonia.Media.Imaging;
using YogiBearAvaloniaCore.Model;
using YogiBearAvaloniaCore.Persistence;

namespace YogiBearAvalonia.ViewModel;


//kulon kezeljuk a megjeleniteset az egyes cellaknak a game mapen
public class CellViewModel : ViewModelBase
{
    #region Private members
    private readonly int _row;
    private readonly int _col;
    private readonly GameModel? _gameModel;
    private Bitmap? _imageSource;

    #endregion
    public CellViewModel(int row, int col, GameModel? gameModel)
    {
        _row = row;
        _col = col;
        _gameModel = gameModel;
    }

    //public property, amire lehet bindolni
    public Bitmap? ImageSource
    {
        get => _imageSource;
        set{
            _imageSource = value;
            OnPropertyChanged(nameof(ImageSource));
        } 
    }

    //frissiti a cella kepet az alapjan, h a modell szerint mi van benne
    public void UpdateCell()
    {
        Console.WriteLine($"Updating cell [{_row},{_col}] - Yogi: {_gameModel?.IsYogiAt(_row, _col)}, Ranger: {_gameModel?.IsRangerAt(_row, _col)}");
        if (_gameModel != null) {
            if (_gameModel.IsYogiAt(_row, _col))
            {
                ImageSource = LoadImageFromAssets("bear.png");
                Console.WriteLine($"  -> Setting bear image");
            }
            else if (_gameModel.IsRangerAt(_row, _col))
            {
                ImageSource = LoadImageFromAssets("ranger.png");
                Console.WriteLine($"  -> Setting ranger image");
            }
            else
            {
                switch (_gameModel[_row, _col])
                {
                    case Fillers.None:
                        ImageSource = null;
                        break;
                    case Fillers.Tree:
                        ImageSource = LoadImageFromAssets("tree.png"); ;
                        break;
                    case Fillers.Bush:
                        ImageSource = LoadImageFromAssets("bush.png"); ;
                        break;
                    case Fillers.Basket:
                        ImageSource = LoadImageFromAssets("picnic-basket.png"); ;
                        break;
                }
            }
        }
    }
}