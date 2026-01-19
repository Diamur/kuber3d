# AGENTS.md вЂ” РђСЂС…РёС‚РµРєС‚СѓСЂР° Kuber3D (WinForms + OpenTK) РІ СЃС‚РёР»Рµ MVP

## Р¦РµР»СЊ РјРёРЅРёРјСѓРјР° (MVP)
Р—Р°РїСѓСЃРєР°РµРјС‹Р№ 3D-РІСЊСЋРІРµСЂ РІ WinForms, РєРѕС‚РѕСЂС‹Р№:
- СЃРѕР·РґР°С‘С‚ OpenGL-РІСЊСЋРїРѕСЂС‚ РїРѕ РєРЅРѕРїРєРµ `btnStart3D`
- СЂРµР°РіРёСЂСѓРµС‚ РЅР° `chkGrid`, `chkAxes`
- СЂРµР°РіРёСЂСѓРµС‚ РЅР° РјС‹С€СЊ (orbit / pan / zoom) Рё РїРµСЂРµСЂРёСЃРѕРІС‹РІР°РµС‚ СЃС†РµРЅСѓ

## РњРёРЅРёРјР°Р»СЊРЅС‹Р№ РЅР°Р±РѕСЂ UI-СЌР»РµРјРµРЅС‚РѕРІ
- `FormMain`
  - `pnlTop` (Dock: Top)
    - `chkGrid`
    - `chkAxes`
    - `btnStart3D`
  - `splitMain` (Dock: Fill)
    - Panel1 (Р»РµРІР°СЏ)
      - `tvScene` (Dock: Fill)
    - Panel2 (РїСЂР°РІР°СЏ)
      - `pnlViewport` (Dock: Fill)

## РЎР»РѕРё Рё РїСЂР°РІРёР»Рѕ Р·Р°РІРёСЃРёРјРѕСЃС‚РµР№
РђСЂС…РёС‚РµРєС‚СѓСЂР° СЂР°Р·РґРµР»РµРЅР° РЅР° СЃР»РѕРё: View в†’ Presenter в†’ (Models/Core/Input/Rendering).

- View (WinForms) РќР• РїСЂРёРЅРёРјР°РµС‚ СЂРµС€РµРЅРёР№ Рё РќР• СЃРѕРґРµСЂР¶РёС‚ OpenGL-Р»РѕРіРёРєРё СЃС†РµРЅС‹.
- Presenter РїСЂРёРЅРёРјР°РµС‚ СЂРµС€РµРЅРёСЏ Рё СѓРїСЂР°РІР»СЏРµС‚ Р¶РёР·РЅРµРЅРЅС‹Рј С†РёРєР»РѕРј.
- Rendering СЂРёСЃСѓРµС‚, Core СЃС‡РёС‚Р°РµС‚ РјР°С‚РµРјР°С‚РёРєСѓ, Models С…СЂР°РЅРёС‚ СЃРѕСЃС‚РѕСЏРЅРёРµ, Input РёРЅС‚РµСЂРїСЂРµС‚РёСЂСѓРµС‚ РІРІРѕРґ.
- Р”РѕРїСѓСЃС‚РёРјС‹Рµ Р·Р°РІРёСЃРёРјРѕСЃС‚Рё:
  - View Р·Р°РІРёСЃРёС‚ РѕС‚ Contracts Рё РјРѕР¶РµС‚ Р·Р°РІРёСЃРµС‚СЊ РѕС‚ OpenTK (РІ С‡Р°СЃС‚Рё GLControl).
  - Presenter Р·Р°РІРёСЃРёС‚ РѕС‚ Contracts + Models/Core/Input/Rendering.
  - Rendering Р·Р°РІРёСЃРёС‚ РѕС‚ OpenTK (GL) Рё РѕС‚ Models/Core/Contracts.
  - Models/Core/Input РїРѕ РІРѕР·РјРѕР¶РЅРѕСЃС‚Рё Р±РµР· WinForms-Р·Р°РІРёСЃРёРјРѕСЃС‚РµР№ (Input РјРѕР¶РµС‚ РїСЂРёРЅРёРјР°С‚СЊ СЃРѕР±С‹С‚РёСЏ С‡РµСЂРµР· Р°Р±СЃС‚СЂР°РєС†РёРё/РєРѕР»Р»Р±РµРєРё).

