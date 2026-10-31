using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PsikologTestApp;
using PsikologTestApp.Tests.YuruYurume.Models;

namespace PsikologTestApp.Tests.YuruYurume
{
    public partial class YuruYurumeResultWindow : Window
    {
        private GoNoGoResult _result;

        public YuruYurumeResultWindow(GoNoGoResult result)
        {
            InitializeComponent();
            _result = result;
            DisplayResults();
        }

        private class TrialRowModel
        {
            public int    TrialNumber  { get; set; }
            public string Uyaran      { get; set; } = "";
            public string UyaranColor { get; set; } = "#333";
            public string Sonuc       { get; set; } = "";
            public string SonucColor  { get; set; } = "#333";
            public string TepkiSuresi { get; set; } = "";
            public string BasisSayisi { get; set; } = "";
            public string BasisSayisiColor { get; set; } = "#333";
        }

        private void DisplayResults()
        {
            var hits = _result.Hits.ToList();
            int goCount   = _result.Trials.Count(t => t.IsGoStimulus);
            int noGoCount = _result.Trials.Count(t => !t.IsGoStimulus);

            TestNameBlock.Text  = _result.TestName;
            TestDateBlock.Text  = _result.TestDate.ToString("dd.MM.yyyy HH:mm:ss");
            GoPercentBlock.Text = $"%{_result.GoPercentage}";
            TotalBlock.Text     = _result.Trials.Count.ToString();
            GoCountBlock.Text   = goCount.ToString();
            NoGoCountBlock.Text = noGoCount.ToString();

            HitBlock.Text        = hits.Count.ToString();
            MissBlock.Text       = _result.Misses.Count().ToString();
            FalseAlarmBlock.Text  = _result.FalseAlarms.Count().ToString();
            CRBlock.Text         = _result.CorrectRejections.Count().ToString();

            HitRateBlock.Text = $"%{_result.HitRate:F1}";
            FABlock.Text      = $"%{_result.FalseAlarmRate:F1}";

            AvgBlock.Text = hits.Count > 0 ? $"{_result.AverageResponseTime:F0} ms" : "--";
            MinBlock.Text = hits.Count > 0 ? $"{_result.MinResponseTime:F0} ms" : "--";
            MaxBlock.Text = hits.Count > 0 ? $"{_result.MaxResponseTime:F0} ms" : "--";

            // Multi-press counts
            int multiGo   = _result.Trials.Count(t =>  t.IsGoStimulus && t.PressCount > 1);
            int multiNoGo = _result.Trials.Count(t => !t.IsGoStimulus && t.PressCount > 1);
            MultiGoBlock.Text   = multiGo   > 0 ? $"{multiGo} deneme" : "Yok";
            MultiNoGoBlock.Text = multiNoGo > 0 ? $"{multiNoGo} deneme" : "Yok";

            static string SonucText(TrialType t) => t switch
            {
                TrialType.Hit              => "✅ Doğru Basış",
                TrialType.Miss             => "⏱ Kaçırma",
                TrialType.FalseAlarm       => "❌ Yanlış Basış",
                TrialType.CorrectRejection => "✓ Doğru Reddiye",
                _ => "-"
            };

            static string SonucColor(TrialType t) => t switch
            {
                TrialType.Hit              => "#43A047",
                TrialType.Miss             => "#FF9800",
                TrialType.FalseAlarm       => "#E53935",
                TrialType.CorrectRejection => "#2196F3",
                _ => "#333333"
            };

            TrialsGrid.ItemsSource = _result.Trials
                .Select(t => new TrialRowModel
                {
                    TrialNumber       = t.TrialNumber,
                    Uyaran           = t.IsGoStimulus ? "🟢 Yeşil" : "🔴 Kırmızı",
                    UyaranColor      = t.IsGoStimulus ? "#43A047" : "#E53935",
                    Sonuc            = SonucText(t.Type),
                    SonucColor       = SonucColor(t.Type),
                    TepkiSuresi      = t.Pressed ? $"{t.ResponseTimeMs:F0} ms" : "-",
                    BasisSayisi      = t.PressCount == 0 ? "—" : $"×{t.PressCount}",
                    BasisSayisiColor = t.PressCount == 0 ? "#9E9E9E"
                                    : t.PressCount == 1 ? "#333333"
                                    : "#C62828",
                })
                .ToList();

            BuildTimeline();
        }

