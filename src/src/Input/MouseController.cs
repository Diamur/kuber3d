using System;
using System.Windows.Forms;
using OpenTK.Mathematics;
using OpenTK.WinForms;
using kuber3d.Core;

namespace kuber3d.Input
{
    public class MouseController
    {
        private readonly GLControl _gl;
        private readonly Camera _cam;

        private bool _mouseInited = false;
        private int _lastX, _lastY;

        private bool _lmbDown = false; // было RMB
        private bool _mmbDown = false;

        public MouseController(GLControl gl, Camera cam)
        {
            _gl = gl;
            _cam = cam;

            _gl.MouseDown += OnDown;
            _gl.MouseUp += OnUp;
            _gl.MouseMove += OnMove;
            _gl.MouseWheel += OnWheel;
            _gl.MouseEnter += (_, __) => { _mouseInited = false; }; // важно
        }

        private void OnDown(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _lmbDown = true;      // LMB
            if (e.Button == MouseButtons.Middle) _mmbDown = true;

            _mouseInited = false; // чтобы не было рывка при клике
        }

        private void OnUp(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _lmbDown = false;     // LMB
            if (e.Button == MouseButtons.Middle) _mmbDown = false;
        }

        private void OnMove(object? s, MouseEventArgs e)
        {
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

            // Orbit: LMB
            if (_lmbDown && !IsShift())
            {
                _cam.Orbit(dx * 0.25f, -dy * 0.25f);
            }

            // Pan: MMB или Shift+LMB
            if (_mmbDown || (_lmbDown && IsShift()))
            {
                // простой пан в экранных осях (быстрый MVP)
                float panSpeed = _cam.Distance * 0.0025f;
                var pan = new Vector3(-dx * panSpeed, dy * panSpeed, 0);
                _cam.AddPan(pan);
            }
        }

        private void OnWheel(object? s, MouseEventArgs e)
        {
            // колесо: приближение/отдаление
            _cam.AddZoom(e.Delta > 0 ? -1.5f : 1.5f);
        }

        private static bool IsShift()
            => (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
    }
}
