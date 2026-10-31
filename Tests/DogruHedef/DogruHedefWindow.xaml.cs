using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PsikologTestApp.Tests.DogruHedef.Models;
using PsikologTestApp.Tests.DogruHedef.ViewModels;

namespace PsikologTestApp.Tests.DogruHedef
{
    public partial class DogruHedefWindow : Window
    {
        private readonly DogruHedefViewModel _vm;
        private readonly DogruHedefAnimal    _targetAnimal;
        private readonly int                 _countdownSeconds;
        private int                          _countdownRemaining;

        private bool _testStarted;
        private bool _isPaused;
        private bool _testEnded;

        // Live counters (updated on each SPACE press)
        private int _livePressesOnTarget;
        private int _livePressesOnFalse;

        // Current segment state (set by OnSegmentStarted)
        private bool _currentIsTarget;

        public DogruHedefWindow(DogruHedefAnimal target, int durationMinutes, int countdownSeconds = 3, int minMs = 100, int maxMs = 3000)
        {
            InitializeComponent();
            _targetAnimal     = target;
            _countdownSeconds = countdownSeconds;

            _vm = new DogruHedefViewModel(target, durationMinutes, minMs, maxMs);
            _vm.OnSegmentStarted += OnSegmentStarted;
            _vm.OnTimeUpdated    += t => Dispatcher.Invoke(() => TimerBlock.Text = $"{t / 60:D2}:{t % 60:D2}");
            _vm.OnTestCompleted  += () => Dispatcher.Invoke(EndTest);

            Loaded += OnLoaded;
        }

        // ── startup ───────────────────────────────────────────────────────

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TargetEmojiLabel.Text        = _targetAnimal.Emoji;
            TargetNameLabel.Text         = _targetAnimal.Name;
            TimerBlock.Text              = _vm.TimeDisplay;
            _countdownRemaining          = _countdownSeconds;
            CountdownNumber.Text         = _countdownRemaining.ToString();
            CountdownOverlay.Visibility  = Visibility.Visible;
            RunCountdown();
        }

        private void RunCountdown()
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            t.Tick += (_, __) =>
            {
                _countdownRemaining--;
                if (_countdownRemaining > 0)
                {
                    CountdownNumber.Text = _countdownRemaining.ToString();
                }
                else
                {
                    t.Stop();
                    CountdownOverlay.Visibility = Visibility.Collapsed;
                    _testStarted = true;
                    _vm.StartTest();
                }
            };
            t.Start();
        }

        // ── segment display ───────────────────────────────────────────────

        private void OnSegmentStarted(DogruHedefAnimal? animal, double durationMs)
        {
            Dispatcher.Invoke(() =>
            {
                if (animal == null)
                {
                    // Gap / blank
                    _currentIsTarget             = false;
                    AnimalDisplayCard.Visibility = Visibility.Collapsed;
                    FixationCross.Visibility     = Visibility.Visible;
                    DurationMsBlock.Visibility   = Visibility.Collapsed;
                    InstructionBlock.Text        = "⬜  Boşluk...";
                    InstructionBlock.Foreground  = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
                }
                else
                {
                    // Animal stimulus
                    _currentIsTarget = animal.Name == _targetAnimal.Name;

                    FixationCross.Visibility       = Visibility.Collapsed;
                    StimulusEmojiBlock.Text        = animal.Emoji;
                    AnimalDisplayCard.Background   = Brushes.White;
                    AnimalDisplayCard.Visibility   = Visibility.Visible;
                    DurationMsBlock.Text           = $"⏱ {durationMs:F0} ms";
                    DurationMsBlock.Visibility     = Visibility.Visible;

                    InstructionBlock.Text       = "SPACE — Hedef hayvanı gördüğünde bas";
                    InstructionBlock.Foreground = new SolidColorBrush(Color.FromRgb(0x54, 0x6E, 0x7A));
                }
            });
        }

        // ── keyboard handling ─────────────────────────────────────────────

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CountdownOverlay.Visibility == Visibility.Collapsed)
            {
                if (_isPaused) ResumeTest();
                else PauseTest();
                return;
            }

            if (e.Key == Key.Space && _testStarted && !_isPaused && !_testEnded)
            {
                _vm.HandleSpacePress();

                if (_currentIsTarget) _livePressesOnTarget++;
                else                  _livePressesOnFalse++;

                HitBlock.Text   = _livePressesOnTarget.ToString();
                FalseBlock.Text = _livePressesOnFalse.ToString();
            }
        }

        // ── pause / resume ────────────────────────────────────────────────

        private void PauseTest()
        {
            _isPaused = true;
            _vm.Stop();
            PauseOverlay.Visibility = Visibility.Visible;
        }

        private void ResumeTest()
        {
            _isPaused = false;
            _vm.Resume();
            PauseOverlay.Visibility = Visibility.Collapsed;
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e) => ResumeTest();
        private void EndTestButton_Click(object sender, RoutedEventArgs e) => EndTest();

        // ── end test ─────────────────────────────────────────────────────

        private void EndTest()
        {
            if (_testEnded) return;
            _testEnded = true;
            _vm.Stop();
            new DogruHedefResultWindow(_vm.GetResult()).Show();
            this.Close();
        }

        // ── helpers ───────────────────────────────────────────────────────

        private static SolidColorBrush HexBrush(string hex)
            => new((Color)ColorConverter.ConvertFromString(hex));
    }
}
