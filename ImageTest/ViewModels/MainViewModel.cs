using Avalonia.Media.Imaging;
using SkiaSharp;
using System.IO;
using Avalonia;
using Avalonia.Platform;
using System;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using Avalonia.Media;
using Avalonia.Skia.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ImageTest.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Action _invalidate;

    public MainViewModel(/*Action invalidate*/)
    {
        //_invalidate = invalidate;
        Init();

        this.WhenAnyValue(x => x.Sliderval)
            .Subscribe(async x =>
            {
                bmp = await Redraw();
                //unsafe
                //{
                //    var buffer = wBitmap.Lock();
                //}
            });
    }

    private float[] kernel = new float[]
    {
        0     ,   0.125f,    0     ,
        0.125f,   0.5f  ,    0.125f,
        0     ,   0.125f,    0     ,
    };

    private async Task<Bitmap> Redraw()
    {
        int w = sourcebitmap.Width;
        int h = sourcebitmap.Height;
        int sy = 500;
        int sx = 100;
        int x = 70;
        int y = 100;

        var filter = SKColorFilter.CreateColorMatrix(new float[]
        {
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0,     0,     0,     1, 0
        });

        using var paint = new SKPaint();
        paint.ColorFilter = filter;

        var canvas = new SKCanvas(displaybitmap);
        canvas.DrawBitmap(sourcebitmap, new SKPoint(0, 0), paint); // make it grayscale

        using SKBitmap _bmp = displaybitmap.Copy();

        // median filter
        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < w; col++)
            {
                List<float> values = new();

                float gv = 100;
                var xcoord = col;
                var ycoord = row;

                for (int yy = -3; yy <= 3; yy++)
                    for (int xx = -3; xx <= 3; xx++)
                    {
                        var _x = col + xx;
                        var _y = row + yy;
                        if (_x >= 0 && _y >= 0 && _x < displaybitmap.Width && _y < displaybitmap.Height)
                        {
                            displaybitmap.GetPixel(_x, _y).ToHsv(out float _, out float _, out float v);
                            gv = v;
                        }

                        values.Add(gv);
                    }

                values.Sort();
                displaybitmap.GetPixel(col, row).ToHsv(out float hue, out float sat, out float _);
                _bmp.SetPixel(col, row, SKColor.FromHsv(hue, sat, values[values.Count / 2]));
            }
        }

        // make bitmap black and white
        /*
        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < w; col++)
            {
                _bmp.GetPixel(col, row).ToHsv(out var hue, out var sat, out float v);
                //var gv = px.Red * 0.21f + px.Green * 0.72f + px.Blue * 0.07f;
                //int val = v > Sliderval ? 100 : 0;
                int val;
                if (v > Sliderval + 20) val = 100;
                else if (v < Sliderval - 20) val = 0;
                else val = 50;
                _bmp.SetPixel(col, row, SKColor.FromHsv(hue, sat, val));
            }
        }*/

        // blur
        //for (int row = 0; row < h; row++)
        //{
        //    for (int col = 0; col < w; col++)
        //    {
        //        float gv = 100;
        //        var xcoord = col;
        //        var ycoord = row;

        //        float sum = 0;

        //        for (int yy = -1; yy <= 1; yy++)
        //            for (int xx = -1; xx <= 1; xx++)
        //            {
        //                var _x = col + xx;
        //                var _y = row + yy;
        //                if (_x >= 0 && _y >= 0 && _x < displaybitmap.Width && _y < displaybitmap.Height)
        //                {
        //                    displaybitmap.GetPixel(_x, _y).ToHsv(out float _, out float _, out float v);
        //                    gv = v;
        //                }

        //                sum += gv;// * kernel[(yy + 1) * 3 + xx + 1];
        //            }

        //        sum /= 9;
        //        displaybitmap.GetPixel(col, row).ToHsv(out float hue, out float sat, out float _);
        //        _bmp.SetPixel(col, row, SKColor.FromHsv(hue, sat, sum));
        //    }
        //}

        //unsafe
        //{
        //    uint* ptr = (uint*)displaybitmap.GetPixels().ToPointer(); // BGRA8888 - 32 bits - Int32

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

        //wBitmap.CopyPixels(new PixelRect(0, 0, displaybitmap.Width, displaybitmap.Height), displaybitmap.GetPixels(), displaybitmap.ByteCount, displaybitmap.RowBytes);
        //_invalidate();

        using var data = _bmp.Encode(SKEncodedImageFormat.Png, 100);
        using var mstream = new MemoryStream(data.ToArray());
        return new Bitmap(mstream);
    }

    private void Init()
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri($"avares://ImageTest/Assets/table.png"));
            using var memstream = new MemoryStream();
            stream.CopyTo(memstream);
            memstream.Seek(0, SeekOrigin.Begin);
            sourcebitmap = SKBitmap.Decode(memstream);
            var pixels = sourcebitmap.GetPixels();
            displaybitmap = sourcebitmap.Copy();

            //using var data = sourcebitmap.Encode(SKEncodedImageFormat.Png, 100);
            //using var mstream = new MemoryStream(data.ToArray());
            //bmp = new Bitmap(mstream);
            //renderBitmap = new RenderTargetBitmap(bmp.PixelSize);

            unsafe
            {
                wBitmap = new WriteableBitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, pixels, new PixelSize(sourcebitmap.Width, sourcebitmap.Height), new Vector(96, 96), sourcebitmap.RowBytes);
            }
        }
        catch
        {

        }
    }

    public string Greeting => "Welcome to Avalonia!";
    public SKBitmap sourcebitmap { get; set; }
    public SKBitmap displaybitmap { get; set; }
    public RenderTargetBitmap renderBitmap { get; set; }
    [Reactive] public WriteableBitmap wBitmap { get; set; }
    [Reactive] public Bitmap bmp { get; set; }
    [Reactive] public float Sliderval { get; set; } = 50;
}
