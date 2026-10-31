namespace PsikologTestApp.Tests.RenkliKelimeler.Models
{
    public class StroopTestResult
    {
        public string TestName { get; set; } = "Renkli Kelimeler";
        public DateTime TestDate { get; set; } = DateTime.Now;
        public int DurationMinutes { get; set; }
        public List<StroopTrial> Trials { get; set; } = new();

        public IEnumerable<StroopTrial> CorrectTrials => Trials.Where(t => t.IsCorrect);
        public IEnumerable<StroopTrial> IncorrectTrials => Trials.Where(t => !t.IsCorrect);

        public double AverageResponseTime => CorrectTrials.Any()
            ? CorrectTrials.Average(t => t.ResponseTimeMs) : 0;
        public double MinResponseTime => CorrectTrials.Any()
            ? CorrectTrials.Min(t => t.ResponseTimeMs) : 0;
        public double MaxResponseTime => CorrectTrials.Any()
            ? CorrectTrials.Max(t => t.ResponseTimeMs) : 0;
        public double Accuracy => Trials.Count > 0
            ? (double)Trials.Count(t => t.IsCorrect) / Trials.Count * 100 : 0;
    }

    public class StroopTrial
    {
        public int TrialNumber { get; set; }
        public string WordText { get; set; } = "";       // Yazılan kelime (örn. "KIRMIZI")
        public string DisplayColor { get; set; } = "";   // Kelime hangi renkle gösterildi (doğru cevap)
        public string SelectedColor { get; set; } = "";  // Kullanıcının seçtiği renk
        public bool IsCorrect { get; set; }
        public double ResponseTimeMs { get; set; }
    }
}
