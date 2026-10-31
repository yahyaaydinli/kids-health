using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PsikologTestApp.Tests.AyakIzi.ViewModels;

namespace PsikologTestApp.Tests.AyakIzi
{
    public partial class AyakIziWindow : Window
    {
        private readonly AyakIziViewModel _vm;
        private readonly int              _countdownSeconds;
        private int                       _countdownRemaining;
        private bool                      _locked    = true;
        private bool                      _isPaused;
        private bool                      _testEnded;

        private readonly List<Button> _buttons = new();
        private List<int>             _currentSeq = new();
        private int                   _wrongAttemptsThisRound;

        // ── colour palette ───────────────────────────────────────────────
        private static readonly SolidColorBrush BgNeutral    = Hex("#E3F2FD");
        private static readonly SolidColorBrush BdNeutral    = Hex("#90CAF9");
        private static readonly SolidColorBrush BgHighlight  = Hex("#FFB300");
        private static readonly SolidColorBrush BdHighlight  = Hex("#E65100");
        private static readonly SolidColorBrush BgCorrect    = Hex("#43A047");
        private static readonly SolidColorBrush BgWrong      = Hex("#E53935");

        private static SolidColorBrush Hex(string h)
            => new((Color)ColorConverter.ConvertFromString(h));

        public AyakIziWindow(int gridSize, int countdownSeconds)
        {
            InitializeComponent();
            _countdownSeconds = countdownSeconds;
            _vm = new AyakIziViewModel(gridSize);
            Loaded += OnLoaded;
        }

        // ── initialisation ────────────────────────────────────────────────

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BuildGrid();
            _countdownRemaining      = _countdownSeconds;
            CountdownNumber.Text     = _countdownRemaining.ToString();
            CountdownOverlay.Visibility = Visibility.Visible;
            RunCountdown();
        }

        private void BuildGrid()
        {
            int cols = _vm.GridCols;
            int sz   = Math.Min(130, 620 / cols - 12);

            BlockGrid.Columns = cols;
            BlockGrid.Children.Clear();
            _buttons.Clear();

            for (int i = 0; i < _vm.GridSize; i++)
            {
                int idx = i;
                var icon = new TextBlock
                {
                    Text      = "👣",
                    FontSize  = Math.Max(12, sz * 28 / 100),
                    Opacity   = 0.18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                };
                var btn = new Button
                {
                    Width           = sz,
                    Height          = sz,
                    Margin          = new Thickness(7),
                    Background      = BgNeutral,
                    BorderBrush     = BdNeutral,
                    BorderThickness = new Thickness(2),
                    Cursor          = Cursors.Hand,
                    Tag             = idx,
                    Content         = icon,
                };
                btn.Click += Block_Click;
                BlockGrid.Children.Add(btn);
                _buttons.Add(btn);
            }
        }

