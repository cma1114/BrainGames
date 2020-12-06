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
        private List<Tuple<DateTime, double>> TrialCountOverTime;

        private IEnumerable<Tuple<int, double>> AvgCorRTByStimType;
        private IEnumerable<Tuple<int, double>> AvgCorPctByStimType;
        private IEnumerable<Tuple<int, double>> AvgCorRTByStimLoc2;
        private IEnumerable<Tuple<int, double>> AvgCorPctByStimLoc2;
        private IEnumerable<Tuple<int, double>> AvgCorRTByStimLoc4;
        private IEnumerable<Tuple<int, double>> AvgCorPctByStimLoc4;

//        private IEnumerable<double> CumAvgCorRTByTrial;

        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByDay1;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByWeek1;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByMonth1;
        private List<Tuple<DateTime, double>> AvgCorRTOverTime1;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByDay2;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByWeek2;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByMonth2;
        private List<Tuple<DateTime, double>> AvgCorRTOverTime2;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByDay4;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByWeek4;
        private IEnumerable<Tuple<DateTime, double>> AvgCorRTByMonth4;
        private List<Tuple<DateTime, double>> AvgCorRTOverTime4;

        private List<Tuple<DateTime, double>> CumAvgCorRTByDay1;
        private List<Tuple<DateTime, double>> CumAvgCorRTByWeek1;
        private List<Tuple<DateTime, double>> CumAvgCorRTByMonth1;
        private List<Tuple<DateTime, double>> CumAvgCorRTOverTime1;

        private List<Tuple<DateTime, double>> CumAvgCorRTByDay2;
        private List<Tuple<DateTime, double>> CumAvgCorRTByWeek2;
        private List<Tuple<DateTime, double>> CumAvgCorRTByMonth2;
        private List<Tuple<DateTime, double>> CumAvgCorRTOverTime2;

        private List<Tuple<DateTime, double>> CumAvgCorRTByDay4;
        private List<Tuple<DateTime, double>> CumAvgCorRTByWeek4;
        private List<Tuple<DateTime, double>> CumAvgCorRTByMonth4;
        private List<Tuple<DateTime, double>> CumAvgCorRTOverTime4;

        private double sf = 0.03;

        private bool _compare = false;
        public bool Compare
        {
            get => _compare;
            set { SetProperty(ref _compare, value); }
        }



        List<Tuple<DateTime, double>> FillTimeList(List<Tuple<DateTime, double>> daylist, List<Tuple<DateTime, double>> weeklist, List<Tuple<DateTime, double>> monthlist)
        {
            IEnumerable<Tuple<DateTime, double>> t;
            List<Tuple<DateTime, double>> v = new List<Tuple<DateTime, double>>();
            DateTime doi;
            timeblock b;
            int historicrange = daylist.Count() > 0 ? DateTime.Now.Subtract(daylist.First().Item1).Days : 0;
            if (historicrange < 7) historicrange = 7;
            if (historicrange > 30)
            {
                historicrange = weeklist.Count() > 0 ? (int)Math.Ceiling(DateTime.Now.Subtract(weeklist.First().Item1).Days / 7.0) : 0;
                if (historicrange < 7) historicrange = 7;
                if (historicrange > 30)
                {
                    historicrange = monthlist.Count() > 0 ? (int)Math.Ceiling(DateTime.Now.Subtract(monthlist.First().Item1).Days / 31.0) : 0;
                    if (historicrange < 7) historicrange = 7;
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

                if (b == timeblock.Day)
                {
                    doi = DateTime.Now.AddDays(i - historicrange).Date;
                    t = daylist.Where(x => x.Item1 == doi);
                }
                else if (b == timeblock.Week)
                {
                    doi = DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddDays(i * 7 - historicrange * 7).Date;
                    t = weeklist.Where(x => x.Item1 == doi);
                }
                else
                {
                    doi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(i - historicrange).Date;
                    t = monthlist.Where(x => x.Item1 == doi);
                }
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

        private List<Tuple<DateTime, double>> MovingAverage(List<Tuple<DateTime, double>> series, int period)
        {
            var result = new List<Tuple<DateTime, double>>();

            for (int i = 0; i < series.Count(); i++)
            {
                double total = 0;
                for (int x = 0; x <= i; x++)
                    total += series[x].Item2;
                double average = total / (i + 1);
                result.Add(Tuple.Create(series[i].Item1, average));
            }
            return result;
        }

        public RTStatsViewModel()
        {
            if (App.mum.GameShares.Where(x => x.game == "RT")?.Count() > 0) Compare = true;

            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.RTGameRecordSchema>("select * from RTGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                TrialsByDay = ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                TrialsByWeek = ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                TrialsByMonth = ur.GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                if (ur.Where(x => x.boxes == 1 && x.cor == true).Count() > 0)
                {
                    AvgCorRTByDay1 = ur.Where(x => x.boxes == 1).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / Math.Max(x.Where(y => y.cor == true).Count(), 1))).OrderBy(x => x.Item1);
                    AvgCorRTByWeek1 = ur.Where(x => x.boxes == 1).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorRTByMonth1 = ur.Where(x => x.boxes == 1).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorRTOverTime1 = FillTimeList(AvgCorRTByDay1.ToList(), AvgCorRTByWeek1.ToList(), AvgCorRTByMonth1.ToList());
                }
                if (ur.Where(x => x.boxes == 2 && x.cor == true).Count() > 0)
                {
                    AvgCorRTByDay2 = ur.Where(x => x.boxes == 2).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / Math.Max(x.Where(y => y.cor == true).Count(), 1))).OrderBy(x => x.Item1);
                    AvgCorRTByWeek2 = ur.Where(x => x.boxes == 2).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorRTByMonth2 = ur.Where(x => x.boxes == 2).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorRTByStimLoc2 = ur.Where(x => x.boxes == 2).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorPctByStimLoc2 = ur.Where(x => x.boxes == 2).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Where(y => y.cor == true || y.cor == false).Count())).OrderBy(x => x.Item1);
                    AvgCorRTOverTime2 = FillTimeList(AvgCorRTByDay2.ToList(), AvgCorRTByWeek2.ToList(), AvgCorRTByMonth2.ToList());
                }
                if (ur.Where(x => x.boxes == 4 && x.cor == true).Count() > 0)
                {
                    AvgCorRTByDay4 = ur.Where(x => x.boxes == 4).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / Math.Max(x.Where(y => y.cor == true).Count(), 1))).OrderBy(x => x.Item1);
                    AvgCorRTByWeek4 = ur.Where(x => x.boxes == 4).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorRTByMonth4 = ur.Where(x => x.boxes == 4).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorRTByStimLoc4 = ur.Where(x => x.boxes == 4).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorPctByStimLoc4 = ur.Where(x => x.boxes == 4).GroupBy(x => x.corbox).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Where(y => y.cor == true || y.cor == false).Count())).OrderBy(x => x.Item1);
                    AvgCorRTOverTime4 = FillTimeList(AvgCorRTByDay4.ToList(), AvgCorRTByWeek4.ToList(), AvgCorRTByMonth4.ToList());
                }
                if (ur.Where(x => x.cor == true).Count() > 0)
                {
                    AvgCorRTByStimType = ur.GroupBy(x => x.boxes).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                    AvgCorPctByStimType = ur.GroupBy(x => x.boxes).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Where(y => y.cor == true || y.cor == false).Count())).OrderBy(x => x.Item1);
                }
