using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace YogiBearAvalonia.ViewModel;

/// <summary>
/// Nézetmodell ősosztály típusa.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    
    /// <summary>
    /// Nézetmodell ősosztály példányosítása.
    /// </summary>
    protected ViewModelBase() { }

    //erre a fuggvenyre azert van szukseg, mert a kepek a propertykben byte[]-kent tarolodnak
    protected Bitmap? LoadImageFromAssets(string path)
    {
        var uri = new Uri($"avares://YogiBearAvalonia/Assets/{path}");
        return new Bitmap(AssetLoader.Open(uri));
    }
}
