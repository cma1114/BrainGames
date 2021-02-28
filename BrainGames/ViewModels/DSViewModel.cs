using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Xamarin.Forms;

using BrainGames.Utility;
using BrainGames.Models;
using BrainGames.Views;
using BrainGames.Converters;
using SkiaSharp;

namespace BrainGames.ViewModels
{
    public class DSViewModel : ViewModelBase
    {
        public ICommand ReadyButtonCommand { get; protected set; }
        //        public ICommand ReactButtonCommand { get; protected set; }
        public ICommand Button0Command { get; protected set; }
        public ICommand Button1Command { get; protected set; }
        public ICommand Button2Command { get; protected set; }
        public ICommand Button3Command { get; protected set; }
        public ICommand Button4Command { get; protected set; }
        public ICommand Button5Command { get; protected set; }
        public ICommand Button6Command { get; protected set; }
        public ICommand Button7Command { get; protected set; }
        public ICommand Button8Command { get; protected set; }

        public ICommand Button9Command { get; protected set; }

        public int timeout = 10000;
        public bool timedout = false;
        public Stopwatch timer = new Stopwatch();

        public int trialctr = 0;
        public int blocktrialctr = 0;
        public bool answered = false;

        protected Guid game_session_id;

        private int initspanlen = 6;
        private int initontimems = 500;
        private int initofftimems = 500;
        private int mindigit = 0;
        private int maxdigit = 9;
        private int incthresh = 4;
        private int decthresh = 2;
        private int timeincms = 60;
        private int timedecms = 40;
        private int mintimems = 30;
        private int maxtimems = 1000;

        private bool repeats_set = false;
        private bool repeats_cons = false;

        private int cortrialstreak, errtrialstreak;
        private List<Tuple<int, int>> last_ontimes_by_spanlen;
        private List<Tuple<int, int>> last_offtimes_by_spanlen;
        private List<Tuple<int, bool>> last_outcomes_by_spanlen;
        private int spanlen_f, spanlen_b, stimonms_f, stimonms_b, stimoffms_f, stimoffms_b;
        private double estspan_f = 0, estspan_b = 0;
        private bool auto_f = true, auto_b = true;
        private int cortrialstreak_b, errtrialstreak_b;
        private List<Tuple<int, int>> last_ontimes_by_spanlen_b;
        private List<Tuple<int, int>> last_offtimes_by_spanlen_b;
        private List<Tuple<int, bool>> last_outcomes_by_spanlen_b;

        public List<string> digitlist = new List<string>();
        public List<string> responselist = new List<string>();

        private bool _backward = false;
        public bool Backward
        {
            get => _backward;
            set 
            {//save values
                if (_backward)
                {
                    spanlen_b = spanlen;
                    stimonms_b = stimonms;
                    stimoffms_b = stimoffms;
                    estspan_b = EstSpan;
                    auto_b = AutoIncrement;
                }
                else
                {
                    spanlen_f = spanlen;
                    stimonms_f = stimonms;
                    stimoffms_f = stimoffms;
                    estspan_f = EstSpan;
                    auto_f = AutoIncrement;
                }
                SetProperty(ref _backward, value);
                //restore values
                spanlen = _backward ? spanlen_b : spanlen_f;
                stimonms = _backward ? stimonms_b : stimonms_f;
                stimoffms = _backward ? stimoffms_b : stimoffms_f;
                EstSpan = _backward ? estspan_b : estspan_f;
                AutoIncrement = _backward ? auto_b : auto_f;
            }
        }

        private bool _autoIncrement = true;
        public bool AutoIncrement
        {
            get => _autoIncrement;
            set
            {
                SetProperty(ref _autoIncrement, value);
                if (Backward) auto_b = AutoIncrement;
                else auto_f = AutoIncrement;
            }
        }

        private int _stimonms = 0;
        public int stimonms
        {
            get => _stimonms;
            set { SetProperty(ref _stimonms, value); }
        }
        public int _stimoffms = 0;
        public int stimoffms
        {
            get => _stimoffms;
            set { SetProperty(ref _stimoffms, value); }
        }

        private int _minlen = 2;
        public int minlen
        {
            get => _minlen;
            set { SetProperty(ref _minlen, value); }
        }

        private int _maxlen = 20;
        public int maxlen
        {
            get => _maxlen;
            set { SetProperty(ref _maxlen, value); }
        }

