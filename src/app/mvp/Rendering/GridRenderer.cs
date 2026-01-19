// mvp/Rendering/GridRenderer.cs
//
// GridRenderer — отрисовка "пола" (сетки) в 3D-вьюпорте.
//
// Задача для MVP-минимума:
// - Сетка рисуется линиями в плоскости XZ (Y=0)
// - По умолчанию есть, но должна уметь выключаться через RenderSettings.ShowGrid
//
// Почему отдельный класс:
// - Сетка — это "хелпер", а не объект сцены.
// - Её удобно быстро включать/выключать, не трогая остальной рендер.
// - Геометрия сетки почти не меняется => генерируем один раз и держим в GPU.
//
// Как устроено:
// 1) Build() — генерируем вершины сетки и заливаем в VBO/VAO (StaticDraw)
// 2) Render(mvp) — если ShowGrid=true, включаем шейдер и рисуем GL.Lines
// 3) Dispose() — освобождаем VAO/VBO
//
// Вершина: 7 float
//   position: vec3 (x,y,z)
//   color:    vec4 (r,g,b,a)
//
// Важно:
// - Этот класс не знает про WinForms и не ловит события мыши.
// - Он вызывается SceneRenderer'ом каждый кадр.

using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace kuber3d.Rendering
{
    public sealed class GridRenderer : IDisposable
    {
        private readonly RenderSettings _settings;
        private readonly LineShader _shader;

        // OpenGL ресурсы
        private int _vao;
        private int _vbo;
        private int _vertexCount;

        private bool _built;
        private bool _disposed;

        public GridRenderer(RenderSettings settings, LineShader shader)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));
        }

        /// <summary>
        /// Генерирует геометрию сетки и загружает в GPU.
        /// Вызываем один раз при старте 3D (или "лениво" при первом Render()).
        /// </summary>
        public void Build()
        {
            if (_built) return;

            // Берём параметры из настроек (и страхуемся дефолтами).
            int halfSize = _settings.GridHalfSize > 0 ? _settings.GridHalfSize : 20;
            float step = _settings.GridStep > 0 ? _settings.GridStep : 1.0f;

            // Цвет сетки (минимально: один цвет на все линии).
            // Если захочешь "толстые" главные линии — добавим позже.
            Vector4 color = new Vector4(0.45f, 0.45f, 0.45f, 1.0f);

            // Список float: на одну вершину 7 float.
            var verts = new List<float>(capacity: 50000);

            // Сетка идёт от -halfSize до +halfSize.
            // Мы рисуем:
            // - линии параллельно X (при фиксированном Z)
            // - линии параллельно Z (при фиксированном X)
            //
            // Каждая линия — это 2 вершины.
            float min = -halfSize;
            float max = +halfSize;

            // Сколько "шагов" получаем между min..max
            // Пример: halfSize=20, step=1 => 40 шагов, 41 линия
            int steps = (int)MathF.Round((max - min) / step);

            for (int i = 0; i <= steps; i++)
            {
                float t = min + i * step;

                // Линия по X при Z=t
                AddVertex(verts, new Vector3(min, 0f, t), color);
                AddVertex(verts, new Vector3(max, 0f, t), color);

                // Линия по Z при X=t
                AddVertex(verts, new Vector3(t, 0f, min), color);
                AddVertex(verts, new Vector3(t, 0f, max), color);
            }

            _vertexCount = verts.Count / 7;

            // ---------- Создаём VAO/VBO ----------
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // Заливаем данные в буфер
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                verts.Count * sizeof(float),
                verts.ToArray(),
                BufferUsageHint.StaticDraw
            );

            // Атрибут 0: position (vec3)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                index: 0,
                size: 3,
                type: VertexAttribPointerType.Float,
                normalized: false,
                stride: 7 * sizeof(float),
                offset: 0
            );

            // Атрибут 1: color (vec4)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(
                index: 1,
                size: 4,
                type: VertexAttribPointerType.Float,
                normalized: false,
                stride: 7 * sizeof(float),
                offset: 3 * sizeof(float)
            );

            // Отвязываем (чистота)
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            _built = true;
        }

        /// <summary>
        /// Рисует сетку (если включена).
        /// Вызывается каждый кадр из SceneRenderer.
        /// </summary>
        public void Render(in Matrix4 mvp)
        {
            // Если чекбокс "Сетка" выключен — сетку не рисуем.
            if (!_settings.ShowGrid) return;

            // Если ещё не построили — построим сейчас.
            if (!_built) Build();

            if (_vao == 0 || _vertexCount <= 0) return;

            // Шейдер линий общий для сетки/осей.
            _shader.Use();
            _shader.SetMvp(mvp);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, _vertexCount);
            GL.BindVertexArray(0);
        }

        private static void AddVertex(List<float> list, Vector3 pos, Vector4 col)
        {
            // position
            list.Add(pos.X);
            list.Add(pos.Y);
            list.Add(pos.Z);

            // color
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
