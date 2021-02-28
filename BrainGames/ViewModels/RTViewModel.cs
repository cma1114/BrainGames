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

namespace BrainGames.ViewModels
{
    public class RTViewModel : ViewModelBase
    {
        public ICommand ReadyButtonCommand { get; protected set; }
        //        public ICommand ReactButtonCommand { get; protected set; }
        public ICommand LeftButtonCommand { get; protected set; }
        public ICommand RightButtonCommand { get; protected set; }
        public ICommand Button1Command { get; protected set; }
        public ICommand Button2Command { get; protected set; }
        public ICommand Button3Command { get; protected set; }
        public ICommand Button4Command { get; protected set; }

        protected Guid game_session_id;

        private int pauseminms = 1000;
        private int pausemaxms = 3000;

        public int trialsperset = 6;
        public List<int> pausedurarr = new List<int>();
        public int timeout = 1000;
        public int trialctr = 0;
        public double cumrt = 0;
        public double ss1_cumrt = 0;
        public double ss2_cumcorrt = 0;
        public double ss4_cumcorrt = 0;
        public int ss1_trialcnt;
        public int ss2_trialcnt;
        public int ss2_cortrialcnt;
        public int ss4_trialcnt;
        public int ss4_cortrialcnt;
        public int blocktrialctr = 0;
        public int boxes = 0;
        public bool cor = false;
        public List<int> corboxes = new List<int>();

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set { SetProperty(ref _isRunning, value); }
        }

        private string _boxopt = "auto";
        public string boxopt
        {
            get => _boxopt;
            set { SetProperty(ref _boxopt, value); }
        }

        private bool _showReact1 = false;
        public bool ShowReact1
        {
            get => _showReact1;
            set { SetProperty(ref _showReact1, value); }
        }

        private bool _showReact2 = false;
        public bool ShowReact2
        {
            get => _showReact2;
            set { SetProperty(ref _showReact2, value); }
        }

        private bool _showReact4 = false;
        public bool ShowReact4
        {
            get => _showReact4;
            set { SetProperty(ref _showReact4, value); }
        }

        private double _avgRT = 0;
        public double AvgRT
        {
            get => _avgRT;
            set { SetProperty(ref _avgRT, value); }
        }
        private double _reactionTime = 0;
        public double ReactionTime
        {
            get => _reactionTime;
            set { SetProperty(ref _reactionTime, value); }
        }

        public RTViewModel()
        {
            App.mum.LoadRTGR();
            ReadyButtonCommand = new Command(ReadyButton);
//            ReactButtonCommand = new Command(ReactButton);
            LeftButtonCommand = new Command(LeftButton);
            RightButtonCommand = new Command(RightButton);
            Button1Command = new Command(Button1);
            Button2Command = new Command(Button2);
            Button3Command = new Command(Button3);
            Button4Command = new Command(Button4);
            game_session_id = MasterUtilityModel.WriteGameSession("RT");
            trialctr = App.mum.rt_trialctr;
            ss1_trialcnt = App.mum.rt_ss1_trialcnt;
            ss2_trialcnt = App.mum.rt_ss2_trialcnt;
            ss2_cortrialcnt = App.mum.rt_ss2_cortrialcnt;
            ss4_trialcnt = App.mum.rt_ss4_trialcnt;
            ss4_cortrialcnt = App.mum.rt_ss4_cortrialcnt;
            ss1_cumrt = App.mum.rt_ss1_cumrt;
            ss2_cumcorrt = App.mum.rt_ss2_cumcorrt;
            ss4_cumcorrt = App.mum.rt_ss4_cumcorrt;
            if (App.mum.rt_auto) boxopt = "auto";
            else boxopt = Convert.ToString(App.mum.rt_lastboxes);
            /*
            if (ss2_trialcnt >= 10 && (float)ss2_cortrialcnt / ss2_trialcnt >= 0.9 && ss4_trialcnt >= 10 && (float)ss4_cortrialcnt / ss4_trialcnt >= 0.9)
            {
                AvgRT = ((float)ss1_cumrt / ss1_trialcnt + (float)ss2_cumcorrt / ss2_cortrialcnt + (float)ss4_cumcorrt / ss4_cortrialcnt) / 3.0;
            }
            else if (ss2_trialcnt >= 10 && (float)ss2_cortrialcnt / ss2_trialcnt >= 0.9)
            {
                AvgRT = ((float)ss1_cumrt / ss1_trialcnt + (float)ss2_cumcorrt / ss2_cortrialcnt) / 2.0;
            }
            else if (ss4_trialcnt >= 10 && (float)ss4_cortrialcnt / ss4_trialcnt >= 0.9)
            {
                AvgRT = ((float)ss1_cumrt / ss1_trialcnt + (float)ss4_cumcorrt / ss4_cortrialcnt) / 2.0;
            }
            else { AvgRT = (float)ss1_cumrt / ss1_trialcnt; }*/
        }

