[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net/drawing)

[![Website](https://img.shields.io/website/https/kgysoft.net/drawing.svg)](https://kgysoft.net/drawing) [![Online Help](https://img.shields.io/website/https/docs.kgysoft.net/drawing.svg?label=online%20help&up_message=available)](https://docs.kgysoft.net/drawing) [![GitHub Repo](https://img.shields.io/github/repo-size/koszeggy/KGySoft.Drawing.svg?label=github)](https://github.com/koszeggy/KGySoft.Drawing) [![Drawing Tools](https://img.shields.io/github/repo-size/koszeggy/KGySoft.Drawing.Tools.svg?label=Drawing%20Tools)](https://github.com/koszeggy/KGySoft.Drawing.Tools)

KGy SOFT Drawing Libraries offer advanced features for `System.Drawing` types such as `Bitmap`, `Metafile`, `Image`, `Icon`, `Graphics`.

> ⚠️ _Warning_: Version 7.0.0 introduces several breaking changes. Most importantly, the technology-agnostic and platform independent APIs have been extracted into a separated package: [KGySoft.Drawing.Core](https://www.nuget.org/packages/KGySoft.Drawing.Core/).

> 📝 _Note_: In .NET 7 and above this package can be used on Windows only. When targeting earlier versions, Unix/Linux based systems are also supported (if the libgdiplus library is installed).

Main highlights:
- Fast [direct native Bitmap data access](https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm) for every PixelFormat
- [Quantizing](https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_Quantize.htm) and [dithering](https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_Dither.htm)
- Creating [GIF animations](https://docs.kgysoft.net/drawing/?topic=html/Overload_KGySoft_Drawing_ImageExtensions_SaveAsAnimatedGif.htm) even in high color
- Several [built-in icons](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Icons.htm) as well as simple access to Windows associated and stock icons.
- Extracting bitmaps from multi-frame bitmaps and icons.
- Creating combined icons and multi-resolution bitmaps.
- Saving metafiles in EMF/WMF formats.
- Advanced support for saving images as Icon, BMP, JPEG, PNG, GIF and TIFF formats.
- [Converting between various pixel formats](https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm) preserving transparency if possible.
- Useful extensions for the [Icon](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_IconExtensions.htm), [Bitmap](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_BitmapExtensions.htm), [Image](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_ImageExtensions.htm), [Metafile](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_MetafileExtensions.htm) and [Graphics](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_GraphicsExtensions.htm) types.

See the [online help](https://docs.kgysoft.net/drawing) for the complete documentation or the [project site](https://kgysoft.net/drawing) for some highlighted code examples.

> 💡 _Tip_:
> * For technology-agnostic solutions that can be used on any platform see the [KGySoft.Drawing.Core](https://www.nuget.org/packages/KGySoft.Drawing.Core/) package.
> * For WPF specific solutions see the [KGySoft.Drawing.Wpf](https://www.nuget.org/packages/KGySoft.Drawing.Wpf/) package. It makes possible to create managed, fast accessible bitmap data for the WriteableBitmap class, supporting all pixel formats.
> * See also the [KGySoft.Drawing.Tools](https://github.com/koszeggy/KGySoft.Drawing.Tools) repository, which contains debugger visualizers built on the KGy SOFT Drawing Libraries as well as a test project, which demonstrates its features.