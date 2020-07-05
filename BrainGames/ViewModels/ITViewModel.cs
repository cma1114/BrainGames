using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

using BrainGames.Utility;
using BrainGames.Models;
using BrainGames.Views;
using BrainGames.Converters;

namespace BrainGames.ViewModels
{
    public class ITViewModel : ViewModelBase
    {
//        public ObservableCollection<Item> Items { get; set; }
//        public Command LoadItemsCommand { get; set; }
        public ICommand ReadyButtonCommand { get; protected set; }
        public ICommand LeftButtonCommand { get; protected set; }
        public ICommand RightButtonCommand { get; protected set; }
        public enum answertype { left, right }

        protected Guid game_session_id;

        public int incthresh = 3;
        public int decthresh = 1;
//        public int maxtriallen = 10;
        public int reversalthresh = 8;
        public List<bool> corarr;
        public List<double> stimtimearr;
        public List<double> empstimtimearr;
        public List<int> stimtypearr;
        public bool firstrun;
        public int trialctr;
        public double minstimdur = 1000 / 60;
        public double initstimdur;
        public double maxstimdur;
        public double maskdur = 300;
        public double pausedur = 1000;
        public double emppausedur;
        public double empstimdur;
        public double empmaskdur;
        public double curstimdur;
        public double cumcorstimdur;
        public int cortrialctr;
        public string stimdurtext;
        public answertype cor_ans;
        public bool shown = false;
        bool cor = false;
        bool lastchangefaster = true;
        double estit = 0;

        int reversalctr, cortrialstreak, errtrialstreak;

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set { SetProperty(ref _isRunning, value); }
/*            set
            {
                if (SetProperty(ref _isRunning, value))
                    OnPropertyChanged("IsRunning");
            }*/
        }

        public ITViewModel()
        {
            ReadyButtonCommand = new Command(ReadyButton);
            LeftButtonCommand = new Command(LeftButton);
            RightButtonCommand = new Command(RightButton);

            if (App.mum.it_corarr is null)
            {
                corarr = new List<bool>();
                empstimtimearr = new List<double>();
                trialctr = 0;
                reversalctr = 0;
                cortrialstreak = 0;
                errtrialstreak = 0;
                initstimdur = minstimdur * /*2*/ 3 * Math.Round(100 / (minstimdur * 2));
            }
            else
            {
                corarr = App.mum.it_corarr;
                empstimtimearr = App.mum.it_empstimtimearr;
                trialctr = App.mum.it_trialctr;
                reversalctr = App.mum.it_reversalctr;
                cortrialstreak = App.mum.it_cortrialstreak;
                errtrialstreak = App.mum.it_errtrialstreak;
                initstimdur = App.mum.it_laststimtime;
                if (corarr[corarr.Count() - 1] == false) lastchangefaster = false;
            }
            stimtimearr = new List<double>();
            stimtypearr = new List<int>();

            if (Settings.IT_AvgCorDur > 0)
            {
                cumcorstimdur = Settings.IT_AvgCorDur * Settings.IT_CorTrials;
                cortrialctr = Settings.IT_CorTrials;
            }
            else
            {
                cumcorstimdur = 0;
                cortrialctr = 0;
            }

            if (Settings.IT_EstIT > 0)
            {
                estit = Settings.IT_EstIT;
            }

            maxstimdur = minstimdur * 500 / minstimdur;
            firstrun = true;

            game_session_id = MasterUtilityModel.WriteGameSession("IT");

        /*
                    Title = "Browse";
                    Items = new ObservableCollection<Item>();
                    LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
                    MessagingCenter.Subscribe<NewItemPage, Item>(this, "AddItem", async (obj, item) =>
                    {
                        var newItem = item as Item;
                        Items.Add(newItem);
                        await DataStore.AddItemAsync(newItem);
                    });*/
    }

        public void LeftButton()
        {
            if (!shown) return;
            if (cor_ans == answertype.left)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
            ResponseButton();
        }

        public void RightButton()
        {
            if (!shown) return;
            if (cor_ans == answertype.right)
            {
                cor = true;
            }
            else
            {
                cor = false;
            }
            ResponseButton();
        }

        public void ResponseButton()
        {
            IsRunning = false;
            if (cor)
            {
                corarr.Add(true);
                cortrialstreak++;
                errtrialstreak = 0;
                cortrialctr++;
                cumcorstimdur += empstimdur;
                Settings.IT_CorTrials++;
                Settings.IT_AvgCorDur = (cumcorstimdur / cortrialctr);
            }
            else
            {
                corarr.Add(false);
                cortrialstreak = 0;
                errtrialstreak++;
            }
//            Console.WriteLine("Trialctr: {0}, empstimtimearr[i]: {1}, stimtimearr: {2}", trialctr, empstimtimearr[trialctr - 1], stimtimearr[trialctr - 1]);
//            Console.WriteLine("corrarr[i]: {0}, stimtypearr[i]: {1}", corarr.Select(Convert.ToDouble).ToArray()[trialctr - 1], stimtypearr[trialctr - 1]);
            if (reversalctr >= reversalthresh)
            {
                var llsi = new LinearLeastSquaresInterpolation(empstimtimearr, corarr.Select(Convert.ToDouble));
                Settings.IT_EstIT = (0.9 - llsi.Intercept) / llsi.Slope;
                estit = Settings.IT_EstIT;
            }
            MasterUtilityModel.WriteITGR(game_session_id, trialctr, reversalctr, curstimdur, empstimdur, Settings.IT_AvgCorDur, Settings.IT_EstIT, (int)cor_ans, cor);
        }

        public void ReadyButton()
        {
            shown = false;

            if (firstrun) 
            { 
                curstimdur = initstimdur; 
                firstrun = false; 
            }
/*            else */if (cortrialstreak >= incthresh) 
            { 
                curstimdur = Math.Max(curstimdur - minstimdur, minstimdur); 
                cortrialstreak = 0; 
                if(!lastchangefaster) reversalctr++; 
                lastchangefaster = true; 
            }
            else if (errtrialstreak >= decthresh) 
            { 
                curstimdur = Math.Min(curstimdur + minstimdur, maxstimdur); 
                errtrialstreak = 0; 
                if (lastchangefaster) reversalctr++; 
                lastchangefaster = false; 
            }

            Settings.IT_LastStimDur = curstimdur;
            emppausedur = 0;
            empstimdur = 0;
            empmaskdur = 0;

            if (MasterUtilityModel.RandomNumber(0, 2) == 1)
            {
                cor_ans = answertype.left;
                stimtypearr.Add((int)ITViewModel.answertype.left);
            }
            else
            {
                cor_ans = answertype.right;
                stimtypearr.Add((int)ITViewModel.answertype.right);
            }

            trialctr++;
            IsRunning = true;
        }

        /*        async Task ExecuteLoadItemsCommand()
                {
                    if (IsBusy)
                        return;

                    IsBusy = true;

                    try
                    {
                        Items.Clear();
                        var items = await DataStore.GetItemsAsync(true);
                        foreach (var item in items)
                        {
                            Items.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }*/
    }
}