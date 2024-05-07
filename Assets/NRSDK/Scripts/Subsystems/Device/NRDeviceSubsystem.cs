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

using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static NRKernal.NRDevice;

#if USING_XR_SDK
using UnityEngine.XR;
#endif

namespace NRKernal
{
    /// <summary> Glasses event. </summary>
    /// <param name="eventtype"> The eventtype.</param>
    public delegate void GlassesEvent(NRDevice.GlassesEventType eventtype);
    /// <summary> Glasses disconnect event. </summary>
    /// <param name="reason"> The reason.</param>
    public delegate void GlassesDisconnectEvent(GlassesDisconnectReason reason);
    /// <summary> Glassed temporary level changed. </summary>
    /// <param name="level"> The level.</param>
    public delegate void GlassedTempLevelChanged(GlassesTemperatureLevel level);
    /// <summary> Session event. </summary>
    /// <param name="status"> The eventtype.</param>
    public delegate void SessionSpecialEvent(SessionSpecialEventType status);

    public class NRDeviceSubsystemDescriptor : IntegratedSubsystemDescriptor<NRDeviceSubsystem>
    {
        public const string Name = "Subsystem.HMD";
        public override string id => Name;
    }

    #region brightness KeyEvent on XrealLight.
    /// <summary> Values that represent nr brightness key events. </summary>
    public enum NRBrightnessKEYEvent
    {
        /// <summary> An enum constant representing the nr brightness key down option. </summary>
        NR_BRIGHTNESS_KEY_DOWN = 0,
        /// <summary> An enum constant representing the nr brightness key up option. </summary>
        NR_BRIGHTNESS_KEY_UP = 1,
    }

    /// <summary> Brightness key event. </summary>
    /// <param name="key"> The key.</param>
    public delegate void BrightnessKeyEvent(NRBrightnessKEYEvent key);
    /// <summary> Brightness value changed event. </summary>
    /// <param name="value"> The value.</param>
    public delegate void BrightnessValueChangedEvent(int value);

    /// <summary> Callback, called when the nr glasses control brightness key. </summary>
    /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
    /// <param name="key_event">              The key event.</param>
    /// <param name="user_data">              Information describing the user.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NRGlassesControlBrightnessKeyCallback(UInt64 glasses_control_handle, int key_event, UInt64 user_data);

    /// <summary> Callback, called when the nr glasses control brightness value. </summary>
    /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
    /// <param name="value">                  The value.</param>
    /// <param name="user_data">              Information describing the user.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NRGlassesControlBrightnessValueCallback(UInt64 glasses_control_handle, int value, UInt64 user_data);
    #endregion



    public class NRDeviceSubsystem : IntegratedSubsystem<NRDeviceSubsystemDescriptor>
    {
        internal static event GlassesEvent OnGlassesStateChanged;
        internal static event GlassesDisconnectEvent OnGlassesDisconnect;
        private NativeHMD m_NativeHMD = null;
        private NativeGlassesController m_NativeGlassesController = null;
        private Exception m_InitException = null;
        private static bool m_IsGlassesPlugOut = false;
        private static bool m_ResetStateOnNextResume = false;

        public UInt64 NativeGlassesHandler => m_NativeGlassesController.GlassesControllerHandle;
        public UInt64 NativeHMDHandler => m_NativeHMD.HmdHandle;
        public NativeHMD NativeHMD => m_NativeHMD;
        public bool IsAvailable => !m_IsGlassesPlugOut && running && m_InitException == null;


        /// <summary> Glass controll key event delegate for native. </summary>
        private delegate void NRGlassesControlKeyEventCallback(UInt64 glasses_control_handle, UInt64 key_event_handle, UInt64 user_data);
        /// <summary> Event queue for all listeners interested in OnBrightnessKeyCallback events. </summary>
        private static event BrightnessKeyEvent OnBrightnessKeyCallback;
        /// <summary> Event queue for all listeners interested in OnBrightnessValueCallback events. </summary>
        private static event BrightnessValueChangedEvent OnBrightnessValueCallback;

