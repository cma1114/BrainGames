using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrainGames.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AboutPage : ContentPage
	{
		public AboutPage()
		{
			InitializeComponent();
			BindingContext = this;
		}


		public ICommand OpenUrlCommand => new Command<string>((url) =>
		{
			Device.OpenUri(new System.Uri(url));
		});
	}
}