        private Color _ansClr = Color.Gray;
        public Color AnsClr
        {
            get => _ansClr;
            set { SetProperty(ref _ansClr, value); }
        }

        private int _spanlen = 0;
        public int spanlen
        {
            get => _spanlen;
            set { SetProperty(ref _spanlen, value); }
        }

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set { SetProperty(ref _isRunning, value); }
        }

        private bool _enableButtons = false;
        public bool EnableButtons
        {
            get => _enableButtons;
            set { SetProperty(ref _enableButtons, value); }
        }

        private double _estSpan = 0;
        public double EstSpan
        {
            get => _estSpan;
            set { SetProperty(ref _estSpan, value); }
        }

        public DSViewModel()
        {
            App.mum.LoadDSGR();
            ReadyButtonCommand = new Command(ReadyButton);
            Button0Command = new Command(Button0);
            Button1Command = new Command(Button1);
            Button2Command = new Command(Button2);
            Button3Command = new Command(Button3);
            Button4Command = new Command(Button4);
            Button5Command = new Command(Button5);
            Button6Command = new Command(Button6);
            Button7Command = new Command(Button7);
            Button8Command = new Command(Button8);
            Button9Command = new Command(Button9);
            game_session_id = MasterUtilityModel.WriteGameSession("DS");

            if (App.mum.ds_trialctr == 0)
            {
                trialctr = 0;
                cortrialstreak = 0;
                errtrialstreak = 0;
                spanlen = initspanlen;
                stimonms = initontimems;
                stimoffms = initofftimems;
                spanlen_f = initspanlen;
                stimonms_f = initontimems;
                stimoffms_f = initofftimems;
                spanlen_b = initspanlen;
                stimonms_b = initontimems;
                stimoffms_b = initofftimems;
                last_ontimes_by_spanlen = new List<Tuple<int, int>>();
                last_offtimes_by_spanlen = new List<Tuple<int, int>>();
                last_outcomes_by_spanlen = new List<Tuple<int, bool>>();
                cortrialstreak_b = 0;
                errtrialstreak_b = 0;
                last_ontimes_by_spanlen_b = new List<Tuple<int, int>>();
                last_offtimes_by_spanlen_b = new List<Tuple<int, int>>();
                last_outcomes_by_spanlen_b = new List<Tuple<int, bool>>();
            }
            else
            {
                trialctr = App.mum.ds_trialctr;
                cortrialstreak = App.mum.ds_cortrialstreak;
                errtrialstreak = App.mum.ds_errtrialstreak;
                last_ontimes_by_spanlen = App.mum.ds_last_ontimes_by_spanlen;
                last_offtimes_by_spanlen = App.mum.ds_last_offtimes_by_spanlen;
                last_outcomes_by_spanlen = App.mum.ds_last_outcomes_by_spanlen;
                cortrialstreak_b = App.mum.ds_cortrialstreak_b;
                errtrialstreak_b = App.mum.ds_errtrialstreak_b;
                last_ontimes_by_spanlen_b = App.mum.ds_last_ontimes_by_spanlen_b;
                last_offtimes_by_spanlen_b = App.mum.ds_last_offtimes_by_spanlen_b;
                last_outcomes_by_spanlen_b = App.mum.ds_last_outcomes_by_spanlen_b;
                spanlen_f = App.mum.ds_lastspan == 0 ? initspanlen : App.mum.ds_lastspan;
                spanlen_b = App.mum.ds_lastspan_b == 0 ? initspanlen : App.mum.ds_lastspan_b;
                stimonms_f = last_ontimes_by_spanlen.Count() == 0 ? initontimems : last_ontimes_by_spanlen.Where(x => x.Item1 == spanlen_f).First().Item2;
                stimoffms_f = last_offtimes_by_spanlen.Count() == 0 ? initofftimems : last_offtimes_by_spanlen.Where(x => x.Item1 == spanlen_f).First().Item2;
                stimonms_b = last_ontimes_by_spanlen_b.Count() == 0 ? initontimems : last_ontimes_by_spanlen_b.Where(x => x.Item1 == spanlen_b).First().Item2;
                stimoffms_b = last_offtimes_by_spanlen_b.Count() == 0 ? initofftimems : last_offtimes_by_spanlen_b.Where(x => x.Item1 == spanlen_b).First().Item2;
                estspan_f = App.mum.ds_estspan_f;
                estspan_b = App.mum.ds_estspan_b;
                auto_f = App.mum.ds_auto_f;
                auto_b = App.mum.ds_auto_b;
                if (App.mum.ds_lastdir == "f") AutoIncrement = auto_f;
                else AutoIncrement = auto_b;

                if (AutoIncrement)
                {
                    if (cortrialstreak == incthresh)
                    {
                        spanlen_f = Math.Max(spanlen_f + 1, minlen);
                        cortrialstreak = 0;
                    }
                    else if (errtrialstreak == decthresh)
                    {
                        spanlen_f = Math.Min(spanlen_f - 1, maxlen);
                        errtrialstreak = 0;
                    }
                    if (cortrialstreak_b == incthresh)
                    {
                        spanlen_b = Math.Max(spanlen_b + 1, minlen);
                        cortrialstreak_b = 0;
                    }
                    else if (errtrialstreak_b == decthresh)
                    {
                        spanlen_b = Math.Min(spanlen_b - 1, maxlen);
                        errtrialstreak_b = 0;
                    }
                }

                //Backward initialized to false by default, so preset these
                spanlen = spanlen_f;
                stimonms = stimonms_f;
                stimoffms = stimoffms_f;
                EstSpan = estspan_f;
                AutoIncrement = auto_f;
                if (App.mum.ds_lastdir == "f")
                {
                    Backward = false;
                }
                else
                {
                    Backward = true;
                }
            }
        }

