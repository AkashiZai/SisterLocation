using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class OpeningCutsceneScene : IGameScene
    {
        public SceneId Id => SceneId.OPENING_CUTSCENE;

        private readonly SceneGraphRouter _router;
        private readonly ProceduralAudioSynthesizer _audio;
        private float _timer;
        private int _dialogueIndex;

        public OpeningCutsceneScene(SceneGraphRouter router, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _timer = 0f;
            _dialogueIndex = 0;
            _audio.ClearDialogue();
            _audio.Speak("Board Member", "There's no doubting what you've achieved on a technical level. These are clearly state-of-the-art. There are just certain design choices that were made for these robots that we don't fully understand. We were hoping that you could shed some light on those.", VoiceProfileId.BoardMember, 8.5f, true);
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
            if (_dialogueIndex == 0 && _timer >= 8.6f)
            {
                _dialogueIndex = 1;
                _audio.Speak("William Afton", "She can dance, she can sing, she's equipped with a built-in helium tank for inflating balloons right at her fingertips. She can take song requests. She can even dispense ice cream.", VoiceProfileId.WilliamAfton, 7.5f, true);
            }
            else if (_dialogueIndex == 1 && _timer >= 16.2f)
            {
                _dialogueIndex = 2;
                _audio.Speak("Board Member", "With all due respect, those aren't the design choices we were curious about, Mr. Afton.", VoiceProfileId.BoardMember, 5.5f, true);
            }
            else if (_timer >= 22.5f || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                _router.TransitionTo(SceneId.TITLE_MENU);
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.02f, 0.02f, 0.04f));

            // Slow panning silhouette of Circus Baby endoskeleton & blueprint lines
            float panY = Mathf.Lerp(520f, 260f, Mathf.Clamp01(_timer / 22f));
            canvas.StrokeRect(340, 60, 600, 600, 1f, new Color(0.1f, 0.35f, 0.55f, 0.25f));
            canvas.DrawAnimatronicHead("circus_baby", 640f, panY, 2.2f, 0.35f, 0.1f, false);

            // Dark shadow overlay
            canvas.FillRect(0, 0, 1280, 720, new Color(0f, 0f, 0f, 0.52f));
            canvas.DrawText("AFTON ROBOTICS, LLC — BOARD INQUIRY ARCHIVE", 60, 40, 18, new Color(0.45f, 0.7f, 0.9f, 0.7f));

            if (canvas.Button(1080, 36, 150, 38, "SKIP >>", new Color(0.15f, 0.15f, 0.22f), Color.white, 15))
            {
                _router.TransitionTo(SceneId.TITLE_MENU);
            }

            canvas.DrawCrtScanlinesAndStatic(0.12f, 0.45f);
        }

        public void Exit()
        {
            _audio.ClearDialogue();
        }
    }

    public sealed class TitleMenuScene : IGameScene
    {
        public SceneId Id => SceneId.TITLE_MENU;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private float _twitchTimer;
        private int _activeCharIndex;
        private float _faceplateOpen;
        private float _whisperTimer;

        private static readonly string[] ShowcaseIds = { "circus_baby", "funtime_freddy", "ballora", "funtime_foxy" };

        public TitleMenuScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _twitchTimer = 0f;
            _activeCharIndex = 0;
            _faceplateOpen = 0f;
            _whisperTimer = 6f;
            _audio.StartAmbientLoop("elevator_hum", 0.18f);
        }

        public void Update(float deltaTime)
        {
            _twitchTimer += deltaTime;
            if (_twitchTimer > 2.8f)
            {
                _twitchTimer = 0f;
                _activeCharIndex = (_activeCharIndex + 1) % ShowcaseIds.Length;
            }

            _faceplateOpen = (UnityEngine.Random.value < 0.12f) ? UnityEngine.Random.Range(0.25f, 0.95f) : Mathf.Lerp(_faceplateOpen, 0f, deltaTime * 8f);

            _whisperTimer -= deltaTime;
            if (_whisperTimer <= 0f)
            {
                _whisperTimer = UnityEngine.Random.Range(9f, 16f);
                _audio.Speak("Whisper", "Help me...", VoiceProfileId.ElizabethAfton, 1.8f, false);
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.03f, 0.03f, 0.05f));

            // Right-side twitching animatronic showcase
            float jx = _faceplateOpen > 0.2f ? UnityEngine.Random.Range(-8f, 8f) : 0f;
            float jy = _faceplateOpen > 0.2f ? UnityEngine.Random.Range(-6f, 6f) : 0f;
            canvas.DrawAnimatronicHead(ShowcaseIds[_activeCharIndex], 880f + jx, 360f + jy, 2.15f, _faceplateOpen, _faceplateOpen * 0.5f);

            // Title Logo
            canvas.DrawText("FIVE NIGHTS AT FREDDY'S", 80, 60, 42, new Color(0.92f, 0.18f, 0.18f), TextAnchor.UpperLeft, 650, 60);
            canvas.DrawText("S I S T E R   L O C A T I O N", 82, 112, 30, new Color(0.85f, 0.88f, 0.98f), TextAnchor.UpperLeft, 650, 50);

            // 4 Large Canonical Stars on Left Side
            var stars = _state.State.starsUnlocked;
            DrawStarBadge(canvas, 80, 175, stars.star1_realEnding, "★ STAR 1: REAL ENDING (SCOOPED)");
            DrawStarBadge(canvas, 80, 212, stars.star2_minigame, "★ STAR 2: 8-BIT BABY MINIGAME");
            DrawStarBadge(canvas, 80, 249, stars.star3_fakeEnding, "★ STAR 3: FAKE ENDING (PRIVATE ROOM)");
            DrawStarBadge(canvas, 80, 286, stars.star4_goldenFreddyVHard, "★ STAR 4: GOLDEN FREDDY 10/20 V.HARD");

            // Main Menu Buttons
            if (canvas.Button(80, 350, 320, 48, "NEW GAME", new Color(0.18f, 0.06f, 0.06f), Color.white, 22))
            {
                _state.ResetNewGame();
                _router.TransitionTo(SceneId.PRE_NIGHT_INTERLUDE, new Dictionary<string, object> { { "night", 1 } });
            }

            string contLabel = $"CONTINUE (NIGHT {_state.State.currentNight})";
            if (canvas.Button(80, 412, 320, 48, contLabel, new Color(0.12f, 0.15f, 0.24f), Color.white, 22))
            {
                _router.TransitionTo(SceneId.PRE_NIGHT_INTERLUDE, new Dictionary<string, object> { { "night", _state.State.currentNight } });
            }

            bool extrasUnlocked = stars.star1_realEnding || stars.star3_fakeEnding || _state.State.currentNight >= 5 || _state.State.debugForceEasterEggs;
            Color extrasCol = extrasUnlocked ? new Color(0.12f, 0.22f, 0.16f) : new Color(0.1f, 0.1f, 0.1f);
            if (canvas.Button(80, 474, 320, 48, extrasUnlocked ? "EXTRAS MENU" : "EXTRAS (LOCKED)", extrasCol, extrasUnlocked ? Color.white : Color.gray, 20) && extrasUnlocked)
            {
                _router.TransitionTo(SceneId.EXTRAS_MENU);
            }

            bool customUnlocked = _state.GetEarnedStarCount() >= 3 || _state.State.debugForceEasterEggs;
            Color customCol = customUnlocked ? new Color(0.28f, 0.18f, 0.05f) : new Color(0.1f, 0.1f, 0.1f);
            if (canvas.Button(80, 536, 320, 48, customUnlocked ? "CUSTOM NIGHT" : "CUSTOM NIGHT (NEED 3 STARS)", customCol, customUnlocked ? Color.yellow : Color.gray, 20) && customUnlocked)
            {
                _router.TransitionTo(SceneId.CUSTOM_NIGHT_MENU);
            }

            if (canvas.Button(80, 598, 320, 40, "REPLAY OPENING CUTSCENE", new Color(0.12f, 0.12f, 0.18f), new Color(0.8f, 0.85f, 1f), 16))
            {
                _router.TransitionTo(SceneId.OPENING_CUTSCENE);
            }

            canvas.DrawCrtScanlinesAndStatic(0.18f, 0.35f);
        }

        private void DrawStarBadge(ProceduralCanvas canvas, float x, float y, bool unlocked, string label)
        {
            Color c = unlocked ? new Color(1f, 0.85f, 0.15f) : new Color(0.32f, 0.32f, 0.36f);
            canvas.DrawText(label, x, y, 16, c, TextAnchor.UpperLeft, 500, 32);
        }

        public void Exit()
        {
        }
    }

    public sealed class ExtrasMenuScene : IGameScene
    {
        public SceneId Id => SceneId.EXTRAS_MENU;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _tabIndex; // 0=Animatronics, 1=Blueprints, 2=Making Of, 3=Jumpscares, 4=Cutscene Theater
        private int _subIndex;
        private float _makingOfFaceplate;

        private static readonly string[] AnimList = { "circus_baby", "ballora", "funtime_freddy", "funtime_foxy", "ennard", "yenndo", "lolbit" };

        public ExtrasMenuScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _tabIndex = 0;
            _subIndex = 0;
            _makingOfFaceplate = 0.2f;
        }

        public void Update(float deltaTime)
        {
            _makingOfFaceplate = Mathf.PingPong(Time.time * 0.45f, 1.0f);
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.04f, 0.05f, 0.08f));
            canvas.DrawText("EXTRAS & ARCHIVES", 60, 35, 30, Color.white);

            // Top category tabs
            string[] tabs = { "ANIMATRONICS", "BLUEPRINTS", "MAKING OF", "JUMPSCARES", "CUTSCENE THEATER" };
            for (int i = 0; i < tabs.Length; i++)
            {
                Color bg = (_tabIndex == i) ? new Color(0.25f, 0.42f, 0.68f) : new Color(0.12f, 0.15f, 0.22f);
                if (canvas.Button(60 + i * 215, 85, 205, 40, tabs[i], bg, Color.white, 15))
                {
                    _tabIndex = i;
                    _subIndex = 0;
                }
            }

            if (_tabIndex == 0)
            {
                string id = AnimList[_subIndex % AnimList.Length];
                canvas.DrawAnimatronicHead(id, 640, 380, 2.0f, 0.25f, 0.1f);
                canvas.DrawText($"MODEL VIEWER: {id.ToUpperInvariant()}", 440, 150, 22, Color.cyan, TextAnchor.UpperCenter, 400, 40);
                if (canvas.Button(340, 580, 160, 40, "< PREV", new Color(0.15f, 0.2f, 0.3f), Color.white))
                    _subIndex = (_subIndex + AnimList.Length - 1) % AnimList.Length;
                if (canvas.Button(780, 580, 160, 40, "NEXT >", new Color(0.15f, 0.2f, 0.3f), Color.white))
                    _subIndex = (_subIndex + 1) % AnimList.Length;
            }
            else if (_tabIndex == 1)
            {
                canvas.DrawText("TECHNICAL BLUEPRINTS (AFTON ROBOTICS)", 440, 160, 22, Color.cyan, TextAnchor.UpperCenter, 400, 40);
                for (int b = 0; b < 4; b++)
                {
                    if (canvas.Button(460, 230 + b * 60, 360, 46, $"VIEW BLUEPRINT #{b + 1}: {AnimList[b].ToUpperInvariant()}", new Color(0.1f, 0.28f, 0.5f), Color.white, 16))
                    {
                        _router.ForceBlueprintEasterEgg(b);
                    }
                }
            }
            else if (_tabIndex == 2)
            {
                string[] makingTargets = { "circus_baby", "funtime_freddy", "funtime_foxy" };
                string id = makingTargets[_subIndex % makingTargets.Length];
                canvas.DrawText($"MAKING OF: {id.ToUpperInvariant()} (WIREFRAME -> ENDO -> SHELL)", 340, 150, 20, Color.yellow, TextAnchor.UpperCenter, 600, 40);
                canvas.DrawAnimatronicHead(id, 640, 380, 2.0f, _makingOfFaceplate, _makingOfFaceplate * 0.5f, _makingOfFaceplate > 0.7f);
                if (canvas.Button(540, 580, 200, 40, "CYCLE MODEL", new Color(0.2f, 0.2f, 0.3f), Color.white))
                    _subIndex = (_subIndex + 1) % makingTargets.Length;
            }
            else if (_tabIndex == 3)
            {
                canvas.DrawText("JUMPSCARE VIEWER", 440, 150, 22, Color.red, TextAnchor.UpperCenter, 400, 40);
                for (int j = 0; j < AnimList.Length; j++)
                {
                    string anim = AnimList[j];
                    if (canvas.Button(460, 200 + j * 52, 360, 42, $"TRIGGER {anim.ToUpperInvariant()} JUMPSCARE", new Color(0.35f, 0.08f, 0.08f), Color.white, 15))
                    {
                        _router.TriggerJumpscare(anim, SceneId.EXTRAS_MENU);
                    }
                }
            }
            else if (_tabIndex == 4)
            {
                canvas.DrawText("CUTSCENE THEATER (TV EPISODES, 8-BIT DECAY & FINALE)", 340, 150, 20, Color.cyan, TextAnchor.UpperCenter, 600, 40);
                for (int ep = 1; ep <= 4; ep++)
                {
                    int targetEp = ep;
                    if (canvas.Button(240, 205 + (ep - 1) * 52, 360, 42, $"TV EPISODE {ep}: IMMORTAL & RESTLESS", new Color(0.16f, 0.14f, 0.28f), Color.white, 15))
                    {
                        _router.TransitionTo(SceneId.TV_LIVING_ROOM, new Dictionary<string, object> { { "episode", targetEp }, { "returnToExtras", true } });
                    }
                }
                for (int st = 1; st <= 7; st++)
                {
                    int targetStage = st;
                    if (canvas.Button(660, 195 + (st - 1) * 44, 380, 36, $"8-BIT MICHAEL DECAY CUTSCENE #{st}", new Color(0.2f, 0.12f, 0.24f), Color.white, 14))
                    {
                        _router.TransitionTo(SceneId.DECAY_8BIT_CUTSCENE, new Dictionary<string, object> { { "stage", targetStage }, { "returnToExtras", true } });
                    }
                }
                if (canvas.Button(660, 515, 380, 44, "SPRINGTRAP FINALE (FAZBEAR'S FRIGHT)", new Color(0.38f, 0.22f, 0.06f), Color.yellow, 15))
                {
                    _router.TransitionTo(SceneId.SPRINGTRAP_FINALE, new Dictionary<string, object> { { "returnToExtras", true } });
                }
            }

            // Hidden clickable 8-Bit Baby icon in bottom-left corner
            canvas.FillRect(24, 650, 44, 48, new Color(0.88f, 0.2f, 0.15f));
            canvas.FillRect(32, 660, 28, 24, Color.white);
            canvas.FillRect(36, 666, 6, 6, Color.green);
            canvas.FillRect(48, 666, 6, 6, Color.green);
            if (canvas.Button(20, 646, 155, 54, "8-BIT BABY", new Color(0.85f, 0.18f, 0.15f, 0.35f), Color.yellow, 13))
            {
                _router.TransitionTo(SceneId.BABY_8BIT_MINIGAME);
            }

            if (canvas.Button(1060, 645, 170, 44, "BACK TO TITLE", new Color(0.2f, 0.2f, 0.25f), Color.white, 16))
            {
                _router.TransitionTo(SceneId.TITLE_MENU);
            }

            canvas.DrawCrtScanlinesAndStatic(0.1f, 0.3f);
        }

        public void Exit()
        {
        }
    }
}
