using System.Diagnostics;
using System.Windows.Threading;
using PsikologTestApp.Tests.YuruYurume.Models;

namespace PsikologTestApp.Tests.YuruYurume.ViewModels
{
    public class YuruYurumeViewModel
    {
        private readonly Random _random = new();
        private GoNoGoResult _result;
        private DispatcherTimer _displayTimer;
        private DispatcherTimer _testTimer;
        private Stopwatch _responseTimer = new();

        private int _remainingSeconds;
        private int _trialNumber = 0;
        private bool _stimulusActive = false;
        private bool _currentIsGo = false;
        private bool _responded = false;
        private bool _inGapPhase = false;
        private GoNoGoTrial? _currentTrial = null;

        public int GoPercentage { get; }
        public int DisplayIntervalMs { get; }
        public int GapIntervalMs { get; }

        /// <summary>Yeni uyaran gösterildiğinde: (isGo)</summary>
        public event Action<bool>? OnStimulusShown;
        /// <summary>Uyaran gizlendiğinde</summary>
        public event Action? OnStimulusHidden;
        /// <summary>Deneme sonuçlandığında: (TrialType)</summary>
        public event Action<TrialType>? OnTrialCompleted;
        public event Action? OnTestCompleted;
        public event Action<int>? OnTimeUpdated;

        public YuruYurumeViewModel(int durationMinutes, int goPercentage, int displayIntervalMs, int gapIntervalMs)
        {
            GoPercentage = goPercentage;
            DisplayIntervalMs = Math.Max(200, displayIntervalMs);
            GapIntervalMs = Math.Max(0, gapIntervalMs);
            _remainingSeconds = durationMinutes * 60;

            _result = new GoNoGoResult
            {
                DurationMinutes = durationMinutes,
                GoPercentage = goPercentage
            };

            _displayTimer = new DispatcherTimer();
            _displayTimer.Tick += DisplayTimer_Tick;

            _testTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _testTimer.Tick += TestTimer_Tick;
        }

        public void StartTest()
        {
            _testTimer.Start();
            ShowNextStimulus();
        }

        public void Stop()
        {
            _displayTimer.Stop();
            _testTimer.Stop();
        }

        public void Resume()
        {
            _displayTimer.Start();
            _testTimer.Start();
        }

        private void ShowNextStimulus()
        {
            _currentIsGo = _random.Next(100) < GoPercentage;
            _stimulusActive = true;
            _responded = false;
            _currentTrial = null;
            _trialNumber++;
            _inGapPhase = false;

            _responseTimer.Restart();
            _displayTimer.Interval = TimeSpan.FromMilliseconds(DisplayIntervalMs);
            _displayTimer.Start();

            OnStimulusShown?.Invoke(_currentIsGo);
        }

        private void DisplayTimer_Tick(object? sender, EventArgs e)
        {
            _displayTimer.Stop();

            if (_inGapPhase)
            {
                _inGapPhase = false;
                ShowNextStimulus();
                return;
            }

            // Gösterim süresi bitti
            _stimulusActive = false;

            if (!_responded)
            {
                TrialType type = _currentIsGo ? TrialType.Miss : TrialType.CorrectRejection;
                _result.Trials.Add(new GoNoGoTrial
                {
                    TrialNumber = _trialNumber,
                    IsGoStimulus = _currentIsGo,
                    Pressed = false,
                    ResponseTimeMs = 0,
                    Type = type
                });
                OnTrialCompleted?.Invoke(type);
            }

            OnStimulusHidden?.Invoke();

            if (GapIntervalMs > 0)
            {
                _inGapPhase = true;
                _displayTimer.Interval = TimeSpan.FromMilliseconds(GapIntervalMs);
                _displayTimer.Start();
            }
            else
            {
                ShowNextStimulus();
            }
        }

        public void HandleSpaceKeyPressed()
        {
            if (!_stimulusActive) return;

            if (!_responded)
            {
                _responded = true;
                _responseTimer.Stop();

                TrialType type = _currentIsGo ? TrialType.Hit : TrialType.FalseAlarm;
                _currentTrial = new GoNoGoTrial
                {
                    TrialNumber    = _trialNumber,
                    IsGoStimulus   = _currentIsGo,
                    Pressed        = true,
                    PressCount     = 1,
                    ResponseTimeMs = _responseTimer.Elapsed.TotalMilliseconds,
                    Type           = type
                };
                _result.Trials.Add(_currentTrial);
                OnTrialCompleted?.Invoke(type);
            }
            else if (_currentTrial != null)
            {
                // Subsequent presses on the same stimulus — count them
                _currentTrial.PressCount++;
            }
        }

        private void TestTimer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;
            OnTimeUpdated?.Invoke(_remainingSeconds);

            if (_remainingSeconds <= 0)
            {
                _displayTimer.Stop();
                _testTimer.Stop();
                OnTestCompleted?.Invoke();
            }
        }

        public GoNoGoResult GetResult() => _result;
    }
}
