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
            var pixels = bitmap.GetPixels();
            int w = bitmap.Width;
            int h = bitmap.Height;

            int sy = 500;
            int sx = 100;
            int x = 70;
            int y = 100;

            var filter = SKColorFilter.CreateColorMatrix( new float[]
            {
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0,     0,     0,     1, 0
            });

            using var paint = new SKPaint();
            paint.ColorFilter = filter;

            var canvas = new SKCanvas(bitmap);
            canvas.DrawBitmap(bitmap, new SKPoint(0, 0), paint); // make it grayscale

            // make bitmap black and white
            for (int row = 0; row < h; row++)
            {
                for (int col = 0; col < w; col++)
                {
                    bitmap.GetPixel(col, row).ToHsv(out var hue, out var sat, out float v);
                    int val = v > 60f ? 100 : 0;
                    bitmap.SetPixel(col, row, SKColor.FromHsv(hue, sat, val));
                }
            }

            //unsafe
            //{
            //    uint* ptr = (uint*)pixels.ToPointer(); // BGRA8888 - 32 bits - Int32

            //    // draw a 50x50 black square on the top left of the bitmap
            //    ptr += (x * w) + y;

            //    for (int row = 0; row < sy; row++)
            //    {
            //        for (int col = 0; col < sx; col++)
            //        {
            //            *ptr = 0xffff6f00;
            //            ptr++;
            //        }
            //        ptr += w - sx;
            //    }
            //}

            using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            using var mstream = new MemoryStream(data.ToArray());
            bmp = new Bitmap(mstream);
        }
        catch
        {

        }


        //var fs = new StreamReader(stream);
        //var bmp = new Bitmap(fs.BaseStream);
        //Console.WriteLine($"{bmp.PixelSize} {bmp.Size} {bmp.Format}");
    }

    public string Greeting => "Welcome to Avalonia!";
    public SKBitmap bitmap { get; set; }
    public RenderTargetBitmap renderBitmap { get; set; }
    [Reactive] public Bitmap bmp { get; set; }
    [Reactive] public float Sliderval { get; set; } = 0;
}
