using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using YogiBearAvaloniaCore.Model;
using YogiBearAvaloniaCore.Persistence;

namespace YogiBearAvalonia.ViewModel;

public class GameViewModel : ViewModelBase, IDisposable
{
    #region Private members

    private GameModel? _gameModel;
    private bool _isPaused;
    private bool _isGameOver;
    private int _gameTime;
    private string? _basketsCollected;
    private Bitmap? _welcomeBackground;
    private Bitmap? _pauseIcon;
    private bool _isInWelcomeMode = true;
    private string? _lastLoadedFilePath;
    private string _pauseMenuText = "Pause Game";

    #endregion

    #region Public properties

    public string PauseMenuText {
        get => _pauseMenuText;
        set {
            _pauseMenuText = value;
            OnPropertyChanged();
        }
    }

    public Bitmap? WelcomeBackground
    {
        get => _welcomeBackground;
        set
        {
            _welcomeBackground = value;
            OnPropertyChanged();
        }
    }

    public Bitmap? PauseIcon
    {
        get => _pauseIcon;
        set
        {
            _pauseIcon = value;
            OnPropertyChanged();
        }
    }

    //automatikus onpropertychanged hivas amikor a lista barmely eleme valtozik
    public ObservableCollection<CellViewModel> GameBoardCells { get; private set; }
    public int GameSize => _gameModel?.GameBoardSize ?? 0;
    public int CellSize => Config.CELL_SIZE;
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            _isPaused = value;
            OnPropertyChanged();
        }
    }
    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            OnPropertyChanged();
        }
    }
    public int GameTime
    {
        get => _gameTime;
        private set
        {
            _gameTime = value;
            OnPropertyChanged();
        }
    }
    public string? BasketsCollected
    {
        get => _basketsCollected;
        private set
        {
            _basketsCollected = value;
            OnPropertyChanged();
        }
    }
    public bool IsInWelcomeMode
    {
        get => _isInWelcomeMode;
        private set
        {
            _isInWelcomeMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGameBoardVisible));
            OnPropertyChanged(nameof(IsStatusBarVisible));
        }
    }
    public bool IsGameBoardVisible => !IsInWelcomeMode;
    public bool IsStatusBarVisible => !IsInWelcomeMode;

    #endregion

    #region Commands

    public ICommand LoadGameCommand { get; }
    public ICommand PauseGameCommand { get; }
    public ICommand ShowRulesCommand { get; }
    public ICommand MoveYogiCommand { get; }

    #endregion

    public GameViewModel()
    {
        //a jatek elindulasakor alapvetoen a menubol kell indulnunk
        IsInWelcomeMode = true;
        //kell egy ures lista, mezokkel feltoltendo
        GameBoardCells = new ObservableCollection<CellViewModel>();

        //hozzarendeljuk a commandokat a megfelelo folyamatokhoz    
        LoadGameCommand = new RelayCommand(async () => await LoadGameAsync());
        PauseGameCommand = new RelayCommand(TogglePause, CanPauseGame);
        ShowRulesCommand = new RelayCommand(ShowRules);
        MoveYogiCommand = new RelayCommand<string>(MoveYogiBear);

        WelcomeBackground = LoadImageFromAssets("background.jpg");
        PauseIcon = LoadImageFromAssets("pause.png");
    }

    #region Command Implementations

    public async Task LoadGameAsync(string? filePath = null)
    {
        try
        {
                if (string.IsNullOrEmpty(filePath))
                {
                    // main window lekerdezese
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var mainWindow = desktop.MainWindow;
                        if (mainWindow == null) return;

                        // Avalonia file picker
                        var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Select a game file (.txt extension)",
                            AllowMultiple = false,
                            FileTypeFilter = new[]
                            {
                            new FilePickerFileType("Game files") { Patterns = new[] { "*.txt" } },
                            new FilePickerFileType("All files") { Patterns = new[] { "*" } }
                        }
                        });

                        if (files.Count == 0) return;
                        filePath = files[0].Path.LocalPath;
                    }
                    else
                    {
                        return;
                    }
                }

                // ha volt mar elozo jatek, akkor le kell iratkozni az eventekrol
                if (_gameModel != null)
            {
                _gameModel.GameAdvanced -= OnGameAdvanced;
                _gameModel.GameWon -= OnGameWon;
                _gameModel.GameLost -= OnGameLost;
            }

            //elvegezzuk a persistence segitsegevel az adatbeolvasast a kivalasztott fajlbol
            var dataAccess = new FileDataAccess();
            var (board, rangerTuples, basketCount) = await dataAccess.LoadAsync(filePath);
            var rangers = rangerTuples.Select(tuple => new Ranger(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4)).ToList();

            MessageRequested?.Invoke(this, new MessageEventArgs(
                "Game loaded successfully!",
                "Success",
                MessageType.Information));

            // elmentjuk arra az esetre, ha a jatekos gyorsinditani szeretene a palyat vereseg eseten
            _lastLoadedFilePath = filePath;

            _gameModel = new GameModel(board, rangers, basketCount);
            Console.WriteLine($"GameModel created - Timer enabled: {_gameModel.IsTimerRunning}");
            //feliratkozunk az eventekre
            _gameModel.GameAdvanced += OnGameAdvanced;
            _gameModel.GameWon += OnGameWon;
            _gameModel.GameLost += OnGameLost;

            IsPaused = false;
            IsGameOver = false;
            IsInWelcomeMode = false;

            InitializeGameBoard();
            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageRequested?.Invoke(this, new MessageEventArgs(
                $"Failed to load game: {ex.Message}",
                "Load Error",
                MessageType.Error));
        }
    }

    //ezzel a fuggvennyel a parameterben kapott irany alapjan modositjuk a UI-on a maci megjeleneset
    private void MoveYogiBear(string? direction)
    {
        //ha nincs a jatek vagy az irany inicializalva, megallt vagy vegetert:
        if (_gameModel == null || IsPaused || IsGameOver || string.IsNullOrEmpty(direction))
            return;

        try
        {
            //a kapott parametert atforditjuk koordinata valtozasra
            int deltaRow = 0, deltaCol = 0;

            switch (direction.ToLower())
            {
                case "up":
                    deltaRow = -1;
                    break;
                case "down":
                    deltaRow = 1;
                    break;
                case "left":
                    deltaCol = -1;
                    break;
                case "right":
                    deltaCol = 1;
                    break;
            }
            //a modellben is regisztraljuk a valtozast
            _gameModel.MoveYogiBear(deltaRow, deltaCol);
            //frissitjuk a maci es a rangerek helyzetet a UI-on
            UpdateGameBoard();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    //ezzel a metodussal lehet letiltani a kezdokepernyon es a jatek vegekor a megallitast
    private bool CanPauseGame()
    {
        return !IsInWelcomeMode && !IsGameOver;
    }

    //a jatek megallitasat hivja es frissiti a UI-on a menupont megjeleneset
    public void TogglePause()
    {
        if (IsGameOver) return;

        IsPaused = !IsPaused;
        if (IsPaused)
        {
            _gameModel?.PauseGame();
            PauseIcon = LoadImageFromAssets("resume.png");
            PauseMenuText = "Resume Game";
        }
        else
        {
            _gameModel?.ResumeGame();
            PauseIcon = LoadImageFromAssets("pause.png");
            PauseMenuText = "Pause Game";
        }
    }

    #endregion

    #region Private Methods

    //letisztazza majd ujra feltolti a gameboardot
    private void InitializeGameBoard()
    {
        if (_gameModel != null)
        {
            //"kidobjuk" az osszes cellat a gyujtemenybol
            GameBoardCells.Clear();
            for (int row = 0; row < GameSize; row++)
            {
                for (int col = 0; col < GameSize; col++)
                {
                    GameBoardCells.Add(new CellViewModel(row, col, _gameModel));
                }
            }

            OnPropertyChanged(nameof(GameSize));
            UpdateGameBoard();
        }
    }

    //vegigmegy minden cellan es frissiti oket, valamint a status stripet is
    private void UpdateGameBoard()
    {
        foreach (var cell in GameBoardCells)
        {
            cell.UpdateCell();
        }
        UpdateStatus();
    }
    private void UpdateStatus()
    {
        if (_gameModel != null)
        {
            GameTime = _gameModel.GameTime;
            BasketsCollected = $"{_gameModel.CollectedBasketCount}/{_gameModel.AllBasketCount}";
        }
    }
    private void OnGameAdvanced(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            UpdateGameBoard();
        });
    }
    private void OnGameWon(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsGameOver = true;
            // Event, amire az app feliratkozva megjeleniti a megfelelo messageboxot
            MessageRequested?.Invoke(this, new MessageEventArgs(
                "Congratulations! You won!",
                "Victory",
                MessageType.Information));
            ResetGameState();
        });
    }
    private void OnGameLost(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsGameOver = true;
            // Retry dialogus
            MessageRequested?.Invoke(this, new MessageEventArgs(
                "Game over! You were caught by a ranger!",
                "Defeat",
                MessageType.Warning,
                shouldResumeGameAfter: false,
                shouldShowRetry: true));
            // Itt nem reseteljuk a jatekallapotot, varunk a jatekos valaszara
        });
    }

    private void ShowRules() {
        bool wasGameRunning = !IsPaused && !IsGameOver && !IsInWelcomeMode;
        if (wasGameRunning)
        {
            TogglePause();
        }

        MessageRequested?.Invoke(this, new MessageEventArgs(
            "Game Rules:\n\n" +
            "• Use arrow keys to move Yogi Bear\n" +
            "• Collect all baskets to win\n" +
            "• Avoid the rangers who can see in one block in any direction\n" +
            "• Trees and bushes block your and the rangers' path\n\n" +
            "Good luck!",
            "Game Rules",
            MessageType.Information,
            shouldResumeGameAfter: wasGameRunning));
    }
    //letisztazza a membereket egy jatek utan
    private void ResetGameState()
    {
        Dispose();
        _gameModel = null;

        GameBoardCells.Clear();
        IsGameOver = false;
        IsPaused = false;
        GameTime = 0;
        BasketsCollected = "0/0";
        PauseMenuText = "Pause Game";

        IsInWelcomeMode = true;

        OnPropertyChanged(nameof(GameSize));
    }
    
    //implementalja a dispose interfesz metodusat
    public void Dispose() {
        _gameModel?.Dispose();
    }

    public void OnMessageDialogClosed(bool shouldResumeGame, bool shouldRetry)
    {
        if (shouldRetry && !string.IsNullOrEmpty(_lastLoadedFilePath))
        {
            // Ha ujrainditja a jatekos a palyat, akkor nem kerulunk vissza a menube
            if (_gameModel != null)
            {
                _gameModel.GameAdvanced -= OnGameAdvanced;
                _gameModel.GameWon -= OnGameWon;
                _gameModel.GameLost -= OnGameLost;
                _gameModel.Dispose();
            }

            GameBoardCells.Clear();
            IsGameOver = false;
            IsPaused = false;

            _ = LoadGameAsync(_lastLoadedFilePath);
        }
        else if (shouldResumeGame && IsPaused)
        {
            // elinditja a timert
            TogglePause();
        }
        else if (IsGameOver && !shouldRetry)
        {
            //vissza a kezdokepernyore
            ResetGameState();
        }
    }

    #endregion

    #region Events

    // Event, ami keri a messageboxok letrehozasat
    public event EventHandler<MessageEventArgs>? MessageRequested;

    #endregion
}