        /// <summary> The brightness minimum. </summary>
        public const int BRIGHTNESS_MIN = 0;
        /// <summary> The brightness maximum. </summary>
        private static int m_Brightness_Max = 7;
        public static int BrightnessMax => m_Brightness_Max;

#if USING_XR_SDK
        private const string k_idDisplaySubSystem = "NRSDK Display";

        private XRDisplaySubsystem m_XRDisplaySubsystem;
        public XRDisplaySubsystem XRDisplaySubsystem
        {
            get { return m_XRDisplaySubsystem; }
        }
#endif

        public NRDeviceSubsystem(NRDeviceSubsystemDescriptor descriptor) : base(descriptor)
        {
            NRDebugger.Info("[NRDeviceSubsystem] Create");
            m_NativeGlassesController = new NativeGlassesController();
            m_NativeHMD = new NativeHMD();

#if !UNITY_EDITOR
            try
            {
                m_NativeGlassesController.Create();
                m_NativeGlassesController.RegisGlassesWearCallBack(OnGlassesWear, 1);
                m_NativeGlassesController.RegistGlassesEventCallBack(OnGlassesDisconnectEvent);
#if USING_XR_SDK
                m_XRDisplaySubsystem = NRFrame.CreateXRSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(k_idDisplaySubSystem);
                m_NativeHMD.Create(NativeXRPlugin.GetHMDHandle());
#else
                m_NativeHMD.Create();
#endif
            }
            catch (Exception e)
            {
                m_InitException = e;
                throw e;
            }
            NRDebugger.Info("[NRDeviceSubsystem] Created");
#endif
        }

        #region LifeCycle
        public override void Start()
        {
            base.Start();

            NRDebugger.Info("[NRDeviceSubsystem] Start");
#if !UNITY_EDITOR
            m_NativeGlassesController?.Start();
            NRDevice.OnSessionSpecialEvent?.Invoke(SessionSpecialEventType.GlassesStarted);

            int outBrightnessMax = 0;
            var result = NativeApi.NRGlassesControlGetBrightnessLevelNumber(NativeGlassesHandler, ref outBrightnessMax);
            NativeErrorListener.Check(result, this, "NRGlassesControlGetBrightnessLevelNumber");
            if (result == NativeResult.Success)
                m_Brightness_Max  = outBrightnessMax - 1;

            NRDebugger.Info("[NRDeviceSubsystem] MaxBrightness  = {0}", m_Brightness_Max);

#if USING_XR_SDK
            XRDisplaySubsystem?.Start();
            NativeXRPlugin.RegistDisplaySubSystemEventCallback(DisplaySubSystemStart);
#else
            m_NativeHMD?.Start();
#endif

#endif
            NRDebugger.Info("[NRDeviceSubsystem] Started");
        }

        /// <summary> Executes the 'glasses wear' action. </summary>
        /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
        /// <param name="wearing_status">         The wearing status.</param>
        /// <param name="user_data">              Information describing the user.</param>
        [MonoPInvokeCallback(typeof(NRGlassesControlWearCallback))]
        private static void OnGlassesWear(UInt64 glasses_control_handle, int wearing_status, UInt64 user_data)
        {
            MainThreadDispather.QueueOnMainThread(() =>
            {
                OnGlassesStateChanged?.Invoke(wearing_status == 1 ? GlassesEventType.PutOn : GlassesEventType.PutOff);
            });
        }

        /// <summary> Executes the 'glasses disconnect event' action. </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs.</exception>
        /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
        /// <param name="user_data">              Information describing the user.</param>
        /// <param name="reason">                 The reason.</param>
        [MonoPInvokeCallback(typeof(NRGlassesControlNotifyQuitAppCallback))]
        private static void OnGlassesDisconnectEvent(UInt64 glasses_control_handle, IntPtr user_data, GlassesDisconnectReason reason)
        {
            if (m_IsGlassesPlugOut)
            {
                return;
            }
            m_IsGlassesPlugOut = true;
            OnGlassesDisconnect?.Invoke(reason);
        }