## Contracts (РёРЅС‚РµСЂС„РµР№СЃС‹) вЂ” `mvp/Contracts`
### `IMainView`
РљРѕРЅС‚СЂР°РєС‚ РіР»Р°РІРЅРѕР№ С„РѕСЂРјС‹ (FormMain). Presenter РІРёРґРёС‚ С‚РѕР»СЊРєРѕ СЌС‚Рѕ.

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
- (РћРїС†РёРѕРЅР°Р»СЊРЅРѕ, РµСЃР»Рё РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ РІ РїСЂРѕРµРєС‚Рµ):
  - `Control ViewportHost`
  - `IGLView CreateGLView()`

### `IMainPresenter`
- `Initialize()`
- `Dispose()`

### `IGLView`
РљРѕРЅС‚СЂР°РєС‚ 3D-РІСЊСЋРїРѕСЂС‚Р°.

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
- (РћРїС†РёРѕРЅР°Р»СЊРЅРѕ, РµСЃР»Рё РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ):
  - `StartRendering(IRenderer renderer)`
  - `StopRendering()`
  - `ViewportResized`

### `IRenderer`
Р•РґРёРЅС‹Р№ РєРѕРЅС‚СЂР°РєС‚ СЂРµРЅРґРµСЂР°. РСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ GLView Рё SceneRenderer.

РўРёРїРѕРІРѕР№ Р¶РёР·РЅРµРЅРЅС‹Р№ С†РёРєР»:
- `Init(w, h)` РёР»Рё `Initialize()`
- `Resize(w, h)`
- `Render()`
- `Dispose()`

## Presenters вЂ” `mvp/Presenters`
### `MainPresenter`
Р“Р»Р°РІРЅС‹Р№ Presenter РїСЂРёР»РѕР¶РµРЅРёСЏ.

Р—Р°РґР°С‡Рё:
- РїРѕРґРїРёСЃР°С‚СЊСЃСЏ РЅР° СЃРѕР±С‹С‚РёСЏ `IMainView`
- РїРѕ `Start3DClicked` Р·Р°РїСѓСЃС‚РёС‚СЊ 3D (С‡РµСЂРµР· ViewportPresenter)
- РїРѕ `GridToggled` Рё `AxesToggled` РѕР±РЅРѕРІР»СЏС‚СЊ RenderSettings Рё РїСЂРѕСЃРёС‚СЊ РїРµСЂРµСЂРёСЃРѕРІРєСѓ

### `ViewportPresenter`
Presenter 3D-РІСЊСЋРїРѕСЂС‚Р°.

Р—Р°РґР°С‡Рё:
- СЃРѕР·РґР°С‚СЊ `IGLView` Рё РІСЃС‚СЂРѕРёС‚СЊ РµРіРѕ С‡РµСЂРµР· `IMainView.AttachViewport(glView)`
- Р·Р°РїСѓСЃС‚РёС‚СЊ СЂРµРЅРґРµСЂ: `glView.StartRendering(renderer)`
- РїРѕРґРєР»СЋС‡РёС‚СЊ РІРІРѕРґ: СЃРѕР±С‹С‚РёСЏ РјС‹С€Рё `IGLView` в†’ `MouseController`
- РїСЂРё resize: РѕР±РЅРѕРІРёС‚СЊ РєР°РјРµСЂСѓ/РїСЂРѕРµРєС†РёСЋ/viewport Рё РІС‹Р·РІР°С‚СЊ `RequestRender()`

## Views вЂ” `mvp/Views`
### `GLView`
View-РєРѕРјРїРѕРЅРµРЅС‚ OpenGL РІРЅСѓС‚СЂРё WinForms РЅР° Р±Р°Р·Рµ `OpenTK.WinForms.GLControl`.

