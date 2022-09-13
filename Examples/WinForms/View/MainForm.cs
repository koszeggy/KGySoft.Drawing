using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KGySoft.Drawing.Examples.WinForms.ViewModel;

namespace KGySoft.Drawing.Examples.WinForms.View
{
    public partial class MainForm : Form
    {
        private MainViewModel ViewModel { get; } = default!;

        /// <summary>
        /// This constructor is just for the designer
        /// </summary>
        public MainForm() => InitializeComponent();

        internal MainForm(MainViewModel viewModel) : this()
        {
            ViewModel = viewModel;
        }
    }
}
