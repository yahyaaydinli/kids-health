namespace PsikologTestApp.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public string TestName { get; set; } = "Hedef Avcısı";
        public DateTime TestDate { get; set; } = DateTime.Now;
        public int DurationMinutes { get; set; }
        public char FixedTargetLetter { get; set; }
        public List<ReactionTime> ReactionTimes { get; set; } = new();

        // Yalnızca doğru basışlar
        public IEnumerable<ReactionTime> CorrectPresses => ReactionTimes.Where(r => r.IsCorrect);
        // Yanlış basışlar
        public IEnumerable<ReactionTime> FalsePresses => ReactionTimes.Where(r => !r.IsCorrect);

        public double AverageReactionTime => CorrectPresses.Any()
            ? CorrectPresses.Average(r => r.ResponseTimeMs) : 0;
        public double MinReactionTime => CorrectPresses.Any()
            ? CorrectPresses.Min(r => r.ResponseTimeMs) : 0;
        public double MaxReactionTime => CorrectPresses.Any()
            ? CorrectPresses.Max(r => r.ResponseTimeMs) : 0;
    }

    public class ReactionTime
    {
        public int Id { get; set; }
        public int TestResultId { get; set; }
        public char TargetLetter { get; set; }
        public char DisplayedLetter { get; set; }
        public double WaitTimeMs { get; set; }
        public double ResponseTimeMs { get; set; }
        public bool IsCorrect { get; set; } = true;
        public int PressOrder { get; set; } = 1; // 1=ilk basış, 2+=ek basış
    }
}
