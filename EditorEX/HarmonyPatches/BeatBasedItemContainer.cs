using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.HarmonyPatches
{

    [HarmonyPatch(typeof(BaseBeatBasedItemContainer<BeatmapObjectBeatLine>), "UpdateItems")]
    internal class BaseBeatBasedItemContainerUpdateItems
    {
        static AccessTools.FieldRef<BaseBeatBasedItemContainer<BeatmapObjectBeatLine>, BeatmapObjectPlacementHelper> placementHelperRef =
        AccessTools.FieldRefAccess<BaseBeatBasedItemContainer<BeatmapObjectBeatLine>, BeatmapObjectPlacementHelper>("_beatmapObjectPlacementHelper");

        static AccessTools.FieldRef<BaseBeatBasedItemContainer<BeatmapObjectBeatLine>, IBeatmapLevelState> levelStateRef =
        AccessTools.FieldRefAccess<BaseBeatBasedItemContainer<BeatmapObjectBeatLine>, IBeatmapLevelState>("_beatmapLevelState");

        private static void Prefix(BaseBeatBasedItemContainer<BeatmapObjectBeatLine> __instance)
        {
            if (Managers.SpectrogramViewInstanceManager.Instance != null)
            {
                float currentBeat = levelStateRef(__instance).beat;
                Managers.SpectrogramViewInstanceManager.Instance.RefreshView(currentBeat - 5f, currentBeat + 16f, placementHelperRef(__instance));
            }
        }
    }
}
