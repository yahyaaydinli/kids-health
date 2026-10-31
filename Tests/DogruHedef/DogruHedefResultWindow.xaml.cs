using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PsikologTestApp.Tests.DogruHedef.Models;

namespace PsikologTestApp.Tests.DogruHedef
{
    public partial class DogruHedefResultWindow : Window
    {
        private readonly DogruHedefResult _result;

        public DogruHedefResultWindow(DogruHedefResult result)
        {
            InitializeComponent();
            _result = result;
            DisplayResults();
        }

        private void DisplayResults()
        {
            TestNameBlock.Text     = _result.TestName;
            TestDateBlock.Text     = _result.TestDate.ToString("dd.MM.yyyy HH:mm:ss");
            TargetAnimalBlock.Text = _result.TargetAnimalName;
            DurationBlock.Text     = $"{_result.DurationMinutes} dakika";
            TotalSegBlock.Text     = _result.Segments.Count.ToString();

            TargetCountBlock.Text  = _result.TotalTargetSegments.ToString();
            HitBlock.Text         = _result.HitCount.ToString();
            MissBlock.Text        = _result.MissCount.ToString();
            HitRateBlock.Text     = $"{_result.HitRate:F1} %";
            FalseGapBlock.Text    = _result.FalsePressesOnGap.ToString();
            FalseOtherBlock.Text  = _result.FalsePressesOnOther.ToString();
            AvgRtBlock.Text       = _result.HitCount > 0 ? $"{_result.AvgCorrectRtMs:F0} ms" : "—";

            BuildTimeline();

            SegmentsGrid.ItemsSource = _result.Segments
                .Select(s => new
                {
                    No           = s.SegmentIndex + 1,
                    Segment      = s.SegmentLabel,
                    SegmentColor = s.Kind switch
                    {
                        SegmentKind.Target      => "#1B5E20",
                        SegmentKind.OtherAnimal => "#1565C0",
                        _                       => "#757575",
                    },
                    SureMs       = $"{s.DurationMs:F0} ms",
                    PressCount   = s.PressCount,
                    Sonuc        = GetSonuc(s),
                    SonucColor   = GetSonucColor(s),
                    Zamanlar     = s.PressTimingsDisplay,
                })
                .ToList();
        }

        private static string GetSonuc(DogruHedefSegment s) => s.Kind switch
        {
            SegmentKind.Target      => s.PressCount > 0 ? "✅ İsabet"          : "❌ Kaçırıldı",
            SegmentKind.Gap         => s.PressCount > 0 ? "⚠️ Yanlış (Boşluk)" : "✓ Bekledi",
            _                       => s.PressCount > 0 ? "⚠️ Yanlış Hayvan"   : "✓ Bekledi",
        };

        private static string GetSonucColor(DogruHedefSegment s) => s.Kind switch
        {
            SegmentKind.Target      => s.PressCount > 0 ? "#43A047" : "#E53935",
            SegmentKind.Gap         => s.PressCount > 0 ? "#FF6F00" : "#9E9E9E",
            _                       => s.PressCount > 0 ? "#E53935" : "#9E9E9E",
        };

