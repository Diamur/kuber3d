using System;
using System.Windows.Forms;
using kuber3d.Rendering;

namespace kuber3d
{
    public partial class FormMain : Form
    {
        private GLView? _view;
        private bool _is3D = false;

        public FormMain()
        {
            InitializeComponent();

            Text = "Kuber3D";
            Width = 1400;
            Height = 900;

            btnStart3D.Text = "3D";
            btnStart3D.Click += (_, __) => Toggle3D();
        }

        private void Toggle3D()
        {
            if (!_is3D)
                Start3D();
            else
                Stop3D();
        }

        private void Start3D()
        {
            if (_view != null) return;

            _view = new GLView
            {
                Dock = DockStyle.Fill
            };

            pnlViewport.Controls.Clear();
            pnlViewport.Controls.Add(_view);

            _is3D = true;
            btnStart3D.Text = "Stop";
        }

        private void Stop3D()
        {
            if (_view == null) return;

            pnlViewport.Controls.Remove(_view);
            _view.Dispose();
            _view = null;

            _is3D = false;
            btnStart3D.Text = "3D";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _view?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
