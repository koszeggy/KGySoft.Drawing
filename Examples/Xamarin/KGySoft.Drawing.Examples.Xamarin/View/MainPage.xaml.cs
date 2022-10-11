#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainPage.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.Xamarin.ViewModel;

using SkiaSharp.Views.Forms;
using SkiaSharp;

using Xamarin.Forms;

#endregion

namespace KGySoft.Drawing.Examples.Xamarin.View
{
    public partial class MainPage : ContentPage
    {
        #region Properties

        private MainViewModel? ViewModel => BindingContext as MainViewModel;

        #endregion

        #region Constructors

        public MainPage()
        {
            InitializeComponent();
            sliderPaletteSize.Minimum = 2;
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override void OnAppearing()
        {
            BindingContext = new MainViewModel();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            (BindingContext as IDisposable)?.Dispose();
        }

        #endregion

        #endregion
    }
}