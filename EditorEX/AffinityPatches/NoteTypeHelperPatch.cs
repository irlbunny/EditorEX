using BeatmapEditor3D;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.AffinityPatches
{
    internal class NoteTypeHelperPatch : IAffinity
    {
        private readonly Config _config;
        private readonly ColorManager _colorManager;

        public NoteTypeHelperPatch(Config config, ColorManager colorManager)
        {
            _config = config;
            _colorManager = colorManager;
        }

        [AffinityPatch(typeof(NoteTypeHelper), nameof(NoteTypeHelper.GetColorByNoteType)), AffinityPrefix]
        private bool GetColorByNoteType(ref Color __result, NoteType type)
        {
            if (_config.UseColorScheme && (type == NoteType.NoteA || type == NoteType.NoteB))
            {
                __result = _colorManager.ColorForType((ColorType) type);
                return false;
            }

            return true;
        }
    }
}
