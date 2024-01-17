using BeatmapEditor3D;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.AffinityPatches
{
    internal class ColorEventMarkerObjectPatch : IAffinity
    {
        private readonly Config _config;
        private readonly ColorManager _colorManager;

        public ColorEventMarkerObjectPatch(Config config, ColorManager colorManager)
        {
            _config = config;
            _colorManager = colorManager;
        }

        [AffinityPatch(typeof(ColorEventMarkerObject), "GetColorValue"), AffinityPrefix]
        private bool GetColorValue(ref Color __result, EnvironmentColorType colorType, float brightness)
        {
            if (_config.UseColorScheme)
            {
                var environmentColor = colorType switch
                {
                    EnvironmentColorType.Color0 => _colorManager.ColorForType(EnvironmentColorType.Color0, false),
                    EnvironmentColorType.Color1 => _colorManager.ColorForType(EnvironmentColorType.Color1, false),
                    EnvironmentColorType.ColorW => _colorManager.ColorForType(EnvironmentColorType.ColorW, false),
                    _ => Color.gray
                };
                __result = Color.Lerp(Color.gray, environmentColor, Mathf.Lerp(.3f, 1f, Mathf.Lerp(0f, 1f, brightness)));
                return false;
            }

            return true;
        }
    }
}
