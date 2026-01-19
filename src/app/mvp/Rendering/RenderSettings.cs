// mvp/Rendering/RenderSettings.cs
//
// MVP-настройки рендера. Это "единое место", где лежат флаги и параметры,
// влияющие на отображение (сетка/оси) и на управление камерой (скорости мыши).
//
// Важно для MVP:
// - чекбокс "Сетка" напрямую меняет ShowGrid
// - чекбокс "Оси"   напрямую меняет ShowAxes
// - MouseController использует OrbitSpeed/PanSpeed/ZoomSpeed
//
// Никаких зависимостей от WinForms / UI здесь нет.
// Это чистая конфигурация, которую Presenter может менять, а Renderer читать.

using OpenTK.Mathematics;

namespace kuber3d.Rendering
{
    /// <summary>
    /// Настройки рендера и управления камерой для MVP.
    ///
    /// Как это работает в MVP (простая цепочка):
    /// 1) View (чекбоксы/кнопки) -> сообщает Presenter-у "что изменилось"
    /// 2) Presenter             -> меняет значения здесь (RenderSettings)
    /// 3) Renderer              -> читает эти значения и решает, что рисовать
    /// 4) MouseController        -> читает скорости и двигает/вращает/зумит камеру
    ///
    /// Почему это удобно:
    /// - все "крутилки" и флаги в одном месте
    /// - легко добавлять новые параметры (например: толщина линий, цвет фона)
    /// - легко делать пресеты (на конференцию — крупнее, дома — тоньше)
    /// </summary>
    public class RenderSettings
    {
        // ============================
        // ВИЗУАЛЬНЫЕ ФЛАГИ (UI)
        // ============================

        /// <summary>
        /// Показывать сетку (Grid) в сцене.
        /// Чекбокс "Сетка" должен управлять этим флагом.
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// Показывать оси координат (Axes) в сцене.
        /// Чекбокс "Оси" должен управлять этим флагом.
        /// </summary>
        public bool ShowAxes { get; set; } = true;

        // ============================
        // НАСТРОЙКИ СЕТКИ/ОСЕЙ (MVP)
        // ============================

        /// <summary>
        /// Половина размера сетки в "клетках".
        /// Например 20 => линии пойдут от -20 до +20 по X/Z.
        /// </summary>
        public int GridHalfSize { get; set; } = 20;

        /// <summary>
        /// Шаг сетки. 1.0 = каждая линия через 1 единицу.
        /// </summary>
        public float GridStep { get; set; } = 1.0f;

        /// <summary>
        /// Длина осей (X/Y/Z).
        /// </summary>
        public float AxesLength { get; set; } = 5.0f;

        /// <summary>
        /// Цвет оси X.
        /// </summary>
        public Vector4 AxesColorX { get; set; } = Vector4.Zero;

        /// <summary>
        /// Цвет оси Y.
        /// </summary>
        public Vector4 AxesColorY { get; set; } = Vector4.Zero;

        /// <summary>
        /// Цвет оси Z.
        /// </summary>
        public Vector4 AxesColorZ { get; set; } = Vector4.Zero;

        // ============================
        // УПРАВЛЕНИЕ КАМЕРОЙ (мышь)
        // ============================

        /// <summary>
        /// Скорость вращения (ПКМ).
        /// Коэффициент: чем больше — тем быстрее "орбитим" вокруг цели.
        /// </summary>
        public float OrbitSpeed { get; set; } = 0.35f;

        /// <summary>
        /// Скорость панорамирования (ЛКМ).
        /// Коэффициент: чем больше — тем сильнее смещение при перетаскивании.
        /// </summary>
        public float PanSpeed { get; set; } = 0.01f;

        /// <summary>
        /// Скорость зума (колесо).
        /// Коэффициент: чем больше — тем быстрее приближаемся/удаляемся за один "тик".
        /// </summary>
        public float ZoomSpeed { get; set; } = 0.12f;

        // ============================
        // КАМЕРА (параметры проекции)
        // ============================

        /// <summary>
        /// Вертикальный угол обзора (FOV) в градусах.
        /// Для 3D-видов обычно 45..75.
        /// </summary>
        public float FovDeg { get; set; } = 60.0f;

        /// <summary>
        /// Ближняя плоскость отсечения (чтобы не было артефактов "внутри камеры").
        /// </summary>
        public float Near { get; set; } = 0.01f;

        /// <summary>
        /// Дальняя плоскость отсечения (насколько далеко рисуем).
        /// </summary>
        public float Far { get; set; } = 500.0f;

        // ============================
        // Дополнительно (ограничения)
        // ============================

        /// <summary>
        /// Минимальная дистанция (чтобы зумом не "пролететь" через сцену).
        /// </summary>
        public float MinDistance { get; set; } = 0.2f;

        /// <summary>
        /// Максимальная дистанция (чтобы не улететь в бесконечность).
        /// </summary>
        public float MaxDistance { get; set; } = 2000.0f;

        /// <summary>
        /// Утилита для MVP: ограничить дистанцию камеры.
        /// Можно использовать в MouseController или Camera, чтобы всегда держать дистанцию
        /// в допустимых пределах (MinDistance..MaxDistance).
        /// </summary>
        public float ClampDistance(float distance)
        {
            if (distance < MinDistance) return MinDistance;
            if (distance > MaxDistance) return MaxDistance;
            return distance;
        }
    }
}
