using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Threading;
using Novena_Reminder;
using Novena_Reminder.Controller;
using Novena_Reminder.Model;


namespace BackgroundTaskManager
{
    public class NovenaBackgroundMainenanceTask : IBackgroundTask
    {
        public const string BackgroundTaskName = "NovenaBackgroundTask";




        IBackgroundTaskInstance _taskInstance = null;

        //
        // The Run method is the entry point of a background task.
        //
        public void Run(IBackgroundTaskInstance taskInstance)
        {

            _taskInstance = taskInstance;
           Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");

            //
            // Query BackgroundWorkCost
            // Guidance: If BackgroundWorkCost is high, then perform only the minimum amount
            // of work in the background task and return immediately.
            //

            // read all novenas and perform maintainance tasks to update current progress and deactivate finished novenas



            //read novenas
            var Novenas = Storage.GetCollection();

            
            //perform maintainance
            foreach(Novena nov in Novenas)
            {
                if (nov.Maintenance())
                    Storage.SaveNovena(nov);
                //also manage alarms 
                if (nov.Alarm)
                {
                    Helper.ManageAlarms(nov);
                }
            }
        }
    }
}
