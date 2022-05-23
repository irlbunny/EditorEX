using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using EditorEX.AudioSpectrogram;
using HarmonyLib;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(BaseBeatBasedItemContainer<BeatmapObjectBeatLine>), "UpdateItems")]
    internal class BaseBeatBasedItemContainerUpdateItems
    {
        private static void Prefix(IBeatmapLevelState ____beatmapLevelState, BeatmapObjectPlacementHelper ____beatmapObjectPlacementHelper)
        {
            if (SpectrogramView.Instance != null)
                SpectrogramView.Instance.RefreshView(____beatmapLevelState.beat - 5f, ____beatmapLevelState.beat + 16f, ____beatmapObjectPlacementHelper);
        }
    }
}
