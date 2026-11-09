using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class FuntimeAuditoriumScene : IGameScene
    {
        public SceneId Id => SceneId.FUNTIME_AUDITORIUM;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _night = 3;
        private string _phase = "inbound"; // inbound, outbound_n3, outbound_n5
        private float _progress = 0f;
        private float _flashCooldown = 0f;
        private float _flashActiveTimer = 0f;
        private bool _foxyVisibleOnFlash = false;
        private float _foxyWaitTimer = 0f;

        // Night 5 Audio Maze state
        private int _mazeStep = 0;
        private float _mazeStepTimer = 0f;
        private float _privateRoomRightHold = 0f;

        public FuntimeAuditoriumScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : _state.State.currentNight;
            _phase = parameters != null && parameters.TryGetValue("phase", out object ph) ? (string)ph : "inbound";
            _progress = 0f;
            _flashCooldown = 0f;
            _flashActiveTimer = 0f;
            _foxyVisibleOnFlash = false;
            _foxyWaitTimer = 0f;
            _mazeStep = 0;
            _mazeStepTimer = 0f;
            _privateRoomRightHold = 0f;

            if (_night == 3 && _phase == "inbound")
            {
                _audio.Speak("HandUnit", "Funtime Foxy is motion-activated. Use your Flash Beacon with SPACE sparingly to see in the dark. Stop moving if Funtime Foxy is near.", VoiceProfileId.HandUnit, 6.5f, true);
            }
            else if (_night == 3 && _phase == "outbound_n3")
            {
                _audio.Speak("HandUnit", "Great job in Parts and Service! Now walk back through Funtime Auditorium to complete your shift.", VoiceProfileId.HandUnit, 5.5f, true);
            }
            else if (_night == 5 && _phase == "inbound")
            {
                _audio.Speak("HandUnit", "Walk forward through Funtime Auditorium to reach Parts and Service.", VoiceProfileId.HandUnit, 4.5f, true);
            }
            else if (_night == 5 && _phase == "outbound_n5")
            {
                _audio.Speak("Circus Baby", "Ballora is in front of you. Do not use your flash beacon. Follow my voice carefully. Go forward.", VoiceProfileId.CircusBaby, 5.5f, true);
            }
        }

        public void Update(float deltaTime)
        {
            if (_flashCooldown > 0f) _flashCooldown -= deltaTime;
            if (_flashActiveTimer > 0f) _flashActiveTimer -= deltaTime;

            if (_night == 5 && _phase == "outbound_n5")
            {
                UpdateNight5AudioMaze(deltaTime);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) && _flashCooldown <= 0f)
            {
                FireFlashBeacon();
            }

            bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

            if (_night == 3 && _phase == "outbound_n3")
            {
                if (moving)
                {
                    _progress += 35f * deltaTime;
                    if (_progress >= 35f)
                    {
                        // Scripted inevitable Funtime Foxy jumpscare transitioning to Night 4
                        _state.State.currentNight = Mathf.Max(_state.State.currentNight, 4);
                        _state.Save();
                        _router.TriggerJumpscare("funtime_foxy", SceneId.PRE_NIGHT_INTERLUDE, new Dictionary<string, object> { { "night", 4 } });
                        return;
                    }
                }
                return;
            }

            if (_foxyVisibleOnFlash)
            {
                if (moving)
                {
                    _router.TriggerJumpscare("funtime_foxy", SceneId.FUNTIME_AUDITORIUM, new Dictionary<string, object> { { "night", _night }, { "phase", _phase } });
                    return;
                }
                _foxyWaitTimer += deltaTime;
                if (_foxyWaitTimer >= 4.0f)
                {
                    _foxyVisibleOnFlash = false;
                    _foxyWaitTimer = 0f;
                }
            }
            else if (moving)
            {
                _progress += 18f * deltaTime;
                if (UnityEngine.Random.value < 0.14f * deltaTime)
                {
                    _foxyVisibleOnFlash = true;
                    _foxyWaitTimer = 0f;
                }
            }

            if (_progress >= 100f)
            {
                _router.TransitionTo(SceneId.PARTS_AND_SERVICE, new Dictionary<string, object> { { "night", _night } });
            }
        }

        private void UpdateNight5AudioMaze(float deltaTime)
        {
            // Path 2: Fake Ending (Hold W + D to go Forward & Right to Private Room)
            bool holdForwardRight = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) &&
                                    (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
            if (holdForwardRight && _mazeStep <= 1)
            {
                _privateRoomRightHold += deltaTime;
                if (_privateRoomRightHold >= 3.2f)
                {
                    if (_state.State.starsUnlocked.star2_minigame || _state.State.debugForceEasterEggs)
                    {
                        _audio.Speak("Facility Security", "Access Granted. Entering Private Room.", VoiceProfileId.HandUnit, 3.0f, true);
                        _router.TransitionTo(SceneId.PRIVATE_ROOM, new Dictionary<string, object> { { "customNight", false } });
                    }
                    else
                    {
                        _audio.Speak("Facility Security", "Access Denied. Keycard required from 8-Bit Circus Baby Minigame.", VoiceProfileId.HandUnit, 4.0f, true);
                        _router.TriggerJumpscare("ennard", SceneId.PARTS_AND_SERVICE, new Dictionary<string, object> { { "night", 5 } });
                    }
                    return;
                }
            }

            bool w = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool a = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);

            _mazeStepTimer += deltaTime;
            if (_mazeStep == 0 && w && _mazeStepTimer > 2.5f)
            {
                _mazeStep = 1;
                _mazeStepTimer = 0f;
                _audio.Speak("Circus Baby", "Stop. Go forward and left.", VoiceProfileId.CircusBaby, 3.5f, true);
            }
            else if (_mazeStep == 1 && w && a && _mazeStepTimer > 2.5f)
            {
                _mazeStep = 2;
                _mazeStepTimer = 0f;
                _audio.Speak("Circus Baby", "Stop. Be silent. She is right in front of you. Don't move. He is here to help, Ballora. He is not here to hurt us.", VoiceProfileId.CircusBaby, 6.5f, true);
            }
            else if (_mazeStep == 2)
            {
                if (w && _mazeStepTimer > 1.2f && _mazeStepTimer < 5.5f)
                {
                    _router.TriggerJumpscare("ennard", SceneId.FUNTIME_AUDITORIUM, new Dictionary<string, object> { { "night", 5 }, { "phase", "outbound_n5" } });
                    return;
                }
                if (_mazeStepTimer >= 6.5f)
                {
                    _mazeStep = 3;
                    _mazeStepTimer = 0f;
                    _audio.Speak("Circus Baby", "Go forward again. Nearly there.", VoiceProfileId.CircusBaby, 3.5f, true);
                }
            }
            else if (_mazeStep == 3 && w && _mazeStepTimer > 2.5f)
            {
                _mazeStep = 4;
                _mazeStepTimer = 0f;
                _audio.Speak("Circus Baby", "Stop. Go forward and left into the Scooping Room.", VoiceProfileId.CircusBaby, 4.0f, true);
            }
            else if (_mazeStep == 4 && w && a && _mazeStepTimer > 2.0f)
            {
                _router.TransitionTo(SceneId.SCOOPING_ROOM);
            }
        }

        private void FireFlashBeacon()
        {
            _flashCooldown = 3.0f;
            _flashActiveTimer = 0.45f;
            _audio.PlaySfx("flash_beacon", 0.9f);
        }

        public void Render(ProceduralCanvas canvas)
        {
            bool lit = _flashActiveTimer > 0f;
            canvas.FillRect(0, 0, 1280, 720, lit ? new Color(0.22f, 0.26f, 0.36f) : new Color(0.01f, 0.01f, 0.02f));

            if (lit)
            {
                // Checkered floor perspective
                canvas.FillRect(140, 420, 1000, 260, new Color(0.15f, 0.16f, 0.22f));
                if (_router.RareSpawnActive)
                {
                    canvas.DrawAnimatronicHead("yenndo", 640, 340, 1.55f, 0f, 0f, true);
                    canvas.DrawText("1/1,000 YENNDO SIGHTING!", 520, 120, 18, Color.yellow);
                }
                else if (_foxyVisibleOnFlash)
                {
                    canvas.DrawAnimatronicHead("funtime_foxy", 640, 340, 1.65f, 0.2f);
                    canvas.DrawText("FUNTIME FOXY DETECTED! STOP MOVING & WAIT 4 SECONDS!", 340, 100, 20, Color.red, TextAnchor.UpperCenter, 600, 50);
                }
                else
                {
                    canvas.DrawText("PATH CLEAR — HOLD [W] TO WALK FORWARD", 440, 120, 18, Color.green, TextAnchor.UpperCenter, 400, 40);
                }
            }

            if (_night == 5 && _phase == "outbound_n5")
            {
                canvas.DrawText($"NIGHT 5 AUDIO MAZE — STEP {_mazeStep}/4\n• REAL ENDING: FOLLOW BABY'S COMMANDS ([W] / [W+A] / STOP)\n• FAKE ENDING (PRIVATE ROOM): HOLD [W + D] ({_privateRoomRightHold:F1}/3.2s)", 180, 40, 17, Color.cyan, TextAnchor.UpperCenter, 920, 90);
            }
            else
            {
                string beaconStatus = _flashCooldown <= 0f ? "FLASH BEACON READY [SPACE]" : $"BEACON RECHARGING ({_flashCooldown:F1}s)";
                canvas.DrawText($"{beaconStatus}  |  PROGRESS: {Mathf.RoundToInt(_progress)}%  |  HOLD [W] TO WALK", 240, 640, 18, Color.yellow, TextAnchor.UpperCenter, 800, 40);
            }

            canvas.DrawCrtScanlinesAndStatic(lit ? 0.25f : 0.1f, 0.55f);
        }

        public void Exit()
        {
        }
    }

    public sealed class PartsAndServiceScene : IGameScene
    {
        public SceneId Id => SceneId.PARTS_AND_SERVICE;

        private readonly SceneGraphRouter _router;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _night = 3;

        // Night 3 Funtime Freddy & Bon-Bon repair state
        // 0=RightCheek(screen left), 1=LeftCheek(screen right), 2=RightEye(screen left), 3=Nose, 4=EndoJaw, 5=ChestModule, 6=BonBonCatch
        private int _freddyStep = 0;
        private bool _clarifiedRight = false;
        private float _bonBonHeight = 0f; // 0=hidden behind, 1=peeking above shoulder

        // Night 5 Circus Baby Passcode state
        private bool _babyHatchOpen = false;
        private readonly List<int> _targetPasscode = new List<int>();
        private int _passcodeStep = 0;
        private float _digitSpeakTimer = 0f;
        private bool _cardTaken = false;

        public PartsAndServiceScene(SceneGraphRouter router, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : 3;
            _freddyStep = 0;
            _clarifiedRight = false;
            _bonBonHeight = 0f;

            _babyHatchOpen = false;
            _targetPasscode.Clear();
            for (int i = 0; i < 6; i++) _targetPasscode.Add(UnityEngine.Random.Range(1, 10));
            _passcodeStep = 0;
            _digitSpeakTimer = 1.5f;
            _cardTaken = false;

            if (_night == 3)
            {
                _audio.Speak("HandUnit", "Welcome to Parts and Service. Deactivated Funtime Freddy is in front of you. First, press the small button under Freddy's right cheek.", VoiceProfileId.HandUnit, 6.5f, true);
            }
            else
            {
                _audio.Speak("Circus Baby", "I'm broken. I can't be fixed. I'm going to be taken to the Scooping Room soon, but it won't fix what is wrong with me. Will you help me? Press the button on my cheek.", VoiceProfileId.CircusBaby, 7.5f, true);
            }
        }

        public void Update(float deltaTime)
        {
            if (_night == 3 && _freddyStep == 6)
            {
                // Bon-Bon rises when cursor is kept on Freddy's chest (center) and ducks if cursor shines near shoulders
                Vector2 mp = new Vector2(Input.mousePosition.x / Screen.width * 1280f, (1f - Input.mousePosition.y / Screen.height) * 720f);
                bool cursorOnChest = Vector2.Distance(mp, new Vector2(640f, 470f)) < 140f;
                if (cursorOnChest)
                {
                    _bonBonHeight = Mathf.Clamp01(_bonBonHeight + deltaTime * 0.42f);
                }
                else if (_bonBonHeight < 0.82f)
                {
                    _bonBonHeight = Mathf.Max(0f, _bonBonHeight - deltaTime * 0.9f);
                }
            }
            else if (_night == 5 && _babyHatchOpen && _passcodeStep < _targetPasscode.Count)
            {
                _digitSpeakTimer -= deltaTime;
                if (_digitSpeakTimer <= 0f)
                {
                    _digitSpeakTimer = 3.2f;
                    int digit = _targetPasscode[_passcodeStep];
                    _audio.Speak("Circus Baby", $"{digit}...", VoiceProfileId.CircusBaby, 2.0f, true);
                }
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.03f, 0.04f, 0.06f));

            if (_night == 3)
            {
                RenderNight3FreddyMaintenance(canvas);
            }
            else
            {
                RenderNight5BabyPasscode(canvas);
            }

            canvas.DrawCrtScanlinesAndStatic(0.12f, 0.45f);
        }

        private void RenderNight3FreddyMaintenance(ProceduralCanvas canvas)
        {
            float openAmt = _freddyStep >= 4 ? 0.95f : (_freddyStep * 0.12f);
            canvas.DrawAnimatronicHead("funtime_freddy", 640, 270, 1.65f, openAmt, _freddyStep >= 4 ? 0.55f : 0f);

            // Freddy Torso / Chest Cavity
            canvas.FillRect(510, 410, 260, 240, new Color(0.9f, 0.9f, 0.94f));
            if (_freddyStep >= 5)
            {
                canvas.FillRect(550, 435, 180, 180, new Color(0.14f, 0.15f, 0.18f));
                Color blink = Mathf.PingPong(Time.time * 5f, 1f) > 0.5f ? Color.red : Color.yellow;
                if (_freddyStep == 5 && canvas.Button(610, 495, 60, 60, "PWR", blink, Color.black, 14))
                {
                    _audio.PlaySfx("chime", 0.85f);
                    _freddyStep = 6;
                    _audio.Speak("HandUnit", "Great! Now remove the secondary power module from the Bonnie hand puppet. Keep your beam on Freddy's chest until Bon-Bon peeks over his shoulder, then quickly press the black button under his bowtie.", VoiceProfileId.HandUnit, 8.0f, true);
                }
            }

            // Faceplate & Jaw Maintenance Buttons
            if (_freddyStep == 0)
            {
                // Freddy's Right Cheek = Screen Left (x=510, y=310)
                if (canvas.Button(500, 305, 36, 36, "●", new Color(0.25f, 0.5f, 0.85f), Color.white, 16))
                {
                    _audio.PlaySfx("springlock_wind", 0.9f);
                    _freddyStep = 1;
                    _audio.Speak("HandUnit", "Good. Now press the small button under Freddy's left cheek.", VoiceProfileId.HandUnit, 4.5f, true);
                }
                // Player's Right Cheek = Screen Right (triggers canonical clarification line!)
                if (canvas.Button(744, 305, 36, 36, "●", new Color(0.35f, 0.35f, 0.4f), Color.white, 16))
                {
                    if (!_clarifiedRight)
                    {
                        _clarifiedRight = true;
                        _audio.Speak("HandUnit", "For clarification, please note that I am referring to Freddy's right, not your right.", VoiceProfileId.HandUnit, 5.2f, true);
                    }
                }
            }
            else if (_freddyStep == 1)
            {
                if (canvas.Button(744, 305, 36, 36, "●", new Color(0.25f, 0.5f, 0.85f), Color.white, 16))
                {
                    _audio.PlaySfx("springlock_wind", 0.9f);
                    _freddyStep = 2;
                    _audio.Speak("HandUnit", "Now press the button next to Freddy's right eye.", VoiceProfileId.HandUnit, 4.0f, true);
                }
            }
            else if (_freddyStep == 2)
            {
                if (canvas.Button(535, 225, 34, 34, "●", new Color(0.25f, 0.5f, 0.85f), Color.white, 16))
                {
                    _audio.PlaySfx("springlock_wind", 0.9f);
                    _freddyStep = 3;
                    _audio.Speak("HandUnit", "Now press the button just above Freddy's nose.", VoiceProfileId.HandUnit, 4.0f, true);
                }
            }
            else if (_freddyStep == 3)
            {
                if (canvas.Button(623, 240, 34, 34, "●", new Color(0.25f, 0.5f, 0.85f), Color.white, 16))
                {
                    _audio.PlaySfx("door_slam", 0.8f);
                    _freddyStep = 4;
                    _audio.Speak("HandUnit", "Good job! The faceplates are open. Now press the button on the underside of the endoskeleton jaw to open the chest cavity.", VoiceProfileId.HandUnit, 6.0f, true);
                }
            }
            else if (_freddyStep == 4)
            {
                if (canvas.Button(623, 355, 34, 34, "●", new Color(0.25f, 0.5f, 0.85f), Color.white, 16))
                {
                    _audio.PlaySfx("door_slam", 0.8f);
                    _freddyStep = 5;
                    _audio.Speak("HandUnit", "Excellent. Now take the blinking primary power module from inside the chest cavity.", VoiceProfileId.HandUnit, 5.0f, true);
                }
            }
            else if (_freddyStep == 6)
            {
                // Bon-Bon peeking above left shoulder
                float bx = 450f;
                float by = Mathf.Lerp(430f, 300f, _bonBonHeight);
                canvas.FillRect(bx - 35, by - 40, 70, 80, new Color(0.2f, 0.45f, 0.9f));
                canvas.FillRect(bx - 20, by + 10, 40, 14, Color.red); // Red Bowtie
                if (_bonBonHeight >= 0.65f)
                {
                    if (canvas.Button(bx - 18, by + 26, 36, 28, "BTN", Color.black, Color.white, 12))
                    {
                        _audio.PlaySfx("chime", 1.0f);
                        _freddyStep = 7;
                        _audio.Speak("HandUnit", "Great job! Both power modules retrieved. You may now exit back through Funtime Auditorium.", VoiceProfileId.HandUnit, 5.5f, true);
                    }
                }
            }
            else if (_freddyStep == 7)
            {
                if (canvas.Button(460, 620, 360, 52, "EXIT TO FUNTIME AUDITORIUM", new Color(0.2f, 0.5f, 0.25f), Color.white, 17))
                {
                    _router.TransitionTo(SceneId.FUNTIME_AUDITORIUM, new Dictionary<string, object> { { "night", 3 }, { "phase", "outbound_n3" } });
                }
            }

            canvas.DrawText($"NIGHT 3 PARTS & SERVICE — STEP {_freddyStep + 1}/7", 340, 30, 18, Color.cyan, TextAnchor.UpperCenter, 600, 40);
        }

        private void RenderNight5BabyPasscode(ProceduralCanvas canvas)
        {
            canvas.DrawAnimatronicHead("circus_baby", 640, 290, 1.75f, 0.1f, 0f);
            canvas.FillRect(500, 440, 280, 240, new Color(0.92f, 0.92f, 0.95f));

            if (!_babyHatchOpen)
            {
                canvas.DrawText("CLICK SMALL BUTTON ON BABY'S RIGHT CHEEK (SCREEN LEFT) TO OPEN KEYPAD", 240, 35, 18, Color.yellow, TextAnchor.UpperCenter, 800, 40);
                if (canvas.Button(495, 325, 38, 38, "●", new Color(0.2f, 0.65f, 0.3f), Color.white, 18))
                {
                    _audio.PlaySfx("springlock_wind", 0.9f);
                    _babyHatchOpen = true;
                }
            }
            else if (_passcodeStep < _targetPasscode.Count)
            {
                int currentTarget = _targetPasscode[_passcodeStep];
                canvas.DrawText($"LISTEN TO CIRCUS BABY & ENTER DIGIT ({_passcodeStep + 1}/{_targetPasscode.Count}) — CURRENT SPOKEN DIGIT: [{currentTarget}]", 190, 35, 18, Color.green, TextAnchor.UpperCenter, 900, 40);

                // Cheek 1-9 Keypad Hatch
                canvas.FillRect(320, 240, 180, 200, new Color(0.18f, 0.2f, 0.24f));
                canvas.StrokeRect(320, 240, 180, 200, 3f, Color.cyan);
                for (int d = 1; d <= 9; d++)
                {
                    int col = (d - 1) % 3;
                    int row = (d - 1) / 3;
                    if (canvas.Button(336 + col * 52, 255 + row * 58, 44, 48, d.ToString(), new Color(0.28f, 0.32f, 0.38f), Color.white, 18))
                    {
                        if (d == currentTarget)
                        {
                            _audio.PlaySfx("chime", 0.8f);
                            _passcodeStep++;
                            _digitSpeakTimer = 0.8f;
                            if (_passcodeStep >= _targetPasscode.Count)
                            {
                                _audio.Speak("Circus Baby", "Good. A hatch has opened on my arm. Take the green card that you find inside.", VoiceProfileId.CircusBaby, 5.5f, true);
                            }
                        }
                        else
                        {
                            _router.TriggerJumpscare("ennard", SceneId.PARTS_AND_SERVICE, new Dictionary<string, object> { { "night", 5 } });
                            return;
                        }
                    }
                }
            }
            else if (!_cardTaken)
            {
                if (canvas.Button(460, 490, 110, 52, "TAKE GREEN\nDATA CARD", new Color(0.12f, 0.75f, 0.25f), Color.black, 14))
                {
                    _audio.PlaySfx("chime", 1.0f);
                    _cardTaken = true;
                    _audio.Speak("Circus Baby", "Good. Now press the green button on the far left to send me to the Scooping Room.", VoiceProfileId.CircusBaby, 5.5f, true);
                }
            }
            else
            {
                if (canvas.Button(140, 460, 220, 64, "SEND CONVEYOR TO\nSCOOPING ROOM [GREEN]", new Color(0.15f, 0.7f, 0.25f), Color.white, 16))
                {
                    _audio.PlaySfx("door_slam", 0.9f);
                    _router.TransitionTo(SceneId.FUNTIME_AUDITORIUM, new Dictionary<string, object> { { "night", 5 }, { "phase", "outbound_n5" } });
                }
            }
        }

        public void Exit()
        {
        }
    }
}
