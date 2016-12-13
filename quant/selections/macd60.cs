﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Trade.Factors;
using Interace.Quant;
using Interface.Quant;
using Trade.Indicator;
using Trade.Data;

namespace Trade.selections
{
    public class macd60 : selection
    {
        static ILog log = LogManager.GetLogger(typeof(macd60));

        public override universe Pass(IEnumerable<string> stocks)
        {
            var client = new Db.db();

            log.Info("query market data");
            var data = stocks
                .AsParallel()
                .Select(code => client.kdata(code, "60"))
                .Where(p => p != null && p.Any())
                .ToArray();
            log.InfoFormat("total {0}", data.Count());

            var codes = data
                .Where(p =>
                {
                    var macd = (macd)new MACD(p);
                    return macd != null && macd.MACD > 0 && macd.DIF <= 0.01;
                })
                .Select(p => p.Code)
                .Distinct()
                .ToArray();

            log.InfoFormat("selected {0}", codes.Count());

            return new universe("macd60", codes);
        }
    }
}
