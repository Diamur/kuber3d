// FormMain.cs
//
// Это "View" в MVP.
// Задача FormMain:
// - держать UI-контролы (chkGrid, chkAxes, btnStart3D, pnlViewport, tvScene и т.д.)
// - поднимать события для Presenter-а
// - предоставить Presenter-у доступ к состоянию UI (галочки, доступность кнопки)
// - уметь "встроить" 3D-контрол (IGLView.Control) в pnlViewport
//
// ВАЖНО: FormMain НЕ содержит OpenGL-логики и НЕ решает "что делать".
// Она только сообщает "что произошло" и отображает то, что попросил Presenter.

using System;
using System.Windows.Forms;
using kuber3d.Contracts;

namespace kuber3d
{
    public partial class FormMain : Form, IMainView
    {
        // =========================================================
        // IMainView EVENTS (то, на что подписывается Presenter)
        // =========================================================

        /// <summary>
        /// Кнопка btnStart3D нажата.
        /// Presenter ловит это событие и запускает 3D-визор.
        /// </summary>
        public event EventHandler? Start3DClicked;

        /// <summary>
        /// Чекбокс "Сетка" изменён.
        /// Presenter обновляет RenderSettings.ShowGrid и просит перерисовать.
        /// </summary>
        public event EventHandler? GridToggled;

        /// <summary>
        /// Чекбокс "Оси" изменён.
        /// Presenter обновляет RenderSettings.ShowAxes и просит перерисовать.
        /// </summary>
        public event EventHandler? AxesToggled;

        // =========================================================
        // IMainView PROPERTIES (Presenter читает/меняет UI-состояние)
        // =========================================================

        /// <summary>
        /// Состояние галки "Сетка".
        /// (имя строго как в интерфейсе: IsGridEnabled)
        /// </summary>
        public bool IsGridEnabled => chkGrid.Checked;

        /// <summary>
        /// Состояние галки "Оси".
        /// (имя строго как в интерфейсе: IsAxesEnabled)
        /// </summary>
        public bool IsAxesEnabled => chkAxes.Checked;

        /// <summary>
        /// Presenter может включать/выключать кнопку запуска 3D.
        /// (имя строго как в интерфейсе: IsStart3DEnabled)
        /// </summary>
        public bool IsStart3DEnabled
        {
            get => btnStart3D.Enabled;
            set => btnStart3D.Enabled = value;
        }

        // =========================================================
        // ctor: привязка WinForms UI -> события MVP
        // =========================================================

        public FormMain()
        {
            InitializeComponent();

            // Кнопка "3D" -> поднимаем событие для Presenter
            btnStart3D.Click += (_, __) =>
                Start3DClicked?.Invoke(this, EventArgs.Empty);

            // Чекбокс "Сетка" -> событие
            chkGrid.CheckedChanged += (_, __) =>
                GridToggled?.Invoke(this, EventArgs.Empty);

            // Чекбокс "Оси" -> событие
            chkAxes.CheckedChanged += (_, __) =>
                AxesToggled?.Invoke(this, EventArgs.Empty);

            // Чтобы колесо мыши работало стабильно:
            // при клике в правую область даём фокус панели.
            pnlViewport.MouseDown += (_, __) => pnlViewport.Focus();
        }

        // =========================================================
        // IMainView METHODS (Presenter вызывает)
        // =========================================================

        /// <summary>
        /// Встроить OpenGL-контрол внутрь pnlViewport.
        /// Presenter/ViewportPresenter создаёт IGLView, а FormMain просто размещает.
        /// </summary>
        public void AttachViewport(IGLView glView)
        {
            if (glView == null) throw new ArgumentNullException(nameof(glView));

            // Убираем прошлый контрол (если был)
            pnlViewport.Controls.Clear();

            // Берём реальный WinForms-контрол из абстракции IGLView
            Control c = glView.Control;

            // Растягиваем на всю правую панель
            c.Dock = DockStyle.Fill;

            // Вставляем
            pnlViewport.Controls.Add(c);

            // Фокус — чтобы колесо/мышь работали сразу
            c.Focus();
        }

        /// <summary>
        /// Сообщение пользователю (на MVP достаточно MessageBox).
        /// </summary>
        public void ShowMessage(string text)
        {
            MessageBox.Show(
                this,
                text,
                "Kuber3D",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Показать статус (минимально — в заголовке окна).
        /// </summary>
        public void SetStatus(string text)
        {
            Text = string.IsNullOrWhiteSpace(text)
                ? "Kuber3D"
                : $"Kuber3D — {text}";
        }
    }
}
