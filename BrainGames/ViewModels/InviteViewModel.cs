using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using BrainGames.Utility;
using BrainGames.Models;
using System.Runtime.CompilerServices;

namespace BrainGames.ViewModels
{
    public class InviteViewModel : ViewModelBase
    {
        private string user2 = ""; 
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
            if (s == "")
            {
                return false;
            }
            user2 = s;
            return true;
        }

        public void SendInvite(string games)
        {
//            MasterUtilityModel.SetShare(user2, games);
            Thread t = new Thread(()=>App.mum.SetShare(user2, games));
            t.Start();
        }

        public InviteViewModel()
        {
        }
    }
}
