using OpenTK.Mathematics;

namespace kuber3d.Core
{
    public class Camera
    {
        public Vector3 Target { get; private set; } = Vector3.Zero;
        public Vector3 Pan { get; private set; } = Vector3.Zero;

        public float Distance { get; private set; } = 25f;
        public float YawDeg { get; private set; } = -45f;
        public float PitchDeg { get; private set; } = -30f;

        public float FovDeg { get; set; } = 60f;
        public float Near { get; set; } = 0.1f;
        public float Far { get; set; } = 2000f;

        public float Aspect { get; private set; } = 1f;

        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }

        public void Resize(int width, int height)
        {
            Aspect = (height <= 0) ? 1f : width / (float)height;
            RebuildMatrices();
        }

        public void ResetToOrigin()
        {
            Target = Vector3.Zero;
            Pan = Vector3.Zero;
            Distance = 25f;
            YawDeg = -45f;
            PitchDeg = -30f;
            RebuildMatrices();
        }

        public void Orbit(float yawDeltaDeg, float pitchDeltaDeg)
        {
            YawDeg += yawDeltaDeg;
            PitchDeg = MathHelper.Clamp(PitchDeg + pitchDeltaDeg, -89f, 89f);
            RebuildMatrices();
        }

        public void AddZoom(float delta)
        {
            Distance = MathHelper.Clamp(Distance + delta, 1f, 5000f);
            RebuildMatrices();
        }

        public void AddPan(Vector3 delta)
        {
            Pan += delta;
            RebuildMatrices();
        }

        public void RebuildMatrices()
        {
            float yaw = MathHelper.DegreesToRadians(YawDeg);
            float pitch = MathHelper.DegreesToRadians(PitchDeg);

            var dir = new Vector3(
                MathF.Cos(pitch) * MathF.Cos(yaw),
                MathF.Sin(pitch),
                MathF.Cos(pitch) * MathF.Sin(yaw)
            );

            var focus = Target + Pan;
            var eye = focus + (-dir * Distance);

            View = Matrix4.LookAt(eye, focus, Vector3.UnitY);
            Projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(FovDeg), Aspect, Near, Far
            );
        }
    }
}
