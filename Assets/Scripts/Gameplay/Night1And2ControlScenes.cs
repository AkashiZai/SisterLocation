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
        private PrimaryControlRoom3D _room3D;

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

            _room3D ??= new PrimaryControlRoom3D();
            _room3D.SetActive(true);

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

            _room3D?.Sync(deltaTime, _night, _leftLightOn, _rightLightOn, _leftShockCount, _rightShockCount,
                _router.LolbitMaskOverride, _router.RareSpawnActive, _shockFlashTimer);
        }

        public void Render(ProceduralCanvas canvas)
        {
            if (_room3D != null)
            {
                _room3D.View.Draw(canvas);
            }
            else
            {
                canvas.FillRect(0, 0, 1280, 720, new Color(0.05f, 0.06f, 0.09f));
            }

            // Left Window (Ballora Gallery) & Right Window (Funtime Auditorium) status overlays
            RenderLeftWindow(canvas);
            RenderRightWindow(canvas);

            // Center Vent (Circus Gallery) & Overhead Ennard Mask / Fan / Toys hitboxes
            RenderCenterProps(canvas);

            // Left Keypad (Ballora) & Right Keypad (Funtime Foxy) projected onto 3D keypads
            RenderKeypad3D(canvas, "BALLORA GALLERY", true, PrimaryControlRoom3D.LeftLightButton, PrimaryControlRoom3D.LeftShockButton, 60f);
            RenderKeypad3D(canvas, "FUNTIME AUDITORIUM", false, PrimaryControlRoom3D.RightLightButton, PrimaryControlRoom3D.RightShockButton, 1020f);
            RenderPanHints(canvas);

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
                    float vx = 500f, vy = 320f;
                    if (_room3D != null && _room3D.View.ProjectToLogical(new Vector3(0f, 0.57f, 2.9f), out Vector2 vp))
                    {
                        vx = Mathf.Clamp(vp.x - 140f, 40f, 960f);
                        vy = Mathf.Clamp(vp.y - 26f, 80f, 560f);
                    }
                    if (canvas.Button(vx, vy, 280, 52, "ENTER NORTH VENT\n(CIRCUS CONTROL)", new Color(0.18f, 0.35f, 0.55f, 0.9f), Color.cyan, 16))
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
                    if (canvas.Button(120, 620, 300, 50, "ENTER WEST VENT (BALLORA GALLERY)", new Color(0.35f, 0.15f, 0.45f, 0.9f), Color.white, 15))
                    {
                        _router.TransitionTo(SceneId.BALLORA_GALLERY, new Dictionary<string, object> { { "night", _night }, { "inbound", true } });
                    }
                }

                if (_eastVentUnlocked)
                {
                    if (canvas.Button(860, 620, 320, 50, "ENTER EAST VENT (FUNTIME AUDITORIUM)", new Color(0.45f, 0.18f, 0.28f, 0.9f), Color.white, 15))
                    {
                        _router.TransitionTo(SceneId.FUNTIME_AUDITORIUM, new Dictionary<string, object> { { "night", _night }, { "phase", "inbound" } });
                    }
                }
            }

            if (_shockFlashTimer > 0f)
            {
                canvas.FillRect(0, 0, 1280, 720, new Color(0.65f, 0.85f, 1.0f, _shockFlashTimer * 0.65f));
            }

            canvas.DrawCrtScanlinesAndStatic(0.12f, 0.35f);
        }

        private void RenderLeftWindow(ProceduralCanvas canvas)
        {
            if (!_leftLightOn) return;

            Vector2 pos = new Vector2(260f, 340f);
            if (_room3D != null && !_room3D.View.ProjectToLogical(PrimaryControlRoom3D.LeftWindowLabel, out pos))
            {
                return;
            }

            float lx = Mathf.Clamp(pos.x - 160f, 20f, 940f);
            float ly = Mathf.Clamp(pos.y, 60f, 600f);

            if (_night == 1 || _night == 2)
            {
                if (_leftShockCount >= 1)
                {
                    canvas.DrawText("BALLORA & MINIREENAS ONSTAGE", lx, ly, 14, Color.cyan, TextAnchor.MiddleCenter, 320f, 30f);
                }
                else
                {
                    canvas.DrawText("STAGE EMPTY", lx, ly, 18, Color.red, TextAnchor.MiddleCenter, 320f, 30f);
                }
            }
            else if (_night == 3)
            {
                canvas.DrawText("MINIREENAS HOLDING DISMANTLED BALLORA", lx, ly, 13, Color.yellow, TextAnchor.MiddleCenter, 320f, 30f);
            }
            else if (_night == 5)
            {
                canvas.DrawText("SILHOUETTE: HANGED TECHNICIAN #1", lx, ly, 14, Color.red, TextAnchor.MiddleCenter, 320f, 30f);
            }
        }

        private void RenderRightWindow(ProceduralCanvas canvas)
        {
            if (!_rightLightOn) return;

            Vector2 pos = new Vector2(1020f, 340f);
            if (_room3D != null && !_room3D.View.ProjectToLogical(PrimaryControlRoom3D.RightWindowLabel, out pos))
            {
                return;
            }

            float lx = Mathf.Clamp(pos.x - 160f, 20f, 940f);
            float ly = Mathf.Clamp(pos.y, 60f, 600f);

            if (_night == 1)
            {
                if (_rightShockCount >= 2)
                {
                    canvas.DrawText("FUNTIME FOXY ONSTAGE", lx, ly, 14, Color.magenta, TextAnchor.MiddleCenter, 320f, 30f);
                }
                else
                {
                    canvas.DrawText("STAGE EMPTY", lx, ly, 18, Color.red, TextAnchor.MiddleCenter, 320f, 30f);
                }
            }
            else if (_night == 2)
            {
                if (_rightShockCount >= 1)
                {
                    canvas.DrawText("FUNTIME FOXY ONSTAGE", lx, ly, 14, Color.magenta, TextAnchor.MiddleCenter, 320f, 30f);
                }
                else
                {
                    canvas.DrawText("STAGE EMPTY", lx, ly, 18, Color.red, TextAnchor.MiddleCenter, 320f, 30f);
                }
            }
            else if (_night == 3)
            {
                canvas.DrawText("STAGE EMPTY — FOXY MISSING", lx, ly, 16, Color.red, TextAnchor.MiddleCenter, 320f, 30f);
            }
            else if (_night == 5)
            {
                canvas.DrawText("SILHOUETTE: HANGED TECHNICIAN #2", lx, ly, 14, Color.red, TextAnchor.MiddleCenter, 320f, 30f);
            }
        }

        private void RenderCenterProps(ProceduralCanvas canvas)
        {
            // Ennard Mask (or 1/5,000 Lolbit Head Easter Egg) above the center vent
            if (_room3D != null && _room3D.View.ProjectToLogical(PrimaryControlRoom3D.EnnardNose, out Vector2 nosePos))
            {
                if (_router.LolbitMaskOverride)
                {
                    canvas.DrawText("1/5,000 LOLBIT MASK!", nosePos.x - 100f, nosePos.y - 75f, 14, new Color(1f, 0.55f, 0.1f), TextAnchor.MiddleCenter, 200f, 26f);
                }

                // Clickable Ennard nose above fan -> plays Baby1.ogg cry
                if (canvas.Button(nosePos.x - 22f, nosePos.y - 18f, 44f, 36f, "", new Color(1f, 0f, 0f, 0.01f), Color.clear))
                {
                    _audio.PlaySfx("baby_cry", 0.9f);
                }
            }

            // 1/1,000 Spinning Minireena in Primary Control
            if (_router.RareSpawnActive && _room3D != null &&
                _room3D.View.ProjectToLogical(new Vector3(-0.9f, 0.85f, 1.9f), out Vector2 miniPos))
            {
                canvas.DrawText("1/1,000 SPINNING MINIREENA", miniPos.x - 130f, miniPos.y, 13, Color.yellow, TextAnchor.MiddleCenter, 260f, 26f);
            }

            // Three small Baby Heads ("See/Hear/Speak No Evil") on desk
            for (int h = 0; h < PrimaryControlRoom3D.BabyHeads.Length; h++)
            {
                if (_room3D != null && _room3D.View.ProjectToLogical(PrimaryControlRoom3D.BabyHeads[h], out Vector2 headPos))
                {
                    if (canvas.Button(headPos.x - 28f, headPos.y - 28f, 56f, 56f, $"BABY\n#{h + 1}", new Color(0.18f, 0.12f, 0.12f, 0.55f), new Color(1f, 1f, 1f, 0.85f), 11))
                    {
                        _audio.PlaySfx("chime", 0.75f);
                    }
                }
            }
        }

        private void RenderKeypad3D(ProceduralCanvas canvas, string title, bool isLeft, Vector3 lightWorld, Vector3 shockWorld, float fallbackX)
        {
            Vector3 centerWorld = (lightWorld + shockWorld) * 0.5f;
            float x = fallbackX;
            float y = 410f;
            if (_room3D != null)
            {
                if (!_room3D.View.ProjectToLogical(centerWorld, out Vector2 projected))
                {
                    return;
                }
                x = Mathf.Clamp(projected.x - 95f, 16f, 1074f);
                y = Mathf.Clamp(projected.y - 88f, 300f, 510f);
            }

            canvas.FillRect(x, y, 190, 176, new Color(0.10f, 0.12f, 0.16f, 0.82f));
            canvas.StrokeRect(x, y, 190, 176, 2f, new Color(0.35f, 0.42f, 0.52f, 0.9f));
            canvas.DrawText(title, x + 8, y + 8, 13, Color.white, TextAnchor.UpperCenter, 174, 24);

            bool lightActive = isLeft ? _leftLightOn : _rightLightOn;
            if (canvas.Button(x + 20, y + 40, 150, 50, lightActive ? "LIGHT: ON" : "LIGHT [BLUE]", new Color(0.12f, 0.38f, 0.85f, 0.92f), Color.white, 15))
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

            if (canvas.Button(x + 20, y + 104, 150, 50, "SHOCK [RED]", new Color(0.85f, 0.15f, 0.15f, 0.92f), Color.white, 15))
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

        private void RenderPanHints(ProceduralCanvas canvas)
        {
            if (_room3D == null) return;
            float yaw = _room3D.View.Yaw;
            float maxYaw = _room3D.View.MaxYaw;
            if (yaw > -maxYaw + 4f)
            {
                canvas.DrawText("◄ MOUSE LEFT\nBALLORA GALLERY", 14, 320, 13, new Color(0.65f, 0.8f, 1f, 0.65f), TextAnchor.MiddleLeft, 150, 44);
            }
            if (yaw < maxYaw - 4f)
            {
                canvas.DrawText("MOUSE RIGHT ►\nFUNTIME AUDITORIUM", 1116, 320, 13, new Color(1f, 0.7f, 0.9f, 0.65f), TextAnchor.MiddleRight, 150, 44);
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
            _room3D?.SetActive(false);
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
        private CircusControlRoom3D _room3D;

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

            _room3D ??= new CircusControlRoom3D();
            _room3D.SetActive(true);

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
                return;
            }

            _room3D?.Sync(deltaTime, _lightOn, _powerOutage, _router.RareSpawnActive, _shockFlash);
        }

        public void Render(ProceduralCanvas canvas)
        {
            if (_room3D != null)
            {
                _room3D.View.Draw(canvas);
            }
            else
            {
                Color bg = _powerOutage ? new Color(0.01f, 0.01f, 0.02f) : new Color(0.06f, 0.07f, 0.11f);
                canvas.FillRect(0, 0, 1280, 720, bg);
            }

            if (_lightOn && !_powerOutage)
            {
                Vector2 winPos = new Vector2(640f, 210f);
                if (_room3D == null || _room3D.View.ProjectToLogical(CircusControlRoom3D.WindowLabelPos, out winPos))
                {
                    canvas.DrawText("CIRCUS GALLERY STAGE (CIRCUS BABY NOT VISIBLE)", winPos.x - 260f, winPos.y - 15f, 17, Color.yellow, TextAnchor.MiddleCenter, 520f, 30f);
                }
            }
            if (_router.RareSpawnActive)
            {
                Vector2 bidyPos = new Vector2(640f, 290f);
                if (_room3D == null || _room3D.View.ProjectToLogical(new Vector3(0f, 1.45f, 3.18f), out bidyPos))
                {
                    canvas.DrawText("1/1,000 BIDYBAB AT WINDOW", bidyPos.x - 160f, bidyPos.y - 45f, 14, Color.yellow, TextAnchor.MiddleCenter, 320f, 28f);
                }
            }

            // Control Desk with 4 colored eye-buttons (Blue, Green, Pink/Purple, Yellow) projected on the 3D desk
            Color[] eyeCols = { Color.cyan, Color.green, Color.magenta, Color.yellow };
            for (int i = 0; i < 4; i++)
            {
                Vector2 btnPos = new Vector2(342f + i * 115f, 482f);
                if (_room3D == null || _room3D.View.ProjectToLogical(CircusControlRoom3D.EyeButtons[i], out btnPos))
                {
                    Color c = eyeCols[i];
                    if (canvas.Button(btnPos.x - 36f, btnPos.y - 36f, 72f, 72f, "◉", new Color(c.r, c.g, c.b, 0.45f), Color.white, 24))
                    {
                        _audio.PlaySfx("chime", 0.7f);
                    }
                }
            }

            // Right Keypad projected on the 3D right stand
            float kx = 850f, ky = 390f;
            bool showKeypad = true;
            if (_room3D != null)
            {
                showKeypad = _room3D.View.ProjectToLogical(CircusControlRoom3D.KeypadCenter, out Vector2 kp);
                if (showKeypad)
                {
                    kx = Mathf.Clamp(kp.x - 95f, 20f, 1060f);
                    ky = Mathf.Clamp(kp.y - 88f, 280f, 510f);
                }
            }
            if (showKeypad)
            {
                canvas.FillRect(kx, ky, 195, 176, new Color(0.10f, 0.12f, 0.16f, 0.82f));
                canvas.StrokeRect(kx, ky, 195, 176, 2f, new Color(0.35f, 0.42f, 0.52f, 0.9f));
                if (canvas.Button(kx + 18, ky + 20, 160, 55, _lightOn ? "LIGHT: ON" : "LIGHT [BLUE]", new Color(0.12f, 0.38f, 0.85f, 0.92f), Color.white, 16))
                {
                    _lightOn = !_lightOn;
                }
                if (canvas.Button(kx + 18, ky + 92, 160, 55, $"SHOCK ({_shockCount}/3)", new Color(0.85f, 0.15f, 0.15f, 0.92f), Color.white, 16))
                {
                    HandleShockPressed();
                }
            }

            if (_night1Complete)
            {
                if (canvas.Button(460, 610, 360, 56, "CRAWL BACK [S] -> FINISH NIGHT 1", new Color(0.15f, 0.5f, 0.25f, 0.92f), Color.white, 18))
                {
                    CompleteNight1();
                }
            }

            if (_powerOutage || _night == 3)
            {
                float ux = 460f, uy = 600f;
                if (_room3D != null && _room3D.View.ProjectToLogical(CircusControlRoom3D.UnderDeskPos, out Vector2 up))
                {
                    ux = Mathf.Clamp(up.x - 180f, 60f, 860f);
                    uy = Mathf.Clamp(up.y - 28f, 420f, 610f);
                }
                if (canvas.Button(ux, uy, 360, 56, "HIDE UNDER THE DESK", new Color(0.55f, 0.22f, 0.12f, 0.92f), Color.yellow, 18))
                {
                    _router.TransitionTo(SceneId.UNDER_DESK, new Dictionary<string, object> { { "night", _night } });
                }
            }

            if (_shockFlash > 0f)
            {
                canvas.FillRect(0, 0, 1280, 720, new Color(0.7f, 0.9f, 1.0f, _shockFlash * 0.65f));
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
            _room3D?.SetActive(false);
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
