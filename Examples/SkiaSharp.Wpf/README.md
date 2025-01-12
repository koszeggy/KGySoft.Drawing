[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)

# KGy SOFT Drawing SkiaSharp in WPF Example

This example demonstrates how to obtain an [`IReadWriteBitmapData`](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm) instance for the `SKBitmap` class of any pixel format in a WPF application using the [`KGySoft.Drawing.SkiaSharp`](https://www.nuget.org/packages/KGySoft.Drawing.SkiaSharp) package and perform various operations on it.

> 💡 _Tip:_ There are similar example applications for [MAUI](../Maui) and [Xamarin](../Xamarin) that also work with SkiaSharp, but they do it without using the dedicated SkiaSharp-specific package. They demonstrate how to manually obtain a bitmap data for a typical 3rd party bitmap implementation. But if you need to handle all possible color types and alpha types that an `SKBitmap` can represent, then this one is the recommended example.

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/WPF Example App on Windows 11" src="https://github.com/user-attachments/assets/c517cb34-014d-4e52-8a51-7a7bdd2e2009"/>
  <br/><em>The KGy SOFT Drawing SkiaSharp/WPF Example App running on Windows 11</em>
</p>
