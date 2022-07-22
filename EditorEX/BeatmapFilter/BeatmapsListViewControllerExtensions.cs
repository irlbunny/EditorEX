using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EditorEX.BeatmapFilter
{
    internal static class BeatmapsListViewControllerExtensions
    {
        private static List<IBeatmapInfoData> Filter(List<IBeatmapInfoData> beatmapInfos, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return beatmapInfos;

            var terms = text.Split(' ');
            var beatmapSortInfos = new List<BeatmapSortInfo>();

            foreach (var beatmapInfo in beatmapInfos)
            {
                var points = 0;

                for (var i = 0; i < terms.Length; i++)
                {
                    var term = terms[i];
                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        if (beatmapInfo.songSubName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 1;

                        if (beatmapInfo.songAuthorName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 3;

                        if (beatmapInfo.levelAuthorName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 4;

                        if (beatmapInfo.songName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 5;
                    }
                }

                if (points > 1)
                    beatmapSortInfos.Add(new(points, beatmapInfo));
            }

            beatmapSortInfos.Sort((x, y) => y.Points.CompareTo(x.Points));

            return beatmapSortInfos.Select((info) => info.BeatmapInfoData).ToList();
        }

        public static void Filter(this BeatmapsListViewController instance, string text)
        {
            var beatmapsCollectionDataModel = instance.GetField<IReadonlyBeatmapCollectionDataModel, BeatmapsListViewController>("_beatmapsCollectionDataModel");
            var filteredMaps = Filter(beatmapsCollectionDataModel.beatmapInfos.ToList(), text);
            instance.GetField<BeatmapsListTableView, BeatmapsListViewController>("_beatmapsListTableView").SetData(filteredMaps);
        }
    }
}
