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
    using UnityEngine.Serialization;
	using System.Collections.Generic;

    /// <summary> A configuration used to track the world. </summary>
    [CreateAssetMenu(fileName = "NRKernalSessionConfig", menuName = "NRSDK/SessionConfig", order = 1)]
    public class NRSessionConfig : ScriptableObject
    {
        /// <summary> Chooses which plane finding mode will be used. </summary>
        [Tooltip("Chooses which plane finding mode will be used.")]
        [FormerlySerializedAs("EnablePlaneFinding")]
        public TrackablePlaneFindingMode PlaneFindingMode = TrackablePlaneFindingMode.DISABLE;

        /// <summary> Chooses which marker finding mode will be used. </summary>
        [Tooltip("Chooses which marker finding mode will be used.")]
        [FormerlySerializedAs("EnableImageTracking")]
        public TrackableImageFindingMode ImageTrackingMode = TrackableImageFindingMode.DISABLE;

        /// <summary>
        /// A scriptable object specifying the NRSDK TrackingImageDatabase configuration. </summary>
        [Tooltip("A scriptable object specifying the NRSDK TrackingImageDatabase configuration.")]
        public NRTrackingImageDatabase TrackingImageDatabase;

        /// <summary> Chooses whether notification will be used. </summary>
        [Tooltip("Chooses whether notification will be used.")]
        public bool EnableNotification = false;
        
        /// <summary> Chooses whether to kill process while receive OnGlassesDisconnectEvent for NOTIFY_TO_QUIT_APP reason. </summary>
        [Tooltip("Chooses whether to force kill while receive OnGlassesDisconnectEvent for NOTIFY_TO_QUIT_APP reason.")]
        public bool ForceKillWhileGlassesSwitchMode = true;
        
#if ENABLE_MONO_MODE
        /// <summary> Chooses  whether to support to run in mono mode.")]. </summary>
        [Tooltip("Chooses whether to support to run in mono mode.")]
        public bool SupportMonoMode = false;
#else
        public bool SupportMonoMode { 
            get { return false; }
        }
#endif

        /// <summary> An error prompt will pop up when the device fails to connect. </summary>
        [Tooltip("An error prompt will pop up when the device fails to connect.")]
        public NRGlassesInitErrorTip GlassesErrorTipPrefab;

        /// <summary> An warnning prompt will pop up when the lost tracking. </summary>
        [Tooltip("An warnning prompt will pop up when the lost tracking.")]
        public NRTrackingModeChangedTip TrackingModeChangeTipPrefab;

        /// <summary> It will be read automatically from PlayerdSetting. </summary>
        [Tooltip("It will be read automatically from PlayerdSetting.")]
        public bool UseMultiThread = false;
        
        /// <summary> The NRProjectConfig whick is global unique. All NRSessionConfig in project should refer to the same NRProjectConfig. </summary>
        [SerializeField]
        [Tooltip("Donot change this manually, it always refer to the NRProjectConfig whick is global unique.")]
        NRProjectConfig ProjectConfig;

        public NRProjectConfig GlobalProjectConfig
        {
            get
            {
                return ProjectConfig;
            }
        }

        /// <summary> whether to support to run in multi-resume mode. </summary>
        public bool SupportMultiResume
        {
            get
            {
                return ProjectConfig.supportMultiResume;
            }
        }

        /// <summary> ValueType check if two NRSessionConfig objects are equal. </summary>
        /// <param name="other"> .</param>
        /// <returns>
        /// True if the two NRSessionConfig objects are value-type equal, otherwise false. </returns>
        public override bool Equals(object other)
        {
            NRSessionConfig otherConfig = other as NRSessionConfig;
            if (other == null)
            {
                return false;
            }

            if (PlaneFindingMode != otherConfig.PlaneFindingMode ||
                ImageTrackingMode != otherConfig.ImageTrackingMode ||
                TrackingImageDatabase != otherConfig.TrackingImageDatabase)
            {
                return false;
            }

            if (ProjectConfig != otherConfig.ProjectConfig)
                return false;

            return true;
        }

        /// <summary> Return a hash code for this object. </summary>
        /// <returns> A hash code for this object. </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary> ValueType copy from another SessionConfig object into this one. </summary>
        /// <param name="other"> .</param>
        public void CopyFrom(NRSessionConfig other)
        {
            PlaneFindingMode                = other.PlaneFindingMode;
            ImageTrackingMode               = other.ImageTrackingMode;
            TrackingImageDatabase           = other.TrackingImageDatabase;
            GlassesErrorTipPrefab           = other.GlassesErrorTipPrefab;
            TrackingModeChangeTipPrefab     = other.TrackingModeChangeTipPrefab;
            EnableNotification              = other.EnableNotification;
            ForceKillWhileGlassesSwitchMode = other.ForceKillWhileGlassesSwitchMode;
#if ENABLE_MONO_MODE
            SupportMonoMode                 = other.SupportMonoMode;
#endif
            ProjectConfig                   = other.ProjectConfig;
        }

        public bool IsTargetDevice(NRDeviceCategory device)
        {
            return ProjectConfig ? ProjectConfig.targetDevices.Contains(device) : false;
        }

        public string GetTargetDeviceTypesDesc()
        {
            return ProjectConfig ? ProjectConfig.GetTargetDeviceTypesDesc() : string.Empty;
        }
        
#if UNITY_EDITOR
        public void SetProjectConfig(NRProjectConfig projectConfig)
        {
            ProjectConfig = projectConfig;
        }

        public void SetUseMultiThread(bool useMultiThread)
        {
            UseMultiThread = useMultiThread;
        }
#endif
    }
}
