using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

using BrainGames.Utility;
using BrainGames.Models;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class TMDPage : ContentPage
    {
        public TMDViewModel viewModel
        {
            get { return BindingContext as TMDViewModel; }
            set { BindingContext = value; }
        }

        List<string> orderedgames = new List<string>();
        public TMDPage()
        {
            viewModel = new TMDViewModel();
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //RunIt();
        }

 /*       private async void RunIt()
        {
            int i = 0;
            while (i < orderedgames.Count())
            {
                if (viewModel.InProgress)
                {
                    break;// continue;
                }
                viewModel.InProgress = true;
                this.GameView.Content = null;
                switch (orderedgames[i])
                {
                    case "IT":
                        this.GameView.Content = new ITView(viewModel);
                        break;
                    case "RT":
                        this.GameView.Content = new ITView(viewModel);
                        break;
                    case "Stroop":
                        this.GameView.Content = new ITView(viewModel);
                        break;
                    case "DS":
                        this.GameView.Content = new ITView(viewModel);
                        break;
                    case "LS":
                        this.GameView.Content = new ITView(viewModel);
                        break;
                }
                i++;
            }
            //await Navigation.PopAsync();
        }*/
    }
}
