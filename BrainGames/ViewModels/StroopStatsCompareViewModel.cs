using System;
using System.Collections.Generic;
using System.Text;
using Microcharts;
using SkiaSharp;
using System.Linq;
using BrainGames.Utility;
using BrainGames.Models;

namespace BrainGames.ViewModels
{
    public class StroopStatsCompareViewModel : ViewModelBase
    {
        private List<DataSchemas.StroopGameRecordSchema> ur = new List<DataSchemas.StroopGameRecordSchema>();
        private double cumavgrt, cumavgcidif;

        public StroopStatsCompareViewModel()
        {
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.StroopGameRecordSchema>("select * from StroopGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                cumavgrt = Utilities.MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, ((x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()) + (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count())) / 2)).OrderBy(x => x.Item1).ToList(), 1).Last().Item2;
                cumavgcidif = Utilities.MovingAverage(ur.Where(x => x.cor == true).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, (x.Where(y => y.congruent == false).Sum(y => y.reactiontime) / x.Where(y => y.congruent == false).Count()) - (x.Where(y => y.congruent == true).Sum(y => y.reactiontime) / x.Where(y => y.congruent == true).Count()))).OrderBy(x => x.Item1).ToList(), 1).Last().Item2;
            }
        }

        private List<ChartEntry> GetCompetitors(bool avg)
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            ChartEntry e;
            if (avg)
            {
                e = new ChartEntry((float)cumavgrt);
                e.ValueLabel = Math.Round(cumavgrt, 1).ToString() + " ms";
            }
            else
            {
                e = new ChartEntry((float)cumavgcidif);
                e.ValueLabel = Math.Round(cumavgcidif, 1).ToString() + " ms";
            }
            e.TextColor = SKColors.Black;
            e.Label = "Me";
            e.Color = clrs[0];
            es.Add(e);

            List<GameShare> sgsl = new List<GameShare>();
            sgsl = App.mum.GameShares.Where(x => x.game == "Stroop").ToList();
            foreach (GameShare gs in sgsl)
            {
                if (avg)
                {
                    e = new ChartEntry((float)gs.avgscores[0]);
                    e.ValueLabel = Math.Round(gs.avgscores[0], 1).ToString() + " ms";
                }
                else
                {
                    e = new ChartEntry((float)gs.avgscores[1]);
                    e.ValueLabel = Math.Round(gs.avgscores[1], 1).ToString() + " ms";
                }
                e.TextColor = SKColors.Black;
                e.Label = gs.Screenname;
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }

        public Chart AvgCorRTChart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(true),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart AvgCIDifChart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(false),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

    }
}