        private void BuildTimeline()
        {
            const double pixelsPerRow = 860;
            const double rowMs        = 30_000; // 30 seconds per row
            const double topH         = 22;     // space above block (emoji label)
            const double blockH       = 36;     // block rectangle height
            const double botH         = 18;     // space below block (time axis)
            const double totalRowH    = topH + blockH + botH;

            if (_result.Segments.Count == 0) return;

            // ── Build chunk list ─────────────────────────────────────────────
            // Each "chunk" is one piece of a segment that fits within a 30-second row.
            // Since max segment ≤ 3 s (much less than 30 s), segments rarely split.
            var rowList  = new List<(double StartAbsMs,
                                     List<(DogruHedefSegment Seg, double InRowStartMs, double DurMs, double PrevConsumedMs)> Chunks)>();
            var curChunks = new List<(DogruHedefSegment Seg, double InRowStartMs, double DurMs, double PrevConsumedMs)>();
            double absMs      = 0;
            double rowStartMs = 0;
            var prevConsumed  = new Dictionary<int, double>(); // segmentIndex → ms used in earlier chunks

            foreach (var seg in _result.Segments)
            {
                double remaining = seg.DurationMs;
                int    key       = seg.SegmentIndex;

                while (remaining > 0.001)
                {
                    double usedInRow = absMs - rowStartMs;
                    double space     = rowMs - usedInRow;

                    if (space < 0.001) // current row is full — commit it and start a new one
                    {
                        rowList.Add((rowStartMs, curChunks));
                        curChunks  = new List<(DogruHedefSegment, double, double, double)>();
                        rowStartMs = absMs;
                        continue;
                    }

                    double chunk          = Math.Min(remaining, space);
                    double consumedBefore = prevConsumed.ContainsKey(key) ? prevConsumed[key] : 0;

                    curChunks.Add((seg, usedInRow, chunk, consumedBefore));
                    prevConsumed[key] = consumedBefore + chunk;
                    absMs     += chunk;
                    remaining -= chunk;
                }
            }
            if (curChunks.Count > 0) rowList.Add((rowStartMs, curChunks));

            // ── Draw each row ────────────────────────────────────────────────
            foreach (var (rowAbsStart, chunks) in rowList)
            {
                if (chunks.Count == 0) continue;

                var   lastChunk   = chunks[chunks.Count - 1];
                double rowActualMs = lastChunk.InRowStartMs + lastChunk.DurMs;

                var canvas = new Canvas
                {
                    Width        = pixelsPerRow,
                    Height       = totalRowH,
                    Background   = Brushes.Transparent,
                    ClipToBounds = true,
                    Margin       = new Thickness(0, 0, 0, 6),
                };

                foreach (var (seg, inRowStart, chunkDur, prevConsumedMs) in chunks)
                {
                    double x = inRowStart / rowMs * pixelsPerRow;
                    double w = Math.Max(chunkDur  / rowMs * pixelsPerRow, 2);

                    Color fillColor, strokeColor;
                    switch (seg.Kind)
                    {
                        case SegmentKind.Target:
                            fillColor   = Color.FromRgb(0x38, 0x8E, 0x3C);
                            strokeColor = Color.FromRgb(0x1B, 0x5E, 0x20);
                            break;
                        case SegmentKind.OtherAnimal:
                            fillColor   = Color.FromRgb(0x15, 0x65, 0xC0);
                            strokeColor = Color.FromRgb(0x0D, 0x47, 0xA1);
                            break;
                        default:
                            fillColor   = Color.FromRgb(0xB0, 0xBE, 0xC5);
                            strokeColor = Color.FromRgb(0x90, 0xA4, 0xAE);
                            break;
                    }

                    // Segment rectangle
                    var rect = new Rectangle
                    {
                        Width           = w,
                        Height          = blockH,
                        Fill            = new SolidColorBrush(fillColor),
                        Stroke          = new SolidColorBrush(strokeColor),
                        StrokeThickness = 1,
                        RadiusX         = 2,
                        RadiusY         = 2,
                    };
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect,  topH);
                    canvas.Children.Add(rect);

                    // Emoji + name label above block (only for animal segments, only if wide enough)
                    if (w >= 24 && seg.Kind != SegmentKind.Gap)
                    {
                        var animal = DogruHedefAnimal.All.FirstOrDefault(a => a.Name == seg.AnimalName);
                        string emoji = animal?.Emoji ?? "🐾";
                        var lbl = new TextBlock
                        {
                            Text          = $"{emoji} {seg.AnimalName}",
                            FontSize      = 9,
                            Foreground    = Brushes.Black,
                            TextAlignment = System.Windows.TextAlignment.Center,
                            Width         = w,
                            TextWrapping  = TextWrapping.NoWrap,
                        };
                        Canvas.SetLeft(lbl, x);
                        Canvas.SetTop(lbl,  topH - 16);
                        canvas.Children.Add(lbl);
                    }

                    // Press marks (red vertical line + dot)
                    foreach (var press in seg.Presses)
                    {
                        double pressInChunk = press.TimeFromSegmentStartMs - prevConsumedMs;
                        if (pressInChunk < 0 || pressInChunk > chunkDur) continue;

                        double px = (inRowStart + pressInChunk) / rowMs * pixelsPerRow;

                        canvas.Children.Add(new Line
                        {
                            X1 = px, Y1 = topH - 2,
                            X2 = px, Y2 = topH + blockH + 2,
                            Stroke          = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
                            StrokeThickness = 2,
                        });

                        var dot = new Ellipse
                        {
                            Width  = 7,
                            Height = 7,
                            Fill   = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
                        };
                        Canvas.SetLeft(dot, px - 3.5);
                        Canvas.SetTop(dot,  topH - 7);
                        canvas.Children.Add(dot);
                    }
                }

                // Time-axis labels & ticks every 5 s (or 2 s / 1 s for short rows)
                double step = rowActualMs >= 15_000 ? 5_000 : rowActualMs >= 5_000 ? 2_000 : 1_000;
                for (double t = 0; t <= rowActualMs + 0.001; t += step)
                {
                    double lx = t / rowMs * pixelsPerRow;

                    var tbl = new TextBlock
                    {
                        Text       = $"{(rowAbsStart + t) / 1000:F0}s",
                        FontSize   = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C)),
                    };
                    Canvas.SetLeft(tbl, lx + 1);
                    Canvas.SetTop(tbl,  topH + blockH + 2);
                    canvas.Children.Add(tbl);

                    canvas.Children.Add(new Line
                    {
                        X1 = lx, Y1 = topH + blockH,
                        X2 = lx, Y2 = topH + blockH + 4,
                        Stroke          = new SolidColorBrush(Color.FromRgb(0xCF, 0xD8, 0xDC)),
                        StrokeThickness = 1,
                    });
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
            new DogruHedefStartWindow().Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
