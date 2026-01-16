using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using kuber3d.Core;

namespace kuber3d.Rendering
{
    public class AxesRenderer
    {
        private readonly LineShader _shader = new LineShader();
        private int _vao, _vbo;
        private int _vertexCount;
        private float[] _data = Array.Empty<float>();

        public void Init()
        {
            _shader.Create();

            var v = new System.Collections.Generic.List<float>();

            // X red
            AddLine(v, new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(1, 0, 0));
            // Y green
            AddLine(v, new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(0, 1, 0));
            // Z blue
            AddLine(v, new Vector3(0, 0, 0), new Vector3(0, 0, 10), new Vector3(0, 0, 1));

            _data = v.ToArray();
            _vertexCount = _data.Length / 6;

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
