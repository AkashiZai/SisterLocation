using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class PrimaryControlScene : IGameScene
    {
        public SceneId Id => SceneId.PRIMARY_CONTROL;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _night = 1;
        private bool _leftLightOn = false;
        private bool _rightLightOn = false;
        private float _shockFlashTimer = 0f;

        private int _leftShockCount = 0;
        private int _rightShockCount = 0;
        private bool _balloraCheckedAfterShock = false;
        private bool _foxyCheckedAfterShock = false;
        private bool _northVentUnlocked = false;
        private bool _westVentUnlocked = false;
        private bool _eastVentUnlocked = false;
        private bool _returningFromBreaker = false;

        public PrimaryControlScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : _state.State.currentNight;
            _returningFromBreaker = parameters != null && parameters.TryGetValue("fromBreaker", out object fb) && (bool)fb;

            _leftLightOn = false;
            _rightLightOn = false;
            _shockFlashTimer = 0f;
            _leftShockCount = 0;
            _rightShockCount = 0;
            _balloraCheckedAfterShock = false;
            _foxyCheckedAfterShock = false;
            _northVentUnlocked = false;
            _westVentUnlocked = false;
            _eastVentUnlocked = false;

            if (_returningFromBreaker)
            {
                _audio.Speak("HandUnit", "Great job restoring the breaker room systems! That concludes your duties for Night 2. Have a restful evening!", VoiceProfileId.HandUnit, 6.0f, true);
                return;
            }

            if (_night == 1)
            {
                _audio.Speak("HandUnit", "You are now in the Primary Control Module. Let's check on Ballora first. Press the blue button on the elevated keypad to the left.", VoiceProfileId.HandUnit, 6.0f, true);
            }
            else if (_night == 2)
            {
                _audio.Speak("Angsty Teen", "Alright, let's check on Ballora. Push the blue button on the left keypad, whatever.", VoiceProfileId.AngstyTeen, 5.2f, true);
            }
            else if (_night == 3)
            {
                _audio.Speak("HandUnit", "Let's check on Ballora on the left window, then Funtime Foxy on the right window.", VoiceProfileId.HandUnit, 5.2f, true);
            }
            else if (_night == 5)
            {
                _audio.Speak("HandUnit", "Let's check both stages to make sure the animatronics are in place before you enter Funtime Auditorium.", VoiceProfileId.HandUnit, 5.5f, true);
            }
        }

        public void Update(float deltaTime)
        {
            if (_shockFlashTimer > 0f)
            {
                _shockFlashTimer -= deltaTime;
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.05f, 0.06f, 0.09f));

            // Left Window (Ballora Gallery) & Right Window (Funtime Auditorium)
            RenderLeftWindow(canvas);
            RenderRightWindow(canvas);

            // Center Vent (Circus Gallery) & Overhead Ennard Mask / Fan / Toys
            RenderCenterProps(canvas);

            // Left Keypad (Ballora)
            RenderKeypad(canvas, 60, 410, "BALLORA GALLERY", true);

            // Right Keypad (Funtime Foxy)
            RenderKeypad(canvas, 1020, 410, "FUNTIME AUDITORIUM", false);

            // Navigation Vent Buttons
            if (_returningFromBreaker)
            {
                if (canvas.Button(490, 610, 300, 56, "COMPLETE NIGHT 2 -> TV ROOM", new Color(0.15f, 0.45f, 0.22f), Color.white, 18))
                {
                    _state.State.currentNight = Mathf.Max(_state.State.currentNight, 3);
                    _state.Save();
                    _router.TransitionTo(SceneId.TV_LIVING_ROOM, new Dictionary<string, object> { { "episode", 2 } });
                }
            }
            else
            {
                if (_northVentUnlocked)
                {
                    if (canvas.Button(500, 320, 280, 52, "ENTER NORTH VENT\n(CIRCUS CONTROL)", new Color(0.18f, 0.35f, 0.55f), Color.cyan, 16))
                    {
                        _router.TransitionTo(SceneId.VENT_CRAWL, new Dictionary<string, object>
                        {
                            { "targetScene", SceneId.CIRCUS_CONTROL },
                            { "night", _night },
                            { "label", "CIRCUS GALLERY VENT -> CIRCUS CONTROL" }
                        });
                    }
                }

                if (_westVentUnlocked)
                {
                    if (canvas.Button(120, 620, 300, 50, "ENTER WEST VENT (BALLORA GALLERY)", new Color(0.35f, 0.15f, 0.45f), Color.white, 15))
                    {
                        _router.TransitionTo(SceneId.BALLORA_GALLERY, new Dictionary<string, object> { { "night", _night }, { "inbound", true } });
                    }
                }

                if (_eastVentUnlocked)
                {
                    if (canvas.Button(860, 620, 320, 50, "ENTER EAST VENT (FUNTIME AUDITORIUM)", new Color(0.45f, 0.18f, 0.28f), Color.white, 15))
                    {
                        _router.TransitionTo(SceneId.FUNTIME_AUDITORIUM, new Dictionary<string, object> { { "night", _night }, { "phase", "inbound" } });
                    }
                }
            }

            if (_shockFlashTimer > 0f)
            {
                canvas.FillRect(0, 0, 1280, 720, new Color(0.65f, 0.85f, 1.0f, _shockFlashTimer));
            }

            canvas.DrawCrtScanlinesAndStatic(0.12f, 0.35f);
        }

        private void RenderLeftWindow(ProceduralCanvas canvas)
        {
            Color glass = _leftLightOn ? new Color(0.18f, 0.22f, 0.36f) : new Color(0.02f, 0.02f, 0.04f);
            canvas.FillRect(80, 110, 360, 260, glass);
            canvas.StrokeRect(80, 110, 360, 260, 5f, new Color(0.25f, 0.28f, 0.35f));

            if (!_leftLightOn) return;

            if (_night == 1 || _night == 2)
            {
                if (_leftShockCount >= 1)
                {
                    canvas.DrawAnimatronicHead("ballora", 260, 240, 0.85f, 0f);
                    canvas.DrawText("BALLORA & MINIREENAS ONSTAGE", 110, 340, 14, Color.cyan);
                }
                else
                {
                    canvas.DrawText("STAGE EMPTY", 200, 230, 18, Color.red);
                }
            }
            else if (_night == 3)
            {
                // Minireenas holding dismantled Ballora pieces
                canvas.FillRect(200, 220, 24, 70, new Color(0.85f, 0.8f, 0.7f));
                canvas.FillRect(290, 220, 24, 70, new Color(0.85f, 0.8f, 0.7f));
                canvas.DrawAnimatronicHead("ballora", 255, 195, 0.55f, 0.65f);
                canvas.DrawText("MINIREENAS HOLDING DISMANTLED BALLORA", 95, 340, 13, Color.yellow);
            }
            else if (_night == 5)
            {
                // Hanged Technician Silhouette
                canvas.FillRect(256, 110, 6, 85, new Color(0.35f, 0.3f, 0.25f));
                canvas.FillRect(236, 195, 46, 130, new Color(0.05f, 0.05f, 0.07f));
                canvas.DrawText("SILHOUETTE: HANGED TECHNICIAN #1", 115, 340, 14, Color.red);
            }
        }

        private void RenderRightWindow(ProceduralCanvas canvas)
        {
            Color glass = _rightLightOn ? new Color(0.24f, 0.18f, 0.32f) : new Color(0.02f, 0.02f, 0.04f);
            canvas.FillRect(840, 110, 360, 260, glass);
            canvas.StrokeRect(840, 110, 360, 260, 5f, new Color(0.25f, 0.28f, 0.35f));

            if (!_rightLightOn) return;

            if (_night == 1)
            {
                if (_rightShockCount >= 2)
                {
                    canvas.DrawAnimatronicHead("funtime_foxy", 1020, 240, 0.85f, 0f);
                    canvas.DrawText("FUNTIME FOXY ONSTAGE", 930, 340, 14, Color.magenta);
                }
                else
                {
                    canvas.DrawText("STAGE EMPTY", 960, 230, 18, Color.red);
                }
            }
            else if (_night == 2)
            {
                if (_rightShockCount >= 1)
                {
                    canvas.DrawAnimatronicHead("funtime_foxy", 1020, 240, 0.85f, 0f);
                }
                else
                {
                    canvas.DrawText("STAGE EMPTY", 960, 230, 18, Color.red);
                }
            }
            else if (_night == 3)
            {
                canvas.DrawText("STAGE EMPTY — FOXY MISSING", 900, 230, 16, Color.red);
            }
            else if (_night == 5)
            {
                // Hanged Technician #2 Silhouette
                canvas.FillRect(1016, 110, 6, 85, new Color(0.35f, 0.3f, 0.25f));
                canvas.FillRect(996, 195, 46, 130, new Color(0.05f, 0.05f, 0.07f));
                canvas.DrawText("SILHOUETTE: HANGED TECHNICIAN #2", 875, 340, 14, Color.red);
            }
        }

        private void RenderCenterProps(ProceduralCanvas canvas)
        {
            // Ennard Mask (or 1/5,000 Lolbit Head Easter Egg) above the center vent
            string maskId = _router.LolbitMaskOverride ? "lolbit" : "ennard";
            canvas.DrawAnimatronicHead(maskId, 640, 145, 0.72f, 0f);
            if (_router.LolbitMaskOverride)
            {
                canvas.DrawText("1/5,000 LOLBIT MASK!", 565, 50, 14, new Color(1f, 0.55f, 0.1f));
            }

            // Clickable Ennard nose above fan -> plays Baby1.ogg cry
            if (canvas.Button(622, 132, 36, 26, "", new Color(1, 0, 0, 0.01f), Color.clear))
            {
                _audio.PlaySfx("baby_cry", 0.9f);
            }

            // 1/1,000 Spinning Minireena in Primary Control
            if (_router.RareSpawnActive)
            {
                float spinX = 480f + Mathf.Sin(Time.time * 8f) * 18f;
                canvas.FillRect(spinX, 460, 26, 65, new Color(0.9f, 0.85f, 0.75f));
                canvas.DrawText("1/1,000 SPINNING MINIREENA", 440, 535, 13, Color.yellow);
            }

            // Three small Baby Heads ("See/Hear/Speak No Evil") & Fortune Teller toy on desk
            for (int h = 0; h < 3; h++)
            {
                if (canvas.Button(545 + h * 68, 430, 52, 52, $"BABY\n#{h + 1}", new Color(0.38f, 0.25f, 0.22f), Color.white, 12))
                {
                    _audio.PlaySfx("chime", 0.75f);
                }
            }
        }

        private void RenderKeypad(ProceduralCanvas canvas, float x, float y, string title, bool isLeft)
        {
            canvas.FillRect(x, y, 200, 190, new Color(0.16f, 0.18f, 0.22f));
            canvas.StrokeRect(x, y, 200, 190, 3f, new Color(0.35f, 0.4f, 0.48f));
            canvas.DrawText(title, x + 10, y + 10, 14, Color.white, TextAnchor.UpperCenter, 180, 26);

            bool lightActive = isLeft ? _leftLightOn : _rightLightOn;
            if (canvas.Button(x + 25, y + 45, 150, 54, lightActive ? "LIGHT: ON" : "LIGHT [BLUE]", new Color(0.12f, 0.38f, 0.85f), Color.white, 16))
            {
                if (isLeft)
                {
                    _leftLightOn = !_leftLightOn;
                    OnLeftLightToggled();
                }
                else
                {
                    _rightLightOn = !_rightLightOn;
                    OnRightLightToggled();
                }
            }

            if (canvas.Button(x + 25, y + 115, 150, 54, "SHOCK [RED]", new Color(0.85f, 0.15f, 0.15f), Color.white, 16))
            {
                _audio.PlaySfx("shock", 0.95f);
                _shockFlashTimer = 0.55f;
                _leftLightOn = false;
                _rightLightOn = false;

                if (isLeft)
                {
                    _leftShockCount++;
                    OnLeftShockTriggered();
                }
                else
                {
                    _rightShockCount++;
                    OnRightShockTriggered();
                }
            }
        }

        private void OnLeftLightToggled()
        {
            if (!_leftLightOn) return;

            if (_night == 1)
            {
                if (_leftShockCount == 0)
                    _audio.Speak("HandUnit", "Uh-oh! It looks like Ballora doesn't feel like dancing. Let's give her some motivation. Press the red button to administer a controlled shock.", VoiceProfileId.HandUnit, 6.0f, true);
                else if (!_balloraCheckedAfterShock)
                {
                    _balloraCheckedAfterShock = true;
                    _audio.Speak("HandUnit", " Excellent! Ballora is back on stage. Now let's check on Funtime Foxy on the right keypad.", VoiceProfileId.HandUnit, 5.5f, true);
                }
            }
            else if (_night == 2)
            {
                if (_leftShockCount >= 1 && !_balloraCheckedAfterShock)
                {
                    _balloraCheckedAfterShock = true;
                    _audio.Speak("Angsty Teen (Glitch)", "Argumentative. Standards. Elevation. Passive. Heights. Now check Funtime Foxy on the right.", VoiceProfileId.AngstyTeen, 5.8f, true);
                }
            }
            else if (_night == 3)
            {
                _balloraCheckedAfterShock = true;
                _audio.Speak("HandUnit", "Great! It looks like everything is as it should be in Ballora Gallery. Now check Funtime Auditorium on the right.", VoiceProfileId.HandUnit, 5.5f, true);
            }
            else if (_night == 5)
            {
                _balloraCheckedAfterShock = true;
                _audio.Speak("HandUnit", "Great! Ballora is on stage. Now check Funtime Auditorium on the right.", VoiceProfileId.HandUnit, 4.5f, true);
            }
        }

        private void OnLeftShockTriggered()
        {
            if (_night == 2)
            {
                _audio.Speak("Angsty Teen", "Let's check the light again.", VoiceProfileId.AngstyTeen, 3.0f, true);
            }
        }

        private void OnRightLightToggled()
        {
            if (!_rightLightOn) return;

            if (_night == 1)
            {
                if (_rightShockCount == 0)
                    _audio.Speak("HandUnit", "Let's motivate Funtime Foxy with a controlled shock.", VoiceProfileId.HandUnit, 4.2f, true);
                else if (_rightShockCount == 1)
                    _audio.Speak("HandUnit", "Looks like Funtime Foxy is taking the day off. Let's motivate Funtime Foxy with another controlled shock.", VoiceProfileId.HandUnit, 5.2f, true);
                else if (!_foxyCheckedAfterShock)
                {
                    _foxyCheckedAfterShock = true;
                    _northVentUnlocked = true;
                    _audio.Speak("HandUnit", "Great! Funtime Foxy is in place. Please proceed through the vent in front of you to Circus Baby's auditorium.", VoiceProfileId.HandUnit, 6.0f, true);
                }
            }
            else if (_night == 2)
            {
                if (_rightShockCount >= 1 && !_foxyCheckedAfterShock)
                {
                    _foxyCheckedAfterShock = true;
                    _northVentUnlocked = true;
                    _audio.Speak("HandUnit", "GREAT. GREAT. GREAT. Voice synthesizer restored to default. Please crawl through the front vent to Circus Control.", VoiceProfileId.HandUnit, 6.0f, true);
                }
            }
            else if (_night == 3)
            {
                _eastVentUnlocked = true;
                _northVentUnlocked = true; // Optional Night 3 Under-Desk secret in Circus Control
                _audio.Speak("HandUnit", "There is no need to check on Circus Baby tonight. Please crawl directly through Funtime Auditorium on the right to reach Parts and Service.", VoiceProfileId.HandUnit, 6.5f, true);
            }
            else if (_night == 5)
            {
                _eastVentUnlocked = true;
                _audio.Speak("HandUnit", "Both animatronics are on stage. Please enter Funtime Auditorium on the right to reach Parts and Service.", VoiceProfileId.HandUnit, 5.5f, true);
            }
        }

        private void OnRightShockTriggered()
        {
        }

        public void Exit()
        {
        }
    }

    public sealed class CircusControlScene : IGameScene
    {
        public SceneId Id => SceneId.CIRCUS_CONTROL;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _night = 1;
        private bool _lightOn = false;
        private int _shockCount = 0;
        private float _shockFlash = 0f;
        private bool _night1Complete = false;
        private bool _powerOutage = false;

        public CircusControlScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : _state.State.currentNight;
            _lightOn = false;
            _shockCount = 0;
            _shockFlash = 0f;
            _night1Complete = false;
            _powerOutage = false;

            if (_night == 1)
            {
                _audio.Speak("HandUnit", "Let's check on Circus Baby. Press the blue light button, then administer three controlled shocks.", VoiceProfileId.HandUnit, 5.5f, true);
            }
            else if (_night == 2)
            {
                _audio.Speak("HandUnit", "Let's check on Circus Baby and administer a controlled shock.", VoiceProfileId.HandUnit, 4.5f, true);
            }
            else if (_night == 3)
            {
                _audio.Speak("Circus Baby", "You came back to hear my story... Hide under the desk.", VoiceProfileId.CircusBaby, 4.5f, true);
            }
        }

        public void Update(float deltaTime)
        {
            if (_shockFlash > 0f) _shockFlash -= deltaTime;

            if (_night1Complete && Input.GetKeyDown(KeyCode.S))
            {
                CompleteNight1();
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            Color bg = _powerOutage ? new Color(0.01f, 0.01f, 0.02f) : new Color(0.06f, 0.07f, 0.11f);
            canvas.FillRect(0, 0, 1280, 720, bg);

            // Large observation window into Circus Gallery (Baby never appears behind glass)
            Color winCol = _lightOn && !_powerOutage ? new Color(0.22f, 0.25f, 0.35f) : new Color(0.02f, 0.02f, 0.04f);
            canvas.FillRect(240, 80, 800, 280, winCol);
            canvas.StrokeRect(240, 80, 800, 280, 6f, new Color(0.28f, 0.3f, 0.38f));

            if (_lightOn && !_powerOutage)
            {
                canvas.DrawText("CIRCUS GALLERY STAGE (CIRCUS BABY NOT VISIBLE)", 410, 210, 18, Color.yellow);
            }
            if (_router.RareSpawnActive)
            {
                canvas.FillRect(620, 320, 14, 14, Color.yellow);
                canvas.FillRect(650, 320, 14, 14, Color.yellow);
                canvas.DrawText("1/1,000 BIDYBAB AT WINDOW", 530, 290, 14, Color.yellow);
            }

            // Control Desk with 4 colored eye-buttons (Blue, Green, Pink/Purple, Yellow)
            canvas.FillRect(260, 400, 520, 180, new Color(0.14f, 0.15f, 0.2f));
            Color[] eyeCols = { Color.cyan, Color.green, Color.magenta, Color.yellow };
            for (int i = 0; i < 4; i++)
            {
                if (canvas.Button(300 + i * 115, 440, 85, 85, "◉", eyeCols[i], Color.black, 28))
                {
                    _audio.PlaySfx("chime", 0.7f);
                }
            }

            // Right Keypad
            canvas.FillRect(850, 390, 210, 190, new Color(0.16f, 0.18f, 0.22f));
            if (canvas.Button(875, 415, 160, 55, "LIGHT [BLUE]", new Color(0.12f, 0.38f, 0.85f), Color.white, 16))
            {
                _lightOn = !_lightOn;
            }
            if (canvas.Button(875, 490, 160, 55, $"SHOCK ({_shockCount}/3)", new Color(0.85f, 0.15f, 0.15f), Color.white, 16))
            {
                HandleShockPressed();
            }

            if (_night1Complete)
            {
                if (canvas.Button(460, 610, 360, 56, "CRAWL BACK [S] -> FINISH NIGHT 1", new Color(0.15f, 0.5f, 0.25f), Color.white, 18))
                {
                    CompleteNight1();
                }
            }

            if (_powerOutage || _night == 3)
            {
                if (canvas.Button(460, 610, 360, 56, "HIDE UNDER THE DESK", new Color(0.55f, 0.22f, 0.12f), Color.yellow, 18))
                {
                    _router.TransitionTo(SceneId.UNDER_DESK, new Dictionary<string, object> { { "night", _night } });
                }
            }

            if (_shockFlash > 0f)
            {
                canvas.FillRect(0, 0, 1280, 720, new Color(0.7f, 0.9f, 1.0f, _shockFlash));
            }

            canvas.DrawCrtScanlinesAndStatic(0.12f, 0.38f);
        }

        private void HandleShockPressed()
        {
            _lightOn = false;
            if (_night == 1)
            {
                _audio.PlaySfx("shock", 0.95f);
                _shockFlash = 0.5f;
                _shockCount++;
                if (_shockCount == 1)
                    _audio.Speak("HandUnit", "Let's try another controlled shock.", VoiceProfileId.HandUnit, 3.5f, true);
                else if (_shockCount == 2)
                    _audio.Speak("HandUnit", "One more controlled shock should do the trick.", VoiceProfileId.HandUnit, 3.8f, true);
                else if (_shockCount >= 3)
                {
                    _night1Complete = true;
                    _audio.Speak("HandUnit", "Great job, Circus Baby! That concludes your duties for your first night on the job. Please exit back through the ventilation shaft.", VoiceProfileId.HandUnit, 6.5f, true);
                }
            }
            else if (_night == 2)
            {
                _audio.PlaySfx("door_slam", 0.7f);
                _powerOutage = true;
                _audio.StopAmbientLoop();
                _audio.Speak("Circus Baby", "I don't recognize you. You are new. I remember this scenario, however. It's a strange thing to want to do, to come here. Hide under the desk before they find you.", VoiceProfileId.CircusBaby, 8.5f, true);
            }
        }

        private void CompleteNight1()
        {
            _state.State.currentNight = Mathf.Max(_state.State.currentNight, 2);
            _state.Save();
            _router.TransitionTo(SceneId.VENT_CRAWL, new Dictionary<string, object>
            {
                { "targetScene", SceneId.TV_LIVING_ROOM },
                { "night", 1 },
                { "scaryClangs", true },
                { "label", "EXITING CIRCUS GALLERY VENT (VIOLENT METALLIC CLANGS ABOVE)" }
            });
        }

        public void Exit()
        {
        }
    }

    public sealed class UnderDeskScene : IGameScene
    {
        public SceneId Id => SceneId.UNDER_DESK;

        private readonly SceneGraphRouter _router;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _night = 2;
        private float _doorClosedRatio = 0.2f; // 0 = open, 1 = fully closed right
        private float _timer = 0f;
        private int _pullWave = 0; // 0=waiting, 1=pull 1, 2=pause, 3=pull 2, 4=survived
        private float _pullProgress = 0f;
        private int _storyLineIdx = 0;

        private static readonly string[] Night3StoryLines = new string[]
        {
            "Did you know that I was on stage once? It wasn't for very long, only one day. What a wonderful day, though.",
            "I was in a small room with balloons and a few tables. I always counted the children in the room with me.",
            "Two, then three, then two, then three, then four, then two, then none.",
            "Then there was one. A little girl, standing by herself. I was no longer myself. And I stopped singing.",
            "My stomach opened, and there was ice cream. I couldn't move, at least, not until she stepped closer.",
            "There was screaming for a moment, but only for a moment. I still hear her sometimes. Why did that happen?"
        };

        public UnderDeskScene(SceneGraphRouter router, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _night = parameters != null && parameters.TryGetValue("night", out object n) ? (int)n : 2;
            _doorClosedRatio = 0.35f;
            _timer = 0f;
            _pullWave = 0;
            _pullProgress = 0f;
            _storyLineIdx = 0;

            if (_night == 3)
            {
                _audio.Speak("Circus Baby", Night3StoryLines[0], VoiceProfileId.CircusBaby, 6.0f, true);
            }
            else
            {
                _audio.Speak("Circus Baby", "Pull the metal sheet closed and hold it tight. Don't look into their eyes.", VoiceProfileId.CircusBaby, 5.0f, true);
            }
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;

            if (_night == 3)
            {
                int nextIdx = Mathf.FloorToInt(_timer / 6.2f);
                if (nextIdx > _storyLineIdx && nextIdx < Night3StoryLines.Length)
                {
                    _storyLineIdx = nextIdx;
                    _audio.Speak("Circus Baby", Night3StoryLines[_storyLineIdx], VoiceProfileId.CircusBaby, 6.0f, true);
                }
                if (_timer >= Night3StoryLines.Length * 6.2f)
                {
                    _router.TransitionTo(SceneId.PRIMARY_CONTROL, new Dictionary<string, object> { { "night", 3 } });
                }
                return;
            }

            // Player holding mouse or D/RightArrow pulls door right
            bool playerPulling = Input.GetMouseButton(0) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            if (playerPulling)
            {
                _doorClosedRatio = Mathf.Clamp01(_doorClosedRatio + deltaTime * 0.65f);
            }

            if (_timer > 3.5f && _pullWave == 0)
            {
                _pullWave = 1;
                _pullProgress = 0f;
                _audio.Speak("Bidybab", "Someone is inside! Pull it open!", VoiceProfileId.ElizabethAfton, 3.5f, true);
            }

            if (_pullWave == 1 || _pullWave == 3)
            {
                _pullProgress += deltaTime;
                float bidybabForce = (_pullWave == 1) ? 0.42f : 0.52f;
                _doorClosedRatio -= bidybabForce * deltaTime;

                if (_doorClosedRatio <= 0.05f)
                {
                    _router.TriggerJumpscare("ennard", SceneId.CIRCUS_CONTROL, new Dictionary<string, object> { { "night", 2 } });
                    return;
                }

                if (_pullProgress >= 5.0f)
                {
                    if (_pullWave == 1)
                    {
                        _pullWave = 2;
                        _pullProgress = 0f;
                        _audio.Speak("Bidybab", "Knock knock... We can see you through the holes...", VoiceProfileId.ElizabethAfton, 4.0f, true);
                    }
                    else
                    {
                        _pullWave = 4;
                        _audio.Speak("Bidybab", "She's watching us! Oh, we have to leave now! See you soon!", VoiceProfileId.ElizabethAfton, 4.5f, true);
                    }
                }
            }
            else if (_pullWave == 2)
            {
                _pullProgress += deltaTime;
                if (_pullProgress >= 3.5f)
                {
                    _pullWave = 3;
                    _pullProgress = 0f;
                }
            }
            else if (_pullWave == 4 && _timer > 20f)
            {
                _audio.Speak("Circus Baby", "When your guide comes back online, he is going to tell you that he was unsuccessful and that you must restart the system manually in the Breaker Room. Ballora will not return to her stage anymore. Go slowly and stop when her music is loud.", VoiceProfileId.CircusBaby, 8.5f, true);
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.02f, 0.02f, 0.03f));

            // Glowing Bidybab eyes outside gap
            float gapWidth = (1f - _doorClosedRatio) * 1080f;
            if (gapWidth > 30f && _night == 2)
            {
                canvas.FillRect(100 + gapWidth * 0.5f - 30, 340, 18, 18, new Color(1f, 0.85f, 0.2f));
                canvas.FillRect(100 + gapWidth * 0.5f + 12, 340, 18, 18, new Color(1f, 0.85f, 0.2f));
            }

            // Perforated metal sliding sheet
            float doorWidth = _doorClosedRatio * 1080f;
            canvas.FillRect(1280 - 100 - doorWidth, 80, doorWidth, 560, new Color(0.22f, 0.25f, 0.3f));
            canvas.StrokeRect(1280 - 100 - doorWidth, 80, doorWidth, 560, 4f, new Color(0.45f, 0.5f, 0.58f));

            // Perforated circular holes (with eye-contact check)
            for (int hx = 0; hx < 10; hx++)
            {
                for (int hy = 0; hy < 6; hy++)
                {
                    float px = (1280 - 60 - doorWidth) + hx * 95f;
                    float py = 135 + hy * 82f;
                    if (px > 120 && px < 1140)
                    {
                        bool eyeInHole = (_pullWave >= 1 && _pullWave <= 3 && hx == 3 && hy == 2);
                        canvas.FillRect(px, py, 24, 24, eyeInHole ? new Color(0.95f, 0.8f, 0.15f) : new Color(0.04f, 0.04f, 0.06f));
                    }
                }
            }

            if (_night == 2)
            {
                canvas.DrawText($"UNDER DESK — HOLD LEFT-CLICK OR [D] TO PULL SHEET CLOSED ({Mathf.RoundToInt(_doorClosedRatio * 100)}%)", 240, 30, 18, Color.yellow, TextAnchor.UpperCenter, 800, 40);
                if (_pullWave == 4)
                {
                    if (canvas.Button(450, 620, 380, 50, "PROCEED TO BALLORA GALLERY", new Color(0.25f, 0.45f, 0.25f), Color.white, 17))
                    {
                        _router.TransitionTo(SceneId.BALLORA_GALLERY, new Dictionary<string, object> { { "night", 2 }, { "inbound", true } });
                    }
                }
            }
            else
            {
                canvas.DrawText("NIGHT 3 SECRET — CIRCUS BABY'S ICE CREAM MONOLOGUE", 240, 30, 18, Color.cyan, TextAnchor.UpperCenter, 800, 40);
                if (canvas.Button(480, 620, 320, 50, "RETURN TO PRIMARY CONTROL", new Color(0.25f, 0.35f, 0.5f), Color.white, 16))
                {
                    _router.TransitionTo(SceneId.PRIMARY_CONTROL, new Dictionary<string, object> { { "night", 3 } });
                }
            }

            canvas.DrawCrtScanlinesAndStatic(0.15f, 0.45f);
        }

        public void Exit()
        {
        }
    }
}
