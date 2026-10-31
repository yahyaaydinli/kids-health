using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PsikologTestApp;
using PsikologTestApp.Tests.RenkliKelimeler.ViewModels;

namespace PsikologTestApp.Tests.RenkliKelimeler
{
    public partial class RenkliKelimelerWindow : Window
    {
        private static readonly Dictionary<string, Color> ColorMap = new()
        {
            { "KIRMIZI", Color.FromRgb(229, 57,  53)  },
            { "MAVİ",    Color.FromRgb(30,  136, 229) },
            { "YEŞİL",   Color.FromRgb(67,  160, 71)  },
            { "SARI",    Color.FromRgb(253, 216, 53)  },
        };

        private RenkliKelimelerViewModel _viewModel;
        private bool _testStarted = false;
        private bool _buttonsEnabled = false;
        private int _correctCount = 0;
        private int _wrongCount = 0;
        private DispatcherTimer _feedbackTimer;
        private bool _isPaused = false;
        private bool _wasInFeedback = false;

        public RenkliKelimelerWindow(int durationMinutes)
        {
            InitializeComponent();
            _viewModel = new RenkliKelimelerViewModel(durationMinutes);
            _viewModel.OnWordDisplayed   += OnWordDisplayed;
            _viewModel.OnResponseGiven   += OnResponseGiven;
            _viewModel.OnTestCompleted   += OnTestCompleted;
            _viewModel.OnTimeUpdated     += OnTimeUpdated;

            TimeDisplay.Text = $"{durationMinutes:D2}:00";

            _feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
            _feedbackTimer.Tick += FeedbackTimer_Tick;
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
            if (e.Key == Key.Escape && _testStarted)
                TogglePause();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_testStarted || !_buttonsEnabled || _isPaused) return;
            if (sender is Button btn && btn.Tag is string color)
            {
                _buttonsEnabled = false;
                _viewModel.HandleColorButtonPressed(color);
            }
        }

        private void OnWordDisplayed(string word, string displayColor)
        {
            Dispatcher.Invoke(() =>
            {
                if (!ColorMap.TryGetValue(displayColor, out Color c))
                    c = Colors.White;

                WordDisplay.Text = word;
                WordDisplay.Foreground = new SolidColorBrush(c);

                // Fade-in
                WordDisplay.Opacity = 0;
                var fadeIn = new DoubleAnimation(0, 1,
                    new Duration(TimeSpan.FromMilliseconds(80)))
                { FillBehavior = FillBehavior.HoldEnd };
                WordDisplay.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                _buttonsEnabled = true;
            });
        }

        private void OnResponseGiven(bool isCorrect)
        {
            Dispatcher.Invoke(() =>
            {
                if (isCorrect)
                {
                    _correctCount++;
                    CorrectCountDisplay.Text = _correctCount.ToString();
                    ShowFeedback("✓ Doğru!", Brushes.LimeGreen);
                }
                else
                {
                    _wrongCount++;
                    WrongCountDisplay.Text = _wrongCount.ToString();
                    ShowFeedback("✗ Yanlış!", Brushes.OrangeRed);
                }
            });
        }

        private void ShowFeedback(string message, Brush color)
        {
            FeedbackText.Text = message;
            FeedbackText.Foreground = color;
            FeedbackBorder.Background = new SolidColorBrush(
                Color.FromArgb(40, ((SolidColorBrush)color).Color.R,
                                   ((SolidColorBrush)color).Color.G,
                                   ((SolidColorBrush)color).Color.B));
            FeedbackBorder.Opacity = 1;

            // Kelimeyi gizle — kısa flash etkisi
            WordDisplay.BeginAnimation(UIElement.OpacityProperty, null);
            WordDisplay.Opacity = 0;

            _feedbackTimer.Stop();
            _feedbackTimer.Start();
        }

        private void FeedbackTimer_Tick(object? sender, EventArgs e)
        {
            _feedbackTimer.Stop();
            Dispatcher.Invoke(() =>
            {
                FeedbackBorder.Opacity = 0;
                _viewModel.ShowNextWord();
            });
        }

        private void OnTimeUpdated(int remaining)
        {
            Dispatcher.Invoke(() =>
            {
                int m = remaining / 60;
                int s = remaining % 60;
                TimeDisplay.Text = $"{m:D2}:{s:D2}";
            });
        }

        private void OnTestCompleted()
        {
            Dispatcher.Invoke(() => EndTest());
        }

        private void TogglePause()
        {
            if (_isPaused) ResumeTest();
            else PauseTest();
        }

        private void PauseTest()
        {
            _isPaused = true;
            _buttonsEnabled = false;
            _wasInFeedback = _feedbackTimer.IsEnabled;
            _feedbackTimer.Stop();
            _viewModel.Stop();
            PauseOverlay.Visibility = Visibility.Visible;
        }

        private void ResumeTest()
        {
            _isPaused = false;
            PauseOverlay.Visibility = Visibility.Collapsed;
            _viewModel.Resume();
            if (_wasInFeedback)
            {
                _wasInFeedback = false;
                FeedbackBorder.Opacity = 0;
                _viewModel.ShowNextWord(); // OnWordDisplayed sets _buttonsEnabled = true
            }
            else
            {
                _buttonsEnabled = true;
            }
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e) => ResumeTest();
        private void EndTestButton_Click(object sender, RoutedEventArgs e) => EndTest();

        private void EndTest()
        {
            _viewModel.Stop();
            _feedbackTimer.Stop();
            var result = _viewModel.GetTestResult();
            var resultWindow = new RenkliKelimelerResultWindow(result);
            resultWindow.Show();
            this.Close();
        }
    }
}
