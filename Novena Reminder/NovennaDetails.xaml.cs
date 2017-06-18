using Novena_Reminder.Controller;
using Novena_Reminder.Model;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Novena_Reminder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NovenaDetails : Page
    {


        static CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        private Novena nov;
        public NovenaDetails()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Realize the main page content.
            // FindName("RootPanel");

            cbDuration.Loaded += CbDuration_Loaded;
            cbRepeatNTimes.Loaded += CbRepeatNTimes_Loaded; 
        }

        private void CbRepeatNTimes_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.PopulateComboboxWithIntInterval(cbRepeatNTimes, 1, 30, 1);
        }

        private void CbDuration_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.PopulateComboboxWithIntInterval(cbDuration, 1, 30, 1);
            cbDuration.SelectionChanged += CbDuration_SelectionChanged;
        }

        private  void CbDuration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            Helper.PopulateComboboxWithIntInterval(cbStartAt, 1,  Helper.Combobox2Int(cb),1 );
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                nov = e.Parameter as Novena;
                lblNovennaDetailsActionType.Text = "Modifica ";
            }
            if (nov == null)
            {
                nov = new Novena();
                lblNovennaDetailsActionType.Text = "Adauga ";
                nov.Name = "test";

            }

            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += OnBackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested -= OnBackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }


        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            // Mark event as handled so we don't get bounced out of the app.
            e.Handled = true;
            // Page above us will be our master view.
            // Make sure we are using the "drill out" animation in this transition.
            Frame.Navigate(typeof(MainPage), "Back", new EntranceNavigationTransitionInfo());
        }

      

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            // Page above us will be our master view.
            // Make sure we are using the "drill out" animation in this transition.
            Frame.Navigate(typeof(MainPage), "Back", new EntranceNavigationTransitionInfo());
        }


        private  void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            nov = collectNovenaData();
            var task = new Task<bool>(() => SaveNovena());
            task.RunSynchronously();

           
            // Page above us will be our master view.
            // Make sure we are using the "drill out" animation in this transition.
          
          Frame.Navigate(typeof(MainPage), null, new EntranceNavigationTransitionInfo());
  

        }

        private bool SaveNovena()
        {
            var novs = Storage.GetCollection();

           

            if (novs != null && novs.Count > 0)
            {
                var iterator = novs.GetEnumerator();

                iterator.MoveNext();
                Novena current;
                while ((current = iterator.Current) != null)
                {

                    if (current.ID == nov.ID)
                    {
                        break;
                    }
                    iterator.MoveNext();
                }

                if (current != null)
                {
                    novs.Remove(current);
                }
            }
            novs.Insert(0, nov);
            return  Storage.SaveCollection(novs);
        }

        private Novena collectNovenaData()
        {
            var task = new Task<Novena>(() => Storage.GetNovenaById(nov.ID));
            task.RunSynchronously();
            var novena = task.Result;
            if (novena == null)
                novena = nov;

            novena.Alarm = chkAlarma.IsChecked.Value == true ? true : false;
          //  novena.AlarmTime = new DateTime(tpAlarmTime.Time.Ticks);
            int duration;
            int.TryParse(cbDuration.SelectedValue.ToString(), out duration);
            novena.Duration = duration;
            novena.IsActive = togIsActive.IsOn;
            novena.Name = txtNume.Text;
            //get recurrence type and repetitions no.
            if (chkRepeat.IsChecked == true)
            {
                if (rbInfiniteLoop.IsChecked == true)
                {
                    novena.Recurrence = Novena.RecurrencePattern.Loop;
                    novena.Repetitions = 0;
                }
                if (rbNtimes.IsChecked == true)
                {
                    int reps;
                    int.TryParse(cbRepeatNTimes.SelectedValue.ToString(), out reps);
                    novena.Repetitions = reps;
                    novena.Recurrence = Novena.RecurrencePattern.RepeatNTimes;
                }
            }
            else
            {
                novena.Recurrence = Novena.RecurrencePattern.RunOnce;
                novena.Repetitions = 0;
            }

            novena.ScheduledStart = chkDelayedStart.IsChecked == true;
        //    novena.ScheduledStartDate = new DateTime(dpScheduledDate.Date.Ticks);

            novena.StartAt =  Helper.Combobox2Int(cbStartAt);
            return novena;

        }
    }
}