Р—Р°РґР°С‡Рё:
- Р±С‹С‚СЊ "С…РѕСЃС‚РѕРј" OpenGL-РєРѕРЅС‚РµРєСЃС‚Р°
- РїСЂРѕРєРёРґС‹РІР°С‚СЊ СЃРѕР±С‹С‚РёСЏ РјС‹С€Рё РЅР°СЂСѓР¶Сѓ
- Р·Р°РїСѓСЃРєР°С‚СЊ/РѕСЃС‚Р°РЅР°РІР»РёРІР°С‚СЊ СЂРµРЅРґРµСЂ (РѕР±С‹С‡РЅРѕ С‡РµСЂРµР· Timer, $\approx 60$ FPS)
- РІС‹Р·С‹РІР°С‚СЊ `IRenderer.Render()` РІ Paint Рё `SwapBuffers()`

## Rendering вЂ” `mvp/Rendering`
### `SceneRenderer : IRenderer`
Р¦РµРЅС‚СЂР°Р»СЊРЅС‹Р№ СЂРµРЅРґРµСЂРµСЂ СЃС†РµРЅС‹.

Р”РµСЂР¶РёС‚:
- `Camera` (Core)
- `SceneModel` (Models)
- `RenderSettings` (Rendering)
- `LineShader`, `GridRenderer`, `AxesRenderer`

Р РёСЃСѓРµС‚ РєР°Р¶РґС‹Р№ РєР°РґСЂ:
- `GridRenderer` РµСЃР»Рё `RenderSettings.ShowGrid == true`
- `AxesRenderer` РµСЃР»Рё `RenderSettings.ShowAxes == true`

### `GridRenderer`
Р РёСЃСѓРµС‚ СЃРµС‚РєСѓ РІ РїР»РѕСЃРєРѕСЃС‚Рё $XZ$ (РѕР±С‹С‡РЅРѕ $y = 0$).
- `Build()` СЃРѕР·РґР°С‘С‚ РіРµРѕРјРµС‚СЂРёСЋ Р»РёРЅРёР№ 1 СЂР°Р·
- `Render(mvp)` СЂРёСЃСѓРµС‚ Р»РёРЅРёРё С‡РµСЂРµР· `LineShader`

### `AxesRenderer`
Р РёСЃСѓРµС‚ РѕСЃРё РєРѕРѕСЂРґРёРЅР°С‚ РёР· (0,0,0) РІ +X, +Y, +Z.
- `Build()` СЃРѕР·РґР°С‘С‚ РіРµРѕРјРµС‚СЂРёСЋ 1 СЂР°Р·
- `Render(mvp)` СЂРёСЃСѓРµС‚ Р»РёРЅРёРё С‡РµСЂРµР· `LineShader`

### `LineShader`
РњРёРЅРёРјР°Р»СЊРЅС‹Р№ С€РµР№РґРµСЂ Р»РёРЅРёР№:
- Р°С‚СЂРёР±СѓС‚С‹: position (vec3), color (vec4)
- uniform: MVP РјР°С‚СЂРёС†Р°

### `RenderSettings`
Р•РґРёРЅРѕРµ РјРµСЃС‚Рѕ РїР°СЂР°РјРµС‚СЂРѕРІ:
- РІРёР·СѓР°Р»СЊРЅС‹Рµ С„Р»Р°РіРё: `ShowGrid`, `ShowAxes`
- РїР°СЂР°РјРµС‚СЂС‹ СЃРµС‚РєРё/РѕСЃРµР№
- СЃРєРѕСЂРѕСЃС‚Рё СѓРїСЂР°РІР»РµРЅРёСЏ: orbit/pan/zoom
- РїР°СЂР°РјРµС‚СЂС‹ РїСЂРѕРµРєС†РёРё: FOV/near/far
- РѕРіСЂР°РЅРёС‡РµРЅРёСЏ РґРёСЃС‚Р°РЅС†РёРё

