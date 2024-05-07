/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

#if USING_XR_MANAGEMENT && USING_XR_SDK_XREAL
#define USING_XR_SDK
#endif

namespace NRKernal
{
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
#if USING_XR_SDK
    using UnityEngine.XR;
#endif

    /// <summary> Hmd pose tracker event. </summary>
    public delegate void HMDPoseTrackerEvent();
    public delegate void HMDPoseTrackerModeChangeEvent(TrackingType origin, TrackingType target);
    public delegate void OnTrackingModeChanged(NRHMDPoseTracker.TrackingModeChangedResult result);
    public delegate void OnWorldPoseResetEvent();


    /// <summary>
    /// Interface of external slam provider.
    /// </summary>
    public interface IExternSlamProvider
    {
        /// <summary>
        /// Get head pose at the time of timeStamp
        /// </summary>
        /// <param name="timeStamp"> The specified time. </param>
        /// <returns></returns>
        Pose GetHeadPoseAtTime(UInt64 timeStamp);
    }

    /// <summary> HMD tracking type. </summary>
    public enum TrackingType
    {
        /// <summary>
        /// Track the position an rotation.
        /// </summary>
        Tracking6Dof = 0,

        /// <summary>
        /// Track the rotation only.
        /// </summary>
        Tracking3Dof = 1,

        /// <summary>
        /// Track nothing.
        /// </summary>
        Tracking0Dof = 2,

        /// <summary>
        /// Track nothing. Use rotation to make tracking smoothly.
        /// </summary>
        Tracking0DofStable = 3
    }

    /// <summary>
    /// HMDPoseTracker update the informations of pose tracker. This component is used to initialize
    /// the camera parameter, update the device posture, In addition, application can change
    /// TrackingType through this component. </summary>
    [HelpURL("https://developer.xreal.com/develop/discover/introduction-nrsdk")]
    public class NRHMDPoseTracker : MonoBehaviour
    {
        /// <summary> Event queue for all listeners interested in OnHMDPoseReady events. </summary>
        public static event HMDPoseTrackerEvent OnHMDPoseReady;
        /// <summary> Event queue for all listeners interested in OnHMDLostTracking events. </summary>
        public static event HMDPoseTrackerEvent OnHMDLostTracking;
        /// <summary> Event queue for all listeners interested in OnChangeTrackingMode events. </summary>
        public static event HMDPoseTrackerModeChangeEvent OnChangeTrackingMode;
        /// <summary> Event queue for all listeners interested in OnWorldPoseReset events. </summary>
        public static event OnWorldPoseResetEvent OnWorldPoseReset;

        public struct TrackingModeChangedResult
        {
            public bool success;
            public TrackingType trackingType;
        }

        /// <summary> Type of the tracking. </summary>
        [SerializeField]
        private TrackingType m_TrackingType = TrackingType.Tracking6Dof;

        /// <summary> Gets the tracking mode. </summary>
        /// <value> The tracking mode. </value>
        public TrackingType TrackingMode
        {
            get
            {
                return m_TrackingType;
            }
        }

        /// <summary> Auto adapt trackingType while not supported. </summary>
        public bool TrackingModeAutoAdapt = true;

        /// <summary> Use relative coordinates or not. </summary>
        public bool UseRelative = false;
        /// <summary> Whether to cache world pose automatically, while changing mode or pausing. </summary>
        public bool AutoCacheWorldPose = true;
        /// <summary> The last reason. </summary>
        private LostTrackingReason m_LastReason = LostTrackingReason.NONE;

        /// <summary> The left camera. </summary>
        public Camera leftCamera;
        /// <summary> The center camera. </summary>
        public Camera centerCamera;
        public Transform centerAnchor;
        Pose HeadRotFromCenter = Pose.identity;
        /// <summary> The right camera. </summary>
        public Camera rightCamera;
        private bool m_ModeChangeLock = false;
        public bool IsTrackModeChanging
        {
            get { return m_ModeChangeLock; }
        }

