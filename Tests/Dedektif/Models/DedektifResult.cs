using System;
using System.Collections.Generic;
using System.Linq;

namespace PsikologTestApp.Tests.Dedektif.Models
{
    public class DedektifTrial
    {
        public int    TrialNumber    { get; set; }
        public string TargetName     { get; set; } = "";
        public double ResponseTimeMs { get; set; }
        public int    WrongClicks    { get; set; }
        public bool   Found          { get; set; }
        /// <summary>Wrong figures clicked in order before finding the target.</summary>
        public List<string> WrongClickNames { get; set; } = new();
    }

    public class DedektifResult
    {
        public string   TestName        { get; set; } = "Dedektif";
        public DateTime TestDate        { get; set; } = DateTime.Now;
        public int      FigureCount     { get; set; }
        public int      DurationMinutes { get; set; }

        public List<DedektifTrial> Trials { get; set; } = new();

        public int    TotalFound       => Trials.Count(t => t.Found);
        public int    TotalWrongClicks => Trials.Sum(t => t.WrongClicks);

        public double AverageResponseMs => Trials.Where(t => t.Found)
                                                 .Select(t => t.ResponseTimeMs)
                                                 .DefaultIfEmpty(0).Average();
        public double MinResponseMs     => Trials.Where(t => t.Found)
                                                 .Select(t => t.ResponseTimeMs)
                                                 .DefaultIfEmpty(0).Min();
        public double MaxResponseMs     => Trials.Where(t => t.Found)
                                                 .Select(t => t.ResponseTimeMs)
                                                 .DefaultIfEmpty(0).Max();
    }
}
