using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PsikologTestApp;
using PsikologTestApp.Tests.RenkliKelimeler.Models;

namespace PsikologTestApp.Tests.RenkliKelimeler
{
    public partial class RenkliKelimelerResultWindow : Window
    {
        private StroopTestResult _result;

        public RenkliKelimelerResultWindow(StroopTestResult result)
        {
            InitializeComponent();
            _result = result;
            DisplayResults();
        }

        private void DisplayResults()
        {
            var correct = _result.CorrectTrials.ToList();

            TestNameBlock.Text = _result.TestName;
            TestDateBlock.Text = _result.TestDate.ToString("dd.MM.yyyy HH:mm:ss");
            TotalBlock.Text    = _result.Trials.Count.ToString();
            CorrectBlock.Text  = $"{correct.Count}";
            WrongBlock.Text    = $"{_result.IncorrectTrials.Count()}";
            AccuracyBlock.Text = $"%{_result.Accuracy:F1}";
            AvgBlock.Text      = correct.Count > 0 ? $"{_result.AverageResponseTime:F0} ms" : "--";
            MinBlock.Text      = correct.Count > 0 ? $"{_result.MinResponseTime:F0} ms" : "--";
            MaxBlock.Text      = correct.Count > 0 ? $"{_result.MaxResponseTime:F0} ms" : "--";

            TrialsGrid.ItemsSource = _result.Trials
                .Select(t => new
                {
                    t.TrialNumber,
                    t.WordText,
                    t.DisplayColor,
                    t.SelectedColor,
                    Sonuc       = t.IsCorrect ? "✅" : "❌",
                    TepkiSuresi = $"{t.ResponseTimeMs:F0} ms"
                })
                .ToList();

            BuildTimeline();
        }

        private void BuildTimeline()
        {
            // Each Stroop trial = one bar, width proportional to response time
            const double canvasWidth  = 700;
            const int    trialsPerRow = 10;
            const double topH   = 22;
            const double blockH = 36;
            const double botH   = 18;
            const double totalH = topH + blockH + botH;

            if (_result.Trials.Count == 0) return;

            var rows = new List<List<StroopTrial>>();
            for (int i = 0; i < _result.Trials.Count; i += trialsPerRow)
                rows.Add(_result.Trials.Skip(i).Take(trialsPerRow).ToList());

            foreach (var row in rows)
            {
                double totalMs = row.Sum(t => Math.Max(t.ResponseTimeMs, 50));
                if (totalMs < 1) continue;

                var canvas = new Canvas
                {
                    Width        = canvasWidth,
                    Height       = totalH,
                    Background   = Brushes.Transparent,
                    ClipToBounds = true,
                    Margin       = new Thickness(0, 0, 0, 6),
                };

                double curX = 0;
                foreach (var trial in row)
                {
                    double durMs = Math.Max(trial.ResponseTimeMs, 50);
                    double w     = Math.Max(durMs / totalMs * canvasWidth, 4);

                    var fillColor   = trial.IsCorrect
                        ? Color.FromRgb(0x43, 0xA0, 0x47)
                        : Color.FromRgb(0xE5, 0x39, 0x35);
                    var strokeColor = trial.IsCorrect
                        ? Color.FromRgb(0x1B, 0x5E, 0x20)
                        : Color.FromRgb(0xB7, 0x1C, 0x1C);

                    var rect = new Rectangle
                    {
                        Width = w - 2, Height = blockH,
                        Fill = new SolidColorBrush(fillColor),
                        Stroke = new SolidColorBrush(strokeColor),
                        StrokeThickness = 1, RadiusX = 2, RadiusY = 2,
                    };
                    Canvas.SetLeft(rect, curX);
                    Canvas.SetTop(rect, topH);
                    canvas.Children.Add(rect);

                    // Word label above
                    if (w >= 24)
                    {
                        var lbl = new TextBlock
                        {
                            Text = trial.WordText,
                            FontSize = 9,
                            Foreground = Brushes.Black,
                            TextAlignment = System.Windows.TextAlignment.Center,
                            Width = w - 2,
                        };
                        Canvas.SetLeft(lbl, curX);
                        Canvas.SetTop(lbl, topH - 16);
                        canvas.Children.Add(lbl);
                    }

                    // RT label below
                    if (w >= 30)
                    {
                        var tbl = new TextBlock
                        {
                            Text = $"{trial.ResponseTimeMs:F0}ms",
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C)),
                            Width = w - 2, TextAlignment = System.Windows.TextAlignment.Center,
                        };
                        Canvas.SetLeft(tbl, curX);
                        Canvas.SetTop(tbl, topH + blockH + 2);
                        canvas.Children.Add(tbl);
                    }

                    curX += w;
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
            new RenkliKelimelerStartWindow().Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