        public void ReadyButton()
        {
            blocktrialctr = 0;
            answered = false;
            timedout = false;
            EnableButtons = false;
            AnsClr = Color.Gray;

            digitlist.Clear();
            responselist.Clear();

            for (int i = 0; i < spanlen; i++)
            {
                string d;
                if (repeats_set || maxdigit-mindigit + 1 < spanlen)//circumstances force you to repeat digits
                {
                    repeats_set = true;
                    if (repeats_cons)
                    {
                        d = MasterUtilityModel.RandomNumber(mindigit, maxdigit + 1).ToString();
                    }
                    else
                    {
                        do
                        {
                            d = MasterUtilityModel.RandomNumber(mindigit, maxdigit + 1).ToString();
                        } while (digitlist.Count() > 0 && digitlist[digitlist.Count - 1] == d);
                    }
                }
                else
                {
                    repeats_set = false;
                    do
                    {
                        d = MasterUtilityModel.RandomNumber(mindigit, maxdigit + 1).ToString();
                    } while (digitlist.Contains(d));
                }
                digitlist.Add(d);
            }

            timer.Reset();
            IsRunning = true;
        }

        public void Button0() { responselist.Add("0"); ResponseButton(); }
        public void Button1() { responselist.Add("1"); ResponseButton(); }
        public void Button2() { responselist.Add("2"); ResponseButton(); }
        public void Button3() { responselist.Add("3"); ResponseButton(); }
        public void Button4() { responselist.Add("4"); ResponseButton(); }
        public void Button5() { responselist.Add("5"); ResponseButton(); }
        public void Button6() { responselist.Add("6"); ResponseButton(); }
        public void Button7() { responselist.Add("7"); ResponseButton(); }
        public void Button8() { responselist.Add("8"); ResponseButton(); }
        public void Button9() { responselist.Add("9"); ResponseButton(); }

        private bool MatchLists()
        {
            List<string> testlist = new List<string>(digitlist);
            if (Backward)
            {
                testlist.Reverse();
            }
            for (int i = 0; i < responselist.Count() && i < testlist.Count(); i++)
            {
                if (responselist[i] != testlist[i]) return false;
            }
            return true;
        }

