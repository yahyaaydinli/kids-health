using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PsikologTestApp;
using PsikologTestApp.Tests.HedefAvcisi.Models;

namespace PsikologTestApp.Tests.HedefAvcisi
{
    public partial class HedefAvcisiResultWindow : Window
    {
        private TestResult _testResult;

        public HedefAvcisiResultWindow(TestResult testResult)
        {
            InitializeComponent();
            _testResult = testResult;
            DisplayResults();
        }

        private void DisplayResults()
        {
            var correct = _testResult.CorrectPresses.ToList();
            var falsePresses = _testResult.FalsePresses.ToList();

            TestNameBlock.Text = _testResult.TestName;
            TestDateBlock.Text = _testResult.TestDate.ToString("dd.MM.yyyy HH:mm:ss");
            TargetLetterBlock.Text = _testResult.FixedTargetLetter.ToString();
            AverageBlock.Text = correct.Count > 0 ? $"{_testResult.AverageReactionTime:F0} ms" : "--";
            AvgWaitBlock.Text = correct.Count > 0
                ? $"{correct.Average(r => r.WaitTimeMs):F0} ms" : "--";
            MinBlock.Text = correct.Count > 0 ? $"{_testResult.MinReactionTime:F0} ms" : "--";
            MaxBlock.Text = correct.Count > 0 ? $"{_testResult.MaxReactionTime:F0} ms" : "--";
            CountBlock.Text = correct.Count.ToString();
            FalsePressCountBlock.Text = falsePresses.Count.ToString();
            MultiPressCountBlock.Text = correct.Count(r => r.PressOrder > 1).ToString();

            int rowIndex = 1;
            AppearanceDetailsGrid.ItemsSource = _testResult.ReactionTimes
                .Select(r =>
                {
                    string sonuc;
                    if (!r.IsCorrect)
                        sonuc = "❌ Yanlış";
                    else if (r.PressOrder == 1)
                        sonuc = "✅ Doğru";
                    else
                        sonuc = $"⚡ Çift ({r.PressOrder}. basış)";

                    return new
                    {
                        RowIndex = rowIndex++,
                        Sonuc = sonuc,
                        GosterHarf = r.DisplayedLetter.ToString(),
                        WaitTime = r.IsCorrect && r.PressOrder == 1 ? $"{r.WaitTimeMs:F0} ms" : "-",
                        ReactionTime = r.IsCorrect ? $"{r.ResponseTimeMs:F0} ms" : "-"
                    };
                })
                .ToList();

            BuildTimeline();
        }

        private void BuildTimeline()
        {
            // Each ReactionTime entry shows as a two-segment bar:
            // left (gray) = WaitTimeMs, right (colored) = ResponseTimeMs
            const double canvasWidth  = 640;
            const int    itemsPerRow  = 10;
            const double topH   = 22;
            const double blockH = 36;
            const double botH   = 18;
            const double totalH = topH + blockH + botH;

            var items = _testResult.ReactionTimes.ToList();
            if (items.Count == 0) return;

            var rows = new List<List<ReactionTime>>();
            for (int i = 0; i < items.Count; i += itemsPerRow)
                rows.Add(items.Skip(i).Take(itemsPerRow).ToList());

            foreach (var row in rows)
            {
                // Each item slot gets equal width
                double slotW = canvasWidth / row.Count;

                var canvas = new Canvas
                {
                    Width        = canvasWidth,
                    Height       = totalH,
                    Background   = Brushes.Transparent,
                    ClipToBounds = true,
                    Margin       = new Thickness(0, 0, 0, 6),
                };

                double curX = 0;
                foreach (var rt in row)
                {
                    double w = slotW - 2;

                    Color fillColor;
                    if (!rt.IsCorrect)
                        fillColor = Color.FromRgb(0xE5, 0x39, 0x35);
                    else if (rt.PressOrder > 1)
                        fillColor = Color.FromRgb(0xFF, 0x98, 0x00);
                    else
                        fillColor = Color.FromRgb(0x43, 0xA0, 0x47);

                    // Wait segment (gray, left half)
                    if (rt.IsCorrect && rt.PressOrder == 1 && rt.WaitTimeMs > 0)
                    {
                        double waitFrac = rt.WaitTimeMs / (rt.WaitTimeMs + rt.ResponseTimeMs);
                        double waitW    = Math.Max(w * waitFrac, 2);
                        var waitRect = new Rectangle
                        {
                            Width = waitW, Height = blockH,
                            Fill = new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5)),
                            Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0xA4, 0xAE)),
                            StrokeThickness = 1, RadiusX = 2, RadiusY = 2,
                        };
                        Canvas.SetLeft(waitRect, curX);
                        Canvas.SetTop(waitRect, topH);
                        canvas.Children.Add(waitRect);

                        double respW = Math.Max(w - waitW, 2);
                        var respRect = new Rectangle
                        {
                            Width = respW, Height = blockH,
                            Fill = new SolidColorBrush(fillColor),
                            Stroke = new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20)),
                            StrokeThickness = 1, RadiusX = 2, RadiusY = 2,
                        };
                        Canvas.SetLeft(respRect, curX + waitW);
                        Canvas.SetTop(respRect, topH);
                        canvas.Children.Add(respRect);
                    }
                    else
                    {
                        var rect = new Rectangle
                        {
                            Width = w, Height = blockH,
                            Fill = new SolidColorBrush(fillColor),
                            Stroke = new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20)),
                            StrokeThickness = 1, RadiusX = 2, RadiusY = 2,
                        };
                        Canvas.SetLeft(rect, curX);
                        Canvas.SetTop(rect, topH);
                        canvas.Children.Add(rect);
                    }

                    // Letter label above
                    if (w >= 16)
                    {
                        var lbl = new TextBlock
                        {
                            Text = rt.DisplayedLetter.ToString(),
                            FontSize = 11, FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Black,
                            TextAlignment = System.Windows.TextAlignment.Center,
                            Width = w,
                        };
                        Canvas.SetLeft(lbl, curX);
                        Canvas.SetTop(lbl, topH - 17);
                        canvas.Children.Add(lbl);
                    }

                    // RT label below
                    if (w >= 28)
                    {
                        var tbl = new TextBlock
                        {
                            Text = rt.IsCorrect ? $"{rt.ResponseTimeMs:F0}ms" : "—",
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C)),
                            Width = w, TextAlignment = System.Windows.TextAlignment.Center,
                        };
                        Canvas.SetLeft(tbl, curX);
                        Canvas.SetTop(tbl, topH + blockH + 2);
                        canvas.Children.Add(tbl);
                    }

                    curX += slotW;
                }

                TimelineRows.Items.Add(canvas);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            HedefAvcisiStartWindow startWindow = new HedefAvcisiStartWindow();
            startWindow.Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
