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

    /// <summary>
    /// Information about a subsystem that can be queried before creating a subsystem instance.
    /// </summary>
    public abstract class IntegratedSubsystemDescriptor : ISubsystemDescriptor
    {
        protected IntegratedSubsystemDescriptor() { }

        /// <summary>
        /// A unique string that identifies the subsystem that this Descriptor can create.
        /// </summary>
        public virtual string id { get; }

        public ISubsystem subsystem { get; protected set; }

        public abstract ISubsystem Create();
        public abstract void Destroy();
    }

    public class IntegratedSubsystemDescriptor<TSubsystem> : IntegratedSubsystemDescriptor where TSubsystem : IntegratedSubsystem
    {
        protected static Dictionary<string, TSubsystem> m_SubsystemDict = new Dictionary<string, TSubsystem>();

        public IntegratedSubsystemDescriptor() { }

        public override ISubsystem Create()
        {
            if (!m_SubsystemDict.ContainsKey(id))
            {
                try
                {
                    subsystem = (TSubsystem)Activator.CreateInstance(typeof(TSubsystem), this);
                    m_SubsystemDict.Add(id, (TSubsystem)subsystem);
                }
                catch (Exception e)
                {
                    NRDebugger.Error("Get the instance of Class({0}) faild.", typeof(TSubsystem).FullName);
                    throw e;
                }
            }
            return m_SubsystemDict[id];
        }

        public override void Destroy()
        {
            if (m_SubsystemDict.ContainsKey(id))
            {
                m_SubsystemDict.Remove(id);
            }
        }
    }
}
