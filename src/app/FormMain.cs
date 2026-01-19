// FormMain.cs
//
// Главная форма приложения Kuber3D (UI слой).
// FormMain = View (IMainView). Логика живёт в Presenter.

using System;
using System.Windows.Forms;

using kuber3d.Contracts;
using kuber3d.Presenters;

namespace kuber3d
{
    public partial class FormMain : Form, IMainView
    {
        private readonly IMainPresenter _presenter;

        public FormMain()
        {
            InitializeComponent();

            _presenter = new MainPresenter(this);
        }

        // =========================
        // IMainView: события
        // =========================

        public event EventHandler? Start3DClicked;
        public event EventHandler? GridToggled;
        public event EventHandler? AxesToggled;

        // =========================
        // IMainView: состояние
        // =========================

        public bool IsGridEnabled => chkGrid.Checked;
        public bool IsAxesEnabled => chkAxes.Checked;

        public bool IsStart3DEnabled
        {
            get => btnStart3D.Enabled;
            set => btnStart3D.Enabled = value;
        }

        // =========================
        // IMainView: встраивание 3D
        // =========================

        public void AttachViewport(IGLView glView)
        {
            if (glView == null) throw new ArgumentNullException(nameof(glView));

            pnlViewport.Controls.Clear();

            var ctrl = glView.Control;
            ctrl.Dock = DockStyle.Fill;

            pnlViewport.Controls.Add(ctrl);
        }

        // =========================
        // IMainView: сообщения/статус
        // =========================

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text, "Kuber3D", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetStatus(string text)
        {
            // MVP: можно выводить в заголовок окна.
            // Позже заменим на status-strip.
            Text = string.IsNullOrWhiteSpace(text) ? "Kuber3D" : $"Kuber3D — {text}";
        }

        // =========================
        // Жизненный цикл формы
        // =========================

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Пробрасываем события UI наружу (Presenter подписан на IMainView события)
            btnStart3D.Click += (_, __) => Start3DClicked?.Invoke(this, EventArgs.Empty);
            chkGrid.CheckedChanged += (_, __) => GridToggled?.Invoke(this, EventArgs.Empty);
            chkAxes.CheckedChanged += (_, __) => AxesToggled?.Invoke(this, EventArgs.Empty);

            _presenter.Init();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _presenter.Dispose();
            base.OnFormClosed(e);
        }
    }
}
