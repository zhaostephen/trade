﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trade.Data
{
    public class basics
    {
        public string code { get; set; }
        public string name{ get; set; }
        public string nameabbr { get; set; }
        public string industry{ get; set; }
        public string area{ get; set; }
        public string pe{ get; set; }
        public string outstanding{ get; set; }
        public string totals{ get; set; }
        public string totalAssets{ get; set; }
        public string liquidAssets{ get; set; }
        public string fixedAssets{ get; set; }
        public string reserved{ get; set; }
        public string reservedPerShare{ get; set; }
        public string esp{ get; set; }
        public string bvps{ get; set; }
        public string pb{ get; set; }
        public string timeToMarket{ get; set; }
        public string undp{ get; set; }
        public string perundp{ get; set; }
        public string rev{ get; set; }
        public string profit{ get; set; }
        public string gpr{ get; set; }
        public string npr{ get; set; }
        public string holders{ get; set; }

        public bool st { get; set; }
        public bool suspended { get; set; }
        public bool terminated { get; set; }
        public string assettype { get; set; }
        public string indexes { get; set; }
        public string sectors { get; set; }

        public string mainindex()
        {
            return !string.IsNullOrEmpty(indexes) ? indexes.Split('|').First() : string.Empty;
        }
        public bool belongtoindex(string index)
        {
            return !string.IsNullOrEmpty(indexes) && indexes.Contains(index);
        }

        public bool belongtosector(string sector)
        {
            return !string.IsNullOrEmpty(sectors) && sectors.Contains(sector);
        }

        public void addindex(params string[] i)
        {
            indexes = indexes ?? string.Empty;
            indexes = string.Join("|",new[] {indexes}.Concat(i).Where(p => !string.IsNullOrEmpty(p)).ToArray());
        }

        public void addsector(params string[] s)
        {
            sectors = sectors ?? string.Empty;
            sectors = string.Join("|", new[] { sectors }.Concat(s).Where(p=>!string.IsNullOrEmpty(p)).ToArray());
        }
    }

    public class basicname
    {
        public string code { get; set; }
        public string name { get; set; }
        public string nameabbr { get; set; }
        public string assettype { get; set; }
    }

    public static class assettypes
    {
        public const string index = "index";
        public const string stock = "stock";
        public const string sector = "sector";
    }

    public static class index
    {
        public const string sh = "sh";
        public const string sz = "sz";
        public const string hs300 = "hs300";
        public const string sz50 = "sz50";
        public const string zxb = "zxb";
        public const string cyb = "cyb";
    }
}
