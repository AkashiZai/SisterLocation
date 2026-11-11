using System.Collections.Generic;
using UnityEngine;

namespace FNaFSisterLocation.Core
{
    /// <summary>
    /// Real-time 3D room rendered offscreen and shown through a FNaF-style panorama warp,
    /// mimicking the original game's pre-rendered 3D rooms that pan left/right.
    /// The room lives on its own layer, far from the origin, so the main (2D) camera never sees it.
    /// </summary>
    public sealed class Room3DView
    {
        public const int LAYER = 31;
        private const int RT_WIDTH = 1600;
        private const int RT_HEIGHT = 900;
        private const float WARP = 0.32f;
        private const float PAN_SPEED_DEG = 75f;
        private const float EDGE_ZONE = 0.3f;

        public Transform Root { get; }
        public Camera Camera { get; }

        private readonly Transform _pivot;
        private readonly float _maxYaw;
        private float _yaw;
        private readonly RenderTexture _rt;
        private readonly RenderTexture _warpedRt;
        private readonly Material _warpMat;
        private readonly Material _baseMat;
        private readonly Dictionary<(Color, Color), Material> _materials = new Dictionary<(Color, Color), Material>();
        private readonly Color _ambient;
        private readonly Color _fogColor;
        private readonly float _fogDensity;

        public float Yaw => _yaw;
        public float MaxYaw => _maxYaw;
        public void SetEyePosition(Vector3 localPos) => _pivot.localPosition = localPos;

        public Room3DView(string name, Vector3 worldOrigin, Vector3 eyeLocalPos, float verticalFov, float maxYaw,
            Color ambient, Color fogColor, float fogDensity)
        {
            _maxYaw = maxYaw;
            _ambient = ambient;
            _fogColor = fogColor;
            _fogDensity = fogDensity;

            Root = new GameObject(name).transform;
            Root.position = worldOrigin;
            Object.DontDestroyOnLoad(Root.gameObject);

            _pivot = new GameObject("CameraPivot").transform;
            _pivot.SetParent(Root, false);
            _pivot.localPosition = eyeLocalPos;

            _rt = new RenderTexture(RT_WIDTH, RT_HEIGHT, 24) { antiAliasing = 4, name = name + "_RT" };
            _warpedRt = new RenderTexture(RT_WIDTH, RT_HEIGHT, 0) { name = name + "_Warped" };

            Camera = new GameObject("RoomCamera").AddComponent<Camera>();
            Camera.transform.SetParent(_pivot, false);
            Camera.fieldOfView = verticalFov;
            Camera.nearClipPlane = 0.05f;
            Camera.farClipPlane = 60f;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = Color.black;
            Camera.cullingMask = 1 << LAYER;
            Camera.targetTexture = _rt;
            Camera.enabled = false;

            if (UnityEngine.Camera.main != null)
            {
                UnityEngine.Camera.main.cullingMask &= ~(1 << LAYER);
            }

            Shader warp = Shader.Find("Hidden/FNaFSL/PanoramaWarp") ?? Resources.Load<Shader>("FNaFSL_PanoramaWarp");
            if (warp != null && warp.isSupported)
            {
                _warpMat = new Material(warp);
                _warpMat.SetFloat("_Warp", WARP);
            }

            // Clone the pipeline's default lit material (URP Lit in this project)
            var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _baseMat = new Material(probe.GetComponent<Renderer>().sharedMaterial);
            Object.Destroy(probe);

            Root.gameObject.SetActive(false);
        }

        public void SetActive(bool active)
        {
            Root.gameObject.SetActive(active);
            Camera.enabled = false;
            RenderSettings.fog = active;
            if (!active) return;

            _yaw = 0f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = _ambient;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = _fogColor;
            RenderSettings.fogDensity = _fogDensity;
        }

        /// <summary>
        /// FNaF-style edge scrolling: the view turns while the mouse sits in the outer 30% of the screen.
        /// Call RenderRoom() after updating lights/objects for the frame.
        /// </summary>
        public void Update(float deltaTime)
        {
            float m = Mathf.Clamp01(Input.mousePosition.x / Mathf.Max(1f, Screen.width));
            float speed = 0f;
            if (m < EDGE_ZONE) speed = -(EDGE_ZONE - m) / EDGE_ZONE;
            else if (m > 1f - EDGE_ZONE) speed = (m - (1f - EDGE_ZONE)) / EDGE_ZONE;
            _yaw = Mathf.Clamp(_yaw + speed * PAN_SPEED_DEG * deltaTime, -_maxYaw, _maxYaw);

            // Subtle breathing bob so the room never feels like a still image
            float bob = Mathf.Sin(Time.time * 1.3f) * 0.25f;
            _pivot.localRotation = Quaternion.Euler(bob, _yaw, 0f);
        }

        public void RenderRoom()
        {
            Camera.Render();
            if (_warpMat != null) Graphics.Blit(_rt, _warpedRt, _warpMat);
            else Graphics.Blit(_rt, _warpedRt);
        }

        public void Draw(ProceduralCanvas canvas)
        {
            canvas.DrawFullscreenTexture(_warpedRt);
        }

        /// <summary>
        /// Projects a room-local point to 1280x720 logical canvas space, matching the warp shader.
        /// Returns false if the point is behind the camera or off screen.
        /// </summary>
        public bool ProjectToLogical(Vector3 localPos, out Vector2 logical)
        {
            Vector3 vp = Camera.WorldToViewportPoint(Root.TransformPoint(localPos));
            logical = Vector2.zero;
            if (vp.z <= 0f || vp.x < 0f || vp.x > 1f) return false;

            if (_warpMat != null)
            {
                float x = vp.x - 0.5f;
                vp.y = 0.5f + (vp.y - 0.5f) * (1f + WARP * x * x * 4f);
            }
            logical = new Vector2(vp.x * ProceduralCanvas.LOGICAL_WIDTH, (1f - vp.y) * ProceduralCanvas.LOGICAL_HEIGHT);
            return true;
        }

        public Material GetMaterial(Color color, Color emission = default)
        {
            if (_materials.TryGetValue((color, emission), out Material mat)) return mat;
            mat = new Material(_baseMat) { color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.35f);
            if (emission.maxColorComponent > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            _materials[(color, emission)] = mat;
            return mat;
        }

        public GameObject Shape(PrimitiveType type, Vector3 localPos, Vector3 size, Color color,
            Transform parent = null, Vector3 euler = default, Color emission = default)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.layer = LAYER;
            go.transform.SetParent(parent != null ? parent : Root, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = GetMaterial(color, emission);
            return go;
        }

        public GameObject Box(Vector3 localPos, Vector3 size, Color color, Transform parent = null, Vector3 euler = default, Color emission = default)
        {
            return Shape(PrimitiveType.Cube, localPos, size, color, parent, euler, emission);
        }

        public Transform Group(string name, Vector3 localPos, Transform parent = null)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent != null ? parent : Root, false);
            t.localPosition = localPos;
            return t;
        }

        public Light AddLight(LightType type, Vector3 localPos, Color color, float intensity, float range,
            Vector3 euler = default, float spotAngle = 60f, bool shadows = false)
        {
            var go = new GameObject(type + "Light");
            go.transform.SetParent(Root, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            var light = go.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = spotAngle;
            light.cullingMask = 1 << LAYER;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            return light;
        }
    }
}
