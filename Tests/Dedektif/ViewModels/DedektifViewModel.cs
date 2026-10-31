using System;
using System.Collections.Generic;
using System.Windows.Threading;
using PsikologTestApp.Tests.Dedektif.Models;

namespace PsikologTestApp.Tests.Dedektif.ViewModels
{
    public class DedektifViewModel
    {
        // ── Renk paleti ────────────────────────────────────────────────────────
        private static readonly (string Name, string Hex)[] Colors =
        {
            ("kırmızı", "#E53935"),
            ("mavi",    "#1E88E5"),
            ("yeşil",   "#43A047"),
            ("sarı",    "#F9A825"),
            ("mor",     "#8E24AA"),
            ("turuncu", "#FB8C00"),
        };

        // ── Şekiller ───────────────────────────────────────────────────────────
        private static readonly (string Name, string Symbol)[] Shapes =
        {
            ("kare",        "■"),
            ("daire",       "●"),
            ("yıldız",      "★"),
            ("üçgen",       "▲"),
            ("boş kare",    "□"),
            ("boş daire",   "○"),
            ("boş yıldız",  "☆"),
            ("boş üçgen",   "△"),
        };



        // ── Dahili durum ───────────────────────────────────────────────────────
        private readonly Random           _rng    = new();
        private readonly DispatcherTimer  _timer  = new();
        private readonly List<FigureItem> _pool   = new();
        private readonly DedektifResult   _result;

        private DateTime _trialStart;
        private int      _wrongClicks;
        private int      _trialNumber;
        private TimeSpan _remaining;
        private readonly List<string> _wrongClickNames = new();

        // ── Dışa açık özellikler ───────────────────────────────────────────────
        public string      TimeLeft    { get; private set; } = "";
        public FigureItem? Target      { get; private set; }
        public int         WrongClicks => _wrongClicks;

        // ── Olaylar ────────────────────────────────────────────────────────────
        public event Action<string>? TimeUpdated;
        public event Action?         TestCompleted;

        // ── Yapıcı ─────────────────────────────────────────────────────────────
        public DedektifViewModel(int durationMinutes, int figureCount)
        {
            _result = new DedektifResult
            {
                FigureCount     = figureCount,
                DurationMinutes = durationMinutes,
                TestDate        = DateTime.Now,
            };

            _remaining = TimeSpan.FromMinutes(durationMinutes);
            TimeLeft   = FormatTime(_remaining);

            // Havuzu oluştur: 4 şekil × 6 renk
            foreach (var (cName, cHex) in Colors)
                foreach (var (sName, sSymbol) in Shapes)
                    _pool.Add(new FigureItem
                    {
                        ColorName = cName, ColorHex = cHex,
                        ShapeName = sName, Symbol   = sSymbol
                    });

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick    += Timer_Tick;
        }

        private static string FormatTime(TimeSpan t)
            => $"{(int)t.TotalMinutes:D2}:{t.Seconds:D2}";

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            TimeLeft   = FormatTime(_remaining);
            TimeUpdated?.Invoke(TimeLeft);

            if (_remaining.TotalSeconds <= 0)
            {
                _timer.Stop();

                // Süre bittiğinde aktif denemeyi kaçırıldı olarak kaydet
                if (Target != null)
                {
                    _result.Trials.Add(new DedektifTrial
                    {
                        TrialNumber    = _trialNumber,
                        TargetName     = Target.DisplayName,
                        ResponseTimeMs = 0,
                        WrongClicks    = _wrongClicks,
                        WrongClickNames = new List<string>(_wrongClickNames),
                        Found          = false,
                    });
                }

                TestCompleted?.Invoke();
            }
        }

        /// <summary>Testi başlatır — ilk denemenin figürlerini ve hedefini döndürür.</summary>
        public (List<FigureItem> Figures, FigureItem Target) StartTest()
        {
            _timer.Start();
            return BeginTrial();
        }

        /// <summary>Bir sonraki denemeye geçer.</summary>
        public (List<FigureItem> Figures, FigureItem Target) NextTrial()
            => BeginTrial();

        private (List<FigureItem> Figures, FigureItem Target) BeginTrial()
        {
            _wrongClicks = 0;
            _wrongClickNames.Clear();
            _trialNumber++;

            var figures = _pool.OrderBy(_ => _rng.Next()).Take(_result.FigureCount).ToList();
            Target      = figures[_rng.Next(figures.Count)];
            _trialStart = DateTime.Now;

            return (figures, Target);
        }

        /// <summary>
        /// Kullanıcı bir figüre tıkladı.
        /// Doğru ise denemeyi kaydedip true döner; yanlış ise yanlış sayacını artırıp false döner.
        /// </summary>
        public bool HandleClick(FigureItem figure)
        {
            if (figure.DisplayName == Target?.DisplayName)
            {
                double ms = (DateTime.Now - _trialStart).TotalMilliseconds;
                _result.Trials.Add(new DedektifTrial
                {
                    TrialNumber    = _trialNumber,
                    TargetName     = Target.DisplayName,
                    ResponseTimeMs = ms,
                    WrongClicks    = _wrongClicks,
                    WrongClickNames = new List<string>(_wrongClickNames),
                    Found          = true,
                });
                return true;
            }

            _wrongClicks++;
            _wrongClickNames.Add(figure.DisplayName);
            return false;
        }

        public DedektifResult GetResult() => _result;
        public void Stop() => _timer.Stop();
        public void Resume() => _timer.Start();
    }
}
