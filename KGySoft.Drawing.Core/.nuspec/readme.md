[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net/drawing)

[![Website](https://img.shields.io/website/https/kgysoft.net/drawing.svg)](https://kgysoft.net/drawing) [![Online Help](https://img.shields.io/website/https/docs.kgysoft.net/drawing.svg?label=online%20help&up_message=available)](https://docs.kgysoft.net/drawing) [![GitHub Repo](https://img.shields.io/github/repo-size/koszeggy/KGySoft.Drawing.svg?label=github)](https://github.com/koszeggy/KGySoft.Drawing) [![Drawing Tools](https://img.shields.io/github/repo-size/koszeggy/KGySoft.Drawing.Tools.svg?label=Drawing%20Tools)](https://github.com/koszeggy/KGySoft.Drawing.Tools)

KGy SOFT Drawing Libraries offer advanced drawing features both for completely managed bitmap data as well as native System.Drawing types on multiple platforms.

Among others:
- Creating [managed bitmap data](http://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_BitmapDataFactory.htm) of any pixel format on every platform, including Linux and MacOS
- Quantizing using [predefined](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_PredefinedColorsQuantizer.htm) or [optimized](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_OptimizedPaletteQuantizer.htm) colors
- Dithering using [ordered](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_OrderedDitherer.htm), [error diffusion](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_ErrorDiffusionDitherer.htm), [random noise](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_RandomNoiseDitherer.htm) or [interleaved gradient noise](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_InterleavedGradientNoiseDitherer.htm) dithering techniques
- Creating [GIF animations](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_Imaging_GifEncoder.htm) even in high color

See the [online help](https://docs.kgysoft.net/drawing) for the complete documentation or the [project site](https://kgysoft.net/drawing) for some highlighted code examples.

> 💡 _Tip:_
> * For GDI+ specific solutions see the [KGySoft.Drawing](https://www.nuget.org/packages/KGySoft.Drawing/) package. It makes possible to create managed, fast accessible bitmap data for the `Bitmap` class, supporting all pixel formats. It also contains many extensions for the [Icon](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_IconExtensions.htm), [Bitmap](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_BitmapExtensions.htm), [Image](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_ImageExtensions.htm), [Metafile](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_MetafileExtensions.htm) and [Graphics](https://docs.kgysoft.net/drawing/?topic=html/T_KGySoft_Drawing_GraphicsExtensions.htm) types.
> * For WPF specific solutions see the [KGySoft.Drawing.Wpf](https://www.nuget.org/packages/KGySoft.Drawing.Wpf/) package. It makes possible to create managed, fast accessible bitmap data for the `WriteableBitmap` class, supporting all pixel formats.