#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FNaFSisterLocation.UI;

namespace FNaFSisterLocation.Editor
{
    [InitializeOnLoad]
    public static class FNaFSceneSetupEditor
    {
        private const string MAIN_SCENE_PATH = "Assets/Scenes/Main.unity";

        static FNaFSceneSetupEditor()
        {
            EditorApplication.delayCall += EnsureMainSceneExists;
        }

        [MenuItem("FNaF Sister Location/Generate & Open Main Scene")]
        public static void EnsureMainSceneExists()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            EnsureLegacyInputEnabled();

            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }

            if (!File.Exists(MAIN_SCENE_PATH))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var camObj = new GameObject("Main Camera");
                var cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();

                var hostObj = new GameObject("FNaF_SisterLocation_Host");
                hostObj.AddComponent<FNaFSlGameBootstrapper>();

                EditorSceneManager.SaveScene(scene, MAIN_SCENE_PATH);
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(MAIN_SCENE_PATH, true)
                };
                AssetDatabase.Refresh();
            }
        }

        private static void EnsureLegacyInputEnabled()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets != null && assets.Length > 0)
            {
                var so = new SerializedObject(assets[0]);
                var prop = so.FindProperty("activeInputHandler");
                if (prop != null && prop.intValue == 1)
                {
                    prop.intValue = 2; // Both (Input Manager + Input System)
                    so.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.SaveAssets();
                }
            }
        }

        public static void ValidateAndSetupProject()
        {
            EnsureMainSceneExists();
            FNaFSisterLocation.Tests.GameStateTests.RunAll();
            Debug.Log("[FNaFSceneSetupEditor] Project validated and Main.unity generated.");
        }
    }
}
#endif
