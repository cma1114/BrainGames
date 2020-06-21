using System;
using System.Collections.Generic;
using System.Text;
using Microcharts;
using SkiaSharp;
using System.Linq;
using BrainGames.Utility;
using BrainGames.Models;
using BrainGames.ViewModels;

namespace BrainGames.ViewModels
{
    public class RTStatsViewModel : ViewModelBase
    {

        private List<DataSchemas.RTGameRecordSchema> ur = new List<DataSchemas.RTGameRecordSchema>();

        private IEnumerable<Tuple<DateTime, double>> TrialsByDay;
        private IEnumerable<Tuple<DateTime, double>> TrialsByWeek;
        private IEnumerable<Tuple<DateTime, double>> TrialsByMonth;
        private IEnumerable<Tuple<int, double>> AvgCorRTByStimType;
        private IEnumerable<Tuple<int, double>> AvgCorPctByStimType;
        private IEnumerable<Tuple<int, double>> AvgCorRTByStimLoc2;
        private IEnumerable<Tuple<int, double>> AvgCorPctByStimLoc2;
        private IEnumerable<Tuple<int, double>> AvgCorRTByStimLoc4;
        private IEnumerable<Tuple<int, double>> AvgCorPctByStimLoc4;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByDay;
        private IEnumerable<Tuple<DateTime, double>> CumAvgCorRTByDay;
        private IEnumerable<double> CumAvgCorRTByTrial;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByWeek;
        private IEnumerable<Tuple<DateTime, double>> CumAvgCorRTByWeek;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByMonth;
        private IEnumerable<Tuple<DateTime, double>> CumAvgCorRTByMonth;
        private List<Tuple<DateTime, double>> TrialCountOverTime;
        private List<Tuple<DateTime, double>> AvgCorRTOverTime;
        private List<Tuple<DateTime, double>> CumAvgCorRTOverTime;

        List<Tuple<DateTime, double>> FillTimeList(IEnumerable<Tuple<DateTime, double>> daylist, IEnumerable<Tuple<DateTime, double>> weeklist, IEnumerable<Tuple<DateTime, double>> monthlist)
        {
            IEnumerable<Tuple<DateTime, double>> t;
            List<Tuple<DateTime, double>> v = new List<Tuple<DateTime, double>>();
            DateTime doi;
            timeblock b;
            int historicrange = daylist.Count() > 0 ? DateTime.Now.Subtract(daylist.First().Item1).Days : 0;
            if (historicrange < 7) historicrange = 7;
            if (historicrange > 30)
            {
                historicrange = weeklist.Count() > 0 ? DateTime.Now.Subtract(weeklist.First().Item1).Days : 0;
                if (historicrange < 7) historicrange = 7;
                if (historicrange > 30)
                {
                    historicrange = 30;
                    b = timeblock.Month;
                }
                else
                {
                    b = timeblock.Week;
                }
            }
            else
            {
                b = timeblock.Day;
            }
            for (int i = 1; i <= historicrange; i++)
            {
                doi = DateTime.Now.AddDays(i - historicrange).Date;
                if (b == timeblock.Day) { t = daylist.Where(x => x.Item1 == doi); }
                else if (b == timeblock.Week) { t = weeklist.Where(x => x.Item1 == doi); }
                else { t = monthlist.Where(x => x.Item1 == doi); }
                if (t.Count() == 0)
                {
                    v.Add(Tuple.Create(doi, 0.0));
                }
                else
                {
                    v.Add(t.First());
                }
            }
            return v;
        }

