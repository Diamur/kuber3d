// mvp/Core/Camera.cs
//
// Camera — "математическая" камера сцены (Model-level логика, но лежит в Core).
// Тут НЕТ WinForms, НЕТ GLControl, НЕТ событий мыши.
// Камера — это состояние (позиция/ориентация/зум) + методы, которые изменяют это состояние.
//
// Мы делаем понятную камеру "Orbit + Pan + Zoom":
// - ПКМ (drag) => Orbit: вращаем вокруг точки Target (центра сцены)
// - ЛКМ (drag) => Pan: сдвигаем Target влево/вправо/вверх/вниз (как в Blender/Maya)
// - Колесо => Zoom: меняем расстояние (Distance) до Target
//
// Рендереру камера отдаёт матрицы View и Projection.
// Это минимальный фундамент, который потом можно улучшать (FOV, near/far, ограничения, режимы).

using OpenTK.Mathematics;

namespace kuber3d.Core
{
    public class Camera
    {
        // =========================
        // 1) Публичные параметры
        // =========================

        /// <summary>
        /// Точка, вокруг которой мы "крутимся" (центр интереса).
        /// Это важнее, чем позиция: позиция вычисляется из Target + углы + расстояние.
        /// </summary>
        public Vector3 Target { get; private set; } = Vector3.Zero;

        /// <summary>
        /// Расстояние от камеры до Target (чем меньше — тем ближе "зум").
        /// </summary>
        public float Distance { get; private set; } = 6.0f;

        /// <summary>
        /// Угол поворота вокруг вертикальной оси (в градусах).
        /// Это "вращение вправо-влево" (yaw).
        /// </summary>
        public float YawDeg { get; private set; } = 45.0f;

        /// <summary>
        /// Угол наклона вверх/вниз (в градусах).
        /// Это "поднять камеру" (pitch).
        /// </summary>
        public float PitchDeg { get; private set; } = -25.0f;

        /// <summary>
        /// Поле зрения перспективы.
        /// </summary>
        public float FovDeg { get; set; } = 60f;

        /// <summary>
        /// Ограничения камеры, чтобы не ломать математику.
        /// </summary>
        public float MinDistance { get; set; } = 0.2f;
        public float MaxDistance { get; set; } = 500f;

        /// <summary>
        /// Pitch ограничиваем, чтобы камера не "переворачивалась" и не ловила сингулярность.
        /// Обычно держат в пределах (-89..+89).
        /// </summary>
        public float MinPitchDeg { get; set; } = -89f;
        public float MaxPitchDeg { get; set; } = 89f;

        /// <summary>
        /// Near/Far плоскости для Projection.
        /// </summary>
        public float Near { get; set; } = 0.01f;
        public float Far { get; set; } = 2000f;

        /// <summary>
        /// Соотношение сторон (width/height) для матрицы проекции.
        /// </summary>
        public float Aspect { get; private set; } = 1.0f;

        // =========================
        // 2) Матрицы для рендера
        // =========================

        /// <summary>
        /// Возвращает позицию камеры в мире (вычисляемую).
        /// </summary>
        public Vector3 Position => ComputePosition();

        /// <summary>
        /// Матрица вида: "как камера смотрит на мир".
        /// </summary>
        public Matrix4 GetViewMatrix()
        {
            // Камера всегда смотрит на Target.
            // UpVector — мировой "верх" (Y).
            return Matrix4.LookAt(Position, Target, Vector3.UnitY);
        }

        /// <summary>
        /// Матрица проекции (перспектива).
        /// aspect = width/height вьюпорта.
        /// </summary>
        public Matrix4 GetProjectionMatrix(float aspect)
        {
            // FOV в радианах.
            float fovRad = MathUtil.DegToRad(FovDeg);

            // Perspective: задаём FOV, соотношение сторон, near/far.
            return Matrix4.CreatePerspectiveFieldOfView(fovRad, aspect, Near, Far);
        }

        /// <summary>
        /// Матрица проекции с использованием сохранённого Aspect.
        /// </summary>
        public Matrix4 GetProjectionMatrix()
        {
            return GetProjectionMatrix(Aspect);
        }

        /// <summary>
        /// Обновить соотношение сторон при изменении размера вьюпорта.
        /// </summary>
        public void Resize(int width, int height)
        {
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            Aspect = width / (float)height;
        }

