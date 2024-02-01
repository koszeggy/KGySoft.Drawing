#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainPage.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

using KGySoft.Drawing.Examples.Uwp.ViewModel;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#endregion

namespace KGySoft.Drawing.Examples.Uwp.View
{
    public sealed partial class MainPage : Page
    {
        #region Constructors

        public MainPage() => InitializeComponent();

        #endregion

        #region Methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = new MainViewModel();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            (DataContext as IDisposable)?.Dispose();
        }

        #endregion
    }
}