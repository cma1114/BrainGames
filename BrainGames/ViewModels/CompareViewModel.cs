using System;
using System.Collections.Generic;
using System.Text;
using Microcharts;
using SkiaSharp;
using System.Linq;
using BrainGames.Utility;
using BrainGames.Models;
using Xamarin.Forms;

namespace BrainGames.ViewModels
{
    public class CompareViewModel : ViewModelBase
    {
        private List<DataSchemas.UserStatsSchema> ur = new List<DataSchemas.UserStatsSchema>();
        public Dictionary<string, double> ThisUserStats = new Dictionary<string, double>();
        public Dictionary<string, double> OtherUserStats = new Dictionary<string, double>();
        public Dictionary<string, double> OtherUserSDs = new Dictionary<string, double>();

        public string MapGameKey(string k)
        {
            switch (k)
            {
                case "IT":
                    return "Inspection Time";
                case "RT1":
                case "RT2":
                case "RT3":
                    return "Reaction Time";
                case "Stroop1":
                case "Stroop2":
                    return "Stroop Effect";
                case "DS1":
                case "DS2":
                    return "Digit Span";
                case "LS1":
                case "LS2":
                    return "Location Span";
                default: return "Other";
            }
        }

        public CompareViewModel()
        {
            List<string> scores = new List<string>();
            double d;
            OtherUserStats = MasterUtilityModel.OtherUserStats;
            OtherUserSDs = MasterUtilityModel.OtherUserSDs;
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.UserStatsSchema>("select * from UserStatsSchema"); }
            catch (Exception ex) {; }
            foreach (DataSchemas.UserStatsSchema us in ur)
            {
                switch (us.game)
                {
                    case "IT":
                        scores = us.avgs.Split('~').ToList();
                        d = Convert.ToDouble(scores[0]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("IT", d);
                        }
                        break;
                    case "RT":
                        scores = us.avgs.Split('~').ToList();
                        d = Convert.ToDouble(scores[0]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("RT1", d);
                        }
                        d = Convert.ToDouble(scores[1]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("RT2", d);
                        }
                        d = Convert.ToDouble(scores[2]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("RT3", d);
                        }
                        break;
                    case "Stroop":
                        scores = us.avgs.Split('~').ToList();
                        d = Convert.ToDouble(scores[0]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("Stroop1", d);
                            ThisUserStats.Add("Stroop2", Convert.ToDouble(scores[1]));
                        }
                        break;
                    case "DS":
                        scores = us.bests.Split('~').ToList();
                        d = Convert.ToDouble(scores[0]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("DS1", d);
                        }
                        d = Convert.ToDouble(scores[1]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("DS2", d);
                        }
                        break;
                    case "LS":
                        scores = us.bests.Split('~').ToList();
                        d = Convert.ToDouble(scores[0]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("LS1", d);
                        }
                        d = Convert.ToDouble(scores[1]);
                        if (d > 0 && d < 9999)
                        {
                            ThisUserStats.Add("LS2", d);
                        }
                        break;
                }
            }
        }


        private List<ChartEntry> GetCompetitors(string game, string suffix)
        {

            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Red };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            ChartEntry e;

            e = new ChartEntry((float)ThisUserStats[game]);
            e.ValueLabel = Math.Round(ThisUserStats[game], 1).ToString() + " " + suffix;
            e.Label = "Me";
            e.Color = clrs[0];
            es.Add(e);

            e = new ChartEntry((float)OtherUserStats[game]);
            e.ValueLabel = Math.Round(OtherUserStats[game], 1).ToString() + " " + suffix;
            e.Label = "World Average";
            e.Color = clrs[idx++];
            es.Add(e);

            return es;
        }

        public Chart IT_AvgCorITChart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("IT", "ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart RT_Average1Chart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("RT1", "ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart RT_Average2Chart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("RT2", "ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart RT_Average4Chart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("RT3", "ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart Stroop_AvgCorRTChart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("Stroop1", "ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart Stroop_AvgCIDifChart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("Stroop2", "ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart DS_LongestFChart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("DS1", "digits"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart DS_LongestBChart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("DS2", "digits"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart LS_LongestFChart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("LS1", "locations"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };

        public Chart LS_LongestBChart => new BarChart()
        {
            Margin = 20,
            BackgroundColor = SKColors.Transparent,
            Entries = GetCompetitors("LS2", "locations"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelColor = SKColors.Black,
            LabelTextSize = (float)Device.GetNamedSize(NamedSize.Large, typeof(Label))
        };
    }
}
