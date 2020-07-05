using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        public async void ITPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new ITPage());
        }

        public async void RTPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new RTPage());
        }

        public async void StroopPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new StroopPage());
        }

        public async void DSPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new DSPage());
        }
    }
}