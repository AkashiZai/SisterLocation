using System;
using System.Collections.Generic;
using UnityEngine;

namespace FNaFSisterLocation.Core
{
    /// <summary>
    /// Isolates all 21 scenes, enforces clean Exit() timer cleanup, and runs global RNG Easter Egg checks.
    /// </summary>
    public sealed class SceneGraphRouter
    {
        private readonly Dictionary<SceneId, IGameScene> _scenes = new Dictionary<SceneId, IGameScene>();
        private readonly GameStateService _stateService;
        private readonly ProceduralAudioSynthesizer _audio;

        public IGameScene CurrentScene { get; private set; }
        public event Action<SceneId> OnSceneChanged;

        // Global RNG Easter Egg state
        public float BlueprintOverlayTimer { get; private set; }
        public int BlueprintOverlayIndex { get; private set; }
        public bool LolbitMaskOverride { get; private set; }
        public bool RareSpawnActive { get; private set; }

        // Global Jumpscare Overlay state
        public float JumpscareTimer { get; private set; }
        public string JumpscareAnimatronicId { get; private set; } = "";
        private SceneId _postJumpscareScene = SceneId.TITLE_MENU;
        private Dictionary<string, object> _postJumpscareParams;

        private float _babyWhisperTimer = 14f;
        private static readonly string[] BlueprintTitles = new string[]
        {
            "PROJECT: CIRCUS BABY — HEIGHT: 7.2 FT | WEIGHT: 585 LBS | INTERNAL ICE CREAM DISPENSER & STORAGE TANK",
            "PROJECT: BALLORA — HEIGHT: 6.2 FT | WEIGHT: 347 LBS | AUDIO ACTIVATION SENSOR & DETERRENCE ARRAY",
            "PROJECT: FUNTIME FREDDY — HEIGHT: 6.0 FT | WEIGHT: 350 LBS | VOICE MIMIC / LURE & CHEST CAVITY STORAGE (OCCUPIED)",
            "PROJECT: FUNTIME FOXY — HEIGHT: 5.9 FT | WEIGHT: 290 LBS | LIGHT-ACTIVATED MOTION SENSOR & PARENTAL VOICE SYNC"
        };

        private static readonly string[] BlueprintAnimIds = new string[]
        {
            "circus_baby", "ballora", "funtime_freddy", "funtime_foxy"
        };

        public SceneGraphRouter(GameStateService stateService, ProceduralAudioSynthesizer audio)
        {
            _stateService = stateService;
            _audio = audio;
        }

        public void RegisterScene(IGameScene scene)
        {
            _scenes[scene.Id] = scene;
        }

        public void TransitionTo(SceneId nextId, Dictionary<string, object> parameters = null)
        {
            if (CurrentScene != null)
            {
                CurrentScene.Exit();
            }

            _audio.SetMusicBoxStage(0);
            EvaluateTransitionEasterEggs(nextId);

            if (_scenes.TryGetValue(nextId, out IGameScene nextScene))
            {
                CurrentScene = nextScene;
                CurrentScene.Enter(parameters ?? new Dictionary<string, object>());
                OnSceneChanged?.Invoke(nextId);
            }
        }

        public void TriggerJumpscare(string animatronicId, SceneId returnScene = SceneId.TITLE_MENU, Dictionary<string, object> returnParams = null)
        {
            JumpscareAnimatronicId = animatronicId;
            JumpscareTimer = 1.45f;
            _postJumpscareScene = returnScene;
            _postJumpscareParams = returnParams;
            _audio.ClearDialogue();
            _audio.PlaySfx("jumpscare", 1.0f);
        }

        public void ForceBlueprintEasterEgg(int index = -1)
        {
            BlueprintOverlayIndex = index >= 0 ? (index % 4) : UnityEngine.Random.Range(0, 4);
            BlueprintOverlayTimer = 10f;
        }

        private void EvaluateTransitionEasterEggs(SceneId nextId)
        {
            bool force = _stateService.State.debugForceEasterEggs;

            // 1 in 10,000 Blueprint overlay (only trigger automatically on room transitions or if forced)
            if (UnityEngine.Random.Range(0, 10000) == 0)
            {
                ForceBlueprintEasterEgg();
            }

            // 1 in 5,000 Lolbit mask in Primary Control
            LolbitMaskOverride = force || (UnityEngine.Random.Range(0, 5000) == 0);

            // 1 in 1,000 Rare spawn (Yenndo / Bidybab / Spinning Minireena)
            RareSpawnActive = force || (UnityEngine.Random.Range(0, 1000) == 0);
        }

