using BeatmapEditor3D.Visuals;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.AffinityPatches
{
    internal class BeatmapObjectViewColorHelperPatch : IAffinity
    {
        private readonly Config _config;
        private readonly ColorManager _colorManager;

        public BeatmapObjectViewColorHelperPatch(Config config, ColorManager colorManager)
        {
            _config = config;
            _colorManager = colorManager;
        }

        [AffinityPatch(typeof(BeatmapObjectViewColorHelper), nameof(BeatmapObjectViewColorHelper.GetBeatmapObjectColor)), AffinityPrefix]
        private void GetBeatmapObjectColor(ref Color color)
        {
            // YUCKIE
            if (_config.UseColorScheme && color == Color.cyan)
                color = _colorManager.obstaclesColor.ColorWithAlpha(1f);
        }
    }
}
