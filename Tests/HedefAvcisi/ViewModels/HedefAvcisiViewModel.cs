using System.Windows.Input;
using System.Windows.Threading;
using PsikologTestApp.Tests.HedefAvcisi.Models;
using PsikologTestApp.Tests.HedefAvcisi.Services;

namespace PsikologTestApp.Tests.HedefAvcisi.ViewModels
{
    public class HedefAvcisiViewModel
    {
        private ReactionTimeService _reactionTimeService;
        private TestResult _testResult;
        private DispatcherTimer _displayTimer;
        private DispatcherTimer _testTimer;
        private char _currentDisplayedLetter;
        private bool _isTargetLetterDisplaying = false;
        private int _remainingSeconds;
        private readonly Random _random = new Random();
        private int _consecutiveNonTargetCount = 0;
        private bool _lastWasTarget = false;
        private int _currentTargetPressCount = 0;

        public event Action<char, bool>? OnLetterDisplayed;
        public event Action? OnLetterHide;
        public event Action? OnTestCompleted;
        public event Action<int>? OnTimeUpdated;

        public string DisplayedLetter { get; set; } = "";
        public string TimeDisplay { get; set; } = "00:00";
        public string StatisticsDisplay { get; set; } = "";
        public int DurationMinutes { get; set; }
        public int DisplayIntervalMs { get; private set; }
        public int GapIntervalMs { get; private set; }
        private bool _inGapPhase = false;

        public HedefAvcisiViewModel(int durationMinutes, int displayIntervalMs = 500, int gapIntervalMs = 200)
        {
            DurationMinutes = durationMinutes;
            DisplayIntervalMs = Math.Max(100, displayIntervalMs);
            GapIntervalMs = Math.Max(0, gapIntervalMs);
            _remainingSeconds = durationMinutes * 60;
            _reactionTimeService = new ReactionTimeService();
            _testResult = new TestResult { DurationMinutes = durationMinutes };

            InitializeTimers();
        }

        private void InitializeTimers()
        {
            // Display Timer - Kullanıcının seçtiği aralıkta yeni harf göster
            _displayTimer = new DispatcherTimer();
            _displayTimer.Interval = TimeSpan.FromMilliseconds(DisplayIntervalMs);
            _displayTimer.Tick += DisplayTimer_Tick;

            // Test Timer - Genel test süresi kontrolü
            _testTimer = new DispatcherTimer();
            _testTimer.Interval = TimeSpan.FromSeconds(1);
            _testTimer.Tick += TestTimer_Tick;
        }

        public void StartTest()
        {
            _inGapPhase = false;
            _displayTimer.Interval = TimeSpan.FromMilliseconds(DisplayIntervalMs);
            char target = _reactionTimeService.PickRandomTarget();
            _testResult.FixedTargetLetter = target;
            _reactionTimeService.StartWaiting();
            ShowNextLetter();
            _displayTimer.Start();
            _testTimer.Start();
        }

        public char GetTargetLetter() => _reactionTimeService.GetTargetLetter();

        private void DisplayTimer_Tick(object? sender, EventArgs e)
        {
            if (_remainingSeconds <= 0)
            {
                EndTest();
                return;
            }

            // Bekleme fazındaysak: bekleme bitti, harf gösterim fazına geç
            if (_inGapPhase)
            {
                _inGapPhase = false;
                _displayTimer.Interval = TimeSpan.FromMilliseconds(DisplayIntervalMs);
                ShowNextLetter();
                return;
            }

            // Harf gösterim fazı bitti: boşluk fazına geç (GapIntervalMs > 0 ise)
            OnLetterHide?.Invoke();
            _isTargetLetterDisplaying = false;
            _currentTargetPressCount = 0;

            if (GapIntervalMs > 0)
            {
                _inGapPhase = true;
                _displayTimer.Interval = TimeSpan.FromMilliseconds(GapIntervalMs);
            }
            else
            {
                ShowNextLetter();
            }
        }

        private void ShowNextLetter()
        {
            bool showTarget;
            if (_lastWasTarget)
            {
                showTarget = false;
            }
            else if (_consecutiveNonTargetCount >= 5)
            {
                showTarget = true;
            }
            else
            {
                showTarget = _random.Next(10) < 3;
            }

            if (showTarget)
            {
                _currentDisplayedLetter = _reactionTimeService.GetTargetLetter();
                _isTargetLetterDisplaying = true;
                _lastWasTarget = true;
                _consecutiveNonTargetCount = 0;
                _currentTargetPressCount = 0;
                _reactionTimeService.TargetLetterAppeared();
            }
            else
            {
                _currentDisplayedLetter = _reactionTimeService.GetRandomLetter();
                _isTargetLetterDisplaying = false;
                _lastWasTarget = false;
                _consecutiveNonTargetCount++;
            }

            DisplayedLetter = _currentDisplayedLetter.ToString();
            OnLetterDisplayed?.Invoke(_currentDisplayedLetter, _isTargetLetterDisplaying);
        }

        private void TestTimer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;
            int minutes = _remainingSeconds / 60;
            int seconds = _remainingSeconds % 60;
            TimeDisplay = $"{minutes:D2}:{seconds:D2}";
            OnTimeUpdated?.Invoke(_remainingSeconds);

            if (_remainingSeconds <= 0)
            {
                EndTest();
            }
        }

        public void HandleSpaceKeyPressed()
        {
            if (_isTargetLetterDisplaying)
            {
                _currentTargetPressCount++;
                double waitTime = _currentTargetPressCount == 1 ? _reactionTimeService.GetWaitTime() : 0;
                double responseTime = _reactionTimeService.GetResponseTime();
                _testResult.ReactionTimes.Add(new ReactionTime
                {
                    TargetLetter = _reactionTimeService.GetTargetLetter(),
                    DisplayedLetter = _currentDisplayedLetter,
                    WaitTimeMs = waitTime,
                    ResponseTimeMs = responseTime,
                    IsCorrect = true,
                    PressOrder = _currentTargetPressCount
                });
                if (_currentTargetPressCount == 1)
                    _reactionTimeService.StartWaiting();
            }
            else if (_currentDisplayedLetter != default)
            {
                // Yanlış basış — hedef dışı harf gösterilirken veya harf yokken basıldı
                _testResult.ReactionTimes.Add(new ReactionTime
                {
                    TargetLetter = _reactionTimeService.GetTargetLetter(),
                    DisplayedLetter = _currentDisplayedLetter,
                    WaitTimeMs = 0,
                    ResponseTimeMs = 0,
                    IsCorrect = false
                });
            }
        }

        private void EndTest()
        {
            _displayTimer.Stop();
            _testTimer.Stop();
            StatisticsDisplay = FormatStatistics();
            OnTestCompleted?.Invoke();
        }

        private string FormatStatistics()
        {
            if (_testResult.ReactionTimes.Count == 0)
                return "Hiç tepki kaydedilmedi.";

            return $"Ortalama Reaksiyon Süresi: {_testResult.AverageReactionTime:F2}ms\n" +
                   $"En Hızlı: {_testResult.MinReactionTime:F2}ms\n" +
                   $"En Yavaş: {_testResult.MaxReactionTime:F2}ms\n" +
                   $"Toplam Tepkiler: {_testResult.ReactionTimes.Count}";
        }

        public TestResult GetTestResult() => _testResult;
        public void Stop() { _displayTimer.Stop(); _testTimer.Stop(); }
        public void Resume() { _displayTimer.Start(); _testTimer.Start(); }
    }
}
