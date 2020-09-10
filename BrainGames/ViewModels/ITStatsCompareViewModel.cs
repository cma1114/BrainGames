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
    public class ITStatsCompareViewModel : ViewModelBase
    {
        private List<DataSchemas.ITGameRecordSchema> ur = new List<DataSchemas.ITGameRecordSchema>();
        private double cumavg, cumit;

        public ITStatsCompareViewModel()
        {
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.ITGameRecordSchema>("select * from ITGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                cumavg = ur.Where(x => x.avgcorit > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.avgcorit) / x.Count())).OrderBy(x => x.Item1).Last().Item2;
                cumit = ur.Where(x => x.estit > 0).GroupBy(x => DateTime.Parse(x.datetime).Date).Select(x => Tuple.Create(x.Key, x.Sum(y => y.estit) / x.Count())).OrderBy(x => x.Item1).Last().Item2;
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
                e = new ChartEntry((float)cumavg);
                e.ValueLabel = Math.Round(cumavg, 1).ToString() + " ms";
            }
            else
            {
                e = new ChartEntry((float)cumit);
                e.ValueLabel = Math.Round(cumit, 1).ToString() + " ms";
            }
            e.TextColor = SKColors.Black;
            e.Label = "Me";
            e.Color = clrs[0];
            es.Add(e);

            List<GameShare> itgsl = new List<GameShare>();
            itgsl = App.mum.GameShares.Where(x => x.game == "IT").ToList();
            foreach (GameShare gs in itgsl)
            {
                if (avg)
                {
                    e = new ChartEntry((float)gs.avgscore[0]);
                    e.ValueLabel = Math.Round(gs.avgscore[0], 1).ToString() + " ms";
                }
                else
                {
                    e = new ChartEntry((float)gs.avgscore[1]);
                    e.ValueLabel = Math.Round(gs.avgscore[1], 1).ToString() + " ms";
                }
                e.TextColor = SKColors.Black;
                e.Label = gs.Screenname;
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }

    }
}
