using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SmartHomeJablotron
{
    public abstract class ThreadSimple
    {
        protected Thread Thd = null;
        protected bool isRunning = false;

        public void Start()
        {
            if (Thd != null)
                throw (new Exception("Thread Already Running"));
            Thd = new Thread(new ParameterizedThreadStart(Run));
            isRunning = true;
            Thd.Start();
        }

        public void Stop()
        {
            if (Thd != null)
            {
                Console.WriteLine(">>> Stopping main thread...");
                isRunning = false;
                Thd.Join(30000);
                Console.WriteLine(">>> Main thread stopped.");
                Thd = null;
            }
            else
            {
                throw (new Exception("Thread Already Stopped"));
            }
        }

        protected abstract void Run(object obj);
    }
}
