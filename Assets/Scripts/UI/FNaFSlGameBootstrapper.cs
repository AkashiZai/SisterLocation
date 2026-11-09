using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;
using FNaFSisterLocation.Gameplay;

namespace FNaFSisterLocation.UI
{
    /// <summary>
    /// Self-assembling entry point for Five Nights at Freddy's: Sister Location.
    /// Wires all 21 scenes, procedural audio synthesis, subtitle overlay, and developer debug bar.
    /// </summary>
    public sealed class FNaFSlGameBootstrapper : MonoBehaviour
    {
        private static FNaFSlGameBootstrapper _instance;

        private GameStateService _state;
        private ProceduralAudioSynthesizer _audio;
        private ProceduralCanvas _canvas;
        private SceneGraphRouter _router;

        private bool _mousePressedThisFrame;
        private bool _debugBarExpanded = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (_instance != null) return;

            if (Camera.main == null)
            {
                var camObj = new GameObject("FNaF_SL_MainCamera");
                var cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
                DontDestroyOnLoad(camObj);
            }
            else if (FindAnyObjectByType<AudioListener>() == null)
            {
                Camera.main.gameObject.AddComponent<AudioListener>();
            }

            var host = new GameObject("FNaF_SisterLocation_Host");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<FNaFSlGameBootstrapper>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Application.targetFrameRate = 60;

            _state = new GameStateService();
            _audio = new ProceduralAudioSynthesizer(gameObject);
            _canvas = new ProceduralCanvas();
            _router = new SceneGraphRouter(_state, _audio);

            RegisterAll21Scenes();
            _router.TransitionTo(SceneId.OPENING_CUTSCENE);
        }

        private void RegisterAll21Scenes()
        {
            _router.RegisterScene(new OpeningCutsceneScene(_router, _audio));
            _router.RegisterScene(new TitleMenuScene(_router, _state, _audio));
            _router.RegisterScene(new ExtrasMenuScene(_router, _state, _audio));
            _router.RegisterScene(new PreNightInterludeScene(_router, _state, _audio));
            _router.RegisterScene(new ElevatorScene(_router, _state, _audio));
            _router.RegisterScene(new VentCrawlScene(_router, _audio));
            _router.RegisterScene(new PrimaryControlScene(_router, _state, _audio));
            _router.RegisterScene(new BalloraGalleryScene(_router, _state, _audio));
            _router.RegisterScene(new BreakerRoomScene(_router, _audio));
            _router.RegisterScene(new CircusControlScene(_router, _state, _audio));
            _router.RegisterScene(new UnderDeskScene(_router, _audio));
            _router.RegisterScene(new FuntimeAuditoriumScene(_router, _state, _audio));
            _router.RegisterScene(new PartsAndServiceScene(_router, _audio));
            _router.RegisterScene(new SpringlockSuitScene(_router, _state, _audio));
            _router.RegisterScene(new ScoopingRoomScene(_router, _state, _audio));
            _router.RegisterScene(new PrivateRoomScene(_router, _state, _audio));
            _router.RegisterScene(new TvLivingRoomScene(_router, _state, _audio));
            _router.RegisterScene(new Baby8BitMinigameScene(_router, _state, _audio));
            _router.RegisterScene(new CustomNightMenuScene(_router, _state, _audio));
            _router.RegisterScene(new Decay8BitCutsceneScene(_router, _state, _audio));
            _router.RegisterScene(new SpringtrapFinaleScene(_router, _state, _audio));
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mousePressedThisFrame = true;
            }

