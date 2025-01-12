[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)

# KGy SOFT Drawing SkiaSharp in MAUI Example

This example demonstrates how to obtain an [`IReadWriteBitmapData`](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm) instance for the `SKBitmap` class of any pixel format in a MAUI application using the [`KGySoft.Drawing.SkiaSharp`](https://www.nuget.org/packages/KGySoft.Drawing.SkiaSharp) package and perform various operations on it.

> 💡 _Tip:_ There are similar example applications for [MAUI](../Maui) and [Xamarin](../Xamarin) that also work with SkiaSharp, but they do it without using the dedicated SkiaSharp-specific package. They demonstrate how to manually obtain a bitmap data for a typical 3rd party bitmap implementation. But if you need to handle all possible color types and alpha types that an `SKBitmap` can represent, then this one is the recommended example.

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on Windows 11" src="https://github.com/user-attachments/assets/09fac26a-3b2f-4776-a9d4-7600906e01bf"/>
  <br/><em>The KGy SOFT Drawing SkiaSharp/MAUI Example App running on Windows 11</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on MacOS" src="https://github.com/user-attachments/assets/20f804c0-8968-4d90-aa8e-c949e420d0bd"/>
  <br/><em>The KGy SOFT Drawing SkiaSharp/MAUI Example App running on MacOS</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on Android Phone" src="https://github.com/user-attachments/assets/c989495f-55e2-41f2-b4b3-73a07ac55ff6"/>
  <br/><em>The KGy SOFT Drawing SkiaSharp/MAUI Example App running on Android Phone</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing SkiaSharp/MAUI Example App on iOS" src="https://github.com/user-attachments/assets/c283862c-9870-4979-a390-9e9b7415da1a"/>
  <br/><em>The KGy SOFT Drawing SkiaSharp/MAUI Example App running on iPhone</em>
</p>
