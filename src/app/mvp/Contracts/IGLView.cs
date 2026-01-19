// mvp/Contracts/IGLView.cs
//
// Контракт для "3D-вьюпорта" (GL-виджета), который встраивается в pnlViewport.
//
// Зачем отдельный интерфейс:
// - Presenter должен уметь управлять 3D-видом, не зная про конкретный GLControl/OpenTK.
// - Сегодня это будет OpenTK.WinForms.GLControl.
// - Завтра можно заменить на другую реализацию (например, Vulkan-хост, WPF-хост и т.д.),
//   а Presenter/Model останутся теми же.
//
// На MVP-минимум нам нужно:
// - Control: чтобы View могла "вставить" его в pnlViewport (Controls.Add)
// - MakeCurrent/SwapBuffers: базовые операции контекста
// - ResizeViewport: когда меняется размер панели
// - RequestRender: просим перерисовать кадр (Invalidate/RenderOnce)
// - StartRendering/StopRendering: запуск/остановка рендер-цикла (таймер/луп решает реализация)
// - События ввода мыши (для камеры): MouseDown/Up/Move/Wheel/Enter
//
// Важно:
// - Здесь НЕТ кода OpenGL-отрисовки. Это только "хост" окна/контекста.
// - НЕТ ссылок на FormMain / pnlViewport / TreeView — это уровень View.
// - Здесь НЕТ логики "что рисовать" — это делает Renderer (SceneRenderer и т.п.).
// - Здесь НЕТ логики камеры — это делает MouseController/Camera.
// - Здесь чисто View-уровень: контекст, таймер кадра, resize, события ввода.

using System;
using System.Windows.Forms;

namespace kuber3d.Contracts
{
    public interface IGLView : IDisposable
    {
        /// <summary>
        /// Реальный WinForms-контрол, который мы вставляем в pnlViewport.
        /// Например: OpenTK.WinForms.GLControl.
        /// </summary>
        Control Control { get; }

        /// <summary>
        /// Сделать контекст OpenGL "текущим" (current) для этого потока.
        /// </summary>
        void MakeCurrent();

        /// <summary>
        /// Поменять буферы местами (показать то, что нарисовали).
        /// </summary>
        void SwapBuffers();

        /// <summary>
        /// Выставить viewport в OpenGL под текущий размер контрола.
        /// Обычно вызывается при Resize.
        /// </summary>
        void ResizeViewport(int width, int height);

        /// <summary>
        /// Попросить перерисовку (Invalidate/RenderOnce).
        /// Рендер-цикл может быть таймерный или событийный — это решает реализация.
        /// </summary>
        void RequestRender();

        /// <summary>
        /// Запуск рендера. Реализация может использовать таймер/idle-loop,
        /// но Presenter передаёт сюда "что рисовать" через IRenderer.
        /// </summary>
        void StartRendering(IRenderer renderer);

        /// <summary>
        /// Остановить рендер-цикл (если он был запущен).
        /// </summary>
        void StopRendering();

        // ------------------------
        // События мыши
        // ------------------------
        // Presenter подпишется на эти события и передаст их в MouseController/Camera.

        event MouseEventHandler? MouseDown;
        event MouseEventHandler? MouseUp;
        event MouseEventHandler? MouseMove;
        event MouseEventHandler? MouseWheel;
        event EventHandler? MouseEnter;

        // ------------------------
        // ДОБАВЛЕНО (MVP-удобство, но не ломает архитектуру):
        // ------------------------
        //
        // Эти свойства НЕ добавляют зависимости от UI наружу — это всё ещё "View-уровень".
        // Просто иногда Presenter-у удобно узнать размеры, не трогая реальный Control напрямую.
        //
        // Если не хочешь — можно не использовать, но оставляем на будущее:
        //
        // - ClientWidth/ClientHeight: текущий размер вьюпорта (под Resize/Init рендера)
        // - IsRendering: запущен ли цикл рендера
        //
        // ВАЖНО: это опционально для реализации (в GLView.cs это делается легко).
        int ClientWidth { get; }
        int ClientHeight { get; }
        bool IsRendering { get; }
    }
}