        private void BuildTimeline()
        {
            const double canvasWidth  = 740;
            const int    trialsPerRow = 10;
            const double topH   = 24;
            const double blockH = 44;
            const double botH   = 18;
            const double totalH = topH + blockH + botH;

            if (_result.Trials.Count == 0) return;

            var rows = new List<List<GoNoGoTrial>>();
            for (int i = 0; i < _result.Trials.Count; i += trialsPerRow)
                rows.Add(_result.Trials.Skip(i).Take(trialsPerRow).ToList());

            foreach (var row in rows)
            {
                double slotW = canvasWidth / row.Count;

                var canvas = new Canvas
                {
                    Width        = canvasWidth,
                    Height       = totalH,
                    Background   = Brushes.Transparent,
                    ClipToBounds = true,
                    Margin       = new Thickness(0, 0, 0, 8),
                };

                double curX = 0;
                foreach (var trial in row)
                {
                    double w = slotW - 3;

                    // ── Background: stimulus tint ─────────────────────────
                    var bgFill = trial.IsGoStimulus
                        ? Color.FromArgb(0x55, 0x43, 0xA0, 0x47)   // light green
                        : Color.FromArgb(0x55, 0xE5, 0x39, 0x35);  // light red

                    var bgRect = new Rectangle
                    {
                        Width = w, Height = blockH,
                        Fill = new SolidColorBrush(bgFill),
                        RadiusX = 3, RadiusY = 3,
                    };
                    Canvas.SetLeft(bgRect, curX);
                    Canvas.SetTop(bgRect, topH);
                    canvas.Children.Add(bgRect);

                    // ── Border: outcome color ─────────────────────────────
                    Color borderColor = trial.Type switch
                    {
                        TrialType.Hit              => Color.FromRgb(0x1B, 0x5E, 0x20),
                        TrialType.Miss             => Color.FromRgb(0xE6, 0x51, 0x00),
                        TrialType.FalseAlarm       => Color.FromRgb(0xB7, 0x1C, 0x1C),
                        TrialType.CorrectRejection => Color.FromRgb(0x0D, 0x47, 0xA1),
                        _                          => Color.FromRgb(0x88, 0x88, 0x88),
                    };

                    var borderRect = new Rectangle
                    {
                        Width = w, Height = blockH,
                        Fill = Brushes.Transparent,
                        Stroke = new SolidColorBrush(borderColor),
                        StrokeThickness = 2.5,
                        RadiusX = 3, RadiusY = 3,
                    };
                    Canvas.SetLeft(borderRect, curX);
                    Canvas.SetTop(borderRect, topH);
                    canvas.Children.Add(borderRect);

                    // ── Press count badge (center) ─────────────────────────
                    string countText = trial.PressCount == 0
                        ? "—"
                        : $"×{trial.PressCount}";

                    var countLbl = new TextBlock
                    {
                        Text          = countText,
                        FontSize      = trial.PressCount > 1 ? 12 : 11,
                        FontWeight    = trial.PressCount > 1 ? FontWeights.Bold : FontWeights.Normal,
                        Foreground    = trial.PressCount > 1
                                          ? new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C))
                                          : new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                        Width         = w,
                        TextAlignment = System.Windows.TextAlignment.Center,
                    };
                    Canvas.SetLeft(countLbl, curX);
                    Canvas.SetTop(countLbl, topH + (blockH - 16) / 2);
                    canvas.Children.Add(countLbl);

                    // ── Stimulus label above ──────────────────────────────
                    var stimLbl = new TextBlock
                    {
                        Text          = trial.IsGoStimulus ? "🟢" : "🔴",
                        FontSize      = 11,
                        Width         = w,
                        TextAlignment = System.Windows.TextAlignment.Center,
                    };
                    Canvas.SetLeft(stimLbl, curX);
                    Canvas.SetTop(stimLbl, topH - 20);
                    canvas.Children.Add(stimLbl);

                    // ── RT label below (only if pressed) ─────────────────
                    if (w >= 28 && trial.Pressed)
                    {
                        var rtLbl = new TextBlock
                        {
                            Text          = $"{trial.ResponseTimeMs:F0}ms",
                            FontSize      = 9,
                            Foreground    = new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C)),
                            Width         = w,
                            TextAlignment = System.Windows.TextAlignment.Center,
                        };
                        Canvas.SetLeft(rtLbl, curX);
                        Canvas.SetTop(rtLbl, topH + blockH + 2);
                        canvas.Children.Add(rtLbl);
                    }

                    curX += slotW;
                }

                TimelineRows.Items.Add(canvas);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            new YuruYurumeStartWindow().Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
