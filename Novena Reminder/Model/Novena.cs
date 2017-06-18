using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novena_Reminder
{
    public class Novena
    {
        public enum RecurrencePattern
        {
            Loop,
            RunOnce,
            RepeatNTimes
        }

        public Novena()
        {
            ID = Guid.NewGuid();
            InitializeDateTimeFields();
        }

        private void InitializeDateTimeFields()
        {
            StartDate =  DateTime.MinValue.ToUniversalTime();
            ScheduledStartDate = DateTime.MinValue.ToUniversalTime();
            AlarmTime = DateTime.MinValue.ToUniversalTime();
        }

        public Novena(Guid id)
        {
            ID = id;
            InitializeDateTimeFields();
        }

        public Guid ID { get;  set; }

        public string Name { get; set; }       

        public int Repetitions { get; set; }

        public int StartAt { get; set; }

        public DateTime StartDate { get; set; }

        public bool ScheduledStart { get; set; }

        public DateTime ScheduledStartDate { get; set; }

        public int Duration { get; set; }        

        public RecurrencePattern Recurrence { get; set; }

        public int Progress { get; }

        public bool Alarm { get; set; }

        public DateTime AlarmTime { get; set; }

        public bool IsActive { get; set; }

        public void UpdateProgress()
        {
            throw new System.NotImplementedException();
        }

        private bool CheckOngoing()
        {

            if (StartDate == DateTime.MinValue) //i.e. not set           
            {
                if (ScheduledStartDate <= DateTime.Now.Date)
                    StartDate = ScheduledStartDate;
                else return false; //the Novenna hasn't started yet or something's wrong
            }
            var days = GetDaysCompleted();

            if (days < 0) return false; //not started yet
            var repetitions = days / Duration;

            switch (Recurrence)
            {
                case RecurrencePattern.Loop:

                    break;
                case RecurrencePattern.RepeatNTimes:
                    if (repetitions >= Repetitions)
                    {
                        Deactivate();
                        return false; //just expired;
                    }
                    break;
                case RecurrencePattern.RunOnce:
                    if (repetitions <= 1)
                    {
                        Deactivate();
                        return false; //just expired
                    }
                    break;
            }
            return true;
        }
        private int GetDaysCompleted() {

            //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
            if (Duration == 0) return -1;

            if (StartDate == DateTime.MinValue) //i.e. not set           
            {
                if (ScheduledStartDate == DateTime.Now.Date)
                    StartDate = DateTime.Now.Date;
                else return -1; //no progress to report for a Novenna that hasn't started yet                
            }
            return (DateTime.Now.Date - StartDate.Date).Days + StartAt; 
        }
        private int GetCurentProgress()
        {
            int progress = -1;
            //we should never actually encounter this as we only call this method for active items:
            if (!IsActive)
                return -1;
            //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
            if (Duration == 0) return -1;
            if (!CheckOngoing()) return -1;

            var days = GetDaysCompleted();

            var repetitions = days / Duration;
          
            progress = days % Duration;

            return progress;
        }

        private void Deactivate()
        {
            //to-do: do various house-cleaning actions before deactivating maybe.
            IsActive = false;
        }
        private bool Activate()
        {
            //to do: if scheduled reset start date.
            //to-do: check if activation makes sense (if scheduled in the past for example);
            IsActive = true;
            return true;
        }
    }
}
