// mvp\Contracts\IMainPresenter.cs
//
// Контракт Presenter для главного окна (FormMain).
//
// Зачем это нужно:
// - В строгом MVP View (FormMain) не содержит логики приложения.
// - View только поднимает события (кнопка/чекбоксы/мышь) и отдаёт минимальные данные.
// - Presenter:
//    1) подписывается на события View
//    2) хранит Model (SceneModel, CameraModel, RenderSettings)
//    3) управляет рендером и 3D-вьюпортом (через IGLView / IRenderer)
//
// На MVP-минимум нам достаточно:
// - Init(): связать всё вместе и подготовить состояние
// - Start3D(): создать GL-вью и рендерер, вмонтировать в pnlViewport через IMainView.AttachViewport()
// - Dispose(): аккуратно освободить OpenGL/таймеры/подписки
//
// Важно: Presenter — "мозг" UI.
// View не знает, как устроен OpenGL и камера.
// Model не знает про WinForms и OpenGL.
// Renderer знает про OpenGL, но не про WinForms.
//
// FormMain в Program.cs создаётся как обычно, но Presenter создаём рядом и вызываем Init().

using System;

namespace kuber3d.Contracts
{
    public interface IMainPresenter : IDisposable
    {
        /// <summary>
        /// Связывает Presenter с View:
        /// - подписывает Presenter на события View (кнопка/чекбоксы)
        /// - подготавливает начальное состояние (например, синхронизация чекбоксов с настройками)
        ///
        /// Этот метод обычно вызывается один раз при старте приложения.
        /// </summary>
        void Init();

        /// <summary>
        /// Запуск 3D-вьюпорта.
        /// Должен быть безопасным при повторном вызове (например, если кнопка нажата 2 раза).
        ///
        /// Что обычно делает внутри:
        /// - создаёт IGLView (реальную WinForms/GL реализацию)
        /// - создаёт/инициализирует IRenderer (SceneRenderer и т.д.)
        /// - передаёт IGLView в View.AttachViewport()
        /// - запускает цикл рендера (внутри IGLView или через таймер)
        /// </summary>
        void Start3D();

        /// <summary>
        /// Включение/выключение сетки.
        /// Обычно вызывается обработчиком события чекбокса "Сетка".
        /// </summary>
        void SetGridEnabled(bool enabled);

        /// <summary>
        /// Включение/выключение осей.
        /// Обычно вызывается обработчиком события чекбокса "Оси".
        /// </summary>
        void SetAxesEnabled(bool enabled);
    }
}
