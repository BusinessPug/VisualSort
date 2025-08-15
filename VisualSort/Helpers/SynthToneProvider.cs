using NAudio.Wave;

namespace VisualSort.Helpers
{
    // Sine-wave provider with smoothed frequency and amplitude (attack/release) to avoid clicks
    public sealed class SynthToneProvider : ISampleProvider
    {
        private readonly WaveFormat _waveFormat;
        private readonly int _sampleRate;

        private double _phase;                 // current phase
        private double _currentFreq = 440.0;
        private double _targetFreq = 440.0;

        private float _currentAmp = 0.0f;
        private float _targetAmp = 0.0f;

        private const float BaseAmp = 0.12f;   // overall loudness (lower to avoid clipping)
        private const double TwoPi = Math.PI * 2.0;

        // Time constants (seconds) for smoothing
        // Faster frequency glide, slower amplitude release to avoid clicks
        private const double TauFreq = 0.003;      // ~3ms glide
        private const double TauAmpAttack = 0.004; // ~4ms attack
        private const double TauAmpRelease = 0.020;// ~20ms release

        private readonly double _alphaFreq;
        private readonly double _alphaAttack;
        private readonly double _alphaRelease;

        public SynthToneProvider(int sampleRate = 44100)
        {
            _sampleRate = sampleRate;
            _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

            // Precompute per-sample smoothing coefficients: alpha = 1 - exp(-dt/tau)
            double dt = 1.0 / _sampleRate;
            _alphaFreq = 1.0 - Math.Exp(-dt / TauFreq);
            _alphaAttack = 1.0 - Math.Exp(-dt / TauAmpAttack);
            _alphaRelease = 1.0 - Math.Exp(-dt / TauAmpRelease);
        }

        public WaveFormat WaveFormat => _waveFormat;

        public void SetTargetFrequency(double hz)
        {
            if (double.IsNaN(hz) || double.IsInfinity(hz)) return;
            if (hz < 20.0) hz = 20.0;
            if (hz > 20000.0) hz = 20000.0;
            _targetFreq = hz;
        }

        public void SetEnabled(bool enabled)
        {
            _targetAmp = enabled ? BaseAmp : 0.0f;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            for (int n = 0; n < count; n++)
            {
                // Smooth frequency towards target (glide)
                _currentFreq += (_targetFreq - _currentFreq) * _alphaFreq;

                // Separate attack/release smoothing for amplitude to avoid clicks
                double ampAlpha = (_targetAmp > _currentAmp) ? _alphaAttack : _alphaRelease;
                _currentAmp += (float)((_targetAmp - _currentAmp) * ampAlpha);

                // Phase advance
                double phaseInc = TwoPi * _currentFreq / _sampleRate;
                _phase += phaseInc;
                if (_phase >= TwoPi) _phase -= TwoPi;

                buffer[offset + n] = (float)(Math.Sin(_phase) * _currentAmp);
            }
            return count;
        }
    }
}