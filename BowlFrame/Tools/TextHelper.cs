using BowlFrame.Config;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Tools
{
    internal class TextHelper
    {
        /// <summary>
        /// 替换占位符
        /// </summary>
        /// <param name="text"></param>
        /// <param name="placeholders"></param>
        /// <param name="disableInsidePlaceholders"></param>
        /// <returns></returns>
        public static string ReplacePlaceholder(string? text, Dictionary<string, (string, int?)>? placeholders = null, bool disableInsidePlaceholders = false)
        {
            if (text == null) return "";

            placeholders ??= [];

            if (!disableInsidePlaceholders)
            {
                placeholders.Add("Path", (PathConfig.Path, null));
                placeholders.Add("Version", (Environment.Version.ToString(), null));
                placeholders.Add("WeekDay", (((int)DateTime.Now.DayOfWeek).ToString(), null));
                placeholders.Add("Timestamp", (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), null));
                placeholders.Add("Number_Timestamp", (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 1));
            }

            int head, foot = 0;
            string placeholder;
            foot = text.IndexOf('}', foot + 1);
            while (foot != -1)
            {
                head = text.LastIndexOf('{', foot - 1);
                placeholder = text.Substring(head + 1, foot - head - 1);
                if (placeholders.TryGetValue(placeholder, out (string, int?) value))
                {
                    if (value.Item2 < 0)
                        for (; value.Item2 < 0; value.Item2++)
                            value.Item1 = "\n" + value.Item1;

                    text = text.Remove(head - (value.Item2 ?? 0), foot - head + (value.Item2 * 2 ?? 0) + 1);
                    text = text.Insert(head - (value.Item2 ?? 0), value.Item1);
                    foot = head - (value.Item2 ?? 0) + value.Item1.Length;
                }
                if (foot + 1 > text.Length) break;
                foot = text.IndexOf('}', foot + 1);
            }

            return text;
        }
    }
}