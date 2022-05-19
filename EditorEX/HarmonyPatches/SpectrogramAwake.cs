using EditorEX.Utilities;
using HarmonyLib;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(Spectrogram), "Awake")]
    internal class SpectrogramAwake
    {
        private static void Prefix(Spectrogram __instance)
        {
            if (Config.Instance.HideEnvironmentSpectrograms && SceneUtil.IsInBeatmapEditor())
                __instance.gameObject.SetActive(false);
        }
    }
}
