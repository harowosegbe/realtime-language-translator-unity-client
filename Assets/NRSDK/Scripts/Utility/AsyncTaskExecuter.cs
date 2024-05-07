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
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary> Only works at Android runtime. </summary>
    public class AsyncTaskExecuter : SingleTon<AsyncTaskExecuter>
    {
        /// <summary> Queue of tasks. </summary>
        public Queue<Action> m_TaskQueue = new Queue<Action>();
        private Thread m_WorkThread = null;
        private bool m_IsRunning = false;
#if !UNITY_EDITOR
        public AsyncTaskExecuter()
        {
            NRDebugger.Info("[AsyncTaskExecuter] Start");
            m_IsRunning = true;
            m_WorkThread = new Thread(RunAsyncTask);
            m_WorkThread.IsBackground = true;
            m_WorkThread.Name = "AsyncTaskExecuter";
            m_WorkThread.Start();
            NRDebugger.Info("[AsyncTaskExecuter] Started");
        }

        private void RunAsyncTask()
        {
            while (m_IsRunning)
            {
                Thread.Sleep(5);
                if (m_TaskQueue.Count != 0)
                {
                    Action task = null;
                    lock (m_TaskQueue)
                    {
                        task = m_TaskQueue.Dequeue();
                    }
                    try
                    {
                        task?.Invoke();
                    }
                    catch (Exception e)
                    {
                        NRDebugger.Error("[AsyncTaskExecuter] Execute async task error:" + e.ToString());
                        throw;
                    }
                }
            }
        }
#endif

        /// <summary> Executes the action. </summary>
        /// <param name="task"> The task.</param>
        public void RunAction(Action task)
        {
            lock (m_TaskQueue)
            {
#if UNITY_EDITOR
                if (true)
                {
                    task?.Invoke();
                }
                else
#endif
                { 
                    m_TaskQueue.Enqueue(task); 
                }
            }
        }

        /// <summary> Executes a task witch has a timeout opration. </summary>
        /// <param name="task">            The task.</param>
        /// <param name="timeoutOpration"> The timeout opration.If the task does not time out, it is not
        ///                                executed.</param>
        /// <param name="timeout">           The duration of timeout.</param>
        /// <param name="runInMainThread"> Run the action in unity main thread.</param>
        internal void RunAction(Action task, Action timeoutOpration, float timeout, bool runInMainThread)
        {
            var cancleToken = new CancellationTokenSource();
            if (timeout > 0 && timeoutOpration != null)
            {
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay((int)(timeout * 1000));
                    if (cancleToken.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        NRDebugger.Info("[AsyncTaskExecuter] Run action timeout...");
                        timeoutOpration?.Invoke();
                    }
                    catch (Exception e)
                    {
                        NRDebugger.Error("[AsyncTaskExecuter] Run action timeout exeption: {0}\n{1}", e.Message, e.StackTrace);
                    }
                }, cancleToken.Token);
            }

            if (runInMainThread)
            {
                MainThreadDispather.QueueOnMainThread(() =>
                {
                    try
                    {
                        task?.Invoke();
                    }
                    catch (Exception e)
                    {
                        NRDebugger.Error("[AsyncTaskExecuter] Run action in main thread exeption: {0}\n{1}", e.Message, e.StackTrace);
                    }
                    cancleToken.Cancel();
                });
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        task?.Invoke();
                    }
                    catch (Exception e)
                    {
                        NRDebugger.Error("[AsyncTaskExecuter] Run action exeption: {0}\n{1}", e.Message, e.StackTrace);
                    }
                    cancleToken.Cancel();
                });
            }
        }
    }
}