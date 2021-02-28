using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;

using Xamarin.Forms;
using SkiaSharp;

using BrainGames.Utility;
using BrainGames.Models;
using BrainGames.Views;
using BrainGames.Controls;

namespace BrainGames.ViewModels
{
    public class TMDViewModel : ViewModelBase
    {
        private double _itpoints = 0;
        public double itpoints
        {
            get => _itpoints;
            set
            {
                SetProperty(ref _itpoints, value);
                SpeedScore = (itpoints + rtpoints + stpoints) * .667;//speed gets downweighted because there are 3 of them
            }
        }
        private double _rtpoints = 0;
        public double rtpoints
        {
            get => _rtpoints;
            set
            {
                SetProperty(ref _rtpoints, value);
                SpeedScore = (itpoints + rtpoints + stpoints) * .667;//speed gets downweighted because there are 3 of them;
            }
        }
        private double _stpoints = 0;
        public double stpoints
        {
            get => _stpoints;
            set
            {
                SetProperty(ref _stpoints, value);
                SpeedScore = (itpoints + rtpoints + stpoints) * .667;//speed gets downweighted because there are 3 of them;
            }
        }
        private double _dspoints = 0;
        public double dspoints
        {
            get => _dspoints;
            set
            {
                SetProperty(ref _dspoints, value);
                MemoryScore = (dspoints + lspoints);
            }
        }
        private double _lspoints = 0;
        public double lspoints
        {
            get => _lspoints;
            set
            {
                SetProperty(ref _lspoints, value);
                MemoryScore = (dspoints + lspoints);
            }
        }
        private double _memoryScore = 0;
        public double MemoryScore
        {
            get => _memoryScore;
            set
            {
                SetProperty(ref _memoryScore, value);
                MemoryScoreString = string.Format("Memory Score: {0}", Math.Round(_memoryScore, 1));
                if (_memoryScore != 0)
                {
                    MemoryScoreColor = _memoryScore > 0 ? Color.ForestGreen : Color.OrangeRed;
                }
                TotalScore = (_memoryScore + SpeedScore) / 2;
            }
        }
        private double _speedScore = 0;
        public double SpeedScore
        {
            get => _speedScore;
            set
            {
                SetProperty(ref _speedScore, value);
                SpeedScoreString = string.Format("Speed Score: {0}", Math.Round(_speedScore, 1));
                if (_speedScore != 0)
                {
                    SpeedScoreColor = _speedScore > 0 ? Color.ForestGreen : Color.OrangeRed;
                }
                TotalScore = (_speedScore + MemoryScore) / 2;
            }
        }
        private double _totalScore = 0;
        public double TotalScore
        {
            get => _totalScore;
            set
            {
                SetProperty(ref _totalScore, value);
                TotalScoreString = string.Format("Total Score: {0}", Math.Round(_totalScore, 1));
                if (_totalScore != 0)
                {
                    double tmin = -10, tmax = 10;
                    double ts_cln = _totalScore < tmin ? tmin : _totalScore;
                    ts_cln = _totalScore > tmax ? tmax : _totalScore;
                    TotalScoreColor = misc.getgradient(Color.IndianRed, Color.LightGreen, (ts_cln - tmin) / (tmax - tmin));//_totalScore > 0 ? Color.LightGreen : Color.IndianRed;
                }
            }
        }
        private string _memoryscoreString = "";
        public string MemoryScoreString
        {
            get => _memoryscoreString;
            set { SetProperty(ref _memoryscoreString, value); }
        }
        private Color _memoryscoreColor = Color.Transparent;
        public Color MemoryScoreColor
        {
            get => _memoryscoreColor;
            set { SetProperty(ref _memoryscoreColor, value); }
        }
        private string _speedscoreString = "";
        public string SpeedScoreString
        {
            get => _speedscoreString;
            set { SetProperty(ref _speedscoreString, value); }
        }
        private Color _speedscoreColor = Color.Transparent;
        public Color SpeedScoreColor
        {
            get => _speedscoreColor;
            set { SetProperty(ref _speedscoreColor, value); }
        }
        private string _totalscoreString = "";
        public string TotalScoreString
        {
            get => _totalscoreString;
            set { SetProperty(ref _totalscoreString, value); }
        }
        private Color _totalscoreColor = Color.Transparent;
        public Color TotalScoreColor
        {
            get => _totalscoreColor;
            set { SetProperty(ref _totalscoreColor, value); }
        }

        private int _gameidx = 0;
        public int gameidx
        {
            get => _gameidx;
            set
            {
                SetProperty(ref _gameidx, value);
/*                if (gameidx > 0)
                {
                    Thread.Sleep(1000);
                    //                var st = DateTime.Now;
                    //                while (DateTime.Now < st.AddMilliseconds(1000)) {; }
                }*/
                if (gameidx >= orderedgames.Count())
                {
                    StopClock();
                    GameCtrString = "Summary";
                    Game = new TMDSummaryView(this);
                }
                else
                {
                    GamesPlayed.Add(DataSchemas.GameNames[DataSchemas.GameTypes.IndexOf(orderedgames.ElementAt(gameidx))]);
                    GameCtrString = string.Format("{0}: Game {1} of {2}", DataSchemas.GameNames[DataSchemas.GameTypes.IndexOf(orderedgames.ElementAt(gameidx))], gameidx + 1, orderedgames.Count());
                    switch (orderedgames.ElementAt(gameidx))
                    {
                        case "IT":
                            Game = new ITView(this);
                            break;
                        case "RT":
                            Game = new RTView(this);
                            break;
                        case "Stroop":
                            Game = new StroopView(this);
                            break;
                        case "DS":
                            Game = new DSView(this);
                            break;
                        case "LS":
                            Game = new LSView(this);
                            break;
                    }
                }
            }
        }

        private string _gameCtrString = "";
        public string GameCtrString
        {
            get => _gameCtrString;
            set { SetProperty(ref _gameCtrString, value); }
        }

        private Color _respColor = Color.Transparent;
        public Color RespColor
        {
            get => _respColor;
            set { SetProperty(ref _respColor, value); }
        }

        private View _game = null;
        public View Game
        {
            get => _game;
            set { SetProperty(ref _game, value); }
        }

        public List<string> GamesPlayed = new List<string>();

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set { SetProperty(ref _isRunning, value); }
        }

        public bool cor = false;
        private Guid game_session_id;
        private List<string> orderedgames = new List<string>();
        protected DateTime StartTime;

        #region IT
        public Dictionary<double, List<int>> ITscores = new Dictionary<double, List<int>>();
        public ICommand ITReadyButtonCommand { get; protected set; }
        public ICommand ITLeftButtonCommand { get; protected set; }
        public ICommand ITRightButtonCommand { get; protected set; }
        public int ITtrialctr;
        public double ITminstimdur = 1000 / 60;
        public double ITbasestimdur;
        public double ITmaxstimdur;
        public double ITmaskdur = 300;
        public double ITpausedur = 1000;
        public double ITemppausedur;
        public double ITempstimdur;
        public double ITempmaskdur;
        public double ITcurstimdur;
        public bool ITshown = false;
        public enum ITanswertype { left, right }
        public ITanswertype ITcor_ans;
        public List<double> ITempstimtimearr = new List<double>();
        private List<bool> ITcorarr = new List<bool>();
        private List<double> ITstimtimearr = new List<double>();
        private List<int> ITstimtypearr;
        private bool ITfirstrun;
        private bool ITlastchangefaster = true;
        private int ITreversalctr, ITcortrialstreak, ITerrtrialstreak, ITcortrialctr;
        private int ITincthresh = 3;
        private int ITdecthresh = 1;
        private int ITreversalthresh = 8;
        private int ITtrials = 6;
        public Dictionary<double, double> it_AvgCorPctByStimDur;
        public int it_cortrialcnt = 0;
        public double it_cumcorstimdur = 0;
        private bool _isITReadyEnabled = true;
        public bool IsITReadyEnabled
        {
            get => _isITReadyEnabled;
            set { SetProperty(ref _isITReadyEnabled, value); }
        }
        #endregion

