using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    /// <summary>
    /// 3D Circus Control Module: observation window into Circus Gallery, futuristic control desk
    /// with 4 glowing eye-buttons, under-desk hiding space, right-side keypad, and power outage lighting.
    /// </summary>
    public sealed class CircusControlRoom3D
    {
        public readonly Room3DView View;

        public static readonly Vector3[] EyeButtons =
        {
            new Vector3(-1.05f, 0.96f, 2.15f),
            new Vector3(-0.35f, 0.96f, 2.15f),
            new Vector3(0.35f, 0.96f, 2.15f),
            new Vector3(1.05f, 0.96f, 2.15f)
        };
        public static readonly Vector3 KeypadCenter = new Vector3(2.65f, 0.95f, 1.55f);
        public static readonly Vector3 WindowLabelPos = new Vector3(0f, 1.85f, 3.0f);
        public static readonly Vector3 UnderDeskPos = new Vector3(0f, 0.42f, 2.1f);

        private static readonly Color WallCol = new Color(0.18f, 0.20f, 0.25f);
        private static readonly Color SteelCol = new Color(0.42f, 0.46f, 0.52f);
        private static readonly Color FloorCol = new Color(0.09f, 0.10f, 0.12f);

        private readonly Light _roomLight;
        private readonly Light _emergencyLight;
        private readonly Light _stageSpot;
        private readonly GameObject _bidybabAtWindow;
        private readonly Renderer _blueBtnRenderer;
        private readonly Material _blueOff, _blueOn;

        public CircusControlRoom3D()
        {
            View = new Room3DView("CircusControl3D", new Vector3(100f, -500f, 0f), new Vector3(0f, 1.3f, -0.8f), 60f, 38f,
                new Color(0.03f, 0.035f, 0.05f), new Color(0.01f, 0.012f, 0.02f), 0.065f);
            var v = View;

            // Room floor, ceiling, side walls, back wall
            v.Box(new Vector3(0f, -0.05f, 1.0f), new Vector3(6.4f, 0.1f, 4.4f), FloorCol);
            v.Box(new Vector3(0f, 3.05f, 1.0f), new Vector3(6.4f, 0.1f, 4.4f), new Color(0.06f, 0.07f, 0.09f));
            v.Box(new Vector3(-3.1f, 1.5f, 1.0f), new Vector3(0.15f, 3.0f, 4.4f), WallCol);
            v.Box(new Vector3(3.1f, 1.5f, 1.0f), new Vector3(0.15f, 3.0f, 4.4f), WallCol);
            v.Box(new Vector3(0f, 1.5f, -1.2f), new Vector3(6.4f, 3.0f, 0.15f), WallCol);

            // Front wall with wide observation window (x = -2.1..2.1, y = 1.15..2.55)
            v.Box(new Vector3(0f, 0.575f, 3.0f), new Vector3(6.4f, 1.15f, 0.15f), WallCol);
            v.Box(new Vector3(0f, 2.775f, 3.0f), new Vector3(6.4f, 0.45f, 0.15f), WallCol);
            v.Box(new Vector3(-2.65f, 1.85f, 3.0f), new Vector3(1.1f, 1.4f, 0.15f), WallCol);
            v.Box(new Vector3(2.65f, 1.85f, 3.0f), new Vector3(1.1f, 1.4f, 0.15f), WallCol);

            // Window steel frame + vertical mullions
            v.Box(new Vector3(0f, 2.58f, 2.95f), new Vector3(4.35f, 0.08f, 0.22f), SteelCol);
            v.Box(new Vector3(0f, 1.12f, 2.95f), new Vector3(4.35f, 0.08f, 0.22f), SteelCol);
            v.Box(new Vector3(-2.14f, 1.85f, 2.95f), new Vector3(0.08f, 1.5f, 0.22f), SteelCol);
            v.Box(new Vector3(2.14f, 1.85f, 2.95f), new Vector3(0.08f, 1.5f, 0.22f), SteelCol);
            v.Box(new Vector3(-0.72f, 1.85f, 2.96f), new Vector3(0.05f, 1.4f, 0.08f), SteelCol);
            v.Box(new Vector3(0.72f, 1.85f, 2.96f), new Vector3(0.05f, 1.4f, 0.08f), SteelCol);

            // Control Desk with under-desk hiding alcove and 4 glowing animatronic eye-buttons
            v.Box(new Vector3(0f, 0.88f, 2.15f), new Vector3(3.1f, 0.1f, 0.85f), new Color(0.16f, 0.18f, 0.22f));
            v.Box(new Vector3(-1.45f, 0.44f, 2.15f), new Vector3(0.2f, 0.88f, 0.8f), new Color(0.12f, 0.13f, 0.16f));
            v.Box(new Vector3(1.45f, 0.44f, 2.15f), new Vector3(0.2f, 0.88f, 0.8f), new Color(0.12f, 0.13f, 0.16f));
            // Perforated metal sheet partially slid to the side under the desk
            v.Box(new Vector3(0.55f, 0.42f, 1.78f), new Vector3(1.5f, 0.78f, 0.04f), SteelCol * 0.75f);

            Color[] eyeCols =
            {
                new Color(0.15f, 0.85f, 1.0f),
                new Color(0.2f, 0.95f, 0.35f),
                new Color(0.95f, 0.3f, 0.85f),
                new Color(1.0f, 0.85f, 0.15f)
            };
            for (int i = 0; i < 4; i++)
            {
                v.Shape(PrimitiveType.Cylinder, EyeButtons[i], new Vector3(0.32f, 0.03f, 0.32f), new Color(0.08f, 0.09f, 0.11f));
                v.Shape(PrimitiveType.Sphere, EyeButtons[i] + new Vector3(0f, 0.02f, 0f), new Vector3(0.24f, 0.09f, 0.24f),
                    eyeCols[i], null, default, eyeCols[i] * 1.4f);
            }

            // Right Keypad on an angled stand
            _blueOff = v.GetMaterial(new Color(0.1f, 0.25f, 0.7f), new Color(0.05f, 0.15f, 0.5f));
            _blueOn = v.GetMaterial(new Color(0.3f, 0.55f, 1f), new Color(0.4f, 0.75f, 2.5f));
            v.Box(new Vector3(2.72f, 0.95f, 1.55f), new Vector3(0.12f, 0.65f, 0.52f), new Color(0.14f, 0.16f, 0.20f), null, new Vector3(0f, -25f, 0f));
            var blueBtn = v.Shape(PrimitiveType.Cylinder, new Vector3(2.65f, 1.09f, 1.55f), new Vector3(0.15f, 0.03f, 0.15f), Color.white, null, new Vector3(0f, -25f, 90f));
            _blueBtnRenderer = blueBtn.GetComponent<Renderer>();
            v.Shape(PrimitiveType.Cylinder, new Vector3(2.65f, 0.81f, 1.55f), new Vector3(0.15f, 0.03f, 0.15f),
                new Color(0.85f, 0.12f, 0.12f), null, new Vector3(0f, -25f, 90f), new Color(0.65f, 0.06f, 0.06f));

            // Circus Gallery behind the observation window (z = 3.0..7.5)
            Color galCol = new Color(0.18f, 0.14f, 0.22f);
            v.Box(new Vector3(0f, -0.05f, 5.25f), new Vector3(7.0f, 0.1f, 4.5f), galCol * 0.6f);
            v.Box(new Vector3(0f, 3.25f, 5.25f), new Vector3(7.0f, 0.1f, 4.5f), galCol * 0.4f);
            v.Box(new Vector3(0f, 1.6f, 7.4f), new Vector3(7.0f, 3.3f, 0.15f), galCol);
            v.Box(new Vector3(-3.4f, 1.6f, 5.25f), new Vector3(0.15f, 3.3f, 4.5f), galCol);
            v.Box(new Vector3(3.4f, 1.6f, 5.25f), new Vector3(0.15f, 3.3f, 4.5f), galCol);

            // Empty Circus Baby stage platform + party balloons
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.18f, 5.4f), new Vector3(2.8f, 0.18f, 2.8f), new Color(0.32f, 0.22f, 0.38f));
            Color[] balloonCols = { new Color(0.95f, 0.25f, 0.3f), new Color(0.25f, 0.75f, 0.95f), new Color(0.95f, 0.85f, 0.2f), new Color(0.75f, 0.3f, 0.9f) };
            for (int b = 0; b < 6; b++)
            {
                float bx = -2.0f + b * 0.8f;
                float by = 2.0f + (b % 2) * 0.25f;
                v.Shape(PrimitiveType.Sphere, new Vector3(bx, by, 5.8f), new Vector3(0.34f, 0.42f, 0.34f), balloonCols[b % balloonCols.Length]);
                v.Shape(PrimitiveType.Cylinder, new Vector3(bx, by - 0.55f, 5.8f), new Vector3(0.015f, 0.35f, 0.015f), SteelCol);
            }

            // 1/1,000 Bidybab peeking at the window
            var bidy = v.Group("BidybabWindow", new Vector3(0f, 1.15f, 3.18f));
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), new Vector3(0.32f, 0.34f, 0.28f), new Color(0.9f, 0.86f, 0.82f), bidy);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.07f, 0.24f, -0.13f), Vector3.one * 0.055f, Color.black, bidy, default, new Color(1f, 0.85f, 0.2f) * 2.5f);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.07f, 0.24f, -0.13f), Vector3.one * 0.055f, Color.black, bidy, default, new Color(1f, 0.85f, 0.2f) * 2.5f);
            _bidybabAtWindow = bidy.gameObject;

            // Lighting
            _roomLight = v.AddLight(LightType.Point, new Vector3(0f, 2.75f, 1.2f), new Color(0.62f, 0.76f, 0.88f), 1.8f, 7.5f, default, 60f, true);
            _emergencyLight = v.AddLight(LightType.Point, new Vector3(0f, 2.75f, 1.2f), new Color(1f, 0.15f, 0.1f), 0f, 7f);
            _stageSpot = v.AddLight(LightType.Spot, new Vector3(0f, 2.85f, 3.25f), new Color(0.75f, 0.82f, 1f), 0f, 11f, new Vector3(25f, 0f, 0f), 105f, true);
        }

        public void SetActive(bool active) => View.SetActive(active);

        public void Sync(float deltaTime, bool lightOn, bool powerOutage, bool rareSpawn, float shockFlash)
        {
            View.Update(deltaTime);

            _bidybabAtWindow.SetActive(rareSpawn);
            _blueBtnRenderer.sharedMaterial = (lightOn && !powerOutage) ? _blueOn : _blueOff;

            if (powerOutage)
            {
                _roomLight.intensity = 0.05f;
                _emergencyLight.intensity = 0.6f + Mathf.Sin(Time.time * 4f) * 0.35f;
            }
            else
            {
                float buzz = 0.9f + 0.1f * Mathf.Sin(Time.time * 45f);
                _roomLight.intensity = 1.8f * buzz;
                _emergencyLight.intensity = 0f;
            }

            bool flash = shockFlash > 0f;
            _stageSpot.color = flash ? new Color(0.55f, 0.85f, 1f) : new Color(0.78f, 0.84f, 1f);
            _stageSpot.intensity = flash ? 26f * shockFlash : ((lightOn && !powerOutage) ? 15f : 0f);

            View.RenderRoom();
        }
    }
}
