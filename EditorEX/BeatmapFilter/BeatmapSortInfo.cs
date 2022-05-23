using BeatmapEditor3D.DataModels;

namespace EditorEX.BeatmapFilter
{
    internal class BeatmapSortInfo
    {
        public readonly int Points;
        public readonly IBeatmapInfoData Data;

        public BeatmapSortInfo(int points, IBeatmapInfoData data)
        {
            Points = points;
            Data = data;
        }
    }
}
