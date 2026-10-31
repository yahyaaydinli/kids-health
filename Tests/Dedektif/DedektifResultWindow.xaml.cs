using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PsikologTestApp.Tests.Dedektif.Models;

namespace PsikologTestApp.Tests.Dedektif
{
    public partial class DedektifResultWindow : Window
    {
        private readonly DedektifResult _result;

        public DedektifResultWindow(DedektifResult result)
        {
            InitializeComponent();
            _result = result;
            try
            {
                DisplayResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Sonuç ekranı yüklenirken hata oluştu:\n\n{ex.GetType().Name}: {ex.Message}\n\nStack:\n{ex.StackTrace}",
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayResults()
        {
            TestNameBlock.Text    = _result.TestName;
            TestDateBlock.Text    = _result.TestDate.ToString("dd.MM.yyyy HH:mm:ss");
            FigureCountBlock.Text = $"{_result.FigureCount} figür";
            DurationBlock.Text    = $"{_result.DurationMinutes} dakika";

            FoundBlock.Text  = _result.TotalFound.ToString();
            WrongBlock.Text  = _result.TotalWrongClicks.ToString();

            var found = _result.Trials.Where(t => t.Found).ToList();
            AvgBlock.Text = found.Count > 0 ? $"{_result.AverageResponseMs:F0} ms" : "--";
            MinBlock.Text = found.Count > 0 ? $"{_result.MinResponseMs:F0} ms"     : "--";
            MaxBlock.Text = found.Count > 0 ? $"{_result.MaxResponseMs:F0} ms"     : "--";

            TrialsGrid.ItemsSource = _result.Trials
                .Select(t => new
                {
                    t.TrialNumber,
                    TargetName  = t.TargetName,
                    TepkiSuresi = t.Found ? $"{t.ResponseTimeMs:F0} ms" : "--",
                    WrongClicks = t.WrongClicks == 0
                        ? "—"
                        : t.WrongClickNames?.Count > 0
                            ? $"{t.WrongClicks}× ({string.Join(" → ", t.WrongClickNames)})"
                            : t.WrongClicks.ToString(),
                    Sonuc       = t.Found ? "✅ Bulundu" : "⏱ Kaçırıldı",
                    SonucColor  = t.Found ? "#43A047"   : "#FF9800",
                })
                .ToList();

            BuildTimeline();
        }

        private void BuildTimeline()
        {
            const int    trialsPerRow = 6;
            const double rowWidth     = 720;
            const double cardW        = rowWidth / trialsPerRow - 6;

            if (_result.Trials.Count == 0) return;

            var rows = new List<List<DedektifTrial>>();
            for (int i = 0; i < _result.Trials.Count; i += trialsPerRow)
                rows.Add(_result.Trials.Skip(i).Take(trialsPerRow).ToList());

            foreach (var row in rows)
            {
                var rowPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin      = new Thickness(0, 0, 0, 8),
                };

                foreach (var trial in row)
                    rowPanel.Children.Add(BuildTrialCard(trial, cardW));

                TimelineRows.Items.Add(rowPanel);
            }
        }

        // ── Figure name abbreviation ──────────────────────────────────────────
        private static readonly Dictionary<string, string> ShapeSymbols = new()
        {
            ["kare"]       = "■", ["daire"]      = "●",
            ["yıldız"]     = "★", ["üçgen"]      = "▲",
            ["boş kare"]   = "□", ["boş daire"]  = "○",
            ["boş yıldız"] = "☆", ["boş üçgen"]  = "△",
        };

        private static readonly Dictionary<string, string> ColorCodes = new()
        {
            ["kırmızı"] = "K", ["mavi"] = "M", ["yeşil"] = "Y",
            ["sarı"]    = "S", ["mor"]  = "Mo", ["turuncu"] = "T",
        };

        private static (string Abbrev, string ColorHex) ParseFigure(string displayName)
        {
            string colorHex = "#555555";
            string abbrev   = displayName.Length > 5 ? displayName[..5] : displayName;

            foreach (var (colorName, colorCode) in ColorCodes)
            {
                if (!displayName.StartsWith(colorName)) continue;

                // map color to a hex for text color
                colorHex = colorName switch
                {
                    "kırmızı" => "#C62828",
                    "mavi"    => "#1565C0",
                    "yeşil"   => "#1B5E20",
                    "sarı"    => "#F57F17",
                    "mor"     => "#6A1B9A",
                    "turuncu" => "#E65100",
                    _         => "#555555",
                };

                string shapePart = displayName[(colorName.Length + 1)..];
                string sym = ShapeSymbols.TryGetValue(shapePart, out var s) ? s : "?";
                abbrev = $"{colorCode}{sym}";
                break;
            }

            return (abbrev, colorHex);
        }

        private static UIElement BuildTrialCard(DedektifTrial trial, double cardW)
        {
            var (targetAbbrev, targetColorHex) = ParseFigure(trial.TargetName);

            var outerBorder = new Border
            {
                Width           = cardW,
                Margin          = new Thickness(3, 0, 3, 0),
                Background      = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA)),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(4),
                Padding         = new Thickness(5, 4, 5, 4),
            };

