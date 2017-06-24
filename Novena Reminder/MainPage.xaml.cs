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
                    OnPropertyChanged("SelectedComboBoxOption");
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

       
        

        public bool initializing { get; private set; }

        public MainPage()
        {
            initializing = true;
            InitializeComponent();
         
            Loaded += (s, e) =>
                 {
                     ResetListView();
                     SetMultipleSelectionMode(false);
                  
                  
                 };
        }

        private void LV_LayoutUpdated(object sender, object e)
        {
            if (Novenas == null || LV.Items == null || LV.Items.Count< Novenas.Count)
                initializing = true;
            else
                initializing = false;
           // LV.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
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



        private  void AddNovenaButton_Click(object sender, RoutedEventArgs e)
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

        private void RemoveSelectedNovenaButton_Click(object sender, RoutedEventArgs e)
        {
            if (LV.SelectedItems == null || LV.SelectedItems.Count == 0) return;

            foreach (Novena nov in LV.SelectedItems)
            {
                Storage.DeleteNovena(nov.ID);
            }
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
          
            if  (MultiSelectMode == false)
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

        private void DeleteSingleNovena_click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuFlyoutItem;

            Novena nov = menu.DataContext as Novena;

            Storage.DeleteNovena(nov.ID);
            ResetListView();

        }

        private void Item_RightClick(object sender, RightTappedRoutedEventArgs e)
        {
            ShowItemContextMenu(sender);
        }

      

        private void tgEnabledToggle_Loaded(object sender, RoutedEventArgs e)
        {

            var tg = sender as ToggleSwitch;

            tg.DataContextChanged += Tg_DataContextChanged;
        }

        private void Tg_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var nov = args.NewValue as Novena;
            args.Handled = true;
            Storage.SaveNovena(nov);
        }
    }
}
