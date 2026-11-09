using System;
using System.Collections.Generic;
using UnityEngine;
using FNaFSisterLocation.Core;

namespace FNaFSisterLocation.Tests
{
    /// <summary>
    /// Unit test suite for GameStateService persistence, 4-Star progression, and SceneGraphRouter isolation.
    /// </summary>
    public static class GameStateTests
    {
        public static void RunAll()
        {
            TestSaveStateInitializationAndPresets();
            TestGoldenFreddyVHardUnlocksStar4();
            TestSceneGraphRouterCallsExitOnTransition();
            Debug.Log("[GameStateTests] ALL TESTS PASSED.");
        }

        public static void TestSaveStateInitializationAndPresets()
        {
            // Arrange
            PlayerPrefs.DeleteKey(GameStateService.SAVE_KEY);
            var service = new GameStateService();

            // Act
            var gfStars = service.GetPresetStars("golden_freddy");

            // Assert
            if (service.State.presetStars.Count != 10)
                throw new Exception($"Expected 10 canonical presets, got {service.State.presetStars.Count}");
            if (gfStars == null || gfStars.vhard)
                throw new Exception("Expected golden_freddy preset initialized with vhard = false");
        }

        public static void TestGoldenFreddyVHardUnlocksStar4()
        {
            // Arrange
            PlayerPrefs.DeleteKey(GameStateService.SAVE_KEY);
            var service = new GameStateService();

            // Act
            service.MarkPresetWon("golden_freddy", 3);

            // Assert
            if (!service.State.starsUnlocked.star4_goldenFreddyVHard)
                throw new Exception("Expected Star 4 to unlock after beating golden_freddy on V. Hard (index 3)");
            if (service.GetEarnedStarCount() != 1)
                throw new Exception($"Expected 1 star count, got {service.GetEarnedStarCount()}");
        }

        public static void TestSceneGraphRouterCallsExitOnTransition()
        {
            // Arrange
            var host = new GameObject("TestAudioHost");
            var state = new GameStateService();
            var audio = new ProceduralAudioSynthesizer(host);
            var router = new SceneGraphRouter(state, audio);

            var sceneA = new DummyTestScene(SceneId.OPENING_CUTSCENE);
            var sceneB = new DummyTestScene(SceneId.TITLE_MENU);
            router.RegisterScene(sceneA);
            router.RegisterScene(sceneB);

            // Act
            router.TransitionTo(SceneId.OPENING_CUTSCENE);
            router.TransitionTo(SceneId.TITLE_MENU);

            // Assert
            UnityEngine.Object.DestroyImmediate(host);
            if (!sceneA.Entered || !sceneA.Exited || !sceneB.Entered)
                throw new Exception("SceneGraphRouter failed to invoke Enter/Exit lifecycle cleanly.");
        }

        private sealed class DummyTestScene : IGameScene
        {
            public SceneId Id { get; }
            public bool Entered { get; private set; }
            public bool Exited { get; private set; }

            public DummyTestScene(SceneId id)
            {
                Id = id;
            }

            public void Enter(Dictionary<string, object> parameters) => Entered = true;
            public void Update(float deltaTime) { }
            public void Render(ProceduralCanvas canvas) { }
            public void Exit() => Exited = true;
        }
    }
}
