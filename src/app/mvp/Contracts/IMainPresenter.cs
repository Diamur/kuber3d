// mvp\Contracts\IMainPresenter.cs
//
// - Initialize():      
// FormMain  Program.cs   ,  Presenter     Initialize().
        void Initialize();
        /// <summary>
        /// Связывает Presenter с View:
        /// - подписывает Presenter на события View (кнопка/чекбоксы)
        /// - подготавливает начальное состояние (например, синхронизация чекбоксов с настройками)
        ///
        /// Этот метод обычно вызывается один раз при старте приложения.
        /// </summary>
        void Init();

        /// <summary>
        /// Запуск 3D-вьюпорта.
        /// Должен быть безопасным при повторном вызове (например, если кнопка нажата 2 раза).
        ///
        /// Что обычно делает внутри:
        /// - создаёт IGLView (реальную WinForms/GL реализацию)
        /// - создаёт/инициализирует IRenderer (SceneRenderer и т.д.)
        /// - передаёт IGLView в View.AttachViewport()
        /// - запускает цикл рендера (внутри IGLView или через таймер)
        /// </summary>
        void Start3D();

        /// <summary>
        /// Включение/выключение сетки.
        /// Обычно вызывается обработчиком события чекбокса "Сетка".
        /// </summary>
        void SetGridEnabled(bool enabled);

        /// <summary>
        /// Включение/выключение осей.
        /// Обычно вызывается обработчиком события чекбокса "Оси".
        /// </summary>
        void SetAxesEnabled(bool enabled);
    }
}
