using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Gameplay
{
    public sealed class TvLivingRoomScene : IGameScene
    {
        public SceneId Id => SceneId.TV_LIVING_ROOM;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _episode = 1;
        private bool _returnToExtras = false;
        private float _timer = 0f;
        private float _episodeDuration = 23f;
        private float _popcornMunchAnim = 0f;
        private bool _popcornMinireenaPopped = false;
        private float _ennardDragX = 1320f;

        public TvLivingRoomScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _episode = parameters != null && parameters.TryGetValue("episode", out object ep) ? (int)ep : 1;
            _returnToExtras = parameters != null && parameters.TryGetValue("returnToExtras", out object ret) && (bool)ret;
            _timer = 0f;
            _popcornMunchAnim = 0f;
            _popcornMinireenaPopped = false;
            _ennardDragX = 1320f;

            _audio.ClearDialogue();
            _audio.StartAmbientLoop("casual_bongos", 0.14f);
            QueueEpisodeDialogue(_episode);
        }

        private void QueueEpisodeDialogue(int episode)
        {
            if (episode == 1)
            {
                _episodeDuration = 24f;
                _audio.Speak("TV Announcer", "As the sun sets, so also does another chapter in the saga of The Immortal and the Restless!", VoiceProfileId.Announcer, 4.5f, true);
                _audio.Speak("Clara", "Vlad, you can't keep doing this! The baby is yours!", VoiceProfileId.Clara, 4.0f, false);
                _audio.Speak("Vlad", "Clara, I tell you, the baby isn't mine! Count the toes, that's no vampire baby!", VoiceProfileId.Vlad, 4.8f, false);
                _audio.Speak("Clara", "He turns into a bat when he's angry, Vlad! And he only drinks blood-red cranberry juice!", VoiceProfileId.Clara, 4.8f, false);
                _audio.Speak("Vlad", "Lots of kids do that! Look, I'm a busy undead creature of the night, I don't have time for child support!", VoiceProfileId.Vlad, 5.2f, false);
            }
            else if (episode == 2)
            {
                _episodeDuration = 22f;
                _audio.Speak("TV Announcer", "Welcome back to The Immortal and the Restless!", VoiceProfileId.Announcer, 3.8f, true);
                _audio.Speak("Clara", "Vlad! The baby burned down the nursery and slept in the ceiling rafters!", VoiceProfileId.Clara, 4.5f, false);
                _audio.Speak("Vlad", "Honestly Clara, boys will be boys! Maybe he's just an enthusiastic acrobat!", VoiceProfileId.Vlad, 4.5f, false);
                _audio.Speak("Clara", "Then I'm taking the car, and I'm taking the diamond ring!", VoiceProfileId.Clara, 4.2f, false);
                _audio.Speak("Vlad", "Not the station wagon, Clara!", VoiceProfileId.Vlad, 3.8f, false);
            }
            else if (episode == 3)
            {
                _episodeDuration = 22f;
                _audio.Speak("TV Announcer", "Tonight on The Immortal and the Restless: passions ignite!", VoiceProfileId.Announcer, 4.0f, true);
                _audio.Speak("Vlad", "Clara, I couldn't stay away. Your fiery temper warms my cold, undead heart.", VoiceProfileId.Vlad, 4.5f, false);
                _audio.Speak("Clara", "Oh Vlad! I set your mansion on fire because I missed you so much!", VoiceProfileId.Clara, 4.5f, false);
                _audio.Speak("Vlad", "Wait, you burned down my ancestral manor?!", VoiceProfileId.Vlad, 3.8f, false);
                _audio.Speak("Clara", "Kiss me, you fool!", VoiceProfileId.Clara, 3.5f, false);
            }
            else
            {
                _episodeDuration = 22f;
                _audio.Speak("TV Announcer", "The season finale of The Immortal and the Restless, sponsored by Exotic Butters!", VoiceProfileId.Announcer, 4.8f, true);
                _audio.Speak("Vlad", "Clara, I've finally accepted the boy. Look how gracefully he hisses at the sun.", VoiceProfileId.Vlad, 4.8f, false);
                _audio.Speak("Clara", "Oh Vlad, we're one big happy dysfunctional nocturnal family at last!", VoiceProfileId.Clara, 4.8f, false);
            }
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
            if (_popcornMunchAnim > 0f) _popcornMunchAnim -= deltaTime;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                MunchPopcorn();
            }

            // In Episode 4 (Fake Ending), Ennard drags itself into the right side of the living room
            if (_episode == 4 && _timer >= 12.5f)
            {
                if (_ennardDragX > 960f && Mathf.FloorToInt((_timer - deltaTime) * 2f) != Mathf.FloorToInt(_timer * 2f))
                {
                    _audio.PlaySfx("vent_crawl", 0.65f, 0.65f);
                }
                _ennardDragX = Mathf.MoveTowards(_ennardDragX, 950f, 110f * deltaTime);
            }

            if (_timer >= _episodeDuration)
            {
                FinishEpisode();
            }
        }

        private void MunchPopcorn()
        {
            _popcornMunchAnim = 0.35f;
            _audio.PlaySfx("springlock_wind", 0.75f);

            // Canonical 1/10,000 Popcorn Minireena Easter Egg (100% if debugForceEasterEggs is active)
            if (_state.State.debugForceEasterEggs || UnityEngine.Random.Range(0, 10000) == 0)
            {
                _popcornMinireenaPopped = true;
                _audio.PlaySfx("honk", 0.9f);
            }
        }

        private void FinishEpisode()
        {
            _audio.StopAmbientLoop();
            _audio.ClearDialogue();

            if (_returnToExtras)
            {
                _router.TransitionTo(SceneId.EXTRAS_MENU);
                return;
            }

            if (_episode >= 1 && _episode <= 3)
            {
                int nextNight = _episode + 1;
                _router.TransitionTo(SceneId.PRE_NIGHT_INTERLUDE, new Dictionary<string, object> { { "night", nextNight } });
            }
            else
            {
                _router.TransitionTo(SceneId.TITLE_MENU);
            }
        }

        public void Render(ProceduralCanvas canvas)
        {
            // Dark Living Room Wallpaper & Lamp Glow
            canvas.FillRect(0, 0, 1280, 720, new Color(0.05f, 0.04f, 0.09f));
            for (int sx = 60; sx < 1280; sx += 120)
            {
                canvas.FillRect(sx, 0, 18, 560, new Color(0.07f, 0.05f, 0.12f));
            }
            canvas.FillRect(0, 540, 1280, 180, new Color(0.08f, 0.05f, 0.05f));

            // Exotic Butters Gift Basket on Top of TV (Episode 4 Fake Ending)
            if (_episode == 4)
            {
                // Woven basket
                canvas.FillRect(560, 78, 160, 56, new Color(0.58f, 0.38f, 0.16f));
                canvas.StrokeRect(560, 78, 160, 56, 3f, new Color(0.38f, 0.22f, 0.08f));
                // Sticks of butter
                for (int b = 0; b < 4; b++)
                {
                    canvas.FillRect(574 + b * 34, 52, 28, 30, new Color(0.98f, 0.9f, 0.28f));
                }
                // Red Bow Ribbon
                canvas.FillRect(622, 68, 36, 20, new Color(0.9f, 0.12f, 0.15f));
                canvas.DrawText("EXOTIC BUTTERS", 560, 26, 15, Color.yellow, TextAnchor.UpperCenter, 160, 24);
            }

            // Retro CRT Television Cabinet
            canvas.FillRect(370, 130, 540, 390, new Color(0.24f, 0.15f, 0.09f));
            canvas.StrokeRect(370, 130, 540, 390, 6f, new Color(0.14f, 0.08f, 0.04f));
            canvas.FillRect(395, 155, 430, 340, new Color(0.12f, 0.18f, 0.34f));
            canvas.StrokeRect(395, 155, 430, 340, 4f, new Color(0.3f, 0.32f, 0.38f));

            // TV Knobs & Speaker Grille
            canvas.FillRect(845, 180, 42, 42, new Color(0.4f, 0.4f, 0.44f));
            canvas.FillRect(845, 240, 42, 42, new Color(0.4f, 0.4f, 0.44f));
            for (int g = 0; g < 8; g++)
            {
                canvas.FillRect(840, 310 + g * 18, 52, 6, new Color(0.1f, 0.06f, 0.03f));
            }

            // Inside TV Screen: Vlad (Purple Vampire) & Clara (Blonde in Yellow Dress)
            float mouthBob = Mathf.PingPong(Time.time * 8f, 8f);
            // Vlad (Left)
            canvas.FillRect(450, 290, 90, 185, new Color(0.42f, 0.14f, 0.58f)); // Purple suit
            canvas.FillRect(435, 280, 120, 40, new Color(0.25f, 0.06f, 0.35f)); // Vampire collar
            canvas.FillRect(465, 215, 60, 75, new Color(0.82f, 0.78f, 0.9f));   // Pale face
            canvas.FillRect(462, 198, 66, 24, new Color(0.12f, 0.08f, 0.16f));  // Hair
            canvas.FillRect(482, 265, 26, 4 + mouthBob, Color.red);             // Mouth

            // Clara (Right)
            canvas.FillRect(665, 305, 85, 170, new Color(0.92f, 0.78f, 0.15f)); // Yellow dress
            canvas.FillRect(678, 228, 58, 72, new Color(0.96f, 0.82f, 0.72f));  // Face
            canvas.FillRect(668, 204, 78, 36, new Color(0.98f, 0.88f, 0.25f));  // Blonde hair
            canvas.FillRect(694, 276, 24, 4 + mouthBob * 0.8f, new Color(0.85f, 0.15f, 0.2f));

            canvas.DrawText($"THE IMMORTAL AND THE RESTLESS — EPISODE {_episode}", 405, 165, 14, Color.yellow, TextAnchor.UpperCenter, 410, 28);

            // Foreground Popcorn Bowl (Press SPACE or Click to Munch)
            float bowlY = 565f + (_popcornMunchAnim > 0f ? 12f : 0f);
            if (_popcornMinireenaPopped)
            {
                // 1/10,000 Minireena popping out of the popcorn bowl!
                canvas.FillRect(615, bowlY - 95, 50, 85, new Color(0.92f, 0.88f, 0.78f));
                canvas.FillRect(622, bowlY - 130, 36, 36, Color.white);
                canvas.FillRect(628, bowlY - 118, 6, 6, Color.yellow);
                canvas.FillRect(644, bowlY - 118, 6, 6, Color.yellow);
                canvas.DrawText("1/10,000 POPCORN MINIREENA!", 520, bowlY - 158, 15, Color.yellow, TextAnchor.UpperCenter, 240, 26);
            }

            // Popcorn kernels
            for (int k = 0; k < 9; k++)
            {
                canvas.FillRect(558 + k * 18, bowlY - 18 - (k % 2) * 8, 22, 22, new Color(0.98f, 0.92f, 0.45f));
            }
            if (_popcornMunchAnim > 0f)
            {
                canvas.FillRect(610, bowlY - 65, 16, 16, new Color(0.98f, 0.92f, 0.45f));
                canvas.FillRect(655, bowlY - 80, 14, 14, new Color(0.98f, 0.92f, 0.45f));
            }

            if (canvas.Button(540, bowlY, 200, 90, "POPCORN\n[SPACE TO MUNCH]", new Color(0.75f, 0.16f, 0.16f), Color.white, 15))
            {
                MunchPopcorn();
            }

            // Episode 4 Fake Ending Finale: Ennard dragging itself into the right side of the Living Room
            if (_episode == 4 && _ennardDragX < 1275f)
            {
                canvas.DrawAnimatronicHead("ennard", _ennardDragX, 410f, 1.55f, 0.25f, 0.2f);
                canvas.FillRect(_ennardDragX - 65, 530, 130, 180, new Color(0.28f, 0.3f, 0.34f));
                if (_ennardDragX <= 960f)
                {
                    canvas.DrawText("FAKE ENDING UNLOCKED — ★ STAR 3 AWARDED", 340, 528, 20, Color.yellow, TextAnchor.UpperCenter, 600, 32);
                }
            }

            if (canvas.Button(1060, 36, 170, 40, "SKIP >>", new Color(0.18f, 0.18f, 0.26f), Color.white, 15))
            {
                FinishEpisode();
            }

            canvas.DrawCrtScanlinesAndStatic(0.14f, 0.38f);
        }

        public void Exit()
        {
            _audio.StopAmbientLoop();
            _audio.ClearDialogue();
        }
    }

    /// <summary>
    /// Canonical 7-Stage 8-Bit Michael Afton Neighborhood Decay Cutscenes (Custom Night V. Hard rewards).
    /// </summary>
    public sealed class Decay8BitCutsceneScene : IGameScene
    {
        public SceneId Id => SceneId.DECAY_8BIT_CUTSCENE;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private int _stage = 1; // 1..7
        private bool _returnToExtras = false;
        private float _timer = 0f;
        private float _michaelX = 120f;
        private int _stage7SubPhase = 0; // 0=walking to sewer, 1=convulsing & regurgitating Ennard, 2=collapsed & Baby "You won't die", 3=rising up

        public Decay8BitCutsceneScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _stage = parameters != null && parameters.TryGetValue("stage", out object st) ? Mathf.Clamp((int)st, 1, 7) : Mathf.Clamp(_state.State.vhardPresetsBeatenCount, 1, 7);
            _returnToExtras = parameters != null && parameters.TryGetValue("returnToExtras", out object ret) && (bool)ret;
            _timer = 0f;
            _michaelX = 110f;
            _stage7SubPhase = 0;

            _audio.ClearDialogue();
            if (_stage <= 4)
            {
                _audio.StartAmbientLoop("casual_bongos", 0.2f);
            }
            else
            {
                _audio.StopAmbientLoop();
            }
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;

            if (_stage < 7)
            {
                float walkSpeed = Mathf.Lerp(135f, 75f, (_stage - 1) / 6f);
                _michaelX += walkSpeed * deltaTime;
                if (_michaelX >= 1180f || _timer >= 14f)
                {
                    ExitCutscene();
                }
            }
            else
            {
                // Stage 7: Walk to Sewer Grate (x=680), regurgitate Ennard into sewer, collapse, then rise on "You won't die"
                if (_stage7SubPhase == 0)
                {
                    _michaelX = Mathf.MoveTowards(_michaelX, 680f, 72f * deltaTime);
                    if (Mathf.Abs(_michaelX - 680f) < 2f)
                    {
                        _stage7SubPhase = 1;
                        _timer = 0f;
                        _audio.PlaySfx("vent_crawl", 0.95f);
                    }
                }
                else if (_stage7SubPhase == 1 && _timer >= 2.8f)
                {
                    _stage7SubPhase = 2;
                    _timer = 0f;
                    _audio.PlaySfx("door_slam", 0.85f);
                    _audio.Speak("Circus Baby", "You won't die... You won't die... You won't die...", VoiceProfileId.CircusBaby, 6.5f, true);
                }
                else if (_stage7SubPhase == 2 && _timer >= 4.2f)
                {
                    _stage7SubPhase = 3;
                    _timer = 0f;
                    _audio.PlaySfx("chime", 0.85f);
                }
                else if (_stage7SubPhase == 3 && _timer >= 4.5f)
                {
                    ExitCutscene();
                }
            }
        }

        private void ExitCutscene()
        {
            _audio.StopAmbientLoop();
            _audio.ClearDialogue();
            _router.TransitionTo(_returnToExtras ? SceneId.EXTRAS_MENU : SceneId.CUSTOM_NIGHT_MENU);
        }

        public void Render(ProceduralCanvas canvas)
        {
            // 8-Bit Sky, Brick Houses, Lawn, and Sidewalk
            Color skyCol = _stage <= 4 ? new Color(0.32f, 0.62f, 0.95f) : new Color(0.18f, 0.24f, 0.42f);
            canvas.FillRect(0, 0, 1280, 420, skyCol);
            canvas.FillRect(0, 420, 1280, 90, new Color(0.2f, 0.65f, 0.24f));  // Grass lawn
            canvas.FillRect(0, 510, 1280, 110, new Color(0.68f, 0.68f, 0.72f)); // Sidewalk
            canvas.FillRect(0, 620, 1280, 100, new Color(0.22f, 0.22f, 0.26f)); // Street

            // Background 8-Bit Brick Houses & Bushes
            for (int h = 0; h < 3; h++)
            {
                float hx = 120 + h * 410;
                canvas.FillRect(hx, 210, 260, 210, new Color(0.68f, 0.26f, 0.2f));
                canvas.FillRect(hx - 20, 165, 300, 50, new Color(0.35f, 0.18f, 0.14f));
                canvas.FillRect(hx + 40, 265, 65, 65, new Color(0.85f, 0.92f, 0.98f));
                canvas.FillRect(hx + 155, 290, 60, 130, new Color(0.42f, 0.25f, 0.14f));

                // Neighbor Sprite
                float nx = hx + 315;
                float ny = (_stage >= 5) ? 395f : 365f; // Hiding behind bush in Stages 5-7
                canvas.FillRect(nx, ny, 32, 32, new Color(0.95f, 0.76f, 0.62f));
                canvas.FillRect(nx - 4, ny + 32, 40, 48, new Color(0.2f, 0.5f, 0.85f));
                // Bush in front of neighbor when hiding
                if (_stage >= 5)
                {
                    canvas.FillRect(nx - 18, 390, 72, 45, new Color(0.14f, 0.48f, 0.18f));
                }
            }

            // Sewer Grate on Sidewalk Curb (x=680)
            canvas.FillRect(650, 595, 110, 32, new Color(0.1f, 0.1f, 0.12f));
            for (int g = 0; g < 5; g++)
            {
                canvas.FillRect(660 + g * 20, 595, 6, 32, new Color(0.38f, 0.4f, 0.44f));
            }

            // Glowing Animatronic Eyes inside Sewer Grate after regurgitation (Stage 7 SubPhase 2 & 3)
            if (_stage == 7 && _stage7SubPhase >= 2)
            {
                Color[] sewerEyes = { Color.green, Color.cyan, Color.magenta, Color.yellow, Color.white, Color.red };
                for (int e = 0; e < sewerEyes.Length; e++)
                {
                    canvas.FillRect(662 + e * 15, 606 + (e % 2) * 6, 6, 6, sewerEyes[e]);
                }
            }

            // Render 8-Bit Michael Afton Sprite according to Decay Stage 1..7
            RenderMichaelSprite(canvas, _michaelX, 465f);

            string[] stageDescriptions =
            {
                "",
                "CUTSCENE 1/7: HEALTHY TAN-PINK SKIN, PURPLE SHIRT, NEIGHBORS WAVING",
                "CUTSCENE 2/7: TAN SKIN WITH SHADOWED BLACK EYES",
                "CUTSCENE 3/7: SICKLY PALE GREEN SKIN, SLOUCHED POSTURE, NEIGHBORS CONCERNED",
                "CUTSCENE 4/7: DARK GREENISH-BROWN DECAYING SKIN, HOLLOW WHITE PUPILS",
                "CUTSCENE 5/7: DECREPIT GAIT, ONE GLOWING EYE, NEIGHBORS HIDING",
                "CUTSCENE 6/7: HAIRLESS DULL PURPLE FIGURE, LURCHING STEP",
                "CUTSCENE 7/7: DARK PURPLE CORPSE REGURGITATES ENNARD INTO SEWER — \"YOU WON'T DIE\""
            };
            canvas.DrawText(stageDescriptions[_stage], 60, 28, 19, Color.yellow, TextAnchor.UpperLeft, 1000, 40);

            if (canvas.Button(1060, 28, 170, 40, "SKIP >>", new Color(0.18f, 0.18f, 0.25f), Color.white, 15))
            {
                ExitCutscene();
            }

            canvas.DrawCrtScanlinesAndStatic(0.14f, 0.25f);
        }

        private void RenderMichaelSprite(ProceduralCanvas canvas, float x, float y)
        {
            Color skinCol = new Color(0.96f, 0.76f, 0.64f);
            bool hasHair = true;
            bool hasShirt = true;
            bool hollowEyes = false;

            switch (_stage)
            {
                case 1:
                    skinCol = new Color(0.96f, 0.76f, 0.64f);
                    break;
                case 2:
                    skinCol = new Color(0.88f, 0.68f, 0.54f);
                    hollowEyes = true;
                    break;
                case 3:
                    skinCol = new Color(0.58f, 0.76f, 0.45f);
                    hollowEyes = true;
                    break;
                case 4:
                    skinCol = new Color(0.38f, 0.48f, 0.28f);
                    hollowEyes = true;
                    break;
                case 5:
                    skinCol = new Color(0.45f, 0.36f, 0.52f);
                    hollowEyes = true;
                    break;
                case 6:
                    skinCol = new Color(0.52f, 0.28f, 0.62f);
                    hasHair = false;
                    hollowEyes = true;
                    break;
                case 7:
                    skinCol = new Color(0.38f, 0.14f, 0.48f);
                    hasHair = false;
                    hasShirt = false;
                    hollowEyes = true;
                    break;
            }

            // Collapsed lifeless state in Stage 7 SubPhase 2
            if (_stage == 7 && _stage7SubPhase == 2)
            {
                canvas.FillRect(x - 45, y + 85, 110, 26, skinCol);
                canvas.FillRect(x + 25, y + 85, 42, 26, new Color(0.18f, 0.32f, 0.68f));
                return;
            }

            float bob = Mathf.Sin(Time.time * 9f) * 5f;
            float drawY = y + bob;

            // Head
            canvas.FillRect(x, drawY, 44, 44, skinCol);
            if (hasHair)
            {
                canvas.FillRect(x - 4, drawY - 10, 52, 16, new Color(0.36f, 0.2f, 0.1f));
            }

            // Eyes
            if (!hollowEyes)
            {
                canvas.FillRect(x + 26, drawY + 14, 8, 8, new Color(0.2f, 0.45f, 0.85f));
            }
            else
            {
                canvas.FillRect(x + 22, drawY + 12, 12, 12, Color.black);
                if (_stage >= 4)
                {
                    canvas.FillRect(x + 26, drawY + 15, 5, 5, Color.white);
                }
            }

            // Torso (Purple Shirt in Stages 1-6, bare purple ribs in Stage 7)
            Color torsoCol = hasShirt ? new Color(0.55f, 0.2f, 0.72f) : skinCol;
            canvas.FillRect(x + 4, drawY + 44, 36, 46, torsoCol);

            // Regurgitating mechanical Ennard wires into Sewer in Stage 7 SubPhase 1
            if (_stage == 7 && _stage7SubPhase == 1)
            {
                canvas.FillRect(x + 24, drawY + 28, 14, 115, new Color(0.55f, 0.58f, 0.62f));
                canvas.FillRect(x + 18, drawY + 70, 24, 65, new Color(0.35f, 0.38f, 0.42f));
            }

            // Blue Pants / Legs
            canvas.FillRect(x + 6, drawY + 90, 14, 34, new Color(0.18f, 0.32f, 0.68f));
            canvas.FillRect(x + 24, drawY + 90, 14, 34, new Color(0.18f, 0.32f, 0.68f));
        }

        public void Exit()
        {
            _audio.StopAmbientLoop();
            _audio.ClearDialogue();
        }
    }

    /// <summary>
    /// Golden Freddy 10/20 V. Hard Finale: Michael Afton's Monologue over charred Fazbear's Fright ruins & Springtrap reveal.
    /// </summary>
    public sealed class SpringtrapFinaleScene : IGameScene
    {
        public SceneId Id => SceneId.SPRINGTRAP_FINALE;

        private readonly SceneGraphRouter _router;
        private readonly GameStateService _state;
        private readonly ProceduralAudioSynthesizer _audio;

        private bool _returnToExtras = false;
        private float _timer = 0f;
        private bool _springtrapLunged = false;

        public SpringtrapFinaleScene(SceneGraphRouter router, GameStateService state, ProceduralAudioSynthesizer audio)
        {
            _router = router;
            _state = state;
            _audio = audio;
        }

        public void Enter(Dictionary<string, object> parameters)
        {
            _returnToExtras = parameters != null && parameters.TryGetValue("returnToExtras", out object ret) && (bool)ret;
            _timer = 0f;
            _springtrapLunged = false;

            _state.State.starsUnlocked.star4_goldenFreddyVHard = true;
            _state.Save();

            _audio.ClearDialogue();
            _audio.StartAmbientLoop("elevator_hum", 0.25f);

            // Queue full canonical Michael Afton monologue
            _audio.Speak("Michael Afton", "Father. It's me, Michael.", VoiceProfileId.MichaelAfton, 3.2f, true);
            _audio.Speak("Michael Afton", "I did it. I found it. It was right where you said it would be.", VoiceProfileId.MichaelAfton, 4.2f, false);
            _audio.Speak("Michael Afton", "They were all there. They didn't recognize me at first, but then, they thought I was you.", VoiceProfileId.MichaelAfton, 5.2f, false);
            _audio.Speak("Michael Afton", "And I found her. I put her back together, just like you asked me to. She's free now.", VoiceProfileId.MichaelAfton, 5.2f, false);
            _audio.Speak("Michael Afton", "But something is wrong with me. I should be dead. But I'm not.", VoiceProfileId.MichaelAfton, 4.5f, false);
            _audio.Speak("Michael Afton", "I've been living in shadows. There is only one thing left for me to do now.", VoiceProfileId.MichaelAfton, 4.8f, false);
            _audio.Speak("Michael Afton", "I'm going to come find you.", VoiceProfileId.MichaelAfton, 3.2f, false);
            _audio.Speak("Michael Afton (Robotic)", "I'm going to come find you...", VoiceProfileId.WilliamAfton, 4.2f, false);
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;

            if (!_springtrapLunged && _timer >= 31.0f)
            {
                _springtrapLunged = true;
                _audio.PlaySfx("jumpscare", 0.85f);
            }

            if (_timer >= 36.5f)
            {
                ExitFinale();
            }
        }

        private void ExitFinale()
        {
            _audio.StopAmbientLoop();
            _audio.ClearDialogue();
            _router.TransitionTo(_returnToExtras ? SceneId.EXTRAS_MENU : SceneId.TITLE_MENU);
        }

        public void Render(ProceduralCanvas canvas)
        {
            // Charred, smoking ruins of Fazbear's Fright: The Horror Attraction
            canvas.FillRect(0, 0, 1280, 720, new Color(0.08f, 0.04f, 0.03f));

            float panX = Mathf.Lerp(0f, -260f, Mathf.Clamp01(_timer / 31f));

            // Charred beams & wreckage
            for (int b = 0; b < 7; b++)
            {
                float bx = 120 + b * 230 + panX;
                canvas.FillRect(bx, 80, 48, 560, new Color(0.14f, 0.08f, 0.05f));
                canvas.FillRect(bx - 40, 480, 150, 90, new Color(0.18f, 0.1f, 0.06f));
            }

            // Broken Neon Sign: "FAZBEAR'S FRIGHT"
            canvas.FillRect(420 + panX, 160, 560, 110, new Color(0.12f, 0.06f, 0.05f));
            canvas.StrokeRect(420 + panX, 160, 560, 110, 4f, new Color(0.65f, 0.22f, 0.12f));
            canvas.DrawText("FAZBEAR'S FRIGHT — THE HORROR ATTRACTION (RUINS)", 440 + panX, 195, 22, new Color(0.95f, 0.35f, 0.18f), TextAnchor.MiddleCenter, 520, 44);

            // Drifting fire embers
            for (int e = 0; e < 16; e++)
            {
                float ex = ((e * 97f + Time.time * 45f) % 1280f);
                float ey = 720f - ((e * 63f + Time.time * 70f) % 720f);
                canvas.FillRect(ex, ey, 6, 6, new Color(1f, 0.55f, 0.12f, 0.75f));
            }

            // Springtrap lunging into foreground at the end of the monologue!
            if (_springtrapLunged)
            {
                float sx = 640f + UnityEngine.Random.Range(-8f, 8f);
                float sy = 380f + UnityEngine.Random.Range(-6f, 6f);
                // Withered olive-green Springtrap head & torn rabbit ears
                canvas.FillRect(sx - 135, sy - 270, 52, 130, new Color(0.36f, 0.42f, 0.18f));
                canvas.FillRect(sx + 83, sy - 210, 52, 70, new Color(0.36f, 0.42f, 0.18f)); // Broken right ear
                canvas.FillRect(sx - 150, sy - 140, 300, 290, new Color(0.42f, 0.46f, 0.2f));
                // Glowing silver-white Springtrap eyes
                canvas.FillRect(sx - 95, sy - 55, 58, 44, Color.black);
                canvas.FillRect(sx + 37, sy - 55, 58, 44, Color.black);
                canvas.FillRect(sx - 76, sy - 42, 22, 22, new Color(0.9f, 0.95f, 0.85f));
                canvas.FillRect(sx + 54, sy - 42, 22, 22, new Color(0.9f, 0.95f, 0.85f));
                // Withered grin & springlock teeth
                canvas.FillRect(sx - 95, sy + 40, 190, 68, new Color(0.18f, 0.08f, 0.08f));
                for (int t = 0; t < 6; t++)
                {
                    canvas.FillRect(sx - 80 + t * 28, sy + 44, 18, 24, new Color(0.78f, 0.75f, 0.62f));
                }
            }

            canvas.DrawText("GOLDEN FREDDY 10/20 V. HARD COMPLETED — ★ STAR 4 AWARDED", 290, 28, 22, Color.yellow, TextAnchor.UpperCenter, 700, 40);

            if (canvas.Button(1060, 28, 170, 40, "SKIP >>", new Color(0.22f, 0.14f, 0.12f), Color.white, 15))
            {
                ExitFinale();
            }

            canvas.DrawCrtScanlinesAndStatic(0.22f, 0.45f);
        }

        public void Exit()
        {
            _audio.StopAmbientLoop();
            _audio.ClearDialogue();
        }
    }
}
