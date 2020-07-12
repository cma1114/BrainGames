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
    enum timeblock { Day, Week, Month }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }
    }

    public class ITStatsViewModel : ViewModelBase
    {

        private List<DataSchemas.ITGameRecordSchema> ur = new List<DataSchemas.ITGameRecordSchema>();

        private IEnumerable<Tuple<DateTime, double>> TrialsByDay;
        private IEnumerable<Tuple<DateTime, double>> TrialsByWeek;
        private IEnumerable<Tuple<DateTime, double>> TrialsByMonth;
        private IEnumerable<Tuple<int, double>> AvgCorITByStimType;
        private IEnumerable<Tuple<int, double>> AvgCorPctByStimType;
        private IEnumerable<Tuple<double, double>> AvgCorPctByStimDur;
        private IEnumerable<Tuple<DateTime, double>> AvgCorITByDay;
        private IEnumerable<Tuple<DateTime, double>> CumAvgCorITByDay;
        private IEnumerable<double> CumAvgCorITByTrial;
        private IEnumerable<Tuple<DateTime, double>> CumEstITByDay;
        private IEnumerable<double> CumEstITByTrial;
        private IEnumerable<Tuple<DateTime, double>> AvgCorITByWeek;
        private IEnumerable<Tuple<DateTime, double>> CumAvgCorITByWeek;
        private IEnumerable<Tuple<DateTime, double>> CumEstITByWeek;
        private IEnumerable<Tuple<DateTime, double>> AvgCorITByMonth;
        private IEnumerable<Tuple<DateTime, double>> CumAvgCorITByMonth;
        private IEnumerable<Tuple<DateTime, double>> CumEstITByMonth;
        private List<Tuple<DateTime, double>> TrialCountOverTime;
        private List<Tuple<DateTime, double>> AvgCorITOverTime;
        private List<Tuple<DateTime, double>> CumAvgCorITOverTime;
        private List<Tuple<DateTime, double>> CumEstITOverTime;

        private double sf = 0.03;

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

        public ITStatsViewModel()
        {
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.ITGameRecordSchema>("select * from ITGameRecordSchema"); }
            catch {; }
            if (ur != null && ur.Count() > 0)
            {
                TrialsByDay = ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                TrialsByWeek = ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                TrialsByMonth = ur.GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1);
                AvgCorITByDay = ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.empstimtime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                AvgCorITByStimType = ur.GroupBy(x => x.stimtype).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.empstimtime)/x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                AvgCorPctByStimType = ur.GroupBy(x => x.stimtype).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Count())).OrderBy(x => x.Item1);
                AvgCorPctByStimDur = ur.GroupBy(x => x.stimtime).Select(x => Tuple.Create(x.Key, (double)x.Where(y => y.cor == true).Count() / x.Count())).OrderByDescending(x => x.Item1);
                CumAvgCorITByDay = ur.Where(x => x.avgcorit > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgcorit)/x.Count())).OrderBy(x => x.Item1);
                CumAvgCorITByTrial = ur.Where(x => x.avgcorit > 0).OrderByDescending(x => x.datetime).Select(x => x.avgcorit).Take(30);
                CumEstITByDay = ur.Where(x => x.estit > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estit) / x.Count())).OrderBy(x => x.Item1);
                CumEstITByTrial = ur.OrderByDescending(x => x.datetime).Where(x => x.estit > 0).Select(x => x.estit).Take(30);
                AvgCorITByWeek = ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.empstimtime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                CumAvgCorITByWeek = ur.Where(x => x.avgcorit > 0).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgcorit) / x.Count())).OrderBy(x => x.Item1);
                CumEstITByWeek = ur.Where(x => x.estit > 0).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estit) / x.Count())).OrderBy(x => x.Item1);
                AvgCorITByMonth = ur.GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Sum(y => y.empstimtime) / x.Where(y => y.cor == true).Count())).OrderBy(x => x.Item1);
                CumAvgCorITByMonth = ur.Where(x => x.avgcorit > 0).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgcorit) / x.Count())).OrderBy(x => x.Item1);
                CumEstITByMonth = ur.Where(x => x.estit > 0).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estit) / x.Count())).OrderBy(x => x.Item1);

                AvgCorITOverTime = FillTimeList(AvgCorITByDay, AvgCorITByWeek, AvgCorITByMonth);
                CumAvgCorITOverTime = CumAvgCorITByDay.Count() <= 30 ? CumAvgCorITByDay.ToList() : (CumAvgCorITByWeek.Count() <= 30 ? CumAvgCorITByWeek.ToList() : CumAvgCorITByMonth.ToList());//FillTimeList(CumAvgCorITByDay, CumAvgCorITByWeek, CumAvgCorITByMonth);
                CumEstITOverTime = CumEstITByDay.Count() <= 30 ? CumEstITByDay.ToList() : (CumEstITByWeek.Count() <= 30 ? CumEstITByWeek.ToList() : CumEstITByMonth.ToList());
                TrialCountOverTime = FillTimeList(TrialsByDay, TrialsByWeek, TrialsByMonth);
            }
        }

        private ChartEntry CreateDayEntryMS(Tuple<DateTime, double> dailyitem)
        {
            var label = dailyitem.Item1.ToString("M/dd");
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new ChartEntry((float)dailyitem.Item2)
            { 
                ValueLabel = Math.Round(dailyitem.Item2,1).ToString() + " ms",
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

        public Chart AvgCorITOverTimeChart => new LineChart()
        {
            Entries = AvgCorITOverTime.Where(x => x.Item2 > 0).Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, AvgCorITOverTime.Min(x => x.Item2) - AvgCorITOverTime.Min(x => x.Item2) * sf),
            MaxValue = (float)(AvgCorITOverTime.Max(x => x.Item2) + AvgCorITOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumAvgCorITOverTimeChart => new LineChart()
        {
            Entries = CumAvgCorITOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, CumAvgCorITOverTime.Min(x => x.Item2) - CumAvgCorITOverTime.Min(x => x.Item2) * sf),
            MaxValue = (float)(CumAvgCorITOverTime.Max(x => x.Item2) + CumAvgCorITOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumEstITOverTimeChart => new LineChart()
        {
            Entries = CumEstITOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, CumEstITOverTime.Min(x => x.Item2) - CumEstITOverTime.Min(x => x.Item2) * sf),
            MaxValue = (float)(CumEstITOverTime.Max(x => x.Item2) + CumEstITOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumAvgCorITByTrialChart => new LineChart()
        {
            Entries = CumAvgCorITByTrial.Reverse().Select(CreateEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, CumAvgCorITByTrial.Min() - CumAvgCorITByTrial.Min() * sf),
            MaxValue = (float)(CumAvgCorITByTrial.Max() + CumAvgCorITByTrial.Max() * sf)
        };

        public Chart CumEstITByTrialChart => new LineChart()
        {
            Entries = CumEstITByTrial.Reverse().Select(CreateEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = (float)Math.Max(0, CumEstITByTrial.Min() - CumEstITByTrial.Min() * sf),
            MaxValue = (float)(CumEstITByTrial.Max() + CumEstITByTrial.Max() * sf)
        };

        private List<ChartEntry> GetBestAvgCorITDays()
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            if (AvgCorITByDay.Last().Item1 == DateTime.Today) 
            {
                ChartEntry e = new ChartEntry((float)AvgCorITByDay.Last().Item2);
                e.ValueLabel = Math.Round(AvgCorITByDay.Last().Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                e.Label = "Today";
                e.Color = clrs[0];
                es.Add(e);
            }
            var v = AvgCorITByDay.Where(x => x.Item2 > 0).OrderBy(x => x.Item2).Take(5);
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


        public Chart BestAvgCorITDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestAvgCorITDays(),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
        };

        private List<ChartEntry> GetAvgCorITByStimType()
        {
            List<ChartEntry> es = new List<ChartEntry>();
            foreach (Tuple<int, double> rec in AvgCorITByStimType)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2, 1).ToString() + " ms";
                e.TextColor = SKColors.Black;
                if (rec.Item1 == (int)ITViewModel.answertype.left)
                {
                    e.Label = "Left longer";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else
                {
                    e.Label = "Right longer";
                    e.Color = SKColor.Parse("#77d065");
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
                if (rec.Item1 == (int)ITViewModel.answertype.left)
                {
                    e.Label = "Left longer";
                    e.Color = SKColor.Parse("#2c3e50");
                }
                else
                {
                    e.Label = "Right longer";
                    e.Color = SKColor.Parse("#77d065");
                }
                es.Add(e);
            }
            return es;
        }

        private List<ChartEntry> GetAvgCorPctByStimDur()
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown, 
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            List<ChartEntry> es = new List<ChartEntry>();
            int idx = 0;
            foreach (Tuple<double, double> rec in AvgCorPctByStimDur)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2 * 100, 1).ToString() + "%";
                e.TextColor = SKColors.Black;
                e.Label = ((int)rec.Item1).ToString() + " ms";
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }

        public Chart AvgCorITByStimTypeChart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorITByStimType(),
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

        public Chart AvgCorPctByStimDurChart => new BarChart()
        {
            Margin = 10,
            Entries = GetAvgCorPctByStimDur(),
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
