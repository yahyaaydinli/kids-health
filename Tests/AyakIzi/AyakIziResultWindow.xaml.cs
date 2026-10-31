using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PsikologTestApp.Tests.AyakIzi.Models;

namespace PsikologTestApp.Tests.AyakIzi
{
    public partial class AyakIziResultWindow : Window
    {
        private readonly AyakIziResult _result;

        public AyakIziResultWindow(AyakIziResult result)
        {
            InitializeComponent();
            _result = result;
            DisplayResults();
        }

        private void DisplayResults()
        {
            int cols = (int)System.Math.Sqrt(_result.GridSize);

            TestNameBlock.Text  = _result.TestName;
            TestDateBlock.Text  = _result.TestDate.ToString("dd.MM.yyyy HH:mm:ss");
            GridSizeBlock.Text  = $"{_result.GridSize} kare ({cols}×{cols})";
            MaxSpanBlock.Text   = _result.MaxSpanReached > 0 ? _result.MaxSpanReached.ToString() : "—";

            CorrectBlock.Text      = _result.TotalCorrect.ToString();
            TotalBlock.Text        = _result.TotalAttempts.ToString();
            RateBlock.Text         = $"{_result.SuccessRate:F1} %";
            AvgBlock.Text          = _result.TotalCorrect > 0 ? $"{_result.AvgResponseMs:F0} ms" : "—";
            NewSeqTotalBlock.Text  = _result.TotalNewSequences.ToString();

            BuildTimeline();

            RoundsGrid.ItemsSource = _result.Rounds
                .Select(r => new
                {
                    r.RoundNumber,
                    SpanLen      = r.SequenceLength,
                    ResponseTime = r.Success ? $"{r.ResponseTimeMs:F0} ms" : "—",
                    NewSeqCount  = r.NewSequenceCount > 0 ? r.NewSequenceCount.ToString() : "—",
                    Sonuc        = r.Success ? "✅ Başarılı" : "❌ Başarısız",
                    SonucColor   = r.Success ? "#43A047"    : "#E53935",
                })
                .ToList();
        }

        private void BuildTimeline()
        {
            // Each round is one bar whose width is proportional to its response time.
            // Rows of max ~8 rounds each keep the timeline readable.
            const double canvasWidth = 640;
            const int    roundsPerRow = 8;
            const double topH   = 22;
            const double blockH = 36;
            const double botH   = 18;
            const double totalH = topH + blockH + botH;

            if (_result.Rounds.Count == 0) return;

            // Split rounds into rows
            var rows = new List<List<AyakIziRound>>();
            for (int i = 0; i < _result.Rounds.Count; i += roundsPerRow)
                rows.Add(_result.Rounds.Skip(i).Take(roundsPerRow).ToList());

            foreach (var row in rows)
            {
                // Use response time for successful rounds; use a fixed 1500 ms placeholder for failed
                double maxMs = row.Max(r => r.Success ? Math.Max(r.ResponseTimeMs, 50) : 1500);
                double totalMs = row.Sum(r => r.Success ? r.ResponseTimeMs : 1500);
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
                foreach (var round in row)
                {
                    double durMs = round.Success ? round.ResponseTimeMs : 1500;
                    double w     = Math.Max(durMs / totalMs * canvasWidth, 4);

                    var fillColor   = round.Success
                        ? Color.FromRgb(0x38, 0x8E, 0x3C)
                        : Color.FromRgb(0xE5, 0x39, 0x35);
                    var strokeColor = round.Success
                        ? Color.FromRgb(0x1B, 0x5E, 0x20)
                        : Color.FromRgb(0xB7, 0x1C, 0x1C);

                    var rect = new Rectangle
                    {
                        Width           = w - 2,
                        Height          = blockH,
                        Fill            = new SolidColorBrush(fillColor),
                        Stroke          = new SolidColorBrush(strokeColor),
                        StrokeThickness = 1,
                        RadiusX = 2, RadiusY = 2,
                    };
                    Canvas.SetLeft(rect, curX);
                    Canvas.SetTop(rect,  topH);
                    canvas.Children.Add(rect);

                    // Label: span length
                    if (w >= 20)
                    {
                        var lbl = new TextBlock
                        {
                            Text          = $"Span {round.SequenceLength}",
                            FontSize      = 9,
                            Foreground    = Brushes.Black,
                            TextAlignment = System.Windows.TextAlignment.Center,
                            Width         = w - 2,
                        };
                        Canvas.SetLeft(lbl, curX);
                        Canvas.SetTop(lbl,  topH - 16);
                        canvas.Children.Add(lbl);
                    }

                    // Time label below
                    if (w >= 30)
                    {
                        var tbl = new TextBlock
                        {
                            Text       = round.Success ? $"{round.ResponseTimeMs:F0}ms" : "—",
                            FontSize   = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C)),
                            Width      = w - 2,
                            TextAlignment = System.Windows.TextAlignment.Center,
                        };
                        Canvas.SetLeft(tbl, curX);
                        Canvas.SetTop(tbl,  topH + blockH + 2);
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
            new AyakIziStartWindow().Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
