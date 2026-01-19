// mvp\Models\SceneModel.cs
//
// SceneModel — "модель сцены" (данные), без UI и без OpenGL.
//
// Здесь хранится:
// - список объектов сцены (для дерева tvScene и логики)
// - выбранный объект (на будущее)
// - простейшие утилиты добавления/удаления/поиска
//
// Сейчас нам для MVP достаточно:
// - иметь коллекцию объектов (пусть пока даже пустую)
// - уметь добавить пару тестовых объектов (позже Presenter сам это сделает)
// - безопасно работать с ID
//
// Важно:
// - никаких ссылок на WinForms (TreeView) и OpenGL (GL, Shader) — это всё в других слоях.

using System;
using System.Collections.Generic;
using System.Linq;

namespace kuber3d.Models
{
    public class SceneModel
    {
        /// <summary>
        /// Все объекты сцены.
        /// Порядок важен для дерева/списка (позже можно сделать слои/группы).
        /// </summary>
        public List<SceneObject> Objects { get; } = new();

        /// <summary>
        /// ID выбранного объекта (для будущей подсветки/редактирования).
        /// Пока можно не использовать, но место под это уже есть.
        /// </summary>
        public string? SelectedObjectId { get; private set; }

        /// <summary>
        /// Быстрая проверка: есть ли объект с таким ID.
        /// </summary>
        public bool Contains(string id)
            => Objects.Any(o => o.Id == id);

        /// <summary>
        /// Получить объект по ID (или null если не найден).
        /// </summary>
        public SceneObject? GetById(string id)
            => Objects.FirstOrDefault(o => o.Id == id);

        /// <summary>
        /// Добавить объект в сцену.
        /// </summary>
        public void Add(SceneObject obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (Contains(obj.Id))
                throw new InvalidOperationException($"Scene already contains object with id '{obj.Id}'");

            Objects.Add(obj);
        }

        /// <summary>
        /// Удалить объект по ID. Вернёт true, если реально удалили.
        /// </summary>
        public bool Remove(string id)
        {
            var obj = GetById(id);
            if (obj == null) return false;

            // если удалили выбранный — сбрасываем выбор
            if (SelectedObjectId == id)
                SelectedObjectId = null;

            Objects.Remove(obj);
            return true;
        }

        /// <summary>
        /// Установить выбранный объект (ID должен существовать, иначе сброс).
        /// </summary>
        public void Select(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                SelectedObjectId = null;
                return;
            }

            SelectedObjectId = Contains(id) ? id : null;
        }

        /// <summary>
        /// Утилита для MVP: очистить сцену полностью.
        /// </summary>
        public void Clear()
        {
            Objects.Clear();
            SelectedObjectId = null;
        }
    }
}