        public void OnDisappearing()
        {
            List<DataSchemas.DSGameRecordSchema> ur = new List<DataSchemas.DSGameRecordSchema>();
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                List<double> avgs = new List<double>();
                List<double> bests = new List<double>();
                bests.Add(ur.Where(x => x.cor == true && x.direction == "f").Count() == 0 ? 0 : ur.Where(x => x.cor == true && x.direction == "f").Select(x => x.itemcnt).Max());
                bests.Add(ur.Where(x => x.cor == true && x.direction == "b").Count() == 0 ? 0 : ur.Where(x => x.cor == true && x.direction == "b").Select(x => x.itemcnt).Max());
                bests.Add(bests[0] == 0 ? 9999 : ur.Where(x => x.cor == true && x.direction == "f" && x.itemcnt == bests[0]).Select(x => x.ontimems + x.offtimems).Min());
                bests.Add(bests[1] == 0 ? 9999 : ur.Where(x => x.cor == true && x.direction == "b" && x.itemcnt == bests[1]).Select(x => x.ontimems + x.offtimems).Min());
                Thread t = new Thread(() => App.mum.UpdateUserStats("DS", avgs, bests));
                t.Start();
            }
        }

        public void ResponseButton()
        {
            if (responselist.Count() < digitlist.Count() && MatchLists() && !timedout) return; //no errors yet, and you haven't timed out
            answered = true;
            IsRunning = false;
            EnableButtons = false;
            timer.Stop();

            bool cor;
            if (responselist.Count() == digitlist.Count() && MatchLists())
            {
                cor = true;
                if (AutoIncrement)
                {
                    if (Backward)
                    {
                        cortrialstreak_b++;
                        errtrialstreak_b = 0;
                    }
                    else
                    {
                        cortrialstreak++;
                        errtrialstreak = 0;
                    }
                }
                AnsClr = Color.ForestGreen;
            }
            else
            {
                cor = false;
                if (AutoIncrement)
                {
                    if (Backward)
                    {
                        cortrialstreak_b = 0;
                        errtrialstreak_b++;
                    }
                    else
                    {
                        cortrialstreak = 0;
                        errtrialstreak++;
                    }
                }
                AnsClr = Color.OrangeRed;
            }
            Tuple<double, double> estspans = null;
            bool done = false;
            Task.Run(async () => {
                try
                {
                    estspans = await MasterUtilityModel.WriteDSGR(game_session_id, ++trialctr, spanlen, stimonms, stimoffms, (int)timer.ElapsedMilliseconds, Backward ? "b" : "f", String.Join("~", digitlist), repeats_set, repeats_cons, AutoIncrement, cor);
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
            if (estspans != null)
            { 
                estspan_f = estspans.Item1;
                estspan_b = estspans.Item2;
            }

            if (Backward) EstSpan = estspan_b;
            else EstSpan = estspan_f;

            if (AutoIncrement)
            {
                if (Backward)
                {
                    if (cortrialstreak_b == incthresh)
                    {
                        spanlen = Math.Min(spanlen + 1, maxlen);
                        cortrialstreak_b = 0;
                    }
                    else if (errtrialstreak_b == decthresh)
                    {
                        spanlen = Math.Max(spanlen - 1, minlen);
                        errtrialstreak_b = 0;
                    }
                    else if (cortrialstreak_b > 0)
                    {
                        if (MasterUtilityModel.RandomNumber(0, 3) <= 1) stimonms = Math.Max(stimonms - timedecms, mintimems);
                        else stimoffms = Math.Max(stimoffms - timedecms, mintimems);
                    }
                    else if (errtrialstreak_b > 0)
                    {
                        if (MasterUtilityModel.RandomNumber(0, 3) <= 1) stimonms = Math.Min(stimonms + timeincms, maxtimems);
                        else stimoffms = Math.Min(stimoffms + timeincms, maxtimems);
                    }
                }
                else
                {
                    if (cortrialstreak == incthresh)
                    {
                        spanlen = Math.Min(spanlen + 1, maxlen);
                        cortrialstreak = 0;
                    }
                    else if (errtrialstreak == decthresh)
                    {
                        spanlen = Math.Max(spanlen - 1, minlen);
                        errtrialstreak = 0;
                    }
                    else if (cortrialstreak > 0)
                    {
                        if (stimonms == mintimems) stimoffms = Math.Max(stimoffms - timedecms, mintimems);
                        else if (stimoffms == mintimems) stimonms = Math.Max(stimonms - timedecms, mintimems);
                        else if (MasterUtilityModel.RandomNumber(0, 3) <= 1) stimonms = Math.Max(stimonms - timedecms, mintimems);
                        else stimoffms = Math.Max(stimoffms - timedecms, mintimems);
                    }
                    else if (errtrialstreak > 0)
                    {
                        if (stimonms == maxtimems) stimoffms = Math.Min(stimoffms + timeincms, maxtimems);
                        else if (stimoffms == maxtimems) stimonms = Math.Min(stimonms + timeincms, maxtimems);
                        else if (MasterUtilityModel.RandomNumber(0, 3) <= 1) stimonms = Math.Min(stimonms + timeincms, maxtimems);
                        else stimoffms = Math.Min(stimoffms + timeincms, maxtimems);
                    }
                }
            }
        }
    }
}
