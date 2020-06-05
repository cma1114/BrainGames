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
                return;
            }

            if (itgrs.Count() == 0) return;

            itgrs.OrderBy(x => x.datetime);
            it_corarr = new List<bool>();
            it_empstimtimearr = new List<double>();
            foreach (DataSchemas.ITGameRecordSchema grs in itgrs)
            {
                it_corarr.Add(grs.cor);
                it_empstimtimearr.Add(grs.empstimtime);
            }
            it_trialctr = itgrs.Max(x => x.trialnum);
            it_reversalctr = itgrs.Max(x => x.reversalctr);
            it_laststimtime = itgrs[itgrs.Count()-1].stimtime;
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
                        it_corarr = new List<bool>();
                        it_empstimtimearr = new List<double>();
                        foreach (DataSchemas.ITGameRecordSchema grs in itgrs)
                        {
                            it_corarr.Add(grs.cor);
                            it_empstimtimearr.Add(grs.empstimtime);
                        }
                        it_trialctr = itgrs.Max(x => x.trialnum);
                        it_reversalctr = itgrs.Max(x => x.reversalctr);
                        it_laststimtime = itgrs[itgrs.Count() - 1].stimtime;
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

        public async static void WriteITGR(Guid sessionid, int trialctr, int reversalctr, double curstimdur, double empstimdur, int cor_ans, bool cor)
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
