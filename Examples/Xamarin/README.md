[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)

# KGy SOFT Drawing Xamarin Example

This example demonstrates how to obtain an [`IReadWriteBitmapData`](https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm) instance for the `SKBitmap` class in a Xamarin application and perform various operations on it.

> ℹ️ _Note:_ This example references only the technology-agnostic [`KGySoft.Drawing.Core`](https://www.nuget.org/packages/KGySoft.Drawing.Core) package so it also demonstrates how to obtain a bitmap data for a 3rd party bitmap implementation without dedicated support. See the SkiaSharp specific solutions for [MAUI](../SkiaSharp.Maui) or [WPF](../SkiaSharp.Wpf) for examples that support the wide range of possible pixel formats of the `SKBitmap` type.

> ⚠️ _Warning:_ Visual Studio 2026 does not support the Android and iOS projects of this solution anymore, and opens the Windows project only. You need Visual Studio 2022 or Visual Studio for Mac 2022 to be able to debug the Android and iOS projects. If you use Visual Studio Code, check out the [MAUI](../Maui) example instead.

## Screenshots

<p align="center">
  <img alt="KGy SOFT Drawing Xamarin Example App on Windows 11" src="https://github.com/user-attachments/assets/7b7ec8ed-339c-4903-8640-2d6b98a24c4a"/>
  <br/><em>The KGy SOFT Drawing Xamarin Example App running on Windows 11</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing Xamarin Example App on Android Phone" src="https://github.com/user-attachments/assets/9736c4cd-c1c7-4598-9b18-f64713aa01ee"/>
  <br/><em>The KGy SOFT Drawing Xamarin Example App running on Android Phone</em>
</p>

<p align="center">
  <img alt="KGy SOFT Drawing Xamarin Example App on iOS" src="https://github.com/koszeggy/KGySoft.Drawing/assets/27336165/d5f3546f-f43f-410d-9a09-40ee303368c5"/>
  <br/><em>The KGy SOFT Drawing Xamarin Example App running on iPhone</em>
</p>