            float dt = Time.deltaTime;
            _audio.Update(dt);
            _router.Update(dt);
        }

        private void OnGUI()
        {
            if (_canvas == null || _router == null) return;

            bool consumePress = false;
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseDown)
            {
                consumePress = _mousePressedThisFrame;
            }

            _canvas.BeginFrame(consumePress, Input.GetMouseButton(0));

            if (consumePress && Event.current.type == EventType.Repaint)
            {
                _mousePressedThisFrame = false;
            }

            _router.Render(_canvas);
            RenderSubtitleBar();
            RenderDeveloperDebugToolbar();
        }

        private void RenderSubtitleBar()
        {
            if (string.IsNullOrEmpty(_audio.ActiveSubtitle)) return;

            _canvas.FillRect(140, 652, 1000, 56, new Color(0.02f, 0.03f, 0.06f, 0.88f));
            _canvas.StrokeRect(140, 652, 1000, 56, 2f, new Color(0.35f, 0.65f, 0.95f, 0.65f));
            string formatted = $"[{_audio.ActiveSpeaker}]: \"{_audio.ActiveSubtitle}\"";
            _canvas.DrawText(formatted, 156, 656, 16, new Color(0.95f, 0.96f, 1.0f), TextAnchor.MiddleCenter, 968, 48, true);
        }

        private void RenderDeveloperDebugToolbar()
        {
            string toggleLabel = _debugBarExpanded ? "DEV DEBUG [HIDE ▲]" : "DEV DEBUG / RNG [▼]";
            if (_canvas.Button(1095, 4, 178, 26, toggleLabel, new Color(0.14f, 0.14f, 0.22f, 0.9f), Color.cyan, 12))
            {
                _debugBarExpanded = !_debugBarExpanded;
            }

            if (!_debugBarExpanded) return;

            _canvas.FillRect(480, 32, 792, 76, new Color(0.04f, 0.05f, 0.10f, 0.95f));
            _canvas.StrokeRect(480, 32, 792, 76, 2f, Color.cyan);

            bool rngForced = _state.State.debugForceEasterEggs;
            string rngText = rngForced ? "100% RNG EGGS: ON" : "100% RNG EGGS: OFF";
            Color rngCol = rngForced ? new Color(0.15f, 0.55f, 0.22f) : new Color(0.25f, 0.18f, 0.18f);
            if (_canvas.Button(490, 38, 175, 28, rngText, rngCol, Color.white, 12))
            {
                _state.State.debugForceEasterEggs = !_state.State.debugForceEasterEggs;
                _state.Save();
            }

            bool fastMode = _state.State.fastTimerMode;
            string timerText = fastMode ? "TIMER: FAST" : "TIMER: CANON";
            if (_canvas.Button(672, 38, 130, 28, timerText, new Color(0.2f, 0.28f, 0.42f), Color.white, 12))
            {
                _state.State.fastTimerMode = !_state.State.fastTimerMode;
                _state.Save();
            }

            if (_canvas.Button(808, 38, 150, 28, "UNLOCK ALL (4★)", new Color(0.55f, 0.42f, 0.08f), Color.yellow, 12))
            {
                _state.UnlockAllForTesting();
            }

            if (_canvas.Button(964, 38, 145, 28, "BLUEPRINT EGG", new Color(0.12f, 0.32f, 0.58f), Color.cyan, 12))
            {
                _router.ForceBlueprintEasterEgg();
            }

            if (_canvas.Button(1115, 38, 148, 28, "TITLE SCREEN", new Color(0.25f, 0.25f, 0.32f), Color.white, 12))
            {
                _router.TransitionTo(SceneId.TITLE_MENU);
            }

            // Row 2: Instant Scene / Night Jumpers
            for (int n = 1; n <= 5; n++)
            {
                int targetNight = n;
                if (_canvas.Button(490 + (n - 1) * 66, 72, 60, 28, $"N{n}", new Color(0.18f, 0.22f, 0.35f), Color.white, 12))
                {
                    _state.State.currentNight = targetNight;
                    _state.Save();
                    _router.TransitionTo(SceneId.PRE_NIGHT_INTERLUDE, new Dictionary<string, object> { { "night", targetNight } });
                }
            }

            if (_canvas.Button(822, 72, 105, 28, "8-BIT BABY", new Color(0.55f, 0.16f, 0.16f), Color.white, 12))
            {
                _router.TransitionTo(SceneId.BABY_8BIT_MINIGAME);
            }

            if (_canvas.Button(933, 72, 110, 28, "PVT ROOM", new Color(0.15f, 0.42f, 0.22f), Color.white, 12))
            {
                _router.TransitionTo(SceneId.PRIVATE_ROOM, new Dictionary<string, object> { { "customNight", false } });
            }

            if (_canvas.Button(1049, 72, 110, 28, "CUSTOM N.", new Color(0.45f, 0.32f, 0.08f), Color.yellow, 12))
            {
                _router.TransitionTo(SceneId.CUSTOM_NIGHT_MENU);
            }

            if (_canvas.Button(1165, 72, 98, 28, "FINALE", new Color(0.38f, 0.14f, 0.42f), Color.white, 12))
            {
                _router.TransitionTo(SceneId.SPRINGTRAP_FINALE, new Dictionary<string, object> { { "returnToExtras", false } });
            }
        }
    }
}
