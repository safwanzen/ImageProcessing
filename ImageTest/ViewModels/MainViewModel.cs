using Avalonia.Media.Imaging;
using SkiaSharp;
using System.IO;
using Avalonia;
using Avalonia.Platform;
using System;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace ImageTest.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        Init();
    }

    private async Task Init()
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri($"avares://ImageTest/Assets/table.png"));
            using var memstream = new MemoryStream();
            await stream.CopyToAsync(memstream);
            memstream.Seek(0, SeekOrigin.Begin);
            bitmap = SKBitmap.Decode(memstream);

            using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            using var mstream = new MemoryStream(data.ToArray());
            bmp = new Bitmap(mstream);

            var ptr = bitmap.GetPixels();

        }
        catch
        {

        }


        //var fs = new StreamReader(stream);
        //var bmp = new Bitmap(fs.BaseStream);
        //Console.WriteLine($"{bmp.PixelSize} {bmp.Size} {bmp.Format}");
    }

    public string Greeting => "Welcome to Avalonia!";
    public SKBitmap bitmap {get; set; }
    [Reactive] public Bitmap bmp { get; set; }
}
