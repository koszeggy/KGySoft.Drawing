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

namespace KGySoft.Drawing.Examples.Xamarin.UWP
{
    public sealed partial class MainPage
    {
        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new KGySoft.Drawing.Examples.Xamarin.App());
        }

        #endregion
    }
}