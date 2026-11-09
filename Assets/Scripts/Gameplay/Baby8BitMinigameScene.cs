using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class Baby8BitMinigameScene : IGameScene
    {
        public SceneId Id => SceneId.BABY_8BIT_MINIGAME;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private struct ChildNpc
        {
            public float WorldX;
            public float Y;
            public int HitsNeeded;
            public bool Fed;
        }

        private struct CupcakePickup
        {
            public float WorldX;
            public float Y;
            public int Type; // 1=Pink(10 uses, single), 2=Blue(4 uses, 3-way), 3=Green(3 uses, piercing)
            public bool Collected;
        }

        private struct Projectile
        {
            public float WorldX;
            public float Y;
            public float Vx;
            public float Vy;
            public bool Piercing;
            public bool Active;
        }

        private float _playerX;
        private float _playerY;
        private float _playerVy;
        private bool _grounded;
        private float _timeRemaining;

        private int _cupcakeType;
        private int _cupcakeAmmo;
        private bool _iceCreamSpawned;
        private bool _hasIceCream;

        private bool _elizabethCutsceneActive;
        private float _elizabethX;
        private float _cutsceneTimer;
        private bool _clawExtended;

        private readonly List<ChildNpc> _children = new List<ChildNpc>();
        private readonly List<CupcakePickup> _pickups = new List<CupcakePickup>();
        private readonly List<Projectile> _projectiles = new List<Projectile>();

        private static readonly Vector2[] PitRanges = new Vector2[]
        {
            new Vector2(680f, 790f),
            new Vector2(1480f, 1600f),
            new Vector2(2280f, 2400f)
        };

        public Baby8BitMinigameScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _playerX = 160f;
            _playerY = 500f;
            _playerVy = 0f;
            _grounded = true;
            _timeRemaining = 60.0f;

            _cupcakeType = 1;
            _cupcakeAmmo = 10;
            _iceCreamSpawned = false;
            _hasIceCream = false;
            _elizabethCutsceneActive = false;
            _elizabethX = -80f;
            _cutsceneTimer = 0f;
            _clawExtended = false;

            _children.Clear();
            _pickups.Clear();
            _projectiles.Clear();

            // Canonical child groups along the scrolling 3200px stage
            _children.Add(new ChildNpc { WorldX = 450f, Y = 500f, HitsNeeded = 2 });
            _children.Add(new ChildNpc { WorldX = 560f, Y = 500f, HitsNeeded = 2 });

            // Group of 3 for Blue Shotgun Cupcakes
            _children.Add(new ChildNpc { WorldX = 1120f, Y = 500f, HitsNeeded = 2 });
            _children.Add(new ChildNpc { WorldX = 1190f, Y = 430f, HitsNeeded = 2 });
            _children.Add(new ChildNpc { WorldX = 1260f, Y = 500f, HitsNeeded = 2 });

            // Row of 4 for Green Piercing Cupcakes
            _children.Add(new ChildNpc { WorldX = 1920f, Y = 500f, HitsNeeded = 1 });
            _children.Add(new ChildNpc { WorldX = 2020f, Y = 500f, HitsNeeded = 1 });
            _children.Add(new ChildNpc { WorldX = 2120f, Y = 500f, HitsNeeded = 1 });
            _children.Add(new ChildNpc { WorldX = 2620f, Y = 500f, HitsNeeded = 1 });

            // Pickups
            _pickups.Add(new CupcakePickup { WorldX = 920f, Y = 440f, Type = 2, Collected = false }); // Blue (4 uses)
            _pickups.Add(new CupcakePickup { WorldX = 1750f, Y = 440f, Type = 3, Collected = false }); // Green (3 uses)
            _pickups.Add(new CupcakePickup { WorldX = 2480f, Y = 440f, Type = 1, Collected = false }); // Extra Pink

            _audio.StartAmbientLoop("casual_bongos", 0.35f);
            _audio.ClearDialogue();
        }

        public void Update(float deltaTime)
        {
            if (_elizabethCutsceneActive)
            {
                UpdateElizabethCutscene(deltaTime);
                return;
            }

            _timeRemaining -= deltaTime;
            if (_timeRemaining <= 0f)
            {
                _router.TransitionTo(SceneId.TITLE_MENU);
                return;
            }

            // Horizontal movement (A/D or Left/Right)
            float moveDir = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveDir -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDir += 1f;
            _playerX = Mathf.Clamp(_playerX + moveDir * 330f * deltaTime, 80f, 3050f);

            // Jump (LeftShift / RightShift / W / UpArrow)
            if (_grounded && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.W)))
            {
                _playerVy = -540f;
                _grounded = false;
                _audio.PlaySfx("honk", 0.5f);
            }

            _playerY += _playerVy * deltaTime;
            _playerVy += 1250f * deltaTime;

            bool overPit = IsOverPit(_playerX);
            if (!overPit && _playerY >= 500f)
            {
                _playerY = 500f;
                _playerVy = 0f;
                _grounded = true;
            }
            else if (overPit && _playerY > 720f)
            {
                // Fell in pit -> reset to start of segment
                _playerX = Mathf.Max(140f, _playerX - 240f);
                _playerY = 400f;
                _playerVy = 0f;
            }

            // Fire Cupcake or Drop Ice Cream (/ or Space or Left-Click)
            if (Input.GetKeyDown(KeyCode.Slash) || Input.GetKeyDown(KeyCode.Space))
            {
                HandleFireOrDrop();
            }

            // Collect Pickups
            for (int i = 0; i < _pickups.Count; i++)
            {
                var p = _pickups[i];
                if (!p.Collected && Mathf.Abs(_playerX - p.WorldX) < 55f && Mathf.Abs(_playerY - p.Y) < 80f)
                {
                    p.Collected = true;
                    _pickups[i] = p;
                    _cupcakeType = p.Type;
                    _cupcakeAmmo = p.Type == 1 ? 10 : (p.Type == 2 ? 4 : 3);
                    _audio.PlaySfx("chime", 0.85f);
                }
            }

            // Update Projectiles & Check Child Satisfaction
            for (int i = 0; i < _projectiles.Count; i++)
            {
                var pr = _projectiles[i];
                if (!pr.Active) continue;
                pr.WorldX += pr.Vx * deltaTime;
                pr.Y += pr.Vy * deltaTime;

                for (int c = 0; c < _children.Count; c++)
                {
                    var ch = _children[c];
                    if (!ch.Fed && Mathf.Abs(pr.WorldX - ch.WorldX) < 42f && Mathf.Abs(pr.Y - ch.Y) < 55f)
                    {
                        ch.HitsNeeded--;
                        if (ch.HitsNeeded <= 0) ch.Fed = true;
                        _children[c] = ch;
                        _audio.PlaySfx("chime", 0.6f);
                        if (!pr.Piercing)
                        {
                            pr.Active = false;
                            break;
                        }
                    }
                }

                if (Mathf.Abs(pr.WorldX - _playerX) > 750f) pr.Active = false;
                _projectiles[i] = pr;
            }

            // Check if all children fed -> spawn Ice Cream at right goalpost (x=2920)
            if (!_iceCreamSpawned)
            {
                bool allFed = true;
                for (int c = 0; c < _children.Count; c++)
                {
                    if (!_children[c].Fed) { allFed = false; break; }
                }
                if (allFed)
                {
                    _iceCreamSpawned = true;
                    _audio.PlaySfx("chime", 1.0f);
                }
            }

            if (_iceCreamSpawned && !_hasIceCream && Mathf.Abs(_playerX - 2920f) < 70f)
            {
                _hasIceCream = true;
                _audio.PlaySfx("chime", 1.0f);
            }
        }

        private void HandleFireOrDrop()
        {
            if (_hasIceCream && _playerX <= 260f)
            {
                // Drop Ice Cream at starting screen -> Trigger Elizabeth Afton Cutscene!
                _elizabethCutsceneActive = true;
                _elizabethX = 20f;
                _cutsceneTimer = 0f;
                _audio.StopAmbientLoop();
                _audio.Speak("Elizabeth Afton", "Don't tell Daddy that I'm here. I wanted to watch your show too! Where did the other children go?", VoiceProfileId.ElizabethAfton, 5.5f, true);
                return;
            }

            if (_cupcakeAmmo <= 0) return;
            _cupcakeAmmo--;
            _audio.PlaySfx("honk", 0.6f);

            if (_cupcakeType == 1)
            {
                _projectiles.Add(new Projectile { WorldX = _playerX + 30f, Y = _playerY - 20f, Vx = 520f, Vy = 0f, Piercing = false, Active = true });
            }
            else if (_cupcakeType == 2)
            {
                _projectiles.Add(new Projectile { WorldX = _playerX + 30f, Y = _playerY - 20f, Vx = 500f, Vy = -120f, Piercing = false, Active = true });
                _projectiles.Add(new Projectile { WorldX = _playerX + 30f, Y = _playerY - 20f, Vx = 520f, Vy = 0f, Piercing = false, Active = true });
                _projectiles.Add(new Projectile { WorldX = _playerX + 30f, Y = _playerY - 20f, Vx = 500f, Vy = 120f, Piercing = false, Active = true });
            }
            else
            {
                _projectiles.Add(new Projectile { WorldX = _playerX + 30f, Y = _playerY - 20f, Vx = 620f, Vy = 0f, Piercing = true, Active = true });
            }
        }

        private void UpdateElizabethCutscene(float deltaTime)
        {
            _cutsceneTimer += deltaTime;
            if (_elizabethX < _playerX - 75f)
            {
                _elizabethX += 70f * deltaTime;
            }
            else if (!_clawExtended && _cutsceneTimer >= 5.5f)
            {
                _clawExtended = true;
                _audio.PlaySfx("jumpscare", 1.0f);
            }
            else if (_clawExtended && _cutsceneTimer >= 8.2f)
            {
                _state.State.starsUnlocked.star2_minigame = true;
                _state.Save();
                _router.TransitionTo(SceneId.TITLE_MENU);
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            canvas.FillRect(0, 0, 1280, 720, new Color(0.08f, 0.04f, 0.14f));

            float camX = Mathf.Clamp(_playerX - 380f, 0f, 2000f);

            // 8-Bit Ground Blocks & Pits
            for (int gx = 0; gx < 3200; gx += 40)
            {
                if (IsOverPit(gx + 20f)) continue;
                float sx = gx - camX;
                if (sx > -50 && sx < 1300)
                {
                    canvas.FillRect(sx, 550, 38, 170, new Color(0.75f, 0.25f, 0.45f));
                    canvas.FillRect(sx, 550, 38, 14, new Color(0.95f, 0.75f, 0.25f));
                }
            }

            // Children NPCs
            for (int i = 0; i < _children.Count; i++)
            {
                var ch = _children[i];
                float sx = ch.WorldX - camX;
                if (sx < -60 || sx > 1320) continue;
                Color shirt = ch.Fed ? Color.green : new Color(0.95f, 0.35f, 0.25f);
                canvas.FillRect(sx - 16, ch.Y - 30, 32, 32, new Color(0.95f, 0.8f, 0.65f));
                canvas.FillRect(sx - 20, ch.Y + 2, 40, 48, shirt);
                canvas.DrawText(ch.Fed ? "HAPPY!" : $"HUNGRY ({ch.HitsNeeded})", sx - 35, ch.Y - 52, 12, ch.Fed ? Color.green : Color.yellow);
            }

            // Cupcake Pickups
            for (int i = 0; i < _pickups.Count; i++)
            {
                var p = _pickups[i];
                if (p.Collected) continue;
                float sx = p.WorldX - camX;
                Color cCol = p.Type == 1 ? new Color(1f, 0.4f, 0.75f) : (p.Type == 2 ? Color.cyan : Color.green);
                canvas.FillRect(sx - 18, p.Y - 18, 36, 36, cCol);
                canvas.DrawText(p.Type == 2 ? "BLUE x4" : (p.Type == 3 ? "GREEN x3" : "PINK x10"), sx - 28, p.Y - 38, 12, cCol);
            }

            // Projectiles
            for (int i = 0; i < _projectiles.Count; i++)
            {
                var pr = _projectiles[i];
                if (!pr.Active) continue;
                Color pCol = pr.Piercing ? Color.green : Color.magenta;
                canvas.FillRect(pr.WorldX - camX - 10, pr.Y - 10, 20, 20, pCol);
            }

            // Ice Cream Goalpost at far right (x=2920)
            float goalScreenX = 2920f - camX;
            canvas.FillRect(goalScreenX, 380, 14, 170, Color.white);
            if (_iceCreamSpawned && !_hasIceCream)
            {
                canvas.FillRect(goalScreenX - 45, 470, 32, 48, new Color(1f, 0.9f, 0.7f));
                canvas.DrawText("ICE CREAM!", goalScreenX - 65, 445, 14, Color.cyan);
            }

            // 8-Bit Circus Baby Sprite
            float babyScreenX = _playerX - camX;
            canvas.FillRect(babyScreenX - 28, _playerY - 60, 56, 110, new Color(0.95f, 0.95f, 0.98f));
            canvas.FillRect(babyScreenX - 32, _playerY - 10, 64, 42, new Color(0.88f, 0.18f, 0.15f)); // Red dress
            canvas.FillRect(babyScreenX - 44, _playerY - 58, 14, 34, new Color(0.82f, 0.25f, 0.1f)); // Left pigtail
            canvas.FillRect(babyScreenX + 30, _playerY - 58, 14, 34, new Color(0.82f, 0.25f, 0.1f)); // Right pigtail
            // Eyes (Blue before capture, Green after)
            Color eyeCol = _clawExtended ? Color.green : Color.cyan;
            canvas.FillRect(babyScreenX - 14, _playerY - 46, 8, 8, eyeCol);
            canvas.FillRect(babyScreenX + 6, _playerY - 46, 8, 8, eyeCol);

            // Elizabeth Afton & Stomach Claw during Finale Cutscene
            if (_elizabethCutsceneActive)
            {
                if (!_clawExtended)
                {
                    float eliScreenX = _elizabethX - camX;
                    canvas.FillRect(eliScreenX - 16, 485, 32, 30, new Color(0.98f, 0.85f, 0.25f)); // Blonde hair
                    canvas.FillRect(eliScreenX - 16, 515, 32, 35, new Color(0.92f, 0.25f, 0.55f)); // Pink shirt
                    canvas.FillRect(babyScreenX - 65, 515, 22, 32, new Color(1f, 0.92f, 0.75f)); // Dropped ice cream
                }
                else
                {
                    // Metallic stomach claw snatching Elizabeth
                    canvas.FillRect(babyScreenX - 85, _playerY - 15, 85, 16, new Color(0.55f, 0.58f, 0.62f));
                    canvas.DrawText("★ STAR 2 UNLOCKED (PRIVATE ROOM KEYCARD ACQUIRED) ★", 240, 220, 24, Color.yellow, TextAnchor.UpperCenter, 800, 50);
                }
            }

            // HUD
            string cupcakeName = _cupcakeType == 1 ? "PINK (SINGLE)" : (_cupcakeType == 2 ? "BLUE (3-WAY)" : "GREEN (PIERCING)");
            string objective = !_iceCreamSpawned ? "FEED ALL CHILDREN ([A/D] MOVE, [SHIFT] JUMP, [SPACE / /] FIRE)" :
                               (!_hasIceCream ? "COLLECT ICE CREAM AT FAR RIGHT GOALPOST!" : "RETURN TO FAR LEFT START & PRESS [SPACE / /] TO DROP ICE CREAM!");
            canvas.DrawText($"TIME: {Mathf.CeilToInt(_timeRemaining)}s  |  CUPCAKE: {cupcakeName} x{_cupcakeAmmo}\n{objective}", 80, 25, 18, Color.white, TextAnchor.UpperLeft, 1100, 60);

            if (canvas.Button(1060, 25, 170, 40, "Instant Win (Test)", new Color(0.2f, 0.4f, 0.2f), Color.white, 13))
            {
                _iceCreamSpawned = true;
                _hasIceCream = true;
                _playerX = 180f;
                HandleFireOrDrop();
            }

            canvas.DrawCrtScanlinesAndStatic(0.14f, 0.35f);
        }

        private static bool IsOverPit(float worldX)
        {
            for (int i = 0; i < PitRanges.Length; i++)
            {
                if (worldX >= PitRanges[i].x && worldX <= PitRanges[i].y) return true;
            }
            return false;
        }

        public void Exit()
        {
            _audio.StopAmbientLoop();
        }
    }
}
