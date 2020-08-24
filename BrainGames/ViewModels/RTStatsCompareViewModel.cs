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
    public class RTStatsCompareViewModel : ViewModelBase
    {
        private List<DataSchemas.RTGameRecordSchema> ur = new List<DataSchemas.RTGameRecordSchema>();
        private double bestscore1, bestscore2, bestscore4, avgscore1, avgscore2, avgscore4;

        public RTStatsCompareViewModel()
        {
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.RTGameRecordSchema>("select * from RTGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                bestscore1 = ur.Where(x => x.boxes == 1 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 1 && x.cor == true).GroupBy(x => (int)Math.Ceiling(x.trialnum / 6.0)).Where(x => x.Count() >= 5).Select(x => x.Sum(y => y.reactiontime) / x.Count()).Min();
                bestscore2 = ur.Where(x => x.boxes == 2 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 2 && x.cor == true && x.reactiontime > 0).GroupBy(x => (int)Math.Ceiling(x.trialnum / 6.0)).Where(x => x.Count() >= 5).Select(x => x.Sum(y => y.reactiontime) / x.Count()).Min();
                bestscore4 = ur.Where(x => x.boxes == 4 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 4 && x.cor == true).GroupBy(x => (int)Math.Ceiling(x.trialnum / 6.0)).Where(x => x.Count() >= 5).Select(x => x.Sum(y => y.reactiontime) / x.Count()).Min();
                avgscore1 = ur.Where(x => x.boxes == 1 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 1 && x.cor == true).Sum(y => y.reactiontime) / ur.Where(x => x.boxes == 1 && x.cor == true).Count();
                avgscore2 = ur.Where(x => x.boxes == 2 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 2 && x.cor == true).Sum(y => y.reactiontime) / ur.Where(x => x.boxes == 2 && x.cor == true).Count();
                avgscore4 = ur.Where(x => x.boxes == 4 && x.cor == true).Count() < 5 ? 0 : ur.Where(x => x.boxes == 4 && x.cor == true).Sum(y => y.reactiontime) / ur.Where(x => x.boxes == 4 && x.cor == true).Count();
            }
        }

        private List<ChartEntry> GetCompetitors(bool best, int boxes)
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            ChartEntry e;
            if (best)
            {
                if (boxes == 1)
                {
                    e = new ChartEntry((float)bestscore1);
                    e.ValueLabel = Math.Round(bestscore1, 1).ToString() + " ms";
                }
                else if(boxes == 2)
                {
                    e = new ChartEntry((float)bestscore2);
                    e.ValueLabel = Math.Round(bestscore2, 1).ToString() + " ms";
                }
                else
                {
                    e = new ChartEntry((float)bestscore4);
                    e.ValueLabel = Math.Round(bestscore4, 1).ToString() + " ms";
                }
            }
            else
            {
                if (boxes == 1)
                {
                    e = new ChartEntry((float)avgscore1);
                    e.ValueLabel = Math.Round(avgscore1, 1).ToString() + " ms";
                }
                else if (boxes == 2)
                {
                    e = new ChartEntry((float)avgscore2);
                    e.ValueLabel = Math.Round(avgscore2, 1).ToString() + " ms";
                }
                else
                {
                    e = new ChartEntry((float)avgscore4);
                    e.ValueLabel = Math.Round(avgscore4, 1).ToString() + " ms";
                }
            }
            e.TextColor = SKColors.Black;
            e.Label = "Me";
            e.Color = clrs[0];
            es.Add(e);


            List<GameShare> rtgsl = new List<GameShare>();
            rtgsl = App.mum.GameShares.Where(x => x.game == "RT").ToList();
            foreach (GameShare gs in rtgsl)
            {
                if (best)
                {
                    if (boxes == 1)
                    {
                        e = new ChartEntry((float)gs.bestscore[0]);
                        e.ValueLabel = Math.Round(gs.bestscore[0], 1).ToString() + " ms";
                    }
                    else if (boxes == 2)
                    {
                        e = new ChartEntry((float)gs.bestscore[1]);
                        e.ValueLabel = Math.Round(gs.bestscore[1], 1).ToString() + " ms";
                    }
                    else
                    {
                        e = new ChartEntry((float)gs.bestscore[2]);
                        e.ValueLabel = Math.Round(gs.bestscore[2], 1).ToString() + " ms";
                    }
                }
                else
                {
                    if (boxes == 1)
                    {
                        e = new ChartEntry((float)gs.avgscore[0]);
                        e.ValueLabel = Math.Round(gs.avgscore[0], 1).ToString() + " ms";
                    }
                    else if (boxes == 2)
                    {
                        e = new ChartEntry((float)gs.avgscore[1]);
                        e.ValueLabel = Math.Round(gs.avgscore[1], 1).ToString() + " ms";
                    }
                    else
                    {
                        e = new ChartEntry((float)gs.avgscore[2]);
                        e.ValueLabel = Math.Round(gs.avgscore[2], 1).ToString() + " ms";
                    }
                }
                e.TextColor = SKColors.Black;
                e.Label = gs.Screenname;
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }

        public Chart Fastest1Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(true, 1),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart Fastest2Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(true, 2),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart Fastest4Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(true, 4),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart Average1Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(false, 1),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart Average2Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(false, 2),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart Average4Chart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitors(false, 4),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };
    }
}
