// mvp/Rendering/LineShader.cs
//
// LineShader — минимальный шейдер для рисования линий (сетка, оси, любые отрезки).
//
// Почему отдельный класс:
// - Чтобы GridRenderer/AxesRenderer не занимались компиляцией шейдеров.
// - Чтобы один раз собрать программу и потом просто Use() + SetMvp().
//
// Что шейдер делает:
// - Принимает позицию вершины (vec3) и цвет (vec4)
// - Умножает позицию на матрицу uMvp
// - Передаёт цвет во фрагментный шейдер
//
// MVP-минимум:
// - один uniform: uMvp
// - два атрибута: aPosition (location 0), aColor (location 1)
//
// Важно про "LocationPosition/LocationColor":
// - Мы фиксируем layout(location=0/1) прямо в GLSL.
// - Тогда GridRenderer/AxesRenderer могут спокойно делать VertexAttribPointer
//   по этим индексам без лишних запросов.
//
// Если компиляция упала:
// - Build() бросит исключение с текстом логов.
// - Это удобно, чтобы сразу видеть проблему в консоли/Output.

using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace kuber3d.Rendering
{
    public sealed class LineShader : IDisposable
    {
        // Индексы атрибутов (совпадают с layout(location=...)) в GLSL.
        // GridRenderer/AxesRenderer используют их в VertexAttribPointer.
        public int LocationPosition => 0;
        public int LocationColor => 1;

        // OpenGL id программы и шейдеров
        private int _program;
        private int _vs;
        private int _fs;

        // location uniform'а uMvp
        private int _uMvpLocation = -1;

        private bool _built;
        private bool _disposed;

        /// <summary>
        /// Компилируем и линкуем шейдерную программу.
        /// Вызываем один раз при старте 3D.
        /// </summary>
        public void Build()
        {
            if (_built) return;

            // Вершинный шейдер:
            // - берет aPosition, aColor
            // - умножает позицию на uMvp
            // - передает цвет дальше
            string vsSrc = @"
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;

uniform mat4 uMvp;

out vec4 vColor;

void main()
{
    vColor = aColor;
    gl_Position = uMvp * vec4(aPosition, 1.0);
}";

            // Фрагментный шейдер:
            // - просто выводит интерполированный цвет
            string fsSrc = @"
#version 330 core

in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
}";

            // 1) Компилируем VS/FS
            _vs = CompileShader(ShaderType.VertexShader, vsSrc);
            _fs = CompileShader(ShaderType.FragmentShader, fsSrc);

            // 2) Линкуем программу
            _program = GL.CreateProgram();
            GL.AttachShader(_program, _vs);
            GL.AttachShader(_program, _fs);

            GL.LinkProgram(_program);

            // Проверяем линковку
            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0)
            {
                string log = GL.GetProgramInfoLog(_program);
                throw new InvalidOperationException($"LineShader link failed: {log}");
            }

            // 3) После линковки шейдеры можно отсоединить/удалить (программа уже собрана)
            GL.DetachShader(_program, _vs);
            GL.DetachShader(_program, _fs);
            GL.DeleteShader(_vs);
            GL.DeleteShader(_fs);
            _vs = 0;
            _fs = 0;

            // 4) Находим uniform location
            _uMvpLocation = GL.GetUniformLocation(_program, "uMvp");
            if (_uMvpLocation < 0)
            {
                // Не критично, но значит uniform не найден (например оптимизировался)
                // В нашем коде он должен быть.
                throw new InvalidOperationException("LineShader: uniform 'uMvp' not found.");
            }

            _built = true;
        }

        /// <summary>
        /// Активируем программу.
        /// </summary>
        public void Use()
        {
            if (!_built) throw new InvalidOperationException("LineShader: Build() must be called before Use().");
            GL.UseProgram(_program);
        }

        /// <summary>
        /// Устанавливаем матрицу uMvp.
        /// </summary>
        public void SetMvp(in Matrix4 mvp)
        {
            if (!_built) throw new InvalidOperationException("LineShader: Build() must be called before SetMvp().");

            // В OpenTK Matrix4 хранится в виде float[16].
            // False => не транспонировать (OpenGL ожидает column-major).
            GL.UniformMatrix4(_uMvpLocation, transpose: false, ref UnsafeAsRef(mvp));
        }

        /// <summary>
        /// Вспомогательная штука: OpenTK требует ref Matrix4,
        /// а мы держим in Matrix4 (чтобы не копировать).
        /// </summary>
        private static ref Matrix4 UnsafeAsRef(in Matrix4 m)
        {
            // Безопасно в контексте вызова UniformMatrix4,
            // т.к. Matrix4 — struct и живёт на стеке вызывающего метода.
            return ref System.Runtime.CompilerServices.Unsafe.AsRef(in m);
        }

        private static int CompileShader(ShaderType type, string src)
        {
            int sh = GL.CreateShader(type);
            GL.ShaderSource(sh, src);
            GL.CompileShader(sh);

            GL.GetShader(sh, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0)
            {
                string log = GL.GetShaderInfoLog(sh);
                GL.DeleteShader(sh);
                throw new InvalidOperationException($"{type} compile failed: {log}");
            }

            return sh;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_program != 0)
            {
                GL.DeleteProgram(_program);
                _program = 0;
            }

            // На всякий случай (если Build не дошел до удаления)
            if (_vs != 0) { GL.DeleteShader(_vs); _vs = 0; }
            if (_fs != 0) { GL.DeleteShader(_fs); _fs = 0; }

            _built = false;
            _uMvpLocation = -1;
        }
    }
}
