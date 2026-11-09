using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class PreNightInterludeScene : IGameScene
    {
        public SceneId Id => SceneId.PRE_NIGHT_INTERLUDE;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _night = 1;
        private float _timer = 0f;

        private static readonly string[] ElizabethQuotes = new string[]
        {
            "",
            "Daddy, why won't you let me play with her?",
            "Daddy, you let the other children go see her. Why won't you let me go?",
            "Daddy just once let me go play with her. She's so pretty and shiny. Didn't you make her just for me?",
            "Daddy, she can make balloons! Have you seen her make balloons? Oh Daddy, let me go to her!",
            "Daddy isn't watching."
        };

        public PreNightInterludeScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : _state.State.currentNight;
            _state.State.currentNight = _night;
            _state.Save();

            _timer = 0f;
            _audio.StopAmbientLoop();
            _audio.ClearDialogue();

            string quote = _night >= 1 && _night <= 5 ? ElizabethQuotes[_night] : ElizabethQuotes[1];
            _audio.Speak("Elizabeth Afton", quote, VoiceProfileId.ElizabethAfton, 5.2f, true);
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= 6.0f || Input.GetKeyDown(KeyCode.Space))
            {
                if (_night == 4)
                {
                    // Night 4 begins kidnapped inside the Springlock Suit in the Scooping Room
                    _router.TransitionTo(SceneId.SPRINGLOCK_SUIT);
                }
                else
                {
                    _router.TransitionTo(SceneId.ELEVATOR, new Dictionary<string, object> { { "night", _night } });
                }
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, Color.black);
            canvas.DrawText($"NIGHT {_night}", 440, 290, 44, new Color(0.85f, 0.85f, 0.9f), TextAnchor.MiddleCenter, 400, 80);
            canvas.DrawCrtScanlinesAndStatic(0.08f, 0.5f);
        }

        public void Exit()
        {
            _audio.ClearDialogue();
        }
    }

    public sealed class ElevatorScene : IGameScene
    {
        public SceneId Id => SceneId.ELEVATOR;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _night = 1;
        private float _descentTimer = 0f;
        private bool _keypadActive = false;
        private bool _keypadCompleted = false;
        private int _typedCount = 0;
        private string _autocorrectText = "";
        private bool _doorsReady = false;

        // Deliberately missing the letter 'B' per canonical HandUnit keypad
        private static readonly string KeypadLetters = "ACDEFGHIJKLMNOPQRSTUVWXYZ";

        public ElevatorScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : _state.State.currentNight;
            _descentTimer = 0f;
            _keypadActive = false;
            _keypadCompleted = false;
            _typedCount = 0;
            _autocorrectText = "";
            _doorsReady = false;

            _audio.StartAmbientLoop("elevator_hum", 0.35f);
            _audio.ClearDialogue();

            if (_night == 1)
            {
                _audio.Speak("HandUnit", "Welcome to the first day of your exciting new career! Please enter your name as seen above the keypad. This cannot be changed later, so please be careful.", VoiceProfileId.HandUnit, 6.0f, true);
            }
            else if (_night == 2)
            {
                _audio.Speak("HandUnit", "Welcome back for another night of intellectual stimulation! Please make a selection for your virtual companion voice on the keypad.", VoiceProfileId.HandUnit, 5.5f, true);
            }
            else if (_night == 3)
            {
                _audio.Speak("HandUnit", "Welcome back to another pivotal night of your thriving new career! Please select your therapeutic musical accompaniment on the keypad.", VoiceProfileId.HandUnit, 5.5f, true);
            }
            else if (_night == 5)
            {
                _audio.Speak("HandUnit", "Welcome back to your last day on the job! Please enter your desired one-week bonus gift basket on the keypad. Note: two technicians are still on-site tonight.", VoiceProfileId.HandUnit, 6.0f, true);
            }
        }

        public void Update(float deltaTime)
        {
            _descentTimer += deltaTime;
            if (_descentTimer >= 2.2f && !_keypadCompleted)
            {
                _keypadActive = true;
            }
            if (_keypadCompleted && _descentTimer >= 6.5f)
            {
                _doorsReady = true;
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            float flicker = (UnityEngine.Random.value < 0.06f) ? 0.45f : 1.0f;
            canvas.FillRect(0, 0, 1280, 720, new Color(0.06f * flicker, 0.08f * flicker, 0.12f * flicker));

            // Elevator scrolling shaft walls
            float scrollOffset = (_descentTimer * 240f) % 120f;
            for (int y = -120; y < 760; y += 120)
            {
                float ry = y + scrollOffset;
                canvas.FillRect(120, ry, 1040, 10, new Color(0.14f, 0.16f, 0.22f));
            }

            // Back window (with 1 in 1,000 Bidybab eyes Easter egg)
            canvas.FillRect(490, 130, 300, 220, new Color(0.02f, 0.03f, 0.05f));
            canvas.StrokeRect(490, 130, 300, 220, 6f, new Color(0.28f, 0.32f, 0.38f));
            if (_router.RareSpawnActive)
            {
                canvas.FillRect(615, 230, 14, 14, new Color(0.95f, 0.85f, 0.2f));
                canvas.FillRect(650, 230, 14, 14, new Color(0.95f, 0.85f, 0.2f));
                canvas.DrawText("1/1,000 BIDYBAB PEEK", 560, 140, 14, Color.yellow);
            }

            // Clickable nose-honk red button on bottom-right wall
            if (canvas.Button(1060, 480, 56, 56, "●", new Color(0.75f, 0.12f, 0.12f), Color.white, 24))
            {
                _audio.PlaySfx("honk", 0.9f);
            }
            canvas.DrawText("DO NOT TOUCH", 1030, 542, 13, new Color(0.7f, 0.7f, 0.7f));

            // HandUnit Yellow Jittering Keypad (missing 'B')
            if (_keypadActive && !_keypadCompleted)
            {
                RenderHandUnitKeypad(canvas);
            }

            // Red Door Open Button once elevator arrives
            if (_doorsReady)
            {
                if (canvas.Button(510, 400, 260, 64, "OPEN ELEVATOR DOORS\n& ENTER VENT", new Color(0.85f, 0.15f, 0.15f), Color.white, 18))
                {
                    _audio.PlaySfx("door_slam", 0.85f);
                    _router.TransitionTo(SceneId.VENT_CRAWL, new Dictionary<string, object>
                    {
                        { "targetScene", SceneId.PRIMARY_CONTROL },
                        { "night", _night },
                        { "label", "FRONT VENTILATION SHAFT -> PRIMARY CONTROL MODULE" }
                    });
                }
            }

            canvas.DrawCrtScanlinesAndStatic(0.14f, 0.35f);
        }

        private void RenderHandUnitKeypad(ProceduralCanvas canvas)
        {
            canvas.FillRect(380, 220, 520, 360, new Color(0.92f, 0.75f, 0.12f));
            canvas.StrokeRect(380, 220, 520, 360, 5f, new Color(0.45f, 0.32f, 0.04f));
            canvas.FillRect(410, 245, 460, 52, new Color(0.12f, 0.18f, 0.12f));

            string displayPrompt = _typedCount == 0 ? "ENTER SELECTION (NOTE: NO 'B' KEY)" : $"INPUT: {_autocorrectText}";
            canvas.DrawText(displayPrompt, 425, 258, 18, Color.green, TextAnchor.UpperLeft, 430, 36);

            for (int i = 0; i < KeypadLetters.Length; i++)
            {
                int col = i % 7;
                int row = i / 7;
                float jx = UnityEngine.Random.Range(-3.5f, 3.5f);
                float jy = UnityEngine.Random.Range(-3.5f, 3.5f);
                float bx = 415 + col * 62 + jx;
                float by = 320 + row * 56 + jy;

                if (canvas.Button(bx, by, 52, 46, KeypadLetters[i].ToString(), new Color(0.22f, 0.22f, 0.18f), Color.yellow, 18))
                {
                    _audio.PlaySfx("chime", 0.6f);
                    _typedCount++;
                    _autocorrectText += KeypadLetters[(i + 3) % KeypadLetters.Length];
                    if (_typedCount >= 3)
                    {
                        CompleteHandUnitAutocorrect();
                    }
                }
            }
        }

        private void CompleteHandUnitAutocorrect()
        {
            _keypadCompleted = true;
            _keypadActive = false;

            if (_night == 1)
            {
                _audio.Speak("HandUnit", "It seems that you had some trouble with the keypad. I see what you were trying to type, and I will auto-correct it for you. One moment. Welcome: Eggs Benedict.", VoiceProfileId.HandUnit, 6.2f, true);
            }
            else if (_night == 2)
            {
                _audio.Speak("HandUnit", "It seems that you had some trouble with the keypad. I see what you were trying to type, and I will auto-correct it for you. Thank you for choosing: Angsty Teen.", VoiceProfileId.HandUnit, 6.0f, true);
            }
            else if (_night == 3)
            {
                _audio.StartAmbientLoop("casual_bongos", 0.32f);
                _audio.Speak("HandUnit", "Thank you for selecting: Casual Bongos. Also, due to unforeseen malfunctions from today's shows, your nightly pay has been decreased.", VoiceProfileId.HandUnit, 6.2f, true);
            }
            else if (_night == 5)
            {
                _audio.Speak("HandUnit", "Thank you for selecting: Exotic Butters. Please proceed through the front ventilation shaft.", VoiceProfileId.HandUnit, 5.5f, true);
            }
        }

        public void Exit()
        {
        }
    }

    public sealed class VentCrawlScene : IGameScene
    {
        public SceneId Id => SceneId.VENT_CRAWL;

        private readonly SceneGraphRouter _router;
        private readonly ProceduralAudioSynthesizer _audio;

        private SceneId _targetScene = SceneId.PRIMARY_CONTROL;
        private int _night = 1;
        private string _label = "VENTILATION SHAFT";
        private float _progress = 0f;
        private float _clangStepTimer = 0f;
        private bool _scaryClangs = false;

        public VentCrawlScene(SceneGraphRouter router, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _progress = 0f;
            _clangStepTimer = 0f;
            _targetScene = parameters != null && parameters.TryGetValue("targetScene", out object ts) ? (SceneId)ts : SceneId.PRIMARY_CONTROL;
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : 1;
            _label = parameters != null && parameters.TryGetValue("label", out object lb) ? (string)lb : "VENTILATION SHAFT";
            _scaryClangs = parameters != null && parameters.TryGetValue("scaryClangs", out object sc) && (bool)sc;
        }

        public void Update(float deltaTime)
        {
            bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.S);
            bool fast = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (moving)
            {
                float speed = fast ? 42f : 24f;
                _progress += speed * deltaTime;
                _clangStepTimer += deltaTime * (fast ? 2.2f : 1.2f);
                if (_clangStepTimer >= 0.35f)
                {
                    _clangStepTimer = 0f;
                    _audio.PlaySfx("vent_crawl", _scaryClangs ? 1.0f : 0.65f);
                }
            }

            if (_progress >= 100f)
            {
                _router.TransitionTo(_targetScene, new Dictionary<string, object> { { "night", _night } });
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.03f, 0.04f, 0.06f));

            // Perspective rectangular duct frames
            float phase = (_progress * 0.12f) % 1.0f;
            for (int i = 0; i < 6; i++)
            {
                float t = ((i + phase) / 6f);
                float w = Mathf.Lerp(160f, 1240f, t);
                float h = Mathf.Lerp(100f, 680f, t);
                canvas.StrokeRect(640f - w * 0.5f, 360f - h * 0.5f, w, h, 3f, new Color(0.22f, 0.26f, 0.32f, t));
            }

            canvas.DrawText(_label, 240, 45, 22, Color.cyan, TextAnchor.UpperCenter, 800, 40);
            canvas.DrawText($"HOLD [W] OR [W + SHIFT] TO CRAWL — PROGRESS: {Mathf.RoundToInt(_progress)}%", 340, 620, 20, Color.yellow, TextAnchor.UpperCenter, 600, 40);

            if (canvas.Button(520, 550, 240, 48, "CRAWL FORWARD [W]", new Color(0.18f, 0.28f, 0.38f), Color.white, 17))
            {
                _progress += 25f;
                _audio.PlaySfx("vent_crawl", 0.8f);
            }

            canvas.DrawCrtScanlinesAndStatic(0.12f, 0.45f);
        }

        public void Exit()
        {
        }
    }
}
