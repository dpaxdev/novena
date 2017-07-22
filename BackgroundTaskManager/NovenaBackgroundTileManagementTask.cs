using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Threading;

namespace BackgroundTaskManager
{
    public class NovenaBackgroundTileManagementTask : IBackgroundTask
    {
        public const string BackgroundTaskName = "NovenaBackgroundTileManagementTask";



        BackgroundTaskDeferral _deferral = null;

        IBackgroundTaskInstance _taskInstance = null;

        //
        // The Run method is the entry point of a background task.
        //
        public void Run(IBackgroundTaskInstance taskInstance)
        {


            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");

            //
            // Query BackgroundWorkCost
            // Guidance: If BackgroundWorkCost is high, then perform only the minimum amount
            // of work in the background task and return immediately.
            //

            // read all novenas and perform maintainance tasks to update current progress and deactivate finished novenas


            //read novenas


            //perform maintainance



            //deactivate finished novenas
        }
    }
}
