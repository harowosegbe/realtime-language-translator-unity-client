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
#if UNITY_STANDALONE_WIN
    using System.Runtime.InteropServices;
    using UnityEngine;
#endif

    /// <summary> Manage the Total Native API. </summary>
    public class NativeInterface
    {
        /// <summary> Default constructor. </summary>
        public NativeInterface()
        {
            //Add Standalone plugin search path.
#if UNITY_STANDALONE_WIN
            NativeApi.SetDllDirectory(System.IO.Path.Combine(Application.dataPath, "Plugins"));
#endif
            NativeHeadTracking = new NativeHeadTracking(this);
            NativePerception = new NativePerception(this);
            NativeTrackableImage = new NativeTrackableImage(this);
            NativePlane = new NativePlane(this);
            NativeTrackable = new NativeTrackable(this);
            Configuration = new NativeConfiguration(this);
            NativeRenderring = new NativeRenderring();
        }

        public event Action OnPerceptionHandleChanged;
        /// <summary> Gets or sets the handle of the tracking. </summary>
        /// <value> The tracking handle. </value>
        UInt64 _perceptionHandle;
        public UInt64 PerceptionHandle
        {
            get => _perceptionHandle;
            set
            {
                if (_perceptionHandle != value)
                {
                    UInt64 oriHandle = _perceptionHandle;
                    _perceptionHandle = value;
                    if (oriHandle != 0)
                        OnPerceptionHandleChanged?.Invoke();
                }
            }
        }
        public int PerceptionId { get; set; }

        /// <summary> Gets or sets the native head tracking. </summary>
        /// <value> The native head tracking. </value>
        public NativeHeadTracking NativeHeadTracking { get; set; }

        /// <summary> Gets or sets the native tracking. </summary>
        /// <value> The native tracking. </value>
        internal NativePerception NativePerception { get; set; }

        /// <summary> Gets or sets the native trackable image. </summary>
        /// <value> The native trackable image. </value>
        internal NativeTrackableImage NativeTrackableImage { get; set; }

        /// <summary> Gets or sets the native plane. </summary>
        /// <value> The native plane. </value>
        internal NativePlane NativePlane { get; set; }

        /// <summary> Gets or sets the native trackable. </summary>
        /// <value> The native trackable. </value>
        internal NativeTrackable NativeTrackable { get; set; }

        /// <summary> Gets or sets the configuration. </summary>
        /// <value> The configuration. </value>
        internal NativeConfiguration Configuration { get; set; }

        /// <summary> Gets or sets the configuration. </summary>
        /// <value> The configuration. </value>
        internal NativeRenderring NativeRenderring { get; set; }

        private partial struct NativeApi
        {
#if UNITY_STANDALONE_WIN
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);
#endif
        }
    }
}
