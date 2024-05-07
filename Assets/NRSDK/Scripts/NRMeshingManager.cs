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
    using System.Collections;
    using UnityEngine;

    /// <summary>
    ///  Interface for processing mesh information.
    /// </summary>
    public interface IMeshInfoProcessor
    {
        /// <summary>
        /// Update mesh information for a specific identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier of the mesh.</param>
        /// <param name="meshInfo">The mesh information.</param>
        void UpdateMeshInfo(ulong identifier, NRMeshInfo meshInfo);
        /// <summary>
        /// Clear all stored mesh information.
        /// </summary>
        void ClearMeshInfo();
    }

    /// <summary> Manages meshing operations for the NRSDK. </summary>
    public class NRMeshingManager : SingletonBehaviour<NRMeshingManager>
    {
        /// <summary> The radius for meshing. </summary>
        [SerializeField]
        private float m_MeshingRadius;
        /// <summary> The rate at which meshing updates are submitted. </summary>
        [SerializeField]
        private float m_MeshingSubmitRate;
        /// <summary> Array of mesh info processors. </summary>
        IMeshInfoProcessor[] m_MeshInfoProcessors;

        /// <summary> Native meshing component. </summary>
        private NativeMeshing m_NativeMeshing;
        /// <summary> Coroutine for meshing request operations. </summary>
        Coroutine m_MeshingCoroutine;
        /// <summary> Time counter for controlling mesh update rate. </summary>
        float m_MeshUpdateTime = 0;
        /// <summary> Predicate for mesh block state. </summary>
        Func<BlockInfo, bool> m_Predicate = new Func<BlockInfo, bool>(
            p => p.blockState != NRMeshingBlockState.NR_MESHING_BLOCK_STATE_UNCHANGED);

        /// <summary>
        /// Initializes meshing settings and components when the object starts.
        /// </summary>
        private void Start()
        {
            if (isDirty)
                return;
            NRSessionManager.Instance.NRHMDPoseTracker.OnModeChanged += (result) =>
            {
                if (result.success)
                {
                    if (m_NativeMeshing != null)
                    {
                        m_NativeMeshing.DestroyMeshInfo();
                        m_NativeMeshing = null;
                        foreach (var processor in m_MeshInfoProcessors)
                            processor.ClearMeshInfo();
                    }
                    StartMeshing();
                }
            };
            StartMeshing();
        }

        void StartMeshing()
        {
            if (NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode == TrackingType.Tracking6Dof)
            {
                NRDebugger.Info("[NRMeshingManager] Start");
                EnableMeshing();
                m_NativeMeshing = new NativeMeshing(NRSessionManager.Instance.NativeAPI);
                m_NativeMeshing.SetMeshingFlags(NRMeshingFlags.NR_MESHING_FLAGS_COMPUTE_NORMAL);
                m_NativeMeshing.SetMeshingRadius(m_MeshingRadius);
                m_MeshingSubmitRate = Mathf.Clamp(m_MeshingSubmitRate, 0.2f, 10f);
                m_NativeMeshing.SetMeshingSubmitRate(m_MeshingSubmitRate);
                m_MeshInfoProcessors = GetComponents<IMeshInfoProcessor>();
            }
        }

        /// <summary>
        /// Updates the meshing request process based on a specified submission rate.
        /// </summary>
        void Update()
        {
            if (m_NativeMeshing != null)
            {
                m_MeshUpdateTime += Time.deltaTime;
                if (m_MeshUpdateTime * m_MeshingSubmitRate >= 1)
                {
                    if (m_MeshingCoroutine == null)
                    {
                        RequestMeshing();
                        m_MeshUpdateTime = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Enables meshing functionality when the application resumes.
        /// </summary>
        /// <param name="pause"> Enables meshing functionality when the application resumes. </param>
        void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                EnableMeshing();
            }
        }

        /// <summary>
        /// Enable the meshing functionality.
        /// </summary>
        private void EnableMeshing()
        {
#if !UNITY_EDITOR
            NRSessionManager.Instance.NativeAPI.Configuration.SetMeshingEnabled(true);
#endif
        }

        /// <summary>
        /// Requests meshing information.
        /// </summary>
        void RequestMeshing()
        {
            if (m_MeshingCoroutine != null)
                StopCoroutine(m_MeshingCoroutine);
            m_MeshingCoroutine = StartCoroutine(RequestMeshInfoCoroutine());
        }

        /// <summary>
        /// Coroutine for requesting mesh information.
        /// </summary>
        IEnumerator RequestMeshInfoCoroutine()
        {
            NRDebugger.Info("[NRMeshingManager] Start RequestMeshInfoCoroutine");
            if (m_NativeMeshing.RequestMeshInfo(m_Predicate))
            {
                yield return m_NativeMeshing.GetMeshInfoData(ProcessMeshDetail);
            }
            m_NativeMeshing.DestroyMeshInfoRequest();
            m_MeshingCoroutine = null;
        }

        /// <summary>
        /// Processes mesh detail information.
        /// </summary>
        /// <param name="identifier">The mesh identifier.</param>
        /// <param name="meshInfo">The mesh data.</param>
        void ProcessMeshDetail(ulong identifier, NRMeshInfo meshInfo)
        {
            foreach (var processor in m_MeshInfoProcessors)
            {
                NRDebugger.Debug($"[{this.GetType()}] {nameof(ProcessMeshDetail)} {processor.GetType()} {meshInfo.state}");
                processor.UpdateMeshInfo(identifier,meshInfo);
            }
        }
    }
}
