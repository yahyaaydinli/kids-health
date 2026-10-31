using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PsikologTestApp.Tests.DogruHedef.Models
{
    public class DogruHedefAnimal
    {
        public string Name  { get; init; } = "";
        public string Emoji { get; init; } = "";

        public static readonly DogruHedefAnimal[] All =
        {
            new() { Name = "Kedi",    Emoji = "🐱" },
            new() { Name = "Köpek",   Emoji = "🐶" },
            new() { Name = "Kurbağa", Emoji = "🐸" },
            new() { Name = "Tavşan",  Emoji = "🐰" },
            new() { Name = "Tilki",   Emoji = "🦊" },
        };
    }

    public enum SegmentKind { Target, OtherAnimal, Gap }

    public class PressRecord
    {
        public double TimeFromSegmentStartMs { get; set; }
    }

    public class DogruHedefSegment
    {
        public int         SegmentIndex { get; set; }
        public SegmentKind Kind         { get; set; }
        public string      AnimalName   { get; set; } = "";
        public double      DurationMs   { get; set; }
        public List<PressRecord> Presses { get; set; } = new();

        public int PressCount => Presses.Count;

        public string SegmentLabel => Kind switch
        {
            SegmentKind.Target      => $"🎯 {AnimalName}",
            SegmentKind.OtherAnimal => $"🐾 {AnimalName}",
            SegmentKind.Gap         => "⬜ Boşluk",
            _                       => "—"
        };

        /// <summary>
        /// Displays all press times relative to segment start.
        /// For consecutive presses also shows the interval since the previous press.
        /// Example: "245ms | 612ms (+367ms) | 1100ms (+488ms)"
        /// </summary>
        public string PressTimingsDisplay
        {
            get
            {
                if (Presses.Count == 0) return "—";
                var sb = new StringBuilder();
                for (int i = 0; i < Presses.Count; i++)
                {
                    if (i > 0) sb.Append(" | ");
                    sb.Append($"{Presses[i].TimeFromSegmentStartMs:F0}ms");
                    if (i > 0)
                    {
                        double interval = Presses[i].TimeFromSegmentStartMs - Presses[i - 1].TimeFromSegmentStartMs;
                        sb.Append($" (+{interval:F0}ms)");
                    }
                }
                return sb.ToString();
            }
        }
    }

    public class DogruHedefResult
    {
        public string   TestName         { get; set; } = "Doğru Hedef";
        public DateTime TestDate         { get; set; } = DateTime.Now;
        public string   TargetAnimalName { get; set; } = "";
        public int      DurationMinutes  { get; set; }

        public List<DogruHedefSegment> Segments { get; set; } = new();

        public int TotalTargetSegments  => Segments.Count(s => s.Kind == SegmentKind.Target);
        public int HitCount             => Segments.Count(s => s.Kind == SegmentKind.Target && s.PressCount > 0);
        public int MissCount            => TotalTargetSegments - HitCount;
        public int TotalPressesOnTarget => Segments.Where(s => s.Kind == SegmentKind.Target).Sum(s => s.PressCount);
        public int FalsePressesOnGap    => Segments.Where(s => s.Kind == SegmentKind.Gap).Sum(s => s.PressCount);
        public int FalsePressesOnOther  => Segments.Where(s => s.Kind == SegmentKind.OtherAnimal).Sum(s => s.PressCount);
        public int TotalFalsePresses    => FalsePressesOnGap + FalsePressesOnOther;

        public double HitRate => TotalTargetSegments > 0 ? (double)HitCount / TotalTargetSegments * 100 : 0;

        public double AvgCorrectRtMs
        {
            get
            {
                var firstPresses = Segments
                    .Where(s => s.Kind == SegmentKind.Target && s.Presses.Count > 0)
                    .Select(s => s.Presses[0].TimeFromSegmentStartMs)
                    .ToList();
                return firstPresses.Count > 0 ? firstPresses.Average() : 0;
            }
        }
    }
}
