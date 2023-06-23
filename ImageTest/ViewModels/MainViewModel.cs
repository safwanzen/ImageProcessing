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
using System.Runtime.InteropServices;

namespace ImageTest.ViewModels;

public class MainViewModel : ViewModelBase
{
    public Action _invalidate = delegate { };

    public MainViewModel(/*Action invalidate*/)
    {
        //_invalidate = invalidate;
        Init();

        this.WhenAnyValue(x => x.Sliderint)
            .Subscribe(x =>
            {
                RedrawUnsafe();
                //bmp = await Redraw();
                //bmp.Save("test2.png");
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

    private unsafe void RedrawUnsafe()
    {
        /*
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

        nint pixels = _bmp.GetPixels();

        var data = new int[fb.Size.Width * fb.Size.Height];
        */

        using var fb = wBitmap.Lock();

        // pixel is in BGRA8888 format
        //var ptr = (int*)pixels.ToPointer();
        var ptr = (int*)fb.Address;
        var dispptr = (int*)displaybitmap.GetPixels().ToPointer();
        var bptr = (byte*)dispptr;
        var w = fb.Size.Width;
        var h = fb.Size.Height;
        var count = w * h;
        var data = new byte[count * 4];

        //Marshal.Copy(displaybitmap.Bytes, 0, fb.Address, count);

        /*
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                //var color = new Color(255, 0, Sliderint, 0);
                ////var color = new Color(fillAlpha, 0, 255, 0);

                //if (premul)
                //{
                //    byte r = (byte)(color.R * color.A / 255);
                //    byte g = (byte)(color.G * color.A / 255);
                //    byte b = (byte)(color.B * color.A / 255);

                //    color = new Color(fillAlpha, r, g, b);
                //}

                //data[y * fb.Size.Width + x] = byte.MaxValue | byte.MaxValue << 8 | byte.MaxValue << 16 | Sliderint << 24;
                *(ptr + y * fb.Size.Width + x) = 255 << 24;
            }

        }*/

        // threshold
        /**/
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // 0.21f, 0.72f, 0.07f
                //byte b = (byte)(*(alphaptr - 3) * 0.07f);
                //byte g = (byte)(*(alphaptr - 2) * 0.72f);
                //byte r = (byte)(*(alphaptr - 1) * 0.21f);
                //byte grey = (byte)(b + g + r);
                int i = y * w + x;
                //byte g = *(bptr + i * 4);
                byte g = *(byte*)(dispptr + i);
                byte v = g > Sliderint ? g : (byte)0;
                //if (g < Sliderint) continue;
                //v = g;
                //if (g > Sliderint + 20) v = byte.MaxValue;
                //else if (g < Sliderint - 20) v = 0;
                //else v = 100;

                //*(ptr + i) = v | v << 8 | v << 16 | byte.MaxValue << 24;
                int idx = (y * w + x) * 4;
                data[idx] = v;
                data[idx + 1] = v;
                data[idx + 2] = v;
                data[idx + 3] = byte.MaxValue;
                //*(alphaptr + i * 4) = g > Sliderint ? byte.MaxValue : (byte)0;
            }


        byte Get(int* ptr, int x, int y, int width)
        {
            var end = ptr + count;
            var p = (byte*)(ptr + x + y * width);
            if (p < ptr || p > end) return 0;
            return *p;
        }

        //var data = new int[fb.Size.Width * fb.Size.Height];

        // median
        /*
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                List<byte> glist = new();
                //int sum = GetFirstByte(ptr, x - 1, y - 1, w) +
                //    GetFirstByte(ptr, x - 0, y - 1, w) +
                //    GetFirstByte(ptr, x + 1, y - 1, w) +
                //    GetFirstByte(ptr, x - 1, y - 0, w) +
                //    GetFirstByte(ptr, x - 0, y - 0, w) +
                //    GetFirstByte(ptr, x + 1, y - 0, w) +
                //    GetFirstByte(ptr, x - 1, y + 1, w) +
                //    GetFirstByte(ptr, x - 0, y + 1, w) +
                //    GetFirstByte(ptr, x + 1, y + 1, w);
                //sum /= 9;

                float sum =
                    GetFirstByte(ptr, x - 0, y - 1, w) * 0.125f +
                    GetFirstByte(ptr, x - 1, y - 0, w) * 0.125f +
                    GetFirstByte(ptr, x - 0, y - 0, w) * 0.5f +
                    GetFirstByte(ptr, x + 1, y - 0, w) * 0.125f +
                    GetFirstByte(ptr, x - 0, y + 1, w) * 0.125f;

                var mid = (byte)sum;
                //glist.Sort();
                //var mid = glist[4];
                data[y * w + x] = mid | mid << 8 | mid << 16 | byte.MaxValue << 24;
            }*/

        /*
        #region sobel edge detect

        int[] sobelHkernel = new int[]
        {
            -1, 0, 1,
            -2, 0, 2,
            -1, 0, 1
        };

        int[] sobelVkernel = new int[]
        {
            -1,  -2,  -1,
            0,  0,  0,
            1, 2, 1
        };

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int sobelxsum = 0;
                int sobelysum = 0;

                for (int j = -1; j < 2; j++)
                    for (int i = -1; i < 2; i++)
                    {
                        var val = Get(dispptr, x + i, y + j, w);
                        var valx = val * sobelHkernel[(j + 1) * 3 + i + 1];
                        var valy = val * sobelVkernel[(j + 1) * 3 + i + 1];
                        sobelxsum += valx;
                        sobelysum += valy;
                    }

                // clip value to 0 ~ 255
                sobelxsum = sobelxsum < 0 ? 0 : sobelxsum;
                sobelxsum = sobelxsum > 255 ? 255 : sobelxsum;

                sobelysum = sobelysum < 0 ? 0 : sobelysum;
                sobelysum = sobelysum > 255 ? 255 : sobelysum;

                var mid = (byte)((sobelxsum + sobelysum) / 2);
                //glist.Sort();
                //var mid = glist[4];
                //data[y * w + x] = mid | mid << 8 | mid << 16 | byte.MaxValue << 24;
                int idx = (y * w + x) * 4;
                data[idx] = mid;
                data[idx + 1] = mid;
                data[idx + 2] = mid;
                data[idx + 3] = byte.MaxValue;
                //*(ptr + y * w + x) = mid | mid << 8 | mid << 16 | byte.MaxValue << 24;
            }
        #endregion sobel edge detect*/


        /*
        #region sharpen / blur

        int[] sharpkernel = new int[]
        {
            -1, -1, -1,
            -1, Sliderint, -1,
            -1, -1, -1,
        };

        int[] blurkernel = new int[]
        {
            1,  4,  6,  4,  1,
            4, 16, 24, 16,  4,
            6, 24, 36, 24,  6,
            4, 16, 24, 16,  4,
            1,  4,  6,  4,  1,
        };

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float sum = 0;

                for (int j = -2; j < 3; j++)
                    for (int i = -2; i < 3; i++)
                    {
                        //sum += Get(dispptr, x + i, y + j, w) * sharpkernel[(j + 1) * 3 + i + 1];
                        sum += blurkernel[(j + 2) * 5 + i + 2] * Get(dispptr, x + i, y + j, w); 
                    }

                sum /= 256;
                sum = sum > byte.MaxValue ? byte.MaxValue : sum;
                
                var idx = (y * w + x) * 4;
                data[idx] = (byte)sum;
                data[idx + 1] = (byte)sum;
                data[idx + 2] = (byte)sum;
                data[idx + 3] = byte.MaxValue;
            }

        #endregion sharpen
*/
        Marshal.Copy(data, 0, fb.Address, fb.Size.Width * fb.Size.Height * 4);
        //wBitmap.CopyPixels(new PixelRect(0, 0, _bmp.Width, _bmp.Height), pixels, _bmp.Width*_bmp.Height, _bmp.RowBytes);

        _invalidate();
    }

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

        // make bitmap black and white
        /**/
        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < w; col++)
            {
                _bmp.GetPixel(col, row).ToHsv(out var hue, out var sat, out float v);
                //var gv = px.Red * 0.21f + px.Green * 0.72f + px.Blue * 0.07f;
                int val = v > Sliderval ? 100 : 0;
                //int val;
                //if (v > Sliderval + 20) val = 100;
                //else if (v < Sliderval - 20) val = 0;
                //else val = 50;
                _bmp.SetPixel(col, row, SKColor.FromHsv(hue, sat, val));
            }
        }

