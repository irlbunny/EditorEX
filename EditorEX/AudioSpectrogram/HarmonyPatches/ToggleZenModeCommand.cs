using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.AudioSpectrogram;
using HarmonyLib;

namespace EditorEX.HarmonyPatches
{
    // Hides the spectrogram when Zen Mode is enabled.
    [HarmonyPatch(typeof(ToggleZenModeCommand), "Execute")]
    internal class ToggleZenModeCommandExecute
    {
        private static void Prefix(ILevelEditorState ____levelEditorState)
        {
            if (SpectrogramView.Instance != null)
                SpectrogramView.Instance.SetVisible(____levelEditorState.zenMode);
        }
    }
}
