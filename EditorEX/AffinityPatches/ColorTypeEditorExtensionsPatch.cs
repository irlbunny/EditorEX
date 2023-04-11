using BeatmapEditor3D;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.AffinityPatches
{
    internal class ColorTypeEditorExtensionsPatch : IAffinity
    {
        private readonly Config _config;
        private readonly ColorManager _colorManager;

        public ColorTypeEditorExtensionsPatch(Config config, ColorManager colorManager)
        {
            _config = config;
            _colorManager = colorManager;
        }

        [AffinityPatch(typeof(ColorTypeEditorExtensions), nameof(ColorTypeEditorExtensions.ToColor)), AffinityPrefix]
        private bool GetColorByNoteType(ref Color __result, ColorType colorType)
        {
            if (_config.UseColorScheme && (colorType == ColorType.ColorA || colorType == ColorType.ColorB))
            {
                __result = _colorManager.ColorForType(colorType);
                return false;
            }

            return true;
        }
    }
}
