using BeatmapEditor3D.DataModels;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapProjectManager), "ReplaceBeatmapLevelFromLatestSave")]
    internal class BeatmapProjectManagerReplaceBeatmapLevelFromLatestSave
    {
		static AccessTools.FieldRef<BeatmapProjectManager, bool> projectOpenedRef =
		AccessTools.FieldRefAccess<BeatmapProjectManager, bool>("_projectOpened");

		static AccessTools.FieldRef<BeatmapProjectManager, IBeatmapDataModel> dataModelRef =
		AccessTools.FieldRefAccess<BeatmapProjectManager, IBeatmapDataModel>("_beatmapDataModel");

		static AccessTools.FieldRef<BeatmapProjectManager, string> originalBeatmapProjectRef =
		AccessTools.FieldRefAccess<BeatmapProjectManager, string>("_originalBeatmapProject");

		static AccessTools.FieldRef<BeatmapProjectManager, string> workingBeatmapProjectRef =
		AccessTools.FieldRefAccess<BeatmapProjectManager, string>("_workingBeatmapProject");

		private static bool Prefix(BeatmapProjectManager __instance, BeatmapCharacteristicSO beatmapCharacteristic, BeatmapDifficulty beatmapDifficulty)
        {
			// Prevent crash when exiting level editor without having any saves present
			// Caused by entering a new difficulty and exiting without saving before placing any objects

			if (!projectOpenedRef(__instance))
			{
				return false; // Works the same as return, and prevents running the function
			}

			IDifficultyBeatmapSetData difficultyBeatmapSetData;
			if (!dataModelRef(__instance).difficultyBeatmapSets.TryGetValue(beatmapCharacteristic, out difficultyBeatmapSetData))
			{
				return false;
			}
			IDifficultyBeatmapData difficultyBeatmapData;
			if (!difficultyBeatmapSetData.difficultyBeatmaps.TryGetValue(beatmapDifficulty, out difficultyBeatmapData))
			{
				return false;
			}

			// Perform an additional check to make sure the level exists before attempting a copy
			if (!File.Exists(Path.Combine(originalBeatmapProjectRef(__instance), difficultyBeatmapData.beatmapFilename)))
            {
				return false;
            }

			BeatmapProjectFileHelper.CopyBeatmapLevel(originalBeatmapProjectRef(__instance), difficultyBeatmapData.beatmapFilename, workingBeatmapProjectRef(__instance), difficultyBeatmapData.beatmapFilename);

			// Replacing the whole function to avoid the transpiler
			return false;
		}
    }
}
