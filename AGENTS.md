# AGENTS.md — Архитектура Kuber3D (WinForms + OpenTK) в стиле MVP

## Цель минимума (MVP)
Запускаемый 3D-вьювер в WinForms, который:
- создаёт OpenGL-вьюпорт по кнопке `btnStart3D`
- реагирует на `chkGrid`, `chkAxes`
- реагирует на мышь (orbit / pan / zoom) и перерисовывает сцену

## Минимальный набор UI-элементов
- `FormMain`
  - `pnlTop` (Dock: Top)
    - `chkGrid`
    - `chkAxes`
    - `btnStart3D`
  - `splitMain` (Dock: Fill)
    - Panel1 (левая)
      - `tvScene` (Dock: Fill)
    - Panel2 (правая)
      - `pnlViewport` (Dock: Fill)

## Слои и правило зависимостей
Архитектура разделена на слои: View → Presenter → (Models/Core/Input/Rendering).

- View (WinForms) НЕ принимает решений и НЕ содержит OpenGL-логики сцены.
- Presenter принимает решения и управляет жизненным циклом.
- Rendering рисует, Core считает математику, Models хранит состояние, Input интерпретирует ввод.
- Допустимые зависимости:
  - View зависит от Contracts и может зависеть от OpenTK (в части GLControl).
  - Presenter зависит от Contracts + Models/Core/Input/Rendering.
  - Rendering зависит от OpenTK (GL) и от Models/Core/Contracts.
  - Models/Core/Input по возможности без WinForms-зависимостей (Input может принимать события через абстракции/коллбеки).

## Contracts (интерфейсы) — `mvp/Contracts`
### `IMainView`
Контракт главной формы (FormMain). Presenter видит только это.

- Events:
  - `Start3DClicked`
  - `GridToggled`
  - `AxesToggled`
- Properties:
  - `IsGridEnabled`
  - `IsAxesEnabled`
  - `IsStart3DEnabled`
- Methods:
  - `AttachViewport(IGLView glView)`
  - `ShowMessage(string text)`
  - `SetStatus(string text)`
- (Опционально, если используется в проекте):
  - `Control ViewportHost`
  - `IGLView CreateGLView()`

### `IMainPresenter`
- `Initialize()`
- `Dispose()`

### `IGLView`
Контракт 3D-вьюпорта.

- Properties:
  - `Control Control`
- Methods:
  - `MakeCurrent()`
  - `SwapBuffers()`
  - `ResizeViewport(int width, int height)`
  - `RequestRender()`
- Mouse events:
  - `MouseDown`
  - `MouseUp`
  - `MouseMove`
  - `MouseWheel`
  - `MouseEnter`
- (Опционально, если используется):
  - `StartRendering(IRenderer renderer)`
  - `StopRendering()`
  - `ViewportResized`

### `IRenderer`
Единый контракт рендера. Используется GLView и SceneRenderer.

Типовой жизненный цикл:
- `Init(w, h)` или `Initialize()`
- `Resize(w, h)`
- `Render()`
- `Dispose()`

## Presenters — `mvp/Presenters`
### `MainPresenter`
Главный Presenter приложения.

Задачи:
- подписаться на события `IMainView`
- по `Start3DClicked` запустить 3D (через ViewportPresenter)
- по `GridToggled` и `AxesToggled` обновлять RenderSettings и просить перерисовку

### `ViewportPresenter`
Presenter 3D-вьюпорта.

Задачи:
- создать `IGLView` и встроить его через `IMainView.AttachViewport(glView)`
- запустить рендер: `glView.StartRendering(renderer)`
- подключить ввод: события мыши `IGLView` → `MouseController`
- при resize: обновить камеру/проекцию/viewport и вызвать `RequestRender()`

## Views — `mvp/Views`
### `GLView`
View-компонент OpenGL внутри WinForms на базе `OpenTK.WinForms.GLControl`.

Задачи:
- быть "хостом" OpenGL-контекста
- прокидывать события мыши наружу
- запускать/останавливать рендер (обычно через Timer, $\approx 60$ FPS)
- вызывать `IRenderer.Render()` в Paint и `SwapBuffers()`

## Rendering — `mvp/Rendering`
### `SceneRenderer : IRenderer`
Центральный рендерер сцены.

