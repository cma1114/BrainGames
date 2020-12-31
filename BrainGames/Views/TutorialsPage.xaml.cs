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
	public partial class TutorialsPage : ContentPage
	{
		public TutorialsPage()
		{
			InitializeComponent();
		}

		async void TGR_IT(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(ITLabel, ScrollToPosition.Start, true);
		}

		async void TGR_RT(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(RTLabel, ScrollToPosition.Start, true);
		}

		async void TGR_ST(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(STLabel, ScrollToPosition.Start, true);
		}

		async void TGR_DS(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(DSLabel, ScrollToPosition.Start, true);
		}

		async void TGR_LS(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(LSLabel, ScrollToPosition.Start, true);
		}

		async void TGR_Stats(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(StatsLabel, ScrollToPosition.Start, true);
		}

		async void TGR_Friends(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(FriendsLabel, ScrollToPosition.Start, true);
		}

		async void TGR_TMD(System.Object sender, System.EventArgs e)
		{
			await scrollView.ScrollToAsync(TMDLabel, ScrollToPosition.Start, true);
		}
	}
}