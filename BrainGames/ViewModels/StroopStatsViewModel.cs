using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Microcharts;
using SkiaSharp;
using System.Linq;
using BrainGames.Utility;
using BrainGames.Models;
using BrainGames.ViewModels;

namespace BrainGames.ViewModels
{
    public class StroopStatsViewModel : ViewModelBase
    {
        private List<DataSchemas.StroopGameRecordSchema> ur = new List<DataSchemas.StroopGameRecordSchema>();

        private List<Tuple<DateTime, double>> TrialsByDay;
        private List<Tuple<DateTime, double>> TrialsByWeek;
        private List<Tuple<DateTime, double>> TrialsByMonth;
        private List<Tuple<DateTime, double>> TrialCountOverTime;

        private List<Tuple<DateTime, double>> AvgICDifCorRTByDay;
        private List<Tuple<DateTime, double>> AvgICDifCorRTByWeek;
        private List<Tuple<DateTime, double>> AvgICDifCorRTByMonth;
        private List<Tuple<DateTime, double>> AvgICDifCorRTOverTime;

        private List<Tuple<DateTime, double>> CumAvgCorRTByDay;
        private List<Tuple<DateTime, double>> CumAvgCorRTByWeek;
        private List<Tuple<DateTime, double>> CumAvgCorRTByMonth;
        private List<Tuple<DateTime, double>> CumAvgCorRTOverTime;

        private List<Tuple<DateTime, double>> CumAvgICDifCorRTByDay;
        private List<Tuple<DateTime, double>> CumAvgICDifCorRTByWeek;
        private List<Tuple<DateTime, double>> CumAvgICDifCorRTByMonth;
        private List<Tuple<DateTime, double>> CumAvgICDifCorRTOverTime;

        private List<Tuple<DateTime, double>> CumAvgICDifCorPctByDay;
        private List<Tuple<DateTime, double>> CumAvgICDifCorPctByWeek;
        private List<Tuple<DateTime, double>> CumAvgICDifCorPctByMonth;
        private List<Tuple<DateTime, double>> CumAvgICDifCorPctOverTime;

        private IEnumerable<Tuple<bool, double>> AvgCorRTByStimType;
        private IEnumerable<Tuple<bool, double>> AvgCorPctByStimType;
        private IEnumerable<Tuple<string, double>> AvgICDifCorRTByStimWord;
        private IEnumerable<Tuple<string, double>> AvgICDifCorRTByStimColor;
        private List<double> CumAvgICDifCorRTByBlock;
        private List<double> CumAvgCorRTByBlock;

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

        private List<double> MovingAverage2(List<Tuple<int, double>> series, int period)
        {
            var result = new List<double>();

            for (int i = 0; i < series.Count(); i++)
            {
                double total = 0;
                for (int x = 0; x <= i; x++)
                    total += series[x].Item2;
                double average = total / (i + 1);
                result.Add(average);
            }
            return result;
        }

