// mvp/Input/MouseController.cs
//
// MouseController — интерпретатор ввода мыши для 3D-вьюпорта.
//
// Идея (MVP-минимум):
// - Этот класс подписывается на события мыши IGLView (то есть на GL-контрол, который сидит в pnlViewport)
// - Превращает "сырой" ввод (MouseDown/Move/Wheel) в ПОНЯТНЫЕ ДЕЛЬТЫ управления камерой:
//      1) Rotation  (ПКМ + drag)  -> dxRot, dyRot
//      2) Pan       (ЛКМ + drag)  -> dxPan, dyPan
//      3) Zoom      (колесо)      -> zoomDelta
// - Сам MouseController НЕ обязан знать, как именно крутить/двигать камеру.
//   Он просто копит дельты, а Renderer/Presenter их забирает и применяет к Camera.
//
// Почему это удобно:
// - MouseController = слой Input (чистая логика ввода)
// - Camera/Renderer = слой Core/Rendering (математика + OpenGL)
// - View (GLView) = только источник событий, без логики
//
// Минимальные требования из ТЗ:
// - ПКМ: поворот сцены
// - ЛКМ: смещение сцены
// - Колесо: масштабирование сцены
//
// Использование (типовой сценарий):
//   _mouse = new MouseController(_glView, requestRender: () => _glView.RequestRender());
//
//   // Внутри рендера (каждый кадр или по запросу):
//   if (_mouse.TryConsumeRotation(out var dx, out var dy)) { camera.AddYawPitch(dx * settings.OrbitSpeed, dy * settings.OrbitSpeed); }
//   if (_mouse.TryConsumePan(out var dx, out var dy))      { camera.Pan(dx * settings.PanSpeed,  dy * settings.PanSpeed); }
//   if (_mouse.TryConsumeZoom(out var dz))                 { camera.AddDistance(-dz * settings.ZoomSpeed); }
//
// Важно:
// - Мы копим дельты и "сбрасываем" их при чтении (TryConsume...).
// - Это удобно, если у тебя рендер либо по таймеру, либо по Invalidate().

using System;
using System.Drawing;
using System.Windows.Forms;
using kuber3d.Contracts;
using kuber3d.Views;

namespace kuber3d.Input
{
    public sealed class MouseController : IDisposable
    {
        private readonly IGLView _glView;
        private readonly Action _requestRender;

        // Последняя позиция мыши (для вычисления dx/dy)
        private Point _last;

        // Состояния кнопок мыши
        private bool _lmbDown;
        private bool _rmbDown;

        // Накопленные дельты управления
        // (их будет забирать Renderer/Presenter)
        private float _rotDx, _rotDy; // ПКМ
        private float _panDx, _panDy; // ЛКМ
        private float _zoomDelta;     // колесо (в "тиках" колеса)

        private bool _disposed;

        public MouseController(IGLView glView, Action requestRender)
        {
            _glView = glView ?? throw new ArgumentNullException(nameof(glView));
            _requestRender = requestRender ?? throw new ArgumentNullException(nameof(requestRender));

            // Подписываемся на ввод мыши от GLView
            _glView.MouseDown += Gl_MouseDown;
            _glView.MouseUp += Gl_MouseUp;
            _glView.MouseMove += Gl_MouseMove;
            _glView.MouseWheel += Gl_MouseWheel;
            _glView.MouseEnter += Gl_MouseEnter;
        }

        // ------------------------------------------------------------
        // Публичное API: забрать накопленные дельты (и сбросить их)
        // ------------------------------------------------------------

        /// <summary>
        /// Забрать накопленные дельты вращения (ПКМ+drag).
        /// Возвращает true, если есть что применять.
        /// </summary>
        public bool TryConsumeRotation(out float dx, out float dy)
        {
            dx = _rotDx;
            dy = _rotDy;

            if (dx == 0f && dy == 0f)
                return false;

            _rotDx = 0f;
            _rotDy = 0f;
            return true;
        }

        /// <summary>
        /// Забрать накопленные дельты панорамирования (ЛКМ+drag).
        /// </summary>
        public bool TryConsumePan(out float dx, out float dy)
        {
            dx = _panDx;
            dy = _panDy;

            if (dx == 0f && dy == 0f)
                return false;

            _panDx = 0f;
            _panDy = 0f;
            return true;
        }

        /// <summary>
        /// Забрать накопленный зум (колесо).
        /// </summary>
        public bool TryConsumeZoom(out float delta)
        {
            delta = _zoomDelta;

            if (delta == 0f)
                return false;

            _zoomDelta = 0f;
            return true;
        }

        /// <summary>
        /// Сбросить состояние (например, при Reset камеры/сцены).
        /// </summary>
        public void Reset()
        {
            _lmbDown = false;
            _rmbDown = false;
            _rotDx = _rotDy = 0f;
            _panDx = _panDy = 0f;
            _zoomDelta = 0f;
        }

        // ------------------------------------------------------------
        // Обработчики мыши от GLView
        // ------------------------------------------------------------

        private void Gl_MouseEnter(object? sender, EventArgs e)
        {
            // Чтобы колесо мыши стабильно прилетало именно в GL-контрол,
            // даём ему фокус при наведении.
            // (WinForms: Wheel приходит контролу, который в фокусе)
            _glView.Control.Focus();
        }

        private void Gl_MouseDown(object? sender, MouseEventArgs e)
        {
            _last = e.Location;

            if (e.Button == MouseButtons.Left)
                _lmbDown = true;

            if (e.Button == MouseButtons.Right)
                _rmbDown = true;

            // При клике тоже полезно дать фокус — особенно для колеса.
            _glView.Control.Focus();
        }

        private void Gl_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _lmbDown = false;

            if (e.Button == MouseButtons.Right)
                _rmbDown = false;
        }

        private void Gl_MouseMove(object? sender, MouseEventArgs e)
        {
            // Считаем дельту движения мыши
            int dx = e.X - _last.X;
            int dy = e.Y - _last.Y;

            // Обновляем last сразу, чтобы следующая дельта была корректной
            _last = e.Location;

            // Если ничего не зажато — ничего не копим
            if (!_lmbDown && !_rmbDown)
                return;

            // ПКМ: вращение (orbit)
            if (_rmbDown)
            {
                _rotDx += dx;
                _rotDy += dy;

                // Просим перерисовать — чтобы камера реагировала сразу
                _requestRender();
                return;
            }

            // ЛКМ: панорамирование (pan)
            if (_lmbDown)
            {
                _panDx += dx;
                _panDy += dy;

                _requestRender();
                return;
            }
        }

        private void Gl_MouseWheel(object? sender, MouseEventArgs e)
        {
            // e.Delta обычно кратно 120 (один "щелчок" колеса).
            // Мы копим как float, чтобы потом удобно масштабировать чувствительность.
            _zoomDelta += e.Delta;

            _requestRender();
        }

        // ------------------------------------------------------------
        // Dispose: аккуратно отписываемся
        // ------------------------------------------------------------

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _glView.MouseDown -= Gl_MouseDown;
            _glView.MouseUp -= Gl_MouseUp;
            _glView.MouseMove -= Gl_MouseMove;
            _glView.MouseWheel -= Gl_MouseWheel;
            _glView.MouseEnter -= Gl_MouseEnter;
        }
    }
}