        public void LeftButton()
        {
            if (corboxes[blocktrialctr] == 0)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }

        public void RightButton()
        {
            if (corboxes[blocktrialctr] == 1)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }

        public void Button1()
        {
            if (corboxes[blocktrialctr] == 0)
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
            if (corboxes[blocktrialctr] == 1)
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
            if (corboxes[blocktrialctr] == 2)
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
            if (corboxes[blocktrialctr] == 3)
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
            if (boxopt == "auto") boxes = (int)Math.Pow(2, MasterUtilityModel.RandomNumber(0, 3));
            else boxes = Convert.ToInt32(boxopt);
            if (boxes == 1)
            {
                ShowReact1 = true;
                ShowReact2 = false;
                ShowReact4 = false;
                timeout = 1000;
            }
            else if (boxes == 2)
            {
                ShowReact1 = false;
                ShowReact2 = true;
                ShowReact4 = false;
                timeout = 1500;
            }
            else
            {
                ShowReact1 = false;
                ShowReact2 = false;
                ShowReact4 = true;
                timeout = 2000;
            }
            pausedurarr.Clear();
            corboxes.Clear();
            for (int i = 0; i < trialsperset; i++)
            {
                pausedurarr.Add(MasterUtilityModel.RandomNumber(pauseminms, pausemaxms + 1));
                corboxes.Add(MasterUtilityModel.RandomNumber(0, boxes));
            }
            IsRunning = true;
        }

        public void ReactButton(int tctr, double rt, double avgrt, int corbox, bool correct)
        {
//            MasterUtilityModel.WriteRTGR(game_session_id, trialctr, reactiontime, cumrt/trialctr, boxes, corboxes[blocktrialctr], cor);
            MasterUtilityModel.WriteRTGR(game_session_id, tctr, rt, avgrt, boxes, (boxopt == "auto" ? true : false), corbox, correct);
        }

        public void OnDisappearing()
        {
            List<DataSchemas.RTGameRecordSchema> ur = new List<DataSchemas.RTGameRecordSchema>();
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.RTGameRecordSchema>("select * from RTGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                List<double> avgs = new List<double>();
                List<double> bests = new List<double>();
                bests.Add(ur.Where(x => x.boxes == 1 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 1 && x.cor == true).GroupBy(x => (int)Math.Ceiling(x.trialnum / 6.0)).Where(x => x.Count() >= 5).Select(x => x.Sum(y => y.reactiontime) / x.Count()).Min());
                bests.Add(ur.Where(x => x.boxes == 2 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 2 && x.cor == true).GroupBy(x => (int)Math.Ceiling(x.trialnum / 6.0)).Where(x => x.Count() >= 5).Select(x => x.Sum(y => y.reactiontime) / x.Count()).Min());
                bests.Add(ur.Where(x => x.boxes == 4 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 4 && x.cor == true).GroupBy(x => (int)Math.Ceiling(x.trialnum / 6.0)).Where(x => x.Count() >= 5).Select(x => x.Sum(y => y.reactiontime) / x.Count()).Min());
                avgs.Add(ur.Where(x => x.boxes == 1 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 1 && x.cor == true).Sum(y => y.reactiontime) / ur.Where(x => x.boxes == 1 && x.cor == true).Count());
                avgs.Add(ur.Where(x => x.boxes == 2 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 2 && x.cor == true).Sum(y => y.reactiontime) / ur.Where(x => x.boxes == 2 && x.cor == true).Count());
                avgs.Add(ur.Where(x => x.boxes == 4 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 4 && x.cor == true).Sum(y => y.reactiontime) / ur.Where(x => x.boxes == 4 && x.cor == true).Count());
                Thread t = new Thread(() => App.mum.UpdateUserStats("RT", avgs, bests));
                t.Start();
            }
        }
    }
}

