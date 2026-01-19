// mvp\Models\CameraModel.cs
//
// CameraModel — "снимок" состояния камеры (Model-уровень).
// Тут НЕТ OpenGL, WinForms, GLControl и т.п.
// Это просто данные: куда смотрим, как далеко, какие углы.
// Presenter меняет эти данные (по мыши/кнопкам), а Render/View уже используют.
//
// Зачем отдельная модель камеры, если есть Core/Camera?
// - CameraModel = ЧИСТЫЕ ДАННЫЕ (можно сериализовать/сохранять/откатывать).
// - Core/Camera  = МАТЕМАТИКА и построение матриц View/Projection.
// В MVP это удобно: модель хранит состояние, Core вычисляет математику.

using OpenTK.Mathematics;

namespace kuber3d.Models
{
    /// <summary>
    /// Минимальная модель камеры для Orbit/Pan/Zoom.
    /// </summary>
    public class CameraModel
    {
        // ====== Точка, вокруг которой вращаемся ======
        // На старте это (0,0,0) — центр сцены.
        public Vector3 Target { get; set; } = Vector3.Zero;

        // ====== Панорамирование (смещение в плоскости экрана) ======
        // Это "ручной сдвиг" фокуса.
        // Удобно хранить отдельно от Target, чтобы Target оставался "логическим центром",
        // а пан был пользовательским смещением.
        public Vector3 Pan { get; set; } = Vector3.Zero;

        // ====== Дистанция камеры до фокуса ======
        // Это zoom в терминах орбитальной камеры.
        public float Distance { get; set; } = 25f;

        // ====== Углы вращения (в градусах) ======
        // Yaw   - поворот вокруг оси Y (влево/вправо)
        // Pitch - наклон вверх/вниз
        public float YawDeg { get; set; } = -45f;
        public float PitchDeg { get; set; } = -30f;

        // ====== Параметры перспективы ======
        // Их потом можно вынести в RenderSettings, но на MVP пусть будут тут.
        public float FovDeg { get; set; } = 60f;
        public float Near { get; set; } = 0.1f;
        public float Far { get; set; } = 2000f;

        /// <summary>
        /// Вспомогательное: итоговая точка фокуса с учетом панорамирования.
        /// По сути: куда смотрим.
        /// </summary>
        public Vector3 Focus => Target + Pan;

        /// <summary>
        /// Сброс в "стартовую" позицию камеры.
        /// </summary>
        public void Reset()
        {
            Target = Vector3.Zero;
            Pan = Vector3.Zero;
            Distance = 25f;
            YawDeg = -45f;
            PitchDeg = -30f;

            FovDeg = 60f;
            Near = 0.1f;
            Far = 2000f;
        }

        /// <summary>
        /// Клэмп (ограничения) — чтобы камера не перевернулась и не ушла в ноль.
        /// Вызываем после изменений, которые могут "вылететь" за пределы.
        /// </summary>
        public void Clamp()
        {
            // Не даём камере "врезаться" в фокус
            Distance = MathHelper.Clamp(Distance, 1f, 5000f);

            // Не даём pitch уйти в ±90, иначе будет вырождение и переворот up-вектора
            PitchDeg = MathHelper.Clamp(PitchDeg, -89f, 89f);
        }
    }
}