        [MonoPInvokeCallback(typeof(OnDisplaySubSystemStartCallback))]
        private static void DisplaySubSystemStart(bool start)
        {
            try
            {
                NRDevice.Subsystem?.OnDisplaySubSystemStart(start);
            }
            catch (Exception ex)
            {
                NRDebugger.Error("[NRDeviceSubsystem] DisplaySubSystemStart: {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void ResetStateOnNextResume()
        {
            m_ResetStateOnNextResume = true;
        }

        // Start of displaySubsystem is issued earlier while resuming.
        // Stop of displaySubsystem is issued earlier while pausing.
        void OnDisplaySubSystemStart(bool start)
        {
            NRDebugger.Info("[NRDeviceSubsystem] OnDisplaySubSystemStart: start={0}, running={1}", start, running);

            // Start of displaySubsystem while system is not running, meens a resuming event.
            if (start && !running)
            {
                // we do resuming of glassedControl here for XR.
                m_NativeGlassesController?.Resume();
                NRDevice.OnSessionSpecialEvent?.Invoke(SessionSpecialEventType.GlassesResumed);
            }
        }

        public override void Pause()
        {
            base.Pause();

#if !UNITY_EDITOR
#if USING_XR_SDK
            if (XRDisplaySubsystem != null && XRDisplaySubsystem.running)
            {
                NRDebugger.Warning("[NRDeviceSubsystem] Pause but XRDisplaySubsystem is running");
                // It it not necessary to issue Stop here, as it has been issued in native layer by unity engine.
                // XRDisplaySubsystem?.Stop();
            }
#else
            m_NativeHMD?.Pause();
#endif
            NRDevice.OnSessionSpecialEvent?.Invoke(SessionSpecialEventType.GlassesPrePause);
            m_NativeGlassesController?.Pause();
#endif
        }
        public override void Resume()
        {
            base.Resume();

            if (m_ResetStateOnNextResume)
            {
                m_ResetStateOnNextResume = false;
                m_IsGlassesPlugOut = false;
            }

#if !UNITY_EDITOR
#if USING_XR_SDK
            if (XRDisplaySubsystem != null && XRDisplaySubsystem.running)
            {
                NRDebugger.Warning("[NRDeviceSubsystem] Resume but XRDisplaySubsystem is not running");
                // It it not necessary to issue Start here, as it has been issued in native layer by unity engine.
                // XRDisplaySubsystem?.Start();
            }
#else
            m_NativeGlassesController?.Resume();
            NRDevice.OnSessionSpecialEvent?.Invoke(SessionSpecialEventType.GlassesResumed);

            m_NativeHMD?.Resume();
#endif
#endif
        }

        public override void Destroy()
        {
            base.Destroy();

            NRDebugger.Info("[NRDeviceSubsystem] Destroy");
#if !UNITY_EDITOR
            NRDevice.OnSessionSpecialEvent?.Invoke(SessionSpecialEventType.GlassesPreStop);
            m_NativeGlassesController?.Stop();
            m_NativeGlassesController?.Destroy();
            
#if USING_XR_SDK
            XRDisplaySubsystem?.Destroy();
            m_XRDisplaySubsystem = null;
#else
            m_NativeHMD.Stop();
            m_NativeHMD.Destroy();
#endif
            m_IsGlassesPlugOut = false;
            NRDebugger.Info("[NRDeviceSubsystem] Destroyed");
#endif
        }
        #endregion

        #region Glasses

        /// <summary> Gets current stereo mode of glasses. </summary>
        /// <value> The glasses stereo mode. </value>
        public NativeGlassesStereoMode GlassesStereoMode
        {
            get
            {
                if (!IsAvailable)
                {
                    throw new NRGlassesNotAvailbleError("Device is not available.");
                }
#if !UNITY_EDITOR
                return m_NativeGlassesController.GetStereoMode();
#else
                return NativeGlassesStereoMode.UnKnown;
#endif
            }
        }
        #endregion

        #region HMD
        public NRDeviceType GetDeviceType()
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }

#if !UNITY_EDITOR
            return m_NativeHMD.GetDeviceType();
#else
            return NRDeviceType.XrealLight;
#endif
        }

        public NRDeviceCategory GetDeviceCategory()
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }

#if !UNITY_EDITOR
            return m_NativeHMD.GetDeviceCategory();
#else
            return NRDeviceCategory.REALITY;
#endif
        }

