// mvp\Contracts\IMainView.cs
//
// Контракт "главного окна" (FormMain) для Presenter.
//
// Зачем он нужен:
// - Presenter НЕ должен знать про WinForms-контролы напрямую (Form, Button, CheckBox и т.д.).
// - Presenter работает только с интерфейсом IMainView.
// - FormMain реализует IMainView и внутри уже сам дергает реальные элементы:
//   btnStart3D, checkBox_Grid, checkBox_Axes, pnlViewport, tvScene.
//
// MVP-логика в минимуме:
// - Нажали кнопку "3D" -> Presenter стартует/подключает 3D-вьюпорт.
// - Чекбокс "Сетка" -> Presenter меняет настройки рендера и просит перерисовать.
// - Чекбокс "Оси"   -> то же самое.
// - Presenter должен уметь "вставить" 3D-контрол (GLView) в правую панель pnlViewport.
//
// Важно про имена из твоего правила:
// splitMain, tvScene, btnStart3D, checkBox_Grid, checkBox_Axes
// Эти имена — про WinForms поля. В интерфейсе мы не обязаны повторять 1:1 названия полей,
// но смысл/назначение обязаны совпадать.

using System;

namespace kuber3d.Contracts
{
    /// <summary>
    /// Контракт главного View (FormMain).
    /// Presenter видит только это.
    /// </summary>
    public interface IMainView
    {
        // =========================
        // 1) События UI (ввод)
        // =========================

        /// <summary>
        /// Пользователь нажал кнопку "3D" (btnStart3D).
        /// Presenter должен:
        ///  - создать/инициализировать GL-вью (если ещё нет),
        ///  - встроить его в pnlViewport,
        ///  - запустить цикл отрисовки.
        /// </summary>
        event EventHandler Start3DClicked;

        /// <summary>
        /// Пользователь включил/выключил "Сетка" (checkBox_Grid).
        /// Presenter меняет RenderSettings.ShowGrid и просит перерисовать.
        /// </summary>
        event EventHandler GridToggled;

        /// <summary>
        /// Пользователь включил/выключил "Оси" (checkBox_Axes).
        /// Presenter меняет RenderSettings.ShowAxes и просит перерисовать.
        /// </summary>
        event EventHandler AxesToggled;


        // =========================
        // 2) Состояние UI (вывод)
        // =========================

        /// <summary>
        /// Текущее состояние чекбокса "Сетка" (checkBox_Grid.Checked).
        /// Presenter читает это значение при обработке GridToggled или при инициализации.
        /// </summary>
        bool IsGridEnabled { get; }

        /// <summary>
        /// Текущее состояние чекбокса "Оси" (checkBox_Axes.Checked).
        /// </summary>
        bool IsAxesEnabled { get; }

        /// <summary>
        /// Можно ли нажимать кнопку 3D.
        /// Например, после старта 3D можно временно блокировать кнопку,
        /// чтобы не создавать повторно GLView.
        /// </summary>
        bool IsStart3DEnabled { get; set; }


        // =========================
        // 3) Встраивание 3D-вида
        // =========================

        /// <summary>
        /// Presenter передаёт сюда "3D-контрол" (реализацию IGLView),
        /// а View (FormMain) уже встраивает его в pnlViewport.
        ///
        /// Почему так:
        /// - Presenter не трогает pnlViewport.Controls напрямую (он про WinForms не должен знать).
        /// - View умеет "положить" любой Control внутрь pnlViewport.
        ///
        /// В реализации FormMain это будет что-то вроде:
        ///   pnlViewport.Controls.Clear();
        ///   pnlViewport.Controls.Add(glView.AsControl);
        ///   glView.AsControl.Dock = DockStyle.Fill;
        /// </summary>
        void AttachViewport(IGLView glView);


        // =========================
        // 4) Минимальная диагностика
        // =========================

        /// <summary>
        /// Показать сообщение пользователю (на MVP этапе удобно для ошибок/статуса).
        /// Например: "OpenGL не инициализировался", "3D уже запущен" и т.д.
        /// </summary>
        void ShowMessage(string text);

        /// <summary>
        /// Лог/статус строка (опционально).
        /// Можно выводить в заголовок окна или статус-бар (если появится).
        /// На MVP можно просто оставить пустой реализацией.
        /// </summary>
        void SetStatus(string text);
    }
}
