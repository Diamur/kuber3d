// mvp\Contracts\IGLView.cs
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
// - События ввода мыши (для камеры): MouseDown/Up/Move/Wheel/Enter
//
// Важно:
// - Здесь НЕТ кода OpenGL-отрисовки. Это только "хост" окна/контекста.
// - НЕТ ссылок на FormMain / pnlViewport / TreeView — это уровень View.
// - Здесь НЕТ логики "что рисовать" — это делает Renderer (SceneRenderer и т.п.).
// - Здесь НЕТ логики камеры — это делает MouseController/Camera.
// - Здесь чисто View-уровень: контекст, таймер кадра, resize, события ввода.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL4;
using OpenTK.WinForms;
using kuber3d.Contracts;

namespace kuber3d.Views
{
    /// <summary>
    /// GLView — тонкая оболочка над GLControl.
    /// Реализует IGLView + даёт удобные методы AttachTo/StartRendering для Presenter.
    /// </summary>
    public sealed class GLView : IGLView
    {
        // Реальный OpenGL-контрол (из OpenTK.WinForms).
        private readonly GLControl _gl;

        // Таймер "кадров": по тикам будем просить перерисовку.
        // (Супер MVP-решение — проще чем сложный игровой loop).
        private readonly Timer _frameTimer;

        // Подключённый рендерер (кто реально рисует).
        private IRenderer? _renderer;

        // Флаги жизненного цикла.
        private bool _isLoaded;
        private bool _isRendering;

        /// <summary>
        /// Реальный WinForms-контрол, который вставляется в pnlViewport.
        /// По контракту IGLView.
        /// </summary>
        public Control Control => _gl;

        public GLView()
        {
            // Самый безопасный вариант для разных версий OpenTK.WinForms:
            // используем базовый конструктор GLControl без настроек.
            _gl = new GLControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            // GLControl жизненный цикл:
            // Load  -> контекст гарантированно создан
            // Paint -> можно рисовать
            // Resize-> обновить viewport/renderer
            _gl.Load += OnGlLoad;
            _gl.Paint += OnGlPaint;
            _gl.Resize += OnGlResize;

            // Таймер кадров. 16 мс ~ 60 FPS.
            _frameTimer = new Timer
            {
                Interval = 16
            };
            _frameTimer.Tick += (_, __) =>
            {
                // Таймер НЕ рисует сам.
                // Он только просит перерисовку, чтобы WinForms вызвал Paint.
                if (_isRendering)
                    RequestRender();
            };
        }

        // =========================================================
        // Встраивание в UI (Presenter вызывает это при старте 3D)
        // =========================================================

        /// <summary>
        /// Вставить GLView в указанный host (например pnlViewport).
        /// Это метод удобства (не часть контракта IGLView).
        /// </summary>
        public void AttachTo(Control host)
        {
            // Чтобы было "чисто": если там что-то было, убираем.
            // (Если ты захочешь держать там ещё UI — уберём Clear()).
            host.Controls.Clear();

            _gl.Dock = DockStyle.Fill;
            host.Controls.Add(_gl);
        }

        // =========================================================
        // Запуск/останов рендера (Presenter управляет этим)
        // =========================================================

        /// <summary>
        /// Запустить рендер-цикл с указанным renderer.
        /// Presenter обычно делает:
        /// - renderer.Bind(...)
        /// - glView.StartRendering(renderer)
        /// </summary>
        public void StartRendering(IRenderer renderer)
        {
            _renderer = renderer;

            // Если контрол уже загружен — можно сразу инициировать renderer.
            // Если ещё нет — это сделает OnGlLoad.
            if (_isLoaded)
            {
                SafeMakeCurrent();

                // Init — на усмотрение интерфейса IRenderer.
                // Мы ожидаем, что в IRenderer есть Initialize() / Resize() / Render().
                _renderer.Initialize();

                var sz = _gl.ClientSize;
                _renderer.Resize(sz.Width, sz.Height);
            }

            _isRendering = true;
            _frameTimer.Start();

            // Первый кадр — сразу.
            RequestRender();
        }

        /// <summary>
        /// Остановить рендер-цикл.
        /// </summary>
        public void StopRendering()
        {
            _isRendering = false;
            _frameTimer.Stop();
        }

