using System;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.WinForms;
using kuber3d.Core;
using kuber3d.Input;
using WindowsTimer = System.Windows.Forms.Timer;

namespace kuber3d.Rendering
{
    public class GLView : UserControl
    {
        private readonly GLControl _gl;
        private readonly WindowsTimer _timer;

        public Camera Camera { get; } = new Camera();
        public SceneRenderer Renderer { get; } = new SceneRenderer();
        public MouseController Mouse { get; }

        public GLView()
        {
            // Важно: чтобы WinForms нормально отрисовывал дочерние контролы
            Dock = DockStyle.Fill;

            _gl = new GLControl(new GLControlSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            })
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };

            Controls.Add(_gl);

            Mouse = new MouseController(_gl, Camera);

            _gl.Load += OnLoad;
            _gl.Resize += OnResize;
            _gl.Paint += OnPaint;

            // Стартовая камера
            HandleCreated += (_, __) =>
            {
                Camera.ResetToOrigin();
                Camera.Resize(_gl.ClientSize.Width, _gl.ClientSize.Height);
                _gl.Invalidate();
            };

            _timer = new WindowsTimer { Interval = 16 };
            _timer.Tick += (_, __) => _gl.Invalidate();
            _timer.Start();
        }

        private void OnLoad(object? sender, EventArgs e)
        {
            GL.ClearColor(0.08f, 0.08f, 0.10f, 1f);
            GL.Enable(EnableCap.DepthTest);

            Renderer.Init();

            Camera.ResetToOrigin();
            Camera.Resize(_gl.ClientSize.Width, _gl.ClientSize.Height);
        }

        private void OnResize(object? sender, EventArgs e)
        {
            if (_gl.ClientSize.Height <= 0) return;

            GL.Viewport(0, 0, _gl.ClientSize.Width, _gl.ClientSize.Height);
            Camera.Resize(_gl.ClientSize.Width, _gl.ClientSize.Height);
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Renderer.Render(Camera);

            _gl.SwapBuffers();
        }
    }
}