        public OnTrackingModeChanged OnModeChanged;
        /// <summary> Awakes this object. </summary>
        void Awake()
        {
#if !UNITY_EDITOR
            if (leftCamera != null)
            {
                leftCamera.depth = 0;
                leftCamera.enabled = false;
                leftCamera.depthTextureMode = DepthTextureMode.None;
                leftCamera.rect = new Rect(0, 0, 1, 1);
            }
            if (rightCamera != null)
            {
                rightCamera.depth = 1;
                rightCamera.enabled = false;
                rightCamera.depthTextureMode = DepthTextureMode.None;
                rightCamera.rect = new Rect(0, 0, 1, 1);
            }
            if (centerCamera != null)
            {
                centerCamera.depth = 1;
                centerCamera.enabled = false;
                centerCamera.depthTextureMode = DepthTextureMode.None;
                centerCamera.rect = new Rect(0, 0, 1, 1);
            }
#endif
            StartCoroutine(Initialize());
        }

        /// <summary> Executes the 'enable' action. </summary>
        void OnEnable()
        {
            NRKernalUpdater.OnUpdate += OnUpdate;
        }

        /// <summary> Executes the 'disable' action. </summary>
        void OnDisable()
        {
            NRKernalUpdater.OnUpdate -= OnUpdate;
        }

        /// <summary>
        /// Executes the 'destroy' action.
        /// </summary>
        private void OnDestroy()
        {
            NRKernalUpdater.OnUpdate -= OnUpdate;
        }

        /// <summary> Executes the 'update' action. </summary>
        void OnUpdate()
        {
            CheckHMDPoseState();
            UpdatePoseByTrackingType();
        }

        /// <summary> Auto adaption for current working trackingType based on supported feature on current device. </summary>
        public void AutoAdaptTrackingType()
        {
            if (TrackingModeAutoAdapt)
            {
                TrackingType adjustTrackingType = AdaptTrackingType(m_TrackingType);
                if (adjustTrackingType != m_TrackingType)
                {
                    NRDebugger.Warning("[NRHMDPoseTracker] AutoAdaptTrackingType: {0} => {1}", m_TrackingType, adjustTrackingType);
                    m_TrackingType = adjustTrackingType;
                }
            }
        }

        /// <summary> Auto adaption for trackingType based on supported feature on current device. </summary>
        /// <returns> fallback trackingType. </returns>
        public static TrackingType AdaptTrackingType(TrackingType mode)
        {
            switch (mode)
            {
                case TrackingType.Tracking6Dof:
                    {
                        if (NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_PERCEPTION_HEAD_TRACKING_POSITION))
                            return TrackingType.Tracking6Dof;
                        else if (NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_PERCEPTION_HEAD_TRACKING_ROTATION))
                            return TrackingType.Tracking3Dof;
                        else
                            return TrackingType.Tracking0Dof;
                    }
                case TrackingType.Tracking3Dof:
                    {
                        if (NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_PERCEPTION_HEAD_TRACKING_ROTATION))
                            return TrackingType.Tracking3Dof;
                        else
                            return TrackingType.Tracking0Dof;
                    }
                case TrackingType.Tracking0DofStable:
                    {
                        if (NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_PERCEPTION_HEAD_TRACKING_ROTATION))
                            return TrackingType.Tracking0DofStable;
                        else
                            return TrackingType.Tracking0Dof;
                    }
            }
            return mode;
        }

        /// <summary> Change mode. </summary>
        /// <param name="trackingtype">        The trackingtype.</param>
        /// <param name="OnModeChanged"> The mode changed call back and return the result.</param>
        private bool ChangeMode(TrackingType trackingtype, OnTrackingModeChanged OnModeChanged)
        {
            NRDebugger.Info("[NRHMDPoseTracker] Req ChangeMode: {0} => {1}", m_TrackingType, trackingtype);
            TrackingModeChangedResult result = new TrackingModeChangedResult();
            if (trackingtype == m_TrackingType || m_ModeChangeLock)
            {
                result.success = false;
                result.trackingType = m_TrackingType;
                OnModeChanged?.Invoke(result);
                NRDebugger.Warning("[NRHMDPoseTracker] Change tracking mode faild: modeChangeLocking={0}", m_ModeChangeLock);
                return false;
            }

            OnChangeTrackingMode?.Invoke(m_TrackingType, trackingtype);
            NRSessionManager.OnChangeTrackingMode?.Invoke(m_TrackingType, trackingtype);

#if !UNITY_EDITOR
            m_ModeChangeLock = true;
            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                //Thread.Sleep(20);
                NRDebugger.Info("[NRHMDPoseTracker] Beg ChangeMode: {0} => {1}", m_TrackingType, trackingtype);
                result.success = NRSessionManager.Instance.TrackingSubSystem.SwitchTrackingType(trackingtype);

                if (result.success)
                {
                    m_TrackingType = trackingtype;
                }
                result.trackingType = m_TrackingType;
                OnModeChanged?.Invoke(result);
                m_ModeChangeLock = false;
                NRDebugger.Info("[NRHMDPoseTracker] End ChangeMode: result={0}, curTrackingType={1}", result.success, m_TrackingType);
            });
