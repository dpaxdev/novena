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
    public class NovenaBackgroundMaintenanceTask : IBackgroundTask
    {
        public  const string BackgroundTaskName = "NovenaBackgroundTask";
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        //
        // The Run method is the entry point of a background task.
        //
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get a deferral, to prevent the task from closing prematurely
            // while asynchronous code is still running.
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

           

            if (DateTime.TryParse(localSettings.Values["LastGeneralMaintenanceTime"].ToString(), out DateTime LastMaintenanceTime))
            {
                //if the task has been run today skip it
                if (LastMaintenanceTime.Date == DateTime.UtcNow.Date)
                    return;
            }
            // read all novenas and perform maintainance tasks to update current progress and deactivate finished novenas
            Helper.DoGeneralMaintenace();

            // Inform the system that the task is finished.
            deferral.Complete();
        }//end run

    }//end class backgroundtask

}//end namespace
