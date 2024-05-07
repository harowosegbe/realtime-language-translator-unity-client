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
    using UnityEngine;


    public enum NRControllerType
    {
        NR_CONTROLLER_TYPE_BASIC = 0,
    };

    /// <summary> A controller for handling natives. </summary>
    internal partial class NativeController
    {
        /// <summary> Handle of the controller group. </summary>
        private UInt64 m_ControllerGroupHandle = 0;
        /// <summary> Handle of the controller. </summary>
        private UInt64[] m_ControllerHandle;
        /// <summary> The state handles. </summary>
        private UInt64[] m_StateHandles;

        /// <summary> Create this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public void Create()
        {
            NRDebugger.Info("[NativeController] Create");
            NativeResult result = NativeApi.NRControllerGroupCreate(ref m_ControllerGroupHandle);
            NativeErrorListener.Check(result, this, "GroupCreate", true);
            NRDebugger.Info("[NativeController] NRControllerGroupCreate: {0}", m_ControllerGroupHandle);
            int groupCount = 0;
            result = NativeApi.NRControllerGroupGetCount(m_ControllerGroupHandle, ref groupCount);
            NativeErrorListener.Check(result, this, "GroupGetCount");
            NRDebugger.Info("[NativeController] NRControllerGroupGetCount: {0}", groupCount);
            m_ControllerHandle = new UInt64[groupCount];
            m_StateHandles = new UInt64[groupCount];
            for (int i = 0; i < groupCount; i++)
            {
                bool value = NativeApi.NRControllerGroupCheckControllerType(m_ControllerGroupHandle,
                    i, NRControllerType.NR_CONTROLLER_TYPE_BASIC);
                NRDebugger.Info("[NativeController] CheckControllerType: {0}", value);
                int controller_id = 0;
                result = NativeApi.NRControllerGroupGetControllerId(m_ControllerGroupHandle, i, ref controller_id);
                NativeErrorListener.Check(result, this, "GetControllerId");
                IntPtr out_description = IntPtr.Zero;
                uint out_description_length = 0;
                result = NativeApi.NRControllerGroupGetDescription(m_ControllerGroupHandle, i, ref out_description, ref out_description_length);
                NativeErrorListener.Check(result, this, "GetDescription");
                byte[] bytes = new byte[out_description_length];
                Marshal.Copy(out_description, bytes, 0, (int)out_description_length);
                NRDebugger.Info("[NativeController] GetDescription: {0}", System.Text.Encoding.ASCII.GetString(bytes, 0, (int)out_description_length));
                ControllerAvailableFeature feature = 0;
                result = NativeApi.NRControllerGroupGetSupportedFeatures(m_ControllerGroupHandle, i, ref feature);
                NativeErrorListener.Check(result, this, "GetSupportedFeatures");
                NRDebugger.Info("[NativeController] GetSupportedFeatures: {0}", feature);
                result = NativeApi.NRControllerCreate(controller_id, ref m_ControllerHandle[i]);
                NativeErrorListener.Check(result, this, "NRControllerCreate");
                NRDebugger.Info("[NativeController] Create : {0}", m_ControllerHandle[i]);
            }
        }

        /// <summary> Start this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Start()
        {
            if (m_ControllerHandle == null)
            {
                return false;
            }

            NRDebugger.Info("[NativeController] Start");
            for (int i = 0; i < m_ControllerHandle.Length; i++)
            {
                NativeResult result = NativeApi.NRControllerStart(m_ControllerHandle[i]);
                NativeErrorListener.Check(result, this, "Start", true);
            }

            return true;
        }

        /// <summary> Gets controller count. </summary>
        /// <returns> The controller count. </returns>
        public int GetControllerCount()
        {
            return m_ControllerHandle == null ? 0 : m_ControllerHandle.Length;
        }

        /// <summary> Pauses this object. </summary>
        public void Pause()
        {
            if (m_ControllerHandle == null)
            {
                return;
            }
            for (int i = 0; i < m_ControllerHandle.Length; i++)
            {
                NativeResult result = NativeApi.NRControllerPause(m_ControllerHandle[i]);
                NativeErrorListener.Check(result, this, "Pause", true);
            }
        }

        /// <summary> Resumes this object. </summary>
        public void Resume()
        {
            if (m_ControllerHandle == null)
            {
                return;
            }
            for (int i = 0; i < m_ControllerHandle.Length; i++)
            {
                NativeResult result = NativeApi.NRControllerResume(m_ControllerHandle[i]);
                NativeErrorListener.Check(result, this, "Resume", true);
            }
        }

        /// <summary> Stops this object. </summary>
        public void Stop()
        {
            if (m_ControllerHandle == null)
            {
                return;
            }
            for (int i = 0; i < m_ControllerHandle.Length; i++)
            {
                NativeResult result = NativeApi.NRControllerStop(m_ControllerHandle[i]);
                NativeErrorListener.Check(result, this, "Stop", true);
            }
        }

        /// <summary> Destroys this object. </summary>
        public void Destroy()
        {
            if (m_ControllerHandle == null)
            {
                return;
            }
            NativeResult result = NativeApi.NRControllerGroupDestroy(m_ControllerGroupHandle);
            NativeErrorListener.Check(result, this, "GroupDestroy", true);
            m_ControllerGroupHandle = 0;
            for (int i = 0; i < m_ControllerHandle.Length; i++)
            {
                if (m_StateHandles[i] != 0)
                {
                    result = NativeApi.NRControllerStateDestroy(m_StateHandles[i]);
                    NativeErrorListener.Check(result, this, "StateDestroy", true);
                }
                if (m_ControllerHandle[i] != 0)
                {
                    result = NativeApi.NRControllerDestroy(m_ControllerHandle[i]);
                    NativeErrorListener.Check(result, this, "Destroy", true);
                }
            }
            m_ControllerHandle = null;
        }

        /// <summary> Gets available features. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The available features. </returns>
        public ControllerAvailableFeature GetAvailableFeatures(int controllerIndex)
        {
            if (m_ControllerHandle == null)
            {
                return 0;
            }
            ControllerAvailableFeature availableFeature = 0;
            NativeApi.NRControllerGroupGetSupportedFeatures(m_ControllerGroupHandle, controllerIndex, ref availableFeature);
            return availableFeature;
        }

        /// <summary> Gets controller type. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The controller type. </returns>
        public ControllerType GetControllerType(int controllerIndex)
        {
            if (m_ControllerHandle == null)
            {
                return ControllerType.CONTROLLER_TYPE_UNKNOWN;
            }
            ControllerType controllerType = ControllerType.CONTROLLER_TYPE_UNKNOWN;
            NativeApi.NRControllerGetConnectedType(m_ControllerHandle[controllerIndex], ref controllerType);
            return controllerType;
        }

        /// <summary> Recenter controller. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        public void RecenterController(int controllerIndex)
        {
            if (m_ControllerHandle == null)
            {
                return;
            }
            if (m_ControllerHandle.Length <= controllerIndex)
                return;
            NativeApi.NRControllerRecenter(m_ControllerHandle[controllerIndex]);
        }

        /// <summary> Trigger haptic vibrate. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <param name="duration">        The duration.</param>
        /// <param name="frequency">       The frequency.</param>
        /// <param name="amplitude">       The amplitude.</param>
        public void TriggerHapticVibrate(int controllerIndex, Int64 duration, float frequency, float amplitude)
        {
            if (m_ControllerHandle == null)
            {
                return;
            }
            if (m_ControllerHandle.Length <= controllerIndex)
                return;
            NativeApi.NRControllerHapticVibrate(m_ControllerHandle[controllerIndex], duration, frequency, amplitude);
        }

        public void UpdateState(int controllerIndex)
        {
            if (m_ControllerHandle == null)
            {
                return;
            }
            NativeResult result = NativeResult.Success;
            if (m_StateHandles[controllerIndex] != 0)
            {
                result = NativeApi.NRControllerStateDestroy(m_StateHandles[controllerIndex]);
                NativeErrorListener.Check(result, this, "StateDestroy", true);
                m_StateHandles[controllerIndex] = 0;
            }
            result = NativeApi.NRControllerStateUpdate(m_ControllerHandle[controllerIndex], ref m_StateHandles[controllerIndex]);
            NativeErrorListener.Check(result, this, "ControllerStateUpdate", true);
        }

        /// <summary> Gets connection state. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The connection state. </returns>
        public ControllerConnectionState GetConnectionState(int controllerIndex)
        {
            ControllerConnectionState state = ControllerConnectionState.CONTROLLER_CONNECTION_STATE_NOT_INITIALIZED;
            NativeApi.NRControllerStateGetConnectionState(m_StateHandles[controllerIndex], ref state);
            return state;
        }

        /// <summary> Gets battery level. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The battery level. </returns>
        public int GetBatteryLevel(int controllerIndex)
        {
            int batteryLevel = -1;
            NativeApi.NRControllerStateGetBatteryLevel(m_StateHandles[controllerIndex], ref batteryLevel);
            return batteryLevel;
        }

        /// <summary> Query if 'controllerIndex' is charging. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> True if charging, false if not. </returns>
        public bool IsCharging(int controllerIndex)
        {
            int isCharging = 0;
            NativeApi.NRControllerStateGetCharging(m_StateHandles[controllerIndex], ref isCharging);
            return isCharging == 1;
        }

        /// <summary> Gets a pose. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The pose. </returns>
        public Pose GetPose(int controllerIndex, ulong hmdTime)
        {
            Pose controllerPos = Pose.identity;
            NativeMat4f mat4f = new NativeMat4f(Matrix4x4.identity);
            NativeResult result = NativeApi.NRControllerGetPose(m_ControllerHandle[controllerIndex], hmdTime, ref mat4f);
            if (result == NativeResult.Success)
                ConversionUtility.ApiPoseToUnityPose(mat4f, out controllerPos);
            return controllerPos;
        }

        /// <summary> Gets a gyro. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The gyro. </returns>
        public Vector3 GetGyro(int controllerIndex)
        {
            NativeVector3f vec3f = new NativeVector3f();
            NativeResult result = NativeApi.NRControllerStateGetGyroscope(m_StateHandles[controllerIndex], ref vec3f);
            if (result == NativeResult.Success)
                return vec3f.ToUnityVector3();
            return Vector3.zero;
        }

        /// <summary> Gets an accel. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The accel. </returns>
        public Vector3 GetAccel(int controllerIndex)
        {
            NativeVector3f vec3f = new NativeVector3f();
            NativeResult result = NativeApi.NRControllerStateGetAccelerometer(m_StateHandles[controllerIndex], ref vec3f);
            if (result == NativeResult.Success)
                return vec3f.ToUnityVector3();
            return Vector3.zero;
        }

        /// <summary> Gets a magnitude. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The magnitude. </returns>
        public Vector3 GetMag(int controllerIndex)
        {
            NativeVector3f vec3f = new NativeVector3f();
            NativeResult result = NativeApi.NRControllerStateGetMagnetometer(m_StateHandles[controllerIndex], ref vec3f);
            if (result == NativeResult.Success)
                return vec3f.ToUnityVector3();
            return Vector3.zero;
        }

        /// <summary> Gets button state. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The button state. </returns>
        public uint GetButtonState(int controllerIndex)
        {
            uint buttonPress = 0;
            NativeApi.NRControllerStateGetButtonState(m_StateHandles[controllerIndex], ref buttonPress);
            return buttonPress;
        }

        /// <summary> Gets button up. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The button up. </returns>
        public uint GetButtonUp(int controllerIndex)
        {
            uint buttonUp = 0;
            NativeApi.NRControllerStateGetButtonUp(m_StateHandles[controllerIndex], ref buttonUp);
            return buttonUp;
        }

        /// <summary> Gets button down. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The button down. </returns>
        public uint GetButtonDown(int controllerIndex)
        {
            uint buttonDown = 0;
            NativeApi.NRControllerStateGetButtonDown(m_StateHandles[controllerIndex], ref buttonDown);
            return buttonDown;
        }

        /// <summary> Query if 'controllerIndex' is touching. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> True if touching, false if not. </returns>
        public bool IsTouching(int controllerIndex)
        {
            uint touchState = 0;
            NativeApi.NRControllerStateTouchState(m_StateHandles[controllerIndex], ref touchState);
            return touchState == 1;
        }

        /// <summary> Gets touch up. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool GetTouchUp(int controllerIndex)
        {
            uint touchUp = 0;
            NativeApi.NRControllerStateGetTouchUp(m_StateHandles[controllerIndex], ref touchUp);
            return touchUp == 1;
        }

        /// <summary> Gets touch down. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool GetTouchDown(int controllerIndex)
        {
            uint touchDown = 0;
            NativeApi.NRControllerStateGetTouchDown(m_StateHandles[controllerIndex], ref touchDown);
            return touchDown == 1;
        }

        /// <summary> Gets a touch. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The touch. </returns>
        public Vector2 GetTouch(int controllerIndex)
        {
            NativeVector2f touchPos = new NativeVector2f();
            NativeResult result = NativeApi.NRControllerStateGetTouchPose(m_StateHandles[controllerIndex], ref touchPos);
            if (result == NativeResult.Success)
                return touchPos.ToUnityVector2();
            return Vector3.zero;
        }

        /// <summary> Gets a version. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <returns> The version. </returns>
        public string GetVersion(int controllerIndex)
        {
            if (m_ControllerHandle == null)
            {
                return "";
            }

            byte[] bytes = new byte[128];
            var result = NativeApi.NRControllerGetVersion(m_ControllerHandle[controllerIndex], bytes, bytes.Length);
            if (result == NativeResult.Success)
            {
                return System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            }
            else
            {
                return "";
            }
        }

        public HandednessType GetHandednessType(int controllerIndex)
        {
            HandednessType handedness_type = HandednessType.RIGHT_HANDEDNESS;
            var result = NativeApi.NRControllerGetHandheldType(m_ControllerHandle[controllerIndex], ref handedness_type);
            NativeErrorListener.Check(result, this, "GetHandednessType");
            return handedness_type;
        }

        private partial struct NativeApi
        {
            /// <summary> Create the controller group object. </summary>
            /// <param name="out_controller_group_handle"> [in,out] The handle of controller group object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGroupCreate(ref UInt64 out_controller_group_handle);

            /// <summary> Get the count of controller group. </summary>
            /// <param name="controller_group_handle"> The handle of controller group object. </param>
            /// <param name="out_group_count"> [in,out] The count of controller group. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGroupGetCount(UInt64 controller_group_handle,
                ref int out_group_count);

            /// <summary> Check whether the controller system which the group_index
            /// and the controller_type reference to are the same. </summary>
            /// <param name="controller_group_handle"> The handle of controller group object. </param>
            /// <param name="group_index"> The index of controller group. </param>
            /// <param name="controller_type"> The type of controller system. </param>
            /// <returns> True if the controller systems are the same, false otherwise. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern bool NRControllerGroupCheckControllerType(UInt64 controller_group_handle,
                int group_index, NRControllerType controller_type);

            /// <summary> Get the controller id of controller group. </summary>
            /// <param name="controller_group_handle"> The handle of controller group object. </param>
            /// <param name="group_index"> The index of controller group. </param>
            /// <param name="out_controller_id"> [in,out] The identifier of controller system. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGroupGetControllerId(UInt64 controller_group_handle,
                int group_index, ref int out_controller_id);

            /// <summary> Get the description of controller group. </summary>
            /// <param name="controller_group_handle"> The handle of controller group object. </param>
            /// <param name="group_index"> The index of controller group. </param>
            /// <param name="out_description"> [in,out] The description of controller system. </param>
            /// <param name="out_description_length"> [in,out] The length of description. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGroupGetDescription(UInt64 controller_group_handle,
                int group_index, ref IntPtr out_description, ref uint out_description_length);

            /// <summary> Get the supported features of controller group. </summary>
            /// <param name="controller_group_handle"> The handle of controller group object. </param>
            /// <param name="group_index"> The index of controller group. </param>
            /// <param name="out_supported_features"> [in,out] The supported features of controller group. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGroupGetSupportedFeatures(UInt64 controller_group_handle,
                int group_index, ref ControllerAvailableFeature out_supported_features);

            /// <summary> Release memory used by the controller group object. </summary>
            /// <param name="controller_group_handle"> The handle of controller group object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGroupDestroy(UInt64 controller_group_handle);

            /// <summary> Create the controller system object. </summary>
            /// <param name="controller_id"> The identifier of controller system. </param>
            /// <param name="out_controller_handle"> [in,out] The handle of controller system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerCreate(int controller_id, ref UInt64 out_controller_handle);

            /// <summary> Start the controller system object. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStart(UInt64 controller_handle);

            /// <summary> Stop the controller system object. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStop(UInt64 controller_handle);

            /// <summary> Release memory used by the controller system object. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerDestroy(UInt64 controller_handle);

            /// <summary> Pause the controller system object. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerPause(UInt64 controller_handle);

            /// <summary> Resume the controller system object. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerResume(UInt64 controller_handle);

            /// <summary> Notify the controller to recenter. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerRecenter(UInt64 controller_handle);

            /// <summary> Notify the controller to vibrate. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <param name="duration"> The nanoseconds of the vibration will last. </param>
            /// <param name="frequency"> The frequency of vibration in Hz. </param>
            /// <param name="amplitude"> The amplitude of the vibration between 0.0 and 1.0. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerHapticVibrate(UInt64 controller_handle,
                long duration, float frequency, float amplitude);

            /// <summary> Get the version of the controller. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <param name="out_version"> The buffer to store the version of the controller. </param>
            /// <param name="out_version_length"> The length of the out_version buffer. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGetVersion(UInt64 controller_handle,
                byte[] out_version, int out_version_length);

            /// <summary> Get the handedness left or right. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <param name="handedness_type"> The handedness type returned. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGetHandheldType(UInt64 controller_handle, ref HandednessType handedness_type);

            /// <summary> Get the connected type of the controller. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <param name="out_controller_type"> The type of the controller. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGetConnectedType(UInt64 controller_handle, ref ControllerType out_controller_type);

            /// <summary> Update the controller state. </summary>
            /// <param name="controller_handle"> The handle of controller system object. </param>
            /// <param name="out_controller_state_handle"> The handle of controller state. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateUpdate(UInt64 controller_handle, ref UInt64 out_controller_state_handle);

            /// <summary> Release memory used by the controller state object. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateDestroy(UInt64 controller_state_handle);

            /// <summary> Get the current pose of the controller. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="hmd_time_nanos"> The time to get the controller pose. </param>
            /// <param name="out_controller_pose"> The pose of the controller. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerGetPose(UInt64 controller_state_handle, ulong hmd_time_nanos, ref NativeMat4f out_controller_pose);

            /// <summary> Get the current gyroscope data of the controller. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_gyro"> The gyroscope data of the controller. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetGyroscope(UInt64 controller_state_handle, ref NativeVector3f out_controller_gyro);

            /// <summary> Get the current accelerometer data of the controller. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_accel"> The accelerometer data of the controller. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetAccelerometer(UInt64 controller_state_handle, ref NativeVector3f out_controller_accel);

            /// <summary> Get the current magnetometer data of the controller. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_mag"> The magnetometer data of the controller. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetMagnetometer(UInt64 controller_state_handle, ref NativeVector3f out_controller_mag);

            /// <summary> Get the connection state of the controller. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_connection_state"> The connection state of controller. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetConnectionState(UInt64 controller_state_handle, ref ControllerConnectionState out_controller_connection_state);

            /// <summary> Get the charging state of the controller. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_charging"> The charging state of the controller. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetCharging(UInt64 controller_state_handle, ref int out_controller_charging);

            /// <summary> Get the battery level of the controller. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_battery_level"> The battery level of the controler.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetBatteryLevel(UInt64 controller_state_handle, ref int out_controller_battery_level);

            /// <summary> Get the information of all buttons about which button is currently pressed. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_button_state"> The pressed state of all buttons. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetButtonState(UInt64 controller_state_handle, ref uint out_controller_button_state);

            /// <summary> Get the information of all buttons about which button was just released. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_button_up"> The data which indicates the just released state of all buttons. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetButtonUp(UInt64 controller_state_handle, ref uint out_controller_button_up);

            /// <summary> Get the information of all buttons about which button was just pressed. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_button_down"> The data which indicates the just pressed state of all buttons. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetButtonDown(UInt64 controller_state_handle, ref uint out_controller_button_down);

            /// <summary> Get the information of all touches about which touch is currently pressed. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_touch_state"> The pressed state of all touches. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateTouchState(UInt64 controller_state_handle, ref uint out_controller_touch_state);

            /// <summary> Get the information of all touches about which touch was just released. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_touch_up"> The data which indicates the just released state of all touches. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetTouchUp(UInt64 controller_state_handle, ref uint out_controller_touch_up);

            /// <summary> Get the information of all touches about which touch was just pressed. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_touch_down"> The data which indicates the just pressed state of all touches. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetTouchDown(UInt64 controller_state_handle, ref uint out_controller_touch_down);

            /// <summary> Get the pose of all touches. </summary>
            /// <param name="controller_state_handle"> The handle of controller state object. </param>
            /// <param name="out_controller_touch_pose"> The 2D pose of all touches. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRControllerStateGetTouchPose(UInt64 controller_state_handle, ref NativeVector2f out_controller_touch_pose);
        };
    }
}