//                CumAvgCorRTByTrial = ur.Where(x => x.avgrt > 0).OrderByDescending(x => x.datetime).Select(x => x.avgrt).Take(30);

                CumAvgCorRTByDay1 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 1).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByWeek1 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 1).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByMonth1 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 1).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByDay2 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 2).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByWeek2 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 2).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByMonth2 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 2).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByDay4 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 4).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByWeek4 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 4).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByMonth4 = MovingAverage(ur.Where(x => x.avgrt > 0 && x.boxes == 4).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgrt) / x.Count())).OrderBy(x => x.Item1).ToList(), 1);


                CumAvgCorRTOverTime1 = CumAvgCorRTByDay1.Count() <= 30 ? CumAvgCorRTByDay1.ToList() : (CumAvgCorRTByWeek1.Count() <= 30 ? CumAvgCorRTByWeek1.ToList() : CumAvgCorRTByMonth1.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
                CumAvgCorRTOverTime2 = CumAvgCorRTByDay2.Count() <= 30 ? CumAvgCorRTByDay2.ToList() : (CumAvgCorRTByWeek2.Count() <= 30 ? CumAvgCorRTByWeek2.ToList() : CumAvgCorRTByMonth2.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
                CumAvgCorRTOverTime4 = CumAvgCorRTByDay4.Count() <= 30 ? CumAvgCorRTByDay4.ToList() : (CumAvgCorRTByWeek4.Count() <= 30 ? CumAvgCorRTByWeek4.ToList() : CumAvgCorRTByMonth4.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
                //CumAvgCorRTOverTime1 = FillTimeList(CumAvgCorRTByDay1, CumAvgCorRTByWeek1, CumAvgCorRTByMonth1);
                //CumAvgCorRTOverTime2 = FillTimeList(CumAvgCorRTByDay2, CumAvgCorRTByWeek2, CumAvgCorRTByMonth2);
                //CumAvgCorRTOverTime4 = FillTimeList(CumAvgCorRTByDay4, CumAvgCorRTByWeek4, CumAvgCorRTByMonth4);
                TrialCountOverTime = FillTimeList(TrialsByDay.ToList(), TrialsByWeek.ToList(), TrialsByMonth.ToList());
            }
        }

        private ChartEntry CreateDayEntryMS(Tuple<DateTime, double> dailyitem)
        {
            var label = dailyitem.Item1.ToString("M/dd");
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new ChartEntry((float)dailyitem.Item2)
            {
                ValueLabel = Math.Round(dailyitem.Item2, 1).ToString() + " ms",
                TextColor = textcolor,
                Label = label,
                Color = color
            };
        }

        private ChartEntry CreateEntryMS(double item)
        {
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new ChartEntry((float)item)
            {
                ValueLabel = Math.Round(item, 1).ToString() + " ms",
                TextColor = textcolor,
                Color = color,
                Label = " "
            };
        }

        private ChartEntry CreateDayEntryCnt(Tuple<DateTime, double> dailyitem)
        {
            var label = dailyitem.Item1.ToString("M/dd");
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new ChartEntry((int)dailyitem.Item2)
            {
                ValueLabel = dailyitem.Item2.ToString(),
                TextColor = textcolor,
                Label = label,
                Color = color
            };
        }
/*
        public Chart AvgCorRTOverTimeChart => new LineChart()
        {
            Entries = AvgCorRTOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            LabelOrientation = Orientation.Vertical
        };
*/
        public Chart CumAvgCorRTOverTime1Chart => new LineChart()
        {
            Entries = CumAvgCorRTOverTime1.Count == 0 ? null : CumAvgCorRTOverTime1.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = CumAvgCorRTOverTime1.Count == 0 ? 0 : (float)Math.Max(0, CumAvgCorRTOverTime1.Min(x => x.Item2) - CumAvgCorRTOverTime1.Min(x => x.Item2) * sf),
            MaxValue = CumAvgCorRTOverTime1.Count == 0 ? 0 : (float)(CumAvgCorRTOverTime1.Max(x => x.Item2) + CumAvgCorRTOverTime1.Max(x => x.Item2) * sf)
        };

        public Chart CumAvgCorRTOverTime2Chart => new LineChart()
        {
            Entries = CumAvgCorRTOverTime2.Count == 0 ? null : CumAvgCorRTOverTime2.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = CumAvgCorRTOverTime2.Count == 0 ? 0 : (float)Math.Max(0, (CumAvgCorRTOverTime2.Count == 0 ? 0 : CumAvgCorRTOverTime2.Min(x => x.Item2)) - (CumAvgCorRTOverTime2.Count == 0 ? 0 : CumAvgCorRTOverTime2.Min(x => x.Item2)) * sf),
            MaxValue = (float)((CumAvgCorRTOverTime2.Count == 0 ? 0 : CumAvgCorRTOverTime2.Max(x => x.Item2)) + (CumAvgCorRTOverTime2.Count == 0 ? 0 : CumAvgCorRTOverTime2.Max(x => x.Item2)) * sf)
        };

        public Chart CumAvgCorRTOverTime4Chart => new LineChart()
        {
            Entries = CumAvgCorRTOverTime4.Count == 0 ? null : CumAvgCorRTOverTime4.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, (CumAvgCorRTOverTime4.Count == 0 ? 0 : CumAvgCorRTOverTime4.Min(x => x.Item2)) - (CumAvgCorRTOverTime4.Count == 0 ? 0 : CumAvgCorRTOverTime4.Min(x => x.Item2)) * sf),
            MaxValue = (float)((CumAvgCorRTOverTime4.Count == 0 ? 0 : CumAvgCorRTOverTime4.Max(x => x.Item2)) + (CumAvgCorRTOverTime4.Count == 0 ? 0 : CumAvgCorRTOverTime4.Max(x => x.Item2)) * sf)
        };