        #region RT
        public ICommand RTReadyButtonCommand { get; protected set; }
        public ICommand RTReactButtonCommand { get; protected set; }
        public ICommand RTLeftButtonCommand { get; protected set; }
        public ICommand RTRightButtonCommand { get; protected set; }
        public ICommand RTButton1Command { get; protected set; }
        public ICommand RTButton2Command { get; protected set; }
        public ICommand RTButton3Command { get; protected set; }
        public ICommand RTButton4Command { get; protected set; }
        private int RTpauseminms = 1000;
        private int RTpausemaxms = 3000;
        public int RTtrialsperset = 6;
        public List<int> RTpausedurarr = new List<int>();
        public int RTtimeout = 1000;
        public int RTtrialctr = 0;
        public double RTss1_cumrt = 0;
        public int RTss1_trialcnt;
        public int RTss2_trialcnt;
        public int RTss2_cortrialcnt;
        public int RTss4_trialcnt;
        public int RTss4_cortrialcnt;
        public int RTblocktrialctr = 0;
        public int RTboxes = 0;
        public List<int> RTcorboxes = new List<int>();
        private bool _rTshowReact1 = false;
        public bool RTShowReact1
        {
            get => _rTshowReact1;
            set { SetProperty(ref _rTshowReact1, value); }
        }
        private bool _rTshowReact2 = false;
        public bool RTShowReact2
        {
            get => _rTshowReact2;
            set { SetProperty(ref _rTshowReact2, value); }
        }
        private bool _rTshowReact4 = false;
        public bool RTShowReact4
        {
            get => _rTshowReact4;
            set { SetProperty(ref _rTshowReact4, value); }
        }
        private double _rTreactionTime = 0;
        public double RTReactionTime
        {
            get => _rTreactionTime;
            set { SetProperty(ref _rTreactionTime, value); }
        }
        #endregion

        #region Stroop
        public enum Strooptextcolortypes { red, green, blue, yellow };
        public List<SKColor> Stroopcolortypes = new List<SKColor>() { SKColors.Red, SKColors.Green, SKColors.Blue, SKColors.Yellow };
        public List<string> Stroopcolorwords = new List<string>() { "RED", "GREEN", "BLUE", "YELLOW" };
        public ICommand StroopReadyButtonCommand { get; protected set; }
        public ICommand StroopButton1Command { get; protected set; }
        public ICommand StroopButton2Command { get; protected set; }
        public ICommand StroopButton3Command { get; protected set; }
        public ICommand StroopButton4Command { get; protected set; }

        public int Stroopfixationondurms = 200;
        public int Stroopfixationoffdurms = 100;
        public int Stroopitims = 500;
        public int Strooptrialsperset = 12;
        private int Stroopmaxwcnt = 4;
        private int Stroopminwcnt = 2;
        private int Stroopmaxwccnt = 2;
        private int Stroopminwccnt = 0;
        private int Stroopmaxconcnt = 5;
        private int Stroopminconcnt = 3;

        public int Strooptimeout = 4000;

        public int Strooptrialctr = 0;
        public int Stroopcortrialcnt = 0;
        public int Stroopcontrialcnt = 0;
        public int Stroopcorcontrialcnt = 0;
        public int Stroopincontrialcnt = 0;
        public int Stroopcorincontrialcnt = 0;
        public int Stroopblocktrialctr = 0;
        public double Stroopcumconcorrt = 0;
        public double Stroopcuminconcorrt = 0;

        public int Stroophistcontrialcnt = 0;
        public int Stroophistcorcontrialcnt = 0;
        public int Stroophistincontrialcnt = 0;
        public int Stroophistcorincontrialcnt = 0;
        public double Stroophistcumconcorrt = 0;
        public double Stroophistcuminconcorrt = 0;

        public List<string> Stroopwords = new List<string>();
        public Strooptextcolortypes[] Strooptextcolors;

        private double _stroopConreactionTime = 0;
        public double StroopConReactionTime
        {
            get => _stroopConreactionTime;
            set { SetProperty(ref _stroopConreactionTime, value); }
        }
        private double _stroopInconreactionTime = 0;
        public double StroopInconReactionTime
        {
            get => _stroopInconreactionTime;
            set { SetProperty(ref _stroopInconreactionTime, value); }
        }
        #endregion

        #region DS
        public ICommand DSReadyButtonCommand { get; protected set; }
        public ICommand DSButton0Command { get; protected set; }
        public ICommand DSButton1Command { get; protected set; }
        public ICommand DSButton2Command { get; protected set; }
        public ICommand DSButton3Command { get; protected set; }
        public ICommand DSButton4Command { get; protected set; }
        public ICommand DSButton5Command { get; protected set; }
        public ICommand DSButton6Command { get; protected set; }
        public ICommand DSButton7Command { get; protected set; }
        public ICommand DSButton8Command { get; protected set; }

        public ICommand DSButton9Command { get; protected set; }

        public int DStimeout = 10000;
        public bool DStimedout = false;
        public Stopwatch DStimer = new Stopwatch();

        public Dictionary<double, List<int>> DSscores = new Dictionary<double, List<int>>();
        public Dictionary<int, double> ds_AvgCorPctBySpanLen_f;
        public int DStrialctr = 0;
        public int DSblocktrialctr = 0;
        public bool DSanswered = false;
        private int DStrials = 3;
        private List<int> DSspanlenarr;
        private List<int> DSontimearr;
        private List<int> DSofftimearr;

        private int DSinitspanlen = 6;
        private int DSinitontimems = 500;
        private int DSinitofftimems = 500;
        private int DSmindigit = 0;
        private int DSmaxdigit = 9;
        private int DSincthresh = 4;
        private int DSdecthresh = 2;
        private int DStimeincms = 60;
        private int DStimedecms = 40;
        private int DSmintimems = 30;
        private int DSmaxtimems = 1000;

        private bool DSrepeats_set = false;
        private bool DSrepeats_cons = false;

        private int DScortrialstreak, DSerrtrialstreak;
        private List<Tuple<int, int>> DSlast_ontimes_by_spanlen;
        private List<Tuple<int, int>> DSlast_offtimes_by_spanlen;
        private List<Tuple<int, bool>> DSlast_outcomes_by_spanlen;
        private int DSspanlen_f, DSspanlen_b, DSstimonms_f, DSstimonms_b, DSstimoffms_f, DSstimoffms_b;
        private double DSestspan_f = 0, DSestspan_b = 0;
        private int DScortrialstreak_b, DSerrtrialstreak_b;
        private List<Tuple<int, int>> DSlast_ontimes_by_spanlen_b;
        private List<Tuple<int, int>> DSlast_offtimes_by_spanlen_b;
        private List<Tuple<int, bool>> DSlast_outcomes_by_spanlen_b;

        public List<string> DSdigitlist = new List<string>();
        public List<string> DSresponselist = new List<string>();

        private int _dSstimonms = 0;
        public int DSstimonms
        {
            get => _dSstimonms;
            set { SetProperty(ref _dSstimonms, value); }
        }
        public int _dSstimoffms = 0;
        public int DSstimoffms
        {
            get => _dSstimoffms;
            set { SetProperty(ref _dSstimoffms, value); }
        }

        private int _dSminlen = 2;
        public int DSminlen
        {
            get => _dSminlen;
            set { SetProperty(ref _dSminlen, value); }
        }

        private int _dSmaxlen = 20;
        public int DSmaxlen
        {
            get => _dSmaxlen;
            set { SetProperty(ref _dSmaxlen, value); }
        }

        private int _dSspanlen = 0;
        public int DSspanlen
        {
            get => _dSspanlen;
            set { SetProperty(ref _dSspanlen, value); }
        }

        private bool _dSenableButtons = false;
        public bool DSEnableButtons
        {
            get => _dSenableButtons;
            set { SetProperty(ref _dSenableButtons, value); }
        }

        private bool _dSbackward = false;
        public bool DSBackward
        {
            get => _dSbackward;
            set
            {//save values
                if (_dSbackward)
                {
                    DSspanlen_b = DSspanlen;
                    DSstimonms_b = DSstimonms;
                    DSstimoffms_b = DSstimoffms;
                }
                else
                {
                    DSspanlen_f = DSspanlen;
                    DSstimonms_f = DSstimonms;
                    DSstimoffms_f = DSstimoffms;
                }
                SetProperty(ref _dSbackward, value);
                //restore values
                DSspanlen = _dSbackward ? DSspanlen_b : DSspanlen_f;
                DSstimonms = _dSbackward ? DSstimonms_b : DSstimonms_f;
                DSstimoffms = _dSbackward ? DSstimoffms_b : DSstimoffms_f;
            }
        }
        #endregion

