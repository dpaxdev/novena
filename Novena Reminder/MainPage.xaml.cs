using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Novena_Reminder.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Novena_Reminder.Controller;
using Windows.UI.Notifications;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Novena_Reminder
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

        private bool MultiSelectMode;


        public ObservableCollection<Novena> Novenas
        {
            get
            {
                return _Novenas;
            }
            set
            {
                if (_Novenas != value)
                {
                    _Novenas = value;
                    OnPropertyChanged("Novenas");
                }
            }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<Novena> _Novenas;




        public bool Initializing { get; private set; }

        public MainPage()
        {
            Initializing = true;
            InitializeComponent();

            Loaded += (s, e) =>
                 {
                     ResetListView();
                     SetMultipleSelectionMode(false);
                     // ClearAlarms();
                 };
        }

        private void ClearAlarms() //for testing purposes only. this method nukes all alarms registred.
        {

            if (Helper.tn == null)
                Helper.tn = ToastNotificationManager.CreateToastNotifier();
            var ScheduledToasts = Helper.tn.GetScheduledToastNotifications();
            foreach (ScheduledToastNotification notif in ScheduledToasts)
            {
                Helper.tn.RemoveFromSchedule(notif);
            }
        }

        private void LV_LayoutUpdated(object sender, object e)
        {
            if (Novenas == null || LV.Items == null || LV.Items.Count < Novenas.Count)
                Initializing = true;
            else
                Initializing = false;

        }



        private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {

        }



        private void ResetListView()
        {
            ReadNovenas();
            if (LV.ItemsSource != null)
                LV.ItemsSource = null;
            LV.ItemsSource = Novenas;
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


        }
        private void ReadNovenas()
        {
            Novenas = Storage.GetCollection();
        }



        private void AddNovenaButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToDetails();
        }

        private void MultipleSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMultipleSelectionMode();

        }

        private void SetMultipleSelectionMode(bool state)
        {
            switch (state)
            {
                case false:
                    LV.IsMultiSelectCheckBoxEnabled = false;
                    LV.SelectionMode = ListViewSelectionMode.None;
                    abDeleteSelection.Visibility = Visibility.Collapsed;

                    break;
                case true:
                    LV.IsMultiSelectCheckBoxEnabled = true;
                    LV.SelectionMode = ListViewSelectionMode.Multiple;
                    abDeleteSelection.Visibility = Visibility.Visible;
                    break;
            }

            MultiSelectMode = state;
        }
        private bool ToggleMultipleSelectionMode()
        {
            bool currentStatus = LV.IsMultiSelectCheckBoxEnabled;
            SetMultipleSelectionMode(!currentStatus);
            return !currentStatus;

        }

        private async void RemoveSelectedNovenaButton_Click(object sender, RoutedEventArgs e)
        {
            if (LV.SelectedItems == null || LV.SelectedItems.Count == 0) return;
            bool DialogResult = await Helper.ShowNovenaMassDeleteDialog(LV.SelectedItems.Count);
            if (DialogResult)
            {
                foreach (Novena nov in LV.SelectedItems)
                {
                    Helper.DeleteNovena(nov);
                }
                ResetListView();
            }
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {

            if (MultiSelectMode == false)
            {
                Novena nov = e.ClickedItem as Novena;
                NavigateToDetails(nov);
            }
        }
        private void Item_Holding(object sender, HoldingRoutedEventArgs e)
        {
            ShowItemContextMenu(sender);
        }

        private void ShowItemContextMenu(object sender)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void NavigateToDetails(Novena nov = null)
        {
            Frame.Navigate(typeof(NovenaDetails), nov, new DrillInNavigationTransitionInfo());

        }

        private async void DeleteSingleNovena_click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuFlyoutItem;

            Novena nov = menu.DataContext as Novena;
            bool DialogResult = await Helper.ShowNovenaDeleteDialog(nov);
            if (DialogResult)
            {
                Helper.DeleteNovena(nov);
                ResetListView();
            }
        }

        private void Item_RightClick(object sender, RightTappedRoutedEventArgs e)
        {
            ShowItemContextMenu(sender);
        }

        private void TgEnabledToggle_Loaded(object sender, RoutedEventArgs e)
        {
            var tg = sender as ToggleSwitch;
            tg.Toggled += TgEnabledToggle_Toggled;
        }


        private void TgEnabledToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var tg = sender as ToggleSwitch;
            var nov = tg.DataContext as Novena;
            if (nov == null) { return; }//have to investigate why is null.
            //check if the togglechange has been manually fired
            if (nov.IsActive == tg.IsOn)
            {
                //nothing to see here, move along;
                return;
            }

            if (tg.IsOn)
            {
                try
                {
                    nov.Activate();
                    Storage.SaveNovena(nov);
                    ResetListView();

                }
                catch (Exception ex)
                {
                    Helper.ShowDialog(_t("e0019"), _t(ex.Message));//"Novena nu poate fi activata"
                    tg.IsOn = false;
                }
            }
            else
            {
                nov.Deactivate();
                Storage.SaveNovena(nov);
                ResetListView();
            }
        }

        //remap for fast access:
        private string _t(string stringName)
        {
            return Helper._t(stringName);
        }
    }
}