            var sp = new StackPanel();

            // ── Header: trial# + target ──────────────────────────────────────
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            headerPanel.Children.Add(new TextBlock
            {
                Text       = $"#{trial.TrialNumber}  ",
                FontSize   = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
                VerticalAlignment = VerticalAlignment.Center,
            });
            headerPanel.Children.Add(new TextBlock
            {
                Text       = "🎯 ",
                FontSize   = 10,
                VerticalAlignment = VerticalAlignment.Center,
            });
            headerPanel.Children.Add(new TextBlock
            {
                Text       = targetAbbrev,
                FontSize   = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(targetColorHex)),
                VerticalAlignment = VerticalAlignment.Center,
            });
            sp.Children.Add(headerPanel);

            // ── Click sequence strip ─────────────────────────────────────────
            var strip = new WrapPanel { Orientation = Orientation.Horizontal };

            foreach (var wrongName in trial.WrongClickNames)
            {
                var (wAbbrev, wColorHex) = ParseFigure(wrongName);
                var box = new Border
                {
                    Background      = new SolidColorBrush(Color.FromArgb(0x30, 0xE5, 0x39, 0x35)),
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
                    BorderThickness = new Thickness(1),
                    CornerRadius    = new CornerRadius(3),
                    Margin          = new Thickness(0, 0, 2, 2),
                    Padding         = new Thickness(4, 2, 4, 2),
                    ToolTip         = $"Yanlış: {wrongName}",
                };
                box.Child = new TextBlock
                {
                    Text       = wAbbrev,
                    FontSize   = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(wColorHex)),
                };
                strip.Children.Add(box);
            }

            // Final result block
            var resultBox = new Border
            {
                Background      = trial.Found
                    ? new SolidColorBrush(Color.FromArgb(0x40, 0x43, 0xA0, 0x47))
                    : new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0x98, 0x00)),
                BorderBrush = trial.Found
                    ? new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47))
                    : new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
                BorderThickness = new Thickness(1.5),
                CornerRadius    = new CornerRadius(3),
                Margin          = new Thickness(0, 0, 0, 2),
                Padding         = new Thickness(4, 2, 4, 2),
                ToolTip         = trial.Found ? $"Bulundu: {trial.TargetName}" : "Kaçırıldı",
            };
            resultBox.Child = new TextBlock
            {
                Text       = trial.Found ? $"✅ {targetAbbrev}" : "⏱",
                FontSize   = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = trial.Found
                    ? new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20))
                    : new SolidColorBrush(Color.FromRgb(0xE6, 0x51, 0x00)),
            };
            strip.Children.Add(resultBox);

            sp.Children.Add(strip);

            // ── RT label ─────────────────────────────────────────────────────
            sp.Children.Add(new TextBlock
            {
                Text       = trial.Found ? $"{trial.ResponseTimeMs:F0} ms" : "—",
                FontSize   = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                Margin     = new Thickness(0, 2, 0, 0),
            });

            outerBorder.Child = sp;
            return outerBorder;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            new DedektifStartWindow().Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
