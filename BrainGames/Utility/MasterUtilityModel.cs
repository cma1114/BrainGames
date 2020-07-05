using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;
using Xamarin.Essentials;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using BrainGames.Models;
using BrainGames.Services;

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
        public int rt_trialctr = 0 ;
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

        public MasterUtilityModel()
        {
            bguserinfoService = new BrainGamesAPIDataService(new Uri("https://logicgames.azurewebsites.net"));

            conn_sync = DependencyService.Get<ISQLiteDb>().GetConnection();
            conn = DependencyService.Get<ISQLiteDb>().GetAsyncConnection();

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                SyncLocalDBwithServer(conn);

            #region itgr
            List<DataSchemas.ITGameRecordSchema> itgrs = new List<DataSchemas.ITGameRecordSchema>();
            bool exists;
            try
            {
                try
                {
                    var cmd = new SQLiteCommand(conn_sync);
                    cmd.CommandText="select case when exists((select * from information_schema.tables where table_name = 'ITGameRecordSchema')) then 1 else 0 end";
                    exists = cmd.ExecuteScalar<int>() == 1;
                    Console.WriteLine("exists1: {0}", exists);
                }
                catch
                {
                    try
                    {
                        exists = true;
                        var cmdOthers = new SQLiteCommand(conn_sync);
                        cmdOthers.CommandText = "select 1 from ITGameRecordSchema where 1 = 0";
                        cmdOthers.ExecuteNonQuery();
                        Console.WriteLine("exists2: {0}", exists);
                    }
                    catch
                    {
                        exists = false;
                        Console.WriteLine("exists3: {0}", exists);
                    }
                }
                if (!exists) throw new System.ArgumentException("no table");
                Console.WriteLine("exists4: {0}", exists);
                itgrs = conn_sync.Query<DataSchemas.ITGameRecordSchema>("select * from ITGameRecordSchema");
            }
            catch (Exception ex)
            {
                Console.WriteLine("exists5");
//                return;
            }

            if (itgrs.Count() > 0)
            {
                itgrs.OrderBy(x => x.datetime);
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
                rtgrs.OrderBy(x => x.datetime);
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
                stgrs.OrderBy(x => x.datetime);
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
                dsgrs.OrderBy(x => x.datetime);
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
                dsgrs.OrderBy(x => x.datetime);
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
        }

        private async void SyncLocalDBwithServer(SQLiteAsyncConnection db)
        {
            bool dbexception = false;
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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
                List<DataSchemas.ITGameRecordSchema> tmpitems = new List<DataSchemas.ITGameRecordSchema>();
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
                        itgrs.OrderBy(x => x.datetime);
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
                List<DataSchemas.RTGameRecordSchema> tmpitems = new List<DataSchemas.RTGameRecordSchema>();
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
                        rtgrs.OrderBy(x => x.datetime);
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
                List<DataSchemas.StroopGameRecordSchema> tmpitems = new List<DataSchemas.StroopGameRecordSchema>();
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
                        stgrs.OrderBy(x => x.datetime);
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
                List<DataSchemas.DSGameRecordSchema> tmpitems = new List<DataSchemas.DSGameRecordSchema>();
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
                        dsgrs.OrderBy(x => x.datetime);
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
                        dsgrs.OrderBy(x => x.datetime);
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
                        ds_trialctr = Math.Max(ds_trialctr,dsgrs.Max(x => x.trialnum));
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
                List<DataSchemas.BrainGameSessionSchema> tmpitems = new List<DataSchemas.BrainGameSessionSchema>();
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


            #region UserSchema;
            ///////  CheckServerForDBUpdates
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
                if (q5 != null) 
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
                List<DataSchemas.UserSchema> tmpitems = new List<DataSchemas.UserSchema>();
                IDictionary<string, string> _headers = new Dictionary<string, string>();
                // TODO: Add header with auth-based token in chapter 7
                _headers.Add("zumo-api-version", "2.0.0");
                untypedItems = await bgsessions.ReadAsync("$filter=UserId%20eq%20'" + Settings.UserId, _headers);
                BGUser = untypedItems.ToObject<DataSchemas.UserSchema>();
                if (BGUser != null)
                    onremote = true;

                if (dbexception || (!q5.Any(x => x.Id == BGUser.Id)))//if the local db doesn't have this user, add it
                {
                    try
                    {
                        await db.InsertAsync(BGUser);
                        onlocal = true;
                    }
                    catch (Exception exA)
                    {
                        ;
                    }
                }

                if (!dbexception)
                {
                    if (!BGSessions.Any(x => x.Id == q5[0].Id))//if the remote server doesn't have this user, add it
                    {
                        try
                        {
                            await bguserinfoService.AddUserEntryAsync(q5[0]);
                            onremote = true;
                        }
                        catch (Exception exA)
                        {
                            ;
                        }

                    }
                }
            }
            catch (Exception exB)
            {
                ;
            }
            if (!onlocal) CreateUser(onremote);
            #endregion

            IsBusy = false;
            //            SendToServer(conn);
        }

        public async static void CreateUser(bool onremote)
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
                s.UserId = Settings.UserId;
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

                if(!onremote) await Task.Run(async () => { /*await GetScenariosFromServer(conn); */ SendSubscriptionToServer(s); });
            }
            Settings.LastVerifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }


        public async static void CreateOrUpdateUser(string _subscriptionId)
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

                await Task.Run(async () => { /*await GetScenariosFromServer(conn); */ SendSubscriptionToServer(s); });
            }
            Settings.LastVerifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public async static void SendSubscriptionToServer(DataSchemas.UserSchema user)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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
                IsBusy = false;
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
                SendITGRToServer(s);
        }

        public async static void SendITGRToServer(DataSchemas.ITGameRecordSchema itgr)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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
                IsBusy = false;
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
                SendRTGRToServer(s);
        }

        public async static void SendRTGRToServer(DataSchemas.RTGameRecordSchema rtgr)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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
                IsBusy = false;
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
                SendStroopGRToServer(s);
        }

        public async static void SendStroopGRToServer(DataSchemas.StroopGameRecordSchema stgr)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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
                IsBusy = false;
            }
        }

        public async static void WriteDSGR(Guid sessionid, int trialctr, int itemcnt, int ontimems, int offtimems, string direction, string items, bool repeats, bool repeats_cons, bool auto bool cor)
        {
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
                SendDSGRToServer(s);
        }

        public async static void SendDSGRToServer(DataSchemas.DSGameRecordSchema stgr)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                await bguserinfoService.AddDSGameRecordEntryAsync(stgr);
            }
            catch (Exception ex)
            {
                ;
            }
            finally
            {
                IsBusy = false;
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
                SendGameSessionToServer(s);
            return g;
        }

        public async static void SendGameSessionToServer(DataSchemas.BrainGameSessionSchema sess)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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
                IsBusy = false;
            }
        }

    }
}
