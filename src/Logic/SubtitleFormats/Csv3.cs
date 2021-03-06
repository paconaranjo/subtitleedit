﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Csv3 : SubtitleFormat
    {

        private const string Seperator = ",";

        //01:00:10:03,01:00:15:25,"I thought I should let my sister-in-law know.", ""
        static readonly Regex CsvLine = new Regex(@"^\d\d:\d\d:\d\d:\d\d" + Seperator + @"\d\d:\d\d:\d\d:\d\d" + Seperator, RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".csv"; }
        }

        public override string Name
        {
            get { return "Csv3"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            int fine = 0;
            int failed = 0;
            bool continuation = false;
            foreach (string line in lines)
            {
                Match m = CsvLine.Match(line);
                if (m.Success)
                {
                    fine++;
                    string s = line.Remove(0, m.Length);
                    continuation = s.StartsWith("\"");
                }
                else if (line.Trim().Length > 0)
                {
                    if (continuation)
                        continuation = false;
                    else
                        failed++;
                }

            }
            return fine > failed;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{1}{0}{2}{0}\"{3}\"{0}\"{4}\"";
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(format, Seperator, "Start time (hh:mm:ss:ff)", "End time (hh:mm:ss:ff)", "Line 1", "Line 2"));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var arr = p.Text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 3)
                {
                    string s = Utilities.AutoBreakLine(p.Text);
                    arr = s.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                }
                string line1 = string.Empty;
                string line2 = string.Empty;
                if (arr.Length > 0)
                    line1 = arr[0];
                if (arr.Length > 1)
                    line2 = arr[1];
                line1 = line1.Replace("\"", "\"\"");
                line2 = line2.Replace("\"", "\"\"");
                sb.AppendLine(string.Format(format, Seperator, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), line1, line2));
            }
            return sb.ToString().Trim();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p;
            foreach (string line in lines)
            {
                Match m = CsvLine.Match(line);
                if (m.Success)
                {
                    string[] parts = line.Substring(0, m.Length).Split(Seperator.ToCharArray(),  StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    try
                    {
                        var start = DecodeTimeCode(parts[0]);
                        var end = DecodeTimeCode(parts[1]);
                        string text = ReadText(line.Remove(0, m.Length));                        
                        p = new Paragraph(start, end, text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (line.Trim().Length > 0)
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private string ReadText(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return string.Empty;

            csv = csv.Replace("\"\"", "\"");

            var sb = new StringBuilder();
            csv = csv.Trim();
            if (csv.StartsWith("\""))
                csv = csv.Remove(0, 1);
            if (csv.EndsWith("\""))
                csv = csv.Remove(csv.Length-1, 1);
            bool isBreak = false;
            for (int i=0; i<csv.Length; i++)
            {
                string s = csv.Substring(i, 1);
                if (s == "\"" && csv.Substring(i).StartsWith("\"\""))
                {
                    sb.Append("\"");
                }
                else if (s == "\"")
                {
                    if (isBreak)
                    {
                        isBreak = false;
                    }
                    else if (i == 0 || i == csv.Length - 1 || sb.ToString().EndsWith(Environment.NewLine))
                    {
                        sb.Append("\"");
                    }
                    else
                    {                        
                        isBreak = true;
                    }
                }
                else
                {
                    if (isBreak && s == " ")
                    {
                    }
                    else if (isBreak && s == ",")
                    {
                        sb.Append(Environment.NewLine);
                    }
                    else
                    {
                        isBreak = false;
                        sb.Append(s);
                    }
                }
            }
            return sb.ToString().Trim();
        }

        private TimeCode DecodeTimeCode(string part)
        {
            string[] parts = part.Split(".:".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}
