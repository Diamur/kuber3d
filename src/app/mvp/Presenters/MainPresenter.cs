// mvp/Presenters/MainPresenter.cs
//
// MainPresenter — главный Presenter приложения (центр управления в MVP).
//
// Роли (очень важно понять логику):
// 1) FormMain (View) — ТОЛЬКО UI:
//    - содержит кнопки/чекбоксы/панели
//    - генерирует события: "нажали 3D", "вкл/выкл сетку", "вкл/выкл оси"
//    - предоставляет Presenter'у доступ к pnlViewport (куда встраивать 3D-контрол)
//
// 2) MainPresenter — "дирижёр":
//    - подписывается на события View
//    - решает "что делать" при кликах
//    - не знает ничего про OpenGL напрямую (ни GL, ни шейдеры)
//    - управляет 3D через ViewportPresenter
//
// 3) ViewportPresenter — "внутренний" презентер 3D-вьюпорта:
//    - создаёт GLView и встраивает его в pnlViewport
//    - запускает/останавливает рендер
//    - прокидывает настройки сетки/осей в рендер
//    - подключает MouseController для ПКМ/ЛКМ/колеса
//
// MVP-минимум по требованиям:
// - Нажатие btnStart3D -> запускаем 3D-визор
// - chkGrid -> включает/выключает сетку
// - chkAxes -> включает/выключает оси
// - мышь (ПКМ/ЛКМ/колесо) — не здесь, а внутри ViewportPresenter/MouseController
//
// Важно:
// MainPresenter должен быть "тонким": подписки + команды ViewportPresenter.

// mvp/Presenters/MainPresenter.cs

using System;
using kuber3d.Contracts;
using kuber3d.Core;
using kuber3d.Models;
using kuber3d.Rendering;

namespace kuber3d.Presenters
{
    public sealed class MainPresenter : IMainPresenter
    {
        private readonly IMainView _view;

        private readonly SceneModel _scene;
        private readonly RenderSettings _settings;
        private readonly Camera _camera;
        private readonly ViewportPresenter _viewport;

        private bool _is3DStarted;

        public MainPresenter(IMainView view)
        {
            _view = view;

            // Минимальные модели/настройки
            _scene = new SceneModel();
            _settings = new RenderSettings();
            _camera = new Camera();

            _viewport = new ViewportPresenter(_scene, _settings, _camera);

            // Подписки на события View — строго по IMainView
            _view.Start3DClicked += (_, __) => Start3D();
            _view.GridToggled += (_, __) => SetGridEnabled(_view.IsGridEnabled);
            _view.AxesToggled += (_, __) => SetAxesEnabled(_view.IsAxesEnabled);
        }

        public void Init()
        {
            // Синхронизируем настройки по состоянию чекбоксов
            SetGridEnabled(_view.IsGridEnabled);
            SetAxesEnabled(_view.IsAxesEnabled);

            _is3DStarted = false;
            _view.IsStart3DEnabled = true;
        }

        public void Start3D()
        {
            if (_is3DStarted)
                return;

            // 1) Создаём GLView/Renderer (внутри viewport)
            var glView = _viewport.EnsureViewportCreated();

            // 2) Встраиваем GLView в pnlViewport (это обязанность View)
            _view.AttachViewport(glView);

            // 3) Применяем настройки
            _viewport.SetGridVisible(_settings.ShowGrid);
            _viewport.SetAxesVisible(_settings.ShowAxes);

            // 4) Стартуем рендер
            _viewport.Start();

            _is3DStarted = true;
            _view.IsStart3DEnabled = false;
        }

        public void SetGridEnabled(bool enabled)
        {
            _viewport.SetGridVisible(enabled);
            if (_is3DStarted) _viewport.RequestRender();
        }

        public void SetAxesEnabled(bool enabled)
        {
            _viewport.SetAxesVisible(enabled);
            if (_is3DStarted) _viewport.RequestRender();
        }

        public void Dispose()
        {
            _viewport.Dispose();
        }
    }
}
