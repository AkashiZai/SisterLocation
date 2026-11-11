using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    /// <summary>
    /// 3D Ballora Gallery: long darkened hall with columns, checkered floor, hanging stars,
    /// Breaker Room door ahead, and 3D Ballora twirling through the darkness as the player crawls forward.
    /// </summary>
    public sealed class BalloraGalleryRoom3D
    {
        public readonly Room3DView View;

        private readonly Transform _balloraGroup;
        private readonly Light _balloraSpot;
        private readonly Light _farDoorLight;

        public BalloraGalleryRoom3D()
        {
            View = new Room3DView("BalloraGallery3D", new Vector3(200f, -500f, 0f), new Vector3(0f, 0.65f, 0.5f), 62f, 35f,
                new Color(0.02f, 0.02f, 0.04f), new Color(0.01f, 0.01f, 0.025f), 0.11f);
            var v = View;

            Color wallCol = new Color(0.12f, 0.11f, 0.18f);
            Color colCol = new Color(0.18f, 0.17f, 0.26f);
            Color white = new Color(0.9f, 0.9f, 0.94f);
            Color blue = new Color(0.25f, 0.3f, 0.78f);

            // Hallway shell (z = 0..16)
            v.Box(new Vector3(0f, -0.05f, 8f), new Vector3(7f, 0.1f, 17f), new Color(0.07f, 0.07f, 0.10f));
            v.Box(new Vector3(0f, 3.25f, 8f), new Vector3(7f, 0.1f, 17f), new Color(0.05f, 0.05f, 0.08f));
            v.Box(new Vector3(-3.4f, 1.6f, 8f), new Vector3(0.2f, 3.3f, 17f), wallCol);
            v.Box(new Vector3(3.4f, 1.6f, 8f), new Vector3(0.2f, 3.3f, 17f), wallCol);
            v.Box(new Vector3(0f, 1.6f, 16.2f), new Vector3(7f, 3.3f, 0.2f), wallCol);

            // Checkered floor tiles & side columns along the hall
            for (int z = 0; z < 16; z++)
            {
                for (int x = -3; x <= 2; x++)
                {
                    if (((x + z) & 1) == 0)
                    {
                        v.Box(new Vector3(x + 0.5f, 0.005f, z + 0.5f), new Vector3(0.96f, 0.01f, 0.96f), new Color(0.16f, 0.15f, 0.22f));
                    }
                }
                if (z % 3 == 1)
                {
                    v.Shape(PrimitiveType.Cylinder, new Vector3(-2.9f, 1.6f, z), new Vector3(0.38f, 1.6f, 0.38f), colCol);
                    v.Shape(PrimitiveType.Cylinder, new Vector3(2.9f, 1.6f, z), new Vector3(0.38f, 1.6f, 0.38f), colCol);
                    v.Box(new Vector3((z % 2 == 0 ? -1.1f : 1.1f), 2.55f, z), new Vector3(0.22f, 0.22f, 0.04f),
                        new Color(0.9f, 0.8f, 0.3f), null, new Vector3(0f, 0f, 45f));
                }
            }

            // Far Breaker Room / Control Module door
            v.Box(new Vector3(0f, 1.1f, 16.05f), new Vector3(1.4f, 2.2f, 0.1f), new Color(0.28f, 0.3f, 0.35f));
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 2.4f, 15.95f), Vector3.one * 0.14f, Color.red, null, default, new Color(1.5f, 0.2f, 0.2f));
            _farDoorLight = v.AddLight(LightType.Point, new Vector3(0f, 2.35f, 15.4f), new Color(0.45f, 0.6f, 1f), 2.2f, 6.5f);

            // 3D Ballora figure that twirls and glides across the hall
            _balloraGroup = v.Group("BalloraGliding", new Vector3(0f, 0f, 5f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0f, 1.0f, 0f), new Vector3(0.34f, 0.4f, 0.3f), blue, _balloraGroup);
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.68f, 0f), new Vector3(0.75f, 0.05f, 0.75f), new Color(0.6f, 0.7f, 0.95f), _balloraGroup);
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.08f, 0.32f, 0f), new Vector3(0.09f, 0.32f, 0.09f), white, _balloraGroup);
            v.Shape(PrimitiveType.Capsule, new Vector3(0.08f, 0.32f, 0f), new Vector3(0.09f, 0.32f, 0.09f), white, _balloraGroup);
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.3f, 1.55f, 0f), new Vector3(0.08f, 0.3f, 0.08f), blue, _balloraGroup, new Vector3(0f, 0f, -150f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0.3f, 1.55f, 0f), new Vector3(0.08f, 0.3f, 0.08f), blue, _balloraGroup, new Vector3(0f, 0f, 150f));
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.85f, 0f), new Vector3(0.4f, 0.44f, 0.4f), white, _balloraGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.92f, 0.08f), new Vector3(0.5f, 0.5f, 0.48f), new Color(0.2f, 0.25f, 0.7f), _balloraGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.08f, 1.88f, -0.18f), new Vector3(0.08f, 0.03f, 0.03f), Color.black, _balloraGroup, default, new Color(0.9f, 0.2f, 0.9f));
            v.Shape(PrimitiveType.Sphere, new Vector3(0.08f, 1.88f, -0.18f), new Vector3(0.08f, 0.03f, 0.03f), Color.black, _balloraGroup, default, new Color(0.9f, 0.2f, 0.9f));

            _balloraSpot = v.AddLight(LightType.Point, new Vector3(0f, 2.5f, 5f), new Color(0.55f, 0.45f, 1f), 2.5f, 5.5f);
        }

        public void SetActive(bool active) => View.SetActive(active);

        public void Sync(float deltaTime, float distancePct, float orbitPhase, int musicStage, bool moving)
        {
            View.Update(deltaTime);

            float playerZ = Mathf.Lerp(0.6f, 11.8f, Mathf.Clamp01(distancePct / 100f));
            float crawlBobY = 0.62f + (moving ? Mathf.Sin(Time.time * 11f) * 0.045f : 0f);
            float crawlBobX = moving ? Mathf.Cos(Time.time * 5.5f) * 0.04f : 0f;
            View.SetEyePosition(new Vector3(crawlBobX, crawlBobY, playerZ));

            float bx = Mathf.Cos(orbitPhase) * 2.2f;
            float proximity = Mathf.Sin(orbitPhase); // -1 far .. +1 very close
            float bz = playerZ + Mathf.Lerp(5.2f, 1.65f, (proximity + 1f) * 0.5f);
            _balloraGroup.localPosition = new Vector3(bx, 0f, bz);
            _balloraGroup.localRotation = Quaternion.Euler(0f, Time.time * 180f, 0f);

            _balloraSpot.transform.localPosition = new Vector3(bx, 2.4f, bz - 0.3f);
            _balloraSpot.intensity = musicStage == 3 ? 5.5f : (musicStage == 2 ? 2.8f : 1.2f);
            _balloraSpot.color = musicStage == 3 ? new Color(0.85f, 0.35f, 0.95f) : new Color(0.45f, 0.45f, 0.95f);
            _farDoorLight.intensity = 1.8f + Mathf.Sin(Time.time * 4f) * 0.4f;

            View.RenderRoom();
        }
    }

    /// <summary>
    /// 3D Breaker Room: industrial electrical room with breaker boxes, pipes, flickering overhead light,
    /// and 3D Funtime Freddy + Bon-Bon advancing out of the darkness across Phases 0..3.
    /// </summary>
    public sealed class BreakerRoom3D
    {
        public readonly Room3DView View;
        public static readonly Vector3 SpeakerButtonPos = new Vector3(1.1f, 0.95f, 2.2f);

        private readonly Transform _freddyGroup;
        private readonly Transform _headPlates;
        private readonly Light _bulbLight;
        private readonly Light _dangerLight;

        public BreakerRoom3D()
        {
            View = new Room3DView("BreakerRoom3D", new Vector3(300f, -500f, 0f), new Vector3(0f, 1.3f, -0.6f), 60f, 35f,
                new Color(0.02f, 0.025f, 0.03f), new Color(0.01f, 0.012f, 0.015f), 0.12f);
            var v = View;

            Color wallCol = new Color(0.14f, 0.15f, 0.17f);
            Color steel = new Color(0.36f, 0.39f, 0.42f);
            Color white = new Color(0.92f, 0.92f, 0.95f);
            Color purple = new Color(0.75f, 0.28f, 0.85f);

            // Industrial room shell
            v.Box(new Vector3(0f, -0.05f, 2.8f), new Vector3(5.6f, 0.1f, 7.6f), new Color(0.08f, 0.08f, 0.09f));
            v.Box(new Vector3(0f, 3.05f, 2.8f), new Vector3(5.6f, 0.1f, 7.6f), new Color(0.05f, 0.05f, 0.06f));
            v.Box(new Vector3(-2.7f, 1.5f, 2.8f), new Vector3(0.2f, 3.0f, 7.6f), wallCol);
            v.Box(new Vector3(2.7f, 1.5f, 2.8f), new Vector3(0.2f, 3.0f, 7.6f), wallCol);
            v.Box(new Vector3(0f, 1.5f, 6.5f), new Vector3(5.6f, 3.0f, 0.2f), wallCol);

            // Electrical breaker cabinets & pipes on left and right walls
            for (int b = 0; b < 4; b++)
            {
                float bz = 0.8f + b * 1.3f;
                v.Box(new Vector3(-2.45f, 1.35f, bz), new Vector3(0.35f, 1.8f, 0.85f), new Color(0.2f, 0.23f, 0.26f));
                v.Shape(PrimitiveType.Sphere, new Vector3(-2.26f, 1.95f, bz), Vector3.one * 0.06f, Color.green, null, default, Color.green * 1.5f);
                v.Shape(PrimitiveType.Cylinder, new Vector3(2.45f, 1.5f, bz), new Vector3(0.14f, 1.5f, 0.14f), steel);
            }

            // Mascot audio speaker stand in front-right
            v.Box(new Vector3(1.1f, 0.5f, 2.2f), new Vector3(0.45f, 1.0f, 0.35f), new Color(0.18f, 0.2f, 0.24f));
            v.Shape(PrimitiveType.Cylinder, SpeakerButtonPos, new Vector3(0.18f, 0.04f, 0.18f), new Color(0.2f, 0.7f, 0.3f),
                null, new Vector3(90f, 0f, 0f), new Color(0.1f, 0.5f, 0.2f));

            // 3D Funtime Freddy + Bon-Bon hand puppet facing the player (-Z)
            _freddyGroup = v.Group("FuntimeFreddy", new Vector3(0.35f, 0f, 5.5f));
            v.Shape(PrimitiveType.Capsule, new Vector3(0f, 1.05f, 0f), new Vector3(0.58f, 0.5f, 0.46f), white, _freddyGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 1.05f, -0.16f), new Vector3(0.4f, 0.44f, 0.2f), purple, _freddyGroup);
            v.Box(new Vector3(0f, 1.44f, -0.22f), new Vector3(0.22f, 0.09f, 0.05f), Color.black, _freddyGroup);
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 1.15f, -0.24f), new Vector3(0.12f, 0.02f, 0.12f), new Color(0.15f, 0.15f, 0.18f), _freddyGroup, new Vector3(90f, 0f, 0f));
            v.Shape(PrimitiveType.Capsule, new Vector3(-0.38f, 1.05f, 0f), new Vector3(0.16f, 0.36f, 0.16f), white, _freddyGroup);
            v.Shape(PrimitiveType.Capsule, new Vector3(0.38f, 1.05f, -0.15f), new Vector3(0.16f, 0.34f, 0.16f), white, _freddyGroup, new Vector3(40f, 0f, 0f));

            // Bon-Bon hand puppet on Freddy's right arm (viewer's left, +X in room or -X)
            Color bonBlue = new Color(0.22f, 0.48f, 0.92f);
            v.Shape(PrimitiveType.Capsule, new Vector3(0.42f, 1.28f, -0.35f), new Vector3(0.22f, 0.2f, 0.2f), bonBlue, _freddyGroup);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.42f, 1.52f, -0.35f), Vector3.one * 0.22f, bonBlue, _freddyGroup);
            v.Box(new Vector3(0.36f, 1.72f, -0.35f), new Vector3(0.06f, 0.22f, 0.04f), bonBlue, _freddyGroup);
            v.Box(new Vector3(0.48f, 1.72f, -0.35f), new Vector3(0.06f, 0.22f, 0.04f), bonBlue, _freddyGroup);
            v.Box(new Vector3(0.42f, 1.38f, -0.45f), new Vector3(0.12f, 0.05f, 0.03f), Color.red, _freddyGroup);

            // Freddy Head
            _headPlates = v.Group("FreddyHead", new Vector3(0f, 1.85f, 0f), _freddyGroup);
            v.Shape(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.56f, 0.5f, 0.52f), white, _headPlates);
            v.Box(new Vector3(0f, -0.06f, -0.24f), new Vector3(0.28f, 0.16f, 0.2f), white, _headPlates);
            v.Shape(PrimitiveType.Sphere, new Vector3(0f, 0.0f, -0.34f), Vector3.one * 0.08f, Color.black, _headPlates);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.11f, 0.07f, -0.23f), Vector3.one * 0.075f, Color.black, _headPlates, default, new Color(0.2f, 0.65f, 1f) * 2.5f);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.11f, 0.07f, -0.23f), Vector3.one * 0.075f, Color.black, _headPlates, default, new Color(0.2f, 0.65f, 1f) * 2.5f);
            v.Shape(PrimitiveType.Cylinder, new Vector3(0f, 0.34f, 0f), new Vector3(0.24f, 0.14f, 0.24f), new Color(0.1f, 0.1f, 0.12f), _headPlates);
            v.Shape(PrimitiveType.Sphere, new Vector3(-0.26f, 0.24f, 0f), new Vector3(0.14f, 0.14f, 0.06f), purple, _headPlates);
            v.Shape(PrimitiveType.Sphere, new Vector3(0.26f, 0.24f, 0f), new Vector3(0.14f, 0.14f, 0.06f), purple, _headPlates);

            _bulbLight = v.AddLight(LightType.Point, new Vector3(0f, 2.65f, 1.8f), new Color(0.75f, 0.82f, 0.9f), 1.6f, 7f, default, 60f, true);
            _dangerLight = v.AddLight(LightType.Point, new Vector3(-1.8f, 2.5f, 1.2f), Color.red, 0.4f, 5f);
        }

        public void SetActive(bool active) => View.SetActive(active);

        public void Sync(float deltaTime, int freddyPhase)
        {
            View.Update(deltaTime);

            float[] phaseZ = { 5.8f, 4.3f, 2.9f, 1.55f };
            int idx = Mathf.Clamp(freddyPhase, 0, 3);
            _freddyGroup.localPosition = new Vector3(0.35f, 0f, phaseZ[idx]);
            float sway = Mathf.Sin(Time.time * 3.2f) * (freddyPhase >= 3 ? 8f : 3f);
            _headPlates.localRotation = Quaternion.Euler(0f, sway, sway * 0.5f);

            float flicker = 0.85f + 0.15f * Mathf.Sin(Time.time * 38f);
            if (Random.value < 0.05f) flicker *= 0.35f;
            _bulbLight.intensity = 1.55f * flicker;

            _dangerLight.color = freddyPhase == 0 ? Color.green :
                                 (freddyPhase == 1 ? Color.yellow :
                                 (freddyPhase == 2 ? new Color(1f, 0.5f, 0f) : Color.red));
            _dangerLight.intensity = freddyPhase >= 3 ? (Mathf.PingPong(Time.time * 8f, 1f) * 2.2f) : 0.7f;

            View.RenderRoom();
        }
    }
}