        public RTStatsViewModel()
        {
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.RTGameRecordSchema>("select * from RTGameRecordSchema"); }
            catch {; }
            if (ur != null && ur.Count() > 0)
            {
                TrialsByDay = ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                TrialsByWeek = ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                TrialsByMonth = ur.GroupBy(x => DateTime.Parse(new { DateTime.Parse(x.datetime).Month, DateTime.Parse(x.datetime).Year }.ToString())).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                AvgCorRTByDay = ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                AvgCorRTByStimType = ur.GroupBy(x => x.boxes).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                AvgCorPctByStimType = ur.GroupBy(x => x.boxes).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Where(y => y.cor == true || y.cor == false).Count())).OrderBy(x => x.Item1);
                AvgCorRTByStimLoc2 = ur.Where(x => x.boxes == 2).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                AvgCorPctByStimLoc2 = ur.Where(x => x.boxes == 2).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Where(y => y.cor == true || y.cor == false).Count())).OrderBy(x => x.Item1);
                AvgCorRTByStimLoc4 = ur.Where(x => x.boxes == 4).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                AvgCorPctByStimLoc4 = ur.Where(x => x.boxes == 4).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Where(y => y.cor == true || y.cor == false).Count())).OrderBy(x => x.Item1);
                CumAvgCorRTByDay = ur.Where(x => x.avgrt > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1);
                CumAvgCorRTByTrial = ur.Where(x => x.avgrt > 0).OrderBy(x => x.datetime).Select(x => x.avgrt).Take(30);
                AvgCorRTByWeek = ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                CumAvgCorRTByWeek = ur.Where(x => x.avgrt > 0).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1);
                AvgCorRTByMonth = ur.GroupBy(x => DateTime.Parse(new { DateTime.Parse(x.datetime).Month, DateTime.Parse(x.datetime).Year }.ToString())).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                CumAvgCorRTByMonth = ur.Where(x => x.avgrt > 0).GroupBy(x => DateTime.Parse(new { DateTime.Parse(x.datetime).Month, DateTime.Parse(x.datetime).Year }.ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1);

                AvgCorRTOverTime = FillTimeList(AvgCorRTByDay, AvgCorRTByWeek, AvgCorRTByMonth);
                CumAvgCorRTOverTime = FillTimeList(CumAvgCorRTByDay, CumAvgCorRTByWeek, CumAvgCorRTByMonth);
                TrialCountOverTime = FillTimeList(TrialsByDay, TrialsByWeek, TrialsByMonth);
            }
        }

        private Entry CreateDayEntryMS(Tuple<DateTime, double> dailyitem)
        {
            var label = dailyitem.Item1.ToString("M/dd");
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new Entry((float)dailyitem.Item2)
            {
                ValueLabel = Math.Round(dailyitem.Item2, 1).ToString() + " ms",
                TextColor = textcolor,
                Label = label,
                Color = color,
            };
        }

        private Entry CreateEntryMS(double item)
        {
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new Entry((float)item)
            {
                ValueLabel = Math.Round(item, 1).ToString() + " ms",
                TextColor = textcolor,
                Color = color,
            };
        }

        private Entry CreateDayEntryCnt(Tuple<DateTime, double> dailyitem)
        {
            var label = dailyitem.Item1.ToString("M/dd");
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new Entry((int)dailyitem.Item2)
            {
                ValueLabel = dailyitem.Item2.ToString(),
                TextColor = textcolor,
                Label = label,
                Color = color,
            };
        }

        public Chart AvgCorRTOverTimeChart => new LineChart()
        {
            Entries = AvgCorRTOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18
        };

        public Chart CumAvgCorRTOverTimeChart => new LineChart()
        {
            Entries = CumAvgCorRTOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18
        };

        public Chart CumAvgCorRTByTrialChart => new LineChart()
        {
            Entries = CumAvgCorRTByTrial.Select(CreateEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18
        };

        private List<Entry> GetBestAvgCorRTDays()
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<Entry> es = new List<Entry>();
            if (AvgCorRTByDay.Last().Item1 == DateTime.Today)
            {
                Entry e = new Entry((float)AvgCorRTByDay.Last().Item2);
                e.ValueLabel = Math.Round(AvgCorRTByDay.Last().Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                e.Label = "Today";
                e.Color = clrs[0];
                es.Add(e);
            }
            var v = AvgCorRTByDay.OrderBy(x => x.Item2).Take(5);
            foreach (Tuple<DateTime, double> rec in v)
            {
                Entry e = new Entry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                e.Label = rec.Item1.ToString("M/dd"); ;
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }


        public Chart BestAvgCorRTDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestAvgCorRTDays()
        };

        private List<Entry> GetAvgCorRTByStimType()
        {
            List<Entry> es = new List<Entry>();
            foreach (Tuple<int, double> rec in AvgCorRTByStimType)
            {
                Entry e = new Entry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == 1)
                {
                    e.Label = "1 Choice";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else if(rec.Item1 == 2)
                {
                    e.Label = "2 Choices";
                    e.Color = SKColor.Parse("#77d065");
                }
                else
                {
                    e.Label = "4 Choices";
                    e.Color = SKColor.Parse("#b455b6");
                }
                es.Add(e);
            }
            return es;
        }

        private List<Entry> GetAvgCorPctByStimType()
        {
            List<Entry> es = new List<Entry>();
            foreach (Tuple<int, double> rec in AvgCorPctByStimType)
            {
                Entry e = new Entry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2 * 100, 1).ToString() + "%";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == 1)
                {
                    e.Label = "1 Choice";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else if (rec.Item1 == 2)
                {
                    e.Label = "2 Choices";
                    e.Color = SKColor.Parse("#77d065");
                }
                else
                {
                    e.Label = "4 Choices";
                    e.Color = SKColor.Parse("#b455b6");
                }
                es.Add(e);
            }
            return es;
        }

        private List<Entry> GetAvgCorRTByStimLoc2()
        {
            List<Entry> es = new List<Entry>();
            foreach (Tuple<int, double> rec in AvgCorRTByStimLoc2)
            {
                Entry e = new Entry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == 0)
                {
                    e.Label = "Left box";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else
                {
                    e.Label = "Right box";
                    e.Color = SKColor.Parse("#77d065");
                }
                es.Add(e);
            }
            return es;
        }

        private List<Entry> GetAvgCorPctByStimLoc2()
        {
            List<Entry> es = new List<Entry>();
            foreach (Tuple<int, double> rec in AvgCorPctByStimLoc2)
            {
                Entry e = new Entry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2 * 100, 1).ToString() + "%";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == 0)
                {
                    e.Label = "Left box";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else
                {
                    e.Label = "Right box";
                    e.Color = SKColor.Parse("#77d065");
                }
                es.Add(e);
            }
            return es;
        }

        private List<Entry> GetAvgCorRTByStimLoc4()
        {
            List<Entry> es = new List<Entry>();
            foreach (Tuple<int, double> rec in AvgCorRTByStimLoc4)
            {
                Entry e = new Entry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == 0)
                {
                    e.Label = "Far left box";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else if (rec.Item1 == 1)
                {
                    e.Label = "Mid left box";
                    e.Color = SKColor.Parse("#77d065");
                }
                else if (rec.Item1 == 2)
                {
                    e.Label = "Mid right box";
                    e.Color = SKColor.Parse("#b455b6");
                }
                else 
                {
                    e.Label = "Far right box";
                    e.Color = SKColor.Parse("#3498db");
                }
                es.Add(e);
            }
            return es;
        }

        private List<Entry> GetAvgCorPctByStimLoc4()
        {
            List<Entry> es = new List<Entry>();
            foreach (Tuple<int, double> rec in AvgCorPctByStimLoc4)
            {
                Entry e = new Entry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2 * 100, 1).ToString() + "%";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == 0)
                {
                    e.Label = "Far left box";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else if (rec.Item1 == 1)
                {
                    e.Label = "Mid left box";
                    e.Color = SKColor.Parse("#77d065");
                }
                else if (rec.Item1 == 2)
                {
                    e.Label = "Mid right box";
                    e.Color = SKColor.Parse("#b455b6");
                }
                else
                {
                    e.Label = "Far right box";
                    e.Color = SKColor.Parse("#3498db");
                }
                es.Add(e);
            }
            return es;
        }

        public Chart AvgCorRTByStimLoc2Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorRTByStimLoc2()
        };

        public Chart AvgCorPctByStimLoc2Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorPctByStimLoc2()
        };

        public Chart AvgCorRTByStimLoc4Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorRTByStimLoc4()
        };

        public Chart AvgCorPctByStimLoc4Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorPctByStimLoc4()
        };

        public Chart AvgCorRTByStimTypeChart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorRTByStimType()
        };

        public Chart AvgCorPctByStimTypeChart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorPctByStimType()
        };

        public Chart TrialCountOverTimeChart => new BarChart()
        {
            Entries = TrialCountOverTime.Select(CreateDayEntryCnt),
            Margin = 10
        };
    }

}
