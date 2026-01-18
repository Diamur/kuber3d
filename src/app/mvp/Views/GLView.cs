// mvp\Views\GLView.cs
//
// GLView — View-компонент для OpenGL внутри WinForms.
// Он НЕ знает ничего про бизнес-логику, MVP-решения и т.п.
// Его задача:
// - быть "хостом" OpenGL (GLControl)
// - прокидывать события ввода наружу (мышь, колесо, resize)
// - уметь запускать/останавливать рендер
// - вызывать IRenderer.Render() когда нужно перерисовать
//
// В MVP это "V" (View) для 3D.
// Presenter управляет: когда создать, когда стартовать, что рендерить.

using System;
using System.Drawing;
using System.Windows.Forms;
using kuber3d.Contracts;

// OpenTK WinForms GLControl
using OpenTK.WinForms;

namespace kuber3d.Views
{
    /// <summary>
    /// Реализация IGLView через OpenTK GLControl.
    /// </summary>
    public sealed class GLView : UserControl, IGLView
    {
        // Внутренний OpenGL-контрол (именно он создает контекст и умеет SwapBuffers)
        private readonly GLControl _gl;

        // Кто реально рисует кадр (рендерер)
        private IRenderer? _renderer;

        // Таймер для "постоянной" отрисовки.
        // Для MVP так проще и на конференции выглядит стабильно.
        private readonly Timer _timer;

        // Флаг, чтобы не вызывать Render() пока контрол не готов
        private bool _isLoaded;

        public GLView()
        {
            // 1) Создаём GLControl
            // В pre-релизе OpenTK.WinForms обычно достаточно просто new GLControl()
            _gl = new GLControl()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            Controls.Add(_gl);

            // 2) Настраиваем таймер рендера
            // 60 FPS примерно => 16 мс (можно поменять)
            _timer = new Timer { Interval = 16 };
            _timer.Tick += (_, _) => RequestRender();

            // 3) Подписываемся на события GLControl
            _gl.Load += OnGlLoad;
            _gl.Paint += OnGlPaint;
            _gl.Resize += OnGlResize;

            // 4) Пробрасываем мышь наружу (Presenter/MouseController будут слушать это)
            _gl.MouseDown += (s, e) => MouseDown?.Invoke(s, e);
            _gl.MouseUp += (s, e) => MouseUp?.Invoke(s, e);
            _gl.MouseMove += (s, e) => MouseMove?.Invoke(s, e);
            _gl.MouseWheel += (s, e) => MouseWheel?.Invoke(s, e);
        }

        // =========================
        // IGLView events
        // =========================
        public new event MouseEventHandler? MouseDown;
        public new event MouseEventHandler? MouseUp;
        public new event MouseEventHandler? MouseMove;
        public new event MouseEventHandler? MouseWheel;

        public event EventHandler? ViewportResized;

        // =========================
        // IGLView API
        // =========================

        /// <summary>
        /// "Пристыковать" GLView внутрь host-контейнера (pnlViewport).
        /// </summary>
        public void AttachTo(Control host)
        {
            // Без магии: просто добавляем в Controls родителя
            Dock = DockStyle.Fill;
            host.Controls.Clear();        // чтобы не осталось старых контролов
            host.Controls.Add(this);

            // Фокус в GL, чтобы колесо/мышь работали сразу
            FocusGL();
        }

        /// <summary>
        /// Запуск рендера. Presenter передает сюда IRenderer.
        /// </summary>
        public void StartRendering(IRenderer renderer)
        {
            _renderer = renderer;

            // Если GL еще не загрузился, таймер включим после Load.
            // Но на практике GLControl.Load приходит быстро.
            if (_isLoaded)
                _timer.Start();

            RequestRender();
        }

        /// <summary>
        /// Остановка рендера (на будущее).
        /// </summary>
        public void StopRendering()
        {
            _timer.Stop();
            _renderer = null;
        }

        /// <summary>
        /// Просим перерисовать кадр.
        /// Делается через Invalidate у GLControl, что приводит к Paint.
        /// </summary>
        public void RequestRender()
        {
            if (!_isLoaded) return;
            if (_renderer == null) return;

            // Invalidate => Paint => Render
            _gl.Invalidate();
        }

        /// <summary>
        /// Дать фокус именно GL-контролу (чтобы колесо/клавиши доходили).
        /// </summary>
        public void FocusGL()
        {
            _gl.Focus();
        }

        // =========================
        // GLControl event handlers
        // =========================

        private void OnGlLoad(object? sender, EventArgs e)
        {
            _isLoaded = true;

            // Сообщаем рендереру, что есть контекст и размер
            // Обычно здесь: включают depth test, настройку clear color и т.п.
            // Мы делаем это внутри SceneRenderer.Init()
            _renderer?.Init(_gl.ClientSize.Width, _gl.ClientSize.Height);

            // После загрузки можно стартовать таймер (если уже назначили renderer)
            if (_renderer != null)
                _timer.Start();

            RequestRender();
        }

        private void OnGlResize(object? sender, EventArgs e)
        {
            if (!_isLoaded) return;

            // Сообщаем миру, что размер поменялся (Presenter обновит камеру Aspect)
            ViewportResized?.Invoke(this, EventArgs.Empty);

            // И сразу говорим рендереру обновить viewport/матрицы
            _renderer?.Resize(_gl.ClientSize.Width, _gl.ClientSize.Height);

            RequestRender();
        }

        private void OnGlPaint(object? sender, PaintEventArgs e)
        {
            if (!_isLoaded) return;
            if (_renderer == null) return;

            // GLControl в WinForms обычно сам делает MakeCurrent внутри,
            // но мы не надеемся на магию.
            // Если понадобится: _gl.MakeCurrent(); (в pre версии может быть иначе)
            // Пока работаем через стандартный пайп.

            // Рисуем кадр
            _renderer.Render();

            // Показываем результат
            _gl.SwapBuffers();
        }

        // =========================
        // Dispose
        // =========================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();

                // Отписки (не обязательно, но аккуратно)
                _gl.Load -= OnGlLoad;
                _gl.Paint -= OnGlPaint;
                _gl.Resize -= OnGlResize;

                _gl.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
