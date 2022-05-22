using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Views;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(EditBeatmapLevelViewController), "DidActivate")]
    internal class EditBeatmapLevelViewControllerDidActivate
    {
        public static SpectrogramView spectrogramView { get; private set; }

        static AccessTools.FieldRef<EditBeatmapLevelViewController, IBeatmapDataModel> dataModelRef =
        AccessTools.FieldRefAccess<EditBeatmapLevelViewController, IBeatmapDataModel>("_beatmapDataModel");

        private static void Prefix(EditBeatmapLevelViewController __instance)
        {
            if (!Config.Instance.ShowSpectrogram)
            {
                return; // Do not show the spectrogram
            }
            // Create spectrogram instance and store it
            if (Managers.SpectrogramViewInstanceManager.Instance != null)
            {
                Managers.SpectrogramViewInstanceManager.Instance.CleanUp();
            }
            new Managers.SpectrogramViewInstanceManager(new SpectrogramView(dataModelRef(__instance).audioClip));
        }
    }
}
