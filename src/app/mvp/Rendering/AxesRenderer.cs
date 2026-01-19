// mvp/Rendering/AxesRenderer.cs
//
// AxesRenderer Р§ РѕС‚СЂРёСЃРѕРІРєР° РѕСЃРµР№ РєРѕРѕСЂРґРёРЅР°С‚ (X, Y, Z) РІ С†РµРЅС‚СЂРµ СЃС†РµРЅС‹.
//
// В«Р°С‡РµРј РѕС‚РґРµР»СЊРЅС‹Р№ РєР»Р°СЃСЃ:
// - СњСЃРё Р§ СЌС‚Рѕ helper (РєР°Рє СЃРµС‚РєР°), Р° РЅРµ РѕР±СЉРµРєС‚ СЃС†РµРЅС‹.
// - Р‹РµРіРєРѕ РІРєР»СЋС‡Р°С‚СЊ/РІС‹РєР»СЋС‡Р°С‚СЊ С‡РµРєР±РѕРєСЃРѕРј "СњСЃРё".
// - в€љРµРѕРјРµС‚СЂРёВ¤ РїРѕСЃС‚РѕВ¤РЅРЅР°В¤ => РјРѕР¶РЅРѕ СЃРѕР·РґР°С‚СЊ РѕРґРёРЅ СЂР°Р· Рё СЂРёСЃРѕРІР°С‚СЊ РєР°Р¶РґС‹Р№ РєР°РґСЂ.
//
// вЂћС‚Рѕ СЂРёСЃСѓРµРј:
// - вЂњСЂРё РѕС‚СЂРµР·РєР° РёР· (0,0,0) РІ +X, +Y, +Z.
// - Г·РІРµС‚Р° РїРѕ РєР»Р°СЃСЃРёРєРµ: X = РєСЂР°СЃРЅС‹Р№, Y = Р·РµР»Р„РЅС‹Р№, Z = СЃРёРЅРёР№ (РјРѕР¶РЅРѕ РїРѕРјРµРЅВ¤С‚СЊ РІ RenderSettings).
//
// В Р°Рє СЂР°Р±РѕС‚Р°РµС‚:
// 1) Build() Р§ РіРµРЅРµСЂРёСЂСѓРµРј 6 РІРµСЂС€РёРЅ (РїРѕ 2 РЅР° РѕСЃСЊ), Р·Р°РіСЂСѓР¶Р°РµРј РІ VBO/VAO.
// 2) Render(mvp) Р§ РІРєР»СЋС‡Р°РµРј LineShader, Р·Р°РґР°Р„Рј uMvp, СЂРёСЃСѓРµРј GL.Lines.
// 3) Dispose() Р§ РѕСЃРІРѕР±РѕР¶РґР°РµРј GL-СЂРµСЃСѓСЂСЃС‹.

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
        /// вЂ”РѕР·РґР°Р„Рј РѕСЃРё Рё РіСЂСѓР·РёРј РёС… РІ GPU. СњР±С‹С‡РЅРѕ РґРµР»Р°РµРј 1 СЂР°Р·.
        /// </summary>
        public void Build()
        {
            if (_built) return;

            // Ж’Р»РёРЅР° РѕСЃРµР№ (РЅР°РїСЂРёРјРµСЂ 5 РµРґРёРЅРёС†).
            // в‰€СЃР»Рё РІ RenderSettings РЅРµ РЅР°СЃС‚СЂРѕРµРЅРѕ Р§ РёСЃРїРѕР»СЊР·СѓРµРј РґРµС„РѕР»С‚.
            float len = _settings.AxesLength <= 0 ? 5.0f : _settings.AxesLength;

            // Г·РІРµС‚Р° РѕСЃРµР№. в‰€СЃР»Рё РЅРµ Р·Р°РґР°РЅС‹ Р§ РґРµС„РѕР»С‚РЅС‹Рµ "RGB".
            Vector4 colX = _settings.AxesColorX == Vector4.Zero
                ? new Vector4(1f, 0f, 0f, 1f)
                : _settings.AxesColorX;

            Vector4 colY = _settings.AxesColorY == Vector4.Zero
                ? new Vector4(0f, 1f, 0f, 1f)
                : _settings.AxesColorY;

            Vector4 colZ = _settings.AxesColorZ == Vector4.Zero
                ? new Vector4(0f, 0.6f, 1f, 1f) // С‡СѓС‚СЊ РїСЂРёВ¤С‚РЅРµРµ С‡РµРј С‡РёСЃС‚С‹Р№ 0,0,1
                : _settings.AxesColorZ;

            // С›С‹ СЂРёСЃСѓРµРј 3 Р»РёРЅРёРё:
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
        /// вЂ“РёСЃСѓРµРј РѕСЃРё (РµСЃР»Рё ShowAxes=true).
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
