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
using System.Runtime.CompilerServices;
using MathNet.Numerics;

namespace BrainGames.ViewModels
{
    public class DSStatsViewModel : ViewModelBase
    {
        private List<DataSchemas.DSGameRecordSchema> ur = new List<DataSchemas.DSGameRecordSchema>();

        private List<Tuple<DateTime, double>> TrialsByDay;
        private List<Tuple<DateTime, double>> TrialsByWeek;
        private List<Tuple<DateTime, double>> TrialsByMonth;
        private List<Tuple<DateTime, double>> TrialCountOverTime;

        private List<Tuple<DateTime, double>> MaxCorFwdSpanLenByDay;
        private List<Tuple<DateTime, double>> MaxCorFwdSpanLenByWeek;
        private List<Tuple<DateTime, double>> MaxCorFwdSpanLenByMonth;
        private List<Tuple<DateTime, double>> MaxCorFwdSpanLenOverTime;

        private List<Tuple<DateTime, double>> MaxCorBwdSpanLenByDay;
        private List<Tuple<DateTime, double>> MaxCorBwdSpanLenByWeek;
        private List<Tuple<DateTime, double>> MaxCorBwdSpanLenByMonth;
        private List<Tuple<DateTime, double>> MaxCorBwdSpanLenOverTime;

        private List<Tuple<DateTime, double>> CumEstSpanFByDay;
        private List<Tuple<DateTime, double>> CumEstSpanFByWeek;
        private List<Tuple<DateTime, double>> CumEstSpanFByMonth;
        private List<Tuple<DateTime, double>> CumEstSpanFOverTime;
        private List<Tuple<DateTime, double>> CumEstSpanBByDay;
        private List<Tuple<DateTime, double>> CumEstSpanBByWeek;
        private List<Tuple<DateTime, double>> CumEstSpanBByMonth;
        private List<Tuple<DateTime, double>> CumEstSpanBOverTime;

        private List<Tuple<DateTime, double>> CumEstStimTimeFByDay;
        private List<Tuple<DateTime, double>> CumEstStimTimeFByWeek;
        private List<Tuple<DateTime, double>> CumEstStimTimeFByMonth;
        private List<Tuple<DateTime, double>> CumEstStimTimeFOverTime;
        private List<Tuple<DateTime, double>> CumEstStimTimeBByDay;
        private List<Tuple<DateTime, double>> CumEstStimTimeBByWeek;
        private List<Tuple<DateTime, double>> CumEstStimTimeBByMonth;
        private List<Tuple<DateTime, double>> CumEstStimTimeBOverTime;

        private IEnumerable<Tuple<int, double>> AvgCorOnTimeBySpanLen_f;
        private IEnumerable<Tuple<int, double>> AvgCorOffTimeBySpanLen_f;
        private IEnumerable<Tuple<int, double>> AvgCorOnTimeBySpanLen_b;
        private IEnumerable<Tuple<int, double>> AvgCorOffTimeBySpanLen_b;
        private IEnumerable<Tuple<int, double>> AvgCorPctBySpanLen_f;
        private IEnumerable<Tuple<int, double>> AvgCorPctBySpanLen_b;

        private IEnumerable<Tuple<int, double>> FastestCorStimTimeBySpanLen_f;
        private IEnumerable<Tuple<int, double>> FastestCorStimTimeBySpanLen_b;

        private double estSpan_f, estStimTime_f, estSpan_b, estStimTime_b;
        private int longestcorspan_f, fastestcorstimtimeatlongestcorspan_f, fastestcorstimtime_f, longestcorspanatfastestcorstimetime_f;
        private int longestcorspan_b, fastestcorstimtimeatlongestcorspan_b, fastestcorstimtime_b, longestcorspanatfastestcorstimetime_b;
        private int tdy_longestcorspan_f, tdy_fastestcorstimtimeatlongestcorspan_f, tdy_fastestcorstimtime_f, tdy_longestcorspanatfastestcorstimetime_f;
        private int tdy_longestcorspan_b, tdy_fastestcorstimtimeatlongestcorspan_b, tdy_fastestcorstimtime_b, tdy_longestcorspanatfastestcorstimetime_b;
        private double sf = 0.03;

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

