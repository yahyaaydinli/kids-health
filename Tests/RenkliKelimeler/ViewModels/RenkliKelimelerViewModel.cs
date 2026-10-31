using System.Diagnostics;
using System.Windows.Threading;
using PsikologTestApp.Tests.RenkliKelimeler.Models;

namespace PsikologTestApp.Tests.RenkliKelimeler.ViewModels
{
    public class RenkliKelimelerViewModel
    {
        public static readonly string[] Colors = { "KIRMIZI", "MAVİ", "YEŞİL", "SARI" };
        private readonly Random _random = new();

        private StroopTestResult _testResult;
        private DispatcherTimer _testTimer;
        private Stopwatch _responseTimer = new();
        private int _remainingSeconds;

        private string _currentWord = "";
        private string _currentDisplayColor = "";
        private bool _waitingForResponse = false;
        private int _trialNumber = 0;

        /// <summary>Kelime ve görüntü rengi hazır olduğunda ateşlenir.</summary>
        public event Action<string, string>? OnWordDisplayed;
        /// <summary>Kullanıcı bir butona basınca ateşlenir (isCorrect).</summary>
        public event Action<bool>? OnResponseGiven;
        public event Action? OnTestCompleted;
        public event Action<int>? OnTimeUpdated;

        public int DurationMinutes { get; }

        public RenkliKelimelerViewModel(int durationMinutes)
        {
            DurationMinutes = durationMinutes;
            _remainingSeconds = durationMinutes * 60;
            _testResult = new StroopTestResult { DurationMinutes = durationMinutes };

            _testTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _testTimer.Tick += TestTimer_Tick;
        }

        public void StartTest()
        {
            _testTimer.Start();
            ShowNextWord();
        }

        public void ShowNextWord()
        {
            _currentWord = Colors[_random.Next(Colors.Length)];

            // Gösterim rengi kelimenin kendi rengi OLMAMALI
            string[] others = Colors.Where(c => c != _currentWord).ToArray();
            _currentDisplayColor = others[_random.Next(others.Length)];

            _trialNumber++;
            _waitingForResponse = true;
            _responseTimer.Restart();

            OnWordDisplayed?.Invoke(_currentWord, _currentDisplayColor);
        }

        /// <summary>
        /// Kullanıcı bir renk butonuna bastığında çağrılır.
        /// Geri dönüş: doğru mu?
        /// </summary>
        public bool HandleColorButtonPressed(string selectedColor)
        {
            if (!_waitingForResponse) return false;
            _waitingForResponse = false;
            _responseTimer.Stop();

            bool isCorrect = selectedColor == _currentDisplayColor;

            _testResult.Trials.Add(new StroopTrial
            {
                TrialNumber = _trialNumber,
                WordText = _currentWord,
                DisplayColor = _currentDisplayColor,
                SelectedColor = selectedColor,
                IsCorrect = isCorrect,
                ResponseTimeMs = _responseTimer.Elapsed.TotalMilliseconds
            });

            OnResponseGiven?.Invoke(isCorrect);
            return isCorrect;
        }

        private void TestTimer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;
            OnTimeUpdated?.Invoke(_remainingSeconds);

            if (_remainingSeconds <= 0)
            {
                _testTimer.Stop();
                OnTestCompleted?.Invoke();
            }
        }

        public StroopTestResult GetTestResult() => _testResult;
        public void Stop() => _testTimer.Stop();
        public void Resume() => _testTimer.Start();
    }
}