        #region LS
        public ICommand LSReadyButtonCommand { get; protected set; }
        public int LStimeout = 10000;
        public bool LStimedout = false;
        public Stopwatch LStimer = new Stopwatch();

        public Dictionary<double, List<int>> LSscores = new Dictionary<double, List<int>>();
        public Dictionary<int, double> ls_AvgCorPctBySpanLen_f;
        public int LStrialctr = 0;
        public int LSblocktrialctr = 0;
        public bool LSanswered = false;
        private int LStrials = 3;
        private List<int> LSspanlenarr;
        private List<int> LSontimearr;
        private List<int> LSofftimearr;

        private int LSinitspanlen = 4;
        private int LSinitgridsize = 4;
        private int LSinitontimems = 500;
        private int LSinitofftimems = 500;
        private int LSmindigit = 0;
        private int LSmaxdigit = 9;
        private int LSincthresh = 4;
        private int LSdecthresh = 2;
        private int LStimeincms = 60;
        private int LStimedecms = 40;
        private int LSmintimems = 30;
        private int LSmaxtimems = 1000;

        private bool LSrepeats_set = false;
        private bool LSrepeats_cons = false;

        private int LScortrialstreak, LSerrtrialstreak;
        private List<Tuple<int, int>> LSlast_ontimes_by_spanlen;
        private List<Tuple<int, int>> LSlast_offtimes_by_spanlen;
        private List<Tuple<int, bool>> LSlast_outcomes_by_spanlen;
        private int LSspanlen_f, LSspanlen_b, LSstimonms_f, LSstimonms_b, LSstimoffms_f, LSstimoffms_b, LSgridsize_f, LSgridsize_b;
        private double LSestspan_f = 0, LSestspan_b = 0;
        private int LScortrialstreak_b, LSerrtrialstreak_b;
        private List<Tuple<int, int>> LSlast_ontimes_by_spanlen_b;
        private List<Tuple<int, int>> LSlast_offtimes_by_spanlen_b;
        private List<Tuple<int, bool>> LSlast_outcomes_by_spanlen_b;

        public List<string> LSdigitlist = new List<string>();
        public List<string> LSresponselist = new List<string>();

        private Tile[,] _LStiles;
        public int LSgridsize;

        private int _lSstimonms = 0;
        public int LSstimonms
        {
            get => _lSstimonms;
            set { SetProperty(ref _lSstimonms, value); }
        }
        public int _lSstimoffms = 0;
        public int LSstimoffms
        {
            get => _lSstimoffms;
            set { SetProperty(ref _lSstimoffms, value); }
        }

        private int _lSminlen = 2;
        public int LSminlen
        {
            get => _lSminlen;
            set { SetProperty(ref _lSminlen, value); }
        }

        private int _lSmaxlen = 20;
        public int LSmaxlen
        {
            get => _lSmaxlen;
            set { SetProperty(ref _lSmaxlen, value); }
        }

        private int _lSspanlen = 0;
        public int LSspanlen
        {
            get => _lSspanlen;
            set { SetProperty(ref _lSspanlen, value); }
        }

        private bool _lSenableButtons = false;
        public bool LSEnableButtons
        {
            get => _lSenableButtons;
            set { SetProperty(ref _lSenableButtons, value); }
        }

        private double _lSestSpan = 0;
        public double LSEstSpan
        {
            get => _lSestSpan;
            set { SetProperty(ref _lSestSpan, value); }
        }

        private bool _lSbackward = false;
        public bool LSBackward
        {
            get => _lSbackward;
            set
            {//save values
                if (_lSbackward)
                {
                    LSspanlen_b = LSspanlen;
                    LSstimonms_b = LSstimonms;
                    LSstimoffms_b = LSstimoffms;
                }
                else
                {
                    LSspanlen_f = LSspanlen;
                    LSstimonms_f = LSstimonms;
                    LSstimoffms_f = LSstimoffms;
                }
                SetProperty(ref _lSbackward, value);
                //restore values
                LSspanlen = _lSbackward ? LSspanlen_b : LSspanlen_f;
                LSstimonms = _lSbackward ? LSstimonms_b : LSstimonms_f;
                LSstimoffms = _lSbackward ? LSstimoffms_b : LSstimoffms_f;
            }
        }

        #endregion

