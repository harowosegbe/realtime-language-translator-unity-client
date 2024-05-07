/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    public class NRHandControllerStateParser : IControllerStateParser
    {
        /// <summary> The buttons down. </summary>
        private bool[] m_ButtonsDown = new bool[3];
        /// <summary> The buttons up. </summary>
        private bool[] m_ButtonsUp = new bool[3];
        /// <summary> The buttons. </summary>
        private bool[] m_Buttons = new bool[3];
        /// <summary> The down. </summary>
        private bool[] m_Down = new bool[3];

        /// <summary> Parser controller state. </summary>
        /// <param name="state"> The state.</param>
        public void ParserControllerState(ControllerState state)
        {
            lock (m_Buttons)
            {
                lock (m_Down)
                {
                    for (int i = 0; i < m_Buttons.Length; ++i)
                    {
                        m_Down[i] = m_Buttons[i];
                    }
                }

                m_Buttons[0] = state.isTouching;  //Trigger
                m_Buttons[1] = false;  //App
                m_Buttons[2] = false;  //Home

                lock (m_ButtonsUp)
                {
                    lock (m_ButtonsDown)
                    {
                        for (int i = 0; i < m_Buttons.Length; ++i)
                        {
                            m_ButtonsUp[i] = (m_Down[i] & !m_Buttons[i]);
                            m_ButtonsDown[i] = (!m_Down[i] & m_Buttons[i]);
                        }
                    }
                }
            }
            state.buttonsState =
                (m_Buttons[0] ? ControllerButton.TRIGGER : 0)
                | (m_Buttons[1] ? ControllerButton.APP : 0)
                | (m_Buttons[2] ? ControllerButton.HOME : 0);
            state.buttonsDown =
                (m_ButtonsDown[0] ? ControllerButton.TRIGGER : 0)
                | (m_ButtonsDown[1] ? ControllerButton.APP : 0)
                | (m_ButtonsDown[2] ? ControllerButton.HOME : 0);
            state.buttonsUp =
                (m_ButtonsUp[0] ? ControllerButton.TRIGGER : 0)
                | (m_ButtonsUp[1] ? ControllerButton.APP : 0)
                | (m_ButtonsUp[2] ? ControllerButton.HOME : 0);
        }
    }
}
