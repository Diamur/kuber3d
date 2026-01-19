// mvp/Presenters/ViewportPresenter.cs
//
// ViewportPresenter — презентер "правой части" (вьюпорта / pnlViewport).
// Его задача: поднять 3D-визор (GLView) внутри pnlViewport, связать:
//
// 1) Camera (Core)           — текущее положение/поворот/дистанция (матрицы View/Proj)
// 2) SceneModel (Model)      — данные сцены (пока минимум, позже объекты, линии, плоскости)
// 3) RenderSettings (Rendering) — флаги: сетка/оси + чувствительность мыши
// 4) SceneRenderer (Rendering)  — кто рисует (сетка, оси, потом объекты)
// 5) MouseController (Input)    — мышь:
//     - ПКМ: orbit (поворот)
//     - ЛКМ: pan   (смещение)
//     - колесо: zoom (масштаб/дистанция)
//
// Важно по MVP:
// - ViewportPresenter НЕ знает про chkGrid/chkAxes напрямую.
//   Эти галки живут в FormMain и их состояние передаётся сюда через методы SetGridVisible/SetAxesVisible.
// - ViewportPresenter НЕ знает про кнопку btnStart3D напрямую.
//   Нажатие кнопки обрабатывает MainPresenter, а он уже вызывает AttachTo/Start.
//
// Минимум функционала после запуска 3D:
// - старт 3D-визора
// - вкл/выкл сетку
// - вкл/выкл оси
// - мышь: orbit/pan/zoom

using System;
using System.Windows.Forms;
using kuber3d.Contracts;
using kuber3d.Core;
using kuber3d.Input;
using kuber3d.Models;
using kuber3d.Rendering;

namespace kuber3d.Presenters
{
    public sealed class ViewportPresenter : IDisposable
    {
        // ----------------------------
        // Состояние "железных" объектов
        // ----------------------------

        private IGLView? _glView;                 // сам 3D-контрол (создаётся при старте)
        private Control? _host;                   // куда встраиваем (FormMain.pnlViewport)

        // ----------------------------
        // Данные (Model) и логика (Core/Rendering/Input)
        // ----------------------------

        private readonly SceneModel _scene;
        private readonly RenderSettings _settings;
        private readonly Camera _camera;
        private readonly SceneRenderer _renderer;

        private MouseController? _mouse;

        // Флаг, чтобы не стартовать второй раз
        private bool _started;

        public ViewportPresenter(SceneModel scene, RenderSettings settings, Camera camera, SceneRenderer renderer)
        {
            _scene = scene;
            _settings = settings;
            _camera = camera;
            _renderer = renderer;
        }

        // =========================================================
        // 1) "Встроить" GLView в pnlViewport (но ещё не стартовать)
        // =========================================================
        public void AttachTo(Control viewportHost)
        {
            // На случай, если вызвали повторно — просто запомним host.
            // (реально повторно AttachTo мы не хотим делать без Dispose/Stop,
            //  но сейчас не усложняем)
            _host = viewportHost;

            // Если GLView уже создан — ничего не пересоздаём.
            if (_glView != null)
                return;

            // 1) Создаём 3D-вью.
            // Реальная реализация будет в mvp/Views/GLView.cs
            _glView = new Views.GLView();

            // 2) Встраиваем его в панель.
            // Идея: pnlViewport.Controls.Add(glControl), Dock=Fill.
            _glView.AttachTo(viewportHost);

            // 3) Важно: при изменении размера хоста мы должны пересчитать камеру.
            // Обычно GLView сам поймает resize, но нам нужно обновить aspect.
            viewportHost.Resize += Host_Resize;
        }

        // =========================================================
        // 2) Старт отрисовки (после кнопки 3D)
        // =========================================================
        public void Start()
        {
            if (_started)
                return;

            if (_glView == null || _host == null)
                throw new InvalidOperationException("ViewportPresenter.Start() вызван до AttachTo(host).");

            // 1) Инициализация камеры под текущий размер (aspect, матрица проекции)
            ApplyViewportSizeToCamera();

            // 2) Подключаем рендерер к данным.
            // Renderer читает _settings.ShowGrid/ShowAxes и решает, рисовать ли элементы.
            _renderer.Bind(_camera, _scene, _settings);

            // 3) Поднимаем контроллер мыши:
            //    - подписывается на события мыши у GLView
            //    - меняет _camera (orbit/pan/zoom)
            //    - просит перерисовку через RequestRender()
            _mouse = new MouseController(
                inputSource: _glView.InputControl,     // откуда брать MouseDown/Move/Wheel
                camera: _camera,
                settings: _settings,
                requestRender: RequestRender
            );

            // 4) Запускаем рендер-луп внутри GLView (таймер/Invalidate — как реализуешь в GLView.cs)
            _glView.StartRendering(_renderer);

            _started = true;

            // 5) Первый кадр
            RequestRender();
        }

        // =========================================================
        // 3) Управление флагами сетка/оси (дергает MainPresenter)
        // =========================================================
        public void SetGridVisible(bool isVisible)
        {
            // Чекбокс "Сетка" -> этот флаг
            _settings.ShowGrid = isVisible;
        }

        public void SetAxesVisible(bool isVisible)
        {
            // Чекбокс "Оси" -> этот флаг
            _settings.ShowAxes = isVisible;
        }

        // =========================================================
        // 4) Перерисовка (чтобы не лазить внутрь GLView снаружи)
        // =========================================================
        public void RequestRender()
        {
            _glView?.RequestRender();
        }

        // =========================================================
        // 5) Resize хоста -> обновляем камеру и перерисовываем
        // =========================================================
        private void Host_Resize(object? sender, EventArgs e)
        {
            if (_host == null)
                return;

            // Даже если 3D ещё не стартовали — размер камеры всё равно полезно обновлять.
            ApplyViewportSizeToCamera();

            // Если 3D уже запущен — перерисуем
            if (_started)
                RequestRender();
        }

        private void ApplyViewportSizeToCamera()
        {
            if (_host == null)
                return;

            // Берём текущий размер клиентской области (без рамок)
            int w = Math.Max(1, _host.ClientSize.Width);
            int h = Math.Max(1, _host.ClientSize.Height);

            _camera.Resize(w, h);
        }

        // =========================================================
        // 6) Освобождение ресурсов
        // =========================================================
        public void Dispose()
        {
            // Отписка resize
            if (_host != null)
                _host.Resize -= Host_Resize;

            // Остановить рендеринг
            _glView?.StopRendering();

            // Мышь отписывается внутри MouseController.Dispose (сделаем в том файле)
            _mouse?.Dispose();
            _mouse = null;

            // GLView уничтожаем (внутри будет Dispose OpenGL ресурсов)
            _glView?.Dispose();
            _glView = null;

            _host = null;
            _started = false;
        }
    }
}
