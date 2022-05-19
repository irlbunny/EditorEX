using UnityEngine.SceneManagement;

namespace EditorEX.Utilities
{
    internal static class SceneUtil
    {
        public static bool IsInBeatmapEditor()
        {
            var activeScene = SceneManager.GetActiveScene();
            return activeScene.name == "BeatmapEditor3D" || activeScene.name == "BeatmapLevelEditorWorldUi";
        }
    }
}
