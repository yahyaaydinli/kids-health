using System.Diagnostics;

namespace PsikologTestApp.Services
{
    public class ReactionTimeService
    {
        private readonly Random _random = new Random();
        private char _targetLetter;
        private double _waitTimeMs;
        private readonly Stopwatch _waitTimer = new();
        private readonly Stopwatch _reactionTimer = new();
        private readonly List<char> _letters = new();

        public ReactionTimeService()
        {
            for (char c = 'A'; c <= 'Z'; c++)
                _letters.Add(c);
        }

        /// <summary>Test başında bir kez çağrılır. Sabit hedef harf seçer ve döndürür.</summary>
        public char PickRandomTarget()
        {
            _targetLetter = _letters[_random.Next(_letters.Count)];
            return _targetLetter;
        }

        public char GetTargetLetter() => _targetLetter;

        /// <summary>Test başlandığında veya her Space'den sonra bekleme saatını başlatır.</summary>
        public void StartWaiting()
        {
            _waitTimer.Restart();
        }

        /// <summary>Hedef harf ekranda göründüğünde çağrılır: bekleme süresini kaydeder, tepki saatını başlatır.</summary>
        public void TargetLetterAppeared()
        {
            _waitTimeMs = _waitTimer.Elapsed.TotalMilliseconds;
            _waitTimer.Stop();
            _reactionTimer.Restart();
        }

        /// <summary>Son hedef görünümüne kadar geçen bekleme süresini döndürür (ms).</summary>
        public double GetWaitTime() => _waitTimeMs;

        /// <summary>Hedef harf göründüğünden bu yana geçen tepki süresini döndürür (ms).</summary>
        public double GetResponseTime()
        {
            _reactionTimer.Stop();
            return _reactionTimer.Elapsed.TotalMilliseconds;
        }

        public char GetRandomLetter()
        {
            if (_letters.Count <= 1) return _letters[0];
            char letter;
            do
            {
                letter = _letters[_random.Next(_letters.Count)];
            } while (letter == _targetLetter);
            return letter;
        }
    }
}