        displaybitmap = _bmp.Copy();
        // blur
        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < w; col++)
            {
                float gv = 100;
                var xcoord = col;
                var ycoord = row;

                float sum = 0;

                for (int yy = -1; yy <= 1; yy++)
                    for (int xx = -1; xx <= 1; xx++)
                    {
                        var _x = col + xx;
                        var _y = row + yy;
                        if (_x >= 0 && _y >= 0 && _x < displaybitmap.Width && _y < displaybitmap.Height)
                        {
                            displaybitmap.GetPixel(_x, _y).ToHsv(out float _, out float _, out float v);
                            gv = v;
                        }

                        sum += gv;// * kernel[(yy + 1) * 3 + xx + 1];
                    }

                sum /= 9;
                displaybitmap.GetPixel(col, row).ToHsv(out float hue, out float sat, out float _);
                _bmp.SetPixel(col, row, SKColor.FromHsv(hue, sat, sum));
            }
        }

        displaybitmap = _bmp.Copy();
        // median filter
        /**/
        int level = 1;
        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < w; col++)
            {
                List<float> values = new();

                float gv = 100;
                var xcoord = col;
                var ycoord = row;

                for (int yy = -level; yy <= level; yy++)
                    for (int xx = -level; xx <= level; xx++)
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

            wBitmap = new WriteableBitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, displaybitmap.GetPixels(), new PixelSize(sourcebitmap.Width, sourcebitmap.Height), new Vector(96, 96), sourcebitmap.RowBytes);
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
    [Reactive] public float Sliderval { get; set; } = .5f;
    [Reactive] public byte Sliderint { get; set; } = 10;
}