        public DSStatsViewModel()
        {
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.DSGameRecordSchema>("select * from DSGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                #region get est spans and times
                List<bool> corarr = ur.Where(x => x.direction == "f").Select(x => x.cor).ToList();
                List<int> spanlenarr = ur.Where(x => x.direction == "f").Select(x => x.itemcnt).ToList();
                Tuple<double, double> p;
                List<Tuple<int, double>> AvgCorStatsBySpan;

                try { p = Fit.Line(spanlenarr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "f").GroupBy(x => x.itemcnt).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estSpan_f = 0.0;
                    }
                    else
                    {
                        estSpan_f = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estSpan_f = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? spanlenarr.Max() : 0) : (0.9 - p.Item1) / p.Item2;
                }

                corarr = ur.Where(x => x.itemcnt <= estSpan_f && x.direction == "f").Select(x => x.cor).ToList();
                List<int> stimtimearr = ur.Where(x => x.itemcnt <= estSpan_f && x.direction == "f").Select(x => x.ontimems + x.offtimems).ToList();
                try { p = Fit.Line(stimtimearr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "f").GroupBy(x => x.ontimems + x.offtimems).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estStimTime_f = 0.0;
                    }
                    else
                    {
                        estStimTime_f = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estStimTime_f = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? stimtimearr.Min() : 0) : (0.9 - p.Item1) / p.Item2;
                }
                corarr = ur.Where(x => x.direction == "b").Select(x => x.cor).ToList();
                spanlenarr = ur.Where(x => x.direction == "b").Select(x => x.itemcnt).ToList();
                try { p = Fit.Line(spanlenarr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "b").GroupBy(x => x.itemcnt).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estSpan_b = 0.0;
                    }
                    else
                    {
                        estSpan_b = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estSpan_b = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? spanlenarr.Max() : 0) : (0.9 - p.Item1) / p.Item2;
                }

                corarr = ur.Where(x => x.itemcnt <= estSpan_b && x.direction == "b").Select(x => x.cor).ToList();
                stimtimearr = ur.Where(x => x.itemcnt <= estSpan_b && x.direction == "b").Select(x => x.ontimems + x.offtimems).ToList();
                try { p = Fit.Line(stimtimearr.Select(Convert.ToDouble).ToArray(), corarr.Select(Convert.ToDouble).ToArray()); }
                catch (Exception ex) { p = Tuple.Create<double, double>(0.0, 0.0); }