        public void Update(float deltaTime)
        {
            if (BlueprintOverlayTimer > 0f)
            {
                BlueprintOverlayTimer -= deltaTime;
            }

            if (JumpscareTimer > 0f)
            {
                JumpscareTimer -= deltaTime;
                if (JumpscareTimer <= 0f)
                {
                    TransitionTo(_postJumpscareScene, _postJumpscareParams);
                }
                return;
            }

            // Ambient 1-5-7-8 slowed whisper in darkened rooms
            _babyWhisperTimer -= deltaTime;
            if (_babyWhisperTimer <= 0f)
            {
                _babyWhisperTimer = UnityEngine.Random.Range(18f, 34f);
                if (CurrentScene != null && IsDarkGameplayScene(CurrentScene.Id) && string.IsNullOrEmpty(_audio.ActiveSubtitle))
                {
                    string[] digits = { "1...", "5...", "7...", "8..." };
                    string seq = digits[UnityEngine.Random.Range(0, digits.Length)] + " " + digits[UnityEngine.Random.Range(0, digits.Length)];
                    _audio.Speak("Whisper (Slowed)", seq, VoiceProfileId.CircusBaby, 2.2f, false);
                }
            }

            CurrentScene?.Update(deltaTime);
        }

        public void Render(ProceduralCanvas canvas)
        {
            if (JumpscareTimer > 0f)
            {
                RenderJumpscare(canvas);
                return;
            }

            CurrentScene?.Render(canvas);

            if (BlueprintOverlayTimer > 0f)
            {
                RenderBlueprintOverlay(canvas);
            }
        }

        private void RenderJumpscare(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.08f, 0.02f, 0.02f));
            float shakeX = UnityEngine.Random.Range(-28f, 28f);
            float shakeY = UnityEngine.Random.Range(-24f, 24f);
            float openAmt = Mathf.PingPong(Time.time * 12f, 1.0f) + 0.35f;
            canvas.DrawAnimatronicHead(JumpscareAnimatronicId, 640f + shakeX, 360f + shakeY, 2.65f, openAmt, 1.0f);
            canvas.DrawCrtScanlinesAndStatic(0.75f, 0.4f);
        }

        private void RenderBlueprintOverlay(ProceduralCanvas canvas)
        {
            canvas.FillRect(40, 40, 1200, 640, new Color(0.04f, 0.14f, 0.38f, 0.96f));
            canvas.StrokeRect(55, 55, 1170, 610, 3f, new Color(0.45f, 0.78f, 1.0f));

            // Blueprint grid lines
            for (int gx = 80; gx < 1200; gx += 40)
                canvas.FillRect(gx, 60, 1f, 600, new Color(0.3f, 0.6f, 0.95f, 0.18f));
            for (int gy = 80; gy < 660; gy += 40)
                canvas.FillRect(60, gy, 1160, 1f, new Color(0.3f, 0.6f, 0.95f, 0.18f));

            canvas.DrawText("AFTON ROBOTICS, LLC — CONFIDENTIAL TECHNICAL BLUEPRINT (1/10,000 ARCHIVE)", 80, 75, 22, Color.cyan, TextAnchor.UpperLeft, 1000, 40);
            canvas.DrawText(BlueprintTitles[BlueprintOverlayIndex], 80, 115, 18, Color.white, TextAnchor.UpperLeft, 1100, 40);

            canvas.DrawAnimatronicHead(BlueprintAnimIds[BlueprintOverlayIndex], 420, 380, 1.65f, 0.45f, 0.25f);

            // Cross-section storage tank diagram for Baby / Freddy
            canvas.StrokeRect(720, 210, 380, 340, 2f, Color.cyan);
            canvas.DrawText("INTERNAL CAVITY SCHEMATIC:\n• A: Positronic Voice Synthesizer\n• B: Articulated Faceplate Servos\n• C: Pressurized Storage Tank\n• D: Automated Capture Claw / Lure\n\n© XXX4 AFTON ROBOTICS, LLC", 740, 230, 17, new Color(0.75f, 0.92f, 1f), TextAnchor.UpperLeft, 340, 300);

            if (BlueprintOverlayIndex == 0 || BlueprintOverlayIndex == 2)
            {
                // Child silhouette inside tank
                canvas.StrokeRect(850, 420, 120, 110, 2f, new Color(1f, 0.35f, 0.35f));
                canvas.FillRect(895, 435, 30, 30, new Color(1f, 0.35f, 0.35f, 0.55f));
                canvas.FillRect(885, 470, 50, 50, new Color(1f, 0.35f, 0.35f, 0.55f));
                canvas.DrawText("SPECIMEN DETECTED", 852, 395, 13, new Color(1f, 0.4f, 0.4f));
            }

            if (canvas.Button(1060, 75, 140, 36, "CLOSE [X]", new Color(0.1f, 0.3f, 0.6f), Color.white, 15))
            {
                BlueprintOverlayTimer = 0f;
            }
        }

        private static bool IsDarkGameplayScene(SceneId id)
        {
            return id == SceneId.PRIMARY_CONTROL || id == SceneId.BALLORA_GALLERY ||
                   id == SceneId.CIRCUS_CONTROL || id == SceneId.FUNTIME_AUDITORIUM ||
                   id == SceneId.PRIVATE_ROOM;
        }
    }
}
