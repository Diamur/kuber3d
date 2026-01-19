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

// mvp/Presenters/ViewportPresenter.cs

using System;
using kuber3d.Contracts;
using kuber3d.Core;
using kuber3d.Input;
using kuber3d.Models;
using kuber3d.Rendering;

namespace kuber3d.Presenters
{
    public sealed class ViewportPresenter : IDisposable
    {
        private readonly SceneModel _scene;
        private readonly RenderSettings _settings;
        private readonly Camera _camera;

        private IGLView? _glView;
        private IRenderer? _renderer;
        private MouseController? _mouse;

        private bool _started;

        public ViewportPresenter(SceneModel scene, RenderSettings settings, Camera camera)
        {
            _scene = scene;
            _settings = settings;
            _camera = camera;
        }

        /// <summary>
        /// Создаёт GLView и Renderer (если ещё не созданы) и возвращает GLView,
        /// чтобы MainView мог встроить его в pnlViewport через AttachViewport().
        /// </summary>
        public IGLView EnsureViewportCreated()
        {
            if (_glView != null)
                return _glView;

            // Реальная реализация IGLView будет в mvp/Views/GLView.cs
            _glView = new kuber3d.Views.GLView();

            // Реальная реализация IRenderer будет в mvp/Rendering/SceneRenderer.cs
            // (она должна реализовывать IRenderer и брать scene/camera/settings через конструктор)
            var sceneRenderer = new SceneRenderer();
            sceneRenderer.Bind(_camera, _scene, _settings);
            _renderer = sceneRenderer; // _renderer пусть остаётся IRenderer



            // Подключаем мышь к событиям IGLView (у нас они есть в интерфейсе)
            _mouse = new MouseController(
                glView: _glView,
                camera: _camera,
                settings: _settings,
                requestRender: RequestRender
            );

            return _glView;
        }

        public void Start()
        {
            if (_started) return;

            var view = EnsureViewportCreated();

            if (_renderer == null)
                throw new InvalidOperationException("Renderer was not created.");

            // Запуск рендера: внутри GLView будет таймер/луп и вызовы IRenderer.Init/Resize/Render
            view.StartRendering(_renderer);

            _started = true;
            RequestRender();
        }

        public void Stop()
        {
            if (!_started) return;

            _glView?.StopRendering();
            _started = false;
        }

        public void SetGridVisible(bool isVisible) => _settings.ShowGrid = isVisible;
        public void SetAxesVisible(bool isVisible) => _settings.ShowAxes = isVisible;

        public void RequestRender() => _glView?.RequestRender();

        public void Dispose()
        {
            Stop();

            _mouse?.Dispose();
            _mouse = null;

            _renderer?.Dispose();
            _renderer = null;

            _glView?.Dispose();
            _glView = null;
        }
    }
}
