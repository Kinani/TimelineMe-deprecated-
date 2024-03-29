﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimelineMe.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class MergeScenario : Page
    {
        private MainPage rootPage;
        private MediasViewModel medias;



        public MergeScenario()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.rootPage = MainPage.Current;
            medias = new MediasViewModel();
            MyListView.ItemsSource = medias.GetItems();
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

        private void OpenItemAppBarBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
