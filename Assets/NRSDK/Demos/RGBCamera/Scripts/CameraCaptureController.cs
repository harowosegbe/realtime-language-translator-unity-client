/****************************************************************************
 * Copyright 2019 Xreal Techonology Limited. All rights reserved.
 *
 * This file is part of NRSDK.
 *
 * https://www.xreal.com/
 *
 *****************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace NRKernal.NRExamples
{
    /// <summary> A controller for handling camera captures. </summary>
    [HelpURL("https://developer.xreal.com/develop/unity/rgb-camera")]
    public class CameraCaptureController : MonoBehaviour
    {
        /// <summary> The capture image. </summary>
        public RawImage CaptureImage;
        /// <summary> Number of frames. </summary>
        public Text FrameCount;
        /// <summary> Gets or sets the RGB camera texture. </summary>
        /// <value> The RGB camera texture. </value>
        private NRRGBCamTexture RGBCamTexture { get; set; }

        void Start()
        {
            if (!NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_RGB_CAMERA))
                throw new Exception("RGBCamera is not supported on current glasses.");
            
            RGBCamTexture = new NRRGBCamTexture();
            CaptureImage.texture = RGBCamTexture.GetTexture();
            RGBCamTexture.Play();
        }

        /// <summary> Updates this object. </summary>
        void Update()
        {
            if (RGBCamTexture == null)
            {
                return;
            }
            FrameCount.text = RGBCamTexture.FrameCount.ToString();
        }

        /// <summary> Plays this object. </summary>
        public void Play()
        {
            if (!NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_RGB_CAMERA))
                throw new Exception("RGBCamera is not supported on current glasses.");
            
            if (RGBCamTexture == null)
            {
                RGBCamTexture = new NRRGBCamTexture();
                CaptureImage.texture = RGBCamTexture.GetTexture();
            }

            RGBCamTexture.Play();
            // The origin texture will be destroyed after call "Stop",
            // Rebind the texture.
            CaptureImage.texture = RGBCamTexture.GetTexture();
        }

        /// <summary> Pauses this object. </summary>
        public void Pause()
        {
            RGBCamTexture?.Pause();
        }

        public void Resume()
        {
            RGBCamTexture?.Resume();
        }

        /// <summary> Stops this object. </summary>
        public void Stop()
        {
            RGBCamTexture?.Stop();
            RGBCamTexture = null;
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        /// <summary> Executes the 'destroy' action. </summary>
        void OnDestroy()
        {
            RGBCamTexture?.Stop();
            RGBCamTexture = null;
        }
    }
}
