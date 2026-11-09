using System;
using UnityEngine;

namespace FNaFSisterLocation.Core
{
    /// <summary>
    /// Zero-allocation immediate-mode procedural rendering helper for 1280x720 logical space.
    /// Draws articulated animatronics, faceplates, CRT scanlines, 8-bit sprites, and room geometry.
    /// </summary>
    public sealed class ProceduralCanvas
    {
        public const float LOGICAL_WIDTH = 1280f;
        public const float LOGICAL_HEIGHT = 720f;

        private readonly Texture2D _whiteTex;
        private readonly GUIStyle _textStyle;
        private float _scaleX = 1f;
        private float _scaleY = 1f;

        public Vector2 MouseLogicalPosition { get; private set; }
        public bool MouseJustPressed { get; private set; }
        public bool MouseHeld { get; private set; }

        public ProceduralCanvas()
        {
            _whiteTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var cols = new Color32[]
            {
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255)
            };
            _whiteTex.SetPixels32(cols);
            _whiteTex.Apply();

            _textStyle = new GUIStyle
            {
                richText = true,
                wordWrap = true
            };
        }

        public void BeginFrame(bool mouseDown, bool mouseHeld)
        {
            _scaleX = Screen.width / LOGICAL_WIDTH;
            _scaleY = Screen.height / LOGICAL_HEIGHT;

            Vector3 mp = Input.mousePosition;
            float lx = mp.x / Mathf.Max(0.001f, _scaleX);
            float ly = (Screen.height - mp.y) / Mathf.Max(0.001f, _scaleY);
            MouseLogicalPosition = new Vector2(lx, ly);
            MouseJustPressed = mouseDown;
            MouseHeld = mouseHeld;
        }

