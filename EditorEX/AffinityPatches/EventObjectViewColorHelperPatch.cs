using BeatmapEditor3D.Visuals;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.AffinityPatches
{
    internal class EventObjectViewColorHelperPatch : IAffinity
    {
        private readonly Config _config;
        private readonly ColorManager _colorManager;

        public EventObjectViewColorHelperPatch(Config config, ColorManager colorManager)
        {
            _config = config;
            _colorManager = colorManager;
        }

        [AffinityPatch(typeof(EventObjectViewColorHelper), nameof(EventObjectViewColorHelper.GetLightEventObjectColor)), AffinityPrefix]
        private bool GetColorByNoteType(ref Color __result, int value, float floatValue)
        {
            if (_config.UseColorScheme)
            {
                if (value == 0)
                {
                    __result = Color.gray;
                    return false;
                }
                var color = (value < 5) ? _colorManager.ColorForType(EnvironmentColorType.Color1, false) : ((value < 9) ?
                    _colorManager.ColorForType(EnvironmentColorType.Color0, false) :
                    _colorManager.ColorForType(EnvironmentColorType.ColorW, false));
                __result = Color.Lerp(Color.gray, color, Mathf.Lerp(.3f, 1f, Mathf.InverseLerp(0f, 1f, floatValue)));
                return false;
            }

            return true;
        }
    }
}
