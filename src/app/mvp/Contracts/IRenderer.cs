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
// На MVP-минимум нам нужно (под текущую GLView.cs):
// - Init(width,height): один раз настроить OpenGL и ресурсы после появления GL-контекста
// - Resize(width,height): обработать изменение размеров (viewport/проекция/камера)
// - Render(): нарисовать 1 кадр (внутри рендерер сам читает Camera/Scene/Settings, т.к. они у него уже "забинжены")
//
// Важно:
// - IRenderer НЕ владеет GLControl напрямую (это делает IGLView/реализация).
// - Но перед Init/Render/Resize должен быть текущий GL-контекст (MakeCurrent делает GLView/IGLView).
// - Рендерер почти всегда держит GL-ресурсы (шейдеры/буферы) => нужен Dispose().

using System;

namespace kuber3d.Contracts
{
    public interface IRenderer : IDisposable
    {
        /// <summary>
        /// Инициализация ресурсов рендера (шейдеры, VBO/VAO и т.д.).
        /// Вызывается 1 раз после создания GL-контекста.
        /// </summary>
        void Init(int width, int height);

        /// <summary>
        /// Сообщаем о смене размера окна/вьюпорта.
        /// Обычно: GL.Viewport + пересчёт проекции/аспекта.
        /// </summary>
        void Resize(int width, int height);

        /// <summary>
        /// Рисуем 1 кадр.
        /// На MVP-минимуме входные данные (камера/сцена/настройки) уже находятся внутри рендерера
        /// (через конструктор или Bind(...) на уровне Presenter-а).
        /// </summary>
        void Render();
    }
}
