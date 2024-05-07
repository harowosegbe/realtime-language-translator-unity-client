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
    using AOT;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;


    /// <summary>
    /// NRNativeRender operate rendering-related things, provides the feature of optimized rendering
    /// and low latency. </summary>
    public class NRRenderer : MonoBehaviour
    {
        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);

        private const int STARTNATIVERENDEREVENT = 0x0002;
        private const int RESUMENATIVERENDEREVENT = 0x0003;
        private const int PAUSENATIVERENDEREVENT = 0x0004;
        private const int STOPNATIVERENDEREVENT = 0x0005;

        public enum Eyes
        {
            /// <summary> Left Display. </summary>
            Left = 0,
            /// <summary> Right Display. </summary>
            Right = 1,
            Count = 2
        }

        /// <value> The native renderring. </value>
        internal static NativeRenderring NativeRenderring
        {
            get
            {
                return NRSessionManager.Instance.NativeAPI.NativeRenderring;
            }
        }
        
        /// <summary> Values that represent renderer states. </summary>
        public enum RendererState
        {
            UnInitialized,
            Initialized,
            Running,
            Paused,
            Destroyed
        }

        /// <summary> The current state. </summary>
        private RendererState m_CurrentState = RendererState.UnInitialized;
        /// <summary> Gets the current state. </summary>
        /// <value> The current state. </value>
        public RendererState CurrentState
        {
            get
            {
                return m_CurrentState;
            }
            set
            {
                m_CurrentState = value;
            }
        }

        public void Create()
        {
            NRDebugger.Info("[NRRender] Create");
#if !UNITY_EDITOR
            NativeRenderring.Create();
#endif
            NRDebugger.Info("[NRRender] Created");
        }

        /// <summary> Start the render pipleline. </summary>
        public void Start()
        {
            NRDebugger.Info("[NRRender] Start");
            if (m_CurrentState != RendererState.UnInitialized)
            {
                return;
            }

            m_CurrentState = RendererState.Initialized;
            GL.IssuePluginEvent(RenderThreadHandlePtr, STARTNATIVERENDEREVENT);
        }

        /// <summary> Pause render. </summary>
        public void Pause()
        {
            NRDebugger.Info("[NRRender] Pause");
            if (m_CurrentState != RendererState.Running)
            {
                return;
            }
            GL.IssuePluginEvent(RenderThreadHandlePtr, PAUSENATIVERENDEREVENT);
        }

        /// <summary> Resume render. </summary>
        public void Resume()
        {
            Invoke("DelayResume", 0.3f);
        }

        /// <summary> Delay resume. </summary>
        private void DelayResume()
        {
            NRDebugger.Info("[NRRender] Resume");
            if (m_CurrentState != RendererState.Paused)
            {
                return;
            }
            GL.IssuePluginEvent(RenderThreadHandlePtr, RESUMENATIVERENDEREVENT);
        }

        /// <param name="distance"> The distance from plane to center camera.</param>
        [Obsolete("Use NRFrame.SetFocusDistance instead", false)]
        public void SetFocusDistance(float distance)
        {
            NRFrame.SetFocusDistance(distance);
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            NRDebugger.Info("[NRRender] RunOnRenderThread : eventID={0}", eventID);

            if (eventID == STARTNATIVERENDEREVENT)
            {
                NativeRenderring?.Start();
                NRSessionManager.Instance.NRSwapChainMan.NRRenderer.CurrentState = RendererState.Running;
            }
            else if (eventID == RESUMENATIVERENDEREVENT)
            {
                NativeRenderring?.Resume();
                NRSessionManager.Instance.NRSwapChainMan.NRRenderer.CurrentState = RendererState.Running;
            }
            else if (eventID == PAUSENATIVERENDEREVENT)
            {
                NRSessionManager.Instance.NRSwapChainMan.NRRenderer.CurrentState = RendererState.Paused;
                NativeRenderring?.Pause();
            }
        }

        public void Destroy()
        {
            if (m_CurrentState == RendererState.Destroyed || m_CurrentState == RendererState.UnInitialized)
            {
                return;
            }

            // NRDebugger.Info("[NRRender] Destroy, issue event");
            // GL.IssuePluginEvent(RenderThreadHandlePtr, STOPNATIVERENDEREVENT);

            NRDebugger.Info("[NRRender] Destroy");
            m_CurrentState = RendererState.Destroyed;
            NativeRenderring?.Stop();
            NativeRenderring?.Destroy();
            NRDebugger.Info("[NRRender] Destroyed");
        }

        private void OnDestroy()
        {
            this.Destroy();
        }
    }
}
