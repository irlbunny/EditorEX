using BeatmapEditor3D.DataModels;
using HarmonyLib;
using System.IO;

namespace EditorEX.HarmonyPatches
{
    // Prevents a crash when exiting level editor without having any saves present.
    // Caused by entering a new difficulty and exiting without saving before placing any objects.
    [HarmonyPatch(typeof(BeatmapProjectManager), nameof(BeatmapProjectManager.ReplaceBeatmapLevelFromLatestSave))]
    internal class BeatmapProjectManagerReplaceBeatmapLevelFromLatestSave
    {
        private static bool Prefix(BeatmapCharacteristicSO beatmapCharacteristic, BeatmapDifficulty beatmapDifficulty,
            bool ____projectOpened, IBeatmapDataModel ____beatmapDataModel,
            string ____originalBeatmapProject, string ____workingBeatmapProject)
        {
            if (!____projectOpened)
                return false;

            IDifficultyBeatmapSetData difficultyBeatmapSetData;
            if (!____beatmapDataModel.difficultyBeatmapSets.TryGetValue(beatmapCharacteristic, out difficultyBeatmapSetData))
                return false;
            IDifficultyBeatmapData difficultyBeatmapData;
            if (!difficultyBeatmapSetData.difficultyBeatmaps.TryGetValue(beatmapDifficulty, out difficultyBeatmapData))
                return false;

            // Perform an additional check to make sure the level exists before attempting a copy.
            if (!File.Exists(Path.Combine(____originalBeatmapProject, difficultyBeatmapData.beatmapFilename)))
                return false;

            BeatmapProjectFileHelper.CopyBeatmapLevel(____originalBeatmapProject, difficultyBeatmapData.beatmapFilename, ____workingBeatmapProject, difficultyBeatmapData.beatmapFilename);
            return false;
        }
    }
}