Держит:
- `Camera` (Core)
- `SceneModel` (Models)
- `RenderSettings` (Rendering)
- `LineShader`, `GridRenderer`, `AxesRenderer`

Рисует каждый кадр:
- `GridRenderer` если `RenderSettings.ShowGrid == true`
- `AxesRenderer` если `RenderSettings.ShowAxes == true`

### `GridRenderer`
Рисует сетку в плоскости $XZ$ (обычно $y = 0$).
- `Build()` создаёт геометрию линий 1 раз
- `Render(mvp)` рисует линии через `LineShader`

### `AxesRenderer`
Рисует оси координат из (0,0,0) в +X, +Y, +Z.
- `Build()` создаёт геометрию 1 раз
- `Render(mvp)` рисует линии через `LineShader`

### `LineShader`
Минимальный шейдер линий:
- атрибуты: position (vec3), color (vec4)
- uniform: MVP матрица

### `RenderSettings`
Единое место параметров:
- визуальные флаги: `ShowGrid`, `ShowAxes`
- параметры сетки/осей
- скорости управления: orbit/pan/zoom
- параметры проекции: FOV/near/far
- ограничения дистанции

## Models — `mvp/Models`
### `SceneModel`
Хранит список объектов сцены (на MVP может быть почти пустым).

### `SceneObject`
Минимальная сущность объекта сцены (id/тип/трансформ и т.д. — расширяемо).

### `CameraModel`
Модель параметров камеры (если нужно хранить/сериализовать отдельно от Core.Camera).

## Core — `mvp/Core`
### `Camera`
Состояние камеры:
- target, distance, yaw/pitch
- вычисление view/projection матриц
- управление: orbit/pan/zoom
- resize/aspect

### `MathUtil`
Математические утилиты: клампы, углы, матрицы, векторы и т.д.

## Input — `mvp/Input`
### `MouseController`
Интерпретация ввода мыши в команды камеры.

Маппинг:
- RMB drag → orbit
- LMB drag → pan
- wheel → zoom

После изменения камеры вызывает callback `RequestRender()`.

## Главные сценарии (потоки событий)
### 1) Запуск 3D
`btnStart3D.Click` → `IMainView.Start3DClicked` → `MainPresenter` → `ViewportPresenter`:
- создать `GLView : IGLView`
- `FormMain.AttachViewport(glView)`
- `glView.StartRendering(sceneRenderer)`
- первый кадр

### 2) Переключение сетки/осей
`chkGrid.CheckedChanged` → `GridToggled` → Presenter:
- `RenderSettings.ShowGrid = IsGridEnabled`
- `RequestRender()`

`chkAxes.CheckedChanged` → `AxesToggled` → Presenter:
- `RenderSettings.ShowAxes = IsAxesEnabled`
- `RequestRender()`

### 3) Управление мышью
События мыши `IGLView` → `MouseController`:
- обновляет `Camera`
- вызывает `RequestRender()`

### 4) Resize
`GLControl.Resize` → `IGLView.ViewportResized` (или прямой вызов Presenter) → Presenter:
- обновляет camera aspect / projection
- `renderer.Resize(w, h)`
- `RequestRender()`

## Обозначения математики (для документации)
- Сетка лежит в плоскости $XZ$ при $y = 0$.
- Оси: отрезки $(0,0,0)\rightarrow(L,0,0)$, $(0,0,0)\rightarrow(0,L,0)$, $(0,0,0)\rightarrow(0,0,L)$.
- Рендер линий использует матрицу $MVP$, где $MVP = P \cdot V \cdot M$ (порядок зависит от соглашения в коде и OpenTK).

- После каждого обновления: записать отчет в AGENTS.md (что изменили, какие файлы трогали, какие ошибки/фиксы, что проверить при запуске).

## Отчет по обновлениям

### 2025-09-02
- Исправил инициализацию GLControl: запросил контекст OpenGL 3.3 Core и включил MakeCurrent перед Init в обработчике Load, чтобы шейдеры #version 330 core компилировались и рендер запускался стабильно.
- Файлы: src/app/mvp/Views/GLView.cs.
- Проверить: запуск btnStart3D, отображение сетки/осей, отсутствие черного экрана при первом кадре.