                if (p.Item2 >= 0 || p.Item1 <= 0.9)
                {
                    AvgCorStatsBySpan = ur.Where(x => x.direction == "b").GroupBy(x => x.ontimems + x.offtimems).Where(grp => grp.Count() >= 3).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1).ToList();
                    if (AvgCorStatsBySpan.Count() == 0 || AvgCorStatsBySpan.Select(x => x.Item2).Max() < 0.9)
                    {
                        estStimTime_b = 0.0;
                    }
                    else
                    {
                        estStimTime_b = AvgCorStatsBySpan.Where(x => x.Item2 == AvgCorStatsBySpan.Select(y => y.Item2).Max()).Select(x => x.Item1).Last();
                    }
                }
                else
                {
                    estStimTime_b = p.Item2 == 0 ? ((corarr.Count() > 0 && corarr[0] == true) ? stimtimearr.Min() : 0) : (0.9 - p.Item1) / p.Item2;
                }
                #endregion

                try
                {
                    longestcorspan_f = ur.Where(x => x.cor == true && x.direction == "f").Select(x => x.itemcnt).Max();
                    fastestcorstimtimeatlongestcorspan_f = ur.Where(x => x.itemcnt == longestcorspan_f && x.cor == true && x.direction == "f").Select(x => x.ontimems + x.offtimems).Min();
                }
                catch (Exception ex) { longestcorspan_f = 0; fastestcorstimtimeatlongestcorspan_f = 0; }
                try
                {
                    fastestcorstimtime_f = ur.Where(x => x.cor == true && x.direction == "f").Select(x => x.ontimems + x.offtimems).Min();
                    longestcorspanatfastestcorstimetime_f = ur.Where(x => x.ontimems + x.offtimems == fastestcorstimtime_f && x.cor == true && x.direction == "f").Select(x => x.itemcnt).Max();
                }
                catch (Exception ex) { fastestcorstimtime_f = 0; longestcorspanatfastestcorstimetime_f = 0; }
                try
                {
                    longestcorspan_b = ur.Where(x => x.cor == true && x.direction == "b").Select(x => x.itemcnt).Max();
                    fastestcorstimtimeatlongestcorspan_b = ur.Where(x => x.itemcnt == longestcorspan_b && x.cor == true && x.direction == "b").Select(x => x.ontimems + x.offtimems).Min();
                }
                catch (Exception ex) { longestcorspan_b = 0; fastestcorstimtimeatlongestcorspan_b = 0; }
                try
                {
                    fastestcorstimtime_b = ur.Where(x => x.cor == true && x.direction == "b").Select(x => x.ontimems + x.offtimems).Min();
                    longestcorspanatfastestcorstimetime_b = ur.Where(x => x.ontimems + x.offtimems == fastestcorstimtime_b && x.cor == true && x.direction == "b").Select(x => x.itemcnt).Max();
                }
                catch (Exception ex) { fastestcorstimtime_b = 0; longestcorspanatfastestcorstimetime_b = 0; }

                try
                { 
                    tdy_longestcorspan_f = ur.Where(x => x.cor == true && x.direction == "f" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.itemcnt).Max();
                    tdy_fastestcorstimtimeatlongestcorspan_f = ur.Where(x => x.itemcnt == tdy_longestcorspan_f && x.cor == true && x.direction == "f" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.ontimems + x.offtimems).Min();
                }
                catch (Exception ex) { tdy_longestcorspan_f = 0; tdy_fastestcorstimtimeatlongestcorspan_f = 0; }
                try
                {
                    tdy_fastestcorstimtime_f = ur.Where(x => x.cor == true && x.direction == "f" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.ontimems + x.offtimems).Min();
                    tdy_longestcorspanatfastestcorstimetime_f = ur.Where(x => x.ontimems + x.offtimems == tdy_fastestcorstimtime_f && x.cor == true && x.direction == "f" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.itemcnt).Max();
                }
                catch (Exception ex) { tdy_fastestcorstimtime_f = 0; tdy_longestcorspanatfastestcorstimetime_f = 0; }
                try
                {
                    tdy_longestcorspan_b = ur.Where(x => x.cor == true && x.direction == "b" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.itemcnt).Max();
                    tdy_fastestcorstimtimeatlongestcorspan_b = ur.Where(x => x.itemcnt == tdy_longestcorspan_b && x.cor == true && x.direction == "b" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.ontimems + x.offtimems).Min();
                }
                catch (Exception ex) { tdy_longestcorspan_b = 0; tdy_fastestcorstimtimeatlongestcorspan_b = 0; }
                try
                {
                    tdy_fastestcorstimtime_b = ur.Where(x => x.cor == true && x.direction == "b" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.ontimems + x.offtimems).Min();
                    tdy_longestcorspanatfastestcorstimetime_b = ur.Where(x => x.ontimems + x.offtimems == tdy_fastestcorstimtime_b && x.cor == true && x.direction == "b" && DateTime.Parse(x.datetime).Date == DateTime.Today).Select(x => x.itemcnt).Max();
                }
                catch (Exception ex) { tdy_fastestcorstimtime_b = 0; tdy_longestcorspanatfastestcorstimetime_b = 0; }

                CumEstSpanFByDay = ur.Where(x => x.estSpan_f > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estSpan_f) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstSpanFByWeek = ur.Where(x => x.estSpan_f > 0).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estSpan_f) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstSpanFByMonth = ur.Where(x => x.estSpan_f > 0).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estSpan_f) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstSpanBByDay = ur.Where(x => x.estSpan_b > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estSpan_b) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstSpanBByWeek = ur.Where(x => x.estSpan_b > 0).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estSpan_b) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstSpanBByMonth = ur.Where(x => x.estSpan_b > 0).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estSpan_b) / x.Count())).OrderBy(x => x.Item1).ToList();

                CumEstStimTimeFByDay = ur.Where(x => x.estStimTime_f > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estStimTime_f) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstStimTimeFByWeek = ur.Where(x => x.estStimTime_f > 0).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estStimTime_f) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstStimTimeFByMonth = ur.Where(x => x.estStimTime_f > 0).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estStimTime_f) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstStimTimeBByDay = ur.Where(x => x.estStimTime_b > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estStimTime_b) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstStimTimeBByWeek = ur.Where(x => x.estStimTime_b > 0).GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estStimTime_b) / x.Count())).OrderBy(x => x.Item1).ToList();
                CumEstStimTimeBByMonth = ur.Where(x => x.estStimTime_b > 0).GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estStimTime_b) / x.Count())).OrderBy(x => x.Item1).ToList();

                TrialsByDay = ur.GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1).ToList();
                TrialsByWeek = ur.GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1).ToList();
                TrialsByMonth = ur.GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (double)x.Count())).OrderBy(x => x.Item1).ToList();

                MaxCorFwdSpanLenByDay = ur.Where(x => x.cor == true && x.direction == "f").GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Select(y => y.itemcnt).Max())).OrderBy(x => x.Item1).ToList();
                MaxCorFwdSpanLenByWeek = ur.Where(x => x.cor == true && x.direction == "f").GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Select(y => y.itemcnt).Max())).OrderBy(x => x.Item1).ToList();
                MaxCorFwdSpanLenByMonth = ur.Where(x => x.cor == true && x.direction == "f").GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (double)x.Select(y => y.itemcnt).Max())).OrderBy(x => x.Item1).ToList();

                MaxCorBwdSpanLenByDay = ur.Where(x => x.cor == true && x.direction == "b").GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (double)x.Select(y => y.itemcnt).Max())).OrderBy(x => x.Item1).ToList();
                MaxCorBwdSpanLenByWeek = ur.Where(x => x.cor == true && x.direction == "b").GroupBy(x => DateTime.Parse(x.datetime).StartOfWeek(DayOfWeek.Monday)).Select(x => Tuple.Create(x.Key, (double)x.Select(y => y.itemcnt).Max())).OrderBy(x => x.Item1).ToList();
                MaxCorBwdSpanLenByMonth = ur.Where(x => x.cor == true && x.direction == "b").GroupBy(x => DateTime.Parse(new DateTime(DateTime.Parse(x.datetime).Year, DateTime.Parse(x.datetime).Month, 1).ToString())).Select(x => Tuple.Create(x.Key, (double)x.Select(y => y.itemcnt).Max())).OrderBy(x => x.Item1).ToList();

                AvgCorOnTimeBySpanLen_f = ur.Where(x => x.cor == true && x.direction == "f").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Select(y => y.ontimems).Average())).OrderBy(x => x.Item1);
                AvgCorOffTimeBySpanLen_f = ur.Where(x => x.cor == true && x.direction == "f").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Select(y => y.offtimems).Average())).OrderBy(x => x.Item1);
                AvgCorOnTimeBySpanLen_b = ur.Where(x => x.cor == true && x.direction == "b").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Select(y => y.ontimems).Average())).OrderBy(x => x.Item1);
                AvgCorOffTimeBySpanLen_b = ur.Where(x => x.cor == true && x.direction == "b").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Select(y => y.offtimems).Average())).OrderBy(x => x.Item1);

                AvgCorPctBySpanLen_f = ur.Where(x => x.direction == "f").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1);
                AvgCorPctBySpanLen_b = ur.Where(x => x.direction == "b").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Where(y => y.cor == true).Count() / (double)Math.Max(x.Count(), 1))).OrderBy(x => x.Item1);

                FastestCorStimTimeBySpanLen_f = ur.Where(x => x.cor == true && x.direction == "f").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Select(y => (double)y.ontimems+y.offtimems).Min())).OrderBy(x => x.Item1);
                FastestCorStimTimeBySpanLen_b = ur.Where(x => x.cor == true && x.direction == "b").GroupBy(x => x.itemcnt).Select(x => Tuple.Create(x.Key, x.Select(y => (double)y.ontimems + y.offtimems).Min())).OrderBy(x => x.Item1);

                TrialCountOverTime = FillTimeList(TrialsByDay, TrialsByWeek, TrialsByMonth);
                MaxCorFwdSpanLenOverTime = MaxCorFwdSpanLenByDay.Count() <= 30 ? MaxCorFwdSpanLenByDay : (MaxCorFwdSpanLenByWeek.Count() <= 30 ? MaxCorFwdSpanLenByWeek : MaxCorFwdSpanLenByMonth);
                MaxCorBwdSpanLenOverTime = MaxCorBwdSpanLenByDay.Count() <= 30 ? MaxCorBwdSpanLenByDay : (MaxCorBwdSpanLenByWeek.Count() <= 30 ? MaxCorBwdSpanLenByWeek : MaxCorBwdSpanLenByMonth);
                CumEstSpanFOverTime = CumEstSpanFByDay.Count() <= 30 ? CumEstSpanFByDay : (CumEstSpanFByWeek.Count() <= 30 ? CumEstSpanFByWeek : CumEstSpanFByMonth);
                CumEstSpanBOverTime = CumEstSpanBByDay.Count() <= 30 ? CumEstSpanBByDay : (CumEstSpanBByWeek.Count() <= 30 ? CumEstSpanBByWeek : CumEstSpanBByMonth);
                CumEstStimTimeFOverTime = CumEstStimTimeFByDay.Count() <= 30 ? CumEstStimTimeFByDay : (CumEstStimTimeFByWeek.Count() <= 30 ? CumEstStimTimeFByWeek : CumEstStimTimeFByMonth);
                CumEstStimTimeBOverTime = CumEstStimTimeBByDay.Count() <= 30 ? CumEstStimTimeBByDay : (CumEstStimTimeBByWeek.Count() <= 30 ? CumEstStimTimeBByWeek : CumEstStimTimeBByMonth);
            }
        }

        public Chart BestSpanDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestDays(tdy_longestcorspan_f, longestcorspan_f, tdy_longestcorspan_b, longestcorspan_b, ""),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart BestTimeAtBestSpanDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestDays(tdy_fastestcorstimtimeatlongestcorspan_f, fastestcorstimtimeatlongestcorspan_f, tdy_fastestcorstimtimeatlongestcorspan_b, fastestcorstimtimeatlongestcorspan_b, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart BestTimeDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestDays(tdy_fastestcorstimtime_f, fastestcorstimtime_f, tdy_fastestcorstimtime_b, fastestcorstimtime_b, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart BestSpanAtBestTimeDaysChart => new BarChart()
        {
            Margin = 10,
            Entries = GetBestDays(tdy_longestcorspanatfastestcorstimetime_f, longestcorspanatfastestcorstimetime_f, tdy_longestcorspanatfastestcorstimetime_b, longestcorspanatfastestcorstimetime_b, ""),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart EstSpanChart => new BarChart()
        {
            Margin = 10,
            Entries = GetEstimates(estSpan_f, estSpan_b, ""),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart EstStimTimeChart => new BarChart()
        {
            Margin = 10,
            Entries = GetEstimates(estStimTime_f, estStimTime_b, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart CumEstSpanFOverTimeChart => new LineChart()
        {
            Entries = CumEstSpanFOverTime.Select(CreateDayEntry),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = CumEstSpanFOverTime.Count() == 0 ? 0 : (float)(CumEstSpanFOverTime.Min(x => x.Item2) - CumEstSpanFOverTime.Min(x => x.Item2) * sf),
            MaxValue = CumEstSpanFOverTime.Count() == 0 ? 0 : (float)(CumEstSpanFOverTime.Max(x => x.Item2) + CumEstSpanFOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumEstSpanBOverTimeChart => new LineChart()
        {
            Entries = CumEstSpanBOverTime.Select(CreateDayEntry),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = CumEstSpanBOverTime.Count() == 0 ? 0 : (float)(CumEstSpanBOverTime.Min(x => x.Item2) - CumEstSpanBOverTime.Min(x => x.Item2) * sf),
            MaxValue = CumEstSpanBOverTime.Count() == 0 ? 0 : (float)(CumEstSpanBOverTime.Max(x => x.Item2) + CumEstSpanBOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumEstStimTimeFOverTimeChart => new LineChart()
        {
            Entries = CumEstStimTimeFOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = CumEstStimTimeFOverTime.Count() == 0 ? 0 : (float)(CumEstStimTimeFOverTime.Min(x => x.Item2) - CumEstStimTimeFOverTime.Min(x => x.Item2) * sf),
            MaxValue = CumEstStimTimeFOverTime.Count() == 0 ? 0 : (float)(CumEstStimTimeFOverTime.Max(x => x.Item2) + CumEstStimTimeFOverTime.Max(x => x.Item2) * sf)
        };

        public Chart CumEstStimTimeBOverTimeChart => new LineChart()
        {
            Entries = CumEstStimTimeBOverTime.Select(CreateDayEntryMS),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18,
            MinValue = CumEstStimTimeBOverTime.Count() == 0 ? 0 : (float)(CumEstStimTimeBOverTime.Min(x => x.Item2) - CumEstStimTimeBOverTime.Min(x => x.Item2) * sf),
            MaxValue = CumEstStimTimeBOverTime.Count() == 0 ? 0 : (float)(CumEstStimTimeBOverTime.Max(x => x.Item2) + CumEstStimTimeBOverTime.Max(x => x.Item2) * sf)
        };
        public Chart MaxCorFwdSpanLenOverTimeChart => new LineChart()
        {
            Entries = MaxCorFwdSpanLenOverTime.Select(CreateDayEntryCnt),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18
        };

        public Chart MaxCorBwdSpanLenOverTimeChart => new LineChart()
        {
            Entries = MaxCorBwdSpanLenOverTime.Select(CreateDayEntryCnt),
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Circle,
            PointSize = 18
        };
        
 /*
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
        */

        public Chart TrialCountOverTimeChart => new BarChart()
        {
            Entries = TrialCountOverTime.Select(CreateDayEntryCnt),
            Margin = 10,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorPctBySpanLen_fChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(AvgCorPctBySpanLen_f, "%"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorPctBySpanLen_bChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(AvgCorPctBySpanLen_b, "%"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorOnTimeBySpanLen_fChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(AvgCorOnTimeBySpanLen_f, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorOffTimeBySpanLen_fChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(AvgCorOffTimeBySpanLen_f, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorOnTimeBySpanLen_bChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(AvgCorOnTimeBySpanLen_b, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCorOffTimeBySpanLen_bChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(AvgCorOffTimeBySpanLen_b, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart FastestCorStimTimeBySpanLen_fChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(FastestCorStimTimeBySpanLen_f, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart FastestCorStimTimeBySpanLen_bChart => new BarChart()
        {
            Margin = 10,
            Entries = GetStatsBySpanLen(FastestCorStimTimeBySpanLen_b, " ms"),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };



        private List<ChartEntry> GetBestDays(int tf, int f, int tb, int b, string suf)
        {
            List<ChartEntry> es = new List<ChartEntry>();
            if (tf > 0)
            {
                ChartEntry e = new ChartEntry((float)tf);
                e.ValueLabel = tf.ToString() + suf;
                e.TextColor = SKColors.Black;
                e.Label = "Today: Fwd";
                e.Color = SKColor.Parse("#2c3e50");
                es.Add(e);
            }
            if (f > 0)
            {
                ChartEntry e = new ChartEntry((float)f);
                e.ValueLabel = f.ToString() + suf;
                e.TextColor = SKColors.Black;
                e.Label = "Best: Fwd";
                e.Color = SKColor.Parse("#77d065");
                es.Add(e);
            }
            if (tb > 0)
            {
                ChartEntry e = new ChartEntry((float)tb);
                e.ValueLabel = tb.ToString() + suf;
                e.TextColor = SKColors.Black;
                e.Label = "Today: Bkwd";
                e.Color = SKColor.Parse("#b455b6");
                es.Add(e);
            }
            if (b > 0)
            {
                ChartEntry e = new ChartEntry((float)b);
                e.ValueLabel = b.ToString() + suf;
                e.TextColor = SKColors.Black;
                e.Label = "Best: Bkwd";
                e.Color = SKColor.Parse("#3498db");
                es.Add(e);
            }
            return es;
        }

        private List<ChartEntry> GetEstimates(double f, double b, string suf)
        {
            List<ChartEntry> es = new List<ChartEntry>();
            if (f > 0)
            {
                ChartEntry e = new ChartEntry((float)f);
                e.ValueLabel = Math.Round(f, 1).ToString() + suf;
                e.TextColor = SKColors.Black;
                e.Label = "Forward";
                e.Color = SKColor.Parse("#2c3e50");
                es.Add(e);
            }
            if (b > 0)
            {
                ChartEntry e = new ChartEntry((float)b);
                e.ValueLabel = Math.Round(b, 1).ToString() + suf;
                e.TextColor = SKColors.Black;
                e.Label = "Backward";
                e.Color = SKColor.Parse("#77d065");
                es.Add(e);
            }
            return es;
        }

        private ChartEntry CreateDayEntry(Tuple<DateTime, double> dailyitem)
        {
            var label = dailyitem.Item1.ToString("M/dd");
            var color = SKColors.Blue;
            var textcolor = SKColors.Black;
            return new ChartEntry((float)dailyitem.Item2)
            {
                ValueLabel = Math.Round(dailyitem.Item2, 1).ToString(),
                TextColor = textcolor,
                Label = label,
                Color = color
            };
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

        private List<ChartEntry> GetStatsBySpanLen(IEnumerable<Tuple<int, double>> v, string suf)
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 0;
            List<ChartEntry> es = new List<ChartEntry>();
            foreach (Tuple<int, double> rec in v)
            {
                ChartEntry e = new ChartEntry((float)rec.Item2);
                e.ValueLabel = Math.Round(rec.Item2 * (suf == "%" ? 100 : 1), 1).ToString() + suf;
                e.TextColor = SKColors.Black;
                e.Label = rec.Item1.ToString();
                e.Color = clrs[idx];
                idx = idx == clrs.Count() - 1 ? 0 : idx + 1;
                es.Add(e);
            }
            return es;
        }

    }
}
