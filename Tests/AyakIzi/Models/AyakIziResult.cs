using System;
using System.Collections.Generic;
using System.Linq;

namespace PsikologTestApp.Tests.AyakIzi.Models
{
    public class AyakIziRound
    {
        public int    RoundNumber       { get; set; }
        public int    SequenceLength    { get; set; }
        public bool   Success           { get; set; }
        public double ResponseTimeMs    { get; set; }
        public int    NewSequenceCount  { get; set; }
    }

    public class AyakIziResult
    {
        public string   TestName       { get; set; } = "Ayak İzi";
        public DateTime TestDate       { get; set; } = DateTime.Now;
        public int      GridSize       { get; set; }
        public int      MaxSpanReached { get; set; }

        public List<AyakIziRound> Rounds { get; set; } = new();

        public int    TotalCorrect  => Rounds.Count(r => r.Success);
        public int    TotalAttempts => Rounds.Count;
        public double SuccessRate   => TotalAttempts > 0 ? (double)TotalCorrect / TotalAttempts * 100 : 0;

        public double AvgResponseMs      => Rounds.Any(r => r.Success)
                                            ? Rounds.Where(r => r.Success).Average(r => r.ResponseTimeMs)
                                            : 0;

        public int    TotalNewSequences  => Rounds.Sum(r => r.NewSequenceCount);
    }
}
