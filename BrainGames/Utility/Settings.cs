using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
//https://jamesmontemagno.github.io/SettingsPlugin/

namespace BrainGames.Utility
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string UserIdKey = "userid_key";
        private static readonly string UserIdDefault = "123";

        private const string ScreennameKey = "screenname_key";
        private static readonly string ScreennameDefault = "";

        private const string IT_AvgCorDurKey = "it_avgcordur_key";
        private static readonly double IT_AvgCorDurDefault = 0;

        private const string IT_EstITKey = "it_estit_key";
        private static readonly double IT_EstITDefault = 0;

        private const string IT_LastStimDurKey = "it_laststimdur_key";
        private static readonly double IT_LastStimDurDefault = 0;

        private const string IT_CorTrialsKey = "it_cortrials_key";
        private static readonly int IT_CorTrialsDefault = 0;

        private const string ActiveSubscriptionKey = "activesubscription_key";
        private static readonly bool ActiveSubscriptionDefault = false;
        private const string NeedToRestoreKey = "needtorestore_key";
        private static readonly bool NeedToRestoreDefault = false;
        private const string LastVerifiedDateKey = "lastverifieddate_key";
        private static readonly string LastVerifiedDateDefault = "";
        #endregion

        public static string UserId
        {
            get
            {
                return AppSettings.GetValueOrDefault(UserIdKey, UserIdDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(UserIdKey, value);
            }
        }

        public static string Screenname
        {
            get
            {
                return AppSettings.GetValueOrDefault(ScreennameKey, ScreennameDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ScreennameKey, value);
            }
        }

        public static double IT_AvgCorDur
        {
            get
            {
                return AppSettings.GetValueOrDefault(IT_AvgCorDurKey, IT_AvgCorDurDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IT_AvgCorDurKey, value);
            }
        }

        public static double IT_EstIT
        {
            get
            {
                return AppSettings.GetValueOrDefault(IT_EstITKey, IT_EstITDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IT_EstITKey, value);
            }
        }

        public static double IT_LastStimDur
        {
            get
            {
                return AppSettings.GetValueOrDefault(IT_LastStimDurKey, IT_LastStimDurDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IT_LastStimDurKey, value);
            }
        }
        public static int IT_CorTrials
        {
            get
            {
                return AppSettings.GetValueOrDefault(IT_CorTrialsKey, IT_CorTrialsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IT_CorTrialsKey, value);
            }
        }

        public static bool ActiveSubscription
        {
            get
            {
                return AppSettings.GetValueOrDefault(ActiveSubscriptionKey, ActiveSubscriptionDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ActiveSubscriptionKey, value);
            }
        }

        public static bool NeedToRestore
        {
            get
            {
                return AppSettings.GetValueOrDefault(NeedToRestoreKey, NeedToRestoreDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(NeedToRestoreKey, value);
            }
        }

        public static string LastVerifiedDate
        {
            get
            {
                return AppSettings.GetValueOrDefault(LastVerifiedDateKey, LastVerifiedDateDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(LastVerifiedDateKey, value);
            }
        }

    }
}
