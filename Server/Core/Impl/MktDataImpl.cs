﻿using System.Collections.Generic;
using System.Linq;
using Trade.Utility;
using log4net;
using Trade.Db;
using Trade.Mixin;
using System.Threading;
using Trade.Cfg;
using Interace.Idx;
using Interace.Mixin;
using System;
using Trade.Data;
using System.Threading.Tasks;

namespace Trade.Impl
{
    public interface IMktDataImpl
    {
        IEnumerable<Fundamental> MakeFundametals();
        Task[] MakeAsync(Tuple<int?, int?> range);
    }

    public class MktDataImpl : IMktDataImpl
    {
        static ILog log = typeof(MktDataImpl).Log();

        readonly MktDb _mktdb;
        readonly RawDb _rawdb;
        readonly Lazy<Index> _mktIndex;
        readonly Lazy<Index> _rawIndex;

        public MktDataImpl()
        {
            _mktdb = new MktDb();
            _rawdb = new RawDb();
            _mktIndex = new Lazy<Index>(() => _mktdb.GetIdx());
            _rawIndex = new Lazy<Index>(() => _rawdb.GetIdx());
        }

        public IEnumerable<Fundamental> MakeFundametals()
        {
            log.Info("Make fundamental");
            var fundamentals = _rawdb.QueryFundamentals();
            _mktdb.Save(fundamentals);

            return fundamentals;
        }

        public Task[] MakeAsync(Tuple<int?, int?> range)
        {
            var fundamentals = _rawdb.QueryFundamentals();
            if (range != null && range.Item1.HasValue && range.Item2.HasValue)
            {
                fundamentals = fundamentals.Skip(range.Item1.Value).Take(range.Item2.Value - range.Item1.Value).ToArray();
                log.WarnFormat("********Range {0}-{1}********", range.Item1, range.Item2);
            }
            log.InfoFormat("GOT, total {0}", fundamentals.Count());

            log.Info("Query codes");
            var codes = fundamentals.Select(p => p.代码).Distinct().ToArray();
            log.InfoFormat("GOT, total {0}", codes.Count());

            log.Info("Make days");
            var t1 = Task.Factory.StartNew(() => MakeDays(codes));

            log.Info("Make minutes");
            var t2 = Task.Factory.StartNew(() => MakeMinutes(codes));

            return new[] { t1, t2};
        }

        private void MakeDays(IEnumerable<string> codes)
        {
            var i = 0;
            var total = codes.Count();
            foreach (var code in codes.AsParallel())
            {
                Interlocked.Increment(ref i);
                log.InfoFormat("{0}/{1} - make days - {2}", i, total, code);
                MakeByCode(code, PeriodEnum.Daily, new[] { PeriodEnum.Daily, PeriodEnum.Weekly, PeriodEnum.Monthly });
            }
        }
        private void MakeMinutes(IEnumerable<string> codes)
        {
            var i = 0;
            var total = codes.Count();
            foreach (var code in codes.AsParallel())
            {
                Interlocked.Increment(ref i);
                log.InfoFormat("{0}/{1} - make minutes - {2}", i, total, code);
                MakeByCode(code, PeriodEnum.Min5, new[] { PeriodEnum.Min5, PeriodEnum.Min15, PeriodEnum.Min30, PeriodEnum.Min60 });
            }
        }
        private void MakeByCode(string code, PeriodEnum rawPeriod, PeriodEnum[] followings)
        {
            var rawUpdate = _rawIndex.Value.LastUpdate(code, rawPeriod);
            var followingUpdates = followings.Select(p => _mktIndex.Value.LastUpdate(code, p)).ToArray();

            if (rawUpdate.HasValue && followingUpdates.All(p => p.HasValue && p.Value >= rawUpdate.Value))
            {
                log.WarnFormat("Ignore {0}, already updated", code);
                return;
            }

            var dataset =
                new[] { _rawdb.Query(code, rawPeriod) }
                .Where(p => p != null)
                .ToArray();

            if (!dataset.Any())
            {
                log.WarnFormat("empty data set {0}", code);
                return;
            }

            foreach (var following in followings.AsParallel())
            {
                log.Info(following);
                var another = dataset.Make(rawPeriod, following);
                _mktdb.Save(another, following);
            }
        }
    }
}