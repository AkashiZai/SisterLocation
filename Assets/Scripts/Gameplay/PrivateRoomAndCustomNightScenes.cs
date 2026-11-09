using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class CustomNightMenuScene : IGameScene
    {
        public SceneId Id => SceneId.CUSTOM_NIGHT_MENU;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        public static readonly string[] AnimIds = new string[]
        {
            "ballora", "funtime_freddy", "yenndo", "funtime_foxy", "bonnet",
            "bidybab", "electrobab", "lolbit", "minireena_1", "minireena_2"
        };

        public static readonly string[] AnimNames = new string[]
        {
            "Ballora", "F. Freddy", "Yenndo", "F. Foxy", "Bonnet",
            "Bidybab", "Electrobab", "Lolbit", "Minireena", "Minireena 2"
        };

        public struct PresetDef
        {
            public string Id;
            public string Title;
            public int[] ActiveIndices;
            public int CutsceneStage;
        }

        public static readonly PresetDef[] Presets = new PresetDef[]
        {
            new PresetDef { Id = "angry_ballet", Title = "Angry Ballet", ActiveIndices = new[]{ 0, 8, 9 }, CutsceneStage = 1 },
            new PresetDef { Id = "freddy_and_co", Title = "Freddy & Co.", ActiveIndices = new[]{ 1, 2, 3, 4 }, CutsceneStage = 2 },
            new PresetDef { Id = "funtime_frenzy", Title = "Funtime Frenzy", ActiveIndices = new[]{ 1, 2, 3, 7 }, CutsceneStage = 3 },
            new PresetDef { Id = "dolls_attack", Title = "Dolls, Attack!", ActiveIndices = new[]{ 4, 5, 6, 8, 9 }, CutsceneStage = 4 },
            new PresetDef { Id = "girls_night", Title = "Girls' Night", ActiveIndices = new[]{ 0, 3, 4, 5, 8, 9 }, CutsceneStage = 5 },
            new PresetDef { Id = "weirdos", Title = "Weirdos", ActiveIndices = new[]{ 2, 4, 6, 7, 8 }, CutsceneStage = 6 },
            new PresetDef { Id = "top_shelf", Title = "Top Shelf", ActiveIndices = new[]{ 0, 1, 2, 3, 4 }, CutsceneStage = 6 },
            new PresetDef { Id = "bottom_shelf", Title = "Bottom Shelf", ActiveIndices = new[]{ 5, 6, 7, 8, 9 }, CutsceneStage = 7 },
            new PresetDef { Id = "cupcake_challenge", Title = "Cupcake Challenge", ActiveIndices = new[]{ 0, 1, 3, 4, 6, 7, 8 }, CutsceneStage = 7 },
            new PresetDef { Id = "golden_freddy", Title = "Golden Freddy", ActiveIndices = new[]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, CutsceneStage = 8 }
        };

        private readonly int[] _aiLevels = new int[10];
        private int _selectedPresetIndex = 0;
        private int _difficultyIndex = 3; // 0=Easy(5), 1=Normal(10), 2=Hard(15), 3=V.Hard(20)

        public CustomNightMenuScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            ApplyPreset(0, 3);
        }

        private void ApplyPreset(int presetIdx, int diffIdx)
        {
            _selectedPresetIndex = presetIdx;
            _difficultyIndex = diffIdx;
            int levelVal = (diffIdx + 1) * 5;
            for (int i = 0; i < 10; i++) _aiLevels[i] = 0;

            var preset = Presets[presetIdx];
            for (int i = 0; i < preset.ActiveIndices.Length; i++)
            {
                _aiLevels[preset.ActiveIndices[i]] = levelVal;
            }
        }

        public void Update(float deltaTime)
        {
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.05f, 0.04f, 0.08f));
            canvas.DrawText("CUSTOM NIGHT — 10 ANIMATRONICS & 10 CHALLENGE PRESETS", 60, 28, 28, Color.yellow, TextAnchor.UpperLeft, 950, 45);

            // Top 2 rows of 5 Animatronics (AI 0..20)
            for (int i = 0; i < 10; i++)
            {
                int col = i % 5;
                int row = i / 5;
                float x = 60 + col * 165;
                float y = 85 + row * 175;

                canvas.FillRect(x, y, 150, 160, new Color(0.12f, 0.14f, 0.2f));
                canvas.StrokeRect(x, y, 150, 160, 2f, _aiLevels[i] > 0 ? Color.cyan : Color.gray);
                canvas.DrawText(AnimNames[i], x + 5, y + 8, 15, Color.white, TextAnchor.UpperCenter, 140, 26);

                string headId = AnimIds[i] == "minireena_1" || AnimIds[i] == "minireena_2" ? "ballora" :
                                (AnimIds[i] == "bonnet" ? "funtime_freddy" :
                                (AnimIds[i] == "bidybab" || AnimIds[i] == "electrobab" ? "circus_baby" : AnimIds[i]));
                canvas.DrawAnimatronicHead(headId, x + 75, y + 72, 0.35f, 0f);

                if (canvas.Button(x + 12, y + 115, 36, 34, "-", new Color(0.3f, 0.15f, 0.15f), Color.white, 18))
                    _aiLevels[i] = Mathf.Max(0, _aiLevels[i] - 1);
                canvas.DrawText(_aiLevels[i].ToString(), x + 50, y + 118, 20, Color.yellow, TextAnchor.UpperCenter, 50, 34);
                if (canvas.Button(x + 102, y + 115, 36, 34, "+", new Color(0.15f, 0.35f, 0.18f), Color.white, 18))
                    _aiLevels[i] = Mathf.Min(20, _aiLevels[i] + 1);
            }

            // Right Side: 10 Challenge Presets & Difficulty Stars
            canvas.DrawText("CHALLENGE PRESETS:", 910, 82, 18, Color.cyan);
            string[] diffNames = { "EASY (5)", "NORMAL (10)", "HARD (15)", "V. HARD (20)" };
            for (int d = 0; d < 4; d++)
            {
                Color dCol = (_difficultyIndex == d) ? new Color(0.65f, 0.45f, 0.1f) : new Color(0.18f, 0.18f, 0.24f);
                if (canvas.Button(910 + d * 82, 112, 78, 32, diffNames[d], dCol, Color.white, 11))
                {
                    ApplyPreset(_selectedPresetIndex, d);
                }
            }

            for (int p = 0; p < Presets.Length; p++)
            {
                var pr = Presets[p];
                var stars = _state.GetPresetStars(pr.Id);
                string starStr = $"{(stars.easy ? "★" : "☆")}{(stars.normal ? "★" : "☆")}{(stars.hard ? "★" : "☆")}{(stars.vhard ? "★" : "☆")}";
                Color pBg = (_selectedPresetIndex == p) ? new Color(0.25f, 0.38f, 0.62f) : new Color(0.14f, 0.15f, 0.22f);
                if (canvas.Button(910, 154 + p * 44, 325, 38, $"{pr.Title}   {starStr}", pBg, Color.white, 14))
                {
                    ApplyPreset(p, _difficultyIndex);
                }
            }

            // Bottom Controls
            if (canvas.Button(60, 460, 240, 48, "ALL TO 20 (10/20 MODE)", new Color(0.55f, 0.35f, 0.05f), Color.yellow, 15))
            {
                _selectedPresetIndex = 9;
                _difficultyIndex = 3;
                for (int i = 0; i < 10; i++) _aiLevels[i] = 20;
            }

            if (canvas.Button(320, 460, 200, 48, "ALL TO 0", new Color(0.22f, 0.22f, 0.28f), Color.white, 15))
            {
                for (int i = 0; i < 10; i++) _aiLevels[i] = 0;
            }

            if (canvas.Button(60, 540, 460, 64, "READY! START CUSTOM NIGHT SHIFT >>", new Color(0.15f, 0.55f, 0.22f), Color.white, 20))
            {
                int[] levelsCopy = (int[])_aiLevels.Clone();
                _router.TransitionTo(SceneId.PRIVATE_ROOM, new Dictionary<string, object>
                {
                    { "customNight", true },
                    { "aiLevels", levelsCopy },
                    { "presetId", Presets[_selectedPresetIndex].Id },
                    { "difficultyIndex", _difficultyIndex },
                    { "cutsceneStage", Presets[_selectedPresetIndex].CutsceneStage }
                });
            }

            if (canvas.Button(60, 630, 220, 44, "BACK TO TITLE", new Color(0.22f, 0.22f, 0.28f), Color.white, 16))
            {
                _router.TransitionTo(SceneId.TITLE_MENU);
            }

            canvas.DrawCrtScanlinesAndStatic(0.12f, 0.35f);
        }

        public void Exit()
        {
        }
    }

    public sealed class PrivateRoomScene : IGameScene
    {
        public SceneId Id => SceneId.PRIVATE_ROOM;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private bool _isCustomNight = false;
        private int[] _ai = new int[10];
        private string _presetId = "angry_ballet";
        private int _difficultyIndex = 3;
        private int _cutsceneStage = 1;

        // Core Office State
        private float _nightElapsed = 0f;
        private float _nightDuration = 180f;
        private float _power = 100f;
        private float _oxygen = 100f;

        private bool _leftDoorClosed = false;
        private bool _rightDoorClosed = false;
        private bool _ventDoorClosed = false;
        private bool _cameraUp = false;
        private int _currentCam = 1; // CAM 01..07

        // 1-9-8-3 Wall Keypad & Lolbit L-O-L input
        private string _keypadBuffer = "";
        private bool _fnaf4MonitorsUnlocked = false;
        private string _lolKeyboardBuffer = "";

        // Mode 1: Ennard Boss AI
        private int _ennardCam = 6;
        private int _ennardStage = 0; // 0=far, 1=mid, 2=at door/vent ready to attack
        private int _ennardAttackDir = 0; // 0=Left(A), 1=Vent(W), 2=Right(D)
        private float _ennardMoveTimer = 0f;

        // Mode 2: 10 Custom Night Animatronic states
        private int _balloraSide = 0; // -1=Left(CAM01), 1=Right(CAM02)
        private int _balloraStage = 0; // 0..3
        private float _balloraTimer = 0f;

        private int _freddyCam = 3; // 3=Left Closet(CAM03), 4=Right Closet(CAM04)
        private int _freddyPendingAttackSide = 0; // 0=none, -1=LeftDoor, 1=RightDoor
        private float _freddyTimer = 0f;

        private bool _yenndoActive = false;
        private float _yenndoKillTimer = 0f;

        private int _foxyCurtainStage = 0; // 0..4 (4 = Gone, sprinting to Right Door)
        private float _foxyTimer = 0f;

        private bool _bonnetActive = false;
        private float _bonnetX = 1100f;

        private int _bidybabVentStage = 0; // 0..3 in CAM 05
        private float _bidybabTimer = 0f;

        private int _electrobabCam = 0; // 0=none, 3=CAM03, 4=CAM04
        private float _electrobabTimer = 0f;

        private int _lolbitMonitors = 0; // 0..3 (3 = full distraction lock)
        private float _lolbitTimer = 0f;

        private bool _minireena1Active = false; // Draining Oxygen in CAM 06
        private float _minireena1Timer = 0f;

        private int _minireena2Count = 0; // 0..3 climbing office view
        private float _minireena2Timer = 0f;

        public PrivateRoomScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _isCustomNight = parameters != null && parameters.TryGetValue("customNight", out object cn) && (bool)cn;
            if (_isCustomNight && parameters.TryGetValue("aiLevels", out object lv))
                _ai = (int[])lv;
            else
                _ai = new int[10];

            _presetId = parameters != null && parameters.TryGetValue("presetId", out object pid) ? (string)pid : "golden_freddy";
            _difficultyIndex = parameters != null && parameters.TryGetValue("difficultyIndex", out object di) ? (int)di : 3;
            _cutsceneStage = parameters != null && parameters.TryGetValue("cutsceneStage", out object cs) ? (int)cs : 1;

            _nightElapsed = 0f;
            _nightDuration = _state.State.fastTimerMode ? 120f : (_isCustomNight ? 360f : 540f);
            _power = 100f;
            _oxygen = 100f;

            _leftDoorClosed = false;
            _rightDoorClosed = false;
            _ventDoorClosed = false;
            _cameraUp = false;
            _currentCam = 1;

            _keypadBuffer = "";
            _fnaf4MonitorsUnlocked = false;
            _lolKeyboardBuffer = "";

            _ennardCam = 6;
            _ennardStage = 0;
            _ennardAttackDir = 0;
            _ennardMoveTimer = 0f;

            _balloraSide = -1;
            _balloraStage = 0;
            _balloraTimer = 0f;
            _freddyCam = 3;
            _freddyPendingAttackSide = 0;
            _freddyTimer = 0f;
            _yenndoActive = false;
            _yenndoKillTimer = 0f;
            _foxyCurtainStage = 0;
            _foxyTimer = 0f;
            _bonnetActive = false;
            _bonnetX = 1100f;
            _bidybabVentStage = 0;
            _bidybabTimer = 0f;
            _electrobabCam = 0;
            _electrobabTimer = 0f;
            _lolbitMonitors = 0;
            _lolbitTimer = 0f;
            _minireena1Active = false;
            _minireena1Timer = 0f;
            _minireena2Count = 0;
            _minireena2Timer = 0f;

            _audio.ClearDialogue();
            if (!_isCustomNight)
            {
                _audio.Speak("HandUnit", "Access Granted. Warning: You have entered a highly restricted area. Power reserves are limited.", VoiceProfileId.HandUnit, 5.5f, true);
                _audio.Speak("Circus Baby (Ennard)", "Why didn't you trust me? Why didn't you believe me? We need you so that we can leave.", VoiceProfileId.CircusBaby, 6.5f, false);
            }
        }

        public void Update(float deltaTime)
        {
            _nightElapsed += deltaTime;
            if (_nightElapsed >= _nightDuration)
            {
                CompleteShiftVictory();
                return;
            }

            HandleHotkeys();

            // Power & Oxygen usage
            int usageBars = 1 + (_leftDoorClosed ? 1 : 0) + (_rightDoorClosed ? 1 : 0) + (_ventDoorClosed ? 1 : 0) + (_cameraUp ? 1 : 0);
            float drainRate = usageBars * 0.16f + (_electrobabCam != 0 ? 0.55f : 0f);
            _power = Mathf.Max(0f, _power - drainRate * deltaTime);

            if (_power <= 0f)
            {
                _leftDoorClosed = false;
                _rightDoorClosed = false;
                _ventDoorClosed = false;
                _cameraUp = false;
                _router.TriggerJumpscare(_isCustomNight ? "funtime_freddy" : "ennard", _isCustomNight ? SceneId.CUSTOM_NIGHT_MENU : SceneId.PRIVATE_ROOM);
                return;
            }

            if (_isCustomNight)
            {
                float oxyDrain = (_minireena1Active ? 3.2f : 0f) + (_yenndoActive ? 14.0f : 0f);
                if (oxyDrain > 0f)
                    _oxygen = Mathf.Max(0f, _oxygen - oxyDrain * deltaTime);
                else
                    _oxygen = Mathf.Min(100f, _oxygen + 4.5f * deltaTime);

                UpdateCustomNightAll10(deltaTime);
            }
            else
            {
                UpdateEnnardBossFight(deltaTime);
            }
        }

        private void HandleHotkeys()
        {
            bool controlsLockedByLolbit = (_lolbitMonitors >= 3);
            if (!controlsLockedByLolbit)
            {
                if (Input.GetKeyDown(KeyCode.A)) ToggleDoor(ref _leftDoorClosed);
                if (Input.GetKeyDown(KeyCode.W)) ToggleDoor(ref _ventDoorClosed);
                if (Input.GetKeyDown(KeyCode.D)) ToggleDoor(ref _rightDoorClosed);
                if (Input.GetKeyDown(KeyCode.S)) ToggleCameraMonitor();
                if (Input.GetKeyDown(KeyCode.E)) TriggerControlledShock();
            }

            // Type L-O-L on keyboard to clear Lolbit
            if (Input.GetKeyDown(KeyCode.L)) _lolKeyboardBuffer += "L";
            if (Input.GetKeyDown(KeyCode.O)) _lolKeyboardBuffer += "O";
            if (_lolKeyboardBuffer.Length > 6) _lolKeyboardBuffer = _lolKeyboardBuffer.Substring(_lolKeyboardBuffer.Length - 3);
            if (_lolKeyboardBuffer.Contains("LOL"))
            {
                _lolKeyboardBuffer = "";
                if (_lolbitMonitors > 0)
                {
                    _lolbitMonitors = 0;
                    _audio.PlaySfx("chime", 0.85f);
                }
            }
        }

        private void ToggleDoor(ref bool doorState)
        {
            doorState = !doorState;
            _audio.PlaySfx("door_slam", 0.85f);
        }

        private void ToggleCameraMonitor()
        {
            _cameraUp = !_cameraUp;
            _audio.PlaySfx("springlock_wind", 0.7f);

            if (_isCustomNight)
            {
                if (_cameraUp)
                {
                    // Flipping monitor up despawns Yenndo
                    _yenndoActive = false;
                    _yenndoKillTimer = 0f;
                }
                else
                {
                    // Lowering monitor can spawn Yenndo or Bonnet
                    if (_ai[2] > 0 && UnityEngine.Random.Range(0, 45) < _ai[2])
                    {
                        _yenndoActive = true;
                        _yenndoKillTimer = 0f;
                    }
                    if (_ai[4] > 0 && !_bonnetActive && UnityEngine.Random.Range(0, 40) < _ai[4])
                    {
                        _bonnetActive = true;
                        _bonnetX = 1060f;
                        _audio.Speak("Bonnet", "Hee-hee-hee!", VoiceProfileId.BonBon, 1.8f, false);
                    }
                }
            }
        }

        private void TriggerControlledShock()
        {
            if (_power < 2f) return;
            _power = Mathf.Max(0f, _power - 1.5f);
            _audio.PlaySfx("shock", 0.95f);
            _electrobabCam = 0;
            _minireena1Active = false;
        }

        private void UpdateEnnardBossFight(float deltaTime)
        {
            int hour = Mathf.FloorToInt((_nightElapsed / _nightDuration) * 6f);
            float interval = (hour >= 3) ? 3.2f : 5.5f;
            _ennardMoveTimer += deltaTime;

            if (_ennardMoveTimer >= interval)
            {
                _ennardMoveTimer = 0f;
                _audio.PlaySfx("vent_crawl", 0.75f);

                if (_ennardStage < 2)
                {
                    _ennardStage++;
                    if (_ennardStage == 1)
                    {
                        _ennardAttackDir = UnityEngine.Random.Range(0, 3); // 0=Left(CAM01), 1=Vent(CAM05), 2=Right(CAM02)
                        _ennardCam = _ennardAttackDir == 0 ? 3 : (_ennardAttackDir == 1 ? 5 : 4);
                    }
                    else if (_ennardStage == 2)
                    {
                        _ennardCam = _ennardAttackDir == 0 ? 1 : (_ennardAttackDir == 1 ? 5 : 2);
                        if (hour < 3)
                            _audio.Speak("Circus Baby (Ennard)", "Hello? Isn't there anyone in there? We need you so that we can hide.", VoiceProfileId.CircusBaby, 4.0f, false);
                        else
                            _audio.Speak("Elizabeth Afton (Ennard)", "I know it was an accident! Isn't this why you came here? To be with me again?", VoiceProfileId.ElizabethAfton, 4.5f, false);
                    }
                }
                else
                {
                    // Attack resolved against matching door
                    bool blocked = (_ennardAttackDir == 0 && _leftDoorClosed) ||
                                   (_ennardAttackDir == 1 && _ventDoorClosed) ||
                                   (_ennardAttackDir == 2 && _rightDoorClosed);
                    if (blocked)
                    {
                        _audio.PlaySfx("door_slam", 0.9f);
                        _ennardStage = 0;
                        _ennardCam = 6;
                    }
                    else
                    {
                        _router.TriggerJumpscare("ennard", SceneId.PRIVATE_ROOM, new Dictionary<string, object> { { "customNight", false } });
                    }
                }
            }
        }

        private void UpdateCustomNightAll10(float deltaTime)
        {
            // 0. Ballora (CAM 01/02 static + 4 music stages)
            if (_ai[0] > 0)
            {
                _balloraTimer += deltaTime;
                if (_balloraTimer >= Mathf.Lerp(7.5f, 3.2f, _ai[0] / 20f))
                {
                    _balloraTimer = 0f;
                    if (_balloraStage == 0)
                    {
                        _balloraSide = UnityEngine.Random.value < 0.5f ? -1 : 1;
                        _balloraStage = 1;
                    }
                    else if (_balloraStage < 3)
                    {
                        _balloraStage++;
                    }
                    else
                    {
                        bool blocked = (_balloraSide == -1 && _leftDoorClosed) || (_balloraSide == 1 && _rightDoorClosed);
                        if (blocked)
                        {
                            _audio.PlaySfx("door_slam", 0.8f);
                            _balloraStage = 0;
                        }
                        else
                        {
                            _router.TriggerJumpscare("ballora", SceneId.CUSTOM_NIGHT_MENU);
                            return;
                        }
                    }
                    _audio.SetMusicBoxStage(_balloraStage, _balloraSide * 0.8f);
                }
            }

            // 1. Funtime Freddy & Bon-Bon (CAM 03 Left Closet / CAM 04 Right Closet)
            if (_ai[1] > 0)
            {
                _freddyTimer += deltaTime;
                if (_freddyTimer >= Mathf.Lerp(9.0f, 4.2f, _ai[1] / 20f))
                {
                    _freddyTimer = 0f;
                    if (_freddyPendingAttackSide == 0)
                    {
                        if (UnityEngine.Random.value < 0.45f)
                        {
                            _freddyCam = (_freddyCam == 3) ? 4 : 3;
                        }
                        else
                        {
                            bool sameDoor = UnityEngine.Random.value < 0.5f;
                            int currentSide = (_freddyCam == 3) ? -1 : 1;
                            if (sameDoor)
                            {
                                _freddyPendingAttackSide = currentSide;
                                _audio.Speak("Funtime Freddy", "Bon-Bon, go get him!", VoiceProfileId.FuntimeFreddy, 2.8f, false);
                            }
                            else
                            {
                                _freddyPendingAttackSide = -currentSide;
                                _audio.Speak("Funtime Freddy", "Get ready for a surpriiiise!", VoiceProfileId.FuntimeFreddy, 2.8f, false);
                            }
                        }
                    }
                    else
                    {
                        bool blocked = (_freddyPendingAttackSide == -1 && _leftDoorClosed) ||
                                       (_freddyPendingAttackSide == 1 && _rightDoorClosed);
                        if (blocked)
                        {
                            _audio.PlaySfx("door_slam", 0.8f);
                            _freddyPendingAttackSide = 0;
                        }
                        else
                        {
                            _router.TriggerJumpscare("funtime_freddy", SceneId.CUSTOM_NIGHT_MENU);
                            return;
                        }
                    }
                }
            }

            // 2. Yenndo (Office spawn -> flip monitor S up to despawn)
            if (_yenndoActive && !_cameraUp)
            {
                _yenndoKillTimer += deltaTime;
                if (_yenndoKillTimer >= 2.6f)
                {
                    _router.TriggerJumpscare("yenndo", SceneId.CUSTOM_NIGHT_MENU);
                    return;
                }
            }

            // 3. Funtime Foxy (CAM 07 Curtain 5 stages -> Right Door D)
            if (_ai[3] > 0)
            {
                bool watchingFoxy = _cameraUp && _currentCam == 7;
                _foxyTimer += deltaTime * (watchingFoxy ? 0.25f : 1.0f);
                if (_foxyTimer >= Mathf.Lerp(8.5f, 3.8f, _ai[3] / 20f))
                {
                    _foxyTimer = 0f;
                    _foxyCurtainStage++;
                    if (_foxyCurtainStage >= 4)
                    {
                        if (_rightDoorClosed)
                        {
                            _audio.PlaySfx("door_slam", 0.9f);
                            _foxyCurtainStage = 0;
                        }
                        else
                        {
                            _router.TriggerJumpscare("funtime_foxy", SceneId.CUSTOM_NIGHT_MENU);
                            return;
                        }
                    }
                }
            }

            // 4. Bonnet (Walks across office -> click nose)
            if (_bonnetActive)
            {
                _bonnetX -= (140f + _ai[4] * 4f) * deltaTime;
                if (_bonnetX <= 180f)
                {
                    _router.TriggerJumpscare("funtime_freddy", SceneId.CUSTOM_NIGHT_MENU);
                    return;
                }
            }

            // 5. Bidybab (CAM 05 Center Vent 3 stages -> Close W)
            if (_ai[5] > 0)
            {
                _bidybabTimer += deltaTime;
                if (_bidybabTimer >= Mathf.Lerp(8.0f, 3.5f, _ai[5] / 20f))
                {
                    _bidybabTimer = 0f;
                    if (_bidybabVentStage < 3)
                    {
                        _bidybabVentStage++;
                        if (_bidybabVentStage == 3) _audio.PlaySfx("vent_crawl", 0.85f);
                    }
                    else
                    {
                        if (_ventDoorClosed)
                        {
                            _audio.PlaySfx("door_slam", 0.8f);
                            _bidybabVentStage = 0;
                        }
                        else
                        {
                            _router.TriggerJumpscare("circus_baby", SceneId.CUSTOM_NIGHT_MENU);
                            return;
                        }
                    }
                }
            }

            // 6. Electrobab (Drains Power in CAM 03/04 -> Press E Shock)
            if (_ai[6] > 0 && _electrobabCam == 0)
            {
                _electrobabTimer += deltaTime;
                if (_electrobabTimer >= Mathf.Lerp(11f, 4.8f, _ai[6] / 20f))
                {
                    _electrobabTimer = 0f;
                    _electrobabCam = (_freddyCam == 3) ? 4 : 3;
                }
            }

            // 7. Lolbit (3 desk monitors -> Type LOL or click LOL)
            if (_ai[7] > 0)
            {
                _lolbitTimer += deltaTime;
                if (_lolbitTimer >= Mathf.Lerp(10f, 4.2f, _ai[7] / 20f))
                {
                    _lolbitTimer = 0f;
                    _lolbitMonitors = Mathf.Min(3, _lolbitMonitors + 1);
                }
            }

            // 8. Minireena 1 (Drains Oxygen in CAM 06 -> Press E Shock)
            if (_ai[8] > 0 && !_minireena1Active)
            {
                _minireena1Timer += deltaTime;
                if (_minireena1Timer >= Mathf.Lerp(11f, 4.8f, _ai[8] / 20f))
                {
                    _minireena1Timer = 0f;
                    _minireena1Active = true;
                }
            }

            // 9. Minireena 2 (Climbs onto office screen blocking vision up to 3)
            if (_ai[9] > 0 && _minireena2Count < 3)
            {
                _minireena2Timer += deltaTime;
                if (_minireena2Timer >= Mathf.Lerp(14f, 6.0f, _ai[9] / 20f))
                {
                    _minireena2Timer = 0f;
                    _minireena2Count++;
                }
            }
        }

        private void CompleteShiftVictory()
        {
            _audio.SetMusicBoxStage(0);
            _audio.PlaySfx("chime", 1.0f);

            if (!_isCustomNight)
            {
                // Night 5 Fake Ending -> Award Star 3 and transition to TV Episode 4 (Exotic Butters + Ennard)
                _state.State.starsUnlocked.star3_fakeEnding = true;
                _state.Save();
                _router.TransitionTo(SceneId.TV_LIVING_ROOM, new Dictionary<string, object> { { "episode", 4 } });
            }
            else
            {
                _state.MarkPresetWon(_presetId, _difficultyIndex);
                if (_difficultyIndex >= 3)
                {
                    if (_presetId == "golden_freddy")
                    {
                        _router.TransitionTo(SceneId.SPRINGTRAP_FINALE, new Dictionary<string, object> { { "returnToExtras", false } });
                    }
                    else
                    {
                        _router.TransitionTo(SceneId.DECAY_8BIT_CUTSCENE, new Dictionary<string, object> { { "stage", _cutsceneStage }, { "returnToExtras", false } });
                    }
                }
                else
                {
                    _router.TransitionTo(SceneId.CUSTOM_NIGHT_MENU);
                }
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.05f, 0.05f, 0.08f));

            // Left Blast Door (A), Center Vent Door (W), Right Blast Door (D)
            RenderOfficeDoorsAndDesk(canvas);

            // Yenndo in office
            if (_yenndoActive && !_cameraUp)
            {
                canvas.DrawAnimatronicHead("yenndo", 640, 330, 1.55f, 0f, 0f, true);
                canvas.DrawText("YENNDO IN OFFICE! FLIP CAMERA [S] UP IMMEDIATELY!", 340, 140, 20, Color.red, TextAnchor.UpperCenter, 600, 40);
            }

            // Bonnet walking across office (click her nose!)
            if (_bonnetActive && !_cameraUp)
            {
                canvas.FillRect(_bonnetX - 35, 430, 70, 95, new Color(0.95f, 0.38f, 0.65f));
                if (canvas.Button(_bonnetX - 24, 445, 48, 38, "NOSE", Color.red, Color.white, 11))
                {
                    _audio.PlaySfx("honk", 0.9f);
                    _bonnetActive = false;
                }
            }

            // Minireena 2 vision blockers inside office
            for (int m = 0; m < _minireena2Count; m++)
            {
                float mx = 360 + m * 260;
                canvas.FillRect(mx, 220, 95, 290, new Color(0.92f, 0.88f, 0.78f, 0.92f));
                canvas.FillRect(mx + 15, 175, 65, 55, Color.white);
            }

            // Camera Monitor Overlay (CAM 01 .. CAM 07)
            if (_cameraUp)
            {
                RenderCameraTablet(canvas);
            }

            // Oxygen blackout dimming
            if (_oxygen < 45f)
            {
                float darkAlpha = Mathf.Clamp01((45f - _oxygen) / 45f) * 0.82f;
                canvas.FillRect(0, 0, 1280, 720, new Color(0f, 0f, 0f, darkAlpha));
            }

            // Lolbit full-screen audio/visual distraction when all 3 monitors fill
            if (_lolbitMonitors >= 3)
            {
                canvas.FillRect(240, 150, 800, 380, new Color(0.95f, 0.5f, 0.1f, 0.92f));
                canvas.DrawAnimatronicHead("lolbit", 640, 310, 1.3f, 0.2f);
                canvas.DrawText("PLEASE STAND BY! TYPE [L-O-L] OR CLICK [L-O-L] ON LEFT WALL KEYPAD!", 260, 470, 20, Color.white, TextAnchor.UpperCenter, 760, 45);
            }

            RenderHudAndControls(canvas);
            canvas.DrawCrtScanlinesAndStatic(0.14f, 0.35f);
        }

        private void RenderOfficeDoorsAndDesk(ProceduralCanvas canvas)
        {
            // Left Doorway (A)
            canvas.FillRect(60, 110, 180, 460, _leftDoorClosed ? new Color(0.35f, 0.38f, 0.42f) : new Color(0.02f, 0.02f, 0.03f));
            canvas.StrokeRect(60, 110, 180, 460, 4f, Color.gray);
            if (_leftDoorClosed) canvas.DrawText("LEFT DOOR\nCLOSED [A]", 75, 310, 18, Color.yellow, TextAnchor.UpperCenter, 150, 60);

            // Right Doorway (D)
            canvas.FillRect(1040, 110, 180, 460, _rightDoorClosed ? new Color(0.35f, 0.38f, 0.42f) : new Color(0.02f, 0.02f, 0.03f));
            canvas.StrokeRect(1040, 110, 180, 460, 4f, Color.gray);
            if (_rightDoorClosed) canvas.DrawText("RIGHT DOOR\nCLOSED [D]", 1055, 310, 18, Color.yellow, TextAnchor.UpperCenter, 150, 60);

            // Center Vent (W)
            canvas.FillRect(530, 90, 220, 160, _ventDoorClosed ? new Color(0.35f, 0.38f, 0.42f) : new Color(0.02f, 0.02f, 0.03f));
            canvas.StrokeRect(530, 90, 220, 160, 4f, Color.gray);
            if (_ventDoorClosed) canvas.DrawText("VENT CLOSED [W]", 545, 155, 16, Color.yellow, TextAnchor.UpperCenter, 190, 35);

            // Wall Keypad on Left (Supports 1-9-8-3 FNaF 4 Easter Egg + L-O-L button)
            canvas.FillRect(265, 160, 155, 220, new Color(0.2f, 0.22f, 0.26f));
            canvas.StrokeRect(265, 160, 155, 220, 2f, Color.cyan);
            canvas.DrawText($"KEYPAD: {_keypadBuffer}", 275, 166, 12, Color.green);
            for (int d = 1; d <= 9; d++)
            {
                int c = (d - 1) % 3;
                int r = (d - 1) / 3;
                if (canvas.Button(277 + c * 44, 188 + r * 42, 40, 36, d.ToString(), new Color(0.3f, 0.32f, 0.38f), Color.white, 14))
                {
                    OnWallKeypadDigitPressed(d);
                }
            }
            if (canvas.Button(277, 320, 128, 44, "L - O - L", new Color(0.85f, 0.45f, 0.1f), Color.white, 16))
            {
                _lolbitMonitors = 0;
                _audio.PlaySfx("chime", 0.85f);
            }

            // Desk with 3 Small CRT Monitors + Fredbear Plush with Walkie-Talkie
            canvas.FillRect(320, 450, 640, 140, new Color(0.16f, 0.14f, 0.12f));
            for (int m = 0; m < 3; m++)
            {
                float mx = 360 + m * 145;
                bool lolOnThis = (_lolbitMonitors > m);
                Color screenCol = lolOnThis ? new Color(0.9f, 0.45f, 0.1f) :
                                  (_fnaf4MonitorsUnlocked ? new Color(0.12f, 0.24f, 0.35f) : new Color(0.04f, 0.05f, 0.06f));
                canvas.FillRect(mx, 345, 130, 100, screenCol);
                canvas.StrokeRect(mx, 345, 130, 100, 3f, Color.gray);

                if (lolOnThis)
                {
                    canvas.DrawText("LOLBIT", mx + 25, 385, 16, Color.white);
                }
                else if (_fnaf4MonitorsUnlocked)
                {
                    string[] fnaf4Rooms = { "FNaF 4 BEDROOM", "FNaF 4 BED", "PLUSHTRAP HALL" };
                    canvas.DrawText($"1983 LIVE FEED:\n{fnaf4Rooms[m]}", mx + 8, 372, 12, Color.cyan);
                }
            }

            // Golden Fredbear Plush with walkie-talkie on right of desk (click nose to honk)
            canvas.FillRect(830, 355, 75, 90, new Color(0.92f, 0.78f, 0.18f));
            canvas.FillRect(845, 335, 45, 22, new Color(0.45f, 0.18f, 0.68f)); // Purple hat
            if (canvas.Button(850, 380, 36, 26, "●", new Color(0.1f, 0.1f, 0.1f), Color.white, 14))
            {
                _audio.PlaySfx("honk", 0.9f);
            }
        }

        private void OnWallKeypadDigitPressed(int d)
        {
            _audio.PlaySfx("springlock_wind", 0.75f);
            _keypadBuffer += d.ToString();
            if (_keypadBuffer.Length > 4)
            {
                _keypadBuffer = _keypadBuffer.Substring(_keypadBuffer.Length - 4);
            }
            if (_keypadBuffer == "1983" && _lolbitMonitors == 0)
            {
                _fnaf4MonitorsUnlocked = true;
                _audio.PlaySfx("chime", 1.0f);
            }
        }

        private void RenderCameraTablet(ProceduralCanvas canvas)
        {
            canvas.FillRect(140, 60, 1000, 520, new Color(0.04f, 0.08f, 0.06f, 0.96f));
            canvas.StrokeRect(140, 60, 1000, 520, 4f, Color.green);

            string[] camTitles =
            {
                "",
                "CAM 01 — LEFT HALLWAY",
                "CAM 02 — RIGHT HALLWAY",
                "CAM 03 — LEFT CLOSET",
                "CAM 04 — RIGHT CLOSET",
                "CAM 05 — CENTER VENTILATION",
                "CAM 06 — EAST HALL / OXYGEN PIPES",
                "CAM 07 — FUNTIME FOXY CURTAIN COVE"
            };

            canvas.DrawText(camTitles[_currentCam], 170, 80, 22, Color.green);

            // Render active camera feed contents
            bool balloraStatic = (_balloraStage > 0) && ((_currentCam == 1 && _balloraSide == -1) || (_currentCam == 2 && _balloraSide == 1));
            if (balloraStatic)
            {
                canvas.FillRect(180, 130, 560, 380, new Color(0.25f, 0.25f, 0.28f));
                canvas.DrawText("SIGNAL LOST — BALLORA IN HALLWAY! LISTEN FOR AUDIO!", 220, 300, 18, Color.red);
            }
            else
            {
                canvas.FillRect(180, 130, 560, 380, new Color(0.06f, 0.1f, 0.12f));

                if (!_isCustomNight && _ennardCam == _currentCam)
                {
                    canvas.DrawAnimatronicHead("ennard", 460, 320, 1.35f, _ennardStage * 0.25f);
                }

                if (_isCustomNight)
                {
                    if (_freddyCam == _currentCam)
                    {
                        canvas.DrawAnimatronicHead("funtime_freddy", 420, 320, 1.25f, 0f);
                    }
                    if (_electrobabCam == _currentCam)
                    {
                        canvas.DrawAnimatronicHead("circus_baby", 560, 380, 0.7f, 0f);
                        canvas.DrawText("ELECTROBAB DRAINING POWER! PRESS [E] SHOCK!", 220, 150, 16, Color.yellow);
                    }
                    if (_currentCam == 5 && _bidybabVentStage > 0)
                    {
                        canvas.DrawAnimatronicHead("circus_baby", 460, 320, 0.6f + _bidybabVentStage * 0.35f, 0f);
                        canvas.DrawText($"BIDYBAB IN VENT — STAGE {_bidybabVentStage}/3", 320, 150, 16, Color.yellow);
                    }
                    if (_currentCam == 6 && _minireena1Active)
                    {
                        canvas.DrawAnimatronicHead("ballora", 460, 320, 0.9f, 0f);
                        canvas.DrawText("MINIREENAS DRAINING OXYGEN! PRESS [E] SHOCK!", 220, 150, 16, Color.cyan);
                    }
                    if (_currentCam == 7)
                    {
                        canvas.DrawText($"FUNTIME FOXY CURTAIN STAGE: {_foxyCurtainStage}/4 {(_foxyCurtainStage >= 4 ? "(GONE! CLOSE RIGHT DOOR [D]!)" : "(VIEWING SLOWS FOXY)")}", 200, 150, 16, _foxyCurtainStage >= 3 ? Color.red : Color.green);
                        if (_foxyCurtainStage > 0 && _foxyCurtainStage < 4)
                        {
                            canvas.DrawAnimatronicHead("funtime_foxy", 460, 330, 0.8f + _foxyCurtainStage * 0.2f, 0f);
                        }
                    }
                }
            }

            // Camera Selection Buttons (CAM 01 .. CAM 07)
            for (int c = 1; c <= 7; c++)
            {
                Color btnCol = (_currentCam == c) ? new Color(0.15f, 0.55f, 0.22f) : new Color(0.12f, 0.22f, 0.16f);
                if (canvas.Button(780, 130 + (c - 1) * 52, 320, 44, camTitles[c], btnCol, Color.white, 14))
                {
                    _currentCam = c;
                    _audio.PlaySfx("springlock_wind", 0.6f);
                }
            }
        }

        private void RenderHudAndControls(ProceduralCanvas canvas)
        {
            int displayHour = Mathf.FloorToInt((_nightElapsed / _nightDuration) * 6f);
            string hourStr = displayHour == 0 ? "12 AM" : $"{displayHour} AM";

            string pwrWarn = _electrobabCam != 0 ? " ⚠ DRAIN!" : "";
            string oxyWarn = _minireena1Active ? " ⚠ DRAIN!" : "";

            canvas.DrawText($"POWER: {Mathf.CeilToInt(_power)}%{pwrWarn}   |   OXYGEN: {Mathf.CeilToInt(_oxygen)}%{oxyWarn}", 40, 22, 22, Color.white);
            canvas.DrawText(hourStr, 1120, 22, 28, Color.yellow);

            // Bottom Action Bar
            if (canvas.Button(40, 640, 190, 50, $"LEFT DOOR [A]\n{(_leftDoorClosed ? "CLOSED" : "OPEN")}", _leftDoorClosed ? Color.red : new Color(0.2f, 0.25f, 0.3f), Color.white, 14))
                ToggleDoor(ref _leftDoorClosed);

            if (canvas.Button(245, 640, 190, 50, $"CENTER VENT [W]\n{(_ventDoorClosed ? "CLOSED" : "OPEN")}", _ventDoorClosed ? Color.red : new Color(0.2f, 0.25f, 0.3f), Color.white, 14))
                ToggleDoor(ref _ventDoorClosed);

            if (canvas.Button(450, 640, 340, 50, _cameraUp ? "LOWER CAMERA MONITOR [S]" : "RAISE CAMERA MONITOR [S]", new Color(0.18f, 0.38f, 0.55f), Color.white, 16))
                ToggleCameraMonitor();

            if (canvas.Button(805, 640, 210, 50, "CONTROLLED SHOCK [E]", new Color(0.65f, 0.35f, 0.08f), Color.yellow, 14))
                TriggerControlledShock();

            if (canvas.Button(1030, 640, 210, 50, $"RIGHT DOOR [D]\n{(_rightDoorClosed ? "CLOSED" : "OPEN")}", _rightDoorClosed ? Color.red : new Color(0.2f, 0.25f, 0.3f), Color.white, 14))
                ToggleDoor(ref _rightDoorClosed);
        }

        public void Exit()
        {
            _audio.SetMusicBoxStage(0);
        }
    }
}
