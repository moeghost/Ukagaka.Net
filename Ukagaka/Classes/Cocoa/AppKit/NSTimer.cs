using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Ukagaka;
using Ukagaka.Classes.Cocoa.AppKit;
using Utils;

namespace Cocoa.AppKit
{
    public class NSTimer:NSObject
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private EventHandler eventHandler;
        private NSSelector nSSelector;
        private object value;
        private bool isRepeat;
        private double time;
        public NSTimer(double time) 
       {
            this.time = time;
       }

        public NSTimer()
        {
         //   this.time = time;
        }

        public NSTimer(double time, EventHandler eventHandler, NSSelector nSSelector, object value, bool isRepeat) : this(time)
        {
            this.eventHandler = eventHandler;
            this.nSSelector = nSSelector;
            this.value = value;
            this.isRepeat = isRepeat;

            this.timer.Interval = TimeSpan.FromSeconds(time);
            timer.Tick += eventHandler;

        }

        public static NSTimer CreateRepeatingTimer(double time, EventHandler eventHandler, object value)
        {
             return new NSTimer(time, eventHandler, null,value,true);
        }

        internal void Schedule(TimerTask timerClickTask, long interval)
        {
           // timer = new Timer(timerClickTask.Callback,interval);
        }

        public void Start()
        {
            timer.IsEnabled = true;
            timer.Start();
        }

        public void Invalidate()
        {
            timer.IsEnabled = false;
            timer.Stop();

        }

        internal static NSTimer CreateTimer(double time, EventHandler eventHandler, object value)
        {
             
            return new NSTimer(time, eventHandler, null, value, false);
        }
    }
}