        public StroopStatsViewModel()
        {
            if (App.mum.GameShares.Where(x => x.game == "Stroop")?.Count() > 0) Compare = true;

            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.StroopGameRecordSchema>("select * from StroopGameRecordSchema"); }
            catch (Exception ex)
            {
                ;
            }
            if (ur != null && ur.Count() > 0)
            {
                TrialsByDay = ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1).ToList();
                TrialsByWeek = ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1).ToList();
                TrialsByMonth = ur.GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1).ToList();

                AvgICDifCorRTByDay = ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList();
                AvgICDifCorRTByWeek = ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList();
                AvgICDifCorRTByMonth = ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList();

                CumAvgCorRTByDay = MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, ((x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()) + (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count())) / 2)).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByWeek = MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, ((x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()) + (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count())) / 2)).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgCorRTByMonth = MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, ((x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()) + (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count())) / 2)).OrderBy(x => x.Item1).ToList(), 1);

                CumAvgICDifCorRTByDay = MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgICDifCorRTByWeek = MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgICDifCorRTByMonth = MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList(), 1);

                CumAvgICDifCorPctByDay = MovingAverage(ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.congruent == true && y.cor == true).Count() / x.Where(y => y.congruent == true && (y.cor == true || y.cor == false)).Count() - (double)x.Where(y => y.congruent == false && y.cor == true).Count() / x.Where(y => y.congruent == false && (y.cor == true || y.cor == false)).Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgICDifCorPctByWeek = MovingAverage(ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.congruent == true && y.cor == true).Count() / x.Where(y => y.congruent == true && (y.cor == true || y.cor == false)).Count() - (double)x.Where(y => y.congruent == false && y.cor == true).Count() / x.Where(y => y.congruent == false && (y.cor == true || y.cor == false)).Count())).OrderBy(x => x.Item1).ToList(), 1);
                CumAvgICDifCorPctByMonth = MovingAverage(ur.GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.congruent == true && y.cor == true).Count() / x.Where(y => y.congruent == true && (y.cor == true || y.cor == false)).Count() - (double)x.Where(y => y.congruent == false && y.cor == true).Count() / x.Where(y => y.congruent == false && (y.cor == true || y.cor == false)).Count())).OrderBy(x => x.Item1).ToList(), 1);


                AvgCorRTByStimType = ur.GroupBy(x => x.congruent).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.reactiontime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                AvgCorPctByStimType = ur.GroupBy(x => x.congruent).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Where(y => y.cor == true || y.cor == false).Count())).OrderBy(x => x.Item1);

                AvgICDifCorRTByStimWord = ur.Where(x => x.cor == true).GroupBy(x => x.word).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1);

                AvgICDifCorRTByStimColor = ur.Where(x => x.cor == true).GroupBy(x => x.textcolor).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1);

                CumAvgCorRTByBlock = MovingAverage2(ur.Where(x => x.cor == true).GroupBy(x => (int)Math.Ceiling(x.trialnum / 12.0)).Select(x => Tuple.Create(x.Key, ((x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / Math.Max(/*numerator will be zero anyway*/x.Where(y => y.congruent == true).Count(),1)) + (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / Math.Max(x.Where(y => y.congruent == false).Count(),1))) / 2.0)).OrderByDescending(x => x.Item1).Take(30).Reverse().ToList(), 1);
                CumAvgICDifCorRTByBlock = MovingAverage2(ur.Where(x => x.cor == true).GroupBy(x => (int)Math.Ceiling(x.trialnum / 12.0)).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / Math.Max(x.Where(y => y.congruent == false).Count(),1)) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / Math.Max(x.Where(y => y.congruent == true).Count(),1)))).OrderByDescending(x => x.Item1).Take(30).Reverse().ToList(), 1);

                TrialCountOverTime = FillTimeList(TrialsByDay, TrialsByWeek, TrialsByMonth);
                AvgICDifCorRTOverTime = AvgICDifCorRTByDay.Count() <= 30 ? AvgICDifCorRTByDay.ToList() : (AvgICDifCorRTByWeek.Count() <= 30 ? AvgICDifCorRTByWeek.ToList() : AvgICDifCorRTByMonth.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
//                AvgICDifCorRTOverTime = FillTimeList(AvgICDifCorRTByDay, AvgICDifCorRTByWeek, AvgICDifCorRTByMonth);
                CumAvgCorRTOverTime = CumAvgCorRTByDay.Count() <= 30 ? CumAvgCorRTByDay.ToList() : (CumAvgCorRTByWeek.Count() <= 30 ? CumAvgCorRTByWeek.ToList() : CumAvgCorRTByMonth.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
                CumAvgICDifCorRTOverTime = CumAvgICDifCorRTByDay.Count() <= 30 ? CumAvgICDifCorRTByDay.ToList() : (CumAvgICDifCorRTByWeek.Count() <= 30 ? CumAvgICDifCorRTByWeek.ToList() : CumAvgICDifCorRTByMonth.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
                CumAvgICDifCorPctOverTime = CumAvgICDifCorPctByDay.Count() <= 30 ? CumAvgICDifCorPctByDay.ToList() : (CumAvgICDifCorPctByWeek.Count() <= 30 ? CumAvgICDifCorPctByWeek.ToList() : CumAvgICDifCorPctByMonth.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
//                CumAvgCorRTOverTime = FillTimeList(CumAvgCorRTByDay, CumAvgCorRTByWeek, CumAvgCorRTByMonth);
//                CumAvgICDifCorRTOverTime = FillTimeList(CumAvgICDifCorRTByDay, CumAvgICDifCorRTByWeek, CumAvgICDifCorRTByMonth);
//                CumAvgICDifCorPctOverTime = FillTimeList(CumAvgICDifCorPctByDay, CumAvgICDifCorPctByWeek, CumAvgICDifCorPctByMonth);
            }
        }

        public Chart BestAvgCorRTDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestAvgCorRTDays(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart CumAvgCorRTOverTimeChart => new LineChart()
        {
            Entries = CumAvgCorRTOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, CumAvgCorRTOverTime.Min(x => x.Item2) - CumAvgCorRTOverTime.Min(x => x.Item2) * sf),
            MaxValue = (float)(CumAvgCorRTOverTime.Max(x => x.Item2) + CumAvgCorRTOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumAvgICDifCorRTOverTimeChart => new LineChart()
        {
            Entries = CumAvgICDifCorRTOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)(CumAvgICDifCorRTOverTime.Min(x => x.Item2) - CumAvgICDifCorRTOverTime.Min(x => x.Item2) * sf),
            MaxValue = (float)(CumAvgICDifCorRTOverTime.Max(x => x.Item2) + CumAvgICDifCorRTOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumAvgICDifCorPctOverTimeChart => new LineChart()
        {
            Entries = CumAvgICDifCorPctOverTime.Select(CreateDayEntryPct),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)(CumAvgICDifCorPctOverTime.Min(x => x.Item2) - CumAvgICDifCorPctOverTime.Min(x => x.Item2) * sf),
            MaxValue = (float)(CumAvgICDifCorPctOverTime.Max(x => x.Item2) + CumAvgICDifCorPctOverTime.Max(x => x.Item2) * sf)
        };

        public Chart AvgICDifCorRTOverTimeChart => new LineChart()
        {
            Entries = AvgICDifCorRTOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)(AvgICDifCorRTOverTime.Min(x => x.Item2) - AvgICDifCorRTOverTime.Min(x => x.Item2) * sf),
            MaxValue = (float)(AvgICDifCorRTOverTime.Max(x => x.Item2) + AvgICDifCorRTOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumAvgCorRTByBlockChart => new LineChart()
        {
            Entries = CumAvgCorRTByBlock.Select(CreateEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, CumAvgCorRTByBlock.Min() - CumAvgCorRTByBlock.Min() * sf),
            MaxValue = (float)(CumAvgCorRTByBlock.Max() + CumAvgCorRTByBlock.Max() * sf)
        };

        public Chart CumAvgICDifCorRTByBlockChart => new LineChart()
        {
            Entries = CumAvgICDifCorRTByBlock.Select(CreateEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)(CumAvgICDifCorRTByBlock.Min() - CumAvgICDifCorRTByBlock.Min() * sf),
            MaxValue = (float)(CumAvgICDifCorRTByBlock.Max() + CumAvgICDifCorRTByBlock.Max() * sf)
        };

        public Chart TrialCountOverTimeChart => new BarChart()
        {
            Entries = TrialCountOverTime.Select(CreateDayEntryCnt),
            Margin = 10,
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

        public Chart AvgICDifCorRTByStimWordChart => new BarChart()
        {
            Margin = 10,
            Entries = AvgICDifCorRTByStimWord.Select(GetAvgICDifCorRTByStimWordOrColor),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgICDifCorRTByStimColorChart => new BarChart()
        {
            Margin = 10,
            Entries = AvgICDifCorRTByStimColor.Select(GetAvgICDifCorRTByStimWordOrColor),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };


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

        private ChartEntry CreateDayEntryPct(Tuple<DateTime, double> dailyitem)
        {
            var label = dailyitem.Item1.ToString("M/dd");
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new ChartEntry((float)dailyitem.Item2)
            {
                ValueLabel = Math.Round(dailyitem.Item2 * 100, 1).ToString() + "%",
                TextColor = textcolor,
                Label = label,
                Color = color
            };
        }

        private List<ChartEntry> GetBestAvgCorRTDays()
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            if (AvgICDifCorRTByDay.Last().Item1 == DateTime.Today)
            {
                ChartEntry e = new ChartEntry((float)AvgICDifCorRTByDay.Last().Item2);
                e.ValueLabel = Math.Round(AvgICDifCorRTByDay.Last().Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                e.Label = "Today";
                e.Color = clrs[0];
                es.Add(e);
            }
            var v = AvgICDifCorRTByDay.Where(x => x.Item2 > 0 && x.Item1 != DateTime.Today).OrderBy(x => x.Item2).Take(5);
            foreach (Tuple<DateTime, double> rec in v)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                e.Label = rec.Item1.ToString("M/dd"); ;
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }

        private ChartEntry GetAvgICDifCorRTByStimWordOrColor(Tuple<string, double> rec)
        {
                ChartEntry e = new ChartEntry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == "RED")
                {
                    e.Label = "RED";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else if (rec.Item1 == "GREEN")
                {
                    e.Label = "GREEN";
                    e.Color = SKColor.Parse("#77d065");
                }
                else if (rec.Item1 == "BLUE")
                {
                    e.Label = "BLUE";
                    e.Color = SKColor.Parse("#b455b6");
                }
                else
                {
                    e.Label = "YELLOW";
                    e.Color = SKColor.Parse("#3498db");
                }
            return e;
        }


        private List<ChartEntry> GetAvgCorRTByStimType()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            foreach (Tuple<bool, double> rec in AvgCorRTByStimType)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == true)
                {
                    e.Label = "Congruent";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else
                {
                    e.Label = "Incongruent";
                    e.Color = SKColor.Parse("#77d065");
                }
                es.Add(e);
            }
            return es;
        }

        private List<ChartEntry> GetAvgCorPctByStimType()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            foreach (Tuple<bool, double> rec in AvgCorPctByStimType)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2 * 100, 1).ToString() + "%";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == true)
                {
                    e.Label = "Congruent";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else
                {
                    e.Label = "Incongruent";
                    e.Color = SKColor.Parse("#77d065");
                }
                es.Add(e);
            }
            return es;
        }

    }
}
