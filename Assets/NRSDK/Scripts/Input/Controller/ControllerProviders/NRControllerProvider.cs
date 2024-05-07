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
    using UnityEngine;

    /// <summary>
    /// This class obtains the runtime information of controller devices through a native controller
    /// plugin, and would use these info to update controller states. </summary>
    internal partial class NRControllerProvider : ControllerProviderBase
    {
        /// <summary> The native controller. </summary>
        private NativeController m_NativeController;
        /// <summary> The processed frame. </summary>
        private int m_ProcessedFrame;
        /// <summary> True to need recenter. </summary>
        private bool m_NeedRecenter;
        /// <summary> Array of home pressing timers. </summary>
        private float[] homePressingTimerArr = new float[NRInput.MAX_CONTROLLER_STATE_COUNT];

        /// <summary> The home long press time. </summary>
        private const float HOME_LONG_PRESS_TIME = 1.1f;

        /// <summary> Constructor. </summary>
        /// <param name="states"> The states.</param>
        public NRControllerProvider(ControllerState[] states) : base(states)
        {
            Create();
        }

        /// <summary> Gets the number of controllers. </summary>
        /// <value> The number of controllers. </value>
        public override int ControllerCount
        {
            get
            {
                if (!running)
                    return 0;

                return m_NativeController.GetControllerCount();
            }
        }

        /// <summary> Gets a version. </summary>
        /// <param name="index"> Zero-based index of the controller.</param>
        /// <returns> The version. </returns>
        public string GetVersion(int index)
        {
            if (m_NativeController != null)
            {
                return m_NativeController.GetVersion(index);
            }
            return string.Empty;
        }

        public ControllerHandEnum GetHandednessType(int index)
        {
            if (m_NativeController != null)
            {
                return m_NativeController.GetHandednessType(index) == HandednessType.LEFT_HANDEDNESS ? ControllerHandEnum.Left : ControllerHandEnum.Right;
            }
            return ControllerHandEnum.Right;
        }

        private void Create()
        {
            m_NativeController = new NativeController();
            m_NativeController.Create();
        }

        /// <summary> Start the controller. </summary>
        public override void Start()
        {
            base.Start();

            if (m_NativeController != null)
            {
                m_NativeController.Start();
            }

#if !UNITY_EDITOR
            NRDebugger.Info("[NRInput] version:" + GetVersion(0));
#endif
        }

        /// <summary> Pause the controller. </summary>
        public override void Pause()
        {
            base.Pause();

            if (m_NativeController != null)
            {
                m_NativeController.Pause();
            }
        }

        /// <summary> Resume the controller. </summary>
        public override void Resume()
        {
            base.Resume();

            if (m_NativeController != null)
            {
                m_NativeController.Resume();
            }
        }

        /// <summary> Updates the controller. </summary>
        public override void Update()
        {
            if (!running)
                return;

            if (m_ProcessedFrame == Time.frameCount)
                return;
            m_ProcessedFrame = Time.frameCount;

            for (int i = 0; i < ControllerCount; i++)
            {
                UpdateControllerState(i);
            }
        }

        /// <summary> Destroy the controller. </summary>
        public override void Destroy()
        {
            base.Destroy();

            if (m_NativeController != null)
            {
                m_NativeController.Stop();
                m_NativeController.Destroy();
                m_NativeController = null;
            }
        }

        /// <summary> Trigger haptic vibration. </summary>
        /// <param name="index">           Zero-based index of the controller.</param>
        /// <param name="durationSeconds"> (Optional) The duration in seconds.</param>
        /// <param name="frequency">       (Optional) The frequency.</param>
        /// <param name="amplitude">       (Optional) The amplitude.</param>
        public override void TriggerHapticVibration(int index, float durationSeconds = 0.1f, float frequency = 1000f, float amplitude = 0.5f)
        {
            if (!running)
                return;

            if (states[index].controllerType == ControllerType.CONTROLLER_TYPE_PHONE)
            {
                PhoneVibrateTool.TriggerVibrate(durationSeconds);
            }
            else
            {
                if (m_NativeController != null && NRInput.GetControllerAvailableFeature(ControllerAvailableFeature.CONTROLLER_AVAILABLE_FEATURE_HAPTIC_VIBRATE))
                {
                    Int64 durationNano = (Int64)(durationSeconds * 1000000000);
                    m_NativeController.TriggerHapticVibrate(index, durationNano, frequency, amplitude);
                }
            }
        }

        /// <summary> Recenters this object. </summary>
        public override void Recenter()
        {
            base.Recenter();
            m_NeedRecenter = true;
        }

        /// <summary> Updates the controller state described by index. </summary>
        /// <param name="index"> Zero-based index of the.</param>
        private void UpdateControllerState(int index)
        {
            m_NativeController.UpdateState(index);
            var hmdTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
            states[index].controllerType = m_NativeController.GetControllerType(index);
#if UNITY_EDITOR
            if (NRInput.EmulateVirtualDisplayInEditor)
            {
                states[index].controllerType = ControllerType.CONTROLLER_TYPE_PHONE;
            }
#endif
            states[index].availableFeature = m_NativeController.GetAvailableFeatures(index);
            states[index].connectionState = m_NativeController.GetConnectionState(index);
            states[index].rotation = m_NativeController.GetPose(index, hmdTime).rotation;
            states[index].position = m_NativeController.GetPose(index, hmdTime).position;
            states[index].gyro = m_NativeController.GetGyro(index);
            states[index].accel = m_NativeController.GetAccel(index);
            states[index].mag = m_NativeController.GetMag(index);
            states[index].touchPos = m_NativeController.GetTouch(index);
            states[index].isTouching = m_NativeController.IsTouching(index);
            states[index].recentered = false;
            states[index].isCharging = m_NativeController.IsCharging(index);
            states[index].batteryLevel = m_NativeController.GetBatteryLevel(index);
            states[index].buttonsState = (ControllerButton)m_NativeController.GetButtonState(index);
            states[index].buttonsDown = (ControllerButton)m_NativeController.GetButtonDown(index);
            states[index].buttonsUp = (ControllerButton)m_NativeController.GetButtonUp(index);

            IControllerStateParser stateParser = ControllerStateParseUtility.GetControllerStateParser(states[index].controllerType, index);
            if (stateParser != null)
            {
                stateParser.ParserControllerState(states[index]);
            }

            CheckRecenter(index);

            if (m_NeedRecenter)
            {
                for (int i = 0; i < ControllerCount; i++)
                {
                    states[i].recentered = true;
                    m_NativeController.RecenterController(i);
                }
                m_NeedRecenter = false;
            }
        }

        /// <summary> Check recenter. </summary>
        /// <param name="index"> Zero-based index of the.</param>
        private void CheckRecenter(int index)
        {
            if (states[index].GetButton(ControllerButton.HOME))
            {
                homePressingTimerArr[index] += Time.deltaTime;
                if (homePressingTimerArr[index] > HOME_LONG_PRESS_TIME)
                {
                    homePressingTimerArr[index] = float.MinValue;
                    Recenter();
                }
            }
            else
            {
                homePressingTimerArr[index] = 0f;
            }
        }
    }
}