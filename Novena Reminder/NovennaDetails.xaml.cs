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

            cbDuration.TextChanged += CbDuration_TextChanged;
            cbStartAt.Loaded += CbStartAt_Loaded;
        }

        private void CbStartAt_Loaded(object sender, RoutedEventArgs e)
        {
            PopuplateCBStartAt();
        }



        private void ComboboxSetSelectedValue(ComboBox comboBox, object value)
        {
            var selected = new ComboBoxItem();
            selected.Content = value;
            comboBox.SelectedItem = value;
        }

        private void CbDuration_TextChanged(object sender, TextChangedEventArgs e)
        {
            PopuplateCBStartAt();
        }



        private void PopuplateCBStartAt()
        {
            Helper.PopulateComboboxWithIntInterval(cbStartAt, 1, ParseValueToInt(cbDuration.Text), 1);
            ComboboxSetSelectedValue(cbStartAt, nov.StartAt);
        }

        private int ParseValueToInt(string text)
        {
            int parsed = 0;
            int.TryParse(text, out parsed);
            return parsed;
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
            NavigateToMainPage();
        }



        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            bool DialogResult = await  ShowNovenaDeleteDialog();
            if (DialogResult)
            {
                Storage.DeleteNovena(nov.ID);
                NavigateToMainPage();
            }
        }

        private async Task<bool> ShowNovenaDeleteDialog()
        {

            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Stergere novena",
                Content = "Sigur doriti sa stergeti aceasta novena?",
                PrimaryButtonText = "Sterge",
                SecondaryButtonText = "Nu"
            };
           
            if (nov.Ongoing)
                deleteDialog.Content = "Aceasta novena este in desfasurare.\n" + deleteDialog.Content; 
            ContentDialogResult result = await deleteDialog.ShowAsync();

            // Delete the novena if the user clicked the primary button.
           
            if (result == ContentDialogResult.Primary)
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigateToMainPage();
        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            SaveNovena();
            NavigateToMainPage();

        }

        private void NavigateToMainPage()
            
        {
            
            Frame.Navigate(typeof(MainPage), "Back", new EntranceNavigationTransitionInfo());
        }

        private void SaveNovena()
        {
            nov = collectNovenaData();
            Storage.SaveNovena(nov);
        }

        private Novena collectNovenaData()
        {

            var novena = Storage.GetNovenaById(nov.ID);
            if (novena == null)
                novena = nov;

            novena.Alarm = chkAlarma.IsChecked.Value == true ? true : false;
            novena.AlarmTime = new DateTime(tpAlarmTime.Time.Ticks);

            novena.Duration = ParseValueToInt(cbDuration.Text);
            novena.IsActive = togIsActive.IsOn;
            novena.Name = txtNume.Text;
            //get recurrence type and repetitions no.
            if (chkRepeat.IsChecked == true)
            {
                if (rbInfiniteLoop.IsChecked == true)
                {
                    novena.Recurrence = Novena.RecurrencePattern.Loop;
                    novena.Reps = 0;
                }
                if (rbNtimes.IsChecked == true)
                {

                    novena.Reps = ParseValueToInt(cbRepeatNTimes.Text);
                    novena.Recurrence = Novena.RecurrencePattern.RepeatNTimes;
                }
            }
            else
            {
                novena.Recurrence = Novena.RecurrencePattern.RunOnce;
                novena.Reps = 0;
            }

            novena.SchedStart = chkDelayedStart.IsChecked == true;
            novena.SchedStartDate = new DateTime(dpScheduledDate.Date.Ticks);

            novena.StartAt = Helper.Combobox2Int(cbStartAt);
            return novena;

        }




    }
}
