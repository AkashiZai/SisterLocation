using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    /// <summary>
    /// 3D Funtime Auditorium: pitch-black auditorium with checkered floor, curtains, flash beacon
    /// lighting decay, 3D Funtime Foxy freeze poses, and 1/1,000 3D Yenndo endoskeleton.
    /// </summary>
    public sealed class FuntimeAuditoriumRoom3D
    {
        public readonly Room3DView View;

        private readonly Transform _foxyGroup;
        private readonly Transform _yenndoGroup;
        private readonly Light _flashLight;
        private readonly Light _exitSignLight;

        public FuntimeAuditoriumRoom3D()
        {
            View = new Room3DView("FuntimeAuditorium3D", new Vector3(400f, -500f, 0f), new Vector3(0f, 1.25f, 0.5f), 62f, 35f,
                new Color(0.004f, 0.004f, 0.007f), new Color(0.005f, 0.006f, 0.01f), 0.14f);
            var v = View;

            Color wallCol = new Color(0.14f, 0.09f, 0.15f);
            Color white = new Color(0.92f, 0.92f, 0.95f);
            Color pink = new Color(0.95f, 0.38f, 0.65f);

            // Auditorium hall (z = 0..16)
            v.Box(new Vector3(0f, -0.05f, 8f), new Vector3(7.2f, 0.1f, 17f), new Color(0.06f, 0.06f, 0.08f));
            v.Box(new Vector3(0f, 3.25f, 8f), new Vector3(7.2f, 0.1f, 17f), new Color(0.04f, 0.04f, 0.06f));
            v.Box(new Vector3(-3.5f, 1.6f, 8f), new Vector3(0.2f, 3.3f, 17f), wallCol);
            v.Box(new Vector3(3.5f, 1.6f, 8f), new Vector3(0.2f, 3.3f, 17f), wallCol);
            v.Box(new Vector3(0f, 1.6f, 16.2f), new Vector3(7.2f, 3.3f, 0.2f), wallCol);

            // Checkered floor & stage curtains
            for (int z = 0; z < 16; z++)
            {
                for (int x = -3; x <= 2; x++)
                {
                    if (((x + z) & 1) == 0)
                    {
                        v.Box(new Vector3(x + 0.5f, 0.005f, z + 0.5f), new Vector3(0.96f, 0.01f, 0.96f), new Color(0.22f, 0.20f, 0.26f));
                    }
                }
                if (z % 3 == 2)
                {
                    v.Box(new Vector3(-3.1f, 1.6f, z), new Vector3(0.35f, 3.1f, 0.65f), new Color(0.45f, 0.12f, 0.28f));
                    v.Box(new Vector3(3.1f, 1.6f, z), new Vector3(0.35f, 3.1f, 0.65f), new Color(0.45f, 0.12f, 0.28f));
                }
            }

            // Parts & Service door at far end
            v.Box(new Vector3(0f, 1.15f, 16.05f), new Vector3(1.5f, 2.3f, 0.1f), new Color(0.28f, 0.3f, 0.34f));
            _exitSignLight = v.AddLight(LightType.Point, new Vector3(0f, 2.5f, 15.6f), new Color(0.2f, 0.9f, 0.35f), 0.7f, 4.5f);

            // 3D Funtime Foxy (facing -Z toward the player)
            _foxyGroup = v.Group("AuditoriumFoxy", new Vector3(0f, 0f, 4f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0f, 1.0f, 0f), new Vector3(0.46f, 0.44f, 0.38f), white, _foxyGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.0f, -0.14f), new Vector3(0.32f, 0.38f, 0.2f), pink, _foxyGroup);
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.13f, 0.32f, 0f), new Vector3(0.12f, 0.32f, 0.12f), white, _foxyGroup);
            v.Shape(PrimitiveType.Capsule, new Vector3(0.13f, 0.32f, 0f), new Vector3(0.12f, 0.32f, 0.12f), white, _foxyGroup);
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.34f, 1.05f, -0.1f), new Vector3(0.11f, 0.34f, 0.11f), white, _foxyGroup, new Vector3(25f, 0f, -20f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0.34f, 1.05f, -0.1f), new Vector3(0.11f, 0.34f, 0.11f), white, _foxyGroup, new Vector3(25f, 0f, 20f));
            v.Box(new Vector3(0f, 1.42f, -0.18f), new Vector3(0.18f, 0.07f, 0.04f), new Color(0.85f, 0.1f, 0.15f), _foxyGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.78f, 0f), new Vector3(0.48f, 0.44f, 0.46f), white, _foxyGroup);
            v.Box(new Vector3(0f, 1.72f, -0.24f), new Vector3(0.2f, 0.14f, 0.22f), white, _foxyGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.77f, -0.36f), Vector3.one * 0.07f, Color.black, _foxyGroup);
            v.Box(new Vector3(-0.15f, 2.05f, 0f), new Vector3(0.1f, 0.24f, 0.06f), pink, _foxyGroup, new Vector3(0f, 0f, 15f));
            v.Box(new Vector3(0.15f, 2.05f, 0f), new Vector3(0.1f, 0.24f, 0.06f), pink, _foxyGroup, new Vector3(0f, 0f, -15f));
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.09f, 1.86f, -0.2f), Vector3.one * 0.065f, Color.black, _foxyGroup, default, new Color(1f, 0.75f, 0.1f) * 2.5f);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.09f, 1.86f, -0.2f), Vector3.one * 0.065f, Color.black, _foxyGroup, default, new Color(1f, 0.75f, 0.1f) * 2.5f);

            // 1/1,000 Yenndo Endoskeleton
            Color endoSteel = new Color(0.38f, 0.40f, 0.45f);
            _yenndoGroup = v.Group("AuditoriumYenndo", new Vector3(0f, 0f, 4f));
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 1.0f, 0f), new Vector3(0.26f, 0.45f, 0.22f), endoSteel, _yenndoGroup);
            for (int r = 0; r < 4; r++)
            {
                v.Box(new Vector3(0f, 0.8f + r * 0.14f, -0.1f), new Vector3(0.42f, 0.04f, 0.18f), endoSteel * 1.2f, _yenndoGroup);
            }
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.82f, 0f), new Vector3(0.42f, 0.44f, 0.4f), endoSteel, _yenndoGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.1f, 1.86f, -0.18f), Vector3.one * 0.07f, Color.black, _yenndoGroup, default, new Color(1f, 0.88f, 0.15f) * 3f);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.1f, 1.86f, -0.18f), Vector3.one * 0.07f, Color.black, _yenndoGroup, default, new Color(1f, 0.88f, 0.15f) * 3f);

            // Flash Beacon Light attached to player position
            _flashLight = v.AddLight(LightType.Point, new Vector3(0f, 1.3f, 0.6f), new Color(0.65f, 0.82f, 1f), 0f, 14f, default, 60f, true);
        }

        public void SetActive(bool active) => View.SetActive(active);

        public void Sync(float deltaTime, float progressPct, float flashTimer, bool foxyDanger, bool rareYenndo, bool moving)
        {
            View.Update(deltaTime);

            float playerZ = Mathf.Lerp(0.6f, 11.8f, Mathf.Clamp01(progressPct / 100f));
            float walkBobY = 1.25f + (moving ? Mathf.Sin(Time.time * 10f) * 0.05f : 0f);
            View.SetEyePosition(new Vector3(0f, walkBobY, playerZ));

            _flashLight.transform.localPosition = new Vector3(0f, walkBobY + 0.05f, playerZ + 0.15f);
            _flashLight.intensity = flashTimer > 0f ? (flashTimer / 0.45f) * 22f : 0f;
            _exitSignLight.intensity = 0.6f + Mathf.Sin(Time.time * 3f) * 0.15f;

            bool showYenndo = flashTimer > 0f && rareYenndo;
            bool showFoxy = flashTimer > 0f && !rareYenndo;
            _yenndoGroup.gameObject.SetActive(showYenndo);
            _foxyGroup.gameObject.SetActive(showFoxy);

            if (showYenndo)
            {
                _yenndoGroup.localPosition = new Vector3(0f, 0f, playerZ + 2.1f);
            }
            else if (showFoxy)
            {
                float dist = foxyDanger ? 1.85f : 4.6f;
                float side = foxyDanger ? 0f : Mathf.Sin(progressPct * 0.3f) * 1.1f;
                _foxyGroup.localPosition = new Vector3(side, 0f, playerZ + dist);
            }

            View.RenderRoom();
        }
    }

    /// <summary>
    /// 3D Parts and Service: maintenance room with overhead surgical lamp, conveyor pedestal,
    /// Night 3 Funtime Freddy (articulated 3D faceplates, chest cavity, power module, peeking Bon-Bon),
    /// and Night 5 Circus Baby (cheek keypad hatch & green card hatch).
    /// </summary>
    public sealed class PartsAndServiceRoom3D
    {
        public readonly Room3DView View;

        // 3D anchor points for Night 3 Freddy maintenance buttons
        public static readonly Vector3 FreddyRightCheekBtn = new Vector3(-0.34f, 1.48f, 1.02f);
        public static readonly Vector3 FreddyLeftCheekBtn = new Vector3(0.34f, 1.48f, 1.02f);
        public static readonly Vector3 FreddyRightEyeBtn = new Vector3(-0.26f, 1.72f, 1.00f);
        public static readonly Vector3 FreddyNoseBtn = new Vector3(0f, 1.66f, 0.94f);
        public static readonly Vector3 FreddyJawBtn = new Vector3(0f, 1.36f, 1.04f);
        public static readonly Vector3 FreddyChestPwrBtn = new Vector3(0f, 0.96f, 1.05f);

        // 3D anchor points for Night 5 Baby maintenance
        public static readonly Vector3 BabyCheekBtn = new Vector3(-0.36f, 1.46f, 1.02f);
        public static readonly Vector3 BabyCardSlot = new Vector3(-0.46f, 0.95f, 1.05f);
        public static readonly Vector3 ConveyorBtn = new Vector3(-1.35f, 0.95f, 1.55f);

        private readonly GameObject _freddyRoot;
        private readonly Transform _tlPlate, _trPlate, _blPlate, _brPlate;
        private readonly Transform _chestDoor;
        private readonly Renderer _pwrModuleRenderer;
        private readonly Transform _bonBonTransform;
        private readonly GameObject _babyRoot;
        private readonly Transform _babyCheekHatch;
        private readonly Light _overheadSpot;

        public PartsAndServiceRoom3D()
        {
            View = new Room3DView("PartsAndService3D", new Vector3(500f, -500f, 0f), new Vector3(0f, 1.32f, -0.45f), 56f, 28f,
                new Color(0.02f, 0.025f, 0.035f), new Color(0.01f, 0.012f, 0.018f), 0.09f);
            var v = View;

            Color wallCol = new Color(0.13f, 0.15f, 0.18f);
            Color steel = new Color(0.38f, 0.41f, 0.46f);
            Color white = new Color(0.93f, 0.93f, 0.96f);
            Color purple = new Color(0.76f, 0.28f, 0.86f);
            Color endoCol = new Color(0.26f, 0.28f, 0.32f);

            // Room shell & conveyor pedestal
            v.Box(new Vector3(0f, -0.05f, 1.5f), new Vector3(5.2f, 0.1f, 5.0f), new Color(0.08f, 0.08f, 0.1f));
            v.Box(new Vector3(0f, 3.05f, 1.5f), new Vector3(5.2f, 0.1f, 5.0f), new Color(0.05f, 0.05f, 0.07f));
            v.Box(new Vector3(-2.5f, 1.5f, 1.5f), new Vector3(0.2f, 3.0f, 5.0f), wallCol);
            v.Box(new Vector3(2.5f, 1.5f, 1.5f), new Vector3(0.2f, 3.0f, 5.0f), wallCol);
            v.Box(new Vector3(0f, 1.5f, 3.8f), new Vector3(5.2f, 3.0f, 0.2f), wallCol);

            // Conveyor belt platform
            v.Box(new Vector3(0f, 0.22f, 1.35f), new Vector3(1.35f, 0.44f, 1.6f), new Color(0.18f, 0.2f, 0.23f));
            v.Box(new Vector3(-1.35f, 0.55f, 1.55f), new Vector3(0.32f, 0.85f, 0.32f), new Color(0.16f, 0.18f, 0.22f));
            v.Shape(PrimitiveType.Cylinder, ConveyorBtn, new Vector3(0.16f, 0.03f, 0.16f), new Color(0.15f, 0.75f, 0.25f),
                null, new Vector3(90f, 0f, 0f), new Color(0.1f, 0.55f, 0.2f));

            // --- Night 3: Seated Funtime Freddy ---
            var freddy = v.Group("MaintenanceFreddy", new Vector3(0f, 0f, 1.3f));
            _freddyRoot = freddy.gameObject;
            // Torso & endo cavity inside
            v.Box(new Vector3(0f, 0.92f, 0f), new Vector3(0.66f, 0.78f, 0.46f), endoCol, freddy);
            var pwr = v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.96f, -0.22f), new Vector3(0.16f, 0.04f, 0.16f),
                Color.red, freddy, new Vector3(90f, 0f, 0f), Color.red * 2f);
            _pwrModuleRenderer = pwr.GetComponent<Renderer>();

            _chestDoor = v.Group("ChestDoor", new Vector3(-0.33f, 0.92f, -0.24f), freddy);
            v.Box(new Vector3(0.33f, 0f, 0f), new Vector3(0.66f, 0.74f, 0.05f), white, _chestDoor);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.33f, 0f, -0.02f), new Vector3(0.44f, 0.48f, 0.06f), purple, _chestDoor);
            v.Box(new Vector3(0.33f, 0.28f, -0.04f), new Vector3(0.24f, 0.09f, 0.04f), Color.black, _chestDoor);

            // Arms & legs
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.46f, 0.9f, 0f), new Vector3(0.18f, 0.36f, 0.18f), white, freddy);
            v.Shape(PrimitiveType.Capsule, new Vector3(0.46f, 0.9f, 0f), new Vector3(0.18f, 0.36f, 0.18f), white, freddy);

            // Endoskeleton head & jaw
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.64f, 0f), new Vector3(0.48f, 0.46f, 0.44f), endoCol, freddy);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.11f, 1.68f, -0.19f), Vector3.one * 0.085f, Color.black, freddy, default, new Color(0.2f, 0.65f, 1f) * 1.8f);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.11f, 1.68f, -0.19f), Vector3.one * 0.085f, Color.black, freddy, default, new Color(0.2f, 0.65f, 1f) * 1.8f);
            v.Box(new Vector3(0f, 1.42f, -0.16f), new Vector3(0.28f, 0.1f, 0.22f), steel, freddy);
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 1.98f, 0f), new Vector3(0.26f, 0.14f, 0.26f), new Color(0.1f, 0.1f, 0.12f), freddy);

            // 4 Articulated Split Faceplates
            _tlPlate = v.Box(new Vector3(-0.15f, 1.74f, -0.22f), new Vector3(0.28f, 0.24f, 0.08f), white, freddy).transform;
            _trPlate = v.Box(new Vector3(0.15f, 1.74f, -0.22f), new Vector3(0.28f, 0.24f, 0.08f), white, freddy).transform;
            _blPlate = v.Box(new Vector3(-0.15f, 1.50f, -0.22f), new Vector3(0.28f, 0.22f, 0.08f), white, freddy).transform;
            _brPlate = v.Box(new Vector3(0.15f, 1.50f, -0.22f), new Vector3(0.28f, 0.22f, 0.08f), white, freddy).transform;
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.64f, -0.28f), Vector3.one * 0.08f, Color.black, freddy);

            // Bon-Bon peeking behind Freddy's right shoulder (viewer's left, x = -0.48)
            Color bonBlue = new Color(0.22f, 0.48f, 0.92f);
            _bonBonTransform = v.Group("PeekingBonBon", new Vector3(-0.48f, 0.95f, 0.12f), freddy);
            v.Shape(PrimitiveType.Capsule, new Vector3(0f, 0.12f, 0f), new Vector3(0.22f, 0.18f, 0.2f), bonBlue, _bonBonTransform);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.34f, 0f), Vector3.one * 0.22f, bonBlue, _bonBonTransform);
            v.Box(new Vector3(-0.06f, 0.54f, 0f), new Vector3(0.06f, 0.22f, 0.04f), bonBlue, _bonBonTransform);
            v.Box(new Vector3(0.06f, 0.54f, 0f), new Vector3(0.06f, 0.22f, 0.04f), bonBlue, _bonBonTransform);
            v.Box(new Vector3(0f, 0.21f, -0.11f), new Vector3(0.12f, 0.05f, 0.03f), Color.red, _bonBonTransform);

            // --- Night 5: Seated Circus Baby ---
            var baby = v.Group("MaintenanceBaby", new Vector3(0f, 0f, 1.3f));
            _babyRoot = baby.gameObject;
            Color red = new Color(0.85f, 0.16f, 0.15f);
            Color hair = new Color(0.75f, 0.22f, 0.12f);
            v.Shape(PrimitiveType.Capsule, new Vector3(0f, 0.92f, 0f), new Vector3(0.68f, 0.48f, 0.48f), white, baby);
            v.Box(new Vector3(0f, 0.95f, -0.23f), new Vector3(0.32f, 0.36f, 0.05f), new Color(0.95f, 0.55f, 0.15f), baby);
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.48f, 0.92f, 0f), new Vector3(0.2f, 0.38f, 0.2f), white, baby);
            v.Shape(PrimitiveType.Capsule, new Vector3(0.48f, 0.92f, 0f), new Vector3(0.2f, 0.38f, 0.2f), white, baby);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.64f, 0f), new Vector3(0.58f, 0.54f, 0.52f), white, baby);
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.42f, 1.72f, 0f), new Vector3(0.22f, 0.32f, 0.22f), hair, baby);
            v.Shape(PrimitiveType.Capsule, new Vector3(0.42f, 1.72f, 0f), new Vector3(0.22f, 0.32f, 0.22f), hair, baby);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.12f, 1.68f, -0.24f), Vector3.one * 0.08f, Color.black, baby);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.12f, 1.68f, -0.24f), Vector3.one * 0.08f, Color.black, baby);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.60f, -0.28f), Vector3.one * 0.07f, red, baby);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.18f, 1.52f, -0.23f), Vector3.one * 0.08f, red, baby);
            _babyCheekHatch = v.Box(new Vector3(-0.22f, 1.52f, -0.24f), new Vector3(0.18f, 0.18f, 0.06f), steel, baby).transform;

            _overheadSpot = v.AddLight(LightType.Spot, new Vector3(0f, 2.85f, 0.2f), new Color(0.85f, 0.92f, 1f), 16f, 8f, new Vector3(52f, 0f, 0f), 78f, true);
        }

        public void SetActive(bool active) => View.SetActive(active);

        public void Sync(float deltaTime, int night, int freddyStep, float bonBonHeight, bool babyHatchOpen)
        {
            View.Update(deltaTime);

            bool isNight3 = (night == 3);
            _freddyRoot.SetActive(isNight3);
            _babyRoot.SetActive(!isNight3);

            if (isNight3)
            {
                float open = freddyStep >= 4 ? 0.14f : (freddyStep * 0.02f);
                _tlPlate.localPosition = new Vector3(-0.15f - open, 1.74f + open * 0.7f, -0.22f);
                _trPlate.localPosition = new Vector3(0.15f + open, 1.74f + open * 0.7f, -0.22f);
                _blPlate.localPosition = new Vector3(-0.15f - open, 1.50f - open * 0.7f, -0.22f);
                _brPlate.localPosition = new Vector3(0.15f + open, 1.50f - open * 0.7f, -0.22f);

                _chestDoor.localRotation = Quaternion.Euler(0f, freddyStep >= 5 ? 105f : 0f, 0f);
                _pwrModuleRenderer.gameObject.SetActive(freddyStep == 5);
                Color blink = Mathf.PingPong(Time.time * 5f, 1f) > 0.5f ? Color.red : Color.yellow;
                _pwrModuleRenderer.sharedMaterial = View.GetMaterial(blink, blink * 2f);

                _bonBonTransform.gameObject.SetActive(freddyStep == 6);
                _bonBonTransform.localPosition = new Vector3(-0.48f, Mathf.Lerp(0.88f, 1.42f, bonBonHeight), 0.02f);
            }
            else
            {
                _babyCheekHatch.localPosition = new Vector3(babyHatchOpen ? -0.36f : -0.22f, 1.52f, -0.25f);
            }

            _overheadSpot.intensity = 15f + Mathf.Sin(Time.time * 35f) * 1.2f;
            View.RenderRoom();
        }
    }
}
