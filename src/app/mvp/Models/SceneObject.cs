// mvp\Models\SceneObject.cs
//
// SceneObject — минимальная сущность "объект сцены" (Model).
// Это то, что будет отображаться в дереве tvScene и позже — в рендере.
//
// Важно (MVP):
// - Это МОДЕЛЬ: никаких ссылок на OpenGL / WinForms / контролы.
// - Только данные: имя, тип, трансформация, видимость.
// - Presenter будет добавлять/удалять/выбирать объекты, а View просто покажет список.
//
// Для MVP нам достаточно:
// - Id (уникально, чтобы связывать с TreeView и выбором)
// - Name (как отображаемое)
// - Type (для будущих иконок/логики)
// - Visible (включать/выключать отображение)
// - Position/Rotation/Scale (на будущее, сейчас можно не использовать)

using OpenTK.Mathematics;

namespace kuber3d.Models
{
    /// <summary>
    /// Тип объекта сцены. На MVP это больше "для порядка".
    /// Позже пригодится для: фильтров, разных рендереров, иконок в TreeView.
    /// </summary>
    public enum SceneObjectType
    {
        Unknown = 0,
        Point,
        Line,
        Plane,
        Mesh,
        Helper
    }

    /// <summary>
    /// Описание одного элемента в сцене.
    /// </summary>
    public class SceneObject
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// Удобно: хранить связь с TreeView (узел -> Id), выбор, команды.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Отображаемое имя (как увидим слева в tvScene).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Тип объекта (на MVP — просто метка).
        /// </summary>
        public SceneObjectType Type { get; set; } = SceneObjectType.Unknown;

        /// <summary>
        /// Видимость: если false — рендерер не рисует объект.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Позиция объекта в мировой системе координат.
        /// </summary>
        public Vector3 Position { get; set; } = Vector3.Zero;

        /// <summary>
        /// Поворот в градусах (Euler).
        /// На будущем этапе можно заменить на Quaternion.
        /// </summary>
        public Vector3 RotationDeg { get; set; } = Vector3.Zero;

        /// <summary>
        /// Масштаб объекта.
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.One;

        public SceneObject(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Для отладки и логов.
        /// </summary>
        public override string ToString()
            => $"{Name} ({Type}) [{Id}]";
    }
}
