using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimelineMe.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TimelineMe.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StudioScenario : Page
    {

        private readonly SolidColorBrush lineBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        

        



        /// <summary>
        /// Reference back to the "root" page of the app.
        /// </summary>
        private MainPage rootPage;

        private MediasViewModel medias;


        public StudioScenario()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.rootPage = MainPage.Current;
            medias = new MediasViewModel();
            MyListView.ItemsSource = medias.GetItems();
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void MyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MergeItemAppBarBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteItemAppBarBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void OpenItemAppBarBtn_Click(object sender, RoutedEventArgs e)
        {
            MediaViewModel media = MyListView.SelectedItem as MediaViewModel;
            
            if (media.VidOrPic)
            {
                var options = new Windows.System.LauncherOptions();
                options.DisplayApplicationPicker = true;
                string temp = media.Name;
                var uriString = "ms-appdata:///local/" + temp + ".mp4";
                Uri muUri = new Uri(uriString);
                Launcher.LaunchUriAsync(new Uri(uriString, UriKind.RelativeOrAbsolute),options);
            }
            else
            {
                var options = new Windows.System.LauncherOptions();
                options.DisplayApplicationPicker = true;
                string temp = media.Name;
                var uriString = "ms-appdata:///local/" + temp + ".jpeg";
                
                Uri muUri = new Uri(uriString);
                Launcher.LaunchUriAsync(new Uri(uriString, UriKind.RelativeOrAbsolute),options);
            }   
        }
    }
    public class VidOrPicBindingConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string type = string.Empty;
            if ((bool)value)
                type = "Video";
            else
                type = "Photo";
            return type;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }

    }

    
}
