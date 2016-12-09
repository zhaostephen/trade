﻿using Trade.Cfg;
using Trade.Data;
using Trade.Mixin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Interace.Mixin;
using Interface.Quant;
using MySql.Data.MySqlClient;
using Dapper;

namespace Trade
{
    public class client
    {
        public universe universe(string name)
        {
            var connString = Configuration.env == "dev" 
                ? @"Server=584a482f41204.gz.cdb.myqcloud.com;Port=17020;Database=quant;Uid=quant;Pwd=Woaiquant123" 
                : @"Server=10.66.111.191;Port=3306 ;Database=quant;Uid=quant;Pwd=Woaiquant123";
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var codes = conn
                    .Query<string>("select code from universe where name=@name",new {name=name })
                    .Distinct()
                    .ToArray();

                return new universe(name, codes);
            }
        }

        public string[] sectors()
        {
            return basics()
                .Select(p => p.sectors)
                .Where(p => !string.IsNullOrEmpty(p))
                .SelectMany(p => p.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .ToArray();
        }

        public Basic basics(string code)
        {
            return basics()
                .FirstOrDefault(p=>string.Equals(p.code, code, StringComparison.InvariantCultureIgnoreCase));
        }

        public Basics basics(IEnumerable<string> codes)
        {
            var set = basics();

            var q = from f in set
                    join c in codes on f.code equals c
                    select f;

            return new Basics(q.ToArray());
        }

        public Basics basics()
        {
            var file = Configuration.data.basics.file("basics.csv");
            return new Basics(file.ReadCsv<Basic>(Configuration.encoding.gbk));
        }

        public IEnumerable<basicname> basicnames()
        {
            var file = Configuration.data.basics.file("basicnames.csv");
            return file.ReadCsv<basicname>(Configuration.encoding.gbk);
        }

        public kdata kdata(string code, string ktype)
        {
            var file = Configuration.data.kdata.file(ktype + "/" + code + ".csv");
            var p = file.ReadCsv<kdatapoint>(Configuration.encoding.gbk);
            return new kdata(code, p);
        }

        public IEnumerable<kdata> kdataall(string ktype, string secorOrIndex = null)
        {
            var codes = this.codes(secorOrIndex).ToArray();
            var results = codes
                .AsParallel()
                .Select(code => kdata(code, ktype))
                .Where(p => p != null)
                .ToArray();
            return results;
        }

        public IEnumerable<string> codes(string secorOrIndex = null)
        {
            return basics()
                .Where(p => string.IsNullOrEmpty(secorOrIndex) || p.belongtoindex(secorOrIndex) || p.belongtosector(secorOrIndex))
                .Select(p => p.code)
                .Distinct()
                .ToArray();
        }

        public Interace.Quant.Trade[] trades(string porflio)
        {
            var path = Configuration.data.trade.EnsurePathCreated();
            var file = Path.Combine(path, porflio + ".csv");
            return file.ReadCsv<Interace.Quant.Trade>().ToArray();
        }
    }
}