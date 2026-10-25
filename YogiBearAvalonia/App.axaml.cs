using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Security.Cryptography.X509Certificates;
using YogiBearAvalonia.View;
using YogiBearAvalonia.ViewModel;

namespace YogiBearAvalonia;

public partial class App : Application, IDisposable
{
    private Window? _mainWindow;
    private GameViewModel? _viewModel;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _viewModel = new GameViewModel();

            // Feliratkozas az ablakkeres eventjere
            _viewModel.MessageRequested += OnMessageRequested;

            _mainWindow = new MainWindow
            {
                DataContext = _viewModel
            };

            desktop.MainWindow = _mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    //event handler az ablakkesziteshez
    private async void OnMessageRequested(object? sender, MessageEventArgs e)
    {
        var icon = e.Type switch
        {
            MessageType.Error => Icon.Error,
            MessageType.Warning => Icon.Warning,
            MessageType.Information => Icon.Info,
            MessageType.Question => Icon.Question,
            _ => Icon.Info
        };

        if (_mainWindow != null)
        {
            if (e.ShouldShowRetry)
            {
                // eldontendo kerdeses messagebox, ha vesztettunk
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    e.Title,
                    e.Message + "\n\nWould you like to retry? \nIf no, you will return to the menu.",
                    ButtonEnum.YesNo,
                    icon);

                var result = await messageBox.ShowWindowDialogAsync(_mainWindow);

                // Igen eseten ujrainditja a palyat, nem eseten visszavisz a menube
                bool shouldRetry = result == ButtonResult.Yes;
                _viewModel?.OnMessageDialogClosed(false, shouldRetry);
            }
            else
            {
                // Sima jovahagyasos messagebox, ha nyertunk
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    e.Title,
                    e.Message,
                    ButtonEnum.Ok,
                    icon);

                //nem hagyjuk, hogy jovahagyas nelkul tovabbmenjen a felhasznalo
                await messageBox.ShowWindowDialogAsync(_mainWindow);

                // A dialogus utan megnezzuk, ujra kell-e inditani
                _viewModel?.OnMessageDialogClosed(e.ShouldResumeGameAfter, false);
            }
        }
    }
    public void Dispose()
    {
        _viewModel?.Dispose();
    }
}
