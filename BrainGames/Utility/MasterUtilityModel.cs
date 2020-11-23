using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;
using Xamarin.Essentials;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using BrainGames.Models;
using BrainGames.Services;
using MathNet.Numerics;

namespace BrainGames.Utility
{
    public static class Utilities
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = MasterUtilityModel.RandomNumber(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<Tuple<DateTime, double>> MovingAverage(List<Tuple<DateTime, double>> series, int period)
        {
            var result = new List<Tuple<DateTime, double>>();

            for (int i = 0; i < series.Count(); i++)
            {
                double total = 0;
                for (int x = 0; x <= i; x++)
                    total += series[x].Item2;
                double average = total / (i + 1);
                result.Add(Tuple.Create(series[i].Item1, average));
            }
            return result;
        }

    }

    public class SharingUserRecord
    {
        public string UserId { get; set; }
        public string Screenname { get; set; }
        public List<string> games { get; set; } = new List<string>();
        public List<bool> status { get; set; } = new List<bool>();
        public List<bool> theirstatus { get; set; } = new List<bool>();
    }

    public class GameShare
    {
        public string Screenname { get; set; }
        public string game { get; set; }
        public List<double> bestscores { get; set; }
        public List<double> avgscores { get; set; }
    }


    public class MasterUtilityModel
    {
        public static SQLiteAsyncConnection conn;
        public static SQLiteConnection conn_sync;
        private static BrainGamesAPIDataService bguserinfoService;
        public List<bool> it_corarr = null;
        public List<double> it_empstimtimearr = null;
        public double it_laststimtime = 0;
        public int it_trialctr = 0;
        public int it_reversalctr = 0;
        public int it_errtrialstreak = 0;
        public int it_cortrialstreak = 0;
        public int rt_trialctr = 0;
        public double rt_ss1_cumrt = 0;
        public double rt_ss2_cumcorrt = 0;
        public double rt_ss4_cumcorrt = 0;
        public int rt_ss1_trialcnt = 0;
        public int rt_ss2_trialcnt = 0;
        public int rt_ss2_cortrialcnt = 0;
        public int rt_ss4_trialcnt = 0;
        public int rt_ss4_cortrialcnt = 0;

        public int st_trialctr = 0;
        public double st_cumcorrt = 0;
        public int st_cortrialcnt = 0;
        public int st_contrialcnt = 0;
        public double st_cumconcorrt = 0;
        public int st_corcontrialcnt = 0;
        public int st_incontrialcnt = 0;
        public double st_cuminconcorrt = 0;
        public int st_corincontrialcnt = 0;

        public int ds_trialctr = 0;
        public int ds_errtrialstreak = 0;
        public int ds_cortrialstreak = 0;
        public int ds_lastspan = 0;
        public List<Tuple<int, int>> ds_last_ontimes_by_spanlen = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> ds_last_offtimes_by_spanlen = new List<Tuple<int, int>>();
        public List<Tuple<int, bool>> ds_last_outcomes_by_spanlen = new List<Tuple<int, bool>>();
        public int ds_errtrialstreak_b = 0;
        public int ds_cortrialstreak_b = 0;
        public int ds_lastspan_b = 0;
        public List<Tuple<int, int>> ds_last_ontimes_by_spanlen_b = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> ds_last_offtimes_by_spanlen_b = new List<Tuple<int, int>>();
        public List<Tuple<int, bool>> ds_last_outcomes_by_spanlen_b = new List<Tuple<int, bool>>();
        public string ds_lastdir;

        public int ls_trialctr = 0;
        public int ls_errtrialstreak = 0;
        public int ls_cortrialstreak = 0;
        public int ls_lastspan = 0;
        public List<Tuple<int, int>> ls_last_ontimes_by_spanlen = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> ls_last_offtimes_by_spanlen = new List<Tuple<int, int>>();
        public List<Tuple<int, bool>> ls_last_outcomes_by_spanlen = new List<Tuple<int, bool>>();
        public int ls_errtrialstreak_b = 0;
        public int ls_cortrialstreak_b = 0;
        public int ls_lastspan_b = 0;
        public List<Tuple<int, int>> ls_last_ontimes_by_spanlen_b = new List<Tuple<int, int>>();
        public List<Tuple<int, int>> ls_last_offtimes_by_spanlen_b = new List<Tuple<int, int>>();
        public List<Tuple<int, bool>> ls_last_outcomes_by_spanlen_b = new List<Tuple<int, bool>>();
        public string ls_lastdir;
        public int ls_lastgridsize_f;
        public int ls_lastgridsize_b;

        public static List<string> UserStatsList = new List<string>();//which games you have stats for

        public bool has_notifications = false;//for notifications panel
        public List<GameShare> GameShares = new List<GameShare>();//for compare pages
        public List<SharingUserRecord> Invitations = new List<SharingUserRecord>();//for invitations page
        public List<SharingUserRecord> Shares = new List<SharingUserRecord>();//for manage sharing page

        private List<DataSchemas.SharingUsersSchema> BGSharingInvitations = new List<DataSchemas.SharingUsersSchema>();
        private List<DataSchemas.SharingUsersSchema> BGSharingUserRecords = new List<DataSchemas.SharingUsersSchema>();
        public static string DeviceId;

        object locker = new object();
        //Function to get random number
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
        public static double RandomNumberDouble()
        {
            lock (syncLock)
            { // synchronize
                return random.NextDouble();
            }
        }
        public static long RandomNumberLong()
        {
            lock (syncLock)
            { // synchronize
                return ((long)random.Next() << 32) | random.Next();
            }
        }
        public static int findgameshare(List<GameShare> gs, string name, string g)
        {
            int i = 0;
            for (; i < gs.Count(); i++)
            {
                if (gs[i].Screenname == name && gs[i].game == g) return i;
            }
            return -1;
        }
        public static int findshare(List<SharingUserRecord> s, string name, string g)
        {
            int i = 0;
            for (; i < s.Count(); i++)
            {
                if (s[i].Screenname == name && s[i].games.Contains(g)) return i;
            }
            return -1;
        }


        //        public LogicGamesApiDataService lgscenarioService;

