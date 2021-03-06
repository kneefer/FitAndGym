﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FitAndGym.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace FitAndGym.View
{
    public partial class DeleteTrainingsByDatePage : PhoneApplicationPage
    {
        public DeleteTrainingsByDatePage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        #if DEBUG
        ~DeleteTrainingsByDatePage()
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                System.Windows.MessageBox.Show("DeleteTrainingsByDatePage Destructing");
                // Seeing this message box assures that this page is being cleaned up
            }));
        }
        #endif

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            while (NavigationService.BackStack.Any())
            {
                NavigationService.RemoveBackEntry();
            }
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var proceedDeleteButton = new ApplicationBarIconButton(new Uri("/Images/delete.png", UriKind.RelativeOrAbsolute));
            var discardChangesButton = new ApplicationBarIconButton(new Uri("/Images/cancel.png", UriKind.RelativeOrAbsolute));

            proceedDeleteButton.Click += proceedDelete_Click;
            proceedDeleteButton.Text = AppResources.ProceedDeleteByDateAppBar;

            discardChangesButton.Click += discardChangesButton_Click;
            discardChangesButton.Text = AppResources.DiscardChangesAppBar;

            ApplicationBar.Buttons.Add(proceedDeleteButton);
            ApplicationBar.Buttons.Add(discardChangesButton);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            discardChangesButton_Click(this, e);
        }

        void discardChangesButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml?PivotMain.SelectedIndex=2", UriKind.RelativeOrAbsolute));
        }

        void proceedDelete_Click(object sender, EventArgs e)
        {
            if (DateToWhichDelete != null && MessageBox.Show(AppResources.OlderThan + " " + DateToWhichDelete.Value.Value.Date.ToLongDateString(), AppResources.DatesOfTrainingsThatWillBeDeleted, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                App.FitAndGymViewModel.DeleteTrainingsByDate(DateToWhichDelete.Value.Value);

            NavigationService.Navigate(new Uri("/MainPage.xaml?viewBag=afterCloning&PivotMain.SelectedIndex=2", UriKind.RelativeOrAbsolute));
        }
    }
}