## Models вЂ” `mvp/Models`
### `SceneModel`
РҐСЂР°РЅРёС‚ СЃРїРёСЃРѕРє РѕР±СЉРµРєС‚РѕРІ СЃС†РµРЅС‹ (РЅР° MVP РјРѕР¶РµС‚ Р±С‹С‚СЊ РїРѕС‡С‚Рё РїСѓСЃС‚С‹Рј).

### `SceneObject`
РњРёРЅРёРјР°Р»СЊРЅР°СЏ СЃСѓС‰РЅРѕСЃС‚СЊ РѕР±СЉРµРєС‚Р° СЃС†РµРЅС‹ (id/С‚РёРї/С‚СЂР°РЅСЃС„РѕСЂРј Рё С‚.Рґ. вЂ” СЂР°СЃС€РёСЂСЏРµРјРѕ).

### `CameraModel`
РњРѕРґРµР»СЊ РїР°СЂР°РјРµС‚СЂРѕРІ РєР°РјРµСЂС‹ (РµСЃР»Рё РЅСѓР¶РЅРѕ С…СЂР°РЅРёС‚СЊ/СЃРµСЂРёР°Р»РёР·РѕРІР°С‚СЊ РѕС‚РґРµР»СЊРЅРѕ РѕС‚ Core.Camera).

## Core вЂ” `mvp/Core`
### `Camera`
РЎРѕСЃС‚РѕСЏРЅРёРµ РєР°РјРµСЂС‹:
- target, distance, yaw/pitch
- РІС‹С‡РёСЃР»РµРЅРёРµ view/projection РјР°С‚СЂРёС†
- СѓРїСЂР°РІР»РµРЅРёРµ: orbit/pan/zoom
- resize/aspect

### `MathUtil`
РњР°С‚РµРјР°С‚РёС‡РµСЃРєРёРµ СѓС‚РёР»РёС‚С‹: РєР»Р°РјРїС‹, СѓРіР»С‹, РјР°С‚СЂРёС†С‹, РІРµРєС‚РѕСЂС‹ Рё С‚.Рґ.

## Input вЂ” `mvp/Input`
### `MouseController`
РРЅС‚РµСЂРїСЂРµС‚Р°С†РёСЏ РІРІРѕРґР° РјС‹С€Рё РІ РєРѕРјР°РЅРґС‹ РєР°РјРµСЂС‹.

РњР°РїРїРёРЅРі:
- RMB drag в†’ orbit
- LMB drag в†’ pan
- wheel в†’ zoom

РџРѕСЃР»Рµ РёР·РјРµРЅРµРЅРёСЏ РєР°РјРµСЂС‹ РІС‹Р·С‹РІР°РµС‚ callback `RequestRender()`.

## Р“Р»Р°РІРЅС‹Рµ СЃС†РµРЅР°СЂРёРё (РїРѕС‚РѕРєРё СЃРѕР±С‹С‚РёР№)
### 1) Р—Р°РїСѓСЃРє 3D
`btnStart3D.Click` в†’ `IMainView.Start3DClicked` в†’ `MainPresenter` в†’ `ViewportPresenter`:
- СЃРѕР·РґР°С‚СЊ `GLView : IGLView`
- `FormMain.AttachViewport(glView)`
- `glView.StartRendering(sceneRenderer)`
- РїРµСЂРІС‹Р№ РєР°РґСЂ

### 2) РџРµСЂРµРєР»СЋС‡РµРЅРёРµ СЃРµС‚РєРё/РѕСЃРµР№
`chkGrid.CheckedChanged` в†’ `GridToggled` в†’ Presenter:
- `RenderSettings.ShowGrid = IsGridEnabled`
- `RequestRender()`

`chkAxes.CheckedChanged` в†’ `AxesToggled` в†’ Presenter:
- `RenderSettings.ShowAxes = IsAxesEnabled`
- `RequestRender()`

