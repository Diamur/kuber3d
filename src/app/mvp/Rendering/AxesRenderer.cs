// mvp/Rendering/AxesRenderer.cs
//
// AxesRenderer Ч отрисовка осей координат (X, Y, Z) в центре сцены.
//
// «ачем отдельный класс:
// - ќси Ч это helper (как сетка), а не объект сцены.
// - Ћегко включать/выключать чекбоксом "ќси".
// - √еометри€ посто€нна€ => можно создать один раз и рисовать каждый кадр.
//
// „то рисуем:
// - “ри отрезка из (0,0,0) в +X, +Y, +Z.
// - ÷вета по классике: X = красный, Y = зелЄный, Z = синий (можно помен€ть в RenderSettings).
//
//  ак работает:
// 1) Build() Ч генерируем 6 вершин (по 2 на ось), загружаем в VBO/VAO.
// 2) Render(mvp) Ч включаем LineShader, задаЄм uMvp, рисуем GL.Lines.
// 3) Dispose() Ч освобождаем GL-ресурсы.

using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace kuber3d.Rendering
{
    public sealed class AxesRenderer : IDisposable
    {
        private readonly RenderSettings _settings;
        private readonly LineShader _shader;

        private int _vao;
        private int _vbo;
        private int _vertexCount;

        private bool _built;
        private bool _disposed;

        public AxesRenderer(RenderSettings settings, LineShader shader)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));
        }

        /// <summary>
        /// —оздаЄм оси и грузим их в GPU. ќбычно делаем 1 раз.
        /// </summary>
        public void Build()
        {
            if (_built) return;

            // ƒлина осей (например 5 единиц).
            // ≈сли в RenderSettings не настроено Ч используем дефолт.
            float len = _settings.AxesLength <= 0 ? 5.0f : _settings.AxesLength;

            // ÷вета осей. ≈сли не заданы Ч дефолтные "RGB".
            Vector4 colX = _settings.AxesColorX == Vector4.Zero
                ? new Vector4(1f, 0f, 0f, 1f)
                : _settings.AxesColorX;

            Vector4 colY = _settings.AxesColorY == Vector4.Zero
                ? new Vector4(0f, 1f, 0f, 1f)
                : _settings.AxesColorY;

            Vector4 colZ = _settings.AxesColorZ == Vector4.Zero
                ? new Vector4(0f, 0.6f, 1f, 1f) // чуть при€тнее чем чистый 0,0,1
                : _settings.AxesColorZ;

            // ћы рисуем 3 линии:
            // X: (0,0,0) -> (len,0,0)
            // Y: (0,0,0) -> (0,len,0)
            // Z: (0,0,0) -> (0,0,len)
            var verts = new List<float>(capacity: 6 * 7);

            // X axis
            AddVertex(verts, Vector3.Zero, colX);
            AddVertex(verts, new Vector3(len, 0f, 0f), colX);

            // Y axis
            AddVertex(verts, Vector3.Zero, colY);
            AddVertex(verts, new Vector3(0f, len, 0f), colY);

            // Z axis
            AddVertex(verts, Vector3.Zero, colZ);
            AddVertex(verts, new Vector3(0f, 0f, len), colZ);

            _vertexCount = verts.Count / 7;

            // --- VAO/VBO ---
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                verts.Count * sizeof(float),
                verts.ToArray(),
                BufferUsageHint.StaticDraw);

            // position (vec3)
            GL.EnableVertexAttribArray(_shader.LocationPosition);
            GL.VertexAttribPointer(
                _shader.LocationPosition,
                3,
                VertexAttribPointerType.Float,
                normalized: false,
                stride: 7 * sizeof(float),
                offset: 0);

            // color (vec4)
            GL.EnableVertexAttribArray(_shader.LocationColor);
            GL.VertexAttribPointer(
                _shader.LocationColor,
                4,
                VertexAttribPointerType.Float,
                normalized: false,
                stride: 7 * sizeof(float),
                offset: 3 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            _built = true;
        }

        /// <summary>
        /// –исуем оси (если ShowAxes=true).
        /// </summary>
        public void Render(in Matrix4 mvp)
        {
            if (!_settings.ShowAxes) return;

            if (!_built) Build();
            if (_vao == 0 || _vertexCount == 0) return;

            _shader.Use();
            _shader.SetMvp(mvp);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Lines, first: 0, count: _vertexCount);
            GL.BindVertexArray(0);
        }

        private static void AddVertex(List<float> list, Vector3 pos, Vector4 col)
        {
            list.Add(pos.X);
            list.Add(pos.Y);
            list.Add(pos.Z);

            list.Add(col.X);
            list.Add(col.Y);
            list.Add(col.Z);
            list.Add(col.W);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_vbo != 0)
            {
                GL.DeleteBuffer(_vbo);
                _vbo = 0;
            }

            if (_vao != 0)
            {
                GL.DeleteVertexArray(_vao);
                _vao = 0;
            }

            _built = false;
            _vertexCount = 0;
        }
    }
}
