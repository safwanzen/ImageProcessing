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

namespace ImageTest.Views;

public partial class MainView : UserControl
{
    Image img;
    MainViewModel viewModel;

    private Action invalidate = delegate { };

    public MainView()
    {
        InitializeComponent();

        img = this.FindControl<Image>("image");

        //invalidate = () => Dispatcher.UIThread.InvokeAsync(img.InvalidateVisual);
        //viewModel = new MainViewModel(invalidate);
        //DataContext = viewModel;
    }
}
