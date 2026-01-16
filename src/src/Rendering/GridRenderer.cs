using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using kuber3d.Core;

namespace kuber3d.Rendering
{
    public class GridRenderer
    {
        private readonly LineShader _shader = new LineShader();
        private int _vao, _vbo;
        private int _vertexCount;

        public void Init()
        {
            _shader.Create();

            BuildGrid(size: 50, step: 1);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _data.Length * sizeof(float), _data, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        private float[] _data = Array.Empty<float>();

        private void BuildGrid(int size, int step)
        {
            // линии в плоскости XZ, центр в (0,0,0)
            // формат вершины: pos(x,y,z) + col(r,g,b)
            var verts = new System.Collections.Generic.List<float>();

            float colMinor = 0.22f;
            float colMajor = 0.30f;

            for (int i = -size; i <= size; i += step)
            {
                bool major = (i % 5 == 0);
                float c = major ? colMajor : colMinor;

                // линия вдоль X на Z=i
                AddLine(verts, new Vector3(-size, 0, i), new Vector3(size, 0, i), new Vector3(c, c, c));
                // линия вдоль Z на X=i
                AddLine(verts, new Vector3(i, 0, -size), new Vector3(i, 0, size), new Vector3(c, c, c));
            }

            _vertexCount = verts.Count / 6;
            _data = verts.ToArray();
        }

        private static void AddLine(
            System.Collections.Generic.List<float> v,
            Vector3 a, Vector3 b, Vector3 col)
        {
            v.Add(a.X); v.Add(a.Y); v.Add(a.Z);
            v.Add(col.X); v.Add(col.Y); v.Add(col.Z);

            v.Add(b.X); v.Add(b.Y); v.Add(b.Z);
            v.Add(col.X); v.Add(col.Y); v.Add(col.Z);
        }

        public void Render(Camera cam)
        {
            _shader.Use(cam.View, cam.Projection);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, _vertexCount);
            GL.BindVertexArray(0);
        }
    }
}