        public TMDViewModel()
        {
            #region IT
            App.mum.LoadITGR();
            ITReadyButtonCommand = new Command(ITReadyButton);
            ITLeftButtonCommand = new Command(ITLeftButton);
            ITRightButtonCommand = new Command(ITRightButton);

            it_AvgCorPctByStimDur = App.mum.it_AvgCorPctByStimDur;
            it_cumcorstimdur = App.mum.it_cumcorstimdur;
            it_cortrialcnt = App.mum.it_cortrialcnt;
            if (App.mum.it_cortrialcnt == 0)
            {
                ITbasestimdur = ITminstimdur * 2 * Math.Round(100 / (ITminstimdur * 2));
            }
            else
            {
                ITbasestimdur = Math.Floor((App.mum.it_cumcorstimdur / App.mum.it_cortrialcnt) / ITminstimdur) * ITminstimdur;
            }
            ITstimtimearr = new List<double>();
            List<double> doublelisttmp = new List<double>();
            doublelisttmp.Add(ITbasestimdur);
            doublelisttmp.Add(ITbasestimdur);
            ITscores.Add(doublelisttmp[doublelisttmp.Count() - 1], new List<int> { 0, 2 });
            doublelisttmp.Add(ITbasestimdur + ITminstimdur);
            doublelisttmp.Add(ITbasestimdur + ITminstimdur);
            ITscores.Add(doublelisttmp[doublelisttmp.Count() - 1], new List<int> { 0, 2 });
            if (ITbasestimdur > ITminstimdur)
            {
                doublelisttmp.Add(ITbasestimdur - ITminstimdur);
                doublelisttmp.Add(ITbasestimdur - ITminstimdur);
                ITscores.Add(doublelisttmp[doublelisttmp.Count() - 1], new List<int> { 0, 2 });
            }
            else
            {
                doublelisttmp.Add(ITbasestimdur);
                ITscores[doublelisttmp[doublelisttmp.Count() - 1]][1]++;
                doublelisttmp.Add(ITbasestimdur + ITminstimdur);
                ITscores[doublelisttmp[doublelisttmp.Count() - 1]][1]++;
            }
            do
            {
                int i = MasterUtilityModel.RandomNumber(0, doublelisttmp.Count());
                ITstimtimearr.Add(doublelisttmp[i]);
                doublelisttmp.RemoveAt(i);
            } while (doublelisttmp.Count() > 0);

            ITstimtypearr = new List<int>();
            ITcortrialctr = 0;
            ITreversalctr = 0;
            ITmaxstimdur = ITminstimdur * 500 / ITminstimdur;
            ITfirstrun = true;
            #endregion

            #region RT
            App.mum.LoadRTGR();
            RTss1_cumrt = App.mum.rt_ss1_cumrt;
            RTss1_trialcnt = App.mum.rt_ss1_trialcnt;
            RTReadyButtonCommand = new Command(RTReadyButton);
//            RTReactButtonCommand = new Command(RTReactButton);
            RTLeftButtonCommand = new Command(RTLeftButton);
            RTRightButtonCommand = new Command(RTRightButton);
            RTButton1Command = new Command(RTButton1);
            RTButton2Command = new Command(RTButton2);
            RTButton3Command = new Command(RTButton3);
            RTButton4Command = new Command(RTButton4);
            #endregion

            #region Stroop
            App.mum.LoadStroopGR();
            Stroophistcontrialcnt = App.mum.st_contrialcnt;
            Stroophistcorcontrialcnt = App.mum.st_corcontrialcnt;
            Stroophistcumconcorrt = App.mum.st_cumconcorrt;
            Stroophistincontrialcnt = App.mum.st_incontrialcnt;
            Stroophistcorincontrialcnt = App.mum.st_corincontrialcnt;
            Stroophistcuminconcorrt = App.mum.st_cuminconcorrt;
            Strooptextcolors = new Strooptextcolortypes[Strooptrialsperset];
            StroopReadyButtonCommand = new Command(StroopReadyButton);
            StroopButton1Command = new Command(StroopButton1);
            StroopButton2Command = new Command(StroopButton2);
            StroopButton3Command = new Command(StroopButton3);
            StroopButton4Command = new Command(StroopButton4);
            Strooptrialctr = 0;
            Stroopcortrialcnt = 0;

            Stroopcontrialcnt = 0;
            Stroopcorcontrialcnt = 0;
            Stroopincontrialcnt = 0;
            Stroopcorincontrialcnt = 0;
            #endregion

            #region DS
            App.mum.LoadDSGR();
            ds_AvgCorPctBySpanLen_f = App.mum.ds_AvgCorPctBySpanLen_f;
            DSReadyButtonCommand = new Command(DSReadyButton);
            DSButton0Command = new Command(DSButton0);
            DSButton1Command = new Command(DSButton1);
            DSButton2Command = new Command(DSButton2);
            DSButton3Command = new Command(DSButton3);
            DSButton4Command = new Command(DSButton4);
            DSButton5Command = new Command(DSButton5);
            DSButton6Command = new Command(DSButton6);
            DSButton7Command = new Command(DSButton7);
            DSButton8Command = new Command(DSButton8);
            DSButton9Command = new Command(DSButton9);

            if (App.mum.ds_trialctr == 0)
            {
                DStrialctr = 0;
                DScortrialstreak = 0;
                DSerrtrialstreak = 0;
                DSspanlen = DSinitspanlen;
                DSstimonms = DSinitontimems;
                DSstimoffms = DSinitofftimems;
                DSspanlen_f = DSinitspanlen;
                DSstimonms_f = DSinitontimems;
                DSstimoffms_f = DSinitofftimems;
                DSspanlen_b = DSinitspanlen;
                DSstimonms_b = DSinitontimems;
                DSstimoffms_b = DSinitofftimems;
                DSlast_ontimes_by_spanlen = new List<Tuple<int, int>>();
                DSlast_offtimes_by_spanlen = new List<Tuple<int, int>>();
                DSlast_outcomes_by_spanlen = new List<Tuple<int, bool>>();
                DScortrialstreak_b = 0;
                DSerrtrialstreak_b = 0;
                DSlast_ontimes_by_spanlen_b = new List<Tuple<int, int>>();
                DSlast_offtimes_by_spanlen_b = new List<Tuple<int, int>>();
                DSlast_outcomes_by_spanlen_b = new List<Tuple<int, bool>>();
            }
            else
            {
                DStrialctr = 0;// App.mum.ds_trialctr;
                DScortrialstreak = App.mum.ds_cortrialstreak;
                DSerrtrialstreak = App.mum.ds_errtrialstreak;
                DSlast_ontimes_by_spanlen = App.mum.ds_last_ontimes_by_spanlen;
                DSlast_offtimes_by_spanlen = App.mum.ds_last_offtimes_by_spanlen;
                DSlast_outcomes_by_spanlen = App.mum.ds_last_outcomes_by_spanlen;
                DScortrialstreak_b = App.mum.ds_cortrialstreak_b;
                DSerrtrialstreak_b = App.mum.ds_errtrialstreak_b;
                DSlast_ontimes_by_spanlen_b = App.mum.ds_last_ontimes_by_spanlen_b;
                DSlast_offtimes_by_spanlen_b = App.mum.ds_last_offtimes_by_spanlen_b;
                DSlast_outcomes_by_spanlen_b = App.mum.ds_last_outcomes_by_spanlen_b;
                //                DSspanlen_f = App.mum.ds_lastspan == 0 ? DSinitspanlen : App.mum.ds_lastspan;
                //                DSspanlen_b = App.mum.ds_lastspan_b == 0 ? DSinitspanlen : App.mum.ds_lastspan_b;
                DSspanlen_f = App.mum.ds_estspan_f == 0 ? DSinitspanlen : (int)Math.Ceiling(App.mum.ds_estspan_f);
                DSspanlen_b = App.mum.ds_estspan_b == 0 ? DSinitspanlen : (int)Math.Ceiling(App.mum.ds_estspan_b);
                DSstimonms_f = DSlast_ontimes_by_spanlen.Where(x => x.Item1 <= DSspanlen_f).Count() == 0 ? DSinitontimems : DSlast_ontimes_by_spanlen.Where(x => x.Item1 <= DSspanlen_f).OrderByDescending(x => x.Item1).First().Item2;
                DSstimoffms_f = DSlast_offtimes_by_spanlen.Where(x => x.Item1 <= DSspanlen_f).Count() == 0 ? DSinitofftimems : DSlast_offtimes_by_spanlen.Where(x => x.Item1 <= DSspanlen_f).OrderByDescending(x => x.Item1).First().Item2;
                DSstimonms_b = DSlast_ontimes_by_spanlen_b.Where(x => x.Item1 <= DSspanlen_b).Count() == 0 ? DSinitontimems : DSlast_ontimes_by_spanlen_b.Where(x => x.Item1 <= DSspanlen_b).OrderByDescending(x => x.Item1).First().Item2;
                DSstimoffms_b = DSlast_offtimes_by_spanlen_b.Where(x => x.Item1 <= DSspanlen_b).Count() == 0 ? DSinitofftimems : DSlast_offtimes_by_spanlen_b.Where(x => x.Item1 <= DSspanlen_b).OrderByDescending(x => x.Item1).First().Item2;
                DSestspan_f = App.mum.ds_estspan_f;
                DSestspan_b = App.mum.ds_estspan_b;

                //Backward initialized to false by default, so preset these
                DSspanlen = DSspanlen_f;
                DSstimonms = DSstimonms_f;
                DSstimoffms = DSstimoffms_f;
                /*if (true)//App.mum.ds_lastdir == "f")
                {
                    DSBackward = false;
                }
                else
                {
                    DSBackward = true;
                }*/
            }
            DSspanlenarr = new List<int>();
            DSontimearr = new List<int>();
            DSofftimearr = new List<int>();
            List<int> intlisttmp = new List<int>();
            List<int> intlisttmp2 = new List<int>();
            List<int> intlisttmp3 = new List<int>();
            intlisttmp.Add(DSspanlen);
            intlisttmp2.Add(DSstimonms);
            intlisttmp3.Add(DSstimoffms);
            DSscores.Add(intlisttmp[intlisttmp.Count() - 1], new List<int> { 0, 1 });
            if (DSspanlen > DSmindigit + 1)
            {
                intlisttmp.Add(DSspanlen - 1);
                DSscores.Add(intlisttmp[intlisttmp.Count() - 1], new List<int> { 0, 1 });
            }
            else
            {
                intlisttmp.Add(DSspanlen);
                DSscores[intlisttmp[intlisttmp.Count() - 1]][1]++;
            }
            if (DSstimonms >= DSmintimems + DStimedecms)
            {
                intlisttmp2.Add(DSstimonms - DStimedecms);
                intlisttmp3.Add(DSstimoffms);
            }
            else
            {
                intlisttmp2.Add(DSstimonms);
                if (DSstimoffms >= DSmintimems + DStimedecms)
                {
                    intlisttmp3.Add(DSstimoffms - DStimedecms);
                }
                else
                {
                    intlisttmp3.Add(DSstimoffms);
                }
            }
            if (DSspanlen < DSmaxdigit)
            {
                intlisttmp.Add(DSspanlen + 1);
                DSscores.Add(intlisttmp[intlisttmp.Count() - 1], new List<int> { 0, 1 });
            }
            else
            {
                intlisttmp.Add(DSspanlen);
                DSscores[intlisttmp[intlisttmp.Count() - 1]][1]++;
            }
            if (DSstimonms <= DSmaxtimems + DStimeincms)
            {
                intlisttmp2.Add(DSstimonms + DStimeincms);
                intlisttmp3.Add(DSstimoffms);
            }
            else
            {
                intlisttmp2.Add(DSstimonms);
                if (DSstimoffms <= DSmaxtimems - DStimeincms)
                {
                    intlisttmp3.Add(DSstimoffms + DStimeincms);
                }
                else
                {
                    intlisttmp3.Add(DSstimoffms);
                }
            }
            do
            {
                int i = MasterUtilityModel.RandomNumber(0, intlisttmp.Count());
                DSspanlenarr.Add(intlisttmp[i]);
                intlisttmp.RemoveAt(i);
                DSontimearr.Add(intlisttmp2[i]);
                intlisttmp2.RemoveAt(i);
                DSofftimearr.Add(intlisttmp3[i]);
                intlisttmp3.RemoveAt(i);
            } while (intlisttmp.Count() > 0);
            /*
            DSspanlen = DSspanlenarr[DStrialctr];
            DSstimonms = DSontimearr[DStrialctr];
            DSstimoffms = DSofftimearr[DStrialctr];*/
            #endregion

            #region LS
            App.mum.LoadLSGR();
            ls_AvgCorPctBySpanLen_f = App.mum.ls_AvgCorPctBySpanLen_f;
            LSReadyButtonCommand = new Command(LSReadyButton);
            if (App.mum.ls_trialctr == 0)
            {
                LStrialctr = 0;
                LScortrialstreak = 0;
                LSerrtrialstreak = 0;
                LSgridsize = LSinitgridsize;
                LSgridsize_f = LSinitgridsize;
                LSgridsize_b = LSinitgridsize;
                LSspanlen = LSinitspanlen;
                LSstimonms = LSinitontimems;
                LSstimoffms = LSinitofftimems;
                LSspanlen_f = LSinitspanlen;
                LSstimonms_f = LSinitontimems;
                LSstimoffms_f = LSinitofftimems;
                LSspanlen_b = LSinitspanlen;
                LSstimonms_b = LSinitontimems;
                LSstimoffms_b = LSinitofftimems;
                LSlast_ontimes_by_spanlen = new List<Tuple<int, int>>();
                LSlast_offtimes_by_spanlen = new List<Tuple<int, int>>();
                LSlast_outcomes_by_spanlen = new List<Tuple<int, bool>>();
                LScortrialstreak_b = 0;
                LSerrtrialstreak_b = 0;
                LSlast_ontimes_by_spanlen_b = new List<Tuple<int, int>>();
                LSlast_offtimes_by_spanlen_b = new List<Tuple<int, int>>();
                LSlast_outcomes_by_spanlen_b = new List<Tuple<int, bool>>();
            }
            else
            {
                LStrialctr = 0; // App.mum.ls_trialctr;
                LScortrialstreak = App.mum.ls_cortrialstreak;
                LSerrtrialstreak = App.mum.ls_errtrialstreak;
                LSlast_ontimes_by_spanlen = App.mum.ls_last_ontimes_by_spanlen;
                LSlast_offtimes_by_spanlen = App.mum.ls_last_offtimes_by_spanlen;
                LSlast_outcomes_by_spanlen = App.mum.ls_last_outcomes_by_spanlen;
                LScortrialstreak_b = App.mum.ls_cortrialstreak_b;
                LSerrtrialstreak_b = App.mum.ls_errtrialstreak_b;
                LSlast_ontimes_by_spanlen_b = App.mum.ls_last_ontimes_by_spanlen_b;
                LSlast_offtimes_by_spanlen_b = App.mum.ls_last_offtimes_by_spanlen_b;
                LSlast_outcomes_by_spanlen_b = App.mum.ls_last_outcomes_by_spanlen_b;
//                spanlen_f = App.mum.ls_lastspan == 0 ? initspanlen : App.mum.ls_lastspan;
//                spanlen_b = App.mum.ls_lastspan_b == 0 ? initspanlen : App.mum.ls_lastspan_b;
                LSspanlen_f = App.mum.ls_estspan_f == 0 ? LSinitspanlen : (int)Math.Ceiling(App.mum.ls_estspan_f);
                LSspanlen_b = App.mum.ls_estspan_b == 0 ? LSinitspanlen : (int)Math.Ceiling(App.mum.ls_estspan_b);
                LSstimonms_f = LSlast_ontimes_by_spanlen.Where(x => x.Item1 <= LSspanlen_f).Count() == 0 ? LSinitontimems : LSlast_ontimes_by_spanlen.Where(x => x.Item1 <= LSspanlen_f).OrderByDescending(x => x.Item1).First().Item2;
                LSstimoffms_f = LSlast_offtimes_by_spanlen.Where(x => x.Item1 <= LSspanlen_f).Count() == 0 ? LSinitofftimems : LSlast_offtimes_by_spanlen.Where(x => x.Item1 <= LSspanlen_f).OrderByDescending(x => x.Item1).First().Item2;
                LSstimonms_b = LSlast_ontimes_by_spanlen_b.Where(x => x.Item1 <= LSspanlen_b).Count() == 0 ? LSinitontimems : LSlast_ontimes_by_spanlen_b.Where(x => x.Item1 <= LSspanlen_b).OrderByDescending(x => x.Item1).First().Item2;
                LSstimoffms_b = LSlast_offtimes_by_spanlen_b.Where(x => x.Item1 <= LSspanlen_b).Count() == 0 ? LSinitofftimems : LSlast_offtimes_by_spanlen_b.Where(x => x.Item1 <= LSspanlen_b).OrderByDescending(x => x.Item1).First().Item2;
                LSgridsize_f = 4;// App.mum.ls_lastgridsize_f == 0 ? initgridsize : App.mum.ls_lastgridsize_f;
                LSgridsize_b = 4;// App.mum.ls_lastgridsize_b == 0 ? initgridsize : App.mum.ls_lastgridsize_b;

                if (LScortrialstreak == LSincthresh)
                {
                    LSspanlen_f = Math.Max(LSspanlen_f + 1, LSminlen);
                    LScortrialstreak = 0;
                }
                else if (LSerrtrialstreak == LSdecthresh)
                {
                    LSspanlen_f = Math.Min(LSspanlen_f - 1, LSmaxlen);
                    LSerrtrialstreak = 0;
                }
                if (LScortrialstreak_b == LSincthresh)
                {
                    LSspanlen_b = Math.Max(LSspanlen_b + 1, LSminlen);
                    LScortrialstreak_b = 0;
                }
                else if (LSerrtrialstreak_b == LSdecthresh)
                {
                    LSspanlen_b = Math.Min(LSspanlen_b - 1, LSmaxlen);
                    LSerrtrialstreak_b = 0;
                }

                //Backward initialized to false by default, so preset these
                LSspanlen = LSspanlen_f;
                LSstimonms = LSstimonms_f;
                LSstimoffms = LSstimoffms_f;
                LSgridsize = LSgridsize_f;
                LSEstSpan = LSestspan_f;
                /*if (true)//App.mum.ls_lastdir == "f")
                {
                    LSBackward = false;
                }
                else
                {
                    LSBackward = true;
                }*/
            }
            LSmaxdigit = LSgridsize * LSgridsize - 1;
            _LStiles = new Tile[LSgridsize, LSgridsize];

            LSspanlenarr = new List<int>();
            LSontimearr = new List<int>();
            LSofftimearr = new List<int>();
            intlisttmp.Clear();
            intlisttmp2.Clear();
            intlisttmp3.Clear();
            intlisttmp.Add(LSspanlen);
            intlisttmp2.Add(LSstimonms);
            intlisttmp3.Add(LSstimoffms);
            LSscores.Add(intlisttmp[intlisttmp.Count() - 1], new List<int> { 0, 1 });
            if (LSspanlen > LSmindigit + 1)
            {
                intlisttmp.Add(LSspanlen - 1);
                LSscores.Add(intlisttmp[intlisttmp.Count() - 1], new List<int> { 0, 1 });
            }
            else
            {
                intlisttmp.Add(LSspanlen);
                LSscores[intlisttmp[intlisttmp.Count() - 1]][1]++;
            }
            if (LSstimonms >= LSmintimems + LStimedecms)
            {
                intlisttmp2.Add(LSstimonms - LStimedecms);
                intlisttmp3.Add(LSstimoffms);
            }
            else
            {
                intlisttmp2.Add(LSstimonms);
                if (LSstimoffms >= LSmintimems + LStimedecms)
                {
                    intlisttmp3.Add(LSstimoffms - LStimedecms);
                }
                else
                {
                    intlisttmp3.Add(LSstimoffms);
                }
            }
            if (LSspanlen < LSmaxdigit)
            {
                intlisttmp.Add(LSspanlen + 1);
                LSscores.Add(intlisttmp[intlisttmp.Count() - 1], new List<int> { 0, 1 });
            }
            else
            {
                intlisttmp.Add(LSspanlen);
                LSscores[intlisttmp[intlisttmp.Count() - 1]][1]++;
            }
            if (LSstimonms <= LSmaxtimems + LStimeincms)
            {
                intlisttmp2.Add(LSstimonms + LStimeincms);
                intlisttmp3.Add(LSstimoffms);
            }
            else
            {
                intlisttmp2.Add(LSstimonms);
                if (LSstimoffms <= LSmaxtimems - LStimeincms)
                {
                    intlisttmp3.Add(LSstimoffms + LStimeincms);
                }
                else
                {
                    intlisttmp3.Add(LSstimoffms);
                }
            }
            do
            {
                int i = MasterUtilityModel.RandomNumber(0, intlisttmp.Count());
                LSspanlenarr.Add(intlisttmp[i]);
                intlisttmp.RemoveAt(i);
                LSontimearr.Add(intlisttmp2[i]);
                intlisttmp2.RemoveAt(i);
                LSofftimearr.Add(intlisttmp3[i]);
                intlisttmp3.RemoveAt(i);
            } while (intlisttmp.Count() > 0);
/*
            LSspanlen = LSspanlenarr[LStrialctr];
            LSstimonms = LSontimearr[LStrialctr];
            LSstimoffms = LSofftimearr[LStrialctr];*/
            #endregion

            game_session_id = MasterUtilityModel.WriteGameSession("TMD");

            var games = Enumerable.Range(0, DataSchemas.GameTypes.Count()).ToList();
            do
            {
                int i = MasterUtilityModel.RandomNumber(0, games.Count());
                orderedgames.Add(DataSchemas.GameTypes[games[i]]);
                games.RemoveAt(i);

            } while (games.Count() > 0);
            /*
            orderedgames.Clear();
            orderedgames.Add("LS");
            orderedgames.Add("Stroop");
            orderedgames.Add("RT");
            orderedgames.Add("IT");
            orderedgames.Add("DS");
            */
            gameidx = 0;
            RespColor = Color.Transparent;
            StartClock();
        }

