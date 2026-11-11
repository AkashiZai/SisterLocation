using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    /// <summary>
    /// 3D Private Room / Custom Night Office: Left & Right animated steel blast doors,
    /// Center overhead vent shutter, desk with 3 CRT monitors, Golden Fredbear plush with walkie-talkie,
    /// left wall keypad, and 3D Yenndo, Bonnet, and Minireena 2 intruders inside the office.
    /// </summary>
    public sealed class PrivateRoomOffice3D
    {
        public readonly Room3DView View;

        public static readonly Vector3 LeftDoorLabel = new Vector3(-2.95f, 1.35f, 1.45f);
        public static readonly Vector3 RightDoorLabel = new Vector3(2.95f, 1.35f, 1.45f);
        public static readonly Vector3 VentLabel = new Vector3(0f, 2.25f, 2.68f);
        public static readonly Vector3 WallKeypadCenter = new Vector3(-1.55f, 1.58f, 2.68f);
        public static readonly Vector3[] MonitorCenters =
        {
            new Vector3(-0.78f, 1.18f, 2.05f),
            new Vector3(-0.08f, 1.18f, 2.05f),
            new Vector3(0.62f, 1.18f, 2.05f)
        };
        public static readonly Vector3 FredbearNosePos = new Vector3(1.22f, 1.16f, 1.92f);

        private readonly Transform _leftBlastDoor;
        private readonly Transform _rightBlastDoor;
        private readonly Transform _ventShutter;
        private float _leftDoorPos;
        private float _rightDoorPos;
        private float _ventDoorPos;

        private readonly Renderer[] _monitorScreens = new Renderer[3];
        private readonly Material _screenOff, _screenFnaf4, _screenLolbit;
        private readonly GameObject _yenndoGroup;
        private readonly Transform _bonnetGroup;
        private readonly GameObject[] _minireenaGroups = new GameObject[3];
        private readonly Light _officeLight;

        public PrivateRoomOffice3D()
        {
            View = new Room3DView("PrivateRoomOffice3D", new Vector3(600f, -500f, 0f), new Vector3(0f, 1.32f, -0.75f), 62f, 42f,
                new Color(0.03f, 0.035f, 0.045f), new Color(0.01f, 0.012f, 0.018f), 0.065f);
            var v = View;

            Color wallCol = new Color(0.17f, 0.19f, 0.22f);
            Color steel = new Color(0.40f, 0.43f, 0.48f);
            Color doorSteel = new Color(0.32f, 0.35f, 0.40f);
            Color white = new Color(0.92f, 0.92f, 0.95f);

            // Floor, ceiling, back wall, front wall
            v.Box(new Vector3(0f, -0.05f, 0.8f), new Vector3(6.4f, 0.1f, 4.4f), new Color(0.09f, 0.09f, 0.11f));
            v.Box(new Vector3(0f, 3.05f, 0.8f), new Vector3(6.4f, 0.1f, 4.4f), new Color(0.06f, 0.06f, 0.08f));
            v.Box(new Vector3(0f, 1.5f, -1.3f), new Vector3(6.4f, 3.0f, 0.15f), wallCol);
            v.Box(new Vector3(0f, 1.5f, 2.8f), new Vector3(6.4f, 3.0f, 0.15f), wallCol);

            // Side walls with doorway cutouts (z = 0.85..2.05, y = 0..2.25) & dark outer hallways
            foreach (float s in new[] { -1f, 1f })
            {
                float wx = s * 3.1f;
                v.Box(new Vector3(wx, 1.5f, -0.22f), new Vector3(0.16f, 3.0f, 2.15f), wallCol);
                v.Box(new Vector3(wx, 1.5f, 2.42f), new Vector3(0.16f, 3.0f, 0.75f), wallCol);
                v.Box(new Vector3(wx, 2.62f, 1.45f), new Vector3(0.16f, 0.75f, 1.2f), wallCol);
                // Steel door frame
                v.Box(new Vector3(wx - s * 0.05f, 1.12f, 0.82f), new Vector3(0.12f, 2.3f, 0.08f), steel);
                v.Box(new Vector3(wx - s * 0.05f, 1.12f, 2.08f), new Vector3(0.12f, 2.3f, 0.08f), steel);
                v.Box(new Vector3(wx - s * 0.05f, 2.28f, 1.45f), new Vector3(0.12f, 0.08f, 1.34f), steel);
                // Dark hallway recess outside doorway
                v.Box(new Vector3(s * 4.1f, 1.15f, 1.45f), new Vector3(1.8f, 2.4f, 1.5f), new Color(0.02f, 0.02f, 0.03f));
            }

            // Animated Left & Right steel blast doors
            _leftBlastDoor = v.Group("LeftBlastDoor", new Vector3(-3.04f, 3.4f, 1.45f));
            v.Box(Vector3.zero, new Vector3(0.08f, 2.25f, 1.2f), doorSteel, _leftBlastDoor);
            for (int r = -2; r <= 2; r++)
                v.Box(new Vector3(0.05f, r * 0.42f, 0f), new Vector3(0.02f, 0.06f, 1.12f), steel * 0.7f, _leftBlastDoor);

            _rightBlastDoor = v.Group("RightBlastDoor", new Vector3(3.04f, 3.4f, 1.45f));
            v.Box(Vector3.zero, new Vector3(0.08f, 2.25f, 1.2f), doorSteel, _rightBlastDoor);
            for (int r = -2; r <= 2; r++)
                v.Box(new Vector3(-0.05f, r * 0.42f, 0f), new Vector3(0.02f, 0.06f, 1.12f), steel * 0.7f, _rightBlastDoor);

            // Center Overhead Vent opening & shutter on front wall (x = -0.6..0.6, y = 1.9..2.6, z = 2.72)
            v.Box(new Vector3(0f, 2.25f, 2.73f), new Vector3(1.24f, 0.74f, 0.08f), new Color(0.02f, 0.02f, 0.03f));
            v.Box(new Vector3(0f, 2.64f, 2.68f), new Vector3(1.34f, 0.07f, 0.14f), steel);
            v.Box(new Vector3(0f, 1.86f, 2.68f), new Vector3(1.34f, 0.07f, 0.14f), steel);
            v.Box(new Vector3(-0.64f, 2.25f, 2.68f), new Vector3(0.07f, 0.84f, 0.14f), steel);
            v.Box(new Vector3(0.64f, 2.25f, 2.68f), new Vector3(0.07f, 0.84f, 0.14f), steel);
            _ventShutter = v.Box(new Vector3(0f, 2.95f, 2.70f), new Vector3(1.2f, 0.7f, 0.05f), doorSteel).transform;

            // Left Wall Keypad (1-9-8-3 & L-O-L)
            v.Box(WallKeypadCenter, new Vector3(0.55f, 0.72f, 0.08f), new Color(0.22f, 0.24f, 0.28f));

            // Office Desk + 3 CRT Monitors + Golden Fredbear Plush with Walkie-Talkie
            v.Box(new Vector3(0f, 0.85f, 2.05f), new Vector3(3.2f, 0.08f, 0.9f), new Color(0.18f, 0.15f, 0.12f));
            v.Box(new Vector3(-1.45f, 0.42f, 2.05f), new Vector3(0.18f, 0.84f, 0.82f), new Color(0.13f, 0.11f, 0.09f));
            v.Box(new Vector3(1.45f, 0.42f, 2.05f), new Vector3(0.18f, 0.84f, 0.82f), new Color(0.13f, 0.11f, 0.09f));

            _screenOff = v.GetMaterial(new Color(0.05f, 0.06f, 0.08f));
            _screenFnaf4 = v.GetMaterial(new Color(0.15f, 0.32f, 0.45f), new Color(0.1f, 0.35f, 0.55f));
            _screenLolbit = v.GetMaterial(new Color(0.95f, 0.5f, 0.12f), new Color(1.2f, 0.55f, 0.1f));

            for (int m = 0; m < 3; m++)
            {
                Vector3 mc = MonitorCenters[m];
                v.Box(mc, new Vector3(0.58f, 0.48f, 0.42f), new Color(0.24f, 0.25f, 0.28f));
                var scr = v.Box(mc + new Vector3(0f, 0f, -0.215f), new Vector3(0.48f, 0.38f, 0.02f), Color.black);
                _monitorScreens[m] = scr.GetComponent<Renderer>();
            }

            // Golden Fredbear Plush with purple top hat & walkie-talkie on right of desk
            Color gold = new Color(0.92f, 0.78f, 0.18f);
            var plush = v.Group("FredbearPlush", new Vector3(1.22f, 0.90f, 2.02f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0f, 0.14f, 0f), new Vector3(0.22f, 0.14f, 0.2f), gold, plush);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.32f, 0f), Vector3.one * 0.22f, gold, plush);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.29f, -0.10f), Vector3.one * 0.05f, Color.black, plush); // Nose
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.46f, 0f), new Vector3(0.14f, 0.07f, 0.14f), new Color(0.45f, 0.18f, 0.68f), plush);
            v.Box(new Vector3(-0.18f, 0.12f, -0.06f), new Vector3(0.08f, 0.18f, 0.06f), new Color(0.2f, 0.22f, 0.25f), plush); // Walkie-talkie

            // --- 3D Office Intruders ---
            // 1. Yenndo Endoskeleton standing right behind the desk
            Color endo = new Color(0.38f, 0.40f, 0.45f);
            var yenndo = v.Group("OfficeYenndo", new Vector3(-0.25f, 0f, 1.35f));
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 1.0f, 0f), new Vector3(0.28f, 0.48f, 0.24f), endo, yenndo);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.82f, 0f), new Vector3(0.44f, 0.46f, 0.42f), endo, yenndo);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.1f, 1.86f, -0.19f), Vector3.one * 0.075f, Color.black, yenndo, default, new Color(1f, 0.88f, 0.15f) * 3f);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.1f, 1.86f, -0.19f), Vector3.one * 0.075f, Color.black, yenndo, default, new Color(1f, 0.88f, 0.15f) * 3f);
            _yenndoGroup = yenndo.gameObject;

            // 2. Bonnet streaking across the desk
            Color pink = new Color(0.95f, 0.38f, 0.65f);
            _bonnetGroup = v.Group("OfficeBonnet", new Vector3(0f, 0.92f, 1.62f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0f, 0.14f, 0f), new Vector3(0.22f, 0.16f, 0.2f), pink, _bonnetGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.34f, 0f), Vector3.one * 0.22f, pink, _bonnetGroup);
            v.Box(new Vector3(-0.06f, 0.54f, 0f), new Vector3(0.05f, 0.20f, 0.04f), pink, _bonnetGroup);
            v.Box(new Vector3(0.06f, 0.54f, 0f), new Vector3(0.05f, 0.20f, 0.04f), pink, _bonnetGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.34f, -0.11f), Vector3.one * 0.055f, Color.red, _bonnetGroup);
            v.Box(new Vector3(0f, 0.22f, -0.11f), new Vector3(0.11f, 0.04f, 0.03f), Color.red, _bonnetGroup);

            // 3. Minireena 2 vision blockers inside the office
            for (int m = 0; m < 3; m++)
            {
                var mg = v.Group("OfficeMinireena" + m, new Vector3(-0.55f + m * 0.55f, 0.85f, 1.15f));
                v.Shape(PrimitiveType.Capsule, new Vector3(0f, 0.28f, 0f), new Vector3(0.16f, 0.22f, 0.14f), new Color(0.92f, 0.88f, 0.78f), mg);
                v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.58f, 0f), Vector3.one * 0.18f, white, mg);
                v.Shape(PrimitiveType.Sphere, new Vector3(-0.04f, 0.59f, -0.08f), Vector3.one * 0.035f, Color.black, mg, default, new Color(1f, 0.85f, 0.2f) * 2f);
                v.Shape(PrimitiveType.Sphere, new Vector3(0.04f, 0.59f, -0.08f), Vector3.one * 0.035f, Color.black, mg, default, new Color(1f, 0.85f, 0.2f) * 2f);
                _minireenaGroups[m] = mg.gameObject;
            }

            _officeLight = v.AddLight(LightType.Point, new Vector3(0f, 2.75f, 0.8f), new Color(0.65f, 0.78f, 0.88f), 1.75f, 7.5f, default, 60f, true);
        }

        public void SetActive(bool active) => View.SetActive(active);

        public Vector3 GetBonnetNoseWorld(float bonnetScreenX)
        {
            float bx = Mathf.Lerp(-1.35f, 1.35f, Mathf.InverseLerp(220f, 1060f, bonnetScreenX));
            return new Vector3(bx, 1.26f, 1.51f);
        }

        public void Sync(float deltaTime, bool leftClosed, bool rightClosed, bool ventClosed,
            int lolbitMonitors, bool fnaf4Unlocked, bool yenndoActive, bool bonnetActive, float bonnetScreenX, int minireena2Count, float oxygen)
        {
            View.Update(deltaTime);

            _leftDoorPos = Mathf.MoveTowards(_leftDoorPos, leftClosed ? 1f : 0f, deltaTime * 6.5f);
            _rightDoorPos = Mathf.MoveTowards(_rightDoorPos, rightClosed ? 1f : 0f, deltaTime * 6.5f);
            _ventDoorPos = Mathf.MoveTowards(_ventDoorPos, ventClosed ? 1f : 0f, deltaTime * 6.5f);

            _leftBlastDoor.localPosition = new Vector3(-3.04f, Mathf.Lerp(3.4f, 1.12f, _leftDoorPos), 1.45f);
            _rightBlastDoor.localPosition = new Vector3(3.04f, Mathf.Lerp(3.4f, 1.12f, _rightDoorPos), 1.45f);
            _ventShutter.localPosition = new Vector3(0f, Mathf.Lerp(2.98f, 2.25f, _ventDoorPos), 2.70f);

            for (int m = 0; m < 3; m++)
            {
                _monitorScreens[m].sharedMaterial = (lolbitMonitors > m) ? _screenLolbit : (fnaf4Unlocked ? _screenFnaf4 : _screenOff);
            }

            _yenndoGroup.SetActive(yenndoActive);
            _bonnetGroup.gameObject.SetActive(bonnetActive);
            if (bonnetActive)
            {
                float bx = Mathf.Lerp(-1.35f, 1.35f, Mathf.InverseLerp(220f, 1060f, bonnetScreenX));
                _bonnetGroup.localPosition = new Vector3(bx, 0.90f, 1.62f);
            }

            for (int m = 0; m < 3; m++)
            {
                _minireenaGroups[m].SetActive(m < minireena2Count);
            }

            float oxyFactor = Mathf.Clamp01(oxygen / 45f);
            _officeLight.intensity = Mathf.Lerp(0.18f, 1.75f, oxyFactor) * (0.92f + 0.08f * Mathf.Sin(Time.time * 42f));

            View.RenderRoom();
        }
    }
}
