// mvp/Core/MathUtil.cs
//
// MathUtil — набор маленьких математических утилит для 3D.
//
// Почему это отдельный файл (Core)?
// - Тут "чистая математика", без WinForms/OpenGL.
// - Этим пользуются Camera, MouseController, Presenter'ы, любые вычисления.
// - Позже сюда добавим серьёзную геометрию: пересечения, расстояния, проекции и т.д.
//
// В MVP-минимуме нам нужно:
// - clamp (ограничить значения)
// - degrees<->radians
// - безопасные проверки (например, не делить на 0)
// - нормализация углов (чтобы yaw не улетал в миллионы)
//
// Это всё делается тезисно и максимально понятно (для Дениса).

using System;

namespace kuber3d.Core
{
    public static class MathUtil
    {
        /// <summary>
        /// Ограничивает значение в диапазон [min..max].
        /// Используется для pitch камеры (чтобы не перевернуть вверх ногами),
        /// зума, чувствительности и т.п.
        /// </summary>
        public static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        /// <summary>
        /// То же самое для double — иногда удобно.
        /// </summary>
        public static double Clamp(double v, double min, double max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        /// <summary>
        /// Перевод градусов в радианы.
        /// OpenTK/матрицы обычно работают в радианах.
        /// </summary>
        public static float DegToRad(float deg)
            => deg * (float)(Math.PI / 180.0);

        /// <summary>
        /// Перевод радиан в градусы.
        /// </summary>
        public static float RadToDeg(float rad)
            => rad * (float)(180.0 / Math.PI);

        /// <summary>
        /// Нормализует угол в градусах в диапазон [-180..+180].
        /// Чтобы значения не "росли бесконечно".
        /// </summary>
        public static float NormalizeAngleDeg(float deg)
        {
            // Приводим к диапазону [0..360)
            deg %= 360f;
            if (deg < 0f) deg += 360f;

            // Переводим в [-180..+180]
            if (deg > 180f) deg -= 360f;
            return deg;
        }

        /// <summary>
        /// Безопасное деление (если делитель слишком мал — возвращаем fallback).
        /// Полезно в местах, где может быть 0.
        /// </summary>
        public static float SafeDiv(float numerator, float denominator, float fallback = 0f)
        {
            if (Math.Abs(denominator) < 1e-8f)
                return fallback;
            return numerator / denominator;
        }

        /// <summary>
        /// Ограничение по модулю: значение в диапазоне [-limit..+limit].
        /// Удобно для ограничения скоростей/дельт.
        /// </summary>
        public static float ClampAbs(float v, float limit)
        {
            if (limit < 0f) limit = -limit;
            return Clamp(v, -limit, limit);
        }
    }
}
