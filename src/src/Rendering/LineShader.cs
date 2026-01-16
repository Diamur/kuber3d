using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace kuber3d.Rendering
{
    internal sealed class LineShader : IDisposable
    {
        public int ProgramId { get; private set; }
        private int _uView;
        private int _uProj;

        public void Create()
        {
            const string vs = @"
#version 330 core
layout(location=0) in vec3 aPos;
layout(location=1) in vec3 aCol;
uniform mat4 uView;
uniform mat4 uProj;
out vec3 vCol;
void main(){
    vCol = aCol;
    gl_Position = uProj * uView * vec4(aPos, 1.0);
}";
            const string fs = @"
#version 330 core
in vec3 vCol;
out vec4 FragColor;
void main(){
    FragColor = vec4(vCol, 1.0);
}";

            int v = Compile(ShaderType.VertexShader, vs);
            int f = Compile(ShaderType.FragmentShader, fs);

            ProgramId = GL.CreateProgram();
            GL.AttachShader(ProgramId, v);
            GL.AttachShader(ProgramId, f);
            GL.LinkProgram(ProgramId);

            GL.GetProgram(ProgramId, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetProgramInfoLog(ProgramId));

            GL.DeleteShader(v);
            GL.DeleteShader(f);

            _uView = GL.GetUniformLocation(ProgramId, "uView");
            _uProj = GL.GetUniformLocation(ProgramId, "uProj");
        }

        public void Use(Matrix4 view, Matrix4 proj)
        {
            GL.UseProgram(ProgramId);
            GL.UniformMatrix4(_uView, false, ref view);
            GL.UniformMatrix4(_uProj, false, ref proj);
        }

        private static int Compile(ShaderType type, string src)
        {
            int id = GL.CreateShader(type);
            GL.ShaderSource(id, src);
            GL.CompileShader(id);
            GL.GetShader(id, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(id));
            return id;
        }

        public void Dispose()
        {
            if (ProgramId != 0)
            {
                GL.DeleteProgram(ProgramId);
                ProgramId = 0;
            }
        }
    }
}