        public bool IsFeatureSupported(NRSupportedFeature feature)
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }

#if !UNITY_EDITOR
            return m_NativeHMD.IsFeatureSupported(feature);
#else
            return true;
#endif
        }

        /// <summary> Gets the resolution of device. </summary>
        /// <param name="eye"> device index.</param>
        /// <returns> The device resolution. </returns>
        public NativeResolution GetDeviceResolution(NativeDevice device)
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }
#if !UNITY_EDITOR
            return m_NativeHMD.GetDeviceResolution(device);
#else
            return new NativeResolution(1920, 1080);
#endif
        }

        /// <summary> Gets device fov. </summary>
        /// <param name="eye">         The display index.</param>
        /// <param name="fov"> [in,out] The out device fov.</param>
        /// <returns> A NativeResult. </returns>
        public void GetEyeFov(NativeDevice device, ref NativeFov4f fov)
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }
#if !UNITY_EDITOR
            fov = m_NativeHMD.GetEyeFov(device);
#else
            fov = new NativeFov4f(0, 0, 1, 1);
#endif
        }

        /// <summary> Get the intrinsic matrix of device. </summary>
        /// <returns> The device intrinsic matrix. </returns>
        public NRDistortionParams GetDeviceDistortion(NativeDevice device)
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }
            NRDistortionParams result = new NRDistortionParams();
#if !UNITY_EDITOR
            m_NativeHMD.GetCameraDistortion(device, ref result);
#endif
            return result;
        }

        /// <summary> Get the intrinsic matrix of device. </summary>
        /// <returns> The device intrinsic matrix. </returns>
        public NativeMat3f GetDeviceIntrinsicMatrix(NativeDevice device)
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }
            NativeMat3f result = new NativeMat3f();
#if !UNITY_EDITOR
             m_NativeHMD.GetCameraIntrinsicMatrix(device, ref result);
#endif
            return result;
        }

        /// <summary> Get the project matrix of camera in unity. </summary>
        /// <param name="result"> [out] True to result.</param>
        /// <param name="znear">  The znear.</param>
        /// <param name="zfar">   The zfar.</param>
        /// <returns> project matrix of camera. </returns>
        public EyeProjectMatrixData GetEyeProjectMatrix(out bool result, float znear, float zfar)
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }
            result = false;
            EyeProjectMatrixData m_EyeProjectMatrix = new EyeProjectMatrixData();
#if !UNITY_EDITOR
            result = m_NativeHMD.GetProjectionMatrix(ref m_EyeProjectMatrix, znear, zfar);