        #region TMD Clock
        public void StartClock()
        {
            StartTime = DateTime.Now;
            Device.StartTimer(TimeSpan.FromMilliseconds(100), OnTimerTick);
        }

        private bool StopTimer = false;

        public void StopClock()
        {
            StopTimer = true;
        }

        protected bool OnTimerTick()
        {
            string min = DateTime.Now.Subtract(StartTime).Minutes.ToString();
            string secs = DateTime.Now.Subtract(StartTime).Seconds.ToString();
            ClockTime = (min.Length == 1 ? "0" + min : min) + ":" + (secs.Length == 1 ? "0" + secs : secs);
            if (StopTimer) return false; else return true;
        }

        private string _clockTime = "00:00";
        public string ClockTime
        {
            get => _clockTime;
            set
            {
                if (SetProperty(ref _clockTime, value))
                    OnPropertyChanged("ClockTime");
            }
        }
        #endregion

        public void ResponseButton(int cor, bool last)
        {
            if (cor == 1)
            {
                RespColor = Color.ForestGreen;
            }
            else if (cor == 0)
            {
                RespColor = Color.OrangeRed;
            }
            if (last)
            {/*
                await Task.Run(() =>
                {
                    var st = DateTime.Now;
                    while (DateTime.Now < st.AddMilliseconds(500)) {; }
                });*/

                RespColor = Color.Transparent;
                if (DateTime.Now.Subtract(StartTime).Minutes >= 2)
                {
                    StopClock();
                    gameidx = orderedgames.Count();
                }
                else
                {
                    gameidx++;
                }
            }
        }

