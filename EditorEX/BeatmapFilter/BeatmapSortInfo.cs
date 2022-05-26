using BeatmapEditor3D.DataModels;

namespace EditorEX.BeatmapFilter
{
    internal class BeatmapSortInfo
    {
        public readonly int Points;
        public readonly IBeatmapInfoData BeatmapInfoData;

        public BeatmapSortInfo(int points, IBeatmapInfoData beatmapInfoData)
        {
            Points = points;
            BeatmapInfoData = beatmapInfoData;
        }
    }
}
