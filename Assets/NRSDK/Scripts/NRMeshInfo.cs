/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal
{
    using UnityEngine;

    public class NRMeshInfo
    {
        /// <summary>
        /// The mesh identifier.
        /// </summary>
        public ulong identifier;
        /// <summary>
        /// The state of the meshing block.
        /// </summary>
        public NRMeshingBlockState state;
        /// <summary>
        /// The mesh data.
        /// </summary>
        public Mesh baseMesh;
        /// <summary>
        /// The semantic label for each vertex of the mesh block.
        /// </summary>
        public NRMeshingVertexSemanticLabel[] labels;
        /// <summary>
        /// The count of vertices of the mesh.
        /// </summary>
        public int VertexCount => baseMesh.vertexCount;
    }
}
