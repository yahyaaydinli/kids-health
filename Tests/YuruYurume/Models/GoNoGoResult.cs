namespace PsikologTestApp.Tests.YuruYurume.Models
{
    public enum TrialType { Hit, Miss, FalseAlarm, CorrectRejection }

    public class GoNoGoResult
    {
        public string TestName { get; set; } = "Yürü ve Yürüme";
        public DateTime TestDate { get; set; } = DateTime.Now;
        public int DurationMinutes { get; set; }
        public int GoPercentage { get; set; }
        public List<GoNoGoTrial> Trials { get; set; } = new();

        public IEnumerable<GoNoGoTrial> Hits             => Trials.Where(t => t.Type == TrialType.Hit);
        public IEnumerable<GoNoGoTrial> Misses            => Trials.Where(t => t.Type == TrialType.Miss);
        public IEnumerable<GoNoGoTrial> FalseAlarms       => Trials.Where(t => t.Type == TrialType.FalseAlarm);
        public IEnumerable<GoNoGoTrial> CorrectRejections => Trials.Where(t => t.Type == TrialType.CorrectRejection);

        public double HitRate => Trials.Count(t => t.IsGoStimulus) > 0
            ? (double)Hits.Count() / Trials.Count(t => t.IsGoStimulus) * 100 : 0;
        public double FalseAlarmRate => Trials.Count(t => !t.IsGoStimulus) > 0
            ? (double)FalseAlarms.Count() / Trials.Count(t => !t.IsGoStimulus) * 100 : 0;

        public double AverageResponseTime => Hits.Any() ? Hits.Average(t => t.ResponseTimeMs) : 0;
        public double MinResponseTime     => Hits.Any() ? Hits.Min(t => t.ResponseTimeMs) : 0;
        public double MaxResponseTime     => Hits.Any() ? Hits.Max(t => t.ResponseTimeMs) : 0;
    }

    public class GoNoGoTrial
    {
        public int TrialNumber { get; set; }
        public bool IsGoStimulus { get; set; }   // true = yeşil (bas), false = kırmızı (basma)
        public bool Pressed { get; set; }
        public int PressCount { get; set; }      // kaç kere basıldı
        public double ResponseTimeMs { get; set; }
        public TrialType Type { get; set; }
    }
}
