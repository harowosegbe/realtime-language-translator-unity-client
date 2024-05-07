/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/         
* 
*****************************************************************************/

using UnityEngine;

namespace NRKernal
{
    public class NRHandControllerProvider : ControllerProviderBase
    {
        /// <summary> The native handtracking. </summary>
#if UNITY_EDITOR
        private NREmulatorHandTracking m_NativeHandTracking;
#else
        private NativeHandTracking m_NativeHandTracking;
#endif
        /// <summary> Constructor. </summary>
        /// <param name="states"> The states.</param>
        public NRHandControllerProvider(ControllerState[] states) : base(states)
        {
#if UNITY_EDITOR
            m_NativeHandTracking = new NREmulatorHandTracking();
#else
            EnableHandTracking();
            m_NativeHandTracking = new NativeHandTracking(NRSessionManager.Instance.NativeAPI);
#endif
        }

        public override int ControllerCount { get { return 2; } }


        /// <summary> Update the controller. </summary>
        public override void Update()
        {
            if (m_NativeHandTracking == null)
            {
                return;
            }
            m_NativeHandTracking.Update(GetHandState(0), GetHandState(1));

            for (int i = 0; i < states.Length; i++)
            {
                UpdateControllerState(i, GetHandState(i));
            }
        }

        public override void Resume()
        {
            base.Resume();
            EnableHandTracking();
        }

        private void EnableHandTracking()
        {
            NRSessionManager.Instance.NativeAPI.Configuration.SetHandTrackingEnabled(true);
        }

        private HandState GetHandState(int index)
        {
            return NRInput.Hands.GetHandState(index == 0 ? HandEnum.RightHand : HandEnum.LeftHand);
        }

        private void UpdateControllerState(int index, HandState handState)
        {
            states[index].controllerType = ControllerType.CONTROLLER_TYPE_HAND;
            states[index].availableFeature = ControllerAvailableFeature.CONTROLLER_AVAILABLE_FEATURE_ROTATION | ControllerAvailableFeature.CONTROLLER_AVAILABLE_FEATURE_POSITION;
            states[index].connectionState = ControllerConnectionState.CONTROLLER_CONNECTION_STATE_CONNECTED;
            states[index].rotation = handState.pointerPose.rotation;
            states[index].position = handState.pointerPose.position;
            states[index].gyro = Vector3.zero;
            states[index].accel = Vector3.zero;
            states[index].mag = Vector3.zero;
            states[index].touchPos = Vector3.zero;
            states[index].isTouching = handState.pointerPoseValid && handState.isPinching;
            states[index].recentered = false;
            states[index].isCharging = false;
            states[index].batteryLevel = 0;

            IControllerStateParser stateParser = ControllerStateParseUtility.GetControllerStateParser(states[index].controllerType, index);
            if (stateParser != null)
            {
                stateParser.ParserControllerState(states[index]);
            }
        }
    }
}
