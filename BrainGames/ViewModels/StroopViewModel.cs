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
    public class StroopViewModel : ViewModelBase
    {
        public enum textcolortypes { red, green, blue, yellow };
        public List<SKColor> colortypes = new List<SKColor>() { SKColors.Red, SKColors.Green, SKColors.Blue, SKColors.Yellow };
        public List<string> colorwords = new List<string>() { "RED", "GREEN", "BLUE", "YELLOW" };

        public ICommand ReadyButtonCommand { get; protected set; }
        //        public ICommand ReactButtonCommand { get; protected set; }
        public ICommand Button1Command { get; protected set; }
        public ICommand Button2Command { get; protected set; }
        public ICommand Button3Command { get; protected set; }
        public ICommand Button4Command { get; protected set; }

        public int fixationondurms = 200;
        public int fixationoffdurms = 100;
        public int itims = 500;
        public int trialsperset = 12;

        protected Guid game_session_id;

        private int maxwcnt = 4;
        private int minwcnt = 2;
        private int maxwccnt = 2;
        private int minwccnt = 0;
        private int maxconcnt = 5;
        private int minconcnt = 3;

        public int timeout = 4000;

        public int trialctr = 0;
        public double cumcorrt = 0;
        public int cortrialcnt = 0;
        public int contrialcnt = 0;
        public double cumconcorrt = 0;
        public int corcontrialcnt;
        public int incontrialcnt = 0;
        public double cuminconcorrt = 0;
        public int corincontrialcnt;
        public int blocktrialctr = 0;
        public bool cor = false;
        public List<string> words = new List<string>();
        public textcolortypes[] textcolors;


        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set { SetProperty(ref _isRunning, value); }
        }

        private double _avgRT = 0;
        public double AvgRT
        {
            get => _avgRT;
            set { SetProperty(ref _avgRT, value); }
        }

        private double _difRT = 0;
        public double DifRT
        {
            get => _difRT;
            set { SetProperty(ref _difRT, value); }
        }

        private double _reactionTime = 0;
        public double ReactionTime
        {
            get => _reactionTime;
            set { SetProperty(ref _reactionTime, value); }
        }

        public StroopViewModel()
        {
            App.mum.LoadStroopGR();
            textcolors = new textcolortypes[trialsperset];
            ReadyButtonCommand = new Command(ReadyButton);
            //            ReactButtonCommand = new Command(ReactButton);
            Button1Command = new Command(Button1);
            Button2Command = new Command(Button2);
            Button3Command = new Command(Button3);
            Button4Command = new Command(Button4);
            game_session_id = MasterUtilityModel.WriteGameSession("Stroop");
            trialctr = App.mum.st_trialctr;
            cortrialcnt = App.mum.st_cortrialcnt;
            cumcorrt = App.mum.st_cumcorrt;

            contrialcnt = App.mum.st_contrialcnt;
            corcontrialcnt = App.mum.st_corcontrialcnt;
            cumconcorrt = App.mum.st_cumconcorrt;
            incontrialcnt = App.mum.st_incontrialcnt;
            corincontrialcnt = App.mum.st_corincontrialcnt;
            cuminconcorrt = App.mum.st_cuminconcorrt;

            if (cortrialcnt > 0) AvgRT = cumcorrt / cortrialcnt;
            if (corcontrialcnt > 0 && corincontrialcnt > 0) DifRT = cuminconcorrt / corincontrialcnt - cumconcorrt / corcontrialcnt;
        }


        public void Button1()
        {
            if (textcolors[blocktrialctr] == textcolortypes.red)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void Button2()
        {
            if (textcolors[blocktrialctr] == textcolortypes.green)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void Button3()
        {
            if (textcolors[blocktrialctr] == textcolortypes.blue)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void Button4()
        {
            if (textcolors[blocktrialctr] == textcolortypes.yellow)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }

        public void ReadyButton()
        {
            blocktrialctr = 0;

            #region load words and colors
            List<string> cwords = new List<string>(colorwords);

            //choose how many reps each word gets
            int w1 = MasterUtilityModel.RandomNumber(minwcnt, maxwcnt + 1);
            int w2 = MasterUtilityModel.RandomNumber(minwcnt, maxwcnt + 1);
            int w3 = MasterUtilityModel.RandomNumber(Math.Max(minwcnt, trialsperset - (w1 + w2) - maxwcnt), Math.Min(maxwcnt + 1, trialsperset - (w1 + w2) - minwcnt) + 1);
            int w4 = trialsperset - (w1 + w2 + w3);
            List<int> wcnts = new List<int>() { w1, w2, w3, w4 };
            ///////////////////////////////////
            
            //choose how many congruent trials each word gets
            List<int> ctcnts = new List<int>();
            int totalconcnt = 0;
            while (totalconcnt < minconcnt || totalconcnt > maxconcnt)
            {
                totalconcnt = 0;
                ctcnts.Clear();
                for (int i = 0; i < 4; i++) 
                {
                    int cc = MasterUtilityModel.RandomNumber(minwccnt, maxwccnt + 1);
                    ctcnts.Add(cc);
                    totalconcnt += cc; 
                }
            }

            List<int> wctnts = new List<int>();
            int cv;
            for (int i = 0; i < wcnts.Count(); i++)
            {
                if (wcnts[i] < maxwccnt + 1)//take care of low-rep words first
                {
                    if (ctcnts.Where(x => x < maxwccnt).Count() > 0)
                    {
                        var l = ctcnts.Where(x => x < maxwccnt).ToList();
                        l.Shuffle();
                        cv = l[0];
                        wctnts.Add(cv);
                        ctcnts.Remove(cv);
                    }
                }
            }
            for (int i = 0; i < wcnts.Count(); i++)
            {
                if (wcnts[i] >= maxwccnt + 1)
                {
                    if (ctcnts.Where(x => x == maxwccnt).Count() > 0)
                    {
                        cv = ctcnts.Where(x => x == maxwccnt).ToList()[0];
                        wctnts.Add(cv);
                        ctcnts.Remove(cv);
                    }
                    else
                    {
                        var l = ctcnts.Where(x => x < maxwccnt).ToList();
                        l.Shuffle();
                        cv = l[0];
                        wctnts.Add(cv);
                        ctcnts.Remove(cv);
                    }
                }
            }
            /////////////////////////

            //choose word-rep mappings
            int idx = MasterUtilityModel.RandomNumber(0, 4);
            string wm1 = cwords[idx];
            cwords.RemoveAt(idx);
            idx = MasterUtilityModel.RandomNumber(0, 3);
            string wm2 = cwords[idx];
            cwords.RemoveAt(idx);
            idx = MasterUtilityModel.RandomNumber(0, 2);
            string wm3 = cwords[idx];
            cwords.RemoveAt(idx);
            string wm4 = cwords[0];
            List<string> wms = new List<string>() { wm1, wm2, wm3, wm4 };
            //////////////////////////////////////////////////////////

            //load up words
            words.Clear();
            for (int i = 0; i < wcnts.Count(); i++)
            {
                for(int j = 0; j < wcnts[i]; j++)
                {
                    words.Add(wms[i]);
                }
            }
            words.Shuffle();
            ///////////////////////////////////////////

            //load up colors
            for (int i = 0; i < wms.Count(); i++)
            {
                //get all indexes for that word
                var idxs = words.Select((v, li) => new { Index = li, Value = v })     // Pair up values and indexes
                                            .Where(p => p.Value == wms[i])   // Do the filtering
                                        .Select(p => p.Index).ToList(); // Keep the index and drop the value
                //take care of congruent trials first
                for (int j = 0; j < wctnts[i]; j++)
                {
                    int ridx = idxs[MasterUtilityModel.RandomNumber(0, idxs.Count)];
                    textcolors[ridx] = (textcolortypes)colorwords.IndexOf(wms[i]);
                    idxs.Remove(ridx);
                }
                //now do incongruent
                for (int j = 0; j < wcnts[i] - wctnts[i]; j++)
                {
                    int ridx = idxs[MasterUtilityModel.RandomNumber(0, idxs.Count)];
                    int cidx;
                    do
                    {
                        cidx = MasterUtilityModel.RandomNumber(0, colorwords.Count());
                    } while (cidx == colorwords.IndexOf(wms[i]));
                    textcolors[ridx] = (textcolortypes)cidx;
                    idxs.Remove(ridx);
                }

            }
            //////////////////////////////////////////
            #endregion
            cor = false;//set this for non-responses
            IsRunning = true;
        }

        public void ReactButton(int tctr, double rt, double avgrt, double difrt, string word, string textcolor, bool congruent, bool correct)
        {
            MasterUtilityModel.WriteStroopGR(game_session_id, tctr, rt, avgrt, difrt, word, textcolor, congruent, correct);
            cor = false;
        }

        public void OnDisappearing()
        {
            List<DataSchemas.StroopGameRecordSchema> ur = new List<DataSchemas.StroopGameRecordSchema>();
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.StroopGameRecordSchema>("select * from StroopGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                List<double> avgs = new List<double>();
                List<double> bests = new List<double>();
                if (ur.Where(x => x.cor == true && x.congruent == true).Count() > 0 && ur.Where(x => x.cor == true && x.congruent == false).Count() > 0)
                {
                    avgs.Add(Utilities.MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, ((x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()) + (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count())) / 2)).OrderBy(x => x.Item1).ToList(), 1).Last().Item2);
                    avgs.Add(Utilities.MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList(), 1).Last().Item2);
                }
                else
                {
                    avgs.Add(9999);
                    avgs.Add(9999);
                }
                Thread t = new Thread(() => App.mum.UpdateUserStats("Stroop", avgs, bests));
                t.Start();
            }
        }
    }
}

