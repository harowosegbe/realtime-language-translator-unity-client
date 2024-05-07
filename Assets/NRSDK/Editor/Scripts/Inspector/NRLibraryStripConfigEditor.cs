/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;

namespace NRKernal
{
    [CustomEditor(typeof(NRLibraryStripConfig))]
    public class NRLibraryStripConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            NRLibraryStripConfig stripConfig = (NRLibraryStripConfig)target;
            DrawStripConfig(stripConfig);
            EditorGUILayout.Space();
        }

        private void DrawStripConfig(NRLibraryStripConfig stripConfig)
        {
            EditorGUILayout.LabelField("NRSDK Library Strip", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            bool modify = false;
            NREditorUtility.BoolField(stripConfig, "StripBaseApi", ref stripConfig.StripBaseApi, ref modify);
            EditorGUILayout.Space();
            NREditorUtility.BoolField(stripConfig, "StripDualAgentTracking", ref stripConfig.StripDualAgentTracking, ref modify);
            EditorGUILayout.Space();
            NREditorUtility.BoolField(stripConfig, "StripHandTracking", ref stripConfig.StripHandTracking, ref modify);
            EditorGUILayout.Space();
            NREditorUtility.BoolField(stripConfig, "StripImageTracking", ref stripConfig.StripImageTracking, ref modify);
            EditorGUILayout.Space();
            NREditorUtility.BoolField(stripConfig, "StripMeshing", ref stripConfig.StripMeshing, ref modify);
            EditorGUILayout.Space();
            NREditorUtility.BoolField(stripConfig, "StripSpatialAnchor", ref stripConfig.StripSpatialAnchor, ref modify);
            EditorGUILayout.EndVertical();
            if (modify)
            {
                EditorUtility.SetDirty(stripConfig);
                ApplyStripConfig(stripConfig);
            }
        }

        private void ApplyStripConfig(NRLibraryStripConfig stripConfig)
        {
            SetExcludeAARFile("nr_api.aar", stripConfig.StripBaseApi);
            SetExcludeAARFile("nr_dual_agent_tracking.aar", stripConfig.StripDualAgentTracking);
            SetExcludeAARFile("nr_hand_tracking.aar", stripConfig.StripHandTracking);
            SetExcludeAARFile("nr_image_tracking.aar", stripConfig.StripImageTracking);
            SetExcludeAARFile("nr_meshing.aar", stripConfig.StripMeshing);
            SetExcludeAARFile("nr_spatial_anchor.aar", stripConfig.StripSpatialAnchor);
        }

        private void SetExcludeAARFile(string aarFileName, bool exclude)
        {
            if (ProjectHasAARResource(aarFileName, out string aarFilePath))
            {
                PluginImporter importer = (PluginImporter)AssetImporter.GetAtPath(aarFilePath);
                if (importer != null)
                {
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, !exclude);
                    importer.SaveAndReimport();
                }
            }
        }

        /// <summary>
        /// 查找工程中是否包含aar文件
        /// </summary>
        /// <param name="aarFileName"> aar文件名</param>
        /// <returns></returns>
        private bool ProjectHasAARResource(string aarFileName, out string filePath)
        {
            var fileNameWithOutExtension = Path.GetFileNameWithoutExtension(aarFileName);
            var guids = AssetDatabase.FindAssets(fileNameWithOutExtension);
            if (guids != null)
            {
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var fileName = Path.GetFileName(path);
                    if (fileName == aarFileName)
                    {
                        filePath = path;
                        return true;
                    }
                }
            }
            filePath = string.Empty;
            return false;
        }
    }
}