#endif
            return m_EyeProjectMatrix;
        }

        /// <summary> Get the offset position between device and head. </summary>
        /// <value> The device pose from head. </value>
        public Pose GetDevicePoseFromHead(NativeDevice device)
        {
            if (!IsAvailable)
            {
                throw new NRGlassesNotAvailbleError("Device is not available.");
            }
#if !UNITY_EDITOR
            return m_NativeHMD.GetDevicePoseFromHead(device);
#else
            return Pose.identity;
#endif
        }
        #endregion

        #region brightness KeyEvent on XrealLight.
        /// <summary> Adds an event listener to 'callback'. </summary>
        /// <param name="callback"> The callback.</param>
        public void AddEventListener(BrightnessKeyEvent callback)
        {
            OnBrightnessKeyCallback += callback;
        }

        /// <summary>
        /// Adds an event listener to 'callback'. </summary>
        /// <param name="callback"> The callback.</param>
        public void AddEventListener(BrightnessValueChangedEvent callback)
        {
            OnBrightnessValueCallback += callback;
        }

        /// <summary> Removes the event listener. </summary>
        /// <param name="callback"> The callback.</param>
        public void RemoveEventListener(BrightnessKeyEvent callback)
        {
            OnBrightnessKeyCallback -= callback;
        }

        /// <summary> Removes the event listener. </summary>
        /// <param name="callback"> The callback.</param>
        public void RemoveEventListener(BrightnessValueChangedEvent callback)
        {
            OnBrightnessValueCallback -= callback;
        }


        /// <summary> Gets the brightness. </summary>
        /// <returns> The brightness. </returns>
        public int GetBrightness()
        {
            if (!IsAvailable)
            {
                return -1;
            }

#if !UNITY_EDITOR
            int brightness = -1;
            var result = NativeApi.NRGlassesControlGetBrightness(NativeGlassesHandler, ref brightness);
            return result == NativeResult.Success ? brightness : -1;
#else
            return 0;
#endif
        }

        /// <summary> Sets the brightness. </summary>
        /// <param name="brightness">        The brightness.</param>
        public void SetBrightness(int brightness)
        {
            if (!IsAvailable)
            {
                return;
            }

            AsyncTaskExecuter.Instance.RunAction(() =>
            {
#if !UNITY_EDITOR
                NativeApi.NRGlassesControlSetBrightness(NativeGlassesHandler, brightness);
#endif
            });
        }


        /// <summary> Executes the 'brightness key callback internal' action. </summary>
        /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
        /// <param name="key_event">              The key event.</param>
        /// <param name="user_data">              Information describing the user.</param>
        [MonoPInvokeCallback(typeof(NRGlassesControlBrightnessKeyCallback))]
        private static void OnBrightnessKeyCallbackInternal(UInt64 glasses_control_handle, int key_event, UInt64 user_data)
        {
            OnBrightnessKeyCallback?.Invoke((NRBrightnessKEYEvent)key_event);
        }

        /// <summary> Executes the 'brightness value callback internal' action. </summary>
        /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
        /// <param name="brightness">             The brightness.</param>
        /// <param name="user_data">              Information describing the user.</param>
        [MonoPInvokeCallback(typeof(NRGlassesControlBrightnessValueCallback))]
        private static void OnBrightnessValueCallbackInternal(UInt64 glasses_control_handle, int brightness, UInt64 user_data)
        {
            OnBrightnessValueCallback?.Invoke(brightness);
        }

        /// <summary> Event queue for all listeners interested in KeyEvent events. </summary>
        private static event NRGlassControlKeyEvent OnKeyEventCallback;

        /// <summary>
        /// Adds an key event listener. </summary>
        /// <param name="callback"> The callback.</param>
        public void AddKeyEventListener(NRGlassControlKeyEvent callback)
        {
            OnKeyEventCallback += callback;
        }

        /// <summary> Removes the key event listener. </summary>
        /// <param name="callback"> The callback.</param>
        public void RemoveKeyEventListener(NRGlassControlKeyEvent callback)
        {
            OnKeyEventCallback -= callback;
        }

        /// <summary> Executes the 'key event callback internal' action. </summary>
        /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
        /// <param name="key_event_handle">       Handle of the key event.</param>
        /// <param name="user_data">              Information describing the user.</param>
        [MonoPInvokeCallback(typeof(NRGlassesControlKeyEventCallback))]
        private static void OnKeyEventCallbackInternal(UInt64 glasses_control_handle, UInt64 key_event_handle, UInt64 user_data)
        {
            int keyType = 0;
            int keyFunc = 0;
            int keyParam = 0;

#if !UNITY_EDITOR
            NativeResult result = NativeApi.NRGlassesControlKeyEventGetType(glasses_control_handle, key_event_handle, ref keyType);
            if (result == NativeResult.Success)
                result = NativeApi.NRGlassesControlKeyEventGetFunction(glasses_control_handle, key_event_handle, ref keyFunc);
            if (result == NativeResult.Success)
                result = NativeApi.NRGlassesControlKeyEventGetParam(glasses_control_handle, key_event_handle, ref keyParam);
#endif
            NRKeyEventInfo keyEvtInfo = new NRKeyEventInfo();
            keyEvtInfo.keyType = (NRKeyType)keyType;
            keyEvtInfo.keyFunc = (NRKeyFunction)keyFunc;
            keyEvtInfo.keyParam = keyParam;

            OnKeyEventCallback?.Invoke(keyEvtInfo);
        }

        /// <summary>
        /// Regis glasses controller extra callbacks. </summary>
        public void RegisGlassesControllerExtraCallbacks()
        {
            if (!IsAvailable)
            {
                NRDebugger.Warning("[NRDevice] Can not regist event when glasses disconnect...");
                return;
            }

#if !UNITY_EDITOR
            NativeApi.NRGlassesControlSetBrightnessKeyCallback(NativeGlassesHandler, OnBrightnessKeyCallbackInternal, 0);
            NativeApi.NRGlassesControlSetBrightnessValueCallback(NativeGlassesHandler, OnBrightnessValueCallbackInternal, 0);
            NativeApi.NRGlassesControlSetKeyEventCallback(NativeGlassesHandler, OnKeyEventCallbackInternal, 0);
#endif
        }
        #endregion

        private struct NativeApi
        {
            /// <summary> Nr glasses control get brightness. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="out_brightness">         [in,out] The out brightness.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetBrightness(UInt64 glasses_control_handle, ref int out_brightness);

            /// <summary> Nr glasses control set brightness. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="brightness">             The brightness.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetBrightness(UInt64 glasses_control_handle, int brightness);


            /// <summary> Callback, called when the nr glasses control set brightness key. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="callback">               The callback.</param>
            /// <param name="user_data">              Information describing the user.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlSetBrightnessKeyCallback(UInt64 glasses_control_handle, NRGlassesControlBrightnessKeyCallback callback, UInt64 user_data);

            /// <summary> Callback, called when the nr glasses control set brightness value. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="callback">               The callback.</param>
            /// <param name="user_data">              Information describing the user.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlSetBrightnessValueCallback(UInt64 glasses_control_handle, NRGlassesControlBrightnessValueCallback callback, UInt64 user_data);

            /// <summary> Registe the callback when the nr glasses control issue key event. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="callback">               The called.</param>
            /// <param name="user_data">              Information describing the user.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlSetKeyEventCallback(UInt64 glasses_control_handle, NRGlassesControlKeyEventCallback callback, UInt64 user_data);

            /// <summary> Get key type of key event. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="key_event_handle">       Handle of key event.</param>
            /// <param name="out_key_event_type">     Key type retrieved.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlKeyEventGetType(UInt64 glasses_control_handle, UInt64 key_event_handle, ref int out_key_event_type);

            /// <summary> Get key function of key event. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="key_event_handle">       Handle of key event.</param>
            /// <param name="out_key_event_type">     Key funtion retrieved.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlKeyEventGetFunction(UInt64 glasses_control_handle, UInt64 key_event_handle, ref int out_key_event_function);

            /// <summary> Get key parameter of key event. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="key_event_handle">       Handle of key event.</param>
            /// <param name="out_key_event_type">     Key parameter retrieved.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlKeyEventGetParam(UInt64 glasses_control_handle, UInt64 key_event_handle, ref int out_key_event_param);

            /// </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control. </param>
            /// <param name="out_brightness_level_number"> return maximum brightness level. </param>
            /// <returns></returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlGetBrightnessLevelNumber(UInt64 glasses_control_handle, ref int out_brightness_level_number);

        }
    }
}