        public void FillRect(float x, float y, float w, float h, Color color)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(x * _scaleX, y * _scaleY, w * _scaleX, h * _scaleY), _whiteTex);
            GUI.color = prev;
        }

        public void StrokeRect(float x, float y, float w, float h, float thickness, Color color)
        {
            FillRect(x, y, w, thickness, color);
            FillRect(x, y + h - thickness, w, thickness, color);
            FillRect(x, y, thickness, h, color);
            FillRect(x + w - thickness, y, thickness, h, color);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, float thickness, Color color)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist < 0.5f) return;
            int steps = Mathf.Clamp(Mathf.CeilToInt(dist / (thickness * 0.65f)), 2, 160);
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                FillRect(x1 + dx * t - thickness * 0.5f, y1 + dy * t - thickness * 0.5f, thickness, thickness, color);
            }
        }

        public void DrawText(string text, float x, float y, int fontSize, Color color, TextAnchor align = TextAnchor.UpperLeft, float width = 600f, float height = 120f, bool bold = true)
        {
            _textStyle.fontSize = Mathf.Max(8, Mathf.RoundToInt(fontSize * Mathf.Min(_scaleX, _scaleY)));
            _textStyle.normal.textColor = color;
            _textStyle.alignment = align;
            _textStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            GUI.Label(new Rect(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY), text, _textStyle);
        }

        public bool Button(float x, float y, float w, float h, string label, Color bgColor, Color textColor, int fontSize = 18)
        {
            bool hovered = IsHovered(x, y, w, h);
            Color drawBg = hovered ? Color.Lerp(bgColor, Color.white, 0.22f) : bgColor;
            FillRect(x, y, w, h, drawBg);
            StrokeRect(x, y, w, h, 2f, hovered ? Color.yellow : new Color(1f, 1f, 1f, 0.35f));
            DrawText(label, x, y, fontSize, textColor, TextAnchor.MiddleCenter, w, h, true);
            return hovered && MouseJustPressed;
        }

        public bool IsHovered(float x, float y, float w, float h)
        {
            return MouseLogicalPosition.x >= x && MouseLogicalPosition.x <= x + w &&
                   MouseLogicalPosition.y >= y && MouseLogicalPosition.y <= y + h;
        }

        /// <summary>
        /// Renders an articulated Funtime Animatronic with 4 split faceplates, endoskeleton, and eye color.
        /// </summary>
        public void DrawAnimatronicHead(string animatronicId, float cx, float cy, float scale, float faceplateOpenAmount, float jawOpenAmount = 0f, bool endoOnly = false)
        {
            float spread = faceplateOpenAmount * 28f * scale;
            float r = 90f * scale;

            // Endoskeleton skull & wires underneath
            FillRect(cx - r * 0.65f, cy - r * 0.75f, r * 1.3f, r * 1.5f, new Color(0.18f, 0.19f, 0.22f));
            for (int i = -2; i <= 2; i++)
            {
                FillRect(cx + i * 18f * scale - 3f, cy - r * 0.7f, 6f * scale, r * 1.35f, new Color(0.45f, 0.48f, 0.52f));
                FillRect(cx - r * 0.6f, cy + i * 20f * scale - 2f, r * 1.2f, 4f * scale, new Color(0.32f, 0.34f, 0.38f));
            }

            // Eyes
            Color eyeCol = new Color(0.2f, 0.85f, 0.35f);
            Color shellMain = new Color(0.92f, 0.92f, 0.95f);
            Color accent = new Color(0.88f, 0.18f, 0.22f);

            switch (animatronicId)
            {
                case "circus_baby":
                    eyeCol = new Color(0.2f, 0.9f, 0.3f);
                    shellMain = new Color(0.95f, 0.94f, 0.96f);
                    accent = new Color(0.88f, 0.18f, 0.15f);
                    // Pigtails
                    FillRect(cx - r * 1.55f - spread * 0.4f, cy - r * 0.85f, r * 0.55f, r * 1.3f, new Color(0.75f, 0.22f, 0.12f));
                    FillRect(cx + r * 1.0f + spread * 0.4f, cy - r * 0.85f, r * 0.55f, r * 1.3f, new Color(0.75f, 0.22f, 0.12f));
                    break;
                case "ballora":
                    eyeCol = new Color(0.85f, 0.2f, 0.85f);
                    shellMain = new Color(0.92f, 0.92f, 0.98f);
                    accent = new Color(0.22f, 0.28f, 0.82f);
                    // Tiara
                    FillRect(cx - r * 0.45f, cy - r * 1.15f - spread * 0.5f, r * 0.9f, r * 0.25f, new Color(0.95f, 0.82f, 0.2f));
                    break;
                case "funtime_freddy":
                    eyeCol = new Color(0.2f, 0.65f, 1.0f);
                    shellMain = new Color(0.94f, 0.94f, 0.96f);
                    accent = new Color(0.78f, 0.28f, 0.88f);
                    // Top hat & ears
                    FillRect(cx - r * 0.35f, cy - r * 1.45f - spread * 0.5f, r * 0.7f, r * 0.55f, new Color(0.12f, 0.12f, 0.15f));
                    FillRect(cx - r * 0.95f - spread, cy - r * 1.1f - spread, r * 0.35f, r * 0.35f, accent);
                    FillRect(cx + r * 0.6f + spread, cy - r * 1.1f - spread, r * 0.35f, r * 0.35f, accent);
                    break;
                case "funtime_foxy":
                    eyeCol = new Color(0.98f, 0.75f, 0.12f);
                    shellMain = new Color(0.96f, 0.94f, 0.96f);
                    accent = new Color(0.95f, 0.35f, 0.68f);
                    // Fox ears
                    FillRect(cx - r * 0.95f - spread, cy - r * 1.25f - spread, r * 0.32f, r * 0.5f, accent);
                    FillRect(cx + r * 0.63f + spread, cy - r * 1.25f - spread, r * 0.32f, r * 0.5f, accent);
                    break;
                case "ennard":
                    eyeCol = new Color(0.25f, 0.75f, 1.0f);
                    shellMain = new Color(0.88f, 0.88f, 0.9f);
                    accent = new Color(0.85f, 0.15f, 0.15f);
                    // Party hat (striped green/yellow)
                    FillRect(cx - r * 0.25f, cy - r * 1.45f, r * 0.5f, r * 0.55f, new Color(0.2f, 0.75f, 0.25f));
                    FillRect(cx - r * 0.22f, cy - r * 1.25f, r * 0.44f, r * 0.12f, new Color(0.95f, 0.85f, 0.15f));
                    break;
                case "yenndo":
                    eyeCol = new Color(0.95f, 0.85f, 0.15f);
                    endoOnly = true;
                    break;
                case "lolbit":
                    eyeCol = new Color(0.98f, 0.98f, 0.98f);
                    shellMain = new Color(0.95f, 0.55f, 0.12f);
                    accent = new Color(0.68f, 0.22f, 0.85f);
                    break;
            }

            // Glowing Endoskeleton Eyes
            FillRect(cx - r * 0.48f, cy - r * 0.28f, r * 0.32f, r * 0.26f, Color.black);
            FillRect(cx + r * 0.16f, cy - r * 0.28f, r * 0.32f, r * 0.26f, Color.black);
            FillRect(cx - r * 0.40f, cy - r * 0.22f, r * 0.16f, r * 0.15f, eyeCol);
            FillRect(cx + r * 0.24f, cy - r * 0.22f, r * 0.16f, r * 0.15f, eyeCol);

            // Endoskeleton Jaw
            float jawDrop = jawOpenAmount * 22f * scale;
            FillRect(cx - r * 0.42f, cy + r * 0.25f + jawDrop, r * 0.84f, r * 0.32f, new Color(0.55f, 0.58f, 0.62f));
            for (int t = 0; t < 6; t++)
            {
                FillRect(cx - r * 0.36f + t * (r * 0.13f), cy + r * 0.18f + jawDrop, r * 0.09f, r * 0.09f, Color.white);
            }

            if (endoOnly) return;

            // 4 Articulated Split Faceplates (Top-Left, Top-Right, Bottom-Left, Bottom-Right)
            float plateW = r * 0.82f;
            float plateH = r * 0.78f;

            // Top-Left Faceplate
            FillRect(cx - plateW - spread - 2f, cy - plateH - spread - 2f, plateW, plateH * 0.55f, shellMain);
            FillRect(cx - plateW - spread - 2f, cy - plateH * 0.25f - spread, plateW * 0.35f, plateH * 0.25f, shellMain);

            // Top-Right Faceplate
            FillRect(cx + spread + 2f, cy - plateH - spread - 2f, plateW, plateH * 0.55f, shellMain);
            FillRect(cx + plateW * 0.65f + spread + 2f, cy - plateH * 0.25f - spread, plateW * 0.35f, plateH * 0.25f, shellMain);

            // Bottom-Left Faceplate + Cheek
            FillRect(cx - plateW - spread - 2f, cy + spread + 2f, plateW, plateH * 0.85f, shellMain);
            FillRect(cx - plateW * 0.75f - spread, cy + plateH * 0.15f + spread, r * 0.28f, r * 0.28f, accent);

            // Bottom-Right Faceplate + Cheek
            FillRect(cx + spread + 2f, cy + spread + 2f, plateW, plateH * 0.85f, shellMain);
            FillRect(cx + plateW * 0.45f + spread, cy + plateH * 0.15f + spread, r * 0.28f, r * 0.28f, accent);

            // Nose
            FillRect(cx - r * 0.14f, cy - r * 0.04f, r * 0.28f, r * 0.18f, animatronicId == "circus_baby" || animatronicId == "ennard" ? Color.red : Color.black);
        }

        public void DrawCrtScanlinesAndStatic(float staticIntensity, float vignetteDarkness = 0.28f)
        {
            // Horizontal scanlines
            Color scanCol = new Color(0f, 0f, 0f, 0.16f);
            for (int y = 0; y < 720; y += 6)
            {
                FillRect(0, y, LOGICAL_WIDTH, 2f, scanCol);
            }

            // Static snow bars
            if (staticIntensity > 0.01f)
            {
                int bars = Mathf.Clamp(Mathf.RoundToInt(staticIntensity * 36f), 2, 45);
                for (int i = 0; i < bars; i++)
                {
                    float by = UnityEngine.Random.Range(0f, LOGICAL_HEIGHT);
                    float bh = UnityEngine.Random.Range(2f, 14f);
                    float alpha = UnityEngine.Random.Range(0.05f, 0.35f) * staticIntensity;
                    FillRect(0f, by, LOGICAL_WIDTH, bh, new Color(0.85f, 0.9f, 0.95f, alpha));
                }
            }

            // Edge vignette
            if (vignetteDarkness > 0.01f)
            {
                Color vCol = new Color(0f, 0f, 0f, vignetteDarkness);
                FillRect(0, 0, LOGICAL_WIDTH, 32f, vCol);
                FillRect(0, LOGICAL_HEIGHT - 32f, LOGICAL_WIDTH, 32f, vCol);
                FillRect(0, 0, 40f, LOGICAL_HEIGHT, vCol);
                FillRect(LOGICAL_WIDTH - 40f, 0, 40f, LOGICAL_HEIGHT, vCol);
            }
        }
    }
}