        // ── countdown ────────────────────────────────────────────────────

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
                    StartNextRound();
                }
            };
            t.Start();
        }

        // ── round management ─────────────────────────────────────────────

        private void StartNextRound()
        {
            if (_testEnded) return;
            _locked = true;
            _wrongAttemptsThisRound = 0;
            YeniDiziButton.Visibility = Visibility.Collapsed;
            _currentSeq = _vm.BeginNextRound();
            RoundBlock.Text      = _vm.RoundNumber.ToString();
            SpanBlock.Text       = _vm.SpanLength.ToString();
            InstructionText.Text = "Sırayı izle...";
            ResetAllBlocks();
            After(500, () => PlaySeq(0, showing: true));
        }

        // ── sequence playback ─────────────────────────────────────────────
        // Alternates between "showing" (highlight block for 600 ms)
        // and "hiding" (clear + 300 ms gap) for each step.

        private void PlaySeq(int step, bool showing)
        {
            if (_isPaused || _testEnded) return;

            if (showing)
            {
                Highlight(_currentSeq[step]);
                After(600, () => PlaySeq(step, showing: false));
            }
            else
            {
                Reset(_currentSeq[step]);
                int next = step + 1;
                if (next >= _currentSeq.Count)
                    After(400, BeginInput);
                else
                    After(300, () => PlaySeq(next, showing: true));
            }
        }

        private void BeginInput()
        {
            if (_isPaused || _testEnded) return;
            _vm.MarkInputStart();
            InstructionText.Text = "Şimdi aynı sırayla tıkla!";
            _locked = false;
        }

        // ── user interaction ──────────────────────────────────────────────

        private void Block_Click(object sender, RoutedEventArgs e)
        {
            if (_locked || _testEnded || sender is not Button btn || btn.Tag is not int idx)
                return;

            _locked = true;
            var result = _vm.HandleClick(idx);

            switch (result)
            {
                case ClickResult.Correct:
                    Flash(idx, BgCorrect, 220, () => { Reset(idx); _locked = false; });
                    break;

                case ClickResult.RoundComplete:
                    Flash(idx, BgCorrect, 300, () =>
                    {
                        Reset(idx);
                        InstructionText.Text = "✅ Harika! Devam ediyor...";
                        After(700, StartNextRound);
                    });
                    break;

                case ClickResult.Wrong:
                    _wrongAttemptsThisRound++;
                    if (_wrongAttemptsThisRound == 1)
                    {
                        // İlk yanlış: uyarı göster ve diziyi baştan oynat
                        Flash(idx, BgWrong, 500, () =>
                        {
                            ResetAllBlocks();
                            ShowWarning("⚠️ Yanlış sıra! Baştan izle...");
                            _vm.ResetInput();
                            InstructionText.Text = "Sırayı izle...";
                            After(900, () => PlaySeq(0, showing: true));
                        });
                    }
                    else
                    {
                        // İkinci (ve sonraki) yanlış: uyarı + Yeni Dizi butonu
                        Flash(idx, BgWrong, 500, () =>
                        {
                            ResetAllBlocks();
                            ShowWarning("⚠️ Tekrar yanlış! Yeni dizi almak için butona bas.");
                            InstructionText.Text = "Yeni dizi almak için ➡ Yeni Dizi butonuna bas.";
                            YeniDiziButton.Visibility = Visibility.Visible;
                            _locked = true;
                        });
                    }
                    break;
            }
        }

        // ── block helpers ────────────────────────────────────────────────

        private void Highlight(int idx)
        {
            _buttons[idx].Background  = BgHighlight;
            _buttons[idx].BorderBrush = BdHighlight;
            if (_buttons[idx].Content is TextBlock tb) tb.Opacity = 1.0;
        }

        private void Reset(int idx)
        {
            _buttons[idx].Background  = BgNeutral;
            _buttons[idx].BorderBrush = BdNeutral;
            if (_buttons[idx].Content is TextBlock tb) tb.Opacity = 0.18;
        }

        private void ResetAllBlocks()
        {
            for (int i = 0; i < _buttons.Count; i++) Reset(i);
        }

        private void Flash(int idx, SolidColorBrush bg, int ms, Action onDone)
        {
            _buttons[idx].Background = bg;
            if (_buttons[idx].Content is TextBlock tb) tb.Opacity = 1.0;
            After(ms, () => { if (!_testEnded) { Reset(idx); onDone(); } });
        }

        // ── timer helper ─────────────────────────────────────────────────

        private void After(int ms, Action action)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            t.Tick += (_, __) => { t.Stop(); action(); };
            t.Start();
        }

        // ── end test ─────────────────────────────────────────────────────

        private void EndTest()
        {
            if (_testEnded) return;
            _testEnded = true;
            _locked    = true;
            var rw = new AyakIziResultWindow(_vm.GetResult());
            rw.Show();
            this.Close();
        }

        // ── pause / resume ────────────────────────────────────────────────

        private void PauseTest()
        {
            if (_testEnded) return;
            _isPaused = true;
            _locked   = true;
            PauseOverlay.Visibility = Visibility.Visible;
        }

        private void ResumeTest()
        {
            _isPaused = false;
            PauseOverlay.Visibility = Visibility.Collapsed;
            // Replay the current round's sequence from the beginning
            _vm.ResetInput();
            _locked = true;
            ResetAllBlocks();
            InstructionText.Text = "Sırayı izle...";
            After(500, () => PlaySeq(0, showing: true));
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)  => ResumeTest();
        private void EndTestButton_Click(object sender, RoutedEventArgs e) => EndTest();

        private void YeniDiziButton_Click(object sender, RoutedEventArgs e)
        {
            if (_testEnded) return;
            YeniDiziButton.Visibility = Visibility.Collapsed;
            _wrongAttemptsThisRound   = 0;
            _currentSeq = _vm.RegenerateCurrentSpan();
            InstructionText.Text = "Yeni dizi — sırayı izle...";
            ResetAllBlocks();
            After(400, () => PlaySeq(0, showing: true));
        }

        private void ShowWarning(string message)
        {
            WarningText.Text = message;
            WarningBanner.Visibility = Visibility.Visible;
            After(2000, () => WarningBanner.Visibility = Visibility.Collapsed);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CountdownOverlay.Visibility == Visibility.Collapsed)
            {
                if (_isPaused) ResumeTest();
                else PauseTest();
            }
        }
    }
}
