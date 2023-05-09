[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)

# KGy SOFT Drawing SkiaSharp in MAUI Example

This example demonstrates how to obtain an [`IReadWriteBitmapData`](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm) instance for the `SKBitmap` class of any pixel format in a MAUI application using the [`KGySoft.Drawing.SkiaSharp`](https://www.nuget.org/packages/KGySoft.Drawing.SkiaSharp) package and perform various operations on it.

> 💡 _Tip:_ There are similar example applications for [MAUI](../Maui) and [Xamarin](../Xamarin) that also work with SkiaSharp, but they do it without using the dedicated SkiaSharp-specific package. They demonstrate how to manually obtain a bitmap data for a typical 3rd party bitmap implementation. But if you need to handle all possible color types and alpha types that an `SKBitmap` can represent, then this one is the recommended example.

## Screenshots

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on Windows 11" src="https://user-images.githubusercontent.com/27336165/237056772-3cf78d62-3487-4af0-9cbd-46ac9df26ded.png"/>
  <br/><em>Windows 11</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on MacOS" src="https://user-images.githubusercontent.com/27336165/237056993-601c21a1-6609-47e4-80f0-d538f73a6499.png"/>
  <br/><em>MacOS</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on Android Phone" src="https://user-images.githubusercontent.com/27336165/237057266-49158fa3-090e-488d-9e20-4b96d28462bf.png"/>
  <br/><em>Android Pixel 5 Phone</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on iOS" src="https://github.com/koszeggy/KGySoft.Drawing/assets/27336165/2d7931e1-cd3f-46f6-8c3b-0fe8b6cc2b70"/>
  <br/><em>iPhone 14 with iOS 16.2</em>
</p>
