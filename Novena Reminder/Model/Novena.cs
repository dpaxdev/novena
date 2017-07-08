﻿using Novena_Reminder.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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
            StartDate = DateTime.MinValue.ToUniversalTime();
            SchedStartDate = DateTime.MinValue.ToUniversalTime();
            AlarmTime = DateTime.MinValue.ToUniversalTime();
        }

        public Novena(string id)
        {
            ID = id;
            InitializeDateTimeFields();
        }

        public void Maintenance()
        {
            if (IsActive)
            {
                if (StartDate == DateTime.MinValue.ToUniversalTime())
                {
                    if (SchedStart)
                    {
                        if (SchedStartDate <= DateTime.Now)
                        {
                            StartDate = SchedStartDate;
                        }
                    }
                    else
                    {
                        StartDate = DateTime.Now;
                    }
                }
            }
        }

        public void Activate()
        {

            if (SchedStart && SchedStartDate < DateTime.Now)
            {
                throw new InvalidOperationException("Data de inceput este programata in trecut");

            }
            IsActive = true;
            Maintenance();
        }

        public void Deactivate()
        {
            IsActive = false;
            StartDate = DateTime.MinValue;
        }

        public string ID { get; set; }

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


        public bool IsOngoing
        {
            get
            {
                if (!IsActive) return false;

                if (StartDate == DateTime.MinValue) //i.e. not set           
                    return false; //the Novenna hasn't started yet or something's wrong

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
                if (!IsOngoing) return -1;
                //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
                if (Duration == 0) return -1;

                var result = DaysCompleted / Duration;
                return result;

            }
        }
        public int DaysCompleted
        {
            get
            {
                //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
                if (Duration == 0) return -1;

                if (StartDate == DateTime.MinValue) //i.e. not set           
                {
                    return -1; //no progress to report for a Novenna that hasn't started yet                
                }
                return (DateTime.Now.Date - StartDate.Date).Days + StartAt;
            }
        }
        public int CurrentProgress //returns the current day of the novena if novena is active and has already started
        {
            get
            {
                int progress = 0;
                if (!IsActive)
                    return -1;
                //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
                if (Duration == 0) return 0;
                if (!IsOngoing) return 0; //not started yet

                if (DaysCompleted < 0) return 0; //not started yet maybe                

                progress = DaysCompleted % Duration;

                return progress;
            }
        }

        [OnSerializing]
        public void OnSerializing(StreamingContext context)
        {
            var properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(DateTime) && property.GetValue(this).Equals(DateTime.MinValue))
                {
                    property.SetValue(this, DateTime.MinValue.ToUniversalTime());
                }
            }
        }
    }
}
#if true

#endif
