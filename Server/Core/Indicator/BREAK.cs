﻿using Screen.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class BREAK : BREAKSeries
    {
        public BREAK(TimeSeries<double> data)
        {
            var peaks = new PEAK(data);
            var bottoms = new BOTTOM(data);

            var peakdate = DateTime.MinValue;
            for (int i = 2; i < data.Count; i++)
            {
                var cur = data[i];
                var next1 = data[i - 1];
                var next2 = data[i - 2];

                //找到前期高点（没有被突破的）
                var peak = peaks.LastOrDefault(p => p.Date < cur.Date && p.Date > peakdate);
                if (peak == null)
                    continue;

                //判断是否突破
                var double3break = (cur.Value > peak.Value
                    && next1.Value > peak.Value
                    && next2.Value > peak.Value
                    && cur.Value >= (1 + 0.02) * peak.Value);
                //判断是否强突破
                var strongbreak = (cur.Value > peak.Value && cur.Value >= (1 + 0.06) * peak.Value);

                if (double3break || strongbreak)
                {
                    peakdate = cur.Date;
                    this.Add(new BREAKPoint
                    {
                        Date = cur.Date,
                        Value = cur.Value,
                        Peak = peak
                    });
                }
            }
        }
    }
}