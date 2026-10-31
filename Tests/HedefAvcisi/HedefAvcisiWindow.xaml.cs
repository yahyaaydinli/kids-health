using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PsikologTestApp;
using PsikologTestApp.Tests.HedefAvcisi.ViewModels;

namespace PsikologTestApp.Tests.HedefAvcisi
{
    public partial class HedefAvcisiWindow : Window
    {
        private HedefAvcisiViewModel _viewModel;
        private bool _testStarted = false;
        private bool _isPaused = false;

        public HedefAvcisiWindow(int durationMinutes, int displayIntervalMs = 500, int gapIntervalMs = 200)
        {
            InitializeComponent();
            _viewModel = new HedefAvcisiViewModel(durationMinutes, displayIntervalMs, gapIntervalMs);
            
            // Event handlers
            _viewModel.OnLetterDisplayed += OnLetterDisplayed;
            _viewModel.OnLetterHide += OnLetterHide;
            _viewModel.OnTimeUpdated += OnTimeUpdated;
            _viewModel.OnTestCompleted += OnTestCompleted;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_testStarted && e.Key != Key.Escape)
            {
                _testStarted = true;
                _viewModel.StartTest();
                HintBlock.Text = "SPACE tuşu bekleniyor...";
                TargetLetterDisplay.Text = _viewModel.GetTargetLetter().ToString();
                return;
            }

            if (e.Key == Key.Space && _testStarted && !_isPaused)
            {
                _viewModel.HandleSpaceKeyPressed();
            }

            if (e.Key == Key.Escape && _testStarted)
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            if (_isPaused) ResumeTest();
            else PauseTest();
        }

        private void PauseTest()
        {
            _isPaused = true;
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

        private void OnTargetLetterChanged(char letter)
        {
            Dispatcher.Invoke(() =>
            {
                TargetLetterDisplay.Text = letter.ToString();
            });
        }

        private void OnLetterHide()
        {
            Dispatcher.Invoke(() =>
            {
                DisplayedLetterBlock.BeginAnimation(UIElement.OpacityProperty, null);
                DisplayedLetterBlock.Opacity = 0;
            });
        }

        private void OnLetterDisplayed(char letter, bool isTarget)
        {
            Dispatcher.Invoke(() =>
            {
                // Önceki animasyonu iptal et - tutarsız opacity'ye yol açmasın
                DisplayedLetterBlock.BeginAnimation(UIElement.OpacityProperty, null);

                Brush targetBrush = isTarget ? Brushes.LimeGreen : Brushes.White;
                DisplayedLetterBlock.Text = letter.ToString();
                DisplayedLetterBlock.Foreground = targetBrush;

                int interval = _viewModel.DisplayIntervalMs;
                int fadeDuration = Math.Min(50, interval / 8);

                if (fadeDuration >= 15)
                {
                    // Anında gizle, sonra fade-in — her harf aynı süre görünür
                    DisplayedLetterBlock.Opacity = 0;
                    var fadeIn = new DoubleAnimation(0, 0.9,
                        new Duration(TimeSpan.FromMilliseconds(fadeDuration)))
                    {
                        FillBehavior = FillBehavior.HoldEnd
                    };
                    DisplayedLetterBlock.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
                else
                {
                    DisplayedLetterBlock.Opacity = 0.9;
                }
            });
        }

        private void OnTimeUpdated(int remainingSeconds)
        {
            Dispatcher.Invoke(() =>
            {
                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;
                TimeDisplay.Text = $"{minutes:D2}:{seconds:D2}";
            });
        }

        private void OnTestCompleted()
        {
            Dispatcher.Invoke(() =>
            {
                EndTest();
            });
        }

        private void EndTest()
        {
            _viewModel.Stop();
            var testResult = _viewModel.GetTestResult();
            HedefAvcisiResultWindow resultWindow = new HedefAvcisiResultWindow(testResult);
            resultWindow.Show();
            this.Close();
        }
    }
}