        static bool _isBusy;
        public static bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                //                OnPropertyChanged("IsBusy");
            }
        }
        static bool _isBusy_local;
        public static bool IsBusy_local
        {
            get { return _isBusy_local; }
            set
            {
                _isBusy_local = value;
                //                OnPropertyChanged("IsBusy");
            }
        }

        static bool _isBusyITGR;
        public static bool IsBusyITGR
        {
            get { return _isBusyITGR; }
            set
            {
                _isBusyITGR = value;
            }
        }

        static bool _isBusyRTGR;
        public static bool IsBusyRTGR
        {
            get { return _isBusyRTGR; }
            set
            {
                _isBusyRTGR = value;
            }
        }

        static bool _isBusyStroopGR;
        public static bool IsBusyStroopGR
        {
            get { return _isBusyStroopGR; }
            set
            {
                _isBusyStroopGR = value;
            }
        }

        static bool _isBusyDSGR;
        public static bool IsBusyDSGR
        {
            get { return _isBusyDSGR; }
            set
            {
                _isBusyDSGR = value;
            }
        }

        static bool _isBusyLSGR;
        public static bool IsBusyLSGR
        {
            get { return _isBusyLSGR; }
            set
            {
                _isBusyLSGR = value;
            }
        }

        static bool _isBusySession;
        public static bool IsBusySession
        {
            get { return _isBusySession; }
            set
            {
                _isBusySession = value;
            }
        }

        static bool _isBusyUser;
        public static bool IsBusyUser
        {
            get { return _isBusyUser; }
            set
            {
                _isBusyUser = value;
            }
        }

        static bool _isBusySharingUser;
        public static bool IsBusySharingUser
        {
            get { return _isBusySharingUser; }
            set
            {
                _isBusySharingUser = value;
            }
        }

        static bool _isBusyUserStats;
        public static bool IsBusyUserStats
        {
            get { return _isBusyUserStats; }
            set
            {
                _isBusyUserStats = value;
            }
        }

        public MasterUtilityModel()
        {
            bguserinfoService = new BrainGamesAPIDataService(new Uri("https://logicgames.azurewebsites.net"));

            conn_sync = DependencyService.Get<ISQLiteDb>().GetConnection();
            conn = DependencyService.Get<ISQLiteDb>().GetAsyncConnection();
            /*
                        var cmnd = new SQLiteCommand(conn_sync);
                        cmnd.CommandText = "ALTER TABLE UserSchema ADD COLUMN Screenname;";
                        cmnd.ExecuteNonQuery();
            */

/*            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Thread t = new Thread(LoadGameShare);//Accesses User, UserStats
                t.Start();
                Thread thrCS = new Thread(CheckSharing);//Accesses User, SharingUsers
                thrCS.Start();
            }*/

            #region itgr
            List<DataSchemas.ITGameRecordSchema> itgrs = new List<DataSchemas.ITGameRecordSchema>();
            bool exists;
            try
            {
                try
                {
                    var cmd = new SQLiteCommand(conn_sync);
                    cmd.CommandText = "select case when exists((select * from information_schema.tables where table_name = 'ITGameRecordSchema')) then 1 else 0 end";
                    exists = cmd.ExecuteScalar<int>() == 1;
                    //                    Console.WriteLine("exists1: {0}", exists);
                }
                catch (Exception ex)
                {
                    try
                    {
                        exists = true;
                        var cmdOthers = new SQLiteCommand(conn_sync);
                        cmdOthers.CommandText = "select 1 from ITGameRecordSchema where 1 = 0";
                        cmdOthers.ExecuteNonQuery();
                        //                        Console.WriteLine("exists2: {0}", exists);
                    }
                    catch (Exception ex1)
                    {
                        exists = false;
                        //                        Console.WriteLine("exists3: {0}", exists);
                    }
                }
                if (!exists) throw new System.ArgumentException("no table");
                //                Console.WriteLine("exists4: {0}", exists);
                itgrs = conn_sync.Query<DataSchemas.ITGameRecordSchema>("select * from ITGameRecordSchema");
            }
            catch (Exception ex)
            {
                //                Console.WriteLine("exists5");
                //                return;
            }

            if (itgrs.Count() > 0)
            {
                itgrs = itgrs.OrderBy(x => x.datetime).ToList();
                it_laststimtime = itgrs[itgrs.Count() - 1].stimtime;
                int i = itgrs.Count() - 1;
                while (i >= 0 && itgrs[i].cor == true && itgrs[i].stimtime == it_laststimtime)
                {
                    it_cortrialstreak++;
                    i--;
                }
                i = itgrs.Count() - 1;
                while (i >= 0 && itgrs[i].cor == false && itgrs[i].stimtime == it_laststimtime)
                {
                    it_errtrialstreak++;
                    i--;
                }
                it_corarr = new List<bool>();
                it_empstimtimearr = new List<double>();
                foreach (DataSchemas.ITGameRecordSchema grs in itgrs)
                {
                    it_corarr.Add(grs.cor);
                    it_empstimtimearr.Add(grs.empstimtime);
                }
                it_trialctr = itgrs.Max(x => x.trialnum);
                it_reversalctr = itgrs.Max(x => x.reversalctr);
                Settings.IT_CorTrials = itgrs.Where(x => x.cor == true).Count();
                Settings.IT_AvgCorDur = itgrs.Where(x => x.cor == true).Sum(x => x.empstimtime) / Settings.IT_CorTrials;
                Settings.IT_LastStimDur = it_laststimtime;
                Settings.IT_EstIT = itgrs[itgrs.Count() - 1].estit;
            }
            #endregion

            #region rtgr
            List<DataSchemas.RTGameRecordSchema> rtgrs = new List<DataSchemas.RTGameRecordSchema>();
            try {
                conn_sync.CreateTable<DataSchemas.RTGameRecordSchema>();
                rtgrs = conn_sync.Query<DataSchemas.RTGameRecordSchema>("select * from RTGameRecordSchema");
            }
            catch (Exception ex2)
            {
                ;
            }
            if (rtgrs.Count() > 0)
            {
                rtgrs = rtgrs.OrderBy(x => x.datetime).ToList();
                rt_trialctr = rtgrs.Max(x => x.trialnum);
                //                rt_avgrt = rtgrs[rtgrs.Count() - 1].avgrt;
                rt_ss1_cumrt = rtgrs.Where(x => x.boxes == 1).Sum(x => x.reactiontime);
                rt_ss1_trialcnt = rtgrs.Where(x => x.boxes == 1).Count();
                rt_ss2_trialcnt = rtgrs.Where(x => x.boxes == 2).Count();
                rt_ss2_cortrialcnt = rtgrs.Where(x => x.boxes == 2 && x.cor == true).Count();
                rt_ss2_cumcorrt = rtgrs.Where(x => x.boxes == 2 && x.cor == true).Sum(x => x.reactiontime);
                rt_ss4_trialcnt = rtgrs.Where(x => x.boxes == 4).Count();
                rt_ss4_cortrialcnt = rtgrs.Where(x => x.boxes == 4 && x.cor == true).Count();
                rt_ss4_cumcorrt = rtgrs.Where(x => x.boxes == 4 && x.cor == true).Sum(x => x.reactiontime);
            }
            #endregion

            #region stroopgr
            List<DataSchemas.StroopGameRecordSchema> stgrs = new List<DataSchemas.StroopGameRecordSchema>();
            try
            {
                conn_sync.CreateTable<DataSchemas.StroopGameRecordSchema>();
                stgrs = conn_sync.Query<DataSchemas.StroopGameRecordSchema>("select * from StroopGameRecordSchema");
            }
            catch (Exception ex2)
            {
                ;
            }
            if (stgrs.Count() > 0)
            {
                stgrs = stgrs.OrderBy(x => x.datetime).ToList();
                st_trialctr = stgrs.Max(x => x.trialnum);
                //                rt_avgrt = rtgrs[rtgrs.Count() - 1].avgrt;
                st_cortrialcnt = stgrs.Where(x => x.cor == true).Count();
                st_cumcorrt = stgrs.Where(x => x.cor == true).Sum(x => x.reactiontime);

                st_contrialcnt = stgrs.Where(x => x.congruent == true).Count();
                st_corcontrialcnt = stgrs.Where(x => x.congruent == true && x.cor == true).Count();
                st_incontrialcnt = stgrs.Where(x => x.congruent == false).Count();
                st_corincontrialcnt = stgrs.Where(x => x.congruent == false && x.cor == true).Count();
                st_cumconcorrt = stgrs.Where(x => x.congruent == true && x.cor == true).Sum(x => x.reactiontime);
                st_cuminconcorrt = stgrs.Where(x => x.congruent == false && x.cor == true).Sum(x => x.reactiontime);
            }
            #endregion

            #region dsgr
            List<DataSchemas.DSGameRecordSchema> dsgrs = new List<DataSchemas.DSGameRecordSchema>();
            try
            {
                conn_sync.CreateTable<DataSchemas.DSGameRecordSchema>();
                dsgrs = conn_sync.Query<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema where direction = 'f'");
            }
            catch (Exception ex2)
            {
                ;
            }
            if (dsgrs.Count() > 0)
            {
                dsgrs = dsgrs.OrderBy(x => x.datetime).ToList();
                ds_lastspan = dsgrs[dsgrs.Count() - 1].itemcnt;
                int i = dsgrs.Count() - 1;
                while (i >= 0 && dsgrs[i].cor == true && dsgrs[i].itemcnt == ds_lastspan)
                {
                    ds_cortrialstreak++;
                    i--;
                }
                i = dsgrs.Count() - 1;
                while (i >= 0 && dsgrs[i].cor == false && dsgrs[i].itemcnt == ds_lastspan)
                {
                    ds_errtrialstreak++;
                    i--;
                }
                ds_trialctr = dsgrs.Max(x => x.trialnum);
                ds_last_ontimes_by_spanlen = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                ds_last_offtimes_by_spanlen = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                ds_last_outcomes_by_spanlen = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
                ds_lastdir = "f";
            }
            try
            {
                conn_sync.CreateTable<DataSchemas.DSGameRecordSchema>();
                dsgrs = conn_sync.Query<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema where direction = 'b'");
            }
            catch (Exception ex2)
            {
                ;
            }
            if (dsgrs.Count() > 0)
            {
                dsgrs = dsgrs.OrderBy(x => x.datetime).ToList();
                ds_lastspan_b = dsgrs[dsgrs.Count() - 1].itemcnt;
                int i = dsgrs.Count() - 1;
                while (i >= 0 && dsgrs[i].cor == true && dsgrs[i].itemcnt == ds_lastspan_b)
                {
                    ds_cortrialstreak_b++;
                    i--;
                }
                i = dsgrs.Count() - 1;
                while (i >= 0 && dsgrs[i].cor == false && dsgrs[i].itemcnt == ds_lastspan_b)
                {
                    ds_errtrialstreak_b++;
                    i--;
                }
                if (dsgrs.Max(x => x.trialnum) > ds_trialctr) ds_lastdir = "b";
                ds_trialctr = Math.Max(ds_trialctr, dsgrs.Max(x => x.trialnum));
                ds_last_ontimes_by_spanlen_b = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                ds_last_offtimes_by_spanlen_b = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                ds_last_outcomes_by_spanlen_b = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
            }
            #endregion

            #region lsgr
            List<DataSchemas.LSGameRecordSchema> lsgrs = new List<DataSchemas.LSGameRecordSchema>();
            try
            {
                conn_sync.CreateTable<DataSchemas.LSGameRecordSchema>();
                lsgrs = conn_sync.Query<DataSchemas.LSGameRecordSchema>("select * from LSGameRecordSchema where direction = 'f'");
            }
            catch (Exception ex2)
            {
                ;
            }
            if (lsgrs.Count() > 0)
            {
                lsgrs = lsgrs.OrderBy(x => x.datetime).ToList();
                ls_lastspan = lsgrs[lsgrs.Count() - 1].itemcnt;
                ls_lastgridsize_f = lsgrs[lsgrs.Count() - 1].gridsize;
                int i = lsgrs.Count() - 1;
                while (i >= 0 && lsgrs[i].cor == true && lsgrs[i].itemcnt == ls_lastspan)
                {
                    ls_cortrialstreak++;
                    i--;
                }
                i = lsgrs.Count() - 1;
                while (i >= 0 && lsgrs[i].cor == false && lsgrs[i].itemcnt == ls_lastspan)
                {
                    ls_errtrialstreak++;
                    i--;
                }
                ls_trialctr = lsgrs.Max(x => x.trialnum);
                ls_last_ontimes_by_spanlen = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                ls_last_offtimes_by_spanlen = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                ls_last_outcomes_by_spanlen = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
                ls_lastdir = "f";
            }
            try
            {
                conn_sync.CreateTable<DataSchemas.LSGameRecordSchema>();
                lsgrs = conn_sync.Query<DataSchemas.LSGameRecordSchema>("select * from LSGameRecordSchema where direction = 'b'");
            }
            catch (Exception ex2)
            {
                ;
            }
            if (lsgrs.Count() > 0)
            {
                lsgrs = lsgrs.OrderBy(x => x.datetime).ToList();
                ls_lastspan_b = lsgrs[lsgrs.Count() - 1].itemcnt;
                ls_lastgridsize_b = lsgrs[lsgrs.Count() - 1].gridsize;
                int i = lsgrs.Count() - 1;
                while (i >= 0 && lsgrs[i].cor == true && lsgrs[i].itemcnt == ls_lastspan_b)
                {
                    ls_cortrialstreak_b++;
                    i--;
                }
                i = lsgrs.Count() - 1;
                while (i >= 0 && lsgrs[i].cor == false && lsgrs[i].itemcnt == ls_lastspan_b)
                {
                    ls_errtrialstreak_b++;
                    i--;
                }
                if (lsgrs.Max(x => x.trialnum) > ls_trialctr) ls_lastdir = "b";
                ls_trialctr = Math.Max(ls_trialctr, lsgrs.Max(x => x.trialnum));
                ls_last_ontimes_by_spanlen_b = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                ls_last_offtimes_by_spanlen_b = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                ls_last_outcomes_by_spanlen_b = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
            }
            #endregion

        }

        public async Task HandleSharingAndSync()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {/*
                var LoadGameShareTask = LoadGameShare();
                var CheckSharingTask = CheckSharing();
                await Task.WhenAll(LoadGameShareTask, CheckSharingTask);*/
//                Task<bool> LoadGameShareTask = LoadGameShare();
//                await LoadGameShareTask;

//                Task<bool> CheckSharingTask = CheckSharing();
//                await CheckSharingTask;

//                await LoadGameShare();
//                await CheckSharing();
                await LoadSharesInvitationsAndSharedGameStats();

                Thread t = new Thread(() => SyncLocalDBwithServer(conn));//Accesses XXGRs, Session, User
                t.Start();
            }
        }

        private async Task<bool> LoadSharesInvitationsAndSharedGameStats()
        {
            IsBusySharingUser = true;
            var sharingrecs = new List<DataSchemas.SharingUsersSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGSharingUsers");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        //                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId2%20eq%20'" + Settings.UserId + "'%20and%20Accepted%20eq%200%20and%20Declined%20eq%200&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId1%20eq%20'" + Settings.UserId + "'%20or%20UserId2%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            BGSharingUserRecords.Add(untypedItems[j].ToObject<DataSchemas.SharingUsersSchema>());

                            if (BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Accepted1 == true && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Accepted2 == true)
                            {
                                sharingrecs.Add(BGSharingUserRecords[BGSharingUserRecords.Count() - 1]);
                            }

                            if (BGSharingUserRecords[BGSharingUserRecords.Count() - 1].UserId2 == Settings.UserId && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Accepted2 == false && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Declined2 == false)
                            {
                                BGSharingInvitations.Add(untypedItems[j].ToObject<DataSchemas.SharingUsersSchema>());
                                BGSharingUserRecords.RemoveAt(BGSharingUserRecords.Count() - 1);//remove invitations you haven't acted upon
                                continue;
                            }

                            if (BGSharingUserRecords[BGSharingUserRecords.Count() - 1].UserId1 == Settings.UserId && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Accepted2 == false && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Declined2 == false)
                            {
                                BGSharingUserRecords.RemoveAt(BGSharingUserRecords.Count() - 1);//remove outstanding invitations you've sent
                            }
                        }
                    } while (untypedItems.Count() > 0);
                }
                catch (Exception ex)
                {
                    ;
                }
            }
            catch (Exception ex)
            {
                ;
            }

            IsBusySharingUser = false;

            if (BGSharingUserRecords.Count() > 0)
            {
                IsBusyUser = true;

                List<string> userids = BGSharingUserRecords.Where(x => x.Accepted1 == true && x.Accepted2 == true).Select(x => x.UserId1).Distinct().ToList();
                userids.AddRange(BGSharingUserRecords.Where(x => x.Accepted1 == true && x.Accepted2 == true).Select(x => x.UserId2).Distinct().ToList());
                userids = userids.Distinct().OrderBy(str => str).ToList();//users you're sharing at least one game with

                foreach (string userid in userids)//Fill Shares (for manage sharing) and GameShares (for comparing game stats)
                {
                    if (userid == Settings.UserId) continue;
                    SharingUserRecord share = new SharingUserRecord();
                    try
                    {
                        var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                        IMobileServiceTable bguserrecord = Client.GetTable("BGUser");
                        JToken untypedItems;
                        int pagesize = 50, ctr = 0;
                        IDictionary<string, string> _headers = new Dictionary<string, string>();
                        // TODO: Add header with auth-based token in chapter 7
                        _headers.Add("zumo-api-version", "2.0.0");
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + userid + "'", _headers);

                        share.UserId = userid;
                        share.Screenname = untypedItems[0].ToObject<DataSchemas.UserSchema>().Screenname;
                        List<Tuple<string, bool, bool>> gameshares = BGSharingUserRecords.Where(x => x.UserId1 == userid/* && (x.Accepted2 == true || x.Declined2 == true)*/).Select(y => Tuple.Create(y.game, y.Accepted2, y.Accepted1)).ToList();//they proposed, get your status
                        gameshares.AddRange(BGSharingUserRecords.Where(x => x.UserId2 == userid/* && (x.Accepted2 == true || x.Declined2 == true)*/).Select(y => Tuple.Create(y.game, y.Accepted1, y.Accepted2)).ToList());//you proposed, get your status
                        gameshares.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                        for (int i = 0; i < DataSchemas.GameTypes.Count(); i++)
                        {
                            if (gameshares.Any(x => x.Item1 == DataSchemas.GameTypes[i]))
                            {
                                share.games.Add(gameshares[i].Item1);
                                share.status.Add(gameshares[i].Item2);
                                share.theirstatus.Add(gameshares[i].Item3);
                            }
                            else//if you've neither accepted nor declined, assume false (maybe it's a new game)
                            {
                                share.games.Add(DataSchemas.GameTypes[i]);
                                share.status.Add(false);
                                share.theirstatus.Add(false);
                            }
                        }
                        Shares.Add(share);

                        List<string> games = sharingrecs.Where(x => x.UserId1 == userid || x.UserId2 == userid).Select(x => x.game).ToList();
                        GameShares.AddRange(await LoadGameShareStats(share.Screenname, games, userid));
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
            }

            if (BGSharingInvitations.Count() > 0)
            {
                has_notifications = true;
                List<string> userids = BGSharingInvitations.Select(x => x.UserId1).OrderBy(str => str).Distinct().ToList();
                foreach (string userid in userids)
                {
                    SharingUserRecord invite = new SharingUserRecord();
                    try
                    {
                        var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                        IMobileServiceTable bguserrecord = Client.GetTable("BGUser");
                        JToken untypedItems;
                        int pagesize = 50, ctr = 0;
                        IDictionary<string, string> _headers = new Dictionary<string, string>();
                        // TODO: Add header with auth-based token in chapter 7
                        _headers.Add("zumo-api-version", "2.0.0");
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + userid + "'", _headers);
                        invite.UserId = userid;
                        invite.Screenname = untypedItems[0].ToObject<DataSchemas.UserSchema>().Screenname;
                        invite.games = BGSharingInvitations.Where(x => x.UserId1 == userid).Select(x => x.game).OrderBy(str => str).ToList();
                        Invitations.Add(invite);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
            }

            IsBusyUser = false;
            return has_notifications;
        }

        private async Task<bool> LoadGameShare()
        {
            IsBusySharingUser = true;

            #region GameShare
            var sharingrecs = new List<DataSchemas.SharingUsersSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGSharingUsers");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        untypedItems = await bguserrecord.ReadAsync("$filter=(UserId1%20eq%20'" + Settings.UserId + "'%20or%20UserId2%20eq%20'" + Settings.UserId + "')%20and%20Accepted1%20eq%201%20and%20Accepted2%20eq%201&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        //                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId2%20eq%20'" + Settings.UserId + "'%20and%20Accepted%20eq%201&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        //untypedItems = await bguserrecord.ReadAsync("$select=UserId");
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            sharingrecs.Add(untypedItems[j].ToObject<DataSchemas.SharingUsersSchema>());
                        }
                    } while (untypedItems.Count() > 0);
                }
                catch (Exception ex)
                {
                    ;
                }
            }
            catch (Exception ex)
            {
                ;
            }

            IsBusySharingUser = false;

            if (sharingrecs.Count() > 0)
            {
                IsBusyUser = true;

                List<string> userids = sharingrecs.Select(x => x.UserId1).Distinct().ToList();
                userids.AddRange(sharingrecs.Select(x => x.UserId2).Distinct().ToList());
                foreach (string userid in userids)
                {
                    if (userid == Settings.UserId) continue;
                    try
                    {
                        var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                        IMobileServiceTable bguserrecord = Client.GetTable("BGUser");
                        JToken untypedItems;
                        int pagesize = 50, ctr = 0;
                        IDictionary<string, string> _headers = new Dictionary<string, string>();
                        // TODO: Add header with auth-based token in chapter 7
                        _headers.Add("zumo-api-version", "2.0.0");
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + userid + "'", _headers);
                        List<string> games = sharingrecs.Where(x => x.UserId1 == userid || x.UserId2 == userid).Select(x => x.game).ToList();
                        GameShares.AddRange(await LoadGameShareStats(untypedItems[0].ToObject<DataSchemas.UserSchema>().Screenname, games, userid));
                        /*                       foreach (string g in games)
                                               {
                                                   GameShare gs = await LoadGameShareStats(untypedItems[0].ToObject<DataSchemas.UserSchema>().Screenname, g, userid);
                                                   GameShares.Add(gs);
                                               }*/
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
                IsBusyUser = false;
            }
            #endregion
            return true;
        }

        private async Task<List<GameShare>> LoadGameShareStats(string screenname, List<string> games, string userid)
        {
            List<GameShare> gslist = new List<GameShare>();
            DataSchemas.UserStatsSchema BGOtherUserRecord = new DataSchemas.UserStatsSchema();
            var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
            IMobileServiceTable bguserrecord = Client.GetTable("BGUserStats");
            JToken untypedItems;
            int pagesize = 50, ctr = 0;
            IDictionary<string, string> _headers = new Dictionary<string, string>();
            _headers = new Dictionary<string, string>();
            // TODO: Add header with auth-based token in chapter 7
            _headers.Add("zumo-api-version", "2.0.0");

            while (IsBusyUserStats)
            {
                Thread.Sleep(1000);
            }
            IsBusyUserStats = true;

            if (UserStatsList.Count() == 0)//only do this (check your own stats) the first time through
            {
                untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'", _headers);
                for (int j = 0; j < untypedItems.Count(); j++)
                {
                    DataSchemas.UserStatsSchema BGUserStats = untypedItems[j].ToObject<DataSchemas.UserStatsSchema>();
                    UserStatsList.Add(BGUserStats.game);
                }
            }

            string gamestr = "";
            for(int i = 0; i < games.Count(); i++)
            {
                gamestr += "game%20eq%20'" + games[i] + "'" + (i < games.Count() - 1 ? "%20or%20" : "");
            }
            try
            {
                do
                {
                    untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + userid + "'%20and%20(" + gamestr + ")&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                    for (int j = 0; j < untypedItems.Count(); j++)
                    {
                        BGOtherUserRecord = untypedItems[j].ToObject<DataSchemas.UserStatsSchema>();
                        GameShare gs = new GameShare();
                        gs.Screenname = screenname;
                        gs.game = BGOtherUserRecord.game;
                        gs.avgscores = new List<double>();
                        gs.bestscores = new List<double>();
                        List<string> scores = new List<string>();
                        if (BGOtherUserRecord.avgs.Length > 0)
                        {
                            scores = BGOtherUserRecord.avgs.Split('~').ToList();
                            for (int i = 0; i < scores.Count(); i++)
                            {
                                gs.avgscores.Add(Convert.ToDouble(scores[i]));
                            }
                        }
                        if (BGOtherUserRecord.bests.Length > 0)
                        {
                            scores = BGOtherUserRecord.bests.Split('~').ToList();
                            for (int i = 0; i < scores.Count(); i++)
                            {
                                gs.bestscores.Add(Convert.ToDouble(scores[i]));
                            }
                        }
                        gslist.Add(gs);
                    }
                } while (untypedItems.Count() > 0);
            }
            catch (Exception ex)
            {
                ;
            }
            IsBusyUserStats = false;
            return gslist;
        }

        private async Task<bool> CheckSharing()//called (on first appearance) by HomePageViewModel. I can't remember why it's not done here.
        {
            while (IsBusySharingUser)
            {
                Thread.Sleep(1000);
            }

            IsBusySharingUser = true;

            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGSharingUsers");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        //                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId2%20eq%20'" + Settings.UserId + "'%20and%20Accepted%20eq%200%20and%20Declined%20eq%200&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId1%20eq%20'" + Settings.UserId + "'%20or%20UserId2%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            BGSharingUserRecords.Add(untypedItems[j].ToObject<DataSchemas.SharingUsersSchema>());
                            if (BGSharingUserRecords[BGSharingUserRecords.Count() - 1].UserId2 == Settings.UserId && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Accepted2 == false && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Declined2 == false)
                            {
                                BGSharingInvitations.Add(untypedItems[j].ToObject<DataSchemas.SharingUsersSchema>());
                                BGSharingUserRecords.RemoveAt(BGSharingUserRecords.Count() - 1);//remove invitations you haven't acted upon
                                continue;
                            }
                            if (BGSharingUserRecords[BGSharingUserRecords.Count() - 1].UserId1 == Settings.UserId && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Accepted2 == false && BGSharingUserRecords[BGSharingUserRecords.Count() - 1].Declined2 == false)
                            {
                                BGSharingUserRecords.RemoveAt(BGSharingUserRecords.Count() - 1);//remove outstanding invitations you've sent
                            }
                        }
                    } while (untypedItems.Count() > 0);
                }
                catch (Exception ex)
                {
                    ;
                }
            }
            catch (Exception ex)
            {
                ;
            }

            IsBusySharingUser = false;

            if (BGSharingUserRecords.Count() > 0)
            {
                while (IsBusyUser)
                {
                    Thread.Sleep(1000);
                }

                IsBusyUser = true;

                List<string> userids = BGSharingUserRecords.Where(x => x.Accepted1 == true && x.Accepted2 == true).Select(x => x.UserId1).Distinct().ToList();
                userids.AddRange(BGSharingUserRecords.Where(x => x.Accepted1 == true && x.Accepted2 == true).Select(x => x.UserId2).Distinct().ToList());
                userids = userids.Distinct().OrderBy(str => str).ToList();//users you're sharing at least one game with

                foreach (string userid in userids)
                {
                    if (userid == Settings.UserId) continue;
                    SharingUserRecord share = new SharingUserRecord();
                    try
                    {
                        var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                        IMobileServiceTable bguserrecord = Client.GetTable("BGUser");
                        JToken untypedItems;
                        int pagesize = 50, ctr = 0;
                        IDictionary<string, string> _headers = new Dictionary<string, string>();
                        // TODO: Add header with auth-based token in chapter 7
                        _headers.Add("zumo-api-version", "2.0.0");
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + userid + "'", _headers);
                        share.UserId = userid;
                        share.Screenname = untypedItems[0].ToObject<DataSchemas.UserSchema>().Screenname;
                        List<Tuple<string, bool, bool>> gameshares = BGSharingUserRecords.Where(x => x.UserId1 == userid/* && (x.Accepted2 == true || x.Declined2 == true)*/).Select(y => Tuple.Create(y.game, y.Accepted2, y.Accepted1)).ToList();//they proposed, get your status
                        gameshares.AddRange(BGSharingUserRecords.Where(x => x.UserId2 == userid/* && (x.Accepted2 == true || x.Declined2 == true)*/).Select(y => Tuple.Create(y.game, y.Accepted1, y.Accepted2)).ToList());//you proposed, get your status
                        gameshares.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                        for (int i = 0; i < DataSchemas.GameTypes.Count(); i++)
                        {
                            if (gameshares.Any(x => x.Item1 == DataSchemas.GameTypes[i]))
                            {
                                share.games.Add(gameshares[i].Item1);
                                share.status.Add(gameshares[i].Item2);
                                share.theirstatus.Add(gameshares[i].Item3);
                            }
                            else//if you've neither accepted nor declined, assume false (maybe it's a new game)
                            {
                                share.games.Add(DataSchemas.GameTypes[i]);
                                share.status.Add(false);
                                share.theirstatus.Add(false);
                            }
                        }
                        Shares.Add(share);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
            }

            if (BGSharingInvitations.Count() > 0)
            {
                has_notifications = true;
                List<string> userids = BGSharingInvitations.Select(x => x.UserId1).OrderBy(str => str).Distinct().ToList();
                foreach (string userid in userids)
                {
                    SharingUserRecord invite = new SharingUserRecord();
                    try
                    {
                        var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                        IMobileServiceTable bguserrecord = Client.GetTable("BGUser");
                        JToken untypedItems;
                        int pagesize = 50, ctr = 0;
                        IDictionary<string, string> _headers = new Dictionary<string, string>();
                        // TODO: Add header with auth-based token in chapter 7
                        _headers.Add("zumo-api-version", "2.0.0");
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + userid + "'", _headers);
                        invite.UserId = userid;
                        invite.Screenname = untypedItems[0].ToObject<DataSchemas.UserSchema>().Screenname;
                        invite.games = BGSharingInvitations.Where(x => x.UserId1 == userid).Select(x => x.game).OrderBy(str => str).ToList();
                        Invitations.Add(invite);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
            }
            IsBusyUser = false;
            return has_notifications;
        }

        private async Task<bool> SyncITGR(SQLiteAsyncConnection db)
        {
            bool dbexception = false;

            IsBusyITGR = true;

            #region ITGameRecordSchema;
            ///////  CheckServerForDBUpdates
            try { await db.CreateTableAsync<DataSchemas.ITGameRecordSchema>(); }
            catch (Exception ex2)
            {
                ;
            }
            Task<List<DataSchemas.ITGameRecordSchema>> t_q = null;
            List<DataSchemas.ITGameRecordSchema> q = new List<DataSchemas.ITGameRecordSchema>();
            lock (locker)
            {
                t_q = db.QueryAsync<DataSchemas.ITGameRecordSchema>("select * from ITGameRecordSchema where UserId = ?", Settings.UserId);//ultimately userid will be set at login and will be unique to a user across devices
            }
            try
            {
                q = t_q.Result;
            }
            catch (Exception ex)
            {
                dbexception = true;
            }

            List<DataSchemas.ITGameRecordSchema> BGUserRecords = new List<DataSchemas.ITGameRecordSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGITGameRecord");
                //                IMobileServiceTable bguserrecord = Client.GetTable("LGUser");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        //untypedItems = await bguserrecord.ReadAsync("$select=UserId");
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            BGUserRecords.Add(untypedItems[j].ToObject<DataSchemas.ITGameRecordSchema>());
                        }
                    } while (untypedItems.Count() > 0);

                    bool added = false;
                    for (int i = 0; i < BGUserRecords.Count; i++)
                    {
                        if (dbexception || (!q.Any(x => x.Id == BGUserRecords[i].Id)))//if the local db doesn't have this user record, add it
                        {
                            try
                            {
                                await db.InsertAsync(BGUserRecords[i]);
                                added = true;
                            }
                            catch (Exception exA)
                            {
                                added = false;
                            }

                        }
                    }
                    if (added)
                    {
                        List<DataSchemas.ITGameRecordSchema> itgrs = new List<DataSchemas.ITGameRecordSchema>();
                        itgrs = conn_sync.Query<DataSchemas.ITGameRecordSchema>("select * from ITGameRecordSchema");
                        itgrs = itgrs.OrderBy(x => x.datetime).ToList();
                        it_laststimtime = itgrs[itgrs.Count() - 1].stimtime;
                        int i = itgrs.Count() - 1;
                        while (i >= 0 && itgrs[i].cor == true && itgrs[i].stimtime == it_laststimtime)
                        {
                            it_cortrialstreak++;
                            i--;
                        }
                        i = itgrs.Count() - 1;
                        while (i >= 0 && itgrs[i].cor == false && itgrs[i].stimtime == it_laststimtime)
                        {
                            it_errtrialstreak++;
                            i--;
                        }
                        it_corarr = new List<bool>();
                        it_empstimtimearr = new List<double>();
                        foreach (DataSchemas.ITGameRecordSchema grs in itgrs)
                        {
                            it_corarr.Add(grs.cor);
                            it_empstimtimearr.Add(grs.empstimtime);
                        }
                        it_trialctr = itgrs.Max(x => x.trialnum);
                        it_reversalctr = itgrs.Max(x => x.reversalctr);
                        Settings.IT_CorTrials = itgrs.Where(x => x.cor == true).Count();
                        Settings.IT_AvgCorDur = itgrs.Where(x => x.cor == true).Sum(x => x.empstimtime) / Settings.IT_CorTrials;
                        Settings.IT_LastStimDur = it_laststimtime;
                        Settings.IT_EstIT = itgrs[itgrs.Count() - 1].estit;
                    }
                }
                catch (Exception exC)
                {
                    ;
                }
                if (!dbexception)
                {
                    for (int i = 0; i < q.Count; i++)
                    {
                        int idx = BGUserRecords.FindIndex(x => x.Id == q[i].Id);
                        if (idx == -1)//if the remote server doesn't have this user record, add it
                        {
                            try
                            {
                                await bguserinfoService.AddITGameRecordEntryAsync(q[i]);
                                //                                    JObject item = JObject.FromObject(q[i]);
                                //                                    await bguserrecord.InsertAsync(item);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                        else if (BGUserRecords[idx].stimtime == 0 && q[i].stimtime > 0)//the update didn't go through, resend
                        {
                            try
                            {
                                await bguserinfoService.UpdateITGameRecordEntryAsync(q[i]);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                    }
                }
            }
            catch (Exception exB)
            {
                ;
            }
            #endregion

            IsBusyITGR = false;
            return true;
        }

        private async Task<bool> SyncRTGR(SQLiteAsyncConnection db)
        {
            bool dbexception = false;

            IsBusyRTGR = true;

            #region RTGameRecordSchema;
            ///////  CheckServerForDBUpdates
            try { await db.CreateTableAsync<DataSchemas.RTGameRecordSchema>(); }
            catch (Exception ex2)
            {
                ;
            }
            Task<List<DataSchemas.RTGameRecordSchema>> t_q_rt = null;
            List<DataSchemas.RTGameRecordSchema> q_rt = new List<DataSchemas.RTGameRecordSchema>();
            lock (locker)
            {
                t_q_rt = db.QueryAsync<DataSchemas.RTGameRecordSchema>("select * from RTGameRecordSchema where UserId = ?", Settings.UserId);//ultimately userid will be set at login and will be unique to a user across devices
            }
            try
            {
                q_rt = t_q_rt.Result;
            }
            catch (Exception ex)
            {
                dbexception = true;
            }

            List<DataSchemas.RTGameRecordSchema> BGRTUserRecords = new List<DataSchemas.RTGameRecordSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGRTGameRecord");
                //                IMobileServiceTable bguserrecord = Client.GetTable("LGUser");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        //untypedItems = await bguserrecord.ReadAsync("$select=UserId");
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            BGRTUserRecords.Add(untypedItems[j].ToObject<DataSchemas.RTGameRecordSchema>());
                        }
                    } while (untypedItems.Count() > 0);

                    bool added = false;
                    for (int i = 0; i < BGRTUserRecords.Count; i++)
                    {
                        if (dbexception || (!q_rt.Any(x => x.Id == BGRTUserRecords[i].Id)))//if the local db doesn't have this user record, add it
                        {
                            try
                            {
                                await db.InsertAsync(BGRTUserRecords[i]);
                                added = true;
                            }
                            catch (Exception exA)
                            {
                                added = false;
                            }

                        }
                    }
                    if (added)
                    {
                        List<DataSchemas.RTGameRecordSchema> rtgrs = new List<DataSchemas.RTGameRecordSchema>();
                        rtgrs = conn_sync.Query<DataSchemas.RTGameRecordSchema>("select * from RTGameRecordSchema");
                        rtgrs = rtgrs.OrderBy(x => x.datetime).ToList();
                        rt_trialctr = rtgrs.Max(x => x.trialnum);
                        //rt_avgrt = rtgrs[rtgrs.Count() - 1].avgrt;
                        rt_ss1_cumrt = rtgrs.Where(x => x.boxes == 1).Sum(x => x.reactiontime);
                        rt_ss1_trialcnt = rtgrs.Where(x => x.boxes == 1).Count();
                        rt_ss2_trialcnt = rtgrs.Where(x => x.boxes == 2).Count();
                        rt_ss2_cortrialcnt = rtgrs.Where(x => x.boxes == 2 && x.cor == true).Count();
                        rt_ss2_cumcorrt = rtgrs.Where(x => x.boxes == 2 && x.cor == true).Sum(x => x.reactiontime);
                        rt_ss4_trialcnt = rtgrs.Where(x => x.boxes == 4).Count();
                        rt_ss4_cortrialcnt = rtgrs.Where(x => x.boxes == 4 && x.cor == true).Count();
                        rt_ss4_cumcorrt = rtgrs.Where(x => x.boxes == 4 && x.cor == true).Sum(x => x.reactiontime);

                    }
                }
                catch (Exception exC)
                {
                    ;
                }
                if (!dbexception)
                {
                    for (int i = 0; i < q_rt.Count; i++)
                    {
                        int idx = BGRTUserRecords.FindIndex(x => x.Id == q_rt[i].Id);
                        if (idx == -1)//if the remote server doesn't have this user record, add it
                        {
                            try
                            {
                                await bguserinfoService.AddRTGameRecordEntryAsync(q_rt[i]);
                                //                                    JObject item = JObject.FromObject(q[i]);
                                //                                    await bguserrecord.InsertAsync(item);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                        else if (BGRTUserRecords[idx].reactiontime == 0 && q_rt[i].reactiontime > 0)//the update didn't go through, resend
                        {
                            try
                            {
                                await bguserinfoService.UpdateRTGameRecordEntryAsync(q_rt[i]);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                    }
                }
            }
            catch (Exception exB)
            {
                ;
            }
            #endregion

            IsBusyRTGR = false;
            return true;
        }

        private async Task<bool> SyncStroopGR(SQLiteAsyncConnection db)
        {
            bool dbexception = false;

            IsBusyStroopGR = true;

            #region StroopGameRecordSchema;
            ///////  CheckServerForDBUpdates
            try { await db.CreateTableAsync<DataSchemas.StroopGameRecordSchema>(); }
            catch (Exception ex2)
            {
                ;
            }
            Task<List<DataSchemas.StroopGameRecordSchema>> t_q_st = null;
            List<DataSchemas.StroopGameRecordSchema> q_st = new List<DataSchemas.StroopGameRecordSchema>();
            lock (locker)
            {
                t_q_st = db.QueryAsync<DataSchemas.StroopGameRecordSchema>("select * from StroopGameRecordSchema where UserId = ?", Settings.UserId);//ultimately userid will be set at login and will be unique to a user across devices
            }
            try
            {
                q_st = t_q_st.Result;
            }
            catch (Exception ex)
            {
                dbexception = true;
            }

            List<DataSchemas.StroopGameRecordSchema> BGStroopUserRecords = new List<DataSchemas.StroopGameRecordSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGStroopGameRecord");
                //                IMobileServiceTable bguserrecord = Client.GetTable("LGUser");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        //untypedItems = await bguserrecord.ReadAsync("$select=UserId");
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            BGStroopUserRecords.Add(untypedItems[j].ToObject<DataSchemas.StroopGameRecordSchema>());
                        }
                    } while (untypedItems.Count() > 0);

                    bool added = false;
                    for (int i = 0; i < BGStroopUserRecords.Count; i++)
                    {
                        if (dbexception || (!q_st.Any(x => x.Id == BGStroopUserRecords[i].Id)))//if the local db doesn't have this user record, add it
                        {
                            try
                            {
                                await db.InsertAsync(BGStroopUserRecords[i]);
                                added = true;
                            }
                            catch (Exception exA)
                            {
                                added = false;
                            }

                        }
                    }
                    if (added)
                    {
                        List<DataSchemas.StroopGameRecordSchema> stgrs = new List<DataSchemas.StroopGameRecordSchema>();
                        stgrs = conn_sync.Query<DataSchemas.StroopGameRecordSchema>("select * from StroopGameRecordSchema");
                        stgrs = stgrs.OrderBy(x => x.datetime).ToList();
                        st_trialctr = stgrs.Max(x => x.trialnum);
                        st_cortrialcnt = stgrs.Where(x => x.cor == true).Count();
                        st_cumcorrt = stgrs.Where(x => x.cor == true).Sum(x => x.reactiontime);
                        st_contrialcnt = stgrs.Where(x => x.congruent == true).Count();
                        st_corcontrialcnt = stgrs.Where(x => x.congruent == true && x.cor == true).Count();
                        st_incontrialcnt = stgrs.Where(x => x.congruent == false).Count();
                        st_corincontrialcnt = stgrs.Where(x => x.congruent == false && x.cor == true).Count();
                        st_cumconcorrt = stgrs.Where(x => x.congruent == true && x.cor == true).Sum(x => x.reactiontime);
                        st_cuminconcorrt = stgrs.Where(x => x.congruent == false && x.cor == true).Sum(x => x.reactiontime);
                    }
                }
                catch (Exception exC)
                {
                    ;
                }
                if (!dbexception)
                {
                    for (int i = 0; i < q_st.Count; i++)
                    {
                        int idx = BGStroopUserRecords.FindIndex(x => x.Id == q_st[i].Id);
                        if (idx == -1)//if the remote server doesn't have this user record, add it
                        {
                            try
                            {
                                await bguserinfoService.AddStroopGameRecordEntryAsync(q_st[i]);
                                //                                    JObject item = JObject.FromObject(q[i]);
                                //                                    await bguserrecord.InsertAsync(item);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                        else if (BGStroopUserRecords[idx].reactiontime == 0 && q_st[i].reactiontime > 0)//the update didn't go through, resend
                        {
                            try
                            {
                                await bguserinfoService.UpdateStroopGameRecordEntryAsync(q_st[i]);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                    }
                }
            }
            catch (Exception exB)
            {
                ;
            }
            #endregion

            IsBusyStroopGR = false;
            return true;
        }

        private async Task<bool> SyncDSGR(SQLiteAsyncConnection db)
        {
            bool dbexception = false;

            IsBusyDSGR = true;

            #region DSGameRecordSchema;
            ///////  CheckServerForDBUpdates
            try { await db.CreateTableAsync<DataSchemas.DSGameRecordSchema>(); }
            catch (Exception ex2)
            {
                ;
            }
            Task<List<DataSchemas.DSGameRecordSchema>> t_q_ds = null;
            List<DataSchemas.DSGameRecordSchema> q_ds = new List<DataSchemas.DSGameRecordSchema>();
            lock (locker)
            {
                t_q_ds = db.QueryAsync<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema where UserId = ?", Settings.UserId);//ultimately userid will be set at login and will be unique to a user across devices
            }
            try
            {
                q_ds = t_q_ds.Result;
            }
            catch (Exception ex)
            {
                dbexception = true;
            }

            List<DataSchemas.DSGameRecordSchema> BGDSUserRecords = new List<DataSchemas.DSGameRecordSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGDSGameRecord");
                //                IMobileServiceTable bguserrecord = Client.GetTable("LGUser");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        //untypedItems = await bguserrecord.ReadAsync("$select=UserId");
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            BGDSUserRecords.Add(untypedItems[j].ToObject<DataSchemas.DSGameRecordSchema>());
                        }
                    } while (untypedItems.Count() > 0);

                    bool added = false;
                    for (int i = 0; i < BGDSUserRecords.Count; i++)
                    {
                        if (dbexception || (!q_ds.Any(x => x.Id == BGDSUserRecords[i].Id)))//if the local db doesn't have this user record, add it
                        {
                            try
                            {
                                await db.InsertAsync(BGDSUserRecords[i]);
                                added = true;
                            }
                            catch (Exception exA)
                            {
                                added = false;
                            }

                        }
                    }
                    if (added)
                    {
                        List<DataSchemas.DSGameRecordSchema> dsgrs = new List<DataSchemas.DSGameRecordSchema>();
                        dsgrs = conn_sync.Query<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema where direction = 'f'");
                        dsgrs = dsgrs.OrderBy(x => x.datetime).ToList();
                        ds_lastspan = dsgrs[dsgrs.Count() - 1].itemcnt;
                        int i = dsgrs.Count() - 1;
                        while (i >= 0 && dsgrs[i].cor == true && dsgrs[i].itemcnt == ds_lastspan)
                        {
                            ds_cortrialstreak++;
                            i--;
                        }
                        i = dsgrs.Count() - 1;
                        while (i >= 0 && dsgrs[i].cor == false && dsgrs[i].itemcnt == ds_lastspan)
                        {
                            ds_errtrialstreak++;
                            i--;
                        }
                        ds_trialctr = dsgrs.Max(x => x.trialnum);
                        ds_last_ontimes_by_spanlen = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                        ds_last_offtimes_by_spanlen = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                        ds_last_outcomes_by_spanlen = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
                        ds_lastdir = "f";

                        dsgrs = conn_sync.Query<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema where direction = 'b'");
                        dsgrs = dsgrs.OrderBy(x => x.datetime).ToList();
                        ds_lastspan_b = dsgrs[dsgrs.Count() - 1].itemcnt;
                        i = dsgrs.Count() - 1;
                        while (i >= 0 && dsgrs[i].cor == true && dsgrs[i].itemcnt == ds_lastspan_b)
                        {
                            ds_cortrialstreak_b++;
                            i--;
                        }
                        i = dsgrs.Count() - 1;
                        while (i >= 0 && dsgrs[i].cor == false && dsgrs[i].itemcnt == ds_lastspan_b)
                        {
                            ds_errtrialstreak_b++;
                            i--;
                        }
                        if (dsgrs.Max(x => x.trialnum) > ds_trialctr) ds_lastdir = "b";
                        ds_trialctr = Math.Max(ds_trialctr, dsgrs.Max(x => x.trialnum));
                        ds_last_ontimes_by_spanlen_b = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                        ds_last_offtimes_by_spanlen_b = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                        ds_last_outcomes_by_spanlen_b = dsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
                    }
                }
                catch (Exception exC)
                {
                    ;
                }
                if (!dbexception)
                {
                    for (int i = 0; i < q_ds.Count; i++)
                    {
                        int idx = BGDSUserRecords.FindIndex(x => x.Id == q_ds[i].Id);
                        if (idx == -1)//if the remote server doesn't have this user record, add it
                        {
                            try
                            {
                                await bguserinfoService.AddDSGameRecordEntryAsync(q_ds[i]);
                                //                                    JObject item = JObject.FromObject(q[i]);
                                //                                    await bguserrecord.InsertAsync(item);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                        else if (BGDSUserRecords[idx].ontimems == 0 && q_ds[i].ontimems > 0)//the update didn't go through, resend
                        {
                            try
                            {
                                await bguserinfoService.UpdateDSGameRecordEntryAsync(q_ds[i]);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                    }
                }
            }
            catch (Exception exB)
            {
                ;
            }
            #endregion

            IsBusyDSGR = false;
            return true;
        }

        private async Task<bool> SyncLSGR(SQLiteAsyncConnection db)
        {
            bool dbexception = false;

            IsBusyLSGR = true;

            #region LSGameRecordSchema;
            ///////  CheckServerForDBUpdates
            try { await db.CreateTableAsync<DataSchemas.LSGameRecordSchema>(); }
            catch (Exception ex2)
            {
                ;
            }
            Task<List<DataSchemas.LSGameRecordSchema>> t_q_ls = null;
            List<DataSchemas.LSGameRecordSchema> q_ls = new List<DataSchemas.LSGameRecordSchema>();
            lock (locker)
            {
                t_q_ls = db.QueryAsync<DataSchemas.LSGameRecordSchema>("select * from LSGameRecordSchema where UserId = ?", Settings.UserId);//ultimately userid will be set at login and will be unique to a user across devices
            }
            try
            {
                q_ls = t_q_ls.Result;
            }
            catch (Exception ex)
            {
                dbexception = true;
            }

            List<DataSchemas.LSGameRecordSchema> BGLSUserRecords = new List<DataSchemas.LSGameRecordSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bguserrecord = Client.GetTable("BGLSGameRecord");
                //                IMobileServiceTable bguserrecord = Client.GetTable("LGUser");
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                try
                {
                    do
                    {
                        untypedItems = await bguserrecord.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                        //untypedItems = await bguserrecord.ReadAsync("$select=UserId");
                        for (int j = 0; j < untypedItems.Count(); j++)
                        {
                            BGLSUserRecords.Add(untypedItems[j].ToObject<DataSchemas.LSGameRecordSchema>());
                        }
                    } while (untypedItems.Count() > 0);

                    bool added = false;
                    for (int i = 0; i < BGLSUserRecords.Count; i++)
                    {
                        if (dbexception || (!q_ls.Any(x => x.Id == BGLSUserRecords[i].Id)))//if the local db doesn't have this user record, add it
                        {
                            try
                            {
                                await db.InsertAsync(BGLSUserRecords[i]);
                                added = true;
                            }
                            catch (Exception exA)
                            {
                                added = false;
                            }

                        }
                    }
                    if (added)
                    {
                        List<DataSchemas.LSGameRecordSchema> lsgrs = new List<DataSchemas.LSGameRecordSchema>();
                        lsgrs = conn_sync.Query<DataSchemas.LSGameRecordSchema>("select * from LSGameRecordSchema where direction = 'f'");
                        lsgrs = lsgrs.OrderBy(x => x.datetime).ToList();
                        ls_lastspan = lsgrs[lsgrs.Count() - 1].itemcnt;
                        ls_lastgridsize_f = lsgrs[lsgrs.Count() - 1].gridsize;
                        int i = lsgrs.Count() - 1;
                        while (i >= 0 && lsgrs[i].cor == true && lsgrs[i].itemcnt == ls_lastspan)
                        {
                            ls_cortrialstreak++;
                            i--;
                        }
                        i = lsgrs.Count() - 1;
                        while (i >= 0 && lsgrs[i].cor == false && lsgrs[i].itemcnt == ls_lastspan)
                        {
                            ls_errtrialstreak++;
                            i--;
                        }
                        ls_trialctr = lsgrs.Max(x => x.trialnum);
                        ls_last_ontimes_by_spanlen = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                        ls_last_offtimes_by_spanlen = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                        ls_last_outcomes_by_spanlen = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
                        ls_lastdir = "f";

                        lsgrs = conn_sync.Query<DataSchemas.LSGameRecordSchema>("select * from LSGameRecordSchema where direction = 'b'");
                        lsgrs = lsgrs.OrderBy(x => x.datetime).ToList();
                        ls_lastspan_b = lsgrs[lsgrs.Count() - 1].itemcnt;
                        ls_lastgridsize_b = lsgrs[lsgrs.Count() - 1].gridsize;
                        i = lsgrs.Count() - 1;
                        while (i >= 0 && lsgrs[i].cor == true && lsgrs[i].itemcnt == ls_lastspan_b)
                        {
                            ls_cortrialstreak_b++;
                            i--;
                        }
                        i = lsgrs.Count() - 1;
                        while (i >= 0 && lsgrs[i].cor == false && lsgrs[i].itemcnt == ls_lastspan_b)
                        {
                            ls_errtrialstreak_b++;
                            i--;
                        }
                        if (lsgrs.Max(x => x.trialnum) > ls_trialctr) ls_lastdir = "b";
                        ls_trialctr = Math.Max(ls_trialctr, lsgrs.Max(x => x.trialnum));
                        ls_last_ontimes_by_spanlen_b = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().ontimems)).OrderBy(x => x.Item1).ToList();
                        ls_last_offtimes_by_spanlen_b = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().offtimems)).OrderBy(x => x.Item1).ToList();
                        ls_last_outcomes_by_spanlen_b = lsgrs.GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Last().cor)).OrderBy(x => x.Item1).ToList();
                    }
                }
                catch (Exception exC)
                {
                    ;
                }
                if (!dbexception)
                {
                    for (int i = 0; i < q_ls.Count; i++)
                    {
                        int idx = BGLSUserRecords.FindIndex(x => x.Id == q_ls[i].Id);
                        if (idx == -1)//if the remote server doesn't have this user record, add it
                        {
                            try
                            {
                                await bguserinfoService.AddLSGameRecordEntryAsync(q_ls[i]);
                                //                                    JObject item = JObject.FromObject(q[i]);
                                //                                    await bguserrecord.InsertAsync(item);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                        else if (BGLSUserRecords[idx].ontimems == 0 && q_ls[i].ontimems > 0)//the update didn't go through, resend
                        {
                            try
                            {
                                await bguserinfoService.UpdateLSGameRecordEntryAsync(q_ls[i]);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }
                        }
                    }
                }
            }
            catch (Exception exB)
            {
                ;
            }
            #endregion

            IsBusyLSGR = false;
            return true;
        }

        private async Task<bool> SyncSession(SQLiteAsyncConnection db)
        {
            bool dbexception = false;

            IsBusySession = true;

            #region BrainGameSessionSchema;
            ///////  CheckServerForDBUpdates
            try { await db.CreateTableAsync<DataSchemas.BrainGameSessionSchema>(); }
            catch (Exception ex3)
            {
                ;
            }
            Task<List<DataSchemas.BrainGameSessionSchema>> t_q3 = null;
            List<DataSchemas.BrainGameSessionSchema> q3 = new List<DataSchemas.BrainGameSessionSchema>();
            lock (locker)
            {
                t_q3 = db.QueryAsync<DataSchemas.BrainGameSessionSchema>("select * from BrainGameSessionSchema where UserId = ?", Settings.UserId);//ultimately userid will be set at login and will be unique to a user across devices
            }
            try
            {
                q3 = t_q3.Result;
            }
            catch (Exception ex)
            {
                dbexception = true;
            }

            List<DataSchemas.BrainGameSessionSchema> BGSessions = new List<DataSchemas.BrainGameSessionSchema>();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bgsessions = Client.GetTable("BGSession");
                //                IMobileServiceTable lguserfeedback_typed = Client.GetTable<LogicGame.LGUserFeedback>();
                JToken untypedItems;
                int pagesize = 50, ctr = 0;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                do
                {
                    untypedItems = await bgsessions.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'&$skip=" + pagesize * ctr++ + "&$take=" + pagesize, _headers);
                    for (int j = 0; j < untypedItems.Count(); j++)
                    {
                        BGSessions.Add(untypedItems[j].ToObject<DataSchemas.BrainGameSessionSchema>());
                    }
                } while (untypedItems.Count() > 0);

                for (int i = 0; i < BGSessions.Count; i++)
                {
                    if (dbexception || (!q3.Any(x => x.Id == BGSessions[i].Id)))//if the local db doesn't have this session, add it
                    {
                        try
                        {
                            await db.InsertAsync(BGSessions[i]);
                        }
                        catch (Exception exA)
                        {
                            ;
                        }

                    }
                }

                if (!dbexception)
                {
                    for (int i = 0; i < q3.Count; i++)
                    {
                        if (!BGSessions.Any(x => x.Id == q3[i].Id))//if the remote server doesn't have this session, add it
                        {
                            try
                            {
                                await bguserinfoService.AddBrainGameSessionEntryAsync(q3[i]);
                            }
                            catch (Exception exA)
                            {
                                ;
                            }

                        }
                    }
                }

            }
            catch (Exception exB)
            {
                ;
            }
            #endregion

            IsBusySession = false;
            return true;
        }

        private async Task<bool> SyncUser(SQLiteAsyncConnection db)
        {
            bool dbexception = false;
            while (IsBusyUser)
            {
                Thread.Sleep(1000);
            }

            IsBusyUser = true;

            #region UserSchema;
            ///////  CheckServerForDBUpdates
            try { await db.CreateTableAsync<DataSchemas.UserSchema>(); }
            catch (Exception ex3)
            {
                ;
            }
            bool onlocal = false;
            bool onremote = false;
            Task<List<DataSchemas.UserSchema>> t_q5 = null;
            List<DataSchemas.UserSchema> q5 = new List<DataSchemas.UserSchema>();
            lock (locker)
            {
                t_q5 = db.QueryAsync<DataSchemas.UserSchema>("select * from UserSchema where UserId = ?", Settings.UserId);
            }
            try
            {
                q5 = t_q5.Result;
                if (q5?.Count() > 0)
                    onlocal = true;
            }
            catch (Exception ex)
            {
                dbexception = true;
            }

            DataSchemas.UserSchema BGUser = new DataSchemas.UserSchema();
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bgsessions = Client.GetTable("BGUser");
                //                IMobileServiceTable lguserfeedback_typed = Client.GetTable<LogicGame.LGUserFeedback>();
                JToken untypedItems;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                untypedItems = await bgsessions.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId + "'", _headers);
                if (untypedItems.Count() > 0)
                {
                    BGUser = untypedItems[0].ToObject<DataSchemas.UserSchema>();
                    onremote = true;
                }

                if (onremote && (dbexception || (!q5.Any(x => x.Id == BGUser.Id))))//if the local db doesn't have this user, add it
                {
                    try
                    {
                        Settings.Screenname = BGUser.Screenname;
                        await db.InsertAsync(BGUser);
                        onlocal = true;
                    }
                    catch (Exception exA)
                    {
                        ;
                    }
                }
            }
            catch (Exception exB)
            {
                ;
            }
            finally
            {
                IsBusy = false;
            }
            if (!onlocal)
            {
                CreateUser(onremote);
            }
            #endregion

            IsBusyUser = false;
            return true;
        }

        private async void SyncLocalDBwithServer(SQLiteAsyncConnection db)
        {
            await SyncITGR(db);

            await SyncRTGR(db);

            await SyncStroopGR(db);

            await SyncLSGR(db);

            await SyncDSGR(db);

            await SyncSession(db);

            await SyncUser(db);
        }

        private static void CreateUser(bool onremote)
        {
            Settings.ActiveSubscription = true;

            if (Settings.LastVerifiedDate != "") //renewing lapsed subscription
            {
                //                await Task.Run(async () => { await GetScenariosFromServer(conn); UpdateSubscriptionOnServer(Settings.UserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), true); });
            }
            else
            {
                try { conn_sync.CreateTable<DataSchemas.UserSchema>(); }
                catch (Exception ex3)
                {
                    ;
                }
                var s = new DataSchemas.UserSchema();
                s.Id = Guid.NewGuid().ToString();
                s.SubscriptionId = new Guid().ToString();
                //////                if (Settings.UserId == -1) Settings.UserId = RandomNumberLong().ToString(); else
                s.UserId = Settings.UserId;
                s.Screenname = Settings.Screenname;
                s.LastSubscriptionVerificationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                s.ActiveSubscription = true;
                s.SignupDevice = DeviceInfo.DeviceType.ToString();
                s.SignupManufacturer = DeviceInfo.Manufacturer;
                s.SignupModel = DeviceInfo.Model;
                s.SignupOS = DeviceInfo.Platform.ToString();
                s.SignupVersion = DeviceInfo.VersionString;
                s.SignupScreenHeight = DeviceDisplay.MainDisplayInfo.Height;
                s.SignupScreenWidth = DeviceDisplay.MainDisplayInfo.Width;
                s.SignupScreenDensity = DeviceDisplay.MainDisplayInfo.Density;

                conn_sync.Insert(s);

                if (!onremote)
                {
                    Thread t = new Thread(() => SendSubscriptionToServer(s));
                    t.Start();
//                    await Task.Run(async () => { /*await GetScenariosFromServer(conn); */ SendSubscriptionToServer(s); });
                }
            }
            Settings.LastVerifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        /*
        public static void CreateOrUpdateUser(string _subscriptionId)
        {
            Settings.ActiveSubscription = true;

            if (Settings.LastVerifiedDate != "") //renewing lapsed subscription
            {
                //                await Task.Run(async () => { await GetScenariosFromServer(conn); UpdateSubscriptionOnServer(Settings.UserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), true); });
            }
            else
            {
                try { conn_sync.CreateTable<DataSchemas.UserSchema>(); }
                catch (Exception ex3)
                {
                    ;
                }
                var s = new DataSchemas.UserSchema();
                s.Id = Guid.NewGuid().ToString();
                s.SubscriptionId = _subscriptionId;
                s.UserId = Settings.UserId;
                s.Screenname = Settings.Screenname;
                s.LastSubscriptionVerificationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                s.ActiveSubscription = true;
                s.SignupDevice = DeviceInfo.DeviceType.ToString();
                s.SignupManufacturer = DeviceInfo.Manufacturer;
                s.SignupModel = DeviceInfo.Model;
                s.SignupOS = DeviceInfo.Platform.ToString();
                s.SignupVersion = DeviceInfo.VersionString;
                s.SignupScreenHeight = DeviceDisplay.MainDisplayInfo.Height;
                s.SignupScreenWidth = DeviceDisplay.MainDisplayInfo.Width;
                s.SignupScreenDensity = DeviceDisplay.MainDisplayInfo.Density;

                conn_sync.Insert(s);

                Thread t = new Thread(() => SendSubscriptionToServer(s));
                t.Start();
//                await Task.Run(async () => { SendSubscriptionToServer(s); });
            }
            Settings.LastVerifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        */
        public async static Task<string> CheckScreenname(string sname)//return userid to allow invitation to proceed if 1) userid exisits, 2) the users are not already connected, and 3) the other user hasn't blocked you
        {
            try
            {
                var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                IMobileServiceTable bgsessions = Client.GetTable("BGUser");
                //                IMobileServiceTable lguserfeedback_typed = Client.GetTable<LogicGame.LGUserFeedback>();
                JToken untypedItems;
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                untypedItems = await bgsessions.ReadAsync("$filter=Screenname%20eq%20'" + sname + "'", _headers);
                if (untypedItems.Count() == 0)
                {
                    return "";
                }
                else
                {
                    try
                    {
                        string uid = untypedItems[0].ToObject<DataSchemas.UserSchema>().UserId;
                        var client = new MobileServiceClient("https://logicgames.azurewebsites.net");
                        bgsessions = Client.GetTable("BGConnectedUsers");
                        untypedItems = await bgsessions.ReadAsync("$filter=UserId1%20eq%20'" + uid + "'%20or%20UserId2%20eq%20'" + uid + "'", _headers);
                        if (untypedItems.Count() == 0)
                        {
                            return uid;
                        }
                        else
                        {
                            var cus = untypedItems[0].ToObject<DataSchemas.ConnectedUsersSchema>();
                            if (cus.connected == false && ((uid == cus.UserId1 && cus.blocked1 == false) || (uid == cus.UserId2 && cus.blocked2 == false)))
                            {
                                return uid;
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        public async static void SetScreenname(string sname)
        {
            Task<List<DataSchemas.UserSchema>> t_q3 = null;
            List<DataSchemas.UserSchema> q3 = new List<DataSchemas.UserSchema>();
            t_q3 = conn.QueryAsync<DataSchemas.UserSchema>("select * from UserSchema where UserId = ?", Settings.UserId);
            q3 = t_q3.Result;

            var s = new DataSchemas.UserSchema();
            s = q3[0];
            s.Screenname = sname;
            await conn.UpdateAsync(s);
//            await Task.Run(async () => { UpdateUserOnServer(s); });
            Thread t = new Thread(() => UpdateUserOnServer(s));
            t.Start();

        }

        private async static void UpdateUserOnServer(DataSchemas.UserSchema user)
        {
            while (IsBusyUser)
            {
                Thread.Sleep(1000);
            }

            IsBusyUser = true;

            try
            {
                await bguserinfoService.UpdateUserEntryAsync(user);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyUser = false;
            }
        }

        public async static void SendSubscriptionToServer(DataSchemas.UserSchema user)
        {
            while (IsBusyUser)
            {
                Thread.Sleep(1000);
            }

            IsBusyUser = true;

            try
            {
                await bguserinfoService.AddUserEntryAsync(user);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyUser = false;
            }
        }

        public async void SetShare(string user, string games)//send invite
        {
            var Client = new MobileServiceClient("https://logicgames.azurewebsites.net");
            IMobileServiceTable bgsessions = Client.GetTable("BGSharingUsers");
            //                IMobileServiceTable lguserfeedback_typed = Client.GetTable<LogicGame.LGUserFeedback>();
            JToken untypedItems;
            IDictionary<string, string> _headers = new Dictionary<string, string>();
            // TODO: Add header with auth-based token in chapter 7
            _headers.Add("zumo-api-version", "2.0.0");

            string[] garr = games.Split(',');
            foreach (string g in garr)
            {
                var s = new DataSchemas.SharingUsersSchema();
                s.Id = Guid.NewGuid().ToString();
                s.Accepted1 = true;
                s.Declined1 = false;
                s.Accepted2 = false;
                s.Declined2 = false;
                s.UserId1 = Settings.UserId;
                s.UserId2 = user;
                s.game = g;
                
                while (IsBusySharingUser)
                {
                    Thread.Sleep(1000);
                }

                IsBusySharingUser = true;

                try//no double invites
                {
                    untypedItems = await bgsessions.ReadAsync("$filter=(UserId1%20eq%20'" + Settings.UserId + "'%20or%20UserId2%20eq%20'" + Settings.UserId + "')%20and%20(UserId1%20eq%20'" + s.UserId2 + "'%20or%20UserId2%20eq%20'" + s.UserId2 + "')%20and%20game%20eq%20'" + g + "'", _headers);
                    if (untypedItems.Count() == 0)
                    {
                        await bguserinfoService.AddSharingEntryAsync(s);
                    }
                }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusySharingUser = false;
                }
            }
        }

        public async void RespondShare(string screenname, string games)//respond to invite
        {
            List<DataSchemas.SharingUsersSchema> requests = new List<DataSchemas.SharingUsersSchema>();
            requests = BGSharingInvitations.Where(x => x.UserId1 == Invitations.Where(y => y.Screenname == screenname).ToList()[0].UserId).ToList();

            if (games != "")//you've accepted at least one game; you are now connected
            {
                var cu = new DataSchemas.ConnectedUsersSchema();
                cu.Id = Guid.NewGuid().ToString();
                cu.UserId1 = requests[0].UserId1;
                cu.UserId2 = Settings.UserId;
                cu.connected = true;
                cu.blocked1 = false;
                cu.blocked2 = false;

                while (IsBusySharingUser)
                {
                    Thread.Sleep(1000);
                }

                IsBusySharingUser = true;

                try
                {
                    await bguserinfoService.AddConnectedEntryAsync(cu);
                }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusySharingUser = false;
                }
            }

            SharingUserRecord share = new SharingUserRecord();
            List<string> gs = games.Split(',').ToList();
            foreach (DataSchemas.SharingUsersSchema r in requests)
            {
                if (gs.Contains(r.game))
                {
                    GameShares.AddRange(await LoadGameShareStats(screenname, new List<string>(){ r.game }, r.UserId1));
                    r.Accepted2 = true;
                    r.Declined2 = false;
                    int idx = findshare(Shares, screenname, r.game);
                    if (idx == -1)
                    {
                        share.games.Add(r.game);
                        share.status.Add(true);
                        share.theirstatus.Add(true);
                    }
                }
                else
                {
                    r.Accepted2 = false;
                    r.Declined2 = true;
                }


                while (IsBusy)
                {
                    ;
                }

                IsBusy = true;

                try
                {
                    await bguserinfoService.UpdateSharingEntryAsync(r);
                }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusy = false;
                    BGSharingInvitations.Remove(r);
                }
            }
            if (share != null)
            {
                Shares.Add(share);
            }
            if (BGSharingInvitations.Count() == 0) has_notifications = false;
        }

        public async void UpdateShare(string screenname, string games)//manage sharing
        {
            List<string> gs = games.Split(',').ToList();
            string uid = Shares.Where(y => y.Screenname == screenname).ToList()[0].UserId;

            List<DataSchemas.SharingUsersSchema> shares = new List<DataSchemas.SharingUsersSchema>();
            shares = BGSharingUserRecords.Where(x => x.UserId1 == uid || x.UserId2 == uid).ToList();

            foreach (DataSchemas.SharingUsersSchema r in shares)
            {
                if (gs.Contains(r.game))
                {
                    if (findgameshare(GameShares, screenname, r.game) == -1)
                    {
                        GameShares.AddRange(await LoadGameShareStats(screenname, new List<string>() { r.game }, uid));
                    }
                    int idx = findshare(Shares, screenname, r.game);
                    int idx2 = Shares[idx].games.IndexOf(r.game);
                    Shares[idx].status[idx2] = true;
                    if (Settings.UserId == r.UserId1)
                    {
                        r.Accepted1 = true;
                        r.Declined1 = false;
                    }
                    else
                    {
                        r.Accepted2 = true;
                        r.Declined2 = false;
                    }
                }
                else
                {
                    int idx = findgameshare(GameShares, screenname, r.game);
                    if (idx > -1)
                    {
                        GameShares.RemoveAt(idx);
                    }
                    idx = findshare(Shares, screenname, r.game);
                    int idx2 = Shares[idx].games.IndexOf(r.game);
                    Shares[idx].status[idx2] = false;
                    if (Settings.UserId == r.UserId1)
                    {
                        r.Accepted1 = false;
                        r.Declined1 = true;
                    }
                    else
                    {
                        r.Accepted2 = false;
                        r.Declined2 = true;
                    }
                }

                while (IsBusySharingUser)
                {
                    Thread.Sleep(1000);
                }

                IsBusySharingUser = true;

                try
                {
                    await bguserinfoService.UpdateSharingEntryAsync(r);
                }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusySharingUser = false;
                }
            }
        }

        public async void UpdateUserStats(string game, List<double> avgs, List<double> bests)
        {
            var s = new DataSchemas.UserStatsSchema();
            s.Id = Guid.NewGuid().ToString();
            s.UserId = Settings.UserId;
            s.game = game;
            string avgstr = "", beststr = "";
            for (int i = 0; i < avgs.Count(); i++)
            {
                avgstr += Convert.ToString(avgs[i]);
                if (i < avgs.Count - 1) avgstr += "~";
            }
            for (int i = 0; i < bests.Count(); i++)
            {
                beststr += Convert.ToString(bests[i]);
                if (i < bests.Count - 1) beststr += "~";
            }
            s.avgs = avgstr;
            s.bests = beststr;

            IsBusyUserStats = true;

            try
            {
                if (UserStatsList.Contains(game))
                {
                    await bguserinfoService.UpdateUserStatsEntryAsync(s);
                }
                else
                {
                    await bguserinfoService.AddUserStatsEntryAsync(s);
                    UserStatsList.Add(game);
                }
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyUserStats = false;
            }

        }

        public async static void WriteITGR(Guid sessionid, int trialctr, int reversalctr, double curstimdur, double empstimdur, double avgcorit, double estit, int cor_ans, bool cor)
        {
            var s = new DataSchemas.ITGameRecordSchema();
            s.Id = Guid.NewGuid().ToString();
            s.SessionId = sessionid.ToString();
            s.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            s.UserId = Settings.UserId;
            s.stimtime = curstimdur;
            s.stimtype = cor_ans;
            s.trialnum = trialctr;
            s.reversalctr = reversalctr;
            s.empstimtime = empstimdur;
            s.avgcorit = avgcorit;
            s.estit = estit;
            s.cor = cor;
            if (!IsBusy_local)
            {
                IsBusy_local = true;
                try { await conn.InsertAsync(s); }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusy_local = false;
                }
            }

            //                await azureService.AddUserRecord(ScenarioId, type, recordid);
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Thread t = new Thread(() => SendITGRToServer(s));
                t.Start();
            }
        }

        private async static void SendITGRToServer(DataSchemas.ITGameRecordSchema itgr)
        {
            while (IsBusyITGR)
            {
                Thread.Sleep(1000);
            }

            IsBusyITGR = true;

            try
            {
                await bguserinfoService.AddITGameRecordEntryAsync(itgr);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyITGR = false;
            }
        }

        public async static void WriteRTGR(Guid sessionid, int trialctr, double reactiontime, double avgrt, int boxes, int corbox, bool cor)
        {
            var s = new DataSchemas.RTGameRecordSchema();
            s.Id = Guid.NewGuid().ToString();
            s.SessionId = sessionid.ToString();
            s.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            s.UserId = Settings.UserId;
            s.reactiontime = reactiontime;
            s.boxes = boxes;
            s.corbox = corbox;
            s.cor = cor;
            s.trialnum = trialctr;
            s.avgrt = avgrt;
            if (!IsBusy_local)
            {
                IsBusy_local = true;
                try { await conn.InsertAsync(s); }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusy_local = false;
                }
            }

            //                await azureService.AddUserRecord(ScenarioId, type, recordid);
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Thread t = new Thread(() => SendRTGRToServer(s));
                t.Start();
            }
        }

        private async static void SendRTGRToServer(DataSchemas.RTGameRecordSchema rtgr)
        {
            while (IsBusyRTGR)
            {
                Thread.Sleep(1000);
            }

            IsBusyRTGR = true;

            try
            {
                await bguserinfoService.AddRTGameRecordEntryAsync(rtgr);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyRTGR = false;
            }
        }

        public async static void WriteStroopGR(Guid sessionid, int trialctr, double reactiontime, double avgrt, double difrt, string word, string textcolor, bool congruent, bool cor)
        {
            var s = new DataSchemas.StroopGameRecordSchema();
            s.Id = Guid.NewGuid().ToString();
            s.SessionId = sessionid.ToString();
            s.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            s.UserId = Settings.UserId;
            s.reactiontime = reactiontime;
            s.word = word;
            s.textcolor = textcolor;
            s.congruent = congruent;
            s.cor = cor;
            s.trialnum = trialctr;
            s.avgrt = avgrt;
            s.difrt = difrt;
            if (!IsBusy_local)
            {
                IsBusy_local = true;
                try { await conn.InsertAsync(s); }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusy_local = false;
                }
            }

            //                await azureService.AddUserRecord(ScenarioId, type, recordid);
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Thread t = new Thread(() => SendStroopGRToServer(s));
                t.Start();
            }
        }

        private async static void SendStroopGRToServer(DataSchemas.StroopGameRecordSchema stgr)
        {
            while (IsBusyStroopGR)
            {
                Thread.Sleep(1000);
            }

            IsBusyStroopGR = true;

            try
            {
                await bguserinfoService.AddStroopGameRecordEntryAsync(stgr);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyStroopGR = false;
            }
        }

        public async static void WriteDSGR(Guid sessionid, int trialctr, int itemcnt, int ontimems, int offtimems, int resptimems, string direction, string items, bool repeats, bool repeats_cons, bool auto, bool cor)
        {
            List<DataSchemas.DSGameRecordSchema> ur = new List<DataSchemas.DSGameRecordSchema>();
            double estSpan_f = 0, estStimTime_f = 0, estSpan_b = 0, estStimTime_b = 0;
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                List<bool> corarr = ur.Where(x => x.direction == "f").Select(x => x.cor).ToList();
                List<int> spanlenarr = ur.Where(x => x.direction == "f").Select(x => x.itemcnt).ToList();
                if(direction == "f")
                {
                    corarr.Add(cor);
                    spanlenarr.Add(itemcnt);
                }
                //                var llsi = new LinearLeastSquaresInterpolation(spanlenarr.Select(Convert.ToDouble), corarr.Select(Convert.ToDouble));
                //                estSpan_f = llsi.Slope == 0 ? llsi.AverageX : (0.9 - llsi.Intercept) / llsi.Slope;
                Tuple<double, double> p;
                List<Tuple<int, double>> AvgCorStatsBySpan;

                try { p = Fit.Line(spanlenarr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0,0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "f").GroupBy(x => x.itemcnt).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estSpan_f = 0.0;
                    }
                    else
                    {
                        estSpan_f = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estSpan_f = (p.Item2 == 0 || Double.IsNaN(p.Item2)) ? ((corarr.Count() > 0 && corarr[0] == true) ? spanlenarr.Max() : 0) : (0.9 - p.Item1) / p.Item2;
                }

                corarr = ur.Where(x => x.itemcnt <= estSpan_f && x.direction == "f").Select(x => x.cor).ToList();
                List<int> stimtimearr = ur.Where(x => x.itemcnt <= estSpan_f && x.direction == "f").Select(x => x.ontimems + x.offtimems).ToList();
                if (direction == "f" && itemcnt <= estSpan_f)
                {
                    corarr.Add(cor);
                    stimtimearr.Add(ontimems + offtimems);
                }
                try { p = Fit.Line(stimtimearr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "f").GroupBy(x => x.ontimems + x.offtimems).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9 || corarr.Count() == 0)
                    {
                        estStimTime_f = 0.0;
                    }
                    else
                    {
                        estStimTime_f = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estStimTime_f = (p.Item2 == 0 || Double.IsNaN(p.Item2)) ? ((corarr.Count() > 0 && corarr[0] == true) ? stimtimearr.Min() : 0) : (0.9 - p.Item1) / p.Item2;
                }
                corarr = ur.Where(x => x.direction == "b").Select(x => x.cor).ToList();
                spanlenarr = ur.Where(x => x.direction == "b").Select(x => x.itemcnt).ToList();
                if (direction == "b")
                {
                    corarr.Add(cor);
                    spanlenarr.Add(itemcnt);
                }
//                var llsi = new LinearLeastSquaresInterpolation(spanlenarr.Select(Convert.ToDouble), corarr.Select(Convert.ToDouble));
                try { p = Fit.Line(spanlenarr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0,0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "b").GroupBy(x => x.itemcnt).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estSpan_b = 0.0;
                    }
                    else
                    {
                        estSpan_b = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estSpan_b = (p.Item2 == 0 || Double.IsNaN(p.Item2)) ? ((corarr.Count() > 0 && corarr[0] == true) ? spanlenarr.Max() : 0) : (0.9 - p.Item1) / p.Item2;
                }

                corarr = ur.Where(x => x.itemcnt <= estSpan_b && x.direction == "b").Select(x => x.cor).ToList();
                stimtimearr = ur.Where(x => x.itemcnt <= estSpan_b && x.direction == "b").Select(x => x.ontimems + x.offtimems).ToList();
                if (direction == "b" && itemcnt <= estSpan_b)
                {
                    corarr.Add(cor);
                    stimtimearr.Add(ontimems + offtimems);
                }
                try { p = Fit.Line(stimtimearr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "b").GroupBy(x => x.ontimems + x.offtimems).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9 || corarr.Count() == 0)
                    {
                        estStimTime_b = 0.0;
                    }
                    else
                    {
                        estStimTime_b = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estStimTime_b = (p.Item2 == 0 || Double.IsNaN(p.Item2)) ? ((corarr.Count() > 0 && corarr[0] == true) ? stimtimearr.Min() : 0) : (0.9 - p.Item1) / p.Item2;
                }
            }

            var s = new DataSchemas.DSGameRecordSchema();
            s.Id = Guid.NewGuid().ToString();
            s.SessionId = sessionid.ToString();
            s.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            s.UserId = Settings.UserId;
            s.itemcnt = itemcnt;
            s.ontimems = ontimems;
            s.offtimems = offtimems;
            s.repeats = repeats;
            s.repeats_cons = repeats_cons;
            s.cor = cor;
            s.trialnum = trialctr;
            s.direction = direction;
            s.items = items;
            s.autoinc = auto;
            s.estSpan_b = estSpan_b;
            s.estSpan_f = estSpan_f;
            s.estStimTime_b = estStimTime_b;
            s.estStimTime_f = estStimTime_f;
            s.resptimems = resptimems;
            if (!IsBusy_local)
            {
                IsBusy_local = true;
                try { await conn.InsertAsync(s); }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusy_local = false;
                }
            }

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Thread t = new Thread(() => SendDSGRToServer(s));
                t.Start();
            }
        }

        public async static void WriteLSGR(Guid sessionid, int trialctr, int itemcnt, int ontimems, int offtimems, int gridsize, int resptimems, string direction, string items, bool repeats, bool repeats_cons, bool auto, bool cor)
        {
            List<DataSchemas.LSGameRecordSchema> ur = new List<DataSchemas.LSGameRecordSchema>();
            double estSpan_f = 0, estStimTime_f = 0, estSpan_b = 0, estStimTime_b = 0;
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.LSGameRecordSchema>("select * from LSGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                List<bool> corarr = ur.Where(x => x.direction == "f").Select(x => x.cor).ToList();
                List<int> spanlenarr = ur.Where(x => x.direction == "f").Select(x => x.itemcnt).ToList();
                if (direction == "f")
                {
                    corarr.Add(cor);
                    spanlenarr.Add(itemcnt);
                }
                //                var llsi = new LinearLeastSquaresInterpolation(spanlenarr.Select(Convert.ToDouble), corarr.Select(Convert.ToDouble));
                //                estSpan_f = llsi.Slope == 0 ? llsi.AverageX : (0.9 - llsi.Intercept) / llsi.Slope;
                Tuple<double, double> p;
                List<Tuple<int, double>> AvgCorStatsBySpan;

                try { p = Fit.Line(spanlenarr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "f").GroupBy(x => x.itemcnt).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estSpan_f = 0.0;
                    }
                    else
                    {
                        estSpan_f = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estSpan_f = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? spanlenarr.Max() : 0) : (0.9 - p.Item1) / p.Item2;
                }

                corarr = ur.Where(x => x.itemcnt <= estSpan_f && x.direction == "f").Select(x => x.cor).ToList();
                List<int> stimtimearr = ur.Where(x => x.itemcnt <= estSpan_f && x.direction == "f").Select(x => x.ontimems + x.offtimems).ToList();
                if (direction == "f" && itemcnt <= estSpan_f)
                {
                    corarr.Add(cor);
                    stimtimearr.Add(ontimems + offtimems);
                }
                try { p = Fit.Line(stimtimearr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "f").GroupBy(x => x.ontimems + x.offtimems).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estStimTime_f = 0.0;
                    }
                    else
                    {
                        estStimTime_f = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estStimTime_f = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? stimtimearr.Min() : 0) : (0.9 - p.Item1) / p.Item2;
                }
                corarr = ur.Where(x => x.direction == "b").Select(x => x.cor).ToList();
                spanlenarr = ur.Where(x => x.direction == "b").Select(x => x.itemcnt).ToList();
                if (direction == "b")
                {
                    corarr.Add(cor);
                    spanlenarr.Add(itemcnt);
                }
                //                var llsi = new LinearLeastSquaresInterpolation(spanlenarr.Select(Convert.ToDouble), corarr.Select(Convert.ToDouble));
                try { p = Fit.Line(spanlenarr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "b").GroupBy(x => x.itemcnt).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estSpan_b = 0.0;
                    }
                    else
                    {
                        estSpan_b = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estSpan_b = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? spanlenarr.Max() : 0) : (0.9 - p.Item1) / p.Item2;
                }

                corarr = ur.Where(x => x.itemcnt <= estSpan_b && x.direction == "b").Select(x => x.cor).ToList();
                stimtimearr = ur.Where(x => x.itemcnt <= estSpan_b && x.direction == "b").Select(x => x.ontimems + x.offtimems).ToList();
                if (direction == "b" && itemcnt <= estSpan_b)
                {
                    corarr.Add(cor);
                    stimtimearr.Add(ontimems + offtimems);
                }
                try { p = Fit.Line(stimtimearr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "b").GroupBy(x => x.ontimems + x.offtimems).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estStimTime_b = 0.0;
                    }
                    else
                    {
                        estStimTime_b = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estStimTime_b = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? stimtimearr.Min() : 0) : (0.9 - p.Item1) / p.Item2;
                }
            }

            var s = new DataSchemas.LSGameRecordSchema();
            s.Id = Guid.NewGuid().ToString();
            s.SessionId = sessionid.ToString();
            s.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            s.UserId = Settings.UserId;
            s.itemcnt = itemcnt;
            s.ontimems = ontimems;
            s.offtimems = offtimems;
            s.repeats = repeats;
            s.repeats_cons = repeats_cons;
            s.cor = cor;
            s.trialnum = trialctr;
            s.direction = direction;
            s.items = items;
            s.autoinc = auto;
            s.estSpan_b = estSpan_b;
            s.estSpan_f = estSpan_f;
            s.estStimTime_b = estStimTime_b;
            s.estStimTime_f = estStimTime_f;
            s.resptimems = resptimems;
            s.gridsize = gridsize;
            if (!IsBusy_local)
            {
                IsBusy_local = true;
                try { await conn.InsertAsync(s); }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusy_local = false;
                }
            }

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Thread t = new Thread(() => SendLSGRToServer(s));
                t.Start();
            }
        }

        private async static void SendDSGRToServer(DataSchemas.DSGameRecordSchema gr)
        {
            while (IsBusyDSGR)
            {
                Thread.Sleep(1000);
            }

            IsBusyDSGR = true;

            try
            {
                await bguserinfoService.AddDSGameRecordEntryAsync(gr);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyDSGR = false;
            }
        }
        private async static void SendLSGRToServer(DataSchemas.LSGameRecordSchema gr)
        {
            while (IsBusyLSGR)
            {
                Thread.Sleep(1000);
            }

            IsBusyLSGR = true;

            try
            {
                await bguserinfoService.AddLSGameRecordEntryAsync(gr);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusyLSGR = false;
            }
        }

        public static Guid WriteGameSession(string gametype)
        {
            Guid g = Guid.NewGuid();
            try { conn_sync.CreateTable<DataSchemas.BrainGameSessionSchema>(); }
            catch (Exception ex3)
            {
                ;
            }
            var s = new DataSchemas.BrainGameSessionSchema();
            s.Id = Guid.NewGuid().ToString();
            s.SessionId = g.ToString();
            s.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            s.Device = DeviceInfo.DeviceType.ToString();
            s.GameType = gametype;
            s.Manufacturer = DeviceInfo.Manufacturer;
            s.DeviceVersion = DeviceInfo.VersionString;
            s.Model = DeviceInfo.Model;
            s.OS = DeviceInfo.Platform.ToString();
            s.ScreenDensity = DeviceDisplay.MainDisplayInfo.Density;
            s.ScreenWidth = DeviceDisplay.MainDisplayInfo.Width;
            s.ScreenHeight = DeviceDisplay.MainDisplayInfo.Height;
            s.UserId = Settings.UserId;
            conn_sync.Insert(s);
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Thread t = new Thread(() => SendGameSessionToServer(s));
                t.Start();
            }
            return g;
        }

        private async static void SendGameSessionToServer(DataSchemas.BrainGameSessionSchema sess)
        {
            while (IsBusySession)
            {
                Thread.Sleep(1000);
            }

            IsBusySession = true;

            try
            {
                await bguserinfoService.AddBrainGameSessionEntryAsync(sess);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusySession = false;
            }
        }

        public async static void WriteUF(string scenarioid, string rating, string context, string feedback)
        {
            var s = new DataSchemas.UserFeedbackSchema();
            s.Id = Guid.NewGuid().ToString();
            s.UserId = Settings.UserId;
            s.context = context;
            s.feedback = feedback;
            s.rating = rating;
            s.DeviceId = DeviceId;
            s.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (!IsBusy_local)
            {
                IsBusy_local = true;
                try { await conn.InsertAsync(s); }
                catch (Exception ex)
                {
                    ;
                }
                finally
                {
                    IsBusy_local = false;
                }
            }

            //                await azureService.AddUserFeedback(scenarioid, rating, context, feedback);
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                SendUFToServer(s);
        }

        private async static void SendUFToServer(DataSchemas.UserFeedbackSchema userFeedback)
        {
            try
            {
                await bguserinfoService.AddUserFeedbackEntryAsync(userFeedback);
            }
            catch (Exception ex)
            {
                ;
            }
        }

    }
}
