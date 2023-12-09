using System.Collections.Generic;
using System.Globalization;
using Configuration;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Splines;
using UnityEngine.Timeline;

namespace Gameplay.Level.Editor
{
    [UsedImplicitly]
    [CustomTimelineEditor(typeof(BurstSignalEmitter))]
    public class BurstSignalEmitterEditor : MarkerEditor
    {
        private static readonly List<Vector3> reusablePointsList = new List<Vector3>();
        
        public override void DrawOverlay(IMarker marker, MarkerUIStates uiState, MarkerOverlayRegion region)
        {
            BurstSignalEmitter emitter = (BurstSignalEmitter)marker;
            Rect markerRegion = region.markerRegion;
            
            DrawEnemyIcon(markerRegion, emitter);
            DrawEnemyPath(markerRegion, emitter);
            DrawTimeCalculation(markerRegion, emitter);
        }

        private static void DrawTimeCalculation(Rect markerRegion, BurstSignalEmitter emitter)
        {
            Rect newRect = markerRegion;
            newRect.size *= 5.0f;
            newRect.center = markerRegion.position;
            
            
            string timeString = ((emitter.parameter.enemyAmount - 1) * emitter.parameter.enemySpawnGap).ToString(CultureInfo.InvariantCulture);
            GUI.contentColor = Color.yellow;
            GUI.Label(newRect, timeString);
        }

        private static void DrawEnemyIcon(Rect markerRegion, BurstSignalEmitter emitter)
        {
            Rect newRect = markerRegion;
            newRect.size *= 10.0f;
            newRect.center = markerRegion.center - Vector2.down * 20.0f;
            EnemyRegistry registry = AssetDatabase.LoadAssetAtPath<EnemyRegistry>("Assets/Configs/Enemy Registry.asset");
            GUI.DrawTexture(newRect, registry.EnemyTypes[emitter.parameter.enemyId].Icon.texture, ScaleMode.ScaleToFit);
        }

        private static void DrawEnemyPath(Rect markerRegion, BurstSignalEmitter emitter)
        {
            SplineContainer paths = AssetDatabase.LoadAssetAtPath<SplineContainer>("Assets/Prefabs/EnemyPaths.prefab");
            Spline path = paths.Splines[emitter.parameter.pathId];

            Rect newRegion = markerRegion;
            newRegion.height *= 5.0f;
            newRegion.width *= 5.0f;
            
            Handles.color = Color.red;
            
            Vector3 offset = (Vector3)newRegion.center
                             - Vector3.down * newRegion.height * 0.25f
                             + Vector3.right * newRegion.width * 0.5f;
            
            reusablePointsList.Clear();
            
            for (int i = 0; i < path.Count; i++)
            {
                reusablePointsList.Add((Vector3)path[i].Position + offset);
            }
            
            Handles.DrawAAPolyLine(reusablePointsList.ToArray());
            
            Handles.color = Color.green;
            Handles.DrawWireCube(newRegion.center, newRegion.size);
        }
    }
}