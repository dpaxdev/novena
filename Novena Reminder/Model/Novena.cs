using Novena_Reminder.Controller;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Novena_Reminder
{
    [DataContract(Name = "Nov")]
    public class Novena
    {
        public enum RecurrencePattern
        {
            RunOnce,
            Loop,
            RepeatNTimes
        }

        //Constructors
        public Novena()
        {
            ID = GenerateId();
            InitializeDateTimeFields();
        }

        public Novena(string id)
        {
            ID = id;
            InitializeDateTimeFields();
        }

        //Properties
        [DataMember(Name = "ID")]
        public string ID { get; set; }

        [DataMember(Name = "N")]
        public string Name { get; set; }

        [DataMember(Name = "I")]
        public string Intention { get; set; }


        [DataMember(Name = "iA")]
        public bool IsActive { get; set; }

        [DataMember(Name = "Re")]
        public int Repetitions { get; set; }

        [DataMember(Name = "SA")]
        public int StartAt { get; set; }

        [DataMember(Name = "SD")]
        public DateTime StartDate { get; set; }

        [DataMember(Name = "SS")]
        public bool ScheduledStart { get; set; }

        [DataMember(Name = "SSD")]
        public DateTime ScheduledStartDate { get; set; }

        [DataMember(Name = "D")]
        public int Duration { get; set; }

        [DataMember(Name = "W")]
        public int Weekday { get; set; }

        [DataMember(Name = "R")]
        public RecurrencePattern Recurrence { get; set; }

        [DataMember(Name = "A")]
        public bool Alarm { get; set; }

        [DataMember(Name = "AT")]
        public DateTime AlarmTime { get; set; }

        [DataMember(Name = "AS")]
        public string AlarmSound { get; set; }


        //Read-Only Properties (complex getters)

        public bool RecurrenceIsOn { get { return Recurrence != RecurrencePattern.RunOnce; } }

        public bool StartDateIsInitial { get { return StartDate == DateTime.MinValue || StartDate == DateTime.MinValue.ToUniversalTime(); } }

        public bool IsOngoing
        {
            get
            {
                if (!IsActive) return false;

                if (StartDateIsInitial) //i.e. not set           
                    return false; //the Novenna hasn't started yet or something's wrong


                if (StartDate.Date > DateTime.Today)
                    return false; //it's in the future. some error must have occured => maintenance should be done.

                var days = DaysCompleted;

                if (days < 0) return false; //not started yet


                switch (Recurrence)
                {
                    case RecurrencePattern.Loop:
                        //if we are here it means StartDate is in the past. since we have a neverending loop, this means the novena is ongoing.
                        // return true; //we default to return true so no need to exit the switch here
                        break;
                    case RecurrencePattern.RepeatNTimes:
                        if (RemainingIterations < 0)
                            return false;
                        break;
                    case RecurrencePattern.RunOnce:
                        if (days + 1 > Duration)
                            return false; //we are past novena duration. this should never occur. we should call maintenance, but we cannot call it from here.                       
                        break;
                }
                return true;
            }
        }

        public int CurrentIteration
        {
            get
            {
                if (Duration == 0) return -1; //avoid division by zero 

                var result = (DaysCompleted / Duration) + 1;
                return result;

            }
        }

        public int RemainingIterations //ongoing iteration is counted as completed
        {
            get { return Repetitions - CurrentIteration; }
        }

        public int DaysCompleted
        {
            get
            {
                if (!IsActive) return -1;

                if (StartDateIsInitial) //i.e. not set           
                {
                    return -1; //no progress to report for a Novenna that hasn't started yet                
                }
                if (StartDate > DateTime.Today)
                    return -1; //it's in the future. some error must have occured => maintenance should be done.
                if(Weekday==0)
                    return (DateTime.Today - StartDate).Days + StartAt - 1;
                else
                {      
                    return  Helper.CountDays((DayOfWeek)(Weekday-1),StartDate, DateTime.Now);

                }
            }
        }

       

        public int CurrentProgress //returns the current day of the novena if novena is active and has already started
        {
            get
            {
                int progress = -1;
                if (!IsActive)
                    return -1;
                //we should never encounter this but we should check nontheless, else we might divide with 0 and the world would break;
                if (Duration == 0) return -1;
                //   if (!IsOngoing) return 0; //not started yet

                if (DaysCompleted < 0) return -1; //not started yet maybe                

                progress = DaysCompleted % Duration;

                return progress + 1;
            }
        }

        //Methods
        private string GenerateId()
        {
            return Helper.EncodeId(DateTime.Now.ToUniversalTime().Ticks);
        }

        private void InitializeDateTimeFields()
        {
            StartDate = DateTime.MinValue.ToUniversalTime();
            ScheduledStartDate = DateTime.MinValue.ToUniversalTime();
            AlarmTime = DateTime.MinValue.ToUniversalTime();

        }

        public bool DoMaintenance()
        {
            bool changed = false;// we change this flag to notify the novena has changed and should be saved.
            if (IsActive)
            {

                //check if novena is active but hasn't started yet
                if (StartDateIsInitial)
                {
                    //if it is scheduled we need to check if it shouldn't start by now
                    if (ScheduledStart)
                    {
                        if (ScheduledStartDate <= DateTime.Today)
                        {
                            //so it should've started already or it should start now
                            //this also takes care of the case when the device has been off 
                            //and the novena should have started in the meantime.
                            StartDate = ScheduledStartDate;
                            changed = true;
                        }
                    }
                    else
                    {
                        //there is no scheduled start, the StartDate is just not set.
                        //this happens usually when the novena has just been activated
                        StartDate = DateTime.Today;
                        changed = true;
                    }
                }
                else //StartDate has been set which means the novena should be ongoing.
                {
                    //unless it's not in which case we should deactivate it, because it probably just ended
                    if (!IsOngoing)
                    {
                        Deactivate();
                        changed = true;
                    }
                }
            }
            else //novena is not active
            {
                //we do nothing for inactive novenas
            }
            return changed;
        }

        public void Activate()
        {
            if (ScheduledStart && ScheduledStartDate < DateTime.Today)
            {
                throw new InvalidOperationException("e0004"); //Data de inceput este programata in trecut
            }
            IsActive = true;
            DoMaintenance();
        }

        public void Deactivate()
        {
            IsActive = false;
            StartDate = DateTime.MinValue.ToUniversalTime();
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
