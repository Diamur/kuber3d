// FormMain.cs
//
// Р“Р»Р°РІРЅР°СЏ С„РѕСЂРјР° РїСЂРёР»РѕР¶РµРЅРёСЏ Kuber3D (UI СЃР»РѕР№).
// FormMain = View (IMainView). Р›РѕРіРёРєР° Р¶РёРІС‘С‚ РІ Presenter.

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
        // IMainView: СЃРѕР±С‹С‚РёСЏ
        // =========================

        public event EventHandler? Start3DClicked;
        public event EventHandler? GridToggled;
        public event EventHandler? AxesToggled;

        // =========================
        // IMainView: СЃРѕСЃС‚РѕСЏРЅРёРµ
        // =========================

        public bool IsGridEnabled => chkGrid.Checked;
        public bool IsAxesEnabled => chkAxes.Checked;

        public bool IsStart3DEnabled
        {
            get => btnStart3D.Enabled;
            set => btnStart3D.Enabled = value;
        }

        // =========================
        // IMainView: РІСЃС‚СЂР°РёРІР°РЅРёРµ 3D
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
        // IMainView: СЃРѕРѕР±С‰РµРЅРёСЏ/СЃС‚Р°С‚СѓСЃ
        // =========================

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text, "Kuber3D", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetStatus(string text)
        {
            // MVP: РјРѕР¶РЅРѕ РІС‹РІРѕРґРёС‚СЊ РІ Р·Р°РіРѕР»РѕРІРѕРє РѕРєРЅР°.
            // РџРѕР·Р¶Рµ Р·Р°РјРµРЅРёРј РЅР° status-strip.
            Text = string.IsNullOrWhiteSpace(text) ? "Kuber3D" : $"Kuber3D вЂ” {text}";
        }

        // =========================
        // Р–РёР·РЅРµРЅРЅС‹Р№ С†РёРєР» С„РѕСЂРјС‹
        // =========================

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // РџСЂРѕР±СЂР°СЃС‹РІР°РµРј СЃРѕР±С‹С‚РёСЏ UI РЅР°СЂСѓР¶Сѓ (Presenter РїРѕРґРїРёСЃР°РЅ РЅР° IMainView СЃРѕР±С‹С‚РёСЏ)
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