        // =========================================================
        // Реализация IGLView
        // =========================================================

        public void MakeCurrent()
        {
            _gl.MakeCurrent();
        }

        public void SwapBuffers()
        {
            _gl.SwapBuffers();
        }

        public void ResizeViewport(int width, int height)
        {
            // В OpenGL viewport задаётся в пикселях.
            // Это "рамка", куда GL будет рисовать.
            if (width <= 0 || height <= 0)
                return;

            GL.Viewport(0, 0, width, height);
        }

        public void RequestRender()
        {
            // Invalidate -> WinForms вызовет Paint.
            _gl.Invalidate();
        }

        // =========================================================
        // События мыши (пробрасываем наружу через IGLView)
        // =========================================================

        public event MouseEventHandler? MouseDown
        {
            add { _gl.MouseDown += value; }
            remove { _gl.MouseDown -= value; }
        }

        public event MouseEventHandler? MouseUp
        {
            add { _gl.MouseUp += value; }
            remove { _gl.MouseUp -= value; }
        }

        public event MouseEventHandler? MouseMove
        {
            add { _gl.MouseMove += value; }
            remove { _gl.MouseMove -= value; }
        }

        public event MouseEventHandler? MouseWheel
        {
            add { _gl.MouseWheel += value; }
            remove { _gl.MouseWheel -= value; }
        }

        public event EventHandler? MouseEnter
        {
            add { _gl.MouseEnter += value; }
            remove { _gl.MouseEnter -= value; }
        }

        // =========================================================
        // Внутренние обработчики GLControl
        // =========================================================

        private void OnGlLoad(object? sender, EventArgs e)
        {
            _isLoaded = true;

            // Контекст уже существует. Делаем current и инициализируем рендерер.
            SafeMakeCurrent();

            // Базовое состояние GL можно задать тут (минимум).
            // Вся "политика" рендеринга всё равно внутри Renderer.
            GL.ClearColor(Color.Black);

            if (_renderer != null)
            {
                _renderer.Initialize();

                var sz = _gl.ClientSize;
                ResizeViewport(sz.Width, sz.Height);
                _renderer.Resize(sz.Width, sz.Height);
            }
        }

        private void OnGlResize(object? sender, EventArgs e)
        {
            if (!_isLoaded)
                return;

            var sz = _gl.ClientSize;
            if (sz.Width <= 0 || sz.Height <= 0)
                return;

            SafeMakeCurrent();

            // 1) Обновляем viewport в OpenGL
            ResizeViewport(sz.Width, sz.Height);

            // 2) Сообщаем renderer'у новый размер (если ему нужно)
            _renderer?.Resize(sz.Width, sz.Height);

            // 3) Просим кадр
            RequestRender();
        }

        private void OnGlPaint(object? sender, PaintEventArgs e)
        {
            // Paint может прилетать даже до Load — проверяем.
            if (!_isLoaded)
                return;

            if (_renderer == null)
            {
                // Пока нет renderer — просто очищаем экран.
                SafeMakeCurrent();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                SwapBuffers();
                return;
            }

            SafeMakeCurrent();

            var sz = _gl.ClientSize;
            if (sz.Width > 0 && sz.Height > 0)
                ResizeViewport(sz.Width, sz.Height);

            // Главный вызов рендера (внутри _renderer решает, что рисовать: сетка/оси/сцена).
            _renderer.Render();

            SwapBuffers();
        }

        /// <summary>
        /// Безопасный MakeCurrent (на случай если в будущем будут исключения/смена потока).
        /// </summary>
        private void SafeMakeCurrent()
        {
            try { MakeCurrent(); }
            catch
            {
                // MVP: глушим, чтобы не падать в UI.
                // Позже можно логировать.
            }
        }

        // =========================================================
        // IDisposable
        // =========================================================

        public void Dispose()
        {
            StopRendering();

            _frameTimer.Dispose();

            // Рендерер может держать GL-ресурсы (шейдеры/буферы).
            // Если у IRenderer есть Dispose() — освобождаем.
            _renderer?.Dispose();
            _renderer = null;

            _gl.Load -= OnGlLoad;
            _gl.Paint -= OnGlPaint;
            _gl.Resize -= OnGlResize;

            _gl.Dispose();
        }
    }
}
