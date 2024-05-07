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
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Collections;

    /// <summary> Meshing Native API. </summary>
    internal class NativeMeshing
    {
        /// <summary> The native interface. </summary>
        private NativeInterface m_NativeInterface;
        public UInt64 PerceptionHandle
        {
            get
            {
                return m_NativeInterface.PerceptionHandle;
            }
        }

        /// <summary> Handle of mesh info request. </summary>
        private UInt64 m_MeshBlockListHandle = 0;

        /// <summary> Dictionary contains the result of GetBlockInfoData. </summary>
        private Dictionary<ulong, BlockInfo> m_BlockInfos = new Dictionary<ulong, BlockInfo>();

        public NativeMeshing(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
        }

        /// <summary> Set flags which mesh runtime will use. </summary>
        /// <param name="flags"> Flags that are a combination of NRMeshingFlags. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SetMeshingFlags(NRMeshingFlags flags)
        {
            if (PerceptionHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRMeshingSetFlags Zero PerceptionHandle.");
                return false;
            }
            var result = NativeApi.NRMeshingSetFlags(PerceptionHandle, flags);
            NRDebugger.Debug("[NativeMeshing] NRMeshingSetFlags result: {0} flag: {1}.", result, flags);
            NativeErrorListener.Check(result, this, "SetMeshingFlags");
            return result == NativeResult.Success;
        }

        /// <summary> Set radius which mesh runtime will use for environment perception. </summary>
        /// <param name="radius"> Radius which runtime will use for environment perception. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SetMeshingRadius(float radius)
        {
            if (PerceptionHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRMeshingSetRadius Zero PerceptionHandle.");
                return false;
            }
            var result = NativeApi.NRMeshingSetRadius(PerceptionHandle, radius);
            NRDebugger.Debug("[NativeMeshing] NRMeshingSetRadius result: {0} radius: {1}.", result, radius);
            NativeErrorListener.Check(result, this, "SetMeshingRadius");
            return result == NativeResult.Success;
        }

        /// <summary> Set the rate of which meshing data submits. </summary>
        /// <param name="submit_rate"> The rate at which the meshing data will submit. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SetMeshingSubmitRate(float submit_rate)
        {
            if (PerceptionHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRMeshingSetSubmitRate Zero PerceptionHandle.");
                return false;
            }
            var result = NativeApi.NRMeshingSetSubmitRate(PerceptionHandle, submit_rate);
            NRDebugger.Debug("[NativeMeshing] NRMeshingSetSubmitRate result: {0} submit_rate: {1}.", result, submit_rate);
            NativeErrorListener.Check(result, this, "SetMeshingSubmitRate");
            return result == NativeResult.Success;
        }

        /// <summary>
        /// Request mesh info which includes state and bounding extents of the block.
        /// </summary>
        /// <param name="predicate"> The search function of block infos. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool RequestMeshInfo(Func<BlockInfo, bool> predicate = null)
        {
            if (PerceptionHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRPerceptionObjectListCreate Zero PerceptionHandle.");
                return false;
            }
            if (m_MeshBlockListHandle != 0)
            {
                DestroyMeshInfoRequest();
            }
            if (m_BlockInfos.Count != 0)
            {
                DestroyMeshInfo();
            }
            var result = NativeApi.NRPerceptionObjectListCreate(PerceptionHandle, ref m_MeshBlockListHandle);
            NRDebugger.Debug("[NativeMeshing] NRMeshingRequestMeshInfo result: {0} Handle: {1}.", result, m_MeshBlockListHandle);
            result = NativeApi.NRPerceptionUpdateMeshingBlock(PerceptionHandle, m_MeshBlockListHandle);
            NRDebugger.Debug("[NativeMeshing] NRPerceptionUpdateMeshingBlock result: {0} Handle: {1}.", result, m_MeshBlockListHandle);
            uint blockListSize = 0;
            result = NativeApi.NRPerceptionObjectListGetSize(PerceptionHandle, m_MeshBlockListHandle, ref blockListSize);
            NRDebugger.Debug("[NativeMeshing] NRPerceptionObjectListGetSize result: {0} blockListSize: {1}.", result, blockListSize);
            for (int i = 0; i < blockListSize; i++)
            {
                UInt64 meshing_block_handle = 0;
                result = NativeApi.NRPerceptionObjectListAcquireItem(PerceptionHandle, m_MeshBlockListHandle, i, ref meshing_block_handle);
                NRDebugger.Debug("[NativeMeshing] NRPerceptionObjectListAcquireItem result: {0} handle: {1}.", result, meshing_block_handle);
                NRMeshingFlags meshingFlag = NRMeshingFlags.NR_MESHING_FLAGS_NULL;
                result = NativeApi.NRMeshingBlockGetFlags(PerceptionHandle, meshing_block_handle, ref meshingFlag);
                NRDebugger.Debug("[NativeMeshing] NRMeshingBlockGetFlags result: {0} meshingFlag: {1}.", result, meshingFlag);
                BlockInfo blockInfo = new BlockInfo();
                result = NativeApi.NRMeshingBlockGetIdentifier(PerceptionHandle, meshing_block_handle, ref blockInfo.identifier);
                NRDebugger.Debug("[NativeMeshing] NRMeshingBlockGetIdentifier result: {0} identifier: {1}.", result, blockInfo.identifier);
                result = NativeApi.NRMeshingBlockGetTimestamp(PerceptionHandle, meshing_block_handle, ref blockInfo.timestamp);
                NRDebugger.Debug("[NativeMeshing] NRMeshingBlockGetTimestamp result: {0} timestamp: {1}.", result, blockInfo.timestamp);
                result = NativeApi.NRMeshingBlockGetState(PerceptionHandle, meshing_block_handle, ref blockInfo.blockState);
                NRDebugger.Debug("[NativeMeshing] NRMeshingBlockGetState result: {0} blockState: {1}.", result, blockInfo.blockState);
                if (predicate(blockInfo))
                {
                    m_BlockInfos.Add(meshing_block_handle, blockInfo);
                }
                else
                {
                    NativeApi.NRMeshingBlockDestroy(PerceptionHandle, meshing_block_handle);
                }
            }
            return m_BlockInfos.Count != 0;
        }

        /// <summary>
        /// Destroy the mesh info handle.
        /// </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool DestroyMeshInfo()
        {
            if (PerceptionHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRMeshInfoDestroy Zero PerceptionHandle.");
                return false;
            }

            if (m_BlockInfos.Count != 0)
            {
                foreach (var blockInfo in m_BlockInfos)
                {
                    NativeResult result = NativeApi.NRMeshingBlockDestroy(PerceptionHandle, blockInfo.Key);
                    NativeErrorListener.Check(result, this, "DestroyMeshInfo");
                }
                m_BlockInfos.Clear();
            }
            return true;
        }

        /// <summary>
        ///  Get block detail data in request
        /// </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public IEnumerator GetMeshInfoData(Action<ulong, NRMeshInfo> action)
        {
            if (PerceptionHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRMeshDetailGetBlockDetailCount Zero MeshingHandle.");
                yield break;
            }

            foreach (var item in m_BlockInfos)
            {
                uint vertexCount = 0;
                var result = NativeApi.NRMeshingBlockGetVertexCount(PerceptionHandle, item.Key, ref vertexCount);
                NRDebugger.Debug("[NativeMeshing] NRBlockDetailGetVertexCount result: {0} vertexCount: {1}.", result, vertexCount);
                if (vertexCount != 0)
                {
                    NativeVector3f[] outVertices = new NativeVector3f[vertexCount];
                    result = NativeApi.NRMeshingBlockGetVertices(PerceptionHandle, item.Key, outVertices);
                    NRDebugger.Debug("[NativeMeshing] NRBlockDetailGetVertices result: {0}.", result);
                    NativeVector3f[] outNormals = new NativeVector3f[vertexCount];
                    result = NativeApi.NRMeshingBlockGetNormals(PerceptionHandle, item.Key, outNormals);
                    NRDebugger.Debug("[NativeMeshing] NRBlockDetailGetNormals result: {0}.", result);
                    NRMeshingVertexSemanticLabel[] outLabels = new NRMeshingVertexSemanticLabel[vertexCount];
                    result = NativeApi.NRMeshingBlockGetLabels(PerceptionHandle, item.Key, outLabels);
                    NRDebugger.Debug("[NativeMeshing] NRMeshingBlockGetLabels result: {0}.", result);
                    uint indexCount = 0;
                    result = NativeApi.NRMeshingBlockGetIndexCount(PerceptionHandle, item.Key, ref indexCount);
                    NRDebugger.Debug("[NativeMeshing] NRBlockDetailGetIndexCount result: {0} indexCount: {1}.", result, indexCount);
                    uint[] outIndex = new uint[indexCount];
                    result = NativeApi.NRMeshingBlockGetIndeices(PerceptionHandle, item.Key, outIndex);
                    NRDebugger.Debug("[NativeMeshing] NRBlockDetailGetIndeices result: {0}.", result);

                    Vector3[] vertices = new Vector3[vertexCount];
                    Vector3[] normals = new Vector3[vertexCount];
                    for (int j = 0; j < vertexCount; j++)
                    {
                        vertices[j] = outVertices[j].ToUnityVector3();
                        normals[j] = outNormals[j].ToUnityVector3();
                    }
                    int[] triangles = new int[indexCount];
                    for (int j = 0; j < indexCount; j++)
                    {
                        triangles[j] = (int)outIndex[j];
                    }
                    Mesh mesh = new Mesh
                    {
                        vertices = vertices,
                        normals = normals,
                        triangles = triangles
                    };
                    
                    mesh.RecalculateBounds();

                    NRMeshInfo meshInfo = new NRMeshInfo { 
                        identifier = item.Value.identifier,
                        state = item.Value.blockState,
                        baseMesh = mesh,
                        labels = outLabels
                    };
                    action?.Invoke(meshInfo.identifier, meshInfo);
                    NRDebugger.Debug("[NativeMeshing] GetMeshDetailData Invoke: {0} {1} {2}.", item.Value.identifier, item.Value.blockState, mesh.vertexCount);
                }
                result = NativeApi.NRMeshingBlockDestroy(PerceptionHandle, item.Key);
                NRDebugger.Debug("[NativeMeshing] NRBlockDetailDestroy result: {0}.", result);
                yield return null;
            }
        }

        /// <summary>
        /// Destroy the request handle.
        /// </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool DestroyMeshInfoRequest()
        {
            if (PerceptionHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRPerceptionObjectListDestroy Zero MeshingHandle.");
                return false;
            }
            if (m_MeshBlockListHandle == 0)
            {
                NRDebugger.Warning("[NativeMeshing] NRPerceptionObjectListDestroy Zero RequestMeshDetailHandle.");
                return true;
            }
            NativeResult result = NativeApi.NRPerceptionObjectListDestroy(PerceptionHandle, m_MeshBlockListHandle);
            NRDebugger.Debug("[NativeMeshing] NRPerceptionObjectListDestroy.");
            m_MeshBlockListHandle = 0;
            return result == NativeResult.Success;
        }

        private partial struct NativeApi
        {
            /// <summary> Set flags which mesh runtime will use. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="flags"> Flags that are a combination of NRMeshingFlags. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingSetFlags(UInt64 perception_handle, NRMeshingFlags flags);

            /// <summary> Get flags which mesh runtime is currently used. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="out_flags"> Flags that are a combination of NRMeshingFlags. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingGetFlags(UInt64 perception_handle, ref NRMeshingFlags out_flags);

            /// <summary> Set radius which mesh runtime will use for environment perception. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="radius"> Radius which runtime will use for environment perception. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingSetRadius(UInt64 perception_handle, float radius);

            /// <summary> Get radius which mesh runtime will use for environment perception. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="out_radius"> Radius which runtime will use for environment perception. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingGetRadius(UInt64 perception_handle, ref float out_radius);

            /// <summary> Set the rate of which meshing data submits. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="submit_rate"> The rate at which the meshing data will submit. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingSetSubmitRate(UInt64 perception_handle, float submit_rate);

            /// <summary> Get the rate of which meshing data submits. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="out_submit_rate"> The rate at which the meshing data will submit. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingGetSubmitRate(UInt64 perception_handle, ref float out_submit_rate);

            /// <summary> Create an empty perception object list. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="out_perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListCreate(UInt64 perception_handle, ref UInt64 out_perception_object_list_handle);

            /// <summary> Create an empty perception object list. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionUpdateMeshingBlock(UInt64 perception_handle, UInt64 perception_object_list_handle);

            /// <summary> Release memory used by the perception object list object. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListDestroy(UInt64 perception_handle, UInt64 perception_object_list_handle);

            /// <summary> Get the perception object list size. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <param name="out_list_size"> The size of perception_object_list. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListGetSize(UInt64 perception_handle, UInt64 perception_object_list_handle, ref uint out_list_size);

            /// <summary> Get the element of perception object list by index. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <param name="index"> Index of elements of perception object list. </param>
            /// <param name="out_perception_object"> The perception object element in the list. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListAcquireItem(UInt64 perception_handle, UInt64 perception_object_list_handle, int index, ref UInt64 out_perception_object);

            /// <summary> Get response timestamp (in nano seconds) to a earlier request. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_hmd_time_nanos"> The timestamp in nano seconds. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetTimestamp(UInt64 perception_handle, UInt64 meshing_block_handle, ref ulong out_hmd_time_nanos);

            /// <summary> Get block identifier. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_block_identifier"> The identifier of block. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetIdentifier(UInt64 perception_handle, UInt64 meshing_block_handle, ref ulong out_block_identifier);

            /// <summary> Get the state of block. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_block_state"> The state of block. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetState(UInt64 perception_handle, UInt64 meshing_block_handle, ref NRMeshingBlockState out_block_state);

            /// <summary> Get block flags which mesh block took place. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_flags"> Flags that are a combination of NRMeshingFlags. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetFlags(UInt64 perception_handle, UInt64 meshing_block_handle, ref NRMeshingFlags out_flags);

            /// <summary> Get the number of vertices in vertex/normal buffer. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_vertex_count">  Number of elements in buffer. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetVertexCount(UInt64 perception_handle, UInt64 meshing_block_handle, ref uint out_vertex_count);

            /// <summary> Get the pointer to vertex buffer. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_vertices"> Pointer to vertex buffer. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetVertices(UInt64 perception_handle, UInt64 meshing_block_handle, NativeVector3f[] out_vertices);

            /// <summary> Get the pointer to normal buffer. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_normals"> Pointer to normal buffer. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetNormals(UInt64 perception_handle, UInt64 meshing_block_handle, NativeVector3f[] out_normals);

            /// <summary> Get the number of elements in face-vertex-index buffer. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_face_vertex_index_count"> Number of elements in buffer. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetIndexCount(UInt64 perception_handle, UInt64 meshing_block_handle, ref uint out_face_vertex_index_count);

            /// <summary>
            /// Get the pointer to face-vertex-index buffer.
            /// In the buffer, each element is a index to vertex buffer.
            /// Three index elements will define one triangle.
            /// For example: the first triangle is: vertex[index[0]], vertex[index[1]], vertex[index[2]].
            /// The second triangle is: vertex[index[3]], vertex[index[4]], vertex[index[5]].
            /// All faces are listed back-to-back in counter-clockwise vertex order.
            /// </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <param name="out_face_vertices_index"> Pointer of face-vertex-index buffer. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetIndeices(UInt64 perception_handle, UInt64 meshing_block_handle, uint[] out_face_vertices_index);

            /// <summary>
            /// Get the pointer to vertex-label buffer.
            /// In the buffer, each element is a semantic label to vertex buffer.
            /// </summary>
            /// <param name="perception_handle"></param>
            /// <param name="meshing_block_handle"></param>
            /// <param name="out_vertices_labels"></param>
            /// <returns></returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockGetLabels(UInt64 perception_handle, UInt64 meshing_block_handle, NRMeshingVertexSemanticLabel[] out_vertices_labels);

            /// <summary> Destroy the block detail handle. </summary>
            /// <param name="perception_handle"> The handle of of perception object. </param>
            /// <param name="meshing_block_handle"> The handle of meshing block. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMeshingBlockDestroy(UInt64 perception_handle, UInt64 meshing_block_handle);
        };
    }
}
