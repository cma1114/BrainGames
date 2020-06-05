using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

//https://medium.com/pageup-tech/vanilla-mvvm-for-xamarin-forms-4f42af8ba34
//https://xamarin.azureedge.net/developer/xamarin-forms-book/XamarinFormsBook-Ch18-Apr2016.pdf

namespace BrainGames.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

/*
 *public INavigation Navigation { get; }
        
        protected ViewModelBase(INavigation navigation)
        {
            Navigation = navigation;
        }
        */
        public virtual void Init(object initData)
        {
        }
        
        public void WireEvents(Page page)
        {
            page.Appearing += ViewIsAppearing;
            page.Disappearing += ViewIsDisappearing;
        }
        protected virtual void ViewIsDisappearing(object sender, EventArgs e)
        {
        }
        
        protected virtual void ViewIsAppearing(object sender, EventArgs e)
        {
        }
/*        
         protected virtual async Task PushPage(ContentPage page)
        {
            await Navigation.PushAsync(page);
        }
        
        protected virtual async Task PopPage()
        {
            await Navigation.PopAsync();
        }
*/
        protected bool SetProperty<T>(ref T storage, T value,
        [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}