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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary> A manager of hand states. </summary>
    public class NRHandsManager
    {
        private readonly Dictionary<HandEnum, NRHand> m_HandsDict;
        private readonly HandState[] m_HandStates; // index 0 represents right and index 1 represents left
        private readonly OneEuroFilter[] m_OneEuroFilters;
        private bool m_Inited;

        public Action OnHandTrackingStarted;
        public Action OnHandStatesUpdated;
        public Action OnHandTrackingStopped;

        /// <summary>
        /// Returns true if the hand tracking is now running normally
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return NRInput.CurrentInputSourceType == InputSourceEnum.Hands;
            }
        }

        public NRHandsManager()
        {
            m_HandsDict = new Dictionary<HandEnum, NRHand>();
            m_HandStates = new HandState[2] { new HandState(HandEnum.RightHand), new HandState(HandEnum.LeftHand) };
            m_OneEuroFilters = new OneEuroFilter[2] { new OneEuroFilter(), new OneEuroFilter() };
        }

        /// <summary>
        /// Regist the left or right NRHand. There would be at most one NRHand for each hand enum
        /// </summary>
        /// <param name="hand"></param>
        internal void RegistHand(NRHand hand)
        {
            if (hand == null || hand.HandEnum == HandEnum.None)
                return;
            var handEnum = hand.HandEnum;
            if (m_HandsDict.ContainsKey(handEnum))
            {
                m_HandsDict[handEnum] = hand;
            }
            else
            {
                m_HandsDict.Add(handEnum, hand);
            }
        }

        /// <summary>
        /// UnRegist the left or right NRHand
        /// </summary>
        /// <param name="hand"></param>
        internal void UnRegistHand(NRHand hand)
        {
            if (hand == null)
                return;
            m_HandsDict.Remove(hand.HandEnum);
        }

        /// <summary>
        /// Init hand tracking with a certain service
        /// </summary>
        internal void Init()
        {
            if (m_Inited)
                return;

            NRInput.OnControllerStatesUpdated += UpdateHandTracking;
            m_Inited = true;
            NRDebugger.Info("[HandsManager] Hand Tracking Inited");
        }

        /// <summary>
        /// Returns true if start hand tracking success
        /// </summary>
        /// <returns></returns>
        internal bool StartHandTracking()
        {
            if (!m_Inited)
            {
                Init();
            }
            else if (IsRunning)
            {
                NRDebugger.Info("[HandsManager] Hand Tracking Start: Success");
                return true;
            }

            NRDebugger.Info("[HandsManager] Hand Tracking Start: Success");
            NRInput.SwitchControllerProvider(typeof(NRHandControllerProvider));
            OnHandTrackingStarted?.Invoke();
            return true;
        }

        /// <summary>
        /// Returns true if stop hand tracking success
        /// </summary>
        /// <returns></returns>
        internal bool StopHandTracking()
        {
            if (!m_Inited)
            {
                NRDebugger.Info("[HandsManager] Hand Tracking Stop: Success");
                return true;
            }

            if (!IsRunning)
            {
                NRDebugger.Info("[HandsManager] Hand Tracking Stop: Success");
                return true;
            }

            NRDebugger.Info("[HandsManager] Hand Tracking Stop: Success");
            NRInput.SwitchControllerProvider(ControllerProviderFactory.controllerProviderType);
            ResetHandStates();
            OnHandTrackingStopped?.Invoke();
            return true;
        }

        /// <summary>
        /// Get the current hand state of the left or right hand
        /// </summary>
        /// <param name="handEnum"></param>
        /// <returns></returns>
        public HandState GetHandState(HandEnum handEnum)
        {
            switch (handEnum)
            {
                case HandEnum.RightHand:
                    return m_HandStates[0];
                case HandEnum.LeftHand:
                    return m_HandStates[1];
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Get the left or right NRHand if it has been registered.
        /// </summary>
        /// <param name="handEnum"></param>
        /// <returns></returns>
        public NRHand GetHand(HandEnum handEnum)
        {
            NRHand hand;
            if (m_HandsDict != null && m_HandsDict.TryGetValue(handEnum, out hand))
            {
                return hand;
            }
            return null;
        }

        /// <summary>
        /// Returns true if user is now performing the systemGesture
        /// </summary>
        /// <returns></returns>
        public bool IsPerformingSystemGesture()
        {
            return IsPerformingSystemGesture(HandEnum.LeftHand) || IsPerformingSystemGesture(HandEnum.RightHand);
        }

        /// <summary>
        /// Returns true if user is now performing the systemGesture
        /// </summary>
        /// <param name="handEnum"></param>
        /// <returns></returns>
        public bool IsPerformingSystemGesture(HandEnum handEnum)
        {
            return IsPerformingSystemGesture(GetHandState(handEnum));
        }

        private void ResetHandStates()
        {
            for (int i = 0; i < m_HandStates.Length; i++)
            {
                m_HandStates[i].Reset();
            }
        }

        private void UpdateHandTracking()
        {
            if (!IsRunning)
                return;
            UpdateHandPointer();
            OnHandStatesUpdated?.Invoke();
        }

        private void UpdateHandPointer()
        {
            for (int i = 0; i < m_HandStates.Length; i++)
            {
                var handState = m_HandStates[i];
                if (handState == null)
                    continue;

                CalculatePointerPose(handState);
            }
        }

        private Vector3 GetNeckPosition()
        {
            Vector3 neckOffset = new Vector3(0, -0.15f, 0);
            Vector3 neckPosition = NRInput.CameraCenter.position +
                Vector3.Lerp(NRInput.CameraCenter.rotation * neckOffset, neckOffset, 0.5f);
            return neckPosition;
        }

        private Vector3 GetShoulderPosition(bool isRight)
        {
            Vector3 shoulderOffset = new Vector3(isRight ? 0.15f : -0.15f, 0, 0);
            Vector3 shoulderPosition = GetNeckPosition() +
                Quaternion.Euler(0, NRInput.CameraCenter.eulerAngles.y, 0) * shoulderOffset;
            return shoulderPosition;
        }

        private Vector3 GetWristOffsetPosition(HandState handState)
        {
            Vector3 localWristOffset = new Vector3(0, -0.0425f, -0.0652f);
            var wristPose = handState.GetJointPose(HandJointID.Wrist);
            Vector3 wristOffset = wristPose.position + wristPose.rotation * localWristOffset;
            return wristOffset;
        }

        private Vector3 GetHandRayDirection(HandState handState)
        {
            Vector3 shoulderPosition = GetShoulderPosition(handState.handEnum == HandEnum.RightHand);
            Vector3 rayOrigin = Vector3.Lerp(GetWristOffsetPosition(handState), shoulderPosition, 0.532f);
            Vector3 indexPosition = handState.GetJointPose(HandJointID.IndexProximal).position;
            return (indexPosition - rayOrigin).normalized;
        }

        private void CalculatePointerPose(HandState handState)
        {
            if (handState.isTracked)
            {
                var wristPose = handState.GetJointPose(HandJointID.Wrist);
                var cameraTransform = NRInput.CameraCenter;
                handState.pointerPoseValid = Vector3.Angle(cameraTransform.forward, wristPose.forward) < 70f;

                if (handState.pointerPoseValid)
                {
                    Vector3 middleToRing = (handState.GetJointPose(HandJointID.MiddleProximal).position
                                          - handState.GetJointPose(HandJointID.RingProximal).position).normalized;
                    Vector3 middleToWrist = (handState.GetJointPose(HandJointID.MiddleProximal).position
                                           - handState.GetJointPose(HandJointID.Wrist).position).normalized;
                    Vector3 middleToCenter = Vector3.Cross(middleToWrist, middleToRing).normalized;
                    var pointerPosition = handState.GetJointPose(HandJointID.MiddleProximal).position
                                        + middleToWrist * 0.02f
                                        + middleToRing * 0.01f
                                        + middleToCenter * (handState.handEnum == HandEnum.RightHand ? 0.06f : -0.06f);

                    Vector3 pointerDirection = GetHandRayDirection(handState);
                    Quaternion pointerRotation = Quaternion.LookRotation(m_OneEuroFilters[(int)handState.handEnum].Step(Time.realtimeSinceStartup, pointerDirection));
                    handState.pointerPose = new Pose(pointerPosition, pointerRotation);
                }
            }
            else
            {
                handState.pointerPoseValid = false;
            }
        }

        private bool IsPerformingSystemGesture(HandState handState)
        {
            if (!IsRunning)
            {
                return false;
            }
            return handState.currentGesture == HandGesture.System;
        }

        public class OneEuroFilter
        {
            public float Beta = 10f;
            public float MinCutoff = 1.0f;
            const float DCutOff = 1.0f;
            (float t, Vector3 x, Vector3 dx) _prev;

            public Vector3 Step(float t, Vector3 x)
            {
                var t_e = t - _prev.t;

                if (t_e < 1e-5f)
                    return _prev.x;

                var dx = (x - _prev.x) / t_e;
                var dx_res = Vector3.Lerp(_prev.dx, dx, Alpha(t_e, DCutOff));

                var cutoff = MinCutoff + Beta * dx_res.magnitude;
                var x_res = Vector3.Lerp(_prev.x, x, Alpha(t_e, cutoff));

                _prev = (t, x_res, dx_res);

                return x_res;
            }

            static float Alpha(float t_e, float cutoff)
            {
                var r = 2 * Mathf.PI * cutoff * t_e;
                return r / (r + 1);
            }
        }
    }
}
