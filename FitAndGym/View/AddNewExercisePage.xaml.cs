﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FitAndGym.Models;
using FitAndGym.Resources;
using FitAndGym.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Windows.Navigation;

namespace FitAndGym.View
{
    public partial class AddNewExercisePage : PhoneApplicationPage
    {
        private ExercisePageViewModel _viewModel;
        private bool actionOfUserToLeaveThePagePerformed = false;

        public AddNewExercisePage()
        {
            InitializeComponent();

            // filling ListPicker by enums
            NewExIntensityListPicker.ItemsSource = Enum.GetValues(typeof(Intensity));
        }

        #if DEBUG
        ~AddNewExercisePage()
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                System.Windows.MessageBox.Show("AddNewExercisePage Destructing");
                // Seeing this message box assures that this page is being cleaned up
            }));
        }
        #endif

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var saveChangesButton = new ApplicationBarIconButton(new Uri("/Images/save.png", UriKind.RelativeOrAbsolute));
            var discardChangesButton = new ApplicationBarIconButton(new Uri("/Images/cancel.png", UriKind.RelativeOrAbsolute));

            string action;

            if (NavigationContext.QueryString.TryGetValue("action", out action))
            {
                if (action == "edit")
                {
                    saveChangesButton.Click += updateChanges_Click;
                    saveChangesButton.Text = AppResources.UpdateChangesAppBar;
                    discardChangesButton.Click += discardChangesButton_Click;
                    discardChangesButton.Text = AppResources.DiscardChangesAppBar;
                }
                else if (action == "add")
                {
                    saveChangesButton.Click += saveChanges_Click;
                    saveChangesButton.Text = AppResources.SaveChangesAppBar;
                    discardChangesButton.Click += discardChangesButton_Click;
                    discardChangesButton.Text = AppResources.DiscardChangesAppBar;
                }
                else
                    throw new Exception(String.Format("Unknown action: '{0}' in ExercisePage", action));

                ApplicationBar.Buttons.Add(saveChangesButton);
                ApplicationBar.Buttons.Add(discardChangesButton);
                return;
            }
            throw new Exception("Lack of action in NavigationContext.QueryString in ExercisePage");
        }

        private void CheckIfEditOrAddActionRequiredAsync()
        {
            string action;

            if (NavigationContext.QueryString.TryGetValue("action", out action))
            {
                if (action == "edit")
                {
                    string exIdStr;
                    int exId;

                    if (NavigationContext.QueryString.TryGetValue("exId", out exIdStr) && Int32.TryParse(exIdStr, out exId))
                    {
                        Exercise exToEdit = App.FitAndGymViewModel.GetExerciseById(exId);
                        if (exToEdit != null)
                            _viewModel = new ExercisePageViewModel(exToEdit);
                        else
                            throw new Exception(String.Format("Not found Exercise with id = {0} in database invoked from ExercisePage!", exId));
                    }
                    else throw new Exception("Wrong NavigationContext.QueryString 'exId' in ExercisePage");
                }
                else if (action == "add")
                    _viewModel = new ExercisePageViewModel();
                else
                    throw new Exception(String.Format("Wrong NavigationContext.QueryString (action) in ExercisePage. Action = '{0}'", action));

                _viewModel.ValidationError += _viewModel_ValidationError;
                DataContext = _viewModel;
            }         
        }

        private void ClearThePage()
        {
            NewExIntensityListPicker.ClearValue(ListPicker.DataContextProperty);
            _viewModel.ValidationError -= _viewModel_ValidationError;
            _viewModel = null;
        }

        #region Events stuff

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if(actionOfUserToLeaveThePagePerformed)
                ClearThePage();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            while (NavigationService.BackStack.Any())
            {
                NavigationService.RemoveBackEntry();
            }

            // I have to commemorate guy who saved me - http://samondotnet.blogspot.com/2011/12/onnavigatedto-will-be-called-after.html
            // Line of rescue:
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back) return;

            BuildLocalizedApplicationBar();
            CheckIfEditOrAddActionRequiredAsync();
        }

        private void updateChanges_Click(object sender, EventArgs e)
        {
            var exerciseToUpdate = _viewModel.GenerateModel();
            if (exerciseToUpdate != null)
            {
                actionOfUserToLeaveThePagePerformed = true;
                App.FitAndGymViewModel.UpdateExercise(exerciseToUpdate);
                NavigationService.Navigate(new Uri("/MainPage.xaml?viewBag=updatedExercise&PivotMain.SelectedIndex=3", UriKind.RelativeOrAbsolute));
            }
        }

        private void discardChangesButton_Click(object sender, EventArgs e)
        {
            actionOfUserToLeaveThePagePerformed = true;
            NavigationService.Navigate(new Uri("/MainPage.xaml?PivotMain.SelectedIndex=3", UriKind.RelativeOrAbsolute));
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            discardChangesButton_Click(this, e);
        }

        private void saveChanges_Click(object sender, EventArgs e)
        {
            var newExercise = _viewModel.GenerateModel();
            if (newExercise != null)
            {
                actionOfUserToLeaveThePagePerformed = true;
                App.FitAndGymViewModel.AddNewExercise(newExercise);
                NavigationService.Navigate(new Uri("/MainPage.xaml?viewBag=addedExercise&PivotMain.SelectedIndex=3", UriKind.RelativeOrAbsolute));
            }
        }

        private void NewExName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtbox = sender as TextBox;
            BindingExpression bindingExpression = txtbox.GetBindingExpression(TextBox.TextProperty);
            bindingExpression.UpdateSource();
        }

        void _viewModel_ValidationError(object sender, ViewModels.ValidationErrorEventArgs e)
        {
            MessageBox.Show(e.ErrorMessage, AppResources.ValidationErrorTitle, MessageBoxButton.OK);
        }

        private void NewExNumOfSetsPlus_Click(object sender, RoutedEventArgs e)
        {
            ++(_viewModel.NumOfSets);
        }

        private void NewExNumOfSetsMinus_Click(object sender, RoutedEventArgs e)
        {
            --(_viewModel.NumOfSets);
        }

        private void NewExNumOfRepsPlus_Click(object sender, RoutedEventArgs e)
        {
            ++(_viewModel.NumOfReps);
        }

        private void NewExNumOfRepsMinus_Click(object sender, RoutedEventArgs e)
        {
            --(_viewModel.NumOfReps);
        }

        private void NewExOthersTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AdvControls.AdvTextBox txtbox = sender as AdvControls.AdvTextBox;
            BindingExpression bindingExpression = txtbox.GetBindingExpression(TextBox.TextProperty);
            bindingExpression.UpdateSource();
        }

        #endregion
    }
}