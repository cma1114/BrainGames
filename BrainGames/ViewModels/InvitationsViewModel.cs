using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BrainGames.Utility;

namespace BrainGames.ViewModels
{
    public class InvitationsViewModel : BaseViewModel
    {
        public void AcceptInvite(string screenname, string games)
        {
            App.mum.RespondShare(screenname, games);
        }

        public void RejectInvite(string screenname)
        {
            App.mum.RespondShare(screenname, "");
        }

        public void UpdateInvite(string screenname, string games)
        {
            App.mum.UpdateShare(screenname, games);
        }
    }
}
