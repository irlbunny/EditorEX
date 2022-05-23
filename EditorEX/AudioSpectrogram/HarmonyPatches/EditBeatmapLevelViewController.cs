using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.AudioSpectrogram;
using HarmonyLib;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(EditBeatmapLevelViewController), "DidActivate")]
    internal class EditBeatmapLevelViewControllerDidActivate
    {
        private static void Prefix(IBeatmapDataModel ____beatmapDataModel)
        {
            if (!Config.Instance.ShowSpectrogram)
                return;

            if (SpectrogramView.Instance != null)
                SpectrogramView.Instance.Dispose();

            SpectrogramView.Instance = new(____beatmapDataModel.audioClip);
        }
    }
}
