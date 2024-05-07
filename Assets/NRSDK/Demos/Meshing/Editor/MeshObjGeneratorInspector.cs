/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using System.IO;
using UnityEditor;
using UnityEngine;

namespace NRKernal.NRExamples
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MeshObjGenerator))]
    public class MeshObjGeneratorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Load Obj Mesh File"))
            {
                string path = EditorUtility.OpenFolderPanel("Load Obj Mesh File", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    if (directoryInfo.Exists)
                    {
                        foreach (var file in directoryInfo.EnumerateFiles("*.obj"))
                        {
                            StreamReader sr = File.OpenText(file.FullName);
                            string meshData = sr.ReadToEnd();
                            sr.Close();
                            Mesh mesh = MeshSaver.StringToMesh(meshData);
                            ulong.TryParse(file.Name.Substring(0, file.Name.IndexOf(".")), out ulong identifier);
                            (serializedObject.targetObject as IMeshInfoProcessor).UpdateMeshInfo(identifier, new NRMeshInfo { 
                                baseMesh = mesh,
                                state = NRMeshingBlockState.NR_MESHING_BLOCK_STATE_NEW
                            });
                        }
                    }
                }
            }
        }
    }
#endif
}
