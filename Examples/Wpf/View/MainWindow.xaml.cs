#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainWindow.cs
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
using System.Windows;

using KGySoft.Drawing.Examples.Wpf.ViewModel;

#endregion

namespace KGySoft.Drawing.Examples.Wpf.View
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        #endregion

        #region Methods
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            (DataContext as IDisposable)?.Dispose();
        }

        #endregion
    }
}