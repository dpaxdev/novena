using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Novena_Reminder
{

  
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

        private ICollection<Novena> Novenas ;

        public  MainPage()
        {


            Loaded += (s, e) =>
            {
                Novenas = Model.Storage.GetCollection();
                LV.ItemsSource = Novenas;
            };
            this.InitializeComponent();
          
        //   
        }
      

        protected override void OnNavigatedTo( NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
           
          
        }

       

        private async void AddNovenaButton_Click(object sender, RoutedEventArgs e)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>NavigateToDetails()
          );
            ;
        }

        private void MultipleSelectionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveNovenaButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            
            NavigateToDetails(e.ClickedItem as Novena);
        }

        private void NavigateToDetails(Novena nov = null)
        {
            Frame.Navigate(typeof(NovenaDetails), nov, new DrillInNavigationTransitionInfo());

        }
    }
}
