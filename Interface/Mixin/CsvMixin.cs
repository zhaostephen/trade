﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Trade.Mixin;

namespace Interace.Mixin
{
    public static class CsvMixin
    {
        public static IEnumerable<dynamic> ReadCsv(this string path, Encoding encoding = null)
        {
            if (!File.Exists(path))
                return Enumerable.Empty<dynamic>();

            var lines = ReadAllLines(path, encoding);
            if (!lines.Any())
                return Enumerable.Empty<dynamic>();
            var columns = lines[0].Split(new[] { ',' });
            return lines
                .Skip(1)
                .Select(p =>
                {
                    var splits = p.Split(new[] { ',' });
                    var obj = new ExpandoObject() as IDictionary<string, object>;
                    for (var i = 0; i < columns.Length; ++i)
                    {
                        var column = columns[i];
                        if (!string.IsNullOrEmpty(column))
                            obj[column] = splits[i];
                    }
                    return obj;
                })
                .ToArray();
        }

        public static IEnumerable<T> ReadCsv<T>(this string path, Encoding encoding=null) where T:new()
        {
            if (!File.Exists(path))
                return Enumerable.Empty<T>();

            var lines = ReadAllLines(path, encoding);
            if (!lines.Any())
                return Enumerable.Empty<T>();

            var columns = lines[0].Split(new[] { ',' });
            return lines
                .Skip(1)
                .Select(p =>
                {
                    var splits = p.Split(new[] { ',' });
                    var f = new T();
                    for (var i = 0; i < columns.Length; ++i)
                    {
                        var column = columns[i];
                        if(!string.IsNullOrEmpty(column))
                            f.SetPropertyValue(column, splits[i]);
                    }
                    return f;
                })
                .ToArray();
        }

        static string[] ReadAllLines(string file, Encoding encoding = null)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var r = (encoding != null ? new StreamReader(stream, encoding) : new StreamReader(stream)))
            {
                var lines = new List<string>();
                while (!r.EndOfStream)
                {
                    var line = r.ReadLine();
                    if (line != null)
                        lines.Add(line);
                }
                return lines.ToArray();
            }
        }
    }
}
