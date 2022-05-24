using BeatmapEditor3D.Visuals;
using HarmonyLib;
using UnityEngine;

namespace EditorEX.AudioSpectrogram.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatGridContainer), "Enable")]
    internal class BeatGridContainerEnable
    {
        private static void Postfix(Transform ____currentBeatLineTransform)
        {
            if (SpectrogramView.Instance != null)
            {
                var localPosition = SpectrogramView.Instance.Container.transform.localPosition;
                localPosition.x = -____currentBeatLineTransform.localScale.x + -1f + Config.Instance.SpectrogramXOffset;
                SpectrogramView.Instance.Container.transform.localPosition = localPosition;
            }
        }
    }
}