### 3) РЈРїСЂР°РІР»РµРЅРёРµ РјС‹С€СЊСЋ
РЎРѕР±С‹С‚РёСЏ РјС‹С€Рё `IGLView` в†’ `MouseController`:
- РѕР±РЅРѕРІР»СЏРµС‚ `Camera`
- РІС‹Р·С‹РІР°РµС‚ `RequestRender()`

### 4) Resize
`GLControl.Resize` в†’ `IGLView.ViewportResized` (РёР»Рё РїСЂСЏРјРѕР№ РІС‹Р·РѕРІ Presenter) в†’ Presenter:
- РѕР±РЅРѕРІР»СЏРµС‚ camera aspect / projection
- `renderer.Resize(w, h)`
- `RequestRender()`

## РћР±РѕР·РЅР°С‡РµРЅРёСЏ РјР°С‚РµРјР°С‚РёРєРё (РґР»СЏ РґРѕРєСѓРјРµРЅС‚Р°С†РёРё)
- РЎРµС‚РєР° Р»РµР¶РёС‚ РІ РїР»РѕСЃРєРѕСЃС‚Рё $XZ$ РїСЂРё $y = 0$.
- РћСЃРё: РѕС‚СЂРµР·РєРё $(0,0,0)\rightarrow(L,0,0)$, $(0,0,0)\rightarrow(0,L,0)$, $(0,0,0)\rightarrow(0,0,L)$.
- Р РµРЅРґРµСЂ Р»РёРЅРёР№ РёСЃРїРѕР»СЊР·СѓРµС‚ РјР°С‚СЂРёС†Сѓ $MVP$, РіРґРµ $MVP = P \cdot V \cdot M$ (РїРѕСЂСЏРґРѕРє Р·Р°РІРёСЃРёС‚ РѕС‚ СЃРѕРіР»Р°С€РµРЅРёСЏ РІ РєРѕРґРµ Рё OpenTK).

- РџРѕСЃР»Рµ РєР°Р¶РґРѕРіРѕ РѕР±РЅРѕРІР»РµРЅРёСЏ: Р·Р°РїРёСЃР°С‚СЊ РѕС‚С‡РµС‚ РІ AGENTS.md (С‡С‚Рѕ РёР·РјРµРЅРёР»Рё, РєР°РєРёРµ С„Р°Р№Р»С‹ С‚СЂРѕРіР°Р»Рё, РєР°РєРёРµ РѕС€РёР±РєРё/С„РёРєСЃС‹, С‡С‚Рѕ РїСЂРѕРІРµСЂРёС‚СЊ РїСЂРё Р·Р°РїСѓСЃРєРµ).

## РћС‚С‡РµС‚ РїРѕ РѕР±РЅРѕРІР»РµРЅРёСЏРј

- Изменения: убрал подавление исключений в MakeCurrent/SwapBuffers/ResizeViewport и добавил MakeCurrent в OnGlLoad, чтобы инициализация рендера выполнялась при текущем контексте.
- Тронутые файлы: `src/app/mvp/Views/GLView.cs`.
- Ошибки/фиксы: потенциальная причина черного экрана — GL вызовы без текущего контекста и скрытые исключения в MakeCurrent/SwapBuffers; теперь ошибки не глушатся и контекст делается текущим перед Init.
- Проверить при запуске: что цепочка Timer -> Invalidate -> Paint -> Render -> SwapBuffers работает, и что при запуске видно хотя бы фоновую заливку.
- Изменения: IGLView.Control теперь возвращает сам GLView, чтобы GLControl не выдергивался из своего контейнера при добавлении в pnlViewport.
- Тронутые файлы: `src/app/mvp/Views/GLView.cs`.
- Ошибки/фиксы: возможная причина черного экрана — переподключение GLControl напрямую в pnlViewport, что может ломать загрузку/контекст/события; теперь встраивается GLView целиком.
- Проверить при запуске: что GLControl.Load/Resize/Paint стабильно отрабатывают после встраивания и что сетка/оси появились.
