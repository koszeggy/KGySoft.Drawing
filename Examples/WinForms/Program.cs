#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Program.cs
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

using KGySoft.Drawing.Examples.WinForms.View;
using KGySoft.Drawing.Examples.WinForms.ViewModel;

#endregion

namespace KGySoft.Drawing.Examples.WinForms
{
    internal static class Program
    {
        #region Methods

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            using var viewModel = new MainViewModel();
            Application.Run(new MainForm(viewModel));
        }

        #endregion
    }
}