using Novena_Reminder.Controller;
using Novena_Reminder.Model;
using System;
using System.Collections.Generic;
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
            comboBox.SelectedItem = value;
        }

        private void CbDuration_TextChanged(object sender, TextChangedEventArgs e)
        {
            PopuplateCBStartAt();
        }



        private void PopuplateCBStartAt()
        {
            Helper.PopulateComboboxWithIntInterval(cbStartAt, 1, ParseValueToInt(cbDuration.Text), 1);
            if (nov.StartAt > 0)
                ComboboxSetSelectedValue(cbStartAt, nov.StartAt);
            else
                ComboboxSetSelectedValue(cbStartAt, 1);
        }

        private int ParseValueToInt(string text)
        {
            int.TryParse(text, out int parsed);
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
                nov.Name = "Novena";

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
            bool DialogResult = await Helper.ShowNovenaDeleteDialog(nov);
            if (DialogResult)
            {
                Storage.DeleteNovena(nov.ID);
                NavigateToMainPage();
            }
        }

       


        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigateToMainPage();
        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (DoValidation())
            {
                SaveNovena();
                NavigateToMainPage();
            }

        }

        private bool DoValidation()
        {

            Dictionary<string, object> Errors = new Dictionary<string, object>();
            if (txtNume.Text == "")
            {
                Errors.Add("Specificati un nume pentru novena.", txtNume);
            }

            if (cbDuration.Text == "")
            {
                Errors.Add("Indicati durata in zile a novenei.", cbDuration);
            }
            else
            {
                var isInt = int.TryParse(cbDuration.Text, out int duration);
                if (!isInt)
                    Errors.Add("Durata novenei trebuie sa fie un numar.", cbDuration);
                else if (duration <= 0)
                    Errors.Add("Durata novenei trebuie sa fie mai mare decat 0.", cbDuration);
            }
            if (chkDelayedStart.IsChecked == true && dpScheduledDate.Date < DateTime.Today)
                Errors.Add("Ati ales inceperea cu intarziere a novenei. Nu puteti programa inceperea novenei in trecut, alegeti o data din viitor.", cbDuration);


            if (Errors.Count > 0)
            {
                ShowErrors(Errors);
                return false;
            }
            else return true;
        }

        private void ShowErrors(Dictionary<string, object> errors)
        {
            string Output = errors.Count>1?"Corectati urmatoarele erori pentru a putea salva:\n":"Corectati eroarea urmatoare pentru a putea salva:\n";
           
            foreach (KeyValuePair<string, object> kv in errors)
            {
                Output += "\n\u2022 " + kv.Key;
                HighlightInputError(kv.Value);
            }
            Helper.ShowDialog("Novena nu poate fi salvata", Output);
        }

        private void HighlightInputError(object value)
        {

            switch (value.GetType().Name)
            {
                case "TextBox":
                    ((TextBox)value).Style = (Style)Resources["tbHighlight"];
                    ((TextBox)value).TextChanged += NovenaDetails_TextChanged;
                    break;
                case "DatePicker":
                    ((DatePicker)value).Style = (Style)Resources["dpHighlight"];
                    ((DatePicker)value).DateChanged += NovenaDetails_DateChanged;
                    break;

                default:
                    break;

            }
            
        }

        private void NovenaDetails_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            var dp = sender as DatePicker;
            dp.Style = new Style() { TargetType = typeof(DatePicker) };
            dp.DateChanged -= NovenaDetails_DateChanged;
        }

        private void NovenaDetails_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.Style = new Style() { TargetType = typeof(TextBox) };
            tb.TextChanged -= NovenaDetails_TextChanged;
        }

        private void NavigateToMainPage()
        {
            Frame.Navigate(typeof(MainPage), "Back", new EntranceNavigationTransitionInfo());
        }

        private void SaveNovena()
        {

            Novena newNov = null;
            bool success = false;

            try
            {
                newNov = CollectNovenaData();
                success = true;
            }
            catch (InvalidOperationException ex)
            {
                Helper.ShowDialog("Novena nu poate fi activata", ex.Message);
                togIsActive.IsOn = false;
            }
            if (success == true)
            {
                nov = newNov;
                Storage.SaveNovena(nov);
            }


        }

        private Novena CollectNovenaData()
        {

            var novena = Storage.GetNovenaById(nov.ID);
            if (novena == null)
                novena = nov;

            var active = nov.IsActive;
            if (active && togIsActive.IsOn == false)
                novena.Deactivate();
            if (!active && togIsActive.IsOn == true)
                novena.Activate();   //will throw InvalidOperationException in certain cases which we need to catch outside of this method
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

        private void ChkDelayedStart_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var chk = sender as CheckBox;


            if (chk.IsChecked == true && dpScheduledDate.Date < DateTime.Today)
            {
                dpScheduledDate.Date = DateTime.Today;
            }

            if (chk.IsChecked == false)
            {
                dpScheduledDate.Date = nov.SchedStartDate;
            }
        }
    }
}
