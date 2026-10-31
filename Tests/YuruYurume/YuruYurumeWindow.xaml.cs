using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PsikologTestApp;
using PsikologTestApp.Tests.YuruYurume.Models;
using PsikologTestApp.Tests.YuruYurume.ViewModels;

namespace PsikologTestApp.Tests.YuruYurume
{
    public partial class YuruYurumeWindow : Window
    {
        private static readonly SolidColorBrush GoColor   = new(Color.FromRgb(67, 160, 71));   // #43A047
        private static readonly SolidColorBrush NoGoColor = new(Color.FromRgb(229, 57, 53));   // #E53935

        private YuruYurumeViewModel _viewModel;
        private bool _testStarted = false;
        private int _hitCount = 0;
        private int _missCount = 0;
        private int _falseAlarmCount = 0;
        private bool _isPaused = false;

        private DispatcherTimer _feedbackTimer;

        public YuruYurumeWindow(int durationMinutes, int goPercentage, int displayMs, int gapMs)
        {
            InitializeComponent();
            _viewModel = new YuruYurumeViewModel(durationMinutes, goPercentage, displayMs, gapMs);
            _viewModel.OnStimulusShown   += OnStimulusShown;
            _viewModel.OnStimulusHidden  += OnStimulusHidden;
            _viewModel.OnTrialCompleted  += OnTrialCompleted;
            _viewModel.OnTestCompleted   += OnTestCompleted;
            _viewModel.OnTimeUpdated     += OnTimeUpdated;

            TimeDisplay.Text = $"{durationMinutes:D2}:00";

            _feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _feedbackTimer.Tick += (s, e) =>
            {
                _feedbackTimer.Stop();
                Dispatcher.Invoke(() => FeedbackBorder.Opacity = 0);
            };
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_testStarted && e.Key != Key.Escape)
            {
                _testStarted = true;
                HintBlock.Visibility = Visibility.Collapsed;
                _viewModel.StartTest();
                return;
            }
            if (e.Key == Key.Space && _testStarted && !_isPaused)
                _viewModel.HandleSpaceKeyPressed();
            if (e.Key == Key.Escape && _testStarted)
                TogglePause();
        }

        private void OnStimulusShown(bool isGo)
        {
            Dispatcher.Invoke(() =>
            {
                StimulusCircle.Background = isGo ? GoColor : NoGoColor;

                StimulusCircle.BeginAnimation(OpacityProperty, null);
                StimulusCircle.Opacity = 0;
                var fadeIn = new DoubleAnimation(0, 1,
                    new Duration(TimeSpan.FromMilliseconds(60)))
                { FillBehavior = FillBehavior.HoldEnd };
                StimulusCircle.BeginAnimation(OpacityProperty, fadeIn);
            });
        }

        private void OnStimulusHidden()
        {
            Dispatcher.Invoke(() =>
            {
                StimulusCircle.BeginAnimation(OpacityProperty, null);
                StimulusCircle.Opacity = 0;
                FeedbackBorder.Opacity = 0;
                _feedbackTimer.Stop();
            });
        }

        private void OnTrialCompleted(TrialType type)
        {
            Dispatcher.Invoke(() =>
            {
                string msg;
                Brush fg;
                switch (type)
                {
                    case TrialType.Hit:
                        _hitCount++;
                        HitDisplay.Text = _hitCount.ToString();
                        msg = "✓ Doğru!";
                        fg = Brushes.LimeGreen;
                        break;
                    case TrialType.Miss:
                        _missCount++;
                        MissDisplay.Text = _missCount.ToString();
                        msg = "⏱ Kaçırdın!";
                        fg = Brushes.Orange;
                        break;
                    case TrialType.FalseAlarm:
                        _falseAlarmCount++;
                        FalseAlarmDisplay.Text = _falseAlarmCount.ToString();
                        msg = "✗ Yanlış Bastın!";
                        fg = Brushes.OrangeRed;
                        break;
                    default: // CorrectRejection — sessiz geri bildirim
                        return;
                }

                FeedbackText.Text = msg;
                FeedbackText.Foreground = fg;
                FeedbackBorder.Background = new SolidColorBrush(
                    Color.FromArgb(45,
                        ((SolidColorBrush)fg).Color.R,
                        ((SolidColorBrush)fg).Color.G,
                        ((SolidColorBrush)fg).Color.B));
                FeedbackBorder.Opacity = 1;
                _feedbackTimer.Stop();
                _feedbackTimer.Start();
            });
        }

        private void OnTimeUpdated(int remaining)
        {
            Dispatcher.Invoke(() =>
            {
                int m = remaining / 60, s = remaining % 60;
                TimeDisplay.Text = $"{m:D2}:{s:D2}";
            });
        }

        private void OnTestCompleted() => Dispatcher.Invoke(EndTest);

        private void TogglePause()
        {
            if (_isPaused) ResumeTest();
            else PauseTest();
        }

        private void PauseTest()
        {
            _isPaused = true;
            _feedbackTimer.Stop();
            _viewModel.Stop();
            PauseOverlay.Visibility = Visibility.Visible;
        }

        private void ResumeTest()
        {
            _isPaused = false;
            PauseOverlay.Visibility = Visibility.Collapsed;
            _viewModel.Resume();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e) => ResumeTest();
        private void EndTestButton_Click(object sender, RoutedEventArgs e) => EndTest();

        private void EndTest()
        {
            _viewModel.Stop();
            _feedbackTimer.Stop();
            var result = _viewModel.GetResult();
            new YuruYurumeResultWindow(result).Show();
            this.Close();
        }
    }
}
