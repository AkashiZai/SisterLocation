using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class SpringlockSuitScene : IGameScene
    {
        public SceneId Id => SceneId.SPRINGLOCK_SUIT;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private float _introTimer = 0f;
        private bool _survivalActive = false;
        private float _survivalRemaining = 180f;
        private readonly float[] _springlocks = new float[10];

        // Climbing lethal Minireenas (left & right edges) + center non-lethal hallucination Minireena
        private float _leftClimberHeight = 0f;  // 0=none, >0 to 1.0=lethal
        private float _rightClimberHeight = 0f;
        private float _centerDropY = -100f;
        private float _wiggleShakeOffset = 0f;
        private float _clickSfxTimer = 0f;

        public SpringlockSuitScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _introTimer = 0f;
            _survivalActive = false;
            _survivalRemaining = _state.State.fastTimerMode ? 90f : 180f;
            for (int i = 0; i < 10; i++) _springlocks[i] = 100f;

            _leftClimberHeight = 0f;
            _rightClimberHeight = 0f;
            _centerDropY = -100f;
            _wiggleShakeOffset = 0f;

            _audio.ClearDialogue();
            _audio.Speak("Technician 1", "Well, I have to be somewhere in 15 minutes, and this place gives me the creeps. Can we just get this over with? Set Ballora on the rollers.", VoiceProfileId.Technician, 6.0f, true);
            _audio.Speak("Circus Baby", "Shh, be still and quiet. You've been sleeping for quite a while. I kidnapped you. You're inside something that came from my old pizzeria. Keep the springlocks wound tight.", VoiceProfileId.CircusBaby, 8.5f, false);
        }

        public void Update(float deltaTime)
        {
            if (!_survivalActive)
            {
                _introTimer += deltaTime;
                if (_introTimer >= 12.5f || Input.GetKeyDown(KeyCode.Space))
                {
                    _survivalActive = true;
                    _audio.PlaySfx("door_slam", 0.9f);
                }
                return;
            }

            _survivalRemaining -= deltaTime;
            if (_survivalRemaining <= 0f)
            {
                _state.State.currentNight = Mathf.Max(_state.State.currentNight, 5);
                _state.Save();
                _router.TransitionTo(SceneId.TV_LIVING_ROOM, new Dictionary<string, object> { { "episode", 3 } });
                return;
            }

            if (_wiggleShakeOffset > 0f)
            {
                _wiggleShakeOffset = Mathf.Max(0f, _wiggleShakeOffset - deltaTime * 35f);
            }

            // Unwind all 10 springlocks
            for (int i = 0; i < 10; i++)
            {
                _springlocks[i] -= 2.35f * deltaTime;
                if (_springlocks[i] <= 0f)
                {
                    _router.TriggerJumpscare("ballora", SceneId.SPRINGLOCK_SUIT);
                    return;
                }
            }

            // Lethal climbing Minireenas on Left & Right
            if (_leftClimberHeight > 0f)
            {
                _leftClimberHeight += 0.14f * deltaTime;
                if (_leftClimberHeight >= 1.0f)
                {
                    _router.TriggerJumpscare("ballora", SceneId.SPRINGLOCK_SUIT);
                    return;
                }
            }
            else if (UnityEngine.Random.value < 0.16f * deltaTime)
            {
                _leftClimberHeight = 0.05f;
            }

            if (_rightClimberHeight > 0f)
            {
                _rightClimberHeight += 0.14f * deltaTime;
                if (_rightClimberHeight >= 1.0f)
                {
                    _router.TriggerJumpscare("ballora", SceneId.SPRINGLOCK_SUIT);
                    return;
                }
            }
            else if (UnityEngine.Random.value < 0.16f * deltaTime)
            {
                _rightClimberHeight = 0.05f;
            }

            // Center non-lethal hallucination Minireena descending
            _centerDropY += 110f * deltaTime;
            if (_centerDropY > 820f) _centerDropY = -120f;

            // Wiggle suit (A or D) to shake off climbing Minireenas (-12% springlock penalty)
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                WiggleSuit();
            }

            if (_clickSfxTimer > 0f) _clickSfxTimer -= deltaTime;
        }

        private void WiggleSuit()
        {
            _audio.PlaySfx("vent_crawl", 0.85f);
            _wiggleShakeOffset = 18f;
            _leftClimberHeight = 0f;
            _rightClimberHeight = 0f;
            for (int i = 0; i < 10; i++)
            {
                _springlocks[i] = Mathf.Max(2f, _springlocks[i] - 12f);
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            float sx = _wiggleShakeOffset > 0f ? UnityEngine.Random.Range(-_wiggleShakeOffset, _wiggleShakeOffset) : 0f;
            canvas.FillRect(0, 0, 1280, 720, new Color(0.04f, 0.03f, 0.04f));

            // Scooping Room view through the suit eyeholes
            if (!_survivalActive)
            {
                float lunge = Mathf.PingPong(_introTimer * 3f, 1f);
                canvas.DrawAnimatronicHead("ballora", 640, 360, 1.3f, lunge * 0.8f);
                canvas.FillRect(610, 120, 60, 180 + lunge * 110f, new Color(0.35f, 0.35f, 0.4f));
                canvas.DrawText("SCOOPING ROOM — TECHNICIANS & SCOOPER DISMANTLING BALLORA (SPACE TO START)", 220, 40, 18, Color.yellow, TextAnchor.UpperCenter, 840, 40);
                return;
            }

            // Center non-lethal Minireena descending (ignore her!)
            if (_centerDropY > -60f && _centerDropY < 720f)
            {
                canvas.FillRect(622, _centerDropY, 36, 85, new Color(0.92f, 0.88f, 0.82f));
                canvas.FillRect(615, _centerDropY - 26, 50, 36, Color.white);
            }

            // Left & Right lethal climbing Minireenas
            if (_leftClimberHeight > 0f)
            {
                float ly = Mathf.Lerp(680f, 160f, _leftClimberHeight);
                canvas.FillRect(220 + sx, ly, 44, 95, new Color(0.95f, 0.85f, 0.7f));
                canvas.DrawText("CLIMBING!\nPRESS [A/D]", 200, ly - 35, 13, Color.red);
            }
            if (_rightClimberHeight > 0f)
            {
                float ry = Mathf.Lerp(680f, 160f, _rightClimberHeight);
                canvas.FillRect(1016 + sx, ry, 44, 95, new Color(0.95f, 0.85f, 0.7f));
                canvas.DrawText("CLIMBING!\nPRESS [A/D]", 996, ry - 35, 13, Color.red);
            }

            // Springlock Suit Opened Faceplate Borders
            canvas.FillRect(0, 0, 175, 720, new Color(0.16f, 0.14f, 0.12f));
            canvas.FillRect(1105, 0, 175, 720, new Color(0.16f, 0.14f, 0.12f));

            // 10 Springlock Dials (5 Left, 5 Right) — Hold Left-Click over dial to wind
            for (int i = 0; i < 10; i++)
            {
                bool leftSide = i < 5;
                int row = i % 5;
                float dx = leftSide ? 45f : 1140f;
                float dy = 110f + row * 105f;
                float val = _springlocks[i];

                Color dialCol = val > 50f ? Color.green :
                                (val > 22f ? Color.yellow :
                                (Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? Color.red : new Color(0.4f, 0f, 0f)));

                if (canvas.IsHovered(dx, dy, 95, 78) && Input.GetMouseButton(0))
                {
                    _springlocks[i] = Mathf.Clamp(_springlocks[i] + 34f * Time.deltaTime, 0f, 100f);
                    if (_clickSfxTimer <= 0f)
                    {
                        _clickSfxTimer = 0.09f;
                        _audio.PlaySfx("springlock_wind", 0.7f);
                    }
                }

                canvas.Button(dx, dy, 95, 78, $"LOCK #{i + 1}\n{Mathf.RoundToInt(val)}%", dialCol, Color.black, 14);
            }

            if (canvas.Button(510, 635, 260, 48, "WIGGLE SUIT [A / D] (-12%)", new Color(0.45f, 0.2f, 0.15f), Color.white, 16))
            {
                WiggleSuit();
            }

            canvas.DrawText($"NIGHT 4 SPRINGLOCK SURVIVAL — TIME REMAINING: {Mathf.CeilToInt(_survivalRemaining)}s\nHOLD CLICK ON DIALS TO WIND | PRESS [A] OR [D] TO SHAKE OFF SIDE MINIREENAS", 200, 28, 17, Color.yellow, TextAnchor.UpperCenter, 880, 55);
            canvas.DrawCrtScanlinesAndStatic(0.14f, 0.4f);
        }

        public void Exit()
        {
        }
    }

    public sealed class ScoopingRoomScene : IGameScene
    {
        public SceneId Id => SceneId.SCOOPING_ROOM;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private float _timer = 0f;
        private int _phase = 0; // 0=Baby monologue, 1=Scooper strike, 2=Mirror purple eyes

        public ScoopingRoomScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _timer = 0f;
            _phase = 0;
            _audio.ClearDialogue();
            _audio.Speak("Circus Baby (Ennard)", "You are in the Scooping Room now. Funtime Foxy has already been here today. Funtime Freddy has already been here today. Ballora has already been here today. Circus Baby has already been here today. Look at us now: Ennard. The Scooper only hurts for a moment.", VoiceProfileId.CircusBaby, 11.5f, true);
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
            if (_phase == 0 && _timer >= 11.8f)
            {
                _phase = 1;
                _audio.PlaySfx("jumpscare", 1.0f);
                _audio.PlaySfx("door_slam", 1.0f);
            }
            else if (_phase == 1 && _timer >= 14.0f)
            {
                _phase = 2;
                _audio.PlaySfx("elevator_hum", 0.6f);
            }
            else if (_phase == 2 && _timer >= 19.5f)
            {
                _state.State.starsUnlocked.star1_realEnding = true;
                _state.Save();
                _router.TransitionTo(SceneId.TITLE_MENU);
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            if (_phase == 0)
            {
                canvas.FillRect(0, 0, 1280, 720, new Color(0.05f, 0.03f, 0.04f));
                // Scattered empty shells on floor
                canvas.DrawAnimatronicHead("funtime_foxy", 260, 560, 0.7f, 0.9f);
                canvas.DrawAnimatronicHead("funtime_freddy", 450, 580, 0.7f, 0.9f);
                canvas.DrawAnimatronicHead("ballora", 820, 570, 0.7f, 0.9f);
                canvas.DrawAnimatronicHead("circus_baby", 1010, 560, 0.7f, 0.9f);

                // Window with Ennard standing behind glass
                canvas.FillRect(380, 110, 520, 320, new Color(0.12f, 0.14f, 0.18f));
                canvas.StrokeRect(380, 110, 520, 320, 6f, new Color(0.32f, 0.35f, 0.4f));
                canvas.DrawAnimatronicHead("ennard", 640, 270, 1.45f, 0.15f);

                // Warning siren light
                if (Mathf.PingPong(Time.time * 4f, 1f) > 0.5f)
                {
                    canvas.FillRect(0, 0, 1280, 720, new Color(0.8f, 0.1f, 0.1f, 0.22f));
                }
            }
            else if (_phase == 1)
            {
                // S.C.U.P. Arm lunging & red blood screen flash
                canvas.FillRect(0, 0, 1280, 720, new Color(0.75f, 0.04f, 0.04f));
                canvas.FillRect(520, 60, 240, 600, new Color(0.15f, 0.15f, 0.18f));
                canvas.DrawText("S.C.U.P. (SCALABLE CREATION OF ULTERIOR PRESENCE) ENGAGED", 240, 340, 26, Color.white, TextAnchor.MiddleCenter, 800, 60);
            }
            else
            {
                // Bathroom Mirror Silhouette Finale — Michael Afton's Eyes Glow Purple
                canvas.FillRect(0, 0, 1280, 720, new Color(0.02f, 0.02f, 0.04f));
                canvas.StrokeRect(390, 90, 500, 540, 8f, new Color(0.22f, 0.25f, 0.32f));
                canvas.FillRect(400, 100, 480, 520, new Color(0.06f, 0.08f, 0.12f));

                // Silhouette head & shoulders
                canvas.FillRect(560, 220, 160, 190, Color.black);
                canvas.FillRect(480, 410, 320, 210, Color.black);

                // Glowing Purple Eyes
                if (_timer >= 15.5f)
                {
                    canvas.FillRect(595, 295, 24, 16, new Color(0.85f, 0.2f, 1.0f));
                    canvas.FillRect(661, 295, 24, 16, new Color(0.85f, 0.2f, 1.0f));
                }

                canvas.DrawText("REAL ENDING UNLOCKED — ★ STAR 1 AWARDED", 340, 35, 24, Color.yellow, TextAnchor.UpperCenter, 600, 40);
            }

            canvas.DrawCrtScanlinesAndStatic(0.18f, 0.45f);
        }

        public void Exit()
        {
        }
    }
}
