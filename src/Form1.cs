using System;
using System.Windows.Forms;
using kuber3d.Rendering;

namespace kuber3d
{
    public partial class FormMain : Form
    {
        private GLView? _view;

        public FormMain()
        {
            InitializeComponent();

            Text = "Kuber3D";
            Width = 1400;
            Height = 900;

            // 3D не стартуем сразу
            // Стартуем только по кнопке
            btnStart3D.Click += (_, __) => Start3D();
        }

        private void Start3D()
        {
            if (_view != null) return; // уже запущено

            _view = new GLView
            {
                Dock = DockStyle.Fill
            };

            pnlHost.Controls.Clear();
            pnlHost.Controls.Add(_view);

            btnStart3D.Enabled = false; // чтобы не плодить вьюхи
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _view?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
