[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net/drawing)

[![Website](https://img.shields.io/website/https/kgysoft.net/drawing.svg)](https://kgysoft.net/drawing) [![Online Help](https://img.shields.io/website/https/docs.kgysoft.net/drawing.svg?label=online%20help&up_message=available)](https://docs.kgysoft.net/drawing) [![GitHub Repo](https://img.shields.io/github/repo-size/koszeggy/KGySoft.Drawing.svg?label=github)](https://github.com/koszeggy/KGySoft.Drawing) [![Drawing Tools](https://img.shields.io/github/repo-size/koszeggy/KGySoft.Drawing.Tools.svg?label=Drawing%20Tools)](https://github.com/koszeggy/KGySoft.Drawing.Tools)

KGy SOFT Drawing Libraries offer advanced features for `System.Drawing` types such as `Bitmap`, `Metafile`, `Image`, `Icon`, `Graphics`.

> ⚠️ _Warning_: Version 7.0.0 introduced several breaking changes. Most importantly, the technology-agnostic and platform independent APIs have been extracted into a separated package: [KGySoft.Drawing.Core](https://www.nuget.org/packages/KGySoft.Drawing.Core/).

> 📝 _Note_: In .NET 7 and above this package can be used on Windows only. When targeting earlier versions, Unix/Linux based systems are also supported (if the libgdiplus library is installed).

Main highlights:
- Fast [direct native Bitmap data access](https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm) for every PixelFormat
- [Quantizing](https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Quantize.htm) and [dithering](https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Dither.htm)
- Creating [GIF animations](https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_SaveAsAnimatedGif.htm) even in high color
- Several [built-in icons](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Icons.htm) as well as simple access to Windows associated and stock icons.
- Extracting bitmaps from multi-frame bitmaps and icons.
- Creating combined icons and multi-resolution bitmaps.
- Saving metafiles in EMF/WMF formats.
- Advanced support for saving images as Icon, BMP, JPEG, PNG, GIF and TIFF formats.
- [Converting between various pixel formats](https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm) preserving transparency if possible.
- Useful extensions for the [Icon](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_IconExtensions.htm), [Bitmap](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_BitmapExtensions.htm), [Image](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_ImageExtensions.htm), [Metafile](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_MetafileExtensions.htm) and [Graphics](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_GraphicsExtensions.htm) types.

See the [online help](https://docs.kgysoft.net/drawing) for the complete documentation or the [project site](https://kgysoft.net/drawing) for some highlighted code examples.

> 💡 _Tip_:
> * Feel free to explore the [application examples](https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples) to see how to use KGy SOFT Drawing Libraries in various environments such as [MAUI](https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/Maui), [UWP](https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/Uwp), [WinForms](https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/WinForms), [WinUI](https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/WinUI), [WPF](https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/Wpf) and [Xamarin](https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/Xamarin).
> * For technology-agnostic solutions that can be used on any platform see the [KGySoft.Drawing.Core](https://www.nuget.org/packages/KGySoft.Drawing.Core/) package.
> * For WPF specific solutions see the [KGySoft.Drawing.Wpf](https://www.nuget.org/packages/KGySoft.Drawing.Wpf/) package. It makes possible to obtain a managed, fast accessible bitmap data for a `WriteableBitmap` instance of any pixel format, offering all features of an [`IReadWriteBitmapData`](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm) for a `WriteableBitmap`.
> * For dedicated UWP support see the [KGySoft.Drawing.Uwp](https://www.nuget.org/packages/KGySoft.Drawing.Uwp/) package.
> * For dedicated WinUI support see the [KGySoft.Drawing.WinUI](https://www.nuget.org/packages/KGySoft.Drawing.WinUI/) package.
> * For SkiaSharp specific solutions see the [KGySoft.Drawing.SkiaSharp](https://www.nuget.org/packages/KGySoft.Drawing.SkiaSharp/) package. It makes possible to obtain managed, fast accessible bitmap data for `SKBitmap` and `SKPixmap` instances of any pixel format, offering all [`IReadWriteBitmapData`](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm) features for them. It also offers extension methods for the `SKImage`, `SKSurface`, `SKBitmap`, `SKPixmap` and `SKImageInfo` classes.
> * See also the [KGySoft.Drawing.Tools](https://github.com/koszeggy/KGySoft.Drawing.Tools) repository, which contains debugger visualizers built on the KGy SOFT Drawing Libraries as well as a test project, which demonstrates its features.