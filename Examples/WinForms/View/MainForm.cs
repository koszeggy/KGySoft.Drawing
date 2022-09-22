using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.WinForms.ViewModel;

namespace KGySoft.Drawing.Examples.WinForms.View
{
    public partial class MainForm : Form
    {
        private readonly MainViewModel viewModel = default!;
        private readonly CommandBindingsCollection commandBindings = new();

        /// <summary>
        /// This constructor is just for the designer
        /// </summary>
        public MainForm() => InitializeComponent();

        internal MainForm(MainViewModel viewModel) : this()
        {
            this.viewModel = viewModel;
            InitPropertyBindings();
            InitCommandBindings();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                commandBindings.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }


        private void InitPropertyBindings()
        {

        }

        private void InitCommandBindings()
        {
            commandBindings.Add(viewModel.UpdateProgressCommand)
                .AddSource(timerProgress, nameof(timerProgress.Tick));
        }
    }
}
