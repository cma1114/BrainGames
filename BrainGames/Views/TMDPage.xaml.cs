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
            var games = Enumerable.Range(0, DataSchemas.GameTypes.Count() - 1).ToList();
            do
            {
                int i = MasterUtilityModel.RandomNumber(0, games.Count());
                orderedgames.Add(DataSchemas.GameTypes[games[i]]);
                games.RemoveAt(i);

            } while (games.Count() > 0);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RunIt();
        }

        private async void RunIt()
        {
            foreach (string g in orderedgames)
            {
                this.GameView.Content = null;
                switch (g)
                {
                    case "IT":
                        this.GameView.Content = new ITView(viewModel);
                        break;
                }
            }
            await Navigation.PopAsync();
        }
    }
}
