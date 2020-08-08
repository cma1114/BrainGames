using System;
using System.Collections.Generic;
using System.Text;

namespace BrainGames.Models
{
    public enum MenuItemType
    {
        Play,
        Invite,
        Invitations,
        Profile,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