        // =========================
        // 3) Управление камерой
        // =========================

        /// <summary>
        /// Orbit: вращаем камеру вокруг Target.
        /// dx, dy — это дельты мыши (в пикселях).
        /// sensitivity — насколько сильно поворачиваем камеру на пиксель.
        /// </summary>
        public void Orbit(float dx, float dy, float sensitivity = 0.25f)
        {
            // Вращение: мышь вправо -> yaw увеличивается,
            // мышь вверх -> pitch уменьшается (чтобы интуитивно тянуть сцену).
            YawDeg += dx * sensitivity;
            PitchDeg -= dy * sensitivity;

            // Нормализуем yaw, чтобы не копился бесконечно
            YawDeg = MathUtil.NormalizeAngleDeg(YawDeg);

            // Ограничиваем pitch, чтобы не перевернуться
            PitchDeg = MathUtil.Clamp(PitchDeg, MinPitchDeg, MaxPitchDeg);
        }

        /// <summary>
        /// Pan: двигаем Target (центр сцены) в плоскости экрана.
        /// dx, dy — дельты мыши (в пикселях).
        /// panSpeed — насколько сильно сдвигаем за пиксель.
        ///
        /// Идея: сдвигаем Target вдоль "Right" и "Up" вектора камеры.
        /// </summary>
        public void Pan(float dx, float dy, float panSpeed = 0.01f)
        {
            // Чем дальше камера — тем сильнее должен быть pan за тот же пиксель,
            // иначе будет ощущение "залипания".
            float scaled = panSpeed * Distance;

            // Получаем локальные оси камеры:
            // Forward: куда смотрит камера
            Vector3 forward = (Target - Position).Normalized();

            // Right = forward x worldUp
            Vector3 right = Vector3.Cross(forward, Vector3.UnitY).Normalized();

            // Up = right x forward
            Vector3 up = Vector3.Cross(right, forward).Normalized();

            // Важно: экранные dx вправо => Target уходит влево (сдвигаем сцену за мышью),
            // поэтому знак часто делают отрицательным.
            Target -= right * (dx * scaled);
            Target += up * (dy * scaled);
        }

        /// <summary>
        /// Zoom: изменение расстояния до Target.
        /// wheelDelta обычно приходит как +/-120 (Windows), но мы не привязываемся.
        /// zoomSpeed — насколько быстро приближать/отдалять.
        /// </summary>
        public void Zoom(float wheelDelta, float zoomSpeed = 0.0015f)
        {
            // wheelDelta > 0 -> приблизить (уменьшаем Distance)
            // wheelDelta < 0 -> отдалить (увеличиваем Distance)
            Distance -= wheelDelta * zoomSpeed * Distance;

            Distance = MathUtil.Clamp(Distance, MinDistance, MaxDistance);
        }

        /// <summary>
        /// Быстрый сброс камеры к "стандартному" виду.
        /// Можно повесить на кнопку позже.
        /// </summary>
        public void Reset()
        {
            Target = Vector3.Zero;
            Distance = 6.0f;
            YawDeg = 45.0f;
            PitchDeg = -25.0f;
        }

        // =========================
        // 4) Внутренняя математика
        // =========================

        /// <summary>
        /// Вычисляет позицию камеры по (Target + yaw/pitch + distance).
        /// Мы используем сферические координаты:
        /// - yaw: поворот вокруг Y
        /// - pitch: наклон вверх/вниз
        /// - distance: радиус
        /// </summary>
        private Vector3 ComputePosition()
        {
            // Переводим углы в радианы
            float yaw = MathUtil.DegToRad(YawDeg);
            float pitch = MathUtil.DegToRad(PitchDeg);

            // Сферические координаты:
            // x = r * cos(pitch) * cos(yaw)
            // y = r * sin(pitch)
            // z = r * cos(pitch) * sin(yaw)
            float cp = MathF.Cos(pitch);
            float sp = MathF.Sin(pitch);
            float cy = MathF.Cos(yaw);
            float sy = MathF.Sin(yaw);

            Vector3 offset = new Vector3(
                Distance * cp * cy,
                Distance * sp,
                Distance * cp * sy
            );

            // Позиция камеры = Target + offset
            return Target + offset;
        }
    }
}
