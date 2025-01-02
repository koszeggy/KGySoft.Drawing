#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: App.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using KGySoft.Drawing.Examples.Xamarin.View;

using Xamarin.Forms;

#endregion

namespace KGySoft.Drawing.Examples.Xamarin
{
    public partial class App : Application
    {
        #region Constructors

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        #endregion

        #region Methods

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        #endregion
    }
}