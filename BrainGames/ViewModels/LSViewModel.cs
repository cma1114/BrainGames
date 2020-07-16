using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using BrainGames.Utility;
using BrainGames.Controls;

namespace BrainGames.ViewModels
{
    public class LSViewModel : ViewModelBase
    {
        public ICommand ReadyButtonCommand { get; protected set; }

        public int timeout = 60000;
        public bool timedout = false;
        public Stopwatch timer = new Stopwatch();

        public int trialctr = 0;
        public int blocktrialctr = 0;
        public bool answered = false;

        protected Guid game_session_id;

        private int initspanlen = 4;
        private int initgridsize = 5;
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
        private int spanlen_f, spanlen_b, stimonms_f, stimonms_b, stimoffms_f, stimoffms_b, gridsize_f, gridsize_b;
        private int cortrialstreak_b, errtrialstreak_b;
        private List<Tuple<int, int>> last_ontimes_by_spanlen_b;
        private List<Tuple<int, int>> last_offtimes_by_spanlen_b;
        private List<Tuple<int, bool>> last_outcomes_by_spanlen_b;

        public List<string> digitlist = new List<string>();
        public List<string> responselist = new List<string>();

        private Tile[,] _tiles;
        public int gridsize;

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
                    gridsize_b = gridsize;
                }
                else
                {
                    spanlen_f = spanlen;
                    stimonms_f = stimonms;
                    stimoffms_f = stimoffms;
                    gridsize_f = gridsize;
                }
                SetProperty(ref _backward, value);
                //restore values
                spanlen = _backward ? spanlen_b : spanlen_f;
                stimonms = _backward ? stimonms_b : stimonms_f;
                stimoffms = _backward ? stimoffms_b : stimoffms_f;
                gridsize = _backward ? gridsize_b : gridsize_f;
            }
        }

        private bool _autoIncrement = true;
        public bool AutoIncrement
        {
            get => _autoIncrement;
            set
            {
                SetProperty(ref _autoIncrement, value);
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

        public void AddTile(Tile tile)
        {
            tile.Tapped += TileTapped;
            _tiles[tile.XPos, tile.YPos] = tile;
        }

        public /*async*/ void FlipTile(string t)
        {
                Console.WriteLine("flipping");
            int ti = Convert.ToInt32(t);
//            await _tiles[t % gridsize, (int)Math.Floor((double)t / gridsize)].Flip();
            _tiles[ti % gridsize, (int)Math.Floor((double)ti / gridsize)].FlipIt();
        }

        private void TileTapped(object sender, TileTappedEventArgs e)
        {
//            Console.WriteLine("tapped");
            responselist.Add((e.XPos + e.YPos * gridsize).ToString()); 
            ResponseButton(); 
        }

        public LSViewModel()
        {
            ReadyButtonCommand = new Command(ReadyButton);
            game_session_id = MasterUtilityModel.WriteGameSession("LS");

            if (App.mum.ls_trialctr == 0)
            {
                trialctr = 0;
                cortrialstreak = 0;
                errtrialstreak = 0;
                gridsize = initgridsize;
                gridsize_f = initgridsize;
                gridsize_b = initgridsize;
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
                trialctr = App.mum.ls_trialctr;
                cortrialstreak = App.mum.ls_cortrialstreak;
                errtrialstreak = App.mum.ls_errtrialstreak;
                last_ontimes_by_spanlen = App.mum.ls_last_ontimes_by_spanlen;
                last_offtimes_by_spanlen = App.mum.ls_last_offtimes_by_spanlen;
                last_outcomes_by_spanlen = App.mum.ls_last_outcomes_by_spanlen;
                cortrialstreak_b = App.mum.ls_cortrialstreak_b;
                errtrialstreak_b = App.mum.ls_errtrialstreak_b;
                last_ontimes_by_spanlen_b = App.mum.ls_last_ontimes_by_spanlen_b;
                last_offtimes_by_spanlen_b = App.mum.ls_last_offtimes_by_spanlen_b;
                last_outcomes_by_spanlen_b = App.mum.ls_last_outcomes_by_spanlen_b;
                spanlen_f = App.mum.ls_lastspan == 0 ? initspanlen : App.mum.ls_lastspan;
                spanlen_b = App.mum.ls_lastspan_b == 0 ? initspanlen : App.mum.ls_lastspan_b;
                stimonms_f = last_ontimes_by_spanlen.Count() == 0 ? initontimems : last_ontimes_by_spanlen.Where(x => x.Item1 == spanlen_f).First().Item2;
                stimoffms_f = last_offtimes_by_spanlen.Count() == 0 ? initofftimems : last_offtimes_by_spanlen.Where(x => x.Item1 == spanlen_f).First().Item2;
                stimonms_b = last_ontimes_by_spanlen_b.Count() == 0 ? initontimems : last_ontimes_by_spanlen_b.Where(x => x.Item1 == spanlen_b).First().Item2;
                stimoffms_b = last_offtimes_by_spanlen_b.Count() == 0 ? initofftimems : last_offtimes_by_spanlen_b.Where(x => x.Item1 == spanlen_b).First().Item2;
                gridsize_f = App.mum.ls_lastgridsize_f == 0 ? initgridsize : App.mum.ls_lastgridsize_f;
                gridsize_b = App.mum.ls_lastgridsize_b == 0 ? initgridsize : App.mum.ls_lastgridsize_b;

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
                gridsize = gridsize_f;
                if (App.mum.ls_lastdir == "f")
                {
                    Backward = false;
                }
                else
                {
                    Backward = true;
                }
            }
            maxdigit = gridsize * gridsize - 1;
            _tiles = new Tile[gridsize, gridsize];
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
            maxdigit = gridsize * gridsize - 1;
            _tiles = new Tile[gridsize, gridsize];

            for (int i = 0; i < spanlen; i++)
            {
                string d;
                if (repeats_set || maxdigit - mindigit + 1 < spanlen)//circumstances force you to repeat digits
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

        private bool MatchLists()
        {
            List<string> testlist = new List<string>(digitlist);
            if (Backward)
            {
                testlist.Reverse();
            }
            for (int i = 0; i < responselist.Count() && i < testlist.Count(); i++)
            {
                if (responselist[i] != testlist[i].ToString()) return false;
            }
            return true;
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
            MasterUtilityModel.WriteLSGR(game_session_id, ++trialctr, spanlen, stimonms, stimoffms, gridsize, (int)timer.ElapsedMilliseconds, Backward ? "b" : "f", String.Join("~", digitlist), repeats_set, repeats_cons, AutoIncrement, cor);
            Console.WriteLine("digitlist: {0}", String.Join("~", digitlist));
            Console.WriteLine("responselist: {0}", String.Join("~", responselist));

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
                        if (MasterUtilityModel.RandomNumber(0, 3) <= 1) stimonms = Math.Max(stimonms - timedecms, mintimems);
                        else stimoffms = Math.Max(stimoffms - timedecms, mintimems);
                    }
                    else if (errtrialstreak > 0)
                    {
                        if (MasterUtilityModel.RandomNumber(0, 3) <= 1) stimonms = Math.Min(stimonms + timeincms, maxtimems);
                        else stimoffms = Math.Min(stimoffms + timeincms, maxtimems);
                    }
                }
            }
        }
    }
}
