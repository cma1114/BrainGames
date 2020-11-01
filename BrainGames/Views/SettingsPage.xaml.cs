using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrainGames.Utility;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.AutoIncrementSwitch.IsToggled = Settings.AutoIncrement;
            //            this.ShowFullScenarioSwitch.IsToggled = Settings.ShowFullScenario;
        }

        public void AutoIncrementToggled(object sender, EventArgs args)
        {
            bool b = Settings.AutoIncrement;
            Settings.AutoIncrement = ((Switch)sender).IsToggled;
            b = Settings.AutoIncrement;
        }
    }
}