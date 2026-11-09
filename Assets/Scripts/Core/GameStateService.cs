using System;
using System.Collections.Generic;
using UnityEngine;

namespace FNaFSisterLocation.Core
{
    /// <summary>
    /// All canonical scene identifiers in Five Nights at Freddy's: Sister Location.
    /// </summary>
    public enum SceneId
    {
        OPENING_CUTSCENE,
        TITLE_MENU,
        EXTRAS_MENU,
        PRE_NIGHT_INTERLUDE,
        ELEVATOR,
        VENT_CRAWL,
        PRIMARY_CONTROL,
        BALLORA_GALLERY,
        BREAKER_ROOM,
        CIRCUS_CONTROL,
        UNDER_DESK,
        FUNTIME_AUDITORIUM,
        PARTS_AND_SERVICE,
        SPRINGLOCK_SUIT,
        SCOOPING_ROOM,
        PRIVATE_ROOM,
        TV_LIVING_ROOM,
        BABY_8BIT_MINIGAME,
        CUSTOM_NIGHT_MENU,
        DECAY_8BIT_CUTSCENE,
        SPRINGTRAP_FINALE
    }

    /// <summary>
    /// Lifecycle contract for every isolated game scene.
    /// </summary>
    public interface IGameScene
    {
        SceneId Id { get; }
        void Enter(Dictionary<string, object> parameters);
        void Update(float deltaTime);
        void Render(ProceduralCanvas canvas);
        void Exit();
    }

    [Serializable]
    public class StarsUnlockedDto
    {
        public bool star1_realEnding;
        public bool star2_minigame;
        public bool star3_fakeEnding;
        public bool star4_goldenFreddyVHard;
    }

    [Serializable]
    public class PresetDifficultyStarsDto
    {
        public string presetId;
        public bool easy;
        public bool normal;
        public bool hard;
        public bool vhard;
    }

    /// <summary>
    /// Persistent save schema stored under FNAF_SL_SAVE_V1 in PlayerPrefs / localStorage.
    /// </summary>
    [Serializable]
    public class SaveStateDto
    {
        public StarsUnlockedDto starsUnlocked = new StarsUnlockedDto();
        public int currentNight = 1;
        public List<PresetDifficultyStarsDto> presetStars = new List<PresetDifficultyStarsDto>();
        public bool debugForceEasterEggs = false;
        public int vhardPresetsBeatenCount = 0;
        public bool fastTimerMode = true;
    }

    /// <summary>
    /// Centralized state and persistence service for FNAF_SL_SAVE_V1.
    /// </summary>
    public sealed class GameStateService
    {
        public const string SAVE_KEY = "FNAF_SL_SAVE_V1";

        public SaveStateDto State { get; private set; }
        public event Action<SaveStateDto> OnStateChanged;

        private static readonly string[] CanonicalPresetIds = new string[]
        {
            "angry_ballet",
            "freddy_and_co",
            "funtime_frenzy",
            "dolls_attack",
            "girls_night",
            "weirdos",
            "top_shelf",
            "bottom_shelf",
            "cupcake_challenge",
            "golden_freddy"
        };

        public GameStateService()
        {
            Load();
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string raw = PlayerPrefs.GetString(SAVE_KEY);
                try
                {
                    State = JsonUtility.FromJson<SaveStateDto>(raw) ?? CreateDefault();
                }
                catch
                {
                    State = CreateDefault();
                }
            }
            else
            {
                State = CreateDefault();
            }

            EnsurePresetsInitialized();
        }

        public void Save()
        {
            EnsurePresetsInitialized();
            string json = JsonUtility.ToJson(State);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            OnStateChanged?.Invoke(State);
        }

        public void ResetNewGame()
        {
            State.currentNight = 1;
            Save();
        }

        public void UnlockAllForTesting()
        {
            State.starsUnlocked.star1_realEnding = true;
            State.starsUnlocked.star2_minigame = true;
            State.starsUnlocked.star3_fakeEnding = true;
            State.starsUnlocked.star4_goldenFreddyVHard = true;
            State.currentNight = 5;
            for (int i = 0; i < State.presetStars.Count; i++)
            {
                State.presetStars[i].easy = true;
                State.presetStars[i].normal = true;
                State.presetStars[i].hard = true;
                State.presetStars[i].vhard = true;
            }
            State.vhardPresetsBeatenCount = 7;
            Save();
        }

        public PresetDifficultyStarsDto GetPresetStars(string presetId)
        {
            EnsurePresetsInitialized();
            for (int i = 0; i < State.presetStars.Count; i++)
            {
                if (State.presetStars[i].presetId == presetId)
                {
                    return State.presetStars[i];
                }
            }
            var created = new PresetDifficultyStarsDto { presetId = presetId };
            State.presetStars.Add(created);
            return created;
        }

        public void MarkPresetWon(string presetId, int difficultyIndex)
        {
            var entry = GetPresetStars(presetId);
            if (difficultyIndex == 0) entry.easy = true;
            else if (difficultyIndex == 1) entry.normal = true;
            else if (difficultyIndex == 2) entry.hard = true;
            else if (difficultyIndex >= 3)
            {
                if (!entry.vhard)
                {
                    State.vhardPresetsBeatenCount = Mathf.Clamp(State.vhardPresetsBeatenCount + 1, 1, 7);
                }
                entry.vhard = true;
                if (presetId == "golden_freddy")
                {
                    State.starsUnlocked.star4_goldenFreddyVHard = true;
                }
            }
            Save();
        }

        public int GetEarnedStarCount()
        {
            int count = 0;
            if (State.starsUnlocked.star1_realEnding) count++;
            if (State.starsUnlocked.star2_minigame) count++;
            if (State.starsUnlocked.star3_fakeEnding) count++;
            if (State.starsUnlocked.star4_goldenFreddyVHard) count++;
            return count;
        }

        private SaveStateDto CreateDefault()
        {
            var dto = new SaveStateDto
            {
                starsUnlocked = new StarsUnlockedDto(),
                currentNight = 1,
                presetStars = new List<PresetDifficultyStarsDto>(),
                debugForceEasterEggs = false,
                vhardPresetsBeatenCount = 0,
                fastTimerMode = true
            };
            return dto;
        }

        private void EnsurePresetsInitialized()
        {
            if (State.starsUnlocked == null) State.starsUnlocked = new StarsUnlockedDto();
            if (State.presetStars == null) State.presetStars = new List<PresetDifficultyStarsDto>();

            for (int i = 0; i < CanonicalPresetIds.Length; i++)
            {
                string id = CanonicalPresetIds[i];
                bool found = false;
                for (int j = 0; j < State.presetStars.Count; j++)
                {
                    if (State.presetStars[j].presetId == id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    State.presetStars.Add(new PresetDifficultyStarsDto { presetId = id });
                }
            }
        }
    }
}
