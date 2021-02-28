using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrainGames.Utility;

namespace BrainGames.ViewModels
{
    public class InvitationsViewModel : BaseViewModel
    {
        public void AcceptInvite(string screenname, string games)
        {
            Thread t = new Thread(() => App.mum.RespondShare(screenname, games));
            t.Start();
//            App.mum.RespondShare(screenname, games);
        }

        public void RejectInvite(string screenname)
        {
            Thread t = new Thread(() => App.mum.RespondShare(screenname, ""));
            t.Start();
//            App.mum.RespondShare(screenname, "");
        }

        public void UpdateInvite(string screenname, string games)
        {
            Thread t = new Thread(() => App.mum.UpdateShare(screenname, games));
            t.Start();
//            App.mum.UpdateShare(screenname, games);
        }
    }
}
