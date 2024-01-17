using BeatmapEditor3D;
using EditorEX.Utilities;
using HarmonyLib;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using IPA.Utilities;
using TMPro;
using EditorEX.BeatmapFilter;
using System.Linq;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapsListViewController), "DidActivate")]
    internal class BeatmapsListViewControllerDidActivate
    {
        public static TMP_InputField FilterInput { get; private set; }

        private static void Prefix(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                var filterSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("EditorEX.Resources.filter.png");
                FilterInput = UIUtil.CreateInputField(new(filterSprite), __instance.transform, new(-950f, 130f), new(400f, 40f), (value) =>
                {
                    __instance.Filter(value);
                });
            }
        }

        private static void Postfix(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
                __instance.Filter(FilterInput.text);
        }
    }

    [HarmonyPatch(typeof(BeatmapsListViewController), "HandleBeatmapsCollectionDataModelUpdated")]
    internal class BeatmapsListViewControllerHandleBeatmapsCollectionDataModelUpdated
    {
        private static bool Prefix(BeatmapsListViewController __instance)
        {
            __instance.Filter(BeatmapsListViewControllerDidActivate.FilterInput.text);
            return false;
        }
    }

    // Fixes index when selecting a filtered map.
    [HarmonyPatch(typeof(BeatmapsListViewController), "HandleBeatmapListTableViewOpenBeatmap")]
    internal class BeatmapsListViewControllerHandleBeatmapListTableViewOpenBeatmap
    {
        private static void Prefix(ref int idx, BeatmapsListTableView ____beatmapsListTableView, IReadonlyBeatmapCollectionDataModel ____beatmapsCollectionDataModel)
        {
            var filteredMaps = ____beatmapsListTableView.GetField<IReadOnlyList<IBeatmapInfoData>, BeatmapsListTableView>("_beatmapInfos");
            idx = ____beatmapsCollectionDataModel.beatmapInfos.ToList().IndexOf(filteredMaps[idx]);
        }
    }
}
