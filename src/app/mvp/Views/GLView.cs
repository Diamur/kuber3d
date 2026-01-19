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

// OpenGL viewport (на ResizeViewport)
using OpenTK.Graphics.OpenGL4;
using WindowsTimer = System.Windows.Forms.Timer;

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
        private readonly WindowsTimer _timer;

        // Флаг, чтобы не вызывать Render() пока контрол не готов
        private bool _isLoaded;

        // ДОБАВЛЕНО:
        // Флаг рендер-цикла (чтобы соответствовать расширенному контракту IGLView.IsRendering)
        private bool _isRendering;

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
            _timer = new WindowsTimer { Interval = 16 };
            _timer.Tick += (_, _) => RequestRender();

            // 3) Подписываемся на события GLControl
            _gl.Load += OnGlLoad;
            _gl.Paint += OnGlPaint;
            _gl.Resize += OnGlResize;
        }

        // =========================
        // IGLView: свойства
        // =========================

        /// <summary>
        /// Реальный WinForms-контрол (по контракту IGLView).
        /// Его встраивают в pnlViewport.
        /// </summary>
        public Control Control => _gl;

        /// <summary>
        /// Доп. удобство для твоих Presenter-ов: контрол, который принимает ввод.
        /// Обычно это тот же _gl.
        /// </summary>
        public Control InputControl => _gl;

        // ДОБАВЛЕНО (под контракт IGLView, который мы расширяли ранее):
        // Эти свойства иногда удобно читать Presenter-у, не лазая в Control напрямую.
        public int ClientWidth => _gl.ClientSize.Width;
        public int ClientHeight => _gl.ClientSize.Height;
        public bool IsRendering => _isRendering;

        // =========================
        // IGLView events
        // =========================
        //
        // ВАЖНО: не поднимаем "свои" new-события, а просто
        // прокидываем add/remove к событиям _gl, чтобы не было перекосов по именам.

        public new event MouseEventHandler? MouseDown
        {
            add { _gl.MouseDown += value; }
            remove { _gl.MouseDown -= value; }
        }

        public new event MouseEventHandler? MouseUp
        {
            add { _gl.MouseUp += value; }
            remove { _gl.MouseUp -= value; }
        }

        public new event MouseEventHandler? MouseMove
        {
            add { _gl.MouseMove += value; }
            remove { _gl.MouseMove -= value; }
        }

        public new event MouseEventHandler? MouseWheel
        {
            add { _gl.MouseWheel += value; }
            remove { _gl.MouseWheel -= value; }
        }

        public new event EventHandler? MouseEnter
        {
            add { _gl.MouseEnter += value; }
            remove { _gl.MouseEnter -= value; }
        }

        /// <summary>
        /// Событие “изменился размер вьюпорта”.
        /// Если в твоём IGLView его нет — можно не использовать.
        /// </summary>
        public event EventHandler? ViewportResized;

        // =========================
        // IGLView API (контекст/буферы/viewport)
        // =========================

        /// <summary>
        /// Сделать контекст OpenGL текущим.
        /// </summary>
        public void MakeCurrent()
        {
            _gl.MakeCurrent();
        }

        /// <summary>
        /// Поменять буферы местами.
        /// </summary>
        public void SwapBuffers()
        {
            _gl.SwapBuffers();
        }

        /// <summary>
        /// Выставить OpenGL viewport под текущий размер.
        /// </summary>
        public void ResizeViewport(int width, int height)
        {
            if (width <= 0 || height <= 0) return;

            MakeCurrent();
            GL.Viewport(0, 0, width, height);
        }

        // =========================
        // Встраивание и запуск рендера (Presenter вызывает)
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

            // ВАЖНО:
            // GLControl.Load может сработать ДО того, как Presenter вызовет StartRendering().
            // Тогда OnGlLoad не сможет сделать Init(), потому что _renderer был null.
            // Поэтому если мы уже loaded — делаем Init здесь.
            if (_isLoaded)
            {
                try
                {
                    // На всякий случай делаем контекст текущим перед Init.
                    MakeCurrent();

                    // Инициализируем рендерер так же "безопасно", как в Load
                    SafeCallRendererInit();

                    // И сразу даём ему актуальный размер (на некоторых версиях это важно)
                    SafeCallRendererResize(_gl.ClientSize.Width, _gl.ClientSize.Height);
                }
                catch
                {
                    // MVP: не падаем
                }

                _timer.Start();
            }
            else
            {
                // Если ещё не loaded — таймер включим в OnGlLoad (как было)
            }

            RequestRender();
        }

        /// <summary>
        /// Остановка рендера (на будущее).
        /// </summary>
        public void StopRendering()
        {
            _timer.Stop();
            _isRendering = false;
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

            MakeCurrent();
	
            // Контекст уже создан — можно инициализировать рендерер.
            // В разных версиях интерфейса IRenderer сигнатуры могут отличаться,
            // поэтому дергаем "аккуратно" через dynamic.
            //
            // ДОБАВЛЕНО:
            // Сейчас мы привели контракт IRenderer к единому виду: Init(w,h), Resize(w,h), Render().
            // Поэтому "аккуратность" оставляем (на случай старых кусков), но основной путь — Init(w,h).
            SafeCallRendererInit();

            // После загрузки можно стартовать таймер (если уже назначили renderer)
            if (_renderer != null)
            {
                _isRendering = true;
                _timer.Start();
            }

            RequestRender();
        }

        private void OnGlResize(object? sender, EventArgs e)
        {
            if (!_isLoaded) return;

            var w = _gl.ClientSize.Width;
            var h = _gl.ClientSize.Height;

            // Сообщаем миру, что размер поменялся (Presenter обновит камеру Aspect)
            ViewportResized?.Invoke(this, EventArgs.Empty);

            // Обновляем viewport в OpenGL
            ResizeViewport(w, h);

            // И сразу говорим рендереру обновить viewport/матрицы
            SafeCallRendererResize(w, h);

            RequestRender();
        }

        private void OnGlPaint(object? sender, PaintEventArgs e)
        {
            if (!_isLoaded) return;
            if (_renderer == null) return;

            // Делаем контекст текущим и выставляем viewport
            MakeCurrent();

            var w = _gl.ClientSize.Width;
            var h = _gl.ClientSize.Height;
            if (w > 0 && h > 0)
                ResizeViewport(w, h);

            // Рисуем кадр (рендерер внутри решает что рисовать: сетка/оси/сцена).
            SafeCallRendererRender();

            // Показываем результат
            SwapBuffers();
        }

        // =========================
        // Safe calls to renderer (чтобы не ловить перекосы сигнатур)
        // =========================

        /// <summary>
        /// Аккуратно дергаем Init() у рендерера.
        /// Поддерживает разные варианты: Init(), Init(w,h), Initialize(), Start() и т.п.
        /// </summary>
        private void SafeCallRendererInit()
        {
            if (_renderer == null) return;

            try
            {
                // ОСНОВНОЙ (актуальный) путь по текущему контракту:
                try
                {
                    _renderer.Init(_gl.ClientSize.Width, _gl.ClientSize.Height);
                    return;
                }
                catch
                {
                    // Если вдруг в старом коде другой контракт — попробуем динамику ниже
                }

                dynamic r = _renderer;

                // 1) Самый частый вариант: Init()
                try { r.Init(); return; } catch { }

                // 2) Если где-то был Init(w,h)
                try { r.Init(_gl.ClientSize.Width, _gl.ClientSize.Height); return; } catch { }

                // 3) Если где-то был Initialize()
                try { r.Initialize(); return; } catch { }

                // 4) Если где-то Start()
                try { r.Start(); return; } catch { }
            }
            catch
            {
                // MVP: не падаем
            }
        }

        /// <summary>
        /// Аккуратно дергаем Resize(w,h) у рендерера.
        /// </summary>
        private void SafeCallRendererResize(int width, int height)
        {
            if (_renderer == null) return;

            try
            {
                // ОСНОВНОЙ путь:
                try
                {
                    _renderer.Resize(width, height);
                    return;
                }
                catch
                {
                    // запасной путь ниже
                }

                dynamic r = _renderer;
                try { r.Resize(width, height); } catch { }
            }
            catch
            {
                // MVP: не падаем
            }
        }

        /// <summary>
        /// Аккуратно дергаем Render() у рендерера.
        /// Поддерживает разные варианты: Render(), RenderFrame(), Render(...)
        /// </summary>
        private void SafeCallRendererRender()
        {
            if (_renderer == null) return;

            try
            {
                // ОСНОВНОЙ путь:
                try
                {
                    _renderer.Render();
                    return;
                }
                catch
                {
                    // запасной путь ниже
                }

                dynamic r = _renderer;

                // 1) Render()
                try { r.Render(); return; } catch { }

                // 2) RenderFrame()
                try { r.RenderFrame(); return; } catch { }

                // 3) Draw()
                try { r.Draw(); return; } catch { }
            }
            catch
            {
                // MVP: не падаем
            }
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

                // Рендерер может держать GL-ресурсы (шейдеры/буферы).
                // Если у него есть Dispose() — освобождаем.
                try { _renderer?.Dispose(); } catch { }

                _renderer = null;

                _gl.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
