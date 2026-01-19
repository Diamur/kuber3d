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

using System;
using kuber3d.Contracts;

namespace kuber3d.Presenters
{
    public sealed class MainPresenter : IMainPresenter
    {
        private readonly IMainView _view;

        // Внутренний презентер, который отвечает именно за 3D-визор.
        private readonly ViewportPresenter _viewport;

        // Чтобы не запускать 3D повторно (и не плодить GL-контролы)
        private bool _is3DStarted;

        public MainPresenter(IMainView view)
        {
            _view = view;

            // Создаём ViewportPresenter здесь.
            // Почему так удобно:
            // - FormMain не знает про ViewportPresenter вообще
            // - Program.cs не занимается DI/проволокой
            _viewport = new ViewportPresenter();

            // Подписываемся на события View (которые FormMain поднимает наружу).
            // ВНИМАНИЕ: имена событий/свойств должны совпадать с тем, что у тебя в IMainView.
            _view.Start3DRequested += OnStart3DRequested;
            _view.GridToggled += OnGridToggled;
            _view.AxesToggled += OnAxesToggled;
        }

        /// <summary>
        /// Вызывается из FormMain.OnLoad() (или сразу после создания формы).
        /// Тут можно выставить начальные настройки, не включая 3D.
        /// </summary>
        public void Initialize()
        {
            // Проставляем настройки видимости по состоянию чекбоксов.
            // Даже если 3D ещё не стартовал — это норм:
            // ViewportPresenter запомнит значения и применит при старте.
            _viewport.SetGridVisible(_view.IsGridEnabled);
            _viewport.SetAxesVisible(_view.IsAxesEnabled);

            _is3DStarted = false;
        }

        // ============================
        // Обработчики событий UI
        // ============================

        /// <summary>
        /// Нажали кнопку "3D".
        /// Тут мы создаём/встраиваем GLView и запускаем рендер.
        /// </summary>
        private void OnStart3DRequested(object? sender, EventArgs e)
        {
            if (_is3DStarted)
            {
                // MVP-поведение: повторное нажатие ничего не делает.
                // Позже можно превратить в "Reset camera" или "Restart".
                return;
            }

            // 1) Встроить 3D-контрол в правую панель (pnlViewport).
            _viewport.AttachTo(_view.ViewportHost);

            // 2) Применить текущие настройки (чекбоксы).
            _viewport.SetGridVisible(_view.IsGridEnabled);
            _viewport.SetAxesVisible(_view.IsAxesEnabled);

            // 3) Запустить рендер-цикл (таймер/loop внутри ViewportPresenter).
            _viewport.Start();

            _is3DStarted = true;
        }

        /// <summary>
        /// Переключили "Сетка".
        /// </summary>
        private void OnGridToggled(object? sender, EventArgs e)
        {
            // Всегда сохраняем значение в ViewportPresenter
            _viewport.SetGridVisible(_view.IsGridEnabled);

            // Если 3D уже запущен — просим перерисовать кадр
            if (_is3DStarted)
                _viewport.RequestRender();
        }

        /// <summary>
        /// Переключили "Оси".
        /// </summary>
        private void OnAxesToggled(object? sender, EventArgs e)
        {
            _viewport.SetAxesVisible(_view.IsAxesEnabled);

            if (_is3DStarted)
                _viewport.RequestRender();
        }

        // ============================
        // Завершение работы
        // ============================

        public void Dispose()
        {
            // Отписываемся от событий, чтобы не было утечек ссылок.
            _view.Start3DRequested -= OnStart3DRequested;
            _view.GridToggled -= OnGridToggled;
            _view.AxesToggled -= OnAxesToggled;

            // Останавливаем 3D и освобождаем ресурсы (GL контекст и т.п.).
            // Реализация внутри ViewportPresenter.
            _viewport.Dispose();
        }
    }
}
