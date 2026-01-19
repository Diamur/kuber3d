// mvp/Input/MouseController.cs
//
// MouseController — связывает события мыши (на IGLView.Control) с Core.Camera.
// Здесь нет OpenGL и рендера — только ввод.
// По умолчанию:
//  - LMB = Orbit
//  - MMB = Pan
//  - Shift + LMB = Pan (удобно без средней кнопки)
//  - Wheel = Zoom
//
// Важно: фикс "стартового рывка" — после входа мыши/клика мы сначала инициализируем
// lastX/lastY и только потом считаем dx/dy.

using System;
using System.Windows.Forms;
using kuber3d.Contracts;
using kuber3d.Core;
using kuber3d.Rendering;

namespace kuber3d.Input
{
    public sealed class MouseController : IDisposable
    {
        private readonly Control _target;
        private readonly Camera _camera;
        private readonly RenderSettings _settings;
        private readonly Action _requestRender;

        private bool _mouseInited;
        private int _lastX;
        private int _lastY;

        private bool _lmbDown;
        private bool _mmbDown;

        public MouseController(IGLView glView, Camera camera, RenderSettings settings, Action requestRender)
            : this(glView.Control, camera, settings, requestRender)
        {
        }

        public MouseController(Control target, Camera camera, RenderSettings settings, Action requestRender)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _requestRender = requestRender ?? throw new ArgumentNullException(nameof(requestRender));

            // Убираем "песочные часы" если вдруг WinForms считает, что занято
            _target.UseWaitCursor = false;
            _target.Cursor = Cursors.Default;

            _target.MouseDown += OnMouseDown;
            _target.MouseUp += OnMouseUp;
            _target.MouseMove += OnMouseMove;
            _target.MouseWheel += OnMouseWheel;
            _target.MouseEnter += OnMouseEnter;
            _target.MouseLeave += OnMouseLeave;
        }

        private void OnMouseEnter(object? sender, EventArgs e)
        {
            // При входе мыши сбрасываем инициализацию — это убирает резкий скачок dx/dy.
            _mouseInited = false;
        }

        private void OnMouseLeave(object? sender, EventArgs e)
        {
            // На выходе отпускаем кнопки, чтобы не "залипало"
            _lmbDown = false;
            _mmbDown = false;
            _mouseInited = false;
            _target.Capture = false;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _lmbDown = true;
            if (e.Button == MouseButtons.Middle) _mmbDown = true;

            // фикс рывка при нажатии
            _mouseInited = false;

            // чтобы продолжать получать события даже если курсор уехал за пределы контрола
            _target.Capture = true;
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _lmbDown = false;
            if (e.Button == MouseButtons.Middle) _mmbDown = false;

            if (!_lmbDown && !_mmbDown)
                _target.Capture = false;
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_lmbDown && !_mmbDown)
            {
                // если кнопки не зажаты — просто синхронизируем last, чтобы не ловить скачок
                _lastX = e.X;
                _lastY = e.Y;
                _mouseInited = true;
                return;
            }

            if (!_mouseInited)
            {
                _mouseInited = true;
                _lastX = e.X;
                _lastY = e.Y;
                return;
            }

            int dx = e.X - _lastX;
            int dy = e.Y - _lastY;
            _lastX = e.X;
            _lastY = e.Y;

            bool shift = IsShift();

            // Orbit: ЛКМ (без Shift)
            if (_lmbDown && !shift)
            {
                // В Camera.Orbit мы передаем dx/dy в пикселях — сама камера умножит на sensitivity.
                // Но у нас есть настройка чувствительности в RenderSettings.
                _camera.Orbit(dx, dy, _settings.OrbitSpeed);
                _requestRender();
                return;
            }

            // Pan: СКМ или Shift+ЛКМ
            if (_mmbDown || (_lmbDown && shift))
            {
                // Чем дальше камера — тем быстрее должен быть пан (внутри Camera.Pan это уже можно учесть,
                // но на MVP уровне даем скорость сюда).
                _camera.Pan(dx, dy, _settings.PanSpeed);
                _requestRender();
                return;
            }
        }

        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            // Zoom: wheelDelta обычно +/-120
            _camera.Zoom(e.Delta, _settings.ZoomSpeed);
            _requestRender();
        }

        private static bool IsShift()
            => (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

        public void Dispose()
        {
            _target.MouseDown -= OnMouseDown;
            _target.MouseUp -= OnMouseUp;
            _target.MouseMove -= OnMouseMove;
            _target.MouseWheel -= OnMouseWheel;
            _target.MouseEnter -= OnMouseEnter;
            _target.MouseLeave -= OnMouseLeave;

            _target.Capture = false;
        }
    }
}