/*
        public Chart CumAvgCorRTByTrialChart => new LineChart()
        {
            Entries = CumAvgCorRTByTrial.Reverse().Select(CreateEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18
        };
*/
        private List<ChartEntry> GetBestAvgCorRTDays()
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            if (AvgCorRTByDay1?.Count() > 0)
            {
                if (AvgCorRTByDay1.Last().Item1 == DateTime.Today && AvgCorRTByDay1.Last().Item2 > 0)
                {
                    ChartEntry e = new ChartEntry((float)AvgCorRTByDay1.Last().Item2);
                    e.ValueLabel = Math.Round(AvgCorRTByDay1.Last().Item2, 1).ToString() + " ms";
                    e.TextColor = SKColors.Black;
                    e.Label = "Today: 1";
                    e.Color = clrs[0];
                    es.Add(e);
                }
            }
            if (AvgCorRTByDay2?.Count() > 0)
            {
                if (AvgCorRTByDay2.Last().Item1 == DateTime.Today && AvgCorRTByDay2.Last().Item2 > 0)
                {
                    ChartEntry e = new ChartEntry((float)AvgCorRTByDay2.Last().Item2);
                    e.ValueLabel = Math.Round(AvgCorRTByDay2.Last().Item2, 1).ToString() + " ms";
                    e.TextColor = SKColors.Black;
                    e.Label = "Today: 2";
                    e.Color = clrs[0];
                    es.Add(e);
                }
            }
            if (AvgCorRTByDay4?.Count() > 0)
            {
                if (AvgCorRTByDay4.Last().Item1 == DateTime.Today && AvgCorRTByDay4.Last().Item2 > 0)
                {
                    ChartEntry e = new ChartEntry((float)AvgCorRTByDay4.Last().Item2);
                    e.ValueLabel = Math.Round(AvgCorRTByDay4.Last().Item2, 1).ToString() + " ms";
                    e.TextColor = SKColors.Black;
                    e.Label = "Today: 4";
                    e.Color = clrs[0];
                    es.Add(e);
                }
            }
            if (AvgCorRTByDay1?.Count() > 0)
            {
                var v = AvgCorRTByDay1.Where(x => x.Item2 > 0 && x.Item1 != DateTime.Today).OrderBy(x => x.Item2).Take(1);
                foreach (Tuple<DateTime, double> rec in v)
                {
                    ChartEntry e = new ChartEntry((float)rec.Item2);
                    e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                    e.TextColor = SKColors.Black;
                    e.Label = "1: " + rec.Item1.ToString("M/dd");
                    e.Color = clrs[idx++];
                    es.Add(e);
                }
            }
            if (AvgCorRTByDay2?.Count() > 0)
            {
                var v = AvgCorRTByDay2.Where(x => x.Item2 > 0 && x.Item1 != DateTime.Today).OrderBy(x => x.Item2).Take(1);
                foreach (Tuple<DateTime, double> rec in v)
                {
                    ChartEntry e = new ChartEntry((float)rec.Item2);
                    e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                    e.TextColor = SKColors.Black;
                    e.Label = "2: " + rec.Item1.ToString("M/dd");
                    e.Color = clrs[idx++];
                    es.Add(e);
                }
            }
            if (AvgCorRTByDay4?.Count() > 0)
            {
                var v = AvgCorRTByDay4.Where(x => x.Item2 > 0 && x.Item1 != DateTime.Today).OrderBy(x => x.Item2).Take(1);
                foreach (Tuple<DateTime, double> rec in v)
                {
                    ChartEntry e = new ChartEntry((float)rec.Item2);
                    e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                    e.TextColor = SKColors.Black;
                    e.Label = "4: " + rec.Item1.ToString("M/dd");
                    e.Color = clrs[idx++];
                    es.Add(e);
                }
            }
            return es;
        }


        public Chart BestAvgCorRTDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestAvgCorRTDays(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        private List<ChartEntry> GetAvgCorRTByStimType()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            foreach (Tuple<int, double> rec in AvgCorRTByStimType)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
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

        private List<ChartEntry> GetAvgCorPctByStimType()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            foreach (Tuple<int, double> rec in AvgCorPctByStimType)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
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

        private List<ChartEntry> GetAvgCorRTByStimLoc2()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            if (AvgCorRTByStimLoc2 != null)
            {
                foreach (Tuple<int, double> rec in AvgCorRTByStimLoc2)
                {
                    ChartEntry e = new ChartEntry((float)rec.Item2);
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
            }
            return es;
        }

        private List<ChartEntry> GetAvgCorPctByStimLoc2()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            if (AvgCorPctByStimLoc2 != null)
            {
                foreach (Tuple<int, double> rec in AvgCorPctByStimLoc2)
                {
                    ChartEntry e = new ChartEntry((float)rec.Item2);
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
            }
            return es;
        }

        private List<ChartEntry> GetAvgCorRTByStimLoc4()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            if (AvgCorRTByStimLoc4 != null)
            {
                foreach (Tuple<int, double> rec in AvgCorRTByStimLoc4)
                {
                    ChartEntry e = new ChartEntry((float)rec.Item2);
                    e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                    e.TextColor = SKColors.Black;
                    if (rec.Item1 == 0)
                    {
                        e.Label = "Far left"; // "Upper left box";
                        e.Color = SKColor.Parse("#2c3e50");
                    }
                    else if (rec.Item1 == 1)
                    {
                        e.Label = "Middle left"; // "Upper right box";
                        e.Color = SKColor.Parse("#77d065");
                    }
                    else if (rec.Item1 == 2)
                    {
                        e.Label = "Middle right"; //"Lower left box";
                        e.Color = SKColor.Parse("#b455b6");
                    }
                    else
                    {
                        e.Label = "Far right"; //"Lower right box";
                        e.Color = SKColor.Parse("#3498db");
                    }
                    es.Add(e);
                }
            }
            return es;
        }

        private List<ChartEntry> GetAvgCorPctByStimLoc4()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            if (AvgCorPctByStimLoc4 != null)
            {
                foreach (Tuple<int, double> rec in AvgCorPctByStimLoc4)
                {
                    ChartEntry e = new ChartEntry((float)rec.Item2);
                    e.ValueLabel = Math.Round(rec.Item2 * 100, 1).ToString() + "%";
                    e.TextColor = SKColors.Black;
                    if (rec.Item1 == 0)
                    {
                        e.Label = "Far left"; // "Upper left box";
                        e.Color = SKColor.Parse("#2c3e50");
                    }
                    else if (rec.Item1 == 1)
                    {
                        e.Label = "Middle left"; // "Upper right box";
                        e.Color = SKColor.Parse("#77d065");
                    }
                    else if (rec.Item1 == 2)
                    {
                        e.Label = "Middle right"; //"Lower left box";
                        e.Color = SKColor.Parse("#b455b6");
                    }
                    else
                    {
                        e.Label = "Far right"; // "Lower right box";
                        e.Color = SKColor.Parse("#3498db");
                    }
                    es.Add(e);
                }
            }
            return es;
        }

        public Chart AvgCorRTByStimLoc2Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorRTByStimLoc2(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorPctByStimLoc2Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorPctByStimLoc2(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorRTByStimLoc4Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorRTByStimLoc4(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorPctByStimLoc4Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorPctByStimLoc4(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorRTByStimTypeChart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorRTByStimType(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorPctByStimTypeChart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorPctByStimType(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart TrialCountOverTimeChart => new BarChart()
        {
            Entries = TrialCountOverTime.Select(CreateDayEntryCnt),
            Margin = 10,
            ValueLabelOrientation = Orientation.Horizontal
        };
    }

}
