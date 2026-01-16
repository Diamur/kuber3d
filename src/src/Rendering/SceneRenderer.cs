using kuber3d.Core;

namespace kuber3d.Rendering
{
    public class SceneRenderer
    {
        private readonly GridRenderer _grid = new GridRenderer();
        private readonly AxesRenderer _axes = new AxesRenderer();

        public void Init()
        {
            _grid.Init();
            _axes.Init();
        }

        public void Render(Camera cam)
        {
            _grid.Render(cam);
            _axes.Render(cam);
        }
    }
}
