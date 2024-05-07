/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace NRKernal.NRExamples
{
    public class WirframeMeshComputor
    {
        static List<Color> wirframeColorList = new List<Color>() {
            new Color(0,0,1),
            new Color(0,1,0),
            new Color(1,0,0),
        };

        static List<Vector3> wireframeVertices = new List<Vector3>();
        static List<Vector3> wireframeNormals = new List<Vector3>();
        static List<Color> wireframeColors = new List<Color>();
        static List<int> wireframeTriangles = new List<int>();

        public static void ComputeWireframeMeshData(Vector3[] baseVertices, Vector3[] baseNormals, List<int> baseTriangles, Mesh mesh)
        {
            wireframeVertices.Clear();
            wireframeNormals.Clear();
            wireframeColors.Clear();
            wireframeTriangles.Clear();

            for (int i = 0; i < baseTriangles.Count; ++i)
            {
                int index = baseTriangles[i];
                wireframeVertices.Add(baseVertices[index]);
                wireframeNormals.Add(baseNormals[index]);
                wireframeTriangles.Add(i);
                wireframeColors.Add(wirframeColorList[i % 3]);
            }

            mesh.Clear();
            mesh.SetVertices(wireframeVertices);
            mesh.SetNormals(wireframeNormals);
            mesh.SetTriangles(wireframeTriangles, 0);
            mesh.SetColors(wireframeColors);
        }
    }
}
