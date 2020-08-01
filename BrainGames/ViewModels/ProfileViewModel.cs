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

namespace BrainGames.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        public bool CheckName(string name)
        {
            if (MasterUtilityModel.CheckScreenname(name).Result)
            {
                return false;
            }
            return true;
        }

        public ProfileViewModel()
        {
        }
    }
}
