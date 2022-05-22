using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using EditorEX.Managers;
using BeatmapEditor3D.DataModels;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapEditor3D.ToggleZenModeCommand), "Execute")]
    internal class ToggleZenModeCommand
    {
        static AccessTools.FieldRef<BeatmapEditor3D.ToggleZenModeCommand, ILevelEditorState> editorStateRef =
        AccessTools.FieldRefAccess<BeatmapEditor3D.ToggleZenModeCommand, ILevelEditorState>("_levelEditorState");

        private static void Prefix(BeatmapEditor3D.ToggleZenModeCommand __instance)
        {
            if (SpectrogramViewInstanceManager.Instance != null)
            {
                // Hides the spectrogram when zen mode is enabled
                SpectrogramViewInstanceManager.Instance.SetVisible(editorStateRef(__instance).zenMode);
            }
        }
    }
}
