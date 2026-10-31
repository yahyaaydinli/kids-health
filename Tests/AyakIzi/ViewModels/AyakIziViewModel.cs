using System;
using System.Collections.Generic;
using PsikologTestApp.Tests.AyakIzi.Models;

namespace PsikologTestApp.Tests.AyakIzi.ViewModels
{
    public enum ClickResult { Correct, RoundComplete, Wrong }

    public class AyakIziViewModel
    {
        private readonly Random        _rng    = new();
        private readonly AyakIziResult _result;
        private List<int>              _sequence = new();
        private int                    _inputIdx;
        private DateTime               _inputStart;
        private int                    _newSeqCount;

        public int GridSize   { get; }
        public int GridCols   { get; }
        public int SpanLength => _sequence.Count;
        public int RoundNumber => _result.Rounds.Count + 1;

        public AyakIziViewModel(int gridSize)
        {
            GridSize = gridSize;
            GridCols = (int)Math.Sqrt(gridSize);
            _result  = new AyakIziResult { GridSize = gridSize };
        }

        /// <summary>Appends one random block to the sequence and returns the full sequence.</summary>
        public List<int> BeginNextRound()
        {
            _sequence.Add(_rng.Next(GridSize));
            _inputIdx    = 0;
            _newSeqCount = 0;
            return new List<int>(_sequence);
        }

        /// <summary>Resets the user-input pointer (used on pause/resume).</summary>
        public void ResetInput() => _inputIdx = 0;

        /// <summary>Generates a brand-new random sequence with the SAME span length (retry at same level).</summary>
        public List<int> RegenerateCurrentSpan()
        {
            int len = _sequence.Count;
            _sequence = new List<int>();
            for (int i = 0; i < len; i++)
                _sequence.Add(_rng.Next(GridSize));
            _inputIdx = 0;
            _newSeqCount++;
            return new List<int>(_sequence);
        }

        public void MarkInputStart() => _inputStart = DateTime.Now;

        public ClickResult HandleClick(int blockIndex)
        {
            if (blockIndex == _sequence[_inputIdx])
            {
                _inputIdx++;
                if (_inputIdx == _sequence.Count)
                {
                    double ms = (DateTime.Now - _inputStart).TotalMilliseconds;
                    _result.Rounds.Add(new AyakIziRound
                    {
                        RoundNumber      = _result.Rounds.Count + 1,
                        SequenceLength   = _sequence.Count,
                        Success          = true,
                        ResponseTimeMs   = ms,
                        NewSequenceCount = _newSeqCount,
                    });
                    if (_sequence.Count > _result.MaxSpanReached)
                        _result.MaxSpanReached = _sequence.Count;
                    return ClickResult.RoundComplete;
                }
                return ClickResult.Correct;
            }
            else
            {
                _result.Rounds.Add(new AyakIziRound
                {
                    RoundNumber      = _result.Rounds.Count + 1,
                    SequenceLength   = _sequence.Count,
                    Success          = false,
                    ResponseTimeMs   = (DateTime.Now - _inputStart).TotalMilliseconds,
                    NewSequenceCount = _newSeqCount,
                });
                return ClickResult.Wrong;
            }
        }

        public AyakIziResult GetResult() => _result;
    }
}