        #region LS
        public void AddTile(Tile tile)
        {
            tile.Tapped += TileTapped;
            _LStiles[tile.XPos, tile.YPos] = tile;
        }

        public /*async*/ void FlipTile(string t)
        {
            Console.WriteLine("flipping");
            int ti = Convert.ToInt32(t);
            //            await _tiles[t % gridsize, (int)Math.Floor((double)t / gridsize)].Flip();
            _LStiles[ti % LSgridsize, (int)Math.Floor((double)ti / LSgridsize)].FlipIt();
        }

        private void TileTapped(object sender, TileTappedEventArgs e)
        {
            //            Console.WriteLine("tapped");
            LSresponselist.Add((e.XPos + e.YPos * LSgridsize).ToString());
            LSResponseButton();
        }

        public void LSReadyButton()
        {
            LSblocktrialctr = 0;
            LSanswered = false;
            LStimedout = false;
            LSEnableButtons = false;
            RespColor = Color.Transparent;

            LSdigitlist.Clear();
            LSresponselist.Clear();
            LSmaxdigit = LSgridsize * LSgridsize - 1;
            _LStiles = new Tile[LSgridsize, LSgridsize];

            LSspanlen = LSspanlenarr[LStrialctr];
            LSstimonms = LSontimearr[LStrialctr];
            LSstimoffms = LSofftimearr[LStrialctr];

            for (int i = 0; i < LSspanlen; i++)
            {
                string d;
                if (LSrepeats_set || LSmaxdigit - LSmindigit + 1 < LSspanlen)//circumstances force you to repeat digits
                {
                    LSrepeats_set = true;
                    if (LSrepeats_cons)
                    {
                        d = MasterUtilityModel.RandomNumber(LSmindigit, LSmaxdigit + 1).ToString();
                    }
                    else
                    {
                        do
                        {
                            d = MasterUtilityModel.RandomNumber(LSmindigit, LSmaxdigit + 1).ToString();
                        } while (LSdigitlist.Count() > 0 && LSdigitlist[LSdigitlist.Count - 1] == d);
                    }
                }
                else
                {
                    LSrepeats_set = false;
                    do
                    {
                        d = MasterUtilityModel.RandomNumber(LSmindigit, LSmaxdigit + 1).ToString();
                    } while (LSdigitlist.Contains(d));
                }
                LSdigitlist.Add(d);
            }

            LStimer.Reset();
            IsRunning = true;
        }

        private bool LSMatchLists()
        {
            List<string> testlist = new List<string>(LSdigitlist);
            if (LSBackward)
            {
                testlist.Reverse();
            }
            for (int i = 0; i < LSresponselist.Count() && i < testlist.Count(); i++)
            {
                if (LSresponselist[i] != testlist[i].ToString()) return false;
            }
            return true;
        }

        public void LSResponseButton()
        {
            if (LSresponselist.Count() < LSdigitlist.Count() && LSMatchLists() && !LStimedout) return; //no errors yet, and you haven't timed out
            LSanswered = true;
            IsRunning = false;
            LSEnableButtons = false;
            LStimer.Stop();
            LStrialctr++;

            bool cor;
            if (LSresponselist.Count() == LSdigitlist.Count() && LSMatchLists())
            {
                cor = true;
                if (LSBackward)
                {
                    LScortrialstreak_b++;
                    LSerrtrialstreak_b = 0;
                }
                else
                {
                    LScortrialstreak++;
                    LSerrtrialstreak = 0;
                }
                LSscores[LSspanlenarr[LStrialctr - 1]][0]++;
            }
            else
            {
                cor = false;
                if (LSBackward)
                {
                    LScortrialstreak_b = 0;
                    LSerrtrialstreak_b++;
                }
                else
                {
                    LScortrialstreak = 0;
                    LSerrtrialstreak++;
                }
            }
/*
            if (LSBackward)
            {
                if (LScortrialstreak_b == LSincthresh)
                {
                    LSspanlen = Math.Min(LSspanlen + 1, LSmaxlen);
                    LScortrialstreak_b = 0;
                }
                else if (LSerrtrialstreak_b == LSdecthresh)
                {
                    LSspanlen = Math.Max(LSspanlen - 1, LSminlen);
                    LSerrtrialstreak_b = 0;
                }
                else if (LScortrialstreak_b > 0)
                {
                    if (MasterUtilityModel.RandomNumber(0, 3) <= 1) LSstimonms = Math.Max(LSstimonms - LStimedecms, LSmintimems);
                    else LSstimoffms = Math.Max(LSstimoffms - LStimedecms, LSmintimems);
                }
                else if (LSerrtrialstreak_b > 0)
                {
                    if (MasterUtilityModel.RandomNumber(0, 3) <= 1) LSstimonms = Math.Min(LSstimonms + LStimeincms, LSmaxtimems);
                    else LSstimoffms = Math.Min(LSstimoffms + LStimeincms, LSmaxtimems);
                }
            }
            else
            {
                if (LScortrialstreak == LSincthresh)
                {
                    LSspanlen = Math.Min(LSspanlen + 1, LSmaxlen);
                    LScortrialstreak = 0;
                }
                else if (LSerrtrialstreak == LSdecthresh)
                {
                    LSspanlen = Math.Max(LSspanlen - 1, LSminlen);
                    LSerrtrialstreak = 0;
                }
                else if (LScortrialstreak > 0)
                {
                    if (MasterUtilityModel.RandomNumber(0, 3) <= 1) LSstimonms = Math.Max(LSstimonms - LStimedecms, LSmintimems);
                    else LSstimoffms = Math.Max(LSstimoffms - LStimedecms, LSmintimems);
                }
                else if (LSerrtrialstreak > 0)
                {
                    if (MasterUtilityModel.RandomNumber(0, 3) <= 1) LSstimonms = Math.Min(LSstimonms + LStimeincms, LSmaxtimems);
                    else LSstimoffms = Math.Min(LSstimoffms + LStimeincms, LSmaxtimems);
                }
            }
*/
            ResponseButton(Convert.ToInt32(cor), LStrialctr == LStrials);
            MasterUtilityModel.WriteLSGR(game_session_id, 0, LSspanlen, LSstimonms, LSstimoffms, LSgridsize, (int)LStimer.ElapsedMilliseconds, LSBackward ? "b" : "f", String.Join("~", LSdigitlist), LSrepeats_set, LSrepeats_cons, true, cor);
        }
        #endregion

