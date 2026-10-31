using System;
using System.Windows.Threading;
using PsikologTestApp.Tests.DogruHedef.Models;

namespace PsikologTestApp.Tests.DogruHedef.ViewModels
{
    public class DogruHedefViewModel
    {
        private static readonly Random _rng = new();

        private readonly DogruHedefResult _result;
        private readonly DogruHedefAnimal _targetAnimal;
        private readonly int              _minMs;
        private readonly int              _maxMs;

        private DispatcherTimer? _segmentTimer;
        private DispatcherTimer  _testTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        private DogruHedefSegment? _currentSegment;
        private DateTime           _segmentStart;
        private double             _lastAnimalDurationMs;
        private double             _segmentRemainingMs;
        private Action             _onSegmentEnd = () => {};

        private int _remainingSeconds;

        // ── events ────────────────────────────────────────────────────────
        /// <summary>Fired when a new segment starts. Null animal = gap/blank. durationMs = display time.</summary>
        public event Action<DogruHedefAnimal?, double>? OnSegmentStarted;
        public event Action<int>?                       OnTimeUpdated;
        public event Action?                            OnTestCompleted;

        public string TimeDisplay => $"{_remainingSeconds / 60:D2}:{_remainingSeconds % 60:D2}";

        public DogruHedefViewModel(DogruHedefAnimal target, int durationMinutes, int minMs = 100, int maxMs = 10000)
        {
            _targetAnimal     = target;
            _minMs            = Math.Max(1, minMs);
            _maxMs            = Math.Max(_minMs, maxMs);
            _remainingSeconds = durationMinutes * 60;

            _result = new DogruHedefResult
            {
                TargetAnimalName = target.Name,
                DurationMinutes  = durationMinutes,
            };

            _testTimer.Tick += TestTimerTick;
        }

        // ── public API ────────────────────────────────────────────────────

        public void StartTest()
        {
            _testTimer.Start();
            ShowAnimal(_targetAnimal);   // first stimulus is always the target
        }

        public void HandleSpacePress()
        {
            if (_currentSegment == null) return;
            _currentSegment.Presses.Add(new PressRecord
            {
                TimeFromSegmentStartMs = (DateTime.Now - _segmentStart).TotalMilliseconds
            });
        }

        public void Stop()
        {
            _segmentRemainingMs -= (DateTime.Now - _segmentStart).TotalMilliseconds;
            if (_segmentRemainingMs < 1) _segmentRemainingMs = 1;
            _segmentTimer?.Stop();
            _testTimer.Stop();
        }

        public void Resume()
        {
            _segmentStart = DateTime.Now;   // reset reference for press timing
            _testTimer.Start();
            ArmSegmentTimer(_segmentRemainingMs);
        }

        public DogruHedefResult GetResult() => _result;

        // ── internal flow ─────────────────────────────────────────────────

        private void ShowAnimal(DogruHedefAnimal animal)
        {
            bool isTarget         = animal.Name == _targetAnimal.Name;
            _lastAnimalDurationMs = _rng.Next(_minMs, _maxMs + 1);

            _currentSegment = new DogruHedefSegment
            {
                SegmentIndex = _result.Segments.Count,
                Kind         = isTarget ? SegmentKind.Target : SegmentKind.OtherAnimal,
                AnimalName   = animal.Name,
                DurationMs   = _lastAnimalDurationMs,
            };
            _result.Segments.Add(_currentSegment);
            _segmentStart       = DateTime.Now;
            _segmentRemainingMs = _lastAnimalDurationMs;
            _onSegmentEnd       = ShowGap;

            OnSegmentStarted?.Invoke(animal, _lastAnimalDurationMs);
            ArmSegmentTimer(_lastAnimalDurationMs);
        }

        private void ShowGap()
        {
            _currentSegment = new DogruHedefSegment
            {
                SegmentIndex = _result.Segments.Count,
                Kind         = SegmentKind.Gap,
                AnimalName   = "",
                DurationMs   = _lastAnimalDurationMs,   // same duration as preceding animal
            };
            _result.Segments.Add(_currentSegment);
            _segmentStart       = DateTime.Now;
            _segmentRemainingMs = _lastAnimalDurationMs;
            _onSegmentEnd       = ShowRandomAnimal;

            OnSegmentStarted?.Invoke(null, 0);
            ArmSegmentTimer(_lastAnimalDurationMs);
        }

        private void ShowRandomAnimal()
            => ShowAnimal(DogruHedefAnimal.All[_rng.Next(DogruHedefAnimal.All.Length)]);

        private void ArmSegmentTimer(double ms)
        {
            _segmentTimer?.Stop();
            _segmentTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Max(1, ms)) };
            _segmentTimer.Tick += (_, __) =>
            {
                _segmentTimer?.Stop();
                _segmentTimer = null;
                if (_remainingSeconds > 0)
                    _onSegmentEnd();
            };
            _segmentTimer.Start();
        }

        private void TestTimerTick(object? sender, EventArgs e)
        {
            _remainingSeconds--;
            OnTimeUpdated?.Invoke(_remainingSeconds);
            if (_remainingSeconds <= 0)
            {
                _segmentTimer?.Stop();
                _testTimer.Stop();
                OnTestCompleted?.Invoke();
            }
        }
    }
}
