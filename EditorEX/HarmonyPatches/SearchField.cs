using BeatmapEditor3D;
using EditorEX.Utilities;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using System;
using System.Linq;
using IPA.Utilities;
using BeatSaberMarkupLanguage;
using TMPro;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapsListViewController), "DidActivate")]
    internal class SearchField
    {
        internal static TMP_InputField searchInput;

        internal class BeatmapSortInfo
        {
            internal int points;
            internal IBeatmapInfoData data;

            public BeatmapSortInfo(int points, IBeatmapInfoData data)
            {
                this.points = points;
                this.data = data;
            }
        }
        internal static List<IBeatmapInfoData> FilterMaps(List<IBeatmapInfoData> input, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return input;
            string[] texts = text.Split(' ');
            List<BeatmapSortInfo> sortInfos = new List<BeatmapSortInfo>();

            foreach (var beatmapInfo in input)
            {
                int points = 0;

                for (int i = 0; i < texts.Length; i++)
                {
                    string term = texts[i];
                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        if (beatmapInfo.songName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            points += 5;
                        }

                        if (beatmapInfo.levelAuthorName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            points += 4;
                        }

                        if (beatmapInfo.songAuthorName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            points += 3;
                        }

                        if (beatmapInfo.songSubName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            points += 1;
                        }
                    }
                }
                if(points > 1)
                {
                    sortInfos.Add(new BeatmapSortInfo(points, beatmapInfo));
                }
            }
            sortInfos.Sort((x, y) => y.points.CompareTo(x.points));
            return sortInfos.Select((BeatmapSortInfo info) => { return info.data; }).ToList();
        }
        internal static void Filter(BeatmapsListViewController instance, string text)
        {
            IReadonlyBeatmapCollectionDataModel collectionDataModel = instance.GetField<IReadonlyBeatmapCollectionDataModel, BeatmapsListViewController>("_beatmapsCollectionDataModel");
            var maps = FilterMaps(collectionDataModel.beatmapInfos.ToList(), text);
            instance.GetField<BeatmapsListTableView, BeatmapsListViewController>("_beatmapsListTableView").SetData(maps);
        }

        private static void Prefix(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                searchInput = UIUtils.CreateInputField(new UIUtils.Label(BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("EditorEX.Assets.icon.png")), __instance.transform, "", new Vector2(-862f, 130f), new Vector2(400f, 40f), (string value) =>
                {
                    Filter(__instance, value);
                }
                );
            }
        }

        private static void Postfix(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
            {
                Filter(__instance, searchInput.text);
            }
        }
    }

    [HarmonyPatch(typeof(BeatmapsListViewController), "HandleBeatmapListTableViewOpenBeatmap")]
    internal class FixMapSelectionIdx
    {

        private static void Prefix(BeatmapsListViewController __instance, ref int idx)
        {
            IReadonlyBeatmapCollectionDataModel collectionDataModel = __instance.GetField<IReadonlyBeatmapCollectionDataModel, BeatmapsListViewController>("_beatmapsCollectionDataModel");
            var ogMaps = collectionDataModel.beatmapInfos;
            //Just for the record I hate reflection.
            var filteredMaps = __instance.GetField<BeatmapsListTableView, BeatmapsListViewController>("_beatmapsListTableView").GetField<IReadOnlyList<IBeatmapInfoData>, BeatmapsListTableView>("_beatmapInfos");

            idx = ogMaps.IndexOf(filteredMaps[idx]);
        }
    }

    [HarmonyPatch(typeof(BeatmapsListViewController), "HandleBeatmapsCollectionDataModelUpdated")]
    internal class FixMapRefresh
    {
        private static bool Prefix(BeatmapsListViewController __instance)
        {
            SearchField.Filter(__instance, SearchField.searchInput.text);
            return false;
        }
    }
}
