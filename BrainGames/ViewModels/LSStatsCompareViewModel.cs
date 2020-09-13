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
    public class LSStatsCompareViewModel : ViewModelBase
    {
        private List<DataSchemas.LSGameRecordSchema> ur = new List<DataSchemas.LSGameRecordSchema>();
        private double fastest_f, fastest_b;
        private int longest_f, longest_b;

        public LSStatsCompareViewModel()
        {
            try { ur = MasterUtilityModel.conn_sync.Query<DataSchemas.LSGameRecordSchema>("select * from LSGameRecordSchema"); }
            catch (Exception ex) {; }
            if (ur != null && ur.Count() > 0)
            {
                longest_f = ur.Where(x => x.cor == true && x.direction == "f").Select(x => x.itemcnt).Max();
                longest_b = ur.Where(x => x.cor == true && x.direction == "b").Select(x => x.itemcnt).Max();
                fastest_f = ur.Where(x => x.cor == true && x.direction == "f" && x.itemcnt == longest_f).Select(x => x.ontimems + x.offtimems).Min();
                fastest_b = ur.Where(x => x.cor == true && x.direction == "b" && x.itemcnt == longest_b).Select(x => x.ontimems + x.offtimems).Min();
            }
        }

        private List<ChartEntry> GetCompetitorsL(bool fwd)
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            ChartEntry e;
            if (fwd)
            {
                e = new ChartEntry((float)longest_f);
                e.ValueLabel = longest_f.ToString();
            }
            else
            {
                e = new ChartEntry((float)longest_b);
                e.ValueLabel = longest_b.ToString();
            }
            e.TextColor = SKColors.Black;
            e.Label = "Me";
            e.Color = clrs[0];
            es.Add(e);

            List<GameShare> lsgsl = new List<GameShare>();
            lsgsl = App.mum.GameShares.Where(x => x.game == "LS").ToList();
            foreach (GameShare gs in lsgsl)
            {
                if (fwd)
                {
                    e = new ChartEntry((float)gs.bestscore[0]);
                    e.ValueLabel = gs.bestscore[0].ToString();
                }
                else
                {
                    e = new ChartEntry((float)gs.bestscore[1]);
                    e.ValueLabel = gs.bestscore[1].ToString();
                }
                e.TextColor = SKColors.Black;
                e.Label = gs.Screenname;
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }

        private List<ChartEntry> GetCompetitorsF(bool fwd)
        {
            SKColor[] clrs = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.YellowGreen, SKColors.Red, SKColors.Brown,
                                                SKColors.Indigo, SKColors.LightGreen, SKColors.Orange, SKColors.Olive, SKColors.Aquamarine, SKColors.Black };
            int idx = 1;
            List<ChartEntry> es = new List<ChartEntry>();
            ChartEntry e;
            if (fwd)
            {
                e = new ChartEntry((float)fastest_f);
                e.ValueLabel = Math.Round(fastest_f, 1).ToString() + " ms";
            }
            else
            {
                e = new ChartEntry((float)fastest_b);
                e.ValueLabel = Math.Round(fastest_b, 1).ToString() + " ms";
            }
            e.TextColor = SKColors.Black;
            e.Label = "Me";
            e.Color = clrs[0];
            es.Add(e);

            List<GameShare> lsgsl = new List<GameShare>();
            lsgsl = App.mum.GameShares.Where(x => x.game == "LS").ToList();
            foreach (GameShare gs in lsgsl)
            {
                if (fwd)
                {
                    e = new ChartEntry((float)gs.bestscore[2]);
                    e.ValueLabel = Math.Round(gs.bestscore[2], 1).ToString() + " ms";
                }
                else
                {
                    e = new ChartEntry((float)gs.bestscore[3]);
                    e.ValueLabel = Math.Round(gs.bestscore[3], 1).ToString() + " ms";
                }
                e.TextColor = SKColors.Black;
                e.Label = gs.Screenname;
                e.Color = clrs[idx++];
                es.Add(e);
            }
            return es;
        }

        public Chart LongestFChart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitorsL(true),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart LongestBChart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitorsL(false),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart FastestFChart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitorsF(true),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };

        public Chart FastestBChart => new BarChart()
        {
            Margin = 10,
            Entries = GetCompetitorsF(false),
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal
        };
    }
}
