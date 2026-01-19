// mvp/Rendering/SceneRenderer.cs
//
// SceneRenderer — центральный рендерер сцены (MVP-слой Rendering).
//
// Минимальная задача для нашего MVP-минимума:
// - Старт 3D по кнопке (инициализация OpenGL + запуск отрисовки)
// - Вкл/выкл сетку (RenderSettings.ShowGrid)
// - Вкл/выкл оси (RenderSettings.ShowAxes)
// - Управление камерой мышью: ПКМ вращение, ЛКМ панорамирование, колесо зум
//
// Важно: SceneRenderer не должен знать про WinForms-форму напрямую.
// Он получает абстракцию IGLView, которая умеет:
// - давать размеры вьюпорта
// - давать контекст GL (если нужно) и вызывать SwapBuffers
// - отдавать события ввода (или ViewportPresenter прокидывает ввод в MouseController)
//
// У нас архитектура такая:
// FormMain (View) -> Presenter -> GLView (контрол) -> SceneRenderer (IRenderer)
//
// SceneRenderer:
// - держит RenderSettings (флаги сетки/осей и прочие параметры)
// - держит Camera (матрицы View/Proj + состояние камеры)
// - держит MouseController (или принимает его извне) — чтобы крутить/двигать/зумить
// - держит простые "хелперы" GridRenderer и AxesRenderer
// - держит LineShader (один шейдер на линии)
//
// В дальнейшем сюда добавятся:
// - рендер объектов сцены (точки/линии/плоскости/меши)
// - выбор объектов мышью (raycast / picking)
// - gizmo (перемещение/вращение/масштаб)
// - отображение текста, осей в углу, и т.д.
//
// В MVP мы разделяем обязанности строго:
// - View (FormMain + GLView) — только UI и события WinForms/OpenTK.
// - Presenter (ViewportPresenter) — собирает всё вместе и решает, что когда запускать.
// - MouseController — преобразует мышь -> изменения камеры, и просит перерисовать.
// - SceneRenderer — только рисует (Render) и инициализирует GL (Initialize).
//
// Минимум, который нам нужен сейчас:
// 1) При старте 3D: Initialize() создаёт шейдер линий + рендереры сетки и осей.
// 2) Каждый кадр Render():
//    - очищаем экран
//    - считаем View/Proj от камеры
//    - рисуем сетку (если ShowGrid)
//    - рисуем оси   (если ShowAxes)
//
// Важно: тут НЕТ логики мыши. Камеру крутит MouseController.
// Тут НЕТ WinForms кода. Только OpenGL и математика.

using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using kuber3d.Contracts;
using kuber3d.Core;
using kuber3d.Models;

namespace kuber3d.Rendering
{
    public sealed class SceneRenderer : IRenderer
    {
        // ------------------------------------------------------------
        // Связанные объекты (передаёт Presenter)
        // ------------------------------------------------------------

        private Camera? _camera;
        private SceneModel? _scene;
        private RenderSettings? _settings;

        // ------------------------------------------------------------
        // OpenGL-ресурсы (создаём один раз, освобождаем в Dispose)
        // ------------------------------------------------------------

        private LineShader? _lineShader;
        private GridRenderer? _grid;
        private AxesRenderer? _axes;

        // ------------------------------------------------------------
        // Состояние
        // ------------------------------------------------------------

        private bool _initialized;

        /// <summary>
        /// Presenter вызывает Bind() один раз, когда создаёт/подключает 3D-систему.
        /// Мы сохраняем ссылки на данные, которые будем читать при Render().
        /// </summary>
        public void Bind(Camera camera, SceneModel scene, RenderSettings settings)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Инициализация OpenGL-ресурсов.
        /// Вызывается один раз после того, как GLView готов и контекст существует.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            if (_camera == null || _scene == null || _settings == null)
                throw new InvalidOperationException("SceneRenderer: Bind(camera, scene, settings) must be called before Initialize().");

            // 1) Базовые настройки OpenGL для 3D
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            // Сглаживание линий (не везде одинаково работает, но для MVP ок)
            GL.Enable(EnableCap.LineSmooth);

            // 2) Создаём шейдер для линий
            _lineShader = new LineShader();
            _lineShader.Build(); // компиляция/линковка + проверка ошибок

            // 3) Создаём рендереры сетки и осей
            // Они используют LineShader, чтобы рисовать линии.
            _grid = new GridRenderer(_settings, _lineShader);
            _axes = new AxesRenderer(_settings, _lineShader);

            // 4) Готовим их буферы (VBO/VAO) заранее
            _grid.Build();
            _axes.Build();

            _initialized = true;
        }

        /// <summary>
        /// Сообщаем рендереру о новом размере вьюпорта.
        /// В MVP здесь главное — обновить камеру (aspect).
        /// </summary>
        public void Resize(int width, int height)
        {
            if (_camera == null) return;

            // Защита от нулевых размеров (бывает при сворачивании)
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            _camera.Resize(width, height);
            GL.Viewport(0, 0, width, height);
        }

        /// <summary>
        /// Рисуем один кадр.
        /// Важно: Camera уже обновлена MouseController'ом (он крутит yaw/pitch, pan, zoom).
        /// </summary>
        public void Render()
        {
            if (!_initialized) return;
            if (_camera == null || _settings == null) return;

            // 1) Очищаем экран (фон)
            // Цвет пока фиксированный — потом можно вынести в RenderSettings.
            GL.ClearColor(0.10f, 0.10f, 0.12f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // 2) Считаем матрицы камеры
            // Важно: порядок умножения зависит от того, как написан LineShader.
            // В нашем MVP договор такой: shader принимает uView и uProj отдельно,
            // или один uMvp = proj * view.
            Matrix4 view = _camera.GetViewMatrix();
            Matrix4 proj = _camera.GetProjectionMatrix();

            // Наиболее типичный порядок: proj * view
            Matrix4 vp = proj * view;

            // 3) Рисуем сетку/оси по флагам (чекбоксы меняют эти флаги)
            if (_settings.ShowGrid)
                _grid?.Render(vp);

            if (_settings.ShowAxes)
                _axes?.Render(vp);

            // 4) Позже здесь появится рендер объектов сцены:
            // foreach (var obj in _scene!.Objects) { ... }
        }

        /// <summary>
        /// Освобождение OpenGL-ресурсов.
        /// Вызывать когда GL-контекст ещё существует (обычно при закрытии формы).
        /// </summary>
        public void Dispose()
        {
            _axes?.Dispose();
            _grid?.Dispose();
            _lineShader?.Dispose();

            _axes = null;
            _grid = null;
            _lineShader = null;

            _initialized = false;
        }
    }
}