        #region DS
        public void DSReadyButton()
        {
            DSblocktrialctr = 0;
            DSanswered = false;
            DStimedout = false;
            DSEnableButtons = false;
            RespColor = Color.Transparent;

            DSdigitlist.Clear();
            DSresponselist.Clear();

            DSspanlen = DSspanlenarr[DStrialctr];
            DSstimonms = DSontimearr[DStrialctr];
            DSstimoffms = DSofftimearr[DStrialctr];

            for (int i = 0; i < DSspanlen; i++)
            {
                string d;
                if (DSrepeats_set || DSmaxdigit - DSmindigit + 1 < DSspanlen)//circumstances force you to repeat digits
                {
                    DSrepeats_set = true;
                    if (DSrepeats_cons)
                    {
                        d = MasterUtilityModel.RandomNumber(DSmindigit, DSmaxdigit + 1).ToString();
                    }
                    else
                    {
                        do
                        {
                            d = MasterUtilityModel.RandomNumber(DSmindigit, DSmaxdigit + 1).ToString();
                        } while (DSdigitlist.Count() > 0 && DSdigitlist[DSdigitlist.Count - 1] == d);
                    }
                }
                else
                {
                    DSrepeats_set = false;
                    do
                    {
                        d = MasterUtilityModel.RandomNumber(DSmindigit, DSmaxdigit + 1).ToString();
                    } while (DSdigitlist.Contains(d));
                }
                DSdigitlist.Add(d);
            }

            DStimer.Reset();
            IsRunning = true;
        }

        public void DSButton0() { DSresponselist.Add("0"); DSResponseButton(); }
        public void DSButton1() { DSresponselist.Add("1"); DSResponseButton(); }
        public void DSButton2() { DSresponselist.Add("2"); DSResponseButton(); }
        public void DSButton3() { DSresponselist.Add("3"); DSResponseButton(); }
        public void DSButton4() { DSresponselist.Add("4"); DSResponseButton(); }
        public void DSButton5() { DSresponselist.Add("5"); DSResponseButton(); }
        public void DSButton6() { DSresponselist.Add("6"); DSResponseButton(); }
        public void DSButton7() { DSresponselist.Add("7"); DSResponseButton(); }
        public void DSButton8() { DSresponselist.Add("8"); DSResponseButton(); }
        public void DSButton9() { DSresponselist.Add("9"); DSResponseButton(); }

        private bool DSMatchLists()
        {
            List<string> testlist = new List<string>(DSdigitlist);
            if (DSBackward)
            {
                testlist.Reverse();
            }
            for (int i = 0; i < DSresponselist.Count() && i < testlist.Count(); i++)
            {
                if (DSresponselist[i] != testlist[i]) return false;
            }
            return true;
        }

        public void DSResponseButton()
        {
            if (DSresponselist.Count() < DSdigitlist.Count() && DSMatchLists() && !DStimedout) return; //no errors yet, and you haven't timed out
            DSanswered = true;
            IsRunning = false;
            DSEnableButtons = false;
            DStimer.Stop();
            DStrialctr++;

            if (DSresponselist.Count() == DSdigitlist.Count() && DSMatchLists())
            {
                cor = true;
                DSscores[DSspanlenarr[DStrialctr - 1]][0]++;
            }
            else
            {
                cor = false;
            }
/*
            if (DSBackward)
            {
                if (DScortrialstreak_b == DSincthresh)
                {
                    DSspanlen = Math.Min(DSspanlen + 1, DSmaxlen);
                    DScortrialstreak_b = 0;
                }
                else if (DSerrtrialstreak_b == DSdecthresh)
                {
                    DSspanlen = Math.Max(DSspanlen - 1, DSminlen);
                    DSerrtrialstreak_b = 0;
                }
                else if (DScortrialstreak_b > 0)
                {
                    if (MasterUtilityModel.RandomNumber(0, 3) <= 1) DSstimonms = Math.Max(DSstimonms - DStimedecms, DSmintimems);
                    else DSstimoffms = Math.Max(DSstimoffms - DStimedecms, DSmintimems);
                }
                else if (DSerrtrialstreak_b > 0)
                {
                    if (MasterUtilityModel.RandomNumber(0, 3) <= 1) DSstimonms = Math.Min(DSstimonms + DStimeincms, DSmaxtimems);
                    else DSstimoffms = Math.Min(DSstimoffms + DStimeincms, DSmaxtimems);
                }
            }
            else
            {
                if (DScortrialstreak == DSincthresh)
                {
                    DSspanlen = Math.Min(DSspanlen + 1, DSmaxlen);
                    DScortrialstreak = 0;
                }
                else if (DSerrtrialstreak == DSdecthresh)
                {
                    DSspanlen = Math.Max(DSspanlen - 1, DSminlen);
                    DSerrtrialstreak = 0;
                }
                else if (DScortrialstreak > 0)
                {
                    if (DSstimonms == DSmintimems) DSstimoffms = Math.Max(DSstimoffms - DStimedecms, DSmintimems);
                    else if (DSstimoffms == DSmintimems) DSstimonms = Math.Max(DSstimonms - DStimedecms, DSmintimems);
                    else if (MasterUtilityModel.RandomNumber(0, 3) <= 1) DSstimonms = Math.Max(DSstimonms - DStimedecms, DSmintimems);
                    else DSstimoffms = Math.Max(DSstimoffms - DStimedecms, DSmintimems);
                }
                else if (DSerrtrialstreak > 0)
                {
                    if (DSstimonms == DSmaxtimems) DSstimoffms = Math.Min(DSstimoffms + DStimeincms, DSmaxtimems);
                    else if (DSstimoffms == DSmaxtimems) DSstimonms = Math.Min(DSstimonms + DStimeincms, DSmaxtimems);
                    else if (MasterUtilityModel.RandomNumber(0, 3) <= 1) DSstimonms = Math.Min(DSstimonms + DStimeincms, DSmaxtimems);
                    else DSstimoffms = Math.Min(DSstimoffms + DStimeincms, DSmaxtimems);
                }
            }
*/
            ResponseButton(Convert.ToInt32(cor), DStrialctr == DStrials);
            MasterUtilityModel.WriteDSGR(game_session_id, 0, DSspanlen, DSstimonms, DSstimoffms, (int)DStimer.ElapsedMilliseconds, DSBackward ? "b" : "f", String.Join("~", DSdigitlist), DSrepeats_set, DSrepeats_cons, true, cor);
        }

        #endregion

