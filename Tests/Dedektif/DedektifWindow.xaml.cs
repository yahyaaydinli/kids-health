using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PsikologTestApp.Tests.Dedektif.Models;
using PsikologTestApp.Tests.Dedektif.ViewModels;

namespace PsikologTestApp.Tests.Dedektif
{
    public partial class DedektifWindow : Window
    {
        private readonly DedektifViewModel _vm;
        private readonly int _countdownSeconds;
        private int  _countdownRemaining;
        private bool _locked = true;
        private bool _isPaused = false;
        private bool _wasInDelay = false;
        private DispatcherTimer? _delayTimer;

        public DedektifWindow(int durationMinutes, int figureCount, int countdownSeconds)
        {
            InitializeComponent();
            _countdownSeconds = countdownSeconds;
            _vm = new DedektifViewModel(durationMinutes, figureCount);
            _vm.TimeUpdated   += t  => TimerBlock.Text = t;
            _vm.TestCompleted += OnTestCompleted;
            Loaded            += OnLoaded;
        }

        // ── Yükleme ───────────────────────────────────────────────────

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TimerBlock.Text = _vm.TimeLeft;
            _countdownRemaining = _countdownSeconds;
            CountdownNumber.Text = _countdownRemaining.ToString();
            CountdownOverlay.Visibility = Visibility.Visible;
            StartCountdown();
        }

        private void StartCountdown()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (_, __) =>
            {
                _countdownRemaining--;
                if (_countdownRemaining > 0)
                {
                    CountdownNumber.Text = _countdownRemaining.ToString();
                }
                else
                {
                    timer.Stop();
                    CountdownOverlay.Visibility = Visibility.Collapsed;
                    var (figures, target) = _vm.StartTest();
                    ShowTrial(figures, target);
                    _locked = false;
                }
            };
            timer.Start();
        }

        // ── Deneme kurulumu ──────────────────────────────────────────────

        private void ShowTrial(List<FigureItem> figures, FigureItem target)
        {
            TargetSymbolBlock.Text       = target.Symbol;
            TargetSymbolBlock.Foreground = HexBrush(target.ColorHex);
            TargetNameBlock.Text         = target.DisplayName;

            int cols  = (int)Math.Ceiling(Math.Sqrt(figures.Count));
            int size  = Math.Min(130, (960 / cols) - 12);
            int fSize = size * 54 / 100;

            FigureGrid.Columns = cols;
            FigureGrid.Children.Clear();

            foreach (var fig in figures)
                FigureGrid.Children.Add(CreateButton(fig, size, fSize));
        }

        private Button CreateButton(FigureItem figure, int size, int fontSize)
        {
            var symbol = new TextBlock
            {
                Text                = figure.Symbol,
                FontSize            = fontSize,
                Foreground          = HexBrush(figure.ColorHex),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
            };

            var btn = new Button
            {
                Width           = size,
                Height          = size,
                Margin          = new Thickness(5),
                Cursor          = Cursors.Hand,
                Tag             = figure,
                Content         = symbol,
                Background      = Brushes.White,
                BorderBrush     = new SolidColorBrush(Color.FromRgb(0xCF, 0xD8, 0xDC)),
                BorderThickness = new Thickness(1.5),
            };
            btn.Click += FigureButton_Click;
            return btn;
        }

        // ── Tıklama işleyicisi ────────────────────────────────────────────

        private void FigureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_locked || sender is not Button btn || btn.Tag is not FigureItem figure)
                return;

            bool correct = _vm.HandleClick(figure);

            if (correct)
            {
                _locked = true;
                FlashButton(btn, "#43A047");
                TrialCountBlock.Text  = _vm.GetResult().TotalFound.ToString();
                WrongClicksBlock.Text = "0";

                _delayTimer?.Stop();
                _delayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
                _delayTimer.Tick += (_, __) =>
                {
                    _delayTimer?.Stop();
                    _delayTimer = null;
                    var (figures, target) = _vm.NextTrial();
                    ShowTrial(figures, target);
                    _locked = false;
                };
                _delayTimer.Start();
            }
            else
            {
                WrongClicksBlock.Text = _vm.WrongClicks.ToString();
                FlashButton(btn, "#E53935", 280);
            }
        }

        // ── Buton parlatma ────────────────────────────────────────────

        private void FlashButton(Button btn, string colorHex, int ms = 160)
        {
            var original = btn.Background;
            btn.Background = HexBrush(colorHex);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            timer.Tick += (_, __) => { timer.Stop(); btn.Background = original; };
            timer.Start();
        }

        // ── Test bitti ────────────────────────────────────────────────

        private void OnTestCompleted() => Dispatcher.Invoke(EndTest);

        private void EndTest()
        {
            _locked = true;
            _delayTimer?.Stop();
            _delayTimer = null;
            _vm.Stop();
            var resultWindow = new DedektifResultWindow(_vm.GetResult());
            resultWindow.Show();
            this.Close();
        }

        // ── Duraklat / Devam Et ──────────────────────────────────────────
        private void TogglePause()
        {
            if (_isPaused) ResumeTest();
            else PauseTest();
        }

        private void PauseTest()
        {
            _isPaused = true;
            _locked = true;
            _wasInDelay = _delayTimer != null;
            _delayTimer?.Stop();
            _vm.Stop();
            PauseOverlay.Visibility = Visibility.Visible;
        }

        private void ResumeTest()
        {
            _isPaused = false;
            PauseOverlay.Visibility = Visibility.Collapsed;
            _vm.Resume();
            if (_wasInDelay)
            {
                _wasInDelay = false;
                _delayTimer = null;
                var (figures, target) = _vm.NextTrial();
                ShowTrial(figures, target);
            }
            _locked = false;
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e) => ResumeTest();
        private void EndTestButton_Click(object sender, RoutedEventArgs e) => EndTest();

        // ── ESC tuşu ────────────────────────────────────────────────────

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CountdownOverlay.Visibility == Visibility.Collapsed)
                TogglePause();
        }

        // ── Yardımcı ──────────────────────────────────────────────────────

        private static SolidColorBrush HexBrush(string hex)
            => new((Color)ColorConverter.ConvertFromString(hex));
    }
}
