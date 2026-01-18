// mvp\Contracts\IRenderer.cs
//
// Контракт "рендерера сцены".
//
// Идея MVP:
// - View отвечает за UI (FormMain, кнопки, чекбоксы, размещение контролов).
// - Presenter связывает UI и модели.
// - Renderer — отдельный слой, который умеет рисовать сцену (OpenGL) по данным моделей.
//
// Зачем интерфейс:
// - Presenter работает с IRenderer, не зная реализацию (SceneRenderer на OpenGL).
// - Можно подменить рендерер (например, на заглушку/тестовый/другую графику).
//
// На MVP-минимум нам нужно:
// - Init(): один раз настроить OpenGL (clear color, depth test, shader, буферы и т.п.)
// - Resize(): обновить матрицу камеры/viewport/зависимости от размера
// - Render(): нарисовать кадр, учитывая флаги сетки/осей, состояние камеры и сцену
//
// Важно:
// - IRenderer НЕ владеет GLControl напрямую (это делает IGLView/реализация).
// - Но перед Render/Init обычно нужен текущий контекст (MakeCurrent делает Presenter через IGLView).

using kuber3d.Models;
using kuber3d.Rendering;

namespace kuber3d.Contracts
{
    public interface IRenderer
    {
        /// <summary>
        /// Инициализация ресурсов рендера (шейдеры, VBO/VAO и т.д.).
        /// Вызывается 1 раз после создания GL-контекста.
        /// </summary>
        void Init();

        /// <summary>
        /// Сообщаем о смене размера окна/вьюпорта.
        /// Обычно: GL.Viewport + Camera/Projection пересчёт.
        /// </summary>
        void Resize(int width, int height);

        /// <summary>
        /// Рисуем 1 кадр.
        /// На вход: текущее состояние камеры, сцены и настройка визуализации (оси/сетка).
        /// </summary>
        void Render(CameraModel camera, SceneModel scene, RenderSettings settings);
    }
}
