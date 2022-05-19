using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using HarmonyLib;
using IntervalTree;
using IPA.Utilities;
using System.Collections.Generic;

namespace EditorEX.HarmonyPatches
{
    // Fixes some exceptions with loading obstacles with negative duration.
    [HarmonyPatch(typeof(ObstaclesRepository), "Add", typeof(ObstacleEditorData))]
    internal class ObstaclesRepositoryAdd
    {
        private static bool Prefix(ObstaclesRepository __instance, ObstacleEditorData obstacle)
        {
            var tree = __instance.GetField<IntervalTree<float, ObstacleEditorData>, ObstaclesRepository>("_tree");
            var dictionary = __instance.GetField<IDictionary<BeatmapEditorObjectId, ObstacleEditorData>, ObstaclesRepository>("_dictionary");
            tree.Add(obstacle.beat, obstacle.beat + (obstacle.duration < 0 ? -obstacle.duration : obstacle.duration), obstacle);
            dictionary.Add(obstacle.id, obstacle);
            return false;
        }
    }
}
