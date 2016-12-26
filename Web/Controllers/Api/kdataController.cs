﻿using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Trade;
using Interace.Attribution;
using Trade.Cfg;
using Trade.Indicator;
using Trade.Data;

namespace Web.Controllers.Api
{
    public class kdataController : ApiController
    {
        [Route("api/kdata/{id}")]
        public chart Get(string id, string ktype)
        {
            var kdata = new Trade.Db.db().kdata(id, ktype);
            var basic = new Trade.Db.db().basics(id);
            var since = Trade.Cfg.Configuration.data.bearcrossbull;
            var k = kdata.Where(p => p.date >= since).ToArray();
            var macd = new MACD(kdata.close()).Where(p => p.Date >= since).ToArray();
            var macdvol = new MACD(kdata.volume(100 * 10000)).Where(p => p.Date >= since).ToArray();
            return new chart {
                data = k.Select(p => new object[] { p.date, p.open, p.high, p.low, p.close }).ToArray(),
                volume = k.Select(p => new object[] { p.date, p.volume/ 100 * 10000 }).ToArray(),
                macd = macd.Select(p => new object[] { p.Date, p.MACD }).ToArray(),
                dif = macd.Select(p => new object[] { p.Date, p.DIF }).ToArray(),
                dea = macd.Select(p => new object[] { p.Date, p.DEA }).ToArray(),
                macdvol = macdvol.Select(p => new object[] { p.Date, p.MACD }).ToArray(),
                difvol = macdvol.Select(p => new object[] { p.Date, p.DIF }).ToArray(),
                deavol = macdvol.Select(p => new object[] { p.Date, p.DEA }).ToArray(),
                code = kdata.Code,
                name = basic.name,
                keyprices = keyprice(k, id, ktype)
            };
        }

        KeyPrice[] keyprice(IEnumerable<kdatapoint> k, string id, string ktype)
        {
            var keyprices = Trade.analytic.keyprice(id, ktype);
            if (k.Any())
            {
                var cur = new[]
                {
                    KeyPrice.low(id, k.Last().date, k.Last().low, true),
                    KeyPrice.high(id, k.Last().date, k.Last().high, true)
                };
                keyprices = keyprices.Concat(cur).ToArray();
            }
            return keyprices;
        }
    }

    public class chart
    {
        public KeyPrice[] keyprices { get; set; }
        public object[][] data { get; set; }
        public object[][] volume { get; set; }
        public object[][] macd { get; set; }
        public object[][] dif { get; set; }
        public object[][] dea { get; set; }
        public object[][] macdvol { get; set; }
        public object[][] difvol { get; set; }
        public object[][] deavol { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }
}