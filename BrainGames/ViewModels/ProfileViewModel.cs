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
        private bool _editable = false;
        public bool Editable
        {
            get => _editable;
            set { SetProperty(ref _editable, value); }
        }

        private bool _edit = false;
        public bool Edit
        {
            get => _edit;
            set { SetProperty(ref _edit, value); }
        }

        public bool CheckName(string name)
        {
/*            Task<bool> task = MasterUtilityModel.CheckScreenname(name);
            task.Wait();
            if (task.Result == true)*/
//            if(MasterUtilityModel.CheckScreenname(name))
            bool done = false;
            string s = "";
            Task.Run(async () => {
                try
                {
                    s = await MasterUtilityModel.CheckScreenname(name);
                    done = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex.Message);
                    done = true;
                }
            });
            while (!done)
            {
                ;
            }
            if (s != "")
            {
                return false;
            }
            Settings.Screenname = name;
            MasterUtilityModel.SetScreenname(name);
            return true;
        }

        public ProfileViewModel()
        {
            if (Settings.Screenname != "")
            {
                Editable = true;
            }
        }
    }
}
