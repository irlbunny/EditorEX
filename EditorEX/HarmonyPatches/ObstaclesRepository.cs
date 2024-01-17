using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using HarmonyLib;
using IntervalTree;
using System.Collections.Generic;

namespace EditorEX.HarmonyPatches
{
    // Fixes some exceptions with loading obstacles with negative duration.
    /*[HarmonyPatch(typeof(ObstaclesRepository), nameof(ObstaclesRepository.Add), typeof(ObstacleEditorData))]
    internal class ObstaclesRepositoryAdd
    {
        private static bool Prefix(
            ObstacleEditorData obstacle,
            IntervalTree<float, ObstacleEditorData> ____tree,
            IDictionary<BeatmapEditorObjectId, ObstacleEditorData> ____dictionary)
        {
            ____tree.Add(obstacle.beat, obstacle.beat + (obstacle.duration < 0 ? -obstacle.duration : obstacle.duration), obstacle);
            ____dictionary.Add(obstacle.id, obstacle);
            return false;
        }
    }*/
}
