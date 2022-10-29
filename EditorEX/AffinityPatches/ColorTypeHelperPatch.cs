using BeatmapEditor3D;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.AffinityPatches
{
    internal class ColorTypeHelperPatch : IAffinity
    {
        private readonly Config _config;
        private readonly ColorManager _colorManager;

        public ColorTypeHelperPatch(Config config, ColorManager colorManager)
        {
            _config = config;
            _colorManager = colorManager;
        }

        [AffinityPatch(typeof(ColorTypeHelper), nameof(ColorTypeHelper.GetColorByColorType)), AffinityPrefix]
        private bool GetColorByColorType(ref Color __result, ColorType type)
        {
            if (_config.UseColorScheme && (type == ColorType.ColorA || type == ColorType.ColorB))
            {
                __result = _colorManager.ColorForType(type);
                return false;
            }

            return true;
        }
    }
}
