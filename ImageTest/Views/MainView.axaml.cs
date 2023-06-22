using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using SkiaSharp;
using System.Diagnostics;
using System;
using SkiaSharp;
using Avalonia.Media;
using ImageTest.ViewModels;
using Avalonia.Threading;
using Avalonia.Media.Imaging;

namespace ImageTest.Views;

public partial class MainView : UserControl
{
    Image img;
    MainViewModel viewModel;

    WriteableBitmap _wbitmap;

    public MainView()
    {
        InitializeComponent();

        _wbitmap = new WriteableBitmap(new PixelSize(256, 256), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Unpremul);

        viewModel = new MainViewModel();
        DataContext = viewModel;
        
        img = this.FindControl<Image>("image");

        var action = () => Dispatcher.UIThread.InvokeAsync(() => img?.InvalidateVisual()).Wait();

        viewModel._invalidate = action;
    }

    //public override void Render(DrawingContext context)
    //{
    //    Console.WriteLine("rendering");
    //    base.Render(context);
    //}
}
