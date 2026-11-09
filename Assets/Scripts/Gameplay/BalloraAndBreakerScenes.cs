using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class BalloraGalleryScene : IGameScene
    {
        public SceneId Id => SceneId.BALLORA_GALLERY;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private bool _inbound = true;
        private float _distance = 0f; // 0 to 100
        private float _balloraOrbitPhase = 0f;
        private int _musicStage = 1; // 1=distant, 2=medium, 3=loud danger
        private float _movementDangerTimer = 0f;
        private bool _sangLyrics = false;

        // Secret 0+1 Debug Blue Screen Easter Egg
        private float _debugHoldTimer = 0f;
        private bool _blueScreenActive = false;
        private int _blueScreenCounter = 1042;

        public BalloraGalleryScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _inbound = parameters == null || !parameters.TryGetValue("inbound", out object ib) || (bool)ib;
            _distance = 0f;
            _balloraOrbitPhase = 0f;
            _musicStage = 1;
            _movementDangerTimer = 0f;
            _sangLyrics = false;
            _debugHoldTimer = 0f;
            _blueScreenActive = false;

            _audio.SetMusicBoxStage(1);
            if (_inbound)
            {
                _audio.Speak("HandUnit", "Go forward through Ballora Gallery as fast as you can to reach the Breaker Room.", VoiceProfileId.HandUnit, 5.0f, true);
            }
            else
            {
                _audio.Speak("HandUnit", "Now crawl carefully back through Ballora Gallery to the Primary Control Module.", VoiceProfileId.HandUnit, 5.0f, true);
            }
        }

        public void Update(float deltaTime)
        {
            // Secret Blue Screen Easter Egg: Hold 0 + 1 for 83s (or 5s when Debug Toggle is active)
            bool holdingZeroAndOne = (Input.GetKey(KeyCode.Alpha0) || Input.GetKey(KeyCode.Keypad0)) &&
                                     (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Keypad1));
            if (holdingZeroAndOne && !_blueScreenActive)
            {
                _debugHoldTimer += deltaTime;
                float requiredSec = _state.State.debugForceEasterEggs ? 5.0f : 83.0f;
                if (_debugHoldTimer >= requiredSec)
                {
                    _blueScreenActive = true;
                    _audio.SetMusicBoxStage(0);
                }
            }
            else if (!holdingZeroAndOne)
            {
                _debugHoldTimer = 0f;
            }

            if (_blueScreenActive)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                {
                    _blueScreenCounter += UnityEngine.Random.Range(11, 97);
                }
                return;
            }

            _balloraOrbitPhase += deltaTime * 0.55f;
            float proximity = Mathf.Sin(_balloraOrbitPhase);
            if (proximity < 0.15f) _musicStage = 1;
            else if (proximity < 0.68f) _musicStage = 2;
            else _musicStage = 3;

            float pan = Mathf.Cos(_balloraOrbitPhase) * 0.75f;
            _audio.SetMusicBoxStage(_musicStage, pan);

            if (_musicStage == 3 && !_sangLyrics && _distance > 25f)
            {
                _sangLyrics = true;
                _audio.Speak("Ballora", "Why do you hide inside your walls, when there is music in my halls? All I see is an empty room, no more joy, an empty tomb. It's so good to sing all day, to dance, to spin, to fly away.", VoiceProfileId.CircusBaby, 8.0f, false);
            }

            bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool sprinting = moving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

            if (moving)
            {
                _distance += (sprinting ? 22f : 12f) * deltaTime;
                if (_musicStage == 3 || sprinting)
                {
                    _movementDangerTimer += deltaTime * (sprinting ? 2.2f : 1.0f);
                    if (_movementDangerTimer >= 1.15f)
                    {
                        _router.TriggerJumpscare("ballora", SceneId.BALLORA_GALLERY, new Dictionary<string, object> { { "inbound", _inbound } });
                        return;
                    }
                }
            }
            else
            {
                _movementDangerTimer = Mathf.Max(0f, _movementDangerTimer - deltaTime * 1.5f);
            }

            if (_distance >= 100f)
            {
                _audio.SetMusicBoxStage(0);
                if (_inbound)
                {
                    _router.TransitionTo(SceneId.BREAKER_ROOM);
                }
                else
                {
                    _router.TransitionTo(SceneId.PRIMARY_CONTROL, new Dictionary<string, object> { { "night", 2 }, { "fromBreaker", true } });
                }
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            if (_blueScreenActive)
            {
                canvas.FillRect(0, 0, 1280, 720, new Color(0.0f, 0.08f, 0.72f));
                canvas.DrawText($"AFTON ROBOTICS DIAGNOSTIC TERMINAL — SECRET 0+1 DEBUG SCREEN\n\nSYSTEM DATE: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\nMEMORY COUNTER [PRESS 1]: {_blueScreenCounter}\nAUDIO TRACK: CRUMBLING_DREAMS_STOPPED", 100, 120, 22, Color.white, TextAnchor.UpperLeft, 1000, 300);
                if (canvas.Button(100, 520, 260, 48, "EXIT DEBUG SCREEN", Color.white, Color.blue, 16))
                {
                    _blueScreenActive = false;
                }
                return;
            }

            canvas.FillRect(0, 0, 1280, 720, new Color(0.02f, 0.02f, 0.05f));

            // Ballora gliding silhouette in the dark
            float bx = 640f + Mathf.Cos(_balloraOrbitPhase) * 380f;
            float scale = _musicStage == 3 ? 1.55f : (_musicStage == 2 ? 0.95f : 0.55f);
            canvas.DrawAnimatronicHead("ballora", bx, 340f, scale, 0f);

            // Music Box Volume Stage Indicator
            Color stageCol = _musicStage == 1 ? Color.green : (_musicStage == 2 ? Color.yellow : Color.red);
            string stageText = _musicStage == 1 ? "STAGE 1: DISTANT (SAFE TO CRAWL [W])" :
                               (_musicStage == 2 ? "STAGE 2: APPROACHING (PROCEED WITH CAUTION)" : "STAGE 3: LOUD! STOP MOVING IMMEDIATELY!");
            canvas.DrawText($"BALLORA MUSIC BOX: {stageText}", 240, 40, 20, stageCol, TextAnchor.UpperCenter, 800, 40);
            canvas.DrawText($"DISTANCE TO {(_inbound ? "BREAKER ROOM" : "CONTROL MODULE")}: {Mathf.RoundToInt(_distance)}%", 340, 630, 20, Color.white, TextAnchor.UpperCenter, 600, 40);

            if (_debugHoldTimer > 0.1f)
            {
                canvas.DrawText($"HOLDING [0 + 1] SECRET TIMER: {_debugHoldTimer:F1}s", 60, 660, 14, Color.cyan);
            }

            canvas.DrawCrtScanlinesAndStatic(0.16f, 0.5f);
        }

        public void Exit()
        {
            _audio.SetMusicBoxStage(0);
        }
    }

    public sealed class BreakerRoomScene : IGameScene
    {
        public SceneId Id => SceneId.BREAKER_ROOM;

        private readonly SceneGraphRouter _router;
        private readonly ProceduralAudioSynthesizer _audio;

        private static readonly string[] SystemNames = new string[]
        {
            "Ballora Gallery",
            "Funtime Auditorium",
            "Circus Gallery",
            "Circus Control",
            "Parts/Services",
            "Control Module",
            "Breaker Room",
            "Elevators"
        };

        private readonly float[] _systemProgress = new float[8];
        private int _selectedSystem = -1;
        private bool _monitorUp = true;
        private int _freddyPhase = 0; // 0=hidden, 1=far, 2=mid, 3=close danger
        private float _freddyAdvanceTimer = 0f;
        private float _mascotCooldown = 0f;

        public BreakerRoomScene(SceneGraphRouter router, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            for (int i = 0; i < 8; i++) _systemProgress[i] = 0f;
            _selectedSystem = -1;
            _monitorUp = true;
            _freddyPhase = 0;
            _freddyAdvanceTimer = 0f;
            _mascotCooldown = 0f;

            _audio.Speak("HandUnit", "Use the interface on the left to reboot all eight facility systems to 100%. Use the Mascot Response Audio button to calm Funtime Freddy.", VoiceProfileId.HandUnit, 6.5f, true);
        }

        public void Update(float deltaTime)
        {
            if (_mascotCooldown > 0f) _mascotCooldown -= deltaTime;

            if (Input.GetKeyDown(KeyCode.S))
            {
                _monitorUp = !_monitorUp;
            }

            if (!_monitorUp && Input.GetKeyDown(KeyCode.Space) && _mascotCooldown <= 0f)
            {
                PlayBonBonCalmingAudio();
            }

            // Funtime Freddy advances faster while monitor is up
            _freddyAdvanceTimer += deltaTime * (_monitorUp ? 1.35f : 0.45f);
            if (_freddyAdvanceTimer >= 4.5f)
            {
                _freddyAdvanceTimer = 0f;
                _freddyPhase++;
                if (_freddyPhase == 1)
                    _audio.Speak("Funtime Freddy", "Bon-Bon, say hi to our friends!", VoiceProfileId.FuntimeFreddy, 3.0f, true);
                else if (_freddyPhase == 2)
                    _audio.Speak("Funtime Freddy", "I see you over there in the dark! Come on out!", VoiceProfileId.FuntimeFreddy, 3.2f, true);
                else if (_freddyPhase == 3)
                    _audio.Speak("Funtime Freddy", "Well, hello again! Ha-ha-ha!", VoiceProfileId.FuntimeFreddy, 3.0f, true);
                else if (_freddyPhase >= 4)
                {
                    _router.TriggerJumpscare("funtime_freddy", SceneId.BREAKER_ROOM);
                    return;
                }
            }

            // Rebooting selected system while holding mouse
            if (_monitorUp && _selectedSystem >= 0 && _selectedSystem < 8 && Input.GetMouseButton(0))
            {
                _systemProgress[_selectedSystem] = Mathf.Clamp(_systemProgress[_selectedSystem] + 24f * deltaTime, 0f, 100f);
            }

            // Check if all 8 systems reached 100%
            bool allOnline = true;
            for (int i = 0; i < 8; i++)
            {
                if (_systemProgress[i] < 100f)
                {
                    allOnline = false;
                    break;
                }
            }

            if (allOnline)
            {
                _router.TransitionTo(SceneId.BALLORA_GALLERY, new Dictionary<string, object> { { "inbound", false } });
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.03f, 0.03f, 0.05f));

            // Right room view (Funtime Freddy advancing across 4 phases)
            float freddyScale = _freddyPhase == 0 ? 0.4f : (_freddyPhase == 1 ? 0.75f : (_freddyPhase == 2 ? 1.25f : 1.85f));
            if (_freddyPhase > 0)
            {
                canvas.DrawAnimatronicHead("funtime_freddy", 900, 360, freddyScale, _freddyPhase >= 3 ? 0.35f : 0f);
            }

            // Top-Right Danger Indicator (Green -> Yellow -> Orange -> Flashing Red)
            Color dangerCol = _freddyPhase == 0 ? Color.green :
                              (_freddyPhase == 1 ? Color.yellow :
                              (_freddyPhase == 2 ? new Color(1f, 0.5f, 0f) :
                              (Mathf.PingPong(Time.time * 8f, 1f) > 0.5f ? Color.red : Color.black)));
            canvas.FillRect(1020, 35, 210, 46, dangerCol);
            canvas.StrokeRect(1020, 35, 210, 46, 3f, Color.white);
            canvas.DrawText($"DANGER PHASE: {_freddyPhase}/3", 1030, 46, 17, Color.black, TextAnchor.UpperCenter, 190, 30);

            if (_monitorUp)
            {
                RenderBreakerMonitor(canvas);
            }
            else
            {
                if (canvas.Button(720, 540, 360, 56, "PLAY MASCOT AUDIO [SPACE]\n(BON-BON CALMING VOICE)", new Color(0.18f, 0.45f, 0.25f), Color.white, 16))
                {
                    PlayBonBonCalmingAudio();
                }
            }

            if (canvas.Button(60, 640, 340, 48, _monitorUp ? "LOWER MONITOR [S] (CHECK FREDDY)" : "RAISE BREAKER MONITOR [S]", new Color(0.25f, 0.28f, 0.38f), Color.white, 16))
            {
                _monitorUp = !_monitorUp;
            }

            canvas.DrawCrtScanlinesAndStatic(0.15f, 0.4f);
        }

        private void RenderBreakerMonitor(ProceduralCanvas canvas)
        {
            canvas.FillRect(50, 50, 920, 560, new Color(0.05f, 0.12f, 0.08f, 0.96f));
            canvas.StrokeRect(50, 50, 920, 560, 4f, Color.green);
            canvas.DrawText("CIRCUS BABY'S ENTERTAINMENT & RENTAL — POWER BREAKER TERMINAL", 75, 68, 19, Color.green);

            // Left Column: 8 Reboot Buttons (Hold Left-Click to Charge to 100%)
            for (int i = 0; i < 8; i++)
            {
                float by = 115 + i * 56;
                bool online = _systemProgress[i] >= 100f;
                string status = online ? "ONLINE (100%)" : $"{Mathf.RoundToInt(_systemProgress[i])}% [HOLD]";
                Color btnCol = online ? new Color(0.1f, 0.45f, 0.15f) : new Color(0.12f, 0.22f, 0.14f);

                if (canvas.IsHovered(75, by, 390, 46) && Input.GetMouseButton(0))
                {
                    _selectedSystem = i;
                }
                canvas.Button(75, by, 390, 46, $"{SystemNames[i]} — {status}", btnCol, online ? Color.green : Color.yellow, 15);
            }

            // Right Column: Facility Map + Faded FNaF 4 Observation Layouts
            canvas.StrokeRect(500, 115, 440, 460, 2f, new Color(0.2f, 0.85f, 0.3f));
            canvas.DrawText("FACILITY & OBSERVATION SCHEMATIC", 520, 125, 16, Color.green);

            // Main Facility Rooms + 4 Grey Animatronic Dots
            canvas.StrokeRect(650, 270, 110, 75, 2f, Color.green);
            canvas.DrawText("CTRL", 685, 298, 13, Color.green);
            canvas.StrokeRect(530, 270, 100, 75, 2f, Color.green); // Ballora Gallery
            canvas.FillRect(575, 300, 10, 10, Color.gray);
            canvas.StrokeRect(780, 270, 100, 75, 2f, Color.green); // Funtime Auditorium
            canvas.FillRect(825, 300, 10, 10, Color.gray);
            canvas.StrokeRect(650, 175, 110, 70, 2f, Color.green); // Circus Gallery
            canvas.FillRect(700, 205, 10, 10, Color.gray);
            canvas.StrokeRect(520, 185, 80, 60, 2f, Color.green); // Breaker Room
            canvas.FillRect(555, 210, 10, 10, Color.gray);

            // Faded FNaF 4 Observation Layouts (Obsv. 1, Obsv. 2, Plushtrap Hallway)
            Color fnaf4Col = new Color(0.45f, 0.55f, 0.45f, 0.55f);
            // Obsv. 1 connected to Circus Gallery (2 grey dots)
            canvas.StrokeRect(790, 165, 125, 75, 2f, fnaf4Col);
            canvas.DrawText("OBSV. 1 (FNaF4)", 800, 172, 12, fnaf4Col);
            canvas.FillRect(820, 205, 8, 8, Color.gray);
            canvas.FillRect(870, 205, 8, 8, Color.gray);

            // Obsv. 2 connected to Private Room (5 grey dots)
            canvas.StrokeRect(760, 420, 155, 115, 2f, fnaf4Col);
            canvas.DrawText("OBSV. 2 (BEDROOM)", 772, 428, 12, fnaf4Col);
            for (int d = 0; d < 5; d++) canvas.FillRect(780 + d * 24, 480, 8, 8, Color.gray);

            // Plushtrap Hallway connected to Ballora Gallery (5 grey dots)
            canvas.StrokeRect(520, 410, 120, 125, 2f, fnaf4Col);
            canvas.DrawText("PLUSHTRAP\nHALL (OBSV)", 530, 418, 11, fnaf4Col);
            for (int d = 0; d < 5; d++) canvas.FillRect(535 + d * 18, 495, 8, 8, Color.gray);
        }

        private void PlayBonBonCalmingAudio()
        {
            _mascotCooldown = 1.5f;
            _freddyPhase = Mathf.Max(0, _freddyPhase - 2);
            _freddyAdvanceTimer = 0f;
            _audio.Speak("Bon-Bon (Recorded)", "Calm down and go back to sleep! No one is here! I think it was just a mouse!", VoiceProfileId.BonBon, 3.8f, true);
        }

        public void Exit()
        {
        }
    }
}