#else
            m_TrackingType = trackingtype;
            result.success = true;
            result.trackingType = m_TrackingType;
            OnModeChanged?.Invoke(result);
#endif
            return true;
        }

        private void OnApplicationPause(bool pause)
        {
            NRDebugger.Info("[NRHMDPoseTracker] OnApplicationPause: pause={0}, headPos={1}, cachedWorldMatrix={2}, AutoCacheWorldPose={3}",
                pause, NRFrame.HeadPose.ToString("F4"), cachedWorldMatrix.ToString(), AutoCacheWorldPose);
            if (pause && AutoCacheWorldPose)
            {
                this.CacheWorldMatrix();
            }
        }

        Pose GetCachPose()
        {
            Pose cachePose;
            if (UseRelative)
                cachePose = new Pose(transform.localPosition, transform.localRotation);
            else
                cachePose = new Pose(transform.position, transform.rotation);

            // Cache position only for 6dof.  The neck mode relative position of 3Dof is ignored.
            if (m_TrackingType != TrackingType.Tracking6Dof)
            {
                cachePose.position = ConversionUtility.GetPositionFromTMatrix(cachedWorldMatrix);
            }
            // Ignore the rotation of stable 0dof.
            if (m_TrackingType == TrackingType.Tracking0DofStable)
            {
                cachePose.rotation = ConversionUtility.GetRotationFromTMatrix(cachedWorldMatrix);
            }
            return cachePose;
        }

        /// <summary> Change to 6 degree of freedom. </summary>
        /// <param name="OnModeChangedCallback"> The mode changed call back and return the result.</param>
        public bool ChangeTo6Dof(OnTrackingModeChanged OnModeChangedCallback = null)
        {
            var trackType = TrackingType.Tracking6Dof;
            trackType = AdaptTrackingType(trackType);
            Pose cachePose = GetCachPose();
            return ChangeMode(trackType, (TrackingModeChangedResult result) =>
            {
                if (result.success && AutoCacheWorldPose)
                    CacheWorldMatrix(cachePose);
                OnModeChangedCallback?.Invoke(result);
                this.OnModeChanged?.Invoke(result);
            });
        }

        /// <summary> Change to 3 degree of freedom. </summary>
        /// <param name="OnModeChangedCallback"> The mode changed call back and return the result.</param>
        public bool ChangeTo3Dof(OnTrackingModeChanged OnModeChangedCallback = null)
        {
            var trackType = TrackingType.Tracking3Dof;
            trackType = AdaptTrackingType(trackType);
            Pose cachePose = GetCachPose();
            return ChangeMode(trackType, (TrackingModeChangedResult result) =>
            {
                if (result.success && AutoCacheWorldPose)
                    CacheWorldMatrix(cachePose);
                OnModeChangedCallback?.Invoke(result);
                this.OnModeChanged?.Invoke(result);
            });
        }

        /// <summary> Change to 0 degree of freedom. </summary>
        /// <param name="OnModeChangedCallback"> The mode changed call back and return the result.</param>
        public bool ChangeTo0Dof(OnTrackingModeChanged OnModeChangedCallback = null)
        {
            var trackType = TrackingType.Tracking0Dof;
            trackType = AdaptTrackingType(trackType);
            Pose cachePose = GetCachPose();
            return ChangeMode(trackType, (TrackingModeChangedResult result) =>
            {
                if (result.success && AutoCacheWorldPose)
                    CacheWorldMatrix(cachePose);
                OnModeChangedCallback?.Invoke(result);
                this.OnModeChanged?.Invoke(result);
            });
        }

        /// <summary> Change to 3 degree of freedom. </summary>
        /// <param name="OnModeChangedCallback"> The mode changed call back and return the result.</param>
        public bool ChangeTo0DofStable(OnTrackingModeChanged OnModeChangedCallback = null)
        {
            var trackType = TrackingType.Tracking0DofStable;
            trackType = AdaptTrackingType(trackType);
            Pose cachePose = GetCachPose();
            return ChangeMode(trackType, (TrackingModeChangedResult result) =>
            {
                if (result.success && AutoCacheWorldPose)
                    CacheWorldMatrix(cachePose);
                OnModeChangedCallback?.Invoke(result);
                this.OnModeChanged?.Invoke(result);
            });
        }

        private Matrix4x4 cachedWorldMatrix = Matrix4x4.identity;
        private float cachedWorldPitch = 0;
        /// <summary> Cache the world matrix. </summary>
        internal void CacheWorldMatrix()
        {
            Pose cachePose = GetCachPose();
            CacheWorldMatrix(cachePose);
        }

        internal void CacheWorldMatrix(Pose pose)
        {
            NRDebugger.Info("[NRHMDPoseTracker] CacheWorldMatrix: UseRelative={0}, trackType={1}, pos={2}, eulerRot={3}", UseRelative, m_TrackingType, pose.ToString("F4"), pose.rotation.eulerAngles.ToString("F4"));
            Plane horizontal_plane = new Plane(Vector3.up, Vector3.zero);
            Vector3 forward_use_gravity = horizontal_plane.ClosestPointOnPlane(pose.forward).normalized;
            Quaternion rotation_use_gravity = Quaternion.LookRotation(forward_use_gravity, Vector3.up);
            cachedWorldMatrix = ConversionUtility.GetTMatrix(pose.position, rotation_use_gravity);
            cachedWorldPitch = 0;
            NRDebugger.Info("CacheWorldMatrix Adjust: pos={0}, {1}, cachedWorldMatrix={2}",
                pose.position.ToString("F4"), rotation_use_gravity.eulerAngles.ToString("F4"), cachedWorldMatrix.ToString());

            NRFrame.ResetHeadPose();
            OnWorldPoseReset?.Invoke();
        }

        /// <summary> 
        ///     Reset the camera to position:(0,0,0) and yaw orientation, or pitch orientation.
        /// </summary>
        /// <param name="resetPitch"> 
        /// Whether to reset the pitch direction.
        ///     if resetPitch is false, reset camera to rotation(x,0,z). Where x&z is the pitch&roll of the HMD device.
        ///     if resetPitch is true,  reset camera to rotation(0,0,z); Where z is the roll of the HMD device.
        /// </param>
        public void ResetWorldMatrix(bool resetPitch = false)
        {
            Pose pose;
            if (UseRelative)
                pose = new Pose(transform.localPosition, transform.localRotation);
            else
                pose = new Pose(transform.position, transform.rotation);

            // Get src pose which is not affected by cachedWorldMatrix
            Matrix4x4 cachedWorldInverse = Matrix4x4.Inverse(cachedWorldMatrix);
            var worldMatrix = ConversionUtility.GetTMatrix(pose.position, pose.rotation);
            var objectMatrix = cachedWorldInverse * worldMatrix;
            var srcPose = new Pose(ConversionUtility.GetPositionFromTMatrix(objectMatrix), ConversionUtility.GetRotationFromTMatrix(objectMatrix));

            Quaternion rotation = Quaternion.identity;
            if (resetPitch)
            {
                // reset on degree of yaw and pitch, so only roll of HMD is not affect.
                Vector3 forward = srcPose.forward;
                Vector3 right = Vector3.Cross(forward, Vector3.up);
                Vector3 up = Vector3.Cross(right, forward);
                rotation = Quaternion.LookRotation(forward, up);
                Debug.LogFormat("ResetWorldMatrix: forward={0}, right={1}, up={2}", forward.ToString("F4"), right.ToString("F4"), up.ToString("F4"));
            }
            else
            {
                // only reset on degree of yaw, so the pitch and roll of HMD is not affect.
                Plane horizontal_plane = new Plane(Vector3.up, Vector3.zero);
                Vector3 forward_use_gravity = horizontal_plane.ClosestPointOnPlane(srcPose.forward).normalized;
                rotation = Quaternion.LookRotation(forward_use_gravity, Vector3.up);
            }

            var matrix = ConversionUtility.GetTMatrix(srcPose.position, rotation);
            cachedWorldMatrix = Matrix4x4.Inverse(matrix);
            cachedWorldPitch = -rotation.eulerAngles.x;

            // update pose immediately
            UpdatePoseByTrackingType();

            // log out
            {
                var correctPos = ConversionUtility.GetPositionFromTMatrix(cachedWorldMatrix);
                var correctRot = ConversionUtility.GetRotationFromTMatrix(cachedWorldMatrix);
                NRDebugger.Info("[NRHMDPoseTracker] ResetWorldMatrix: resetPitch={0}, curPos={1},{2} srcPose={3},{4}, cachedWorldPitch={5}, correctPos={6},{7}",
                    resetPitch, pose.position.ToString("F4"), pose.rotation.eulerAngles.ToString("F4"),
                    srcPose.position.ToString("F4"), srcPose.rotation.eulerAngles.ToString("F4"),
                    cachedWorldPitch, correctPos.ToString("F4"), correctRot.eulerAngles.ToString("F4"));
            }

            OnWorldPoseReset?.Invoke();
        }

        /// <summary>
        /// Get the world offset matrix from native.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetWorldOffsetMatrixFromNative()
        {
            Matrix4x4 parentTransformMatrix;
            if (UseRelative)
            {
                if (transform.parent == null)
                {
                    parentTransformMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                }
                else
                {
                    parentTransformMatrix = Matrix4x4.TRS(transform.parent.position, transform.parent.rotation, Vector3.one);
                }
            }
            else
            {
                parentTransformMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            }

            return parentTransformMatrix * cachedWorldMatrix;
        }

        public float GetCachedWorldPitch()
        {
            return cachedWorldPitch;
        }

        private Pose ApplyWorldMatrix(Pose pose)
        {
            var objectMatrix = ConversionUtility.GetTMatrix(pose.position, pose.rotation);
            var object_in_world = cachedWorldMatrix * objectMatrix;
            return new Pose(ConversionUtility.GetPositionFromTMatrix(object_in_world),
                ConversionUtility.GetRotationFromTMatrix(object_in_world));
        }

        /// <summary> Initializes this object. </summary>
        /// <returns> An IEnumerator. </returns>
        private IEnumerator Initialize()
        {
            while (NRFrame.SessionStatus != SessionState.Running)
            {
                NRDebugger.Debug("[NRHMDPoseTracker] Waitting to initialize.");
                yield return new WaitForEndOfFrame();
            }

#if !UNITY_EDITOR
            bool result;
            var matrix_data = NRFrame.GetEyeProjectMatrix(out result, leftCamera.nearClipPlane, leftCamera.farClipPlane);
            Debug.Assert(result, "[NRHMDPoseTracker] GetEyeProjectMatrix failed.");
            if (result)
            {
                NRDebugger.Info("[NRHMDPoseTracker] Left Camera Project Matrix: {0}", matrix_data.LEyeMatrix.ToString());
                NRDebugger.Info("[NRHMDPoseTracker] RightCamera Project Matrix: {0}", matrix_data.REyeMatrix.ToString());
                NRDebugger.Info("[NRHMDPoseTracker] CenterCamera Project Matrix: {0}", matrix_data.CEyeMatrix.ToString());

                leftCamera.projectionMatrix = matrix_data.LEyeMatrix;
                rightCamera.projectionMatrix = matrix_data.REyeMatrix;
                centerCamera.projectionMatrix = matrix_data.CEyeMatrix;
            }

#if USING_XR_SDK
            List<XRNodeState> nodeStates = new List<XRNodeState>();
            while (true)
            {
                InputTracking.GetNodeStates(nodeStates);
                Pose centerPose, leftPose, rightPose;
                if (NRFrame.GetNodePoseData(nodeStates, XRNode.LeftEye, out leftPose) && NRFrame.GetNodePoseData(nodeStates, XRNode.RightEye, out rightPose) 
                    && NRFrame.GetNodePoseData(nodeStates, XRNode.CenterEye, out centerPose))
                {
                    leftCamera.transform.localPosition = leftPose.position;
                    leftCamera.transform.localRotation = leftPose.rotation;
                    rightCamera.transform.localPosition = rightPose.position;
                    rightCamera.transform.localRotation = rightPose.rotation;
                    // center camera of XR overlap with HMD.
                    centerCamera.transform.localPosition = Vector3.zero;
                    centerCamera.transform.localRotation = Quaternion.identity;
                    centerAnchor.localPosition = centerPose.position;
                    centerAnchor.localRotation = centerPose.rotation;

                    NRDebugger.Info("[NRHMDPoseTracker] XR Init eyePoseFromHead: leftEye={0}-{1}, rightEye={2}-{3}, centerEye={4}-{5}", 
                        leftPose.ToString("F6"), leftPose.rotation.eulerAngles.ToString("F6"), 
                        rightPose.ToString("F6"), rightPose.rotation.eulerAngles.ToString("F6"), 
                        centerPose.ToString("F6"), centerPose.rotation.eulerAngles.ToString("F6"));
                    break;
                }
                else
                {
                    for (int i = 0; i < nodeStates.Count; i++)
                    {
                        var nodeState = nodeStates[i];
                        NRDebugger.Info("[NRHMDPoseTracker] nodeStates-[0]: nodeType={1}", i, nodeState.nodeType);
                    }
                }
                yield return new WaitForEndOfFrame();
            }
#else
            var eyeposeFromHead = NRFrame.EyePoseFromHead;
            leftCamera.transform.localPosition = eyeposeFromHead.LEyePose.position;
            leftCamera.transform.localRotation = eyeposeFromHead.LEyePose.rotation;
            rightCamera.transform.localPosition = eyeposeFromHead.REyePose.position;
            rightCamera.transform.localRotation = eyeposeFromHead.REyePose.rotation;
            centerCamera.transform.localPosition = eyeposeFromHead.CEyePose.position;
            centerCamera.transform.localRotation = eyeposeFromHead.CEyePose.rotation;
            
            centerAnchor.localPosition = eyeposeFromHead.CEyePose.position;
            centerAnchor.localRotation = eyeposeFromHead.CEyePose.rotation;

            NRDebugger.Info("[NRHMDPoseTracker] Init eyePoseFromHead: lEye={0}-{1}, rEye={2}-{3}, cEye={4}-{5}", 
                eyeposeFromHead.LEyePose.ToString("F6"), eyeposeFromHead.LEyePose.rotation.eulerAngles.ToString("F6"), 
                eyeposeFromHead.REyePose.ToString("F6"), eyeposeFromHead.REyePose.rotation.eulerAngles.ToString("F6"),
                eyeposeFromHead.CEyePose.ToString("F6"), eyeposeFromHead.CEyePose.rotation.eulerAngles.ToString("F6"));
#endif

            var centerRotMatrix = ConversionUtility.GetTMatrix(Vector3.zero, centerAnchor.localRotation).inverse;
            HeadRotFromCenter = new Pose(Vector3.zero, ConversionUtility.GetRotationFromTMatrix(centerRotMatrix));
#endif
            NRDebugger.Info("[NRHMDPoseTracker] Initialized success.");
        }

        [Obsolete("This API is no longer in use.", true)]
        public void RegisterSlamProvider(IExternSlamProvider provider)
        {
            NRDebugger.Fatal("[NRHMDPoseTracker] RegisterSlamProvider is abandoned");
        }

        /// <summary> Get head pose by timeStamp in unity world, which involve cache pose. </summary>
        public bool GetHeadPoseByTimeInUnityWorld(ref Pose headPose, ulong timeStamp)
        {
            var nativeHeadPose = Pose.identity;
            if (m_TrackingType == TrackingType.Tracking0Dof)
            {
                nativeHeadPose = HeadRotFromCenter;
            }
            else
            {
                if (timeStamp == NRFrame.CurrentPoseTimeStamp)
                {
                    nativeHeadPose = NRFrame.HeadPose;
                }
                else
                {
                    if (!NRFrame.GetHeadPoseByTime(ref nativeHeadPose, timeStamp))
                        return false;
                }
            }

            // Correct head rotation for stabal 0-Dof.
            if (m_TrackingType == TrackingType.Tracking0DofStable)
            {
                // NRDebugger.Info("[NRHMDPoseTracker] GetHeadPose Tracking0DofStable: {0} * {1} => {2}", nativeHeadPose.rotation.eulerAngles.ToString("F4"), HeadRotFromCenter.rotation.eulerAngles.ToString("F4"), 
                //     (HeadRotFromCenter.rotation * nativeHeadPose.rotation).eulerAngles.ToString("F4"));
                nativeHeadPose.rotation = HeadRotFromCenter.rotation * nativeHeadPose.rotation;
            }

            headPose = nativeHeadPose;
            headPose = cachedWorldMatrix.Equals(Matrix4x4.identity) ? headPose : ApplyWorldMatrix(headPose);
            // NRDebugger.Info("[NRHMDPoseTracker] GetHeadPose: trackType={0}, pos={1}, {2} --> {3}, {4}", m_TrackingType, 
            //     nativeHeadPose.ToString("F4"), nativeHeadPose.rotation.eulerAngles.ToString("F4"), 
            //     headPose.ToString("F4"), headPose.rotation.eulerAngles.ToString("F4"));

            return true;
        }

        /// <summary> Updates the pose by tracking type. </summary>
        private void UpdatePoseByTrackingType()
        {
            Pose headPose = Pose.identity;

            if (!GetHeadPoseByTimeInUnityWorld(ref headPose, NRFrame.CurrentPoseTimeStamp))
                return;

            // NRDebugger.Info("[NRHMDPoseTracker] UpdatePose: trackType={2}, pos={0} --> {1}", NRFrame.HeadPose.ToString("F4"), headPose.ToString("F4"), m_TrackingType);
            if (UseRelative)
            {
                transform.localRotation = headPose.rotation;
                transform.localPosition = headPose.position;
            }
            else
            {
                transform.rotation = headPose.rotation;
                transform.position = headPose.position;
            }
            // NRDebugger.Info("[NRHMDPoseTracker] UpdatePose centerCamera: {0}, {1}", centerCamera.transform.position.ToString("F4"), centerCamera.transform.eulerAngles.ToString("F4"));
        }

        /// <summary> Check hmd pose state. </summary>
        private void CheckHMDPoseState()
        {
            if (NRFrame.SessionStatus != SessionState.Running
                || TrackingMode == TrackingType.Tracking0Dof
                || TrackingMode == TrackingType.Tracking0DofStable
                || IsTrackModeChanging)
            {
                return;
            }

            var currentReason = NRFrame.LostTrackingReason;
            // When LostTrackingReason changed.
            if (currentReason != m_LastReason)
            {
                NRDebugger.Info("[NRHMDPoseTracker] CheckHMDPoseState: {0} -> {1}", m_LastReason, NRFrame.LostTrackingReason);
                if (currentReason != LostTrackingReason.NONE && m_LastReason == LostTrackingReason.NONE)
                {
                    OnHMDLostTracking?.Invoke();
                    NRSessionManager.OnHMDLostTracking?.Invoke();
                }
                else if (currentReason == LostTrackingReason.NONE && m_LastReason != LostTrackingReason.NONE)
                {
                    OnHMDPoseReady?.Invoke();
                    NRSessionManager.OnHMDPoseReady?.Invoke();
                }
                m_LastReason = currentReason;
            }
        }
    }
}
