using Novena_Reminder.Controller;
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
            RunOnce,
            Loop,
            RepeatNTimes
        }

        public Novena()
        {
            ID = GenerateId();
            InitializeDateTimeFields();
        }

        private string GenerateId()
        {
            return Helper.EncodeId(DateTime.Now.ToUniversalTime().Ticks);
        }

        private void InitializeDateTimeFields()
        {
            StartDate =  DateTime.MinValue.ToUniversalTime();
            SchedStartDate = DateTime.MinValue.ToUniversalTime();
            AlarmTime = DateTime.MinValue.ToUniversalTime();
        }

        public Novena(string id)
        {
            ID = id;
            InitializeDateTimeFields();
        }

        public string ID { get;  set; }

        public string Name { get; set; }       

        public int Reps { get; set; }

        public int StartAt { get; set; } 

        public DateTime StartDate { get; set; }

        public bool SchedStart { get; set; }

        public DateTime SchedStartDate { get; set; }

        public int Duration { get; set; }        

        public RecurrencePattern Recurrence { get; set; }

        public int Progress { get; }

        public bool Alarm { get; set; }

        public DateTime AlarmTime { get; set; }

        public bool IsActive { get; set; }

        public bool RecurrenceIsOn { get { return Recurrence != RecurrencePattern.RunOnce; } }
                

        public bool Ongoing
        {
            get
            {
                if (!IsActive) return false;

                if (StartDate == DateTime.MinValue) //i.e. not set           
                {
                    if (SchedStartDate <= DateTime.Now.Date)
                        StartDate = SchedStartDate;
                    else return false; //the Novenna hasn't started yet or something's wrong
                }
                var days = DaysCompleted;

                if (days < 0) return false; //not started yet
                var repetitions = days / Duration;

                switch (Recurrence)
                {
                    case RecurrencePattern.Loop:

                        break;
                    case RecurrencePattern.RepeatNTimes:
                        if (repetitions >= Reps)
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
        }
        public int CurrentIteration
        {
            get
            {
                if (!Ongoing ) return -1;
                //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
                if (Duration == 0) return -1;

                var result = DaysCompleted  / Duration;
                return result;

            }
        }
        public int DaysCompleted {
            get
            {

                //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
                if (Duration == 0) return -1;

                if (StartDate == DateTime.MinValue) //i.e. not set           
                {
                    if (SchedStartDate == DateTime.Now.Date)
                        StartDate = DateTime.Now.Date;
                    else return -1; //no progress to report for a Novenna that hasn't started yet                
                }
                return (DateTime.Now.Date - StartDate.Date).Days + StartAt;
            }
        }
        public int CurentProgress //returns the current day of the novena if novena is active and has already started
        {
            get
            {
                int progress = -1;
                 if (!IsActive)
                    return -1;
                //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
                if (Duration == 0) return -1;
                if (Ongoing) return -1; //not started yet

               if (DaysCompleted < 0) return -1; //not started yet maybe                

                progress = DaysCompleted % Duration;

                return progress;
            }
        }

        public void Deactivate()
        {
            //to-do: do various house-cleaning actions before deactivating maybe.
            IsActive = false;
        }
        public bool Activate()
        {
            //to do: if scheduled reset start date.
            //to-do: check if activation makes sense (if scheduled in the past for example);

            IsActive = true;
            return true;
        }
    }
}
