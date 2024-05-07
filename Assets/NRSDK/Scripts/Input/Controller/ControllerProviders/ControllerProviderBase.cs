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

    /// <summary> A controller provider base. </summary>
    public abstract class ControllerProviderBase
    {
        /// <summary> The states. </summary>
        protected ControllerState[] states;

        /// <summary> Gets or sets a value indicating whether the controller is running. </summary>
        /// <value> True if running, false if not. </value>
        public bool running { get; protected set; } = false;

        /// <summary> Constructor. </summary>
        /// <param name="states"> The states.</param>
        public ControllerProviderBase(ControllerState[] states)
        {
            this.states = states;
        }

        /// <summary> Gets the number of controllers. </summary>
        /// <value> The number of controllers. </value>
        public abstract int ControllerCount { get; }

        /// <summary> Start the controller. </summary>
        public virtual void Start()
        {
            running = true;
        }

        /// <summary> Pause the controller. </summary>
        public virtual void Pause()
        {
            running = false;
        }

        /// <summary> Resume the controller. </summary>
        public virtual void Resume()
        {
            running = true;
        }

        /// <summary> Updates this object. </summary>
        public abstract void Update();
    
        /// <summary> Destroy the controller. </summary>
        public virtual void Destroy()
        {
            running = false;
        }

        /// <summary> Trigger haptic vibration. </summary>
        /// <param name="controllerIndex"> Zero-based index of the controller.</param>
        /// <param name="durationSeconds"> (Optional) The duration in seconds.</param>
        /// <param name="frequency">       (Optional) The frequency.</param>
        /// <param name="amplitude">       (Optional) The amplitude.</param>
        public virtual void TriggerHapticVibration(int controllerIndex, float durationSeconds = 0.1f, float frequency = 200f, float amplitude = 0.8f) { }

        /// <summary> Recenters this object. </summary>
        public virtual void Recenter() { }
    }
}