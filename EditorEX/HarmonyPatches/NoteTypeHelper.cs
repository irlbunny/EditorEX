using BeatmapEditor3D;
using EditorEX.Managers;
using HarmonyLib;
using UnityEngine;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteTypeHelper), "GetColorByNoteType")]
    internal class NoteTypeHelperGetColorByNoteType
    {
        private static bool Prefix(ref Color __result, NoteType type)
        {
            if (type == NoteType.NoteA || type == NoteType.NoteB)
            {
                if (Config.Instance.UseEnvironmentColors && ColorManagerInstanceManager.Instance != null)
                    __result = ColorManagerInstanceManager.Instance.ColorForType((ColorType) type);
                else
                {
                    if (type == NoteType.NoteA)
                        __result = Color.red;
                    else if (type == NoteType.NoteB)
                        __result = Color.blue;
                }
            }
            else
                __result = new(.25f, .25f, .25f, 1f);

            return false;
        }
    }
}