        #region Stroop
        public void StroopButton1()
        {
            if (Strooptextcolors[Stroopblocktrialctr] == Strooptextcolortypes.red)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void StroopButton2()
        {
            if (Strooptextcolors[Stroopblocktrialctr] == Strooptextcolortypes.green)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void StroopButton3()
        {
            if (Strooptextcolors[Stroopblocktrialctr] == Strooptextcolortypes.blue)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void StroopButton4()
        {
            if (Strooptextcolors[Stroopblocktrialctr] == Strooptextcolortypes.yellow)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }

        public void StroopReadyButton()
        {
            Stroopblocktrialctr = 0;

            #region load words and colors
            List<string> cwords = new List<string>(Stroopcolorwords);

            //choose how many reps each word gets
            int w1 = MasterUtilityModel.RandomNumber(Stroopminwcnt, Stroopmaxwcnt + 1);
            int w2 = MasterUtilityModel.RandomNumber(Stroopminwcnt, Stroopmaxwcnt + 1);
            int w3 = MasterUtilityModel.RandomNumber(Math.Max(Stroopminwcnt, Strooptrialsperset - (w1 + w2) - Stroopmaxwcnt), Math.Min(Stroopmaxwcnt + 1, Strooptrialsperset - (w1 + w2) - Stroopminwcnt) + 1);
            int w4 = Strooptrialsperset - (w1 + w2 + w3);
            List<int> wcnts = new List<int>() { w1, w2, w3, w4 };
            ///////////////////////////////////

            //choose how many congruent trials each word gets
            List<int> ctcnts = new List<int>();
            int totalconcnt = 0;
            while (totalconcnt < Stroopminconcnt || totalconcnt > Stroopmaxconcnt)
            {
                totalconcnt = 0;
                ctcnts.Clear();
                for (int i = 0; i < 4; i++)
                {
                    int cc = MasterUtilityModel.RandomNumber(Stroopminwccnt, Stroopmaxwccnt + 1);
                    ctcnts.Add(cc);
                    totalconcnt += cc;
                }
            }

            List<int> wctnts = new List<int>();
            int cv;
            for (int i = 0; i < wcnts.Count(); i++)
            {
                if (wcnts[i] < Stroopmaxwccnt + 1)//take care of low-rep words first
                {
                    if (ctcnts.Where(x => x < Stroopmaxwccnt).Count() > 0)
                    {
                        var l = ctcnts.Where(x => x < Stroopmaxwccnt).ToList();
                        l.Shuffle();
                        cv = l[0];
                        wctnts.Add(cv);
                        ctcnts.Remove(cv);
                    }
                }
            }
            for (int i = 0; i < wcnts.Count(); i++)
            {
                if (wcnts[i] >= Stroopmaxwccnt + 1)
                {
                    if (ctcnts.Where(x => x == Stroopmaxwccnt).Count() > 0)
                    {
                        cv = ctcnts.Where(x => x == Stroopmaxwccnt).ToList()[0];
                        wctnts.Add(cv);
                        ctcnts.Remove(cv);
                    }
                    else
                    {
                        var l = ctcnts.Where(x => x < Stroopmaxwccnt).ToList();
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
            Stroopwords.Clear();
            for (int i = 0; i < wcnts.Count(); i++)
            {
                for (int j = 0; j < wcnts[i]; j++)
                {
                    Stroopwords.Add(wms[i]);
                }
            }
            Stroopwords.Shuffle();
            ///////////////////////////////////////////

            //load up colors
            for (int i = 0; i < wms.Count(); i++)
            {
                //get all indexes for that word
                var idxs = Stroopwords.Select((v, li) => new { Index = li, Value = v })     // Pair up values and indexes
                                            .Where(p => p.Value == wms[i])   // Do the filtering
                                        .Select(p => p.Index).ToList(); // Keep the index and drop the value
                //take care of congruent trials first
                for (int j = 0; j < wctnts[i]; j++)
                {
                    int ridx = idxs[MasterUtilityModel.RandomNumber(0, idxs.Count)];
                    Strooptextcolors[ridx] = (Strooptextcolortypes)Stroopcolorwords.IndexOf(wms[i]);
                    idxs.Remove(ridx);
                }
                //now do incongruent
                for (int j = 0; j < wcnts[i] - wctnts[i]; j++)
                {
                    int ridx = idxs[MasterUtilityModel.RandomNumber(0, idxs.Count)];
                    int cidx;
                    do
                    {
                        cidx = MasterUtilityModel.RandomNumber(0, Stroopcolorwords.Count());
                    } while (cidx == Stroopcolorwords.IndexOf(wms[i]));
                    Strooptextcolors[ridx] = (Strooptextcolortypes)cidx;
                    idxs.Remove(ridx);
                }

            }
            //////////////////////////////////////////
            #endregion
            cor = false;//set this for non-responses
            IsRunning = true;
        }

        public void StroopReactButton(int tctr, double rt, double avgrt, double difrt, string word, string textcolor, bool congruent, bool correct)
        {
            ResponseButton(Convert.ToInt32(cor), Stroopblocktrialctr == Strooptrialsperset);
            MasterUtilityModel.WriteStroopGR(game_session_id, tctr, rt, avgrt, difrt, word, textcolor, congruent, correct);
        }
        #endregion

        #region RT
        public void RTReactButton(int tctr, double rt, double avgrt, int corbox, bool correct)
        {
            MasterUtilityModel.WriteRTGR(game_session_id, tctr, rt, avgrt, RTboxes, true, corbox, correct);
            ResponseButton(RTboxes == 1 ? -1 : Convert.ToInt32(cor), RTblocktrialctr == RTtrialsperset);
        }

        public void RTLeftButton()
        {
            if (RTcorboxes[RTblocktrialctr] == 0)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }

        public void RTRightButton()
        {
            if (RTcorboxes[RTblocktrialctr] == 1)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }

        public void RTButton1()
        {
            if (RTcorboxes[RTblocktrialctr] == 0)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void RTButton2()
        {
            if (RTcorboxes[RTblocktrialctr] == 1)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void RTButton3()
        {
            if (RTcorboxes[RTblocktrialctr] == 2)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }
        public void RTButton4()
        {
            if (RTcorboxes[RTblocktrialctr] == 3)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
        }

        public void RTReadyButton()
        {
            cor = RTboxes == 1 ? true : false;//default to this in the 1-box case
            RTblocktrialctr = 0;
            RTboxes = 1;// (int)Math.Pow(2, MasterUtilityModel.RandomNumber(0, 3));
            if (RTboxes == 1)
            {
                RTShowReact1 = true;
                RTShowReact2 = false;
                RTShowReact4 = false;
                RTtimeout = 1000;
            }
            else if (RTboxes == 2)
            {
                RTShowReact1 = false;
                RTShowReact2 = true;
                RTShowReact4 = false;
                RTtimeout = 1500;
            }
            else
            {
                RTShowReact1 = false;
                RTShowReact2 = false;
                RTShowReact4 = true;
                RTtimeout = 2000;
            }
            RTpausedurarr.Clear();
            RTcorboxes.Clear();
            for (int i = 0; i < RTtrialsperset; i++)
            {
                RTpausedurarr.Add(MasterUtilityModel.RandomNumber(RTpauseminms, RTpausemaxms + 1));
                RTcorboxes.Add(MasterUtilityModel.RandomNumber(0, RTboxes));
            }
            IsRunning = true;
            RespColor = Color.Transparent;
        }
        #endregion

        #region IT
        public void ITReadyButton()
        {
            ITshown = false;

            ITcurstimdur = ITstimtimearr[ITtrialctr];

            ITemppausedur = 0;
            ITempstimdur = 0;
            ITempmaskdur = 0;

            if (MasterUtilityModel.RandomNumber(0, 2) == 1)
            {
                ITcor_ans = ITanswertype.left;
                ITstimtypearr.Add((int)ITViewModel.answertype.left);
            }
            else
            {
                ITcor_ans = ITanswertype.right;
                ITstimtypearr.Add((int)ITViewModel.answertype.right);
            }

            ITtrialctr++;
            IsRunning = true;
            RespColor = Color.Transparent;
        }

        public void ITLeftButton()
        {
            if (!ITshown) return;
            if (ITcor_ans == ITanswertype.left)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
            ITResponseButton();
        }

        public void ITRightButton()
        {
            if (!ITshown) return;
            if (ITcor_ans == ITanswertype.right)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
            ITResponseButton();
        }

        public void ITResponseButton()
        {
            if (cor)
            {
                ITcortrialstreak++;
                ITerrtrialstreak = 0;
                ITcortrialctr++;
                ITscores[ITstimtimearr[ITtrialctr - 1]][0]++;
                it_cortrialcnt++;
                it_cumcorstimdur += ITempstimdur;
            }
            IsRunning = false;
            if (ITtrialctr == ITtrials) IsITReadyEnabled = false;
            ResponseButton(Convert.ToInt32(cor), ITtrialctr == ITtrials);
            MasterUtilityModel.WriteITGR(game_session_id, 0, 0, ITstimtimearr[ITtrialctr - 1], ITempstimdur, it_cumcorstimdur / it_cortrialcnt, 0, (int)ITcor_ans, cor);
        }
        #endregion
    }
}
