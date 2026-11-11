using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    /// <summary>
    /// 3D Primary Control Module built from primitives: the control room, both stage windows
    /// (Ballora Gallery left, Funtime Auditorium right), keypads, center vent, Ennard mask, fan and Baby heads.
    /// Room-local axes: player looks down +Z, +X is right, +Y is up.
    /// </summary>
    public sealed class PrimaryControlRoom3D
    {
        public readonly Room3DView View;

        // UI anchors (room-local) for the 2D buttons/labels overlaid on the 3D view
        public static readonly Vector3 LeftLightButton = new Vector3(-2.88f, 1.00f, 1.25f);
        public static readonly Vector3 LeftShockButton = new Vector3(-2.88f, 0.72f, 1.25f);
        public static readonly Vector3 RightLightButton = new Vector3(2.88f, 1.00f, 1.25f);
        public static readonly Vector3 RightShockButton = new Vector3(2.88f, 0.72f, 1.25f);
        public static readonly Vector3 LeftWindowLabel = new Vector3(-3f, 1.2f, 1.2f);
        public static readonly Vector3 RightWindowLabel = new Vector3(3f, 1.2f, 1.2f);
        public static readonly Vector3 EnnardNose = new Vector3(0f, 2.12f, 2.72f);
        public static readonly Vector3[] BabyHeads =
        {
            new Vector3(-2.2f, 1.08f, 2.5f), new Vector3(-1.75f, 1.08f, 2.5f), new Vector3(-1.3f, 1.08f, 2.5f)
        };

        private const float ROOM_W = 3f;      // half width
        private const float ROOM_H = 3f;
        private const float ROOM_Z0 = -1.5f;
        private const float ROOM_Z1 = 3f;
        private const float WIN_Z0 = 0.2f, WIN_Z1 = 2.3f, WIN_Y0 = 1.15f, WIN_Y1 = 2.45f;

        private static readonly Color WallCol = new Color(0.2f, 0.23f, 0.25f);
        private static readonly Color TrimCol = new Color(0.1f, 0.11f, 0.12f);
        private static readonly Color SteelCol = new Color(0.42f, 0.45f, 0.48f);
        private static readonly Color FloorCol = new Color(0.1f, 0.1f, 0.11f);
        private static readonly Color White = new Color(0.9f, 0.9f, 0.92f);

        private readonly Light _roomLamp;
        private readonly Light _leftSpot;
        private readonly Light _rightSpot;
        private readonly Transform _fanBlades;
        private readonly Transform _minireenaSpin;
        private readonly GameObject _ballora, _leftParts, _leftHanged;
        private readonly GameObject _foxy, _rightHanged;
        private readonly GameObject _ennardMask, _lolbitMask;
        private readonly Renderer _leftBlue, _rightBlue;
        private readonly Material _blueOff, _blueOn;
        private readonly Color _leftSpotCol = new Color(0.65f, 0.65f, 1f);
        private readonly Color _rightSpotCol = new Color(1f, 0.6f, 0.9f);

        public PrimaryControlRoom3D()
        {
            View = new Room3DView("PrimaryControl3D", new Vector3(0f, -500f, 0f), new Vector3(0f, 1.3f, -1.0f), 62f, 45f,
                new Color(0.035f, 0.04f, 0.05f), new Color(0.01f, 0.012f, 0.016f), 0.07f);
            var v = View;

            // --- Control room shell ---
            v.Box(new Vector3(0f, -0.05f, 0.75f), new Vector3(ROOM_W * 2f, 0.1f, ROOM_Z1 - ROOM_Z0), FloorCol);
            v.Box(new Vector3(0f, ROOM_H + 0.05f, 0.75f), new Vector3(ROOM_W * 2f, 0.1f, ROOM_Z1 - ROOM_Z0), new Color(0.07f, 0.075f, 0.08f));
            WallBack(ROOM_Z1, new Rect(-0.55f, 0.12f, 1.1f, 0.9f));                 // front wall + center vent
            WallSide(-ROOM_W, new Rect(WIN_Z0, WIN_Y0, WIN_Z1 - WIN_Z0, WIN_Y1 - WIN_Y0));
            WallSide(ROOM_W, new Rect(WIN_Z0, WIN_Y0, WIN_Z1 - WIN_Z0, WIN_Y1 - WIN_Y0));
            v.Box(new Vector3(0f, 1.5f, ROOM_Z0), new Vector3(ROOM_W * 2f, ROOM_H, 0.15f), WallCol);

            // Wall trim strips & ceiling pipes
            foreach (float y in new[] { 0.12f, 2.85f })
            {
                v.Box(new Vector3(0f, y, ROOM_Z1 - 0.09f), new Vector3(ROOM_W * 2f, 0.06f, 0.04f), TrimCol);
                v.Box(new Vector3(-ROOM_W + 0.09f, y, 0.75f), new Vector3(0.04f, 0.06f, ROOM_Z1 - ROOM_Z0), TrimCol);
                v.Box(new Vector3(ROOM_W - 0.09f, y, 0.75f), new Vector3(0.04f, 0.06f, ROOM_Z1 - ROOM_Z0), TrimCol);
            }
            for (int p = 0; p < 3; p++)
            {
                v.Shape(PrimitiveType.Cylinder, new Vector3(-1.2f + p * 0.25f, 2.85f, 0.75f), new Vector3(0.09f, 2.2f, 0.09f), SteelCol * 0.6f, null, new Vector3(90f, 0f, 0f));
            }

            // Center vent: recessed dark tunnel + steel frame
            v.Box(new Vector3(0f, 0.57f, ROOM_Z1 + 0.6f), new Vector3(1.1f, 0.9f, 1.2f), new Color(0.03f, 0.03f, 0.035f));
            Frame(new Vector3(0f, 0.57f, ROOM_Z1 - 0.08f), 1.1f, 0.9f, false);

            // Counters either side of the vent
            v.Box(new Vector3(-1.75f, 0.45f, 2.6f), new Vector3(2.2f, 0.9f, 0.7f), new Color(0.15f, 0.16f, 0.17f));
            v.Box(new Vector3(1.75f, 0.45f, 2.6f), new Vector3(2.2f, 0.9f, 0.7f), new Color(0.15f, 0.16f, 0.17f));
            v.Box(new Vector3(-1.75f, 0.91f, 2.58f), new Vector3(2.24f, 0.03f, 0.76f), SteelCol * 0.7f);
            v.Box(new Vector3(1.75f, 0.91f, 2.58f), new Vector3(2.24f, 0.03f, 0.76f), SteelCol * 0.7f);

            // Desk fan (right counter)
            v.Shape(PrimitiveType.Cylinder, new Vector3(1.5f, 0.95f, 2.5f), new Vector3(0.25f, 0.02f, 0.25f), TrimCol);
            v.Shape(PrimitiveType.Cylinder, new Vector3(1.5f, 1.1f, 2.5f), new Vector3(0.04f, 0.15f, 0.04f), TrimCol);
            _fanBlades = v.Group("FanBlades", new Vector3(1.5f, 1.3f, 2.42f));
            v.Shape(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.07f, TrimCol, _fanBlades);
            for (int b = 0; b < 3; b++)
            {
                v.Box(Vector3.zero, new Vector3(0.36f, 0.08f, 0.01f), SteelCol, _fanBlades, new Vector3(0f, 0f, b * 60f));
            }

            // Three Baby heads (see / hear / speak no evil)
            foreach (Vector3 pos in BabyHeads) BuildBabyHead(pos);

            // Ennard mask (or 1/5,000 Lolbit mask) above the vent
            _ennardMask = BuildMask(new Color(0.92f, 0.92f, 0.94f), new Color(0.85f, 0.12f, 0.12f), new Color(0.25f, 0.6f, 1f), true);
            _lolbitMask = BuildMask(new Color(0.95f, 0.55f, 0.12f), new Color(0.6f, 0.2f, 0.8f), new Color(0.9f, 0.9f, 0.9f), false);

            // Keypads under each window
            _blueOff = v.GetMaterial(new Color(0.1f, 0.25f, 0.7f), new Color(0.05f, 0.15f, 0.5f));
            _blueOn = v.GetMaterial(new Color(0.3f, 0.5f, 1f), new Color(0.4f, 0.7f, 2.5f));
            _leftBlue = BuildKeypad(-1f, LeftLightButton, LeftShockButton);
            _rightBlue = BuildKeypad(1f, RightLightButton, RightShockButton);

            // Rare 1/1,000 spinning Minireena on the floor
            _minireenaSpin = v.Group("Minireena", new Vector3(-0.9f, 0f, 1.9f));
            BuildMinireena(_minireenaSpin, Vector3.zero, 1.3f);

            // --- Stage galleries behind the windows ---
            BuildGallery(-1f, new Color(0.13f, 0.12f, 0.2f));
            BuildGallery(1f, new Color(0.2f, 0.11f, 0.18f));

            var balloraGroup = v.Group("Ballora", new Vector3(-5.2f, 0.3f, 1.25f));
            balloraGroup.localRotation = Quaternion.Euler(0f, 90f, 0f);
            BuildBallora(balloraGroup);
            _ballora = balloraGroup.gameObject;

            var parts = v.Group("BalloraParts", new Vector3(-5.2f, 0.3f, 1.25f));
            parts.localRotation = Quaternion.Euler(0f, 90f, 0f);
            BuildMinireena(parts, new Vector3(-0.45f, 0f, 0f), 1.4f);
            BuildMinireena(parts, new Vector3(0.45f, 0f, 0.1f), 1.4f);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0.2f), Vector3.one * 0.42f, White, parts);
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.45f, 0.75f, 0.1f), new Vector3(0.08f, 0.3f, 0.08f), new Color(0.25f, 0.3f, 0.75f), parts, new Vector3(0f, 0f, 70f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0.45f, 0.75f, 0.1f), new Vector3(0.08f, 0.3f, 0.08f), new Color(0.25f, 0.3f, 0.75f), parts, new Vector3(0f, 0f, -70f));
            _leftParts = parts.gameObject;

            var foxyGroup = v.Group("FuntimeFoxy", new Vector3(5.2f, 0.3f, 1.25f));
            foxyGroup.localRotation = Quaternion.Euler(0f, -90f, 0f);
            BuildFoxy(foxyGroup);
            _foxy = foxyGroup.gameObject;

            _leftHanged = BuildHangedTechnician(new Vector3(-5.2f, 2.95f, 1.25f));
            _rightHanged = BuildHangedTechnician(new Vector3(5.2f, 2.95f, 1.25f));

            // --- Lighting ---
            _roomLamp = v.AddLight(LightType.Point, new Vector3(0f, 2.75f, 1.1f), new Color(0.6f, 0.78f, 0.82f), 1.8f, 7.5f, default, 60f, true);
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 2.93f, 1.1f), new Vector3(0.5f, 0.03f, 0.5f), White, null, default, new Color(0.8f, 1f, 1f));
            _leftSpot = v.AddLight(LightType.Spot, new Vector3(-3.35f, 2.75f, 1.25f), _leftSpotCol, 0f, 10f, new Vector3(32f, -90f, 0f), 105f, true);
            _rightSpot = v.AddLight(LightType.Spot, new Vector3(3.35f, 2.75f, 1.25f), _rightSpotCol, 0f, 10f, new Vector3(32f, 90f, 0f), 105f, true);
        }

        public void SetActive(bool active) => View.SetActive(active);

        /// <summary>Mirrors the 2D scene's gameplay state into the 3D room each frame.</summary>
        public void Sync(float deltaTime, int night, bool leftLight, bool rightLight, int leftShocks, int rightShocks,
            bool lolbitMask, bool rareSpawn, float shockFlash)
        {
            View.Update(deltaTime);

            _ballora.SetActive((night == 1 || night == 2) && leftShocks >= 1);
            _leftParts.SetActive(night == 3);
            _leftHanged.SetActive(night == 5);
            _foxy.SetActive((night == 1 && rightShocks >= 2) || (night == 2 && rightShocks >= 1));
            _rightHanged.SetActive(night == 5);
            _ennardMask.SetActive(!lolbitMask);
            _lolbitMask.SetActive(lolbitMask);
            _minireenaSpin.gameObject.SetActive(rareSpawn);
            _minireenaSpin.localRotation = Quaternion.Euler(0f, Time.time * 360f, 0f);

            _leftBlue.sharedMaterial = leftLight ? _blueOn : _blueOff;
            _rightBlue.sharedMaterial = rightLight ? _blueOn : _blueOff;

            // Stage spotlights; a shock floods both galleries with electric blue
            bool flash = shockFlash > 0f;
            Color shockCol = new Color(0.55f, 0.85f, 1f);
            _leftSpot.color = flash ? shockCol : _leftSpotCol;
            _rightSpot.color = flash ? shockCol : _rightSpotCol;
            _leftSpot.intensity = flash ? 24f * shockFlash : (leftLight ? 14f : 0f);
            _rightSpot.intensity = flash ? 24f * shockFlash : (rightLight ? 14f : 0f);

            // Buzzing fluorescent lamp: slow drift plus occasional drop-outs
            float buzz = Mathf.PerlinNoise(Time.time * 5f, 0.71f);
            float lamp = 1.6f * (0.85f + 0.15f * Mathf.Sin(Time.time * 50f));
            if (buzz > 0.75f && Random.value < 0.5f) lamp *= 0.25f;
            _roomLamp.intensity = lamp;

            _fanBlades.localRotation = Quaternion.Euler(0f, 0f, -Time.time * 900f);
            View.RenderRoom();
        }

        // ---------- Builders ----------

        /// <summary>Front wall at depth z, with a rectangular hole (x = left, y = bottom, width, height).</summary>
        private void WallBack(float z, Rect hole)
        {
            float w = ROOM_W * 2f;
            View.Box(new Vector3(0f, hole.y * 0.5f, z), new Vector3(w, hole.y, 0.15f), WallCol);
            View.Box(new Vector3(0f, (hole.yMax + ROOM_H) * 0.5f, z), new Vector3(w, ROOM_H - hole.yMax, 0.15f), WallCol);
            float leftW = hole.x + ROOM_W;
            View.Box(new Vector3(-ROOM_W + leftW * 0.5f, hole.center.y, z), new Vector3(leftW, hole.height, 0.15f), WallCol);
            float rightW = ROOM_W - hole.xMax;
            View.Box(new Vector3(ROOM_W - rightW * 0.5f, hole.center.y, z), new Vector3(rightW, hole.height, 0.15f), WallCol);
        }

        /// <summary>Side wall at x, with a hole in z/y (x = near z, y = bottom, width = depth, height).</summary>
        private void WallSide(float x, Rect hole)
        {
            float d = ROOM_Z1 - ROOM_Z0;
            float zc = (ROOM_Z0 + ROOM_Z1) * 0.5f;
            View.Box(new Vector3(x, hole.y * 0.5f, zc), new Vector3(0.15f, hole.y, d), WallCol);
            View.Box(new Vector3(x, (hole.yMax + ROOM_H) * 0.5f, zc), new Vector3(0.15f, ROOM_H - hole.yMax, d), WallCol);
            float nearD = hole.x - ROOM_Z0;
            View.Box(new Vector3(x, hole.center.y, ROOM_Z0 + nearD * 0.5f), new Vector3(0.15f, hole.height, nearD), WallCol);
            float farD = ROOM_Z1 - hole.xMax;
            View.Box(new Vector3(x, hole.center.y, ROOM_Z1 - farD * 0.5f), new Vector3(0.15f, hole.height, farD), WallCol);

            // Window frame + center mullion
            Frame(new Vector3(x, hole.center.y, hole.center.x), hole.width, hole.height, true);
            View.Box(new Vector3(x, hole.center.y, hole.center.x), new Vector3(0.06f, hole.height, 0.05f), SteelCol);
        }

        private void Frame(Vector3 center, float width, float height, bool sideWall)
        {
            const float t = 0.07f;
            Vector3 along = sideWall ? Vector3.forward : Vector3.right;
            Vector3 depth = sideWall ? new Vector3(0.22f, 0f, 0f) : new Vector3(0f, 0f, 0.22f);
            Vector3 horiz = along * (width + t * 2f) + Vector3.up * t + depth;
            Vector3 vert = along * t + Vector3.up * height + depth;
            View.Box(center + Vector3.up * (height * 0.5f + t * 0.5f), horiz, SteelCol);
            View.Box(center - Vector3.up * (height * 0.5f + t * 0.5f), horiz, SteelCol);
            View.Box(center + along * (width * 0.5f + t * 0.5f), vert, SteelCol);
            View.Box(center - along * (width * 0.5f + t * 0.5f), vert, SteelCol);
        }

        private Renderer BuildKeypad(float side, Vector3 lightBtn, Vector3 shockBtn)
        {
            float x = side * (ROOM_W - 0.1f);
            View.Box(new Vector3(x, 0.86f, 1.25f), new Vector3(0.08f, 0.62f, 0.48f), new Color(0.13f, 0.14f, 0.16f));
            View.Box(new Vector3(x - side * 0.045f, 0.86f, 1.25f), new Vector3(0.01f, 0.56f, 0.42f), new Color(0.06f, 0.06f, 0.07f));
            var blue = View.Shape(PrimitiveType.Cylinder, lightBtn, new Vector3(0.14f, 0.03f, 0.14f), Color.white, null, new Vector3(0f, 0f, 90f));
            View.Shape(PrimitiveType.Cylinder, shockBtn, new Vector3(0.14f, 0.03f, 0.14f), new Color(0.8f, 0.1f, 0.1f), null,
                new Vector3(0f, 0f, 90f), new Color(0.6f, 0.05f, 0.05f));
            return blue.GetComponent<Renderer>();
        }

        private void BuildGallery(float side, Color tint)
        {
            float cx = side * 5.25f;
            View.Box(new Vector3(cx, -0.05f, 1.25f), new Vector3(4.5f, 0.1f, 5f), tint * 0.6f);
            View.Box(new Vector3(cx, 3.05f, 1.25f), new Vector3(4.5f, 0.1f, 5f), tint * 0.4f);
            View.Box(new Vector3(side * 7.5f, 1.5f, 1.25f), new Vector3(0.15f, 3f, 5f), tint);
            View.Box(new Vector3(cx, 1.5f, -1.25f), new Vector3(4.5f, 3f, 0.15f), tint);
            View.Box(new Vector3(cx, 1.5f, 3.75f), new Vector3(4.5f, 3f, 0.15f), tint);
            // Round stage platform
            View.Shape(PrimitiveType.Cylinder, new Vector3(cx, 0.15f, 1.25f), new Vector3(2.2f, 0.15f, 2.2f), tint * 1.6f);
            // Hanging stars / decorations
            for (int s = 0; s < 4; s++)
            {
                float z = -0.4f + s * 1.1f;
                View.Box(new Vector3(cx + side * 0.8f, 2.4f - (s % 2) * 0.3f, z), new Vector3(0.2f, 0.2f, 0.04f),
                    new Color(0.9f, 0.8f, 0.3f), null, new Vector3(0f, 90f, 45f));
            }
        }

        private void BuildBabyHead(Vector3 pos)
        {
            var g = View.Group("BabyHead", pos);
            View.Shape(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.24f, 0.26f, 0.24f), White, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.13f, 0.08f, -0.02f), Vector3.one * 0.1f, new Color(0.8f, 0.25f, 0.12f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.13f, 0.08f, -0.02f), Vector3.one * 0.1f, new Color(0.8f, 0.25f, 0.12f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.05f, 0.03f, -0.1f), Vector3.one * 0.035f, Color.black, g, default, new Color(0.1f, 0.9f, 0.3f));
            View.Shape(PrimitiveType.Sphere, new Vector3(0.05f, 0.03f, -0.1f), Vector3.one * 0.035f, Color.black, g, default, new Color(0.1f, 0.9f, 0.3f));
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.07f, -0.04f, -0.09f), Vector3.one * 0.04f, new Color(0.9f, 0.3f, 0.35f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.07f, -0.04f, -0.09f), Vector3.one * 0.04f, new Color(0.9f, 0.3f, 0.35f), g);
        }

        /// <summary>Wall-mounted mask facing the player (front = -Z).</summary>
        private GameObject BuildMask(Color face, Color accent, Color eyeGlow, bool partyHat)
        {
            var g = View.Group("Mask", new Vector3(0f, 2.15f, ROOM_Z1 - 0.12f));
            View.Shape(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.48f, 0.44f, 0.28f), face, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.1f, 0.05f, -0.11f), new Vector3(0.11f, 0.09f, 0.06f), Color.black, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.1f, 0.05f, -0.11f), new Vector3(0.11f, 0.09f, 0.06f), Color.black, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.1f, 0.05f, -0.14f), Vector3.one * 0.035f, Color.black, g, default, eyeGlow * 2f);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.1f, 0.05f, -0.14f), Vector3.one * 0.035f, Color.black, g, default, eyeGlow * 2f);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, -0.03f, -0.15f), Vector3.one * 0.06f, accent, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.15f, -0.08f, -0.09f), new Vector3(0.07f, 0.07f, 0.03f), accent, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.15f, -0.08f, -0.09f), new Vector3(0.07f, 0.07f, 0.03f), accent, g);
            View.Box(new Vector3(0f, -0.13f, -0.11f), new Vector3(0.18f, 0.03f, 0.03f), new Color(0.15f, 0.05f, 0.05f), g);
            if (partyHat)
            {
                for (int i = 0; i < 4; i++)
                {
                    Color c = i % 2 == 0 ? new Color(0.2f, 0.7f, 0.25f) : new Color(0.95f, 0.85f, 0.15f);
                    float r = 0.18f - i * 0.04f;
                    View.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.24f + i * 0.06f, 0f), new Vector3(r, 0.03f, r), c, g);
                }
            }
            return g.gameObject;
        }

        // Figures below are built facing +Z in their own group, then the group is rotated toward the player.

        private void BuildBallora(Transform g)
        {
            Color blue = new Color(0.25f, 0.3f, 0.75f);
            View.Shape(PrimitiveType.Capsule, new Vector3(0f, 1.0f, 0f), new Vector3(0.34f, 0.4f, 0.3f), blue, g);
            View.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.68f, 0f), new Vector3(0.75f, 0.05f, 0.75f), new Color(0.6f, 0.7f, 0.95f), g);
            View.Shape(PrimitiveType.Capsule, new Vector3(-0.08f, 0.32f, 0f), new Vector3(0.09f, 0.32f, 0.09f), White, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(0.08f, 0.32f, 0f), new Vector3(0.09f, 0.32f, 0.09f), White, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(-0.3f, 1.55f, 0f), new Vector3(0.08f, 0.3f, 0.08f), blue, g, new Vector3(0f, 0f, -150f));
            View.Shape(PrimitiveType.Capsule, new Vector3(0.3f, 1.55f, 0f), new Vector3(0.08f, 0.3f, 0.08f), blue, g, new Vector3(0f, 0f, 150f));
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.85f, 0f), new Vector3(0.4f, 0.44f, 0.4f), White, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.92f, -0.08f), new Vector3(0.5f, 0.5f, 0.48f), new Color(0.2f, 0.25f, 0.7f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.08f, 1.88f, 0.18f), new Vector3(0.08f, 0.025f, 0.03f), Color.black, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.08f, 1.88f, 0.18f), new Vector3(0.08f, 0.025f, 0.03f), Color.black, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.11f, 1.79f, 0.16f), Vector3.one * 0.06f, new Color(0.9f, 0.4f, 0.6f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.11f, 1.79f, 0.16f), Vector3.one * 0.06f, new Color(0.9f, 0.4f, 0.6f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.73f, 0.19f), new Vector3(0.08f, 0.03f, 0.03f), new Color(0.5f, 0.2f, 0.6f), g);
        }

        private void BuildFoxy(Transform g)
        {
            Color pink = new Color(0.95f, 0.4f, 0.65f);
            Color eye = new Color(1f, 0.75f, 0.1f);
            View.Shape(PrimitiveType.Capsule, new Vector3(0f, 1.0f, 0f), new Vector3(0.45f, 0.42f, 0.38f), White, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.0f, 0.12f), new Vector3(0.32f, 0.38f, 0.2f), pink, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(-0.12f, 0.3f, 0f), new Vector3(0.12f, 0.32f, 0.12f), White, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(0.12f, 0.3f, 0f), new Vector3(0.12f, 0.32f, 0.12f), White, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(-0.3f, 1.05f, 0f), new Vector3(0.1f, 0.32f, 0.1f), White, g, new Vector3(0f, 0f, 12f));
            View.Shape(PrimitiveType.Capsule, new Vector3(0.3f, 1.05f, 0f), new Vector3(0.1f, 0.32f, 0.1f), White, g, new Vector3(0f, 0f, -12f));
            View.Box(new Vector3(0f, 1.42f, 0.17f), new Vector3(0.18f, 0.07f, 0.04f), new Color(0.8f, 0.1f, 0.15f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.78f, 0f), new Vector3(0.48f, 0.44f, 0.46f), White, g);
            View.Box(new Vector3(0f, 1.72f, 0.24f), new Vector3(0.2f, 0.14f, 0.22f), White, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.77f, 0.36f), Vector3.one * 0.07f, Color.black, g);
            View.Box(new Vector3(-0.15f, 2.05f, 0f), new Vector3(0.1f, 0.24f, 0.06f), pink, g, new Vector3(0f, 0f, 15f));
            View.Box(new Vector3(0.15f, 2.05f, 0f), new Vector3(0.1f, 0.24f, 0.06f), pink, g, new Vector3(0f, 0f, -15f));
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.09f, 1.86f, 0.2f), Vector3.one * 0.065f, Color.black, g, default, eye * 2.5f);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.09f, 1.86f, 0.2f), Vector3.one * 0.065f, Color.black, g, default, eye * 2.5f);
            View.Shape(PrimitiveType.Sphere, new Vector3(-0.15f, 1.7f, 0.17f), Vector3.one * 0.07f, pink, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0.15f, 1.7f, 0.17f), Vector3.one * 0.07f, pink, g);
        }

        private void BuildMinireena(Transform parent, Vector3 pos, float scale)
        {
            var g = View.Group("Minireena", pos, parent);
            g.localScale = Vector3.one * scale;
            View.Shape(PrimitiveType.Capsule, new Vector3(0f, 0.22f, 0f), new Vector3(0.12f, 0.16f, 0.1f), White, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.45f, 0f), Vector3.one * 0.13f, White, g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.48f, -0.02f), new Vector3(0.14f, 0.1f, 0.13f), new Color(0.2f, 0.25f, 0.7f), g);
            View.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.27f, 0f), new Vector3(0.24f, 0.015f, 0.24f), new Color(0.6f, 0.7f, 0.95f), g);
        }

        private GameObject BuildHangedTechnician(Vector3 ropeTop)
        {
            var g = View.Group("HangedTechnician", ropeTop);
            Color dark = new Color(0.05f, 0.05f, 0.06f);
            View.Shape(PrimitiveType.Cylinder, new Vector3(0f, -0.45f, 0f), new Vector3(0.02f, 0.45f, 0.02f), new Color(0.35f, 0.3f, 0.22f), g);
            View.Shape(PrimitiveType.Sphere, new Vector3(0f, -1.02f, 0f), Vector3.one * 0.24f, dark, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(0f, -1.5f, 0f), new Vector3(0.36f, 0.38f, 0.26f), dark, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(-0.09f, -2.15f, 0f), new Vector3(0.12f, 0.38f, 0.12f), dark, g);
            View.Shape(PrimitiveType.Capsule, new Vector3(0.09f, -2.15f, 0f), new Vector3(0.12f, 0.38f, 0.12f), dark, g);
            g.localRotation = Quaternion.Euler(0f, 0f, 4f);
            return g.gameObject;
        }
    }
}
