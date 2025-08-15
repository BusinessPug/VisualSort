using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VisualSort.Helpers
{
    public static class ToneService
    {
        private static readonly object _lock = new();
        private static IWavePlayer _output;
        private static SynthToneProvider _provider;
        private static bool _enabled;

        // Lower frequency range (A2 ~110 Hz to A5 ~1680 Hz)
        private const double MinHz = 110.0;
        private const double MaxHz = 1680.0;

        public static void SetEnabled(bool enabled)
        {
            lock (_lock)
            {
                _enabled = enabled;
                EnsureInitialized();
                if (_provider == null) return;

                if (enabled)
                {
                    _provider.SetEnabled(true);
                    if (_output?.PlaybackState != PlaybackState.Playing)
                        _output?.Play();
                }
                else
                {
                    _provider.SetEnabled(false);
                }
            }
        }

        public static void UpdateValue(int value, int maxValue)
        {
            if (!_enabled) return;
            if (maxValue <= 0) return;

            lock (_lock)
            {
                EnsureInitialized();
                if (_provider == null) return;

                // Normalize 0..1 and map logarithmically to pitch range (perceptually more even)
                double norm = Math.Clamp((value - 1) / (double)Math.Max(1, maxValue - 1), 0.0, 1.0);
                double hz = MinHz * Math.Pow(MaxHz / MinHz, norm);

                _provider.SetTargetFrequency(hz);
                // Keep enabled to avoid attack clicks on every focus change
                _provider.SetEnabled(true);
            }
        }

        public static void Silence()
        {
            lock (_lock)
            {
                _provider?.SetEnabled(false);
            }
        }

        private static void EnsureInitialized()
        {
            if (_provider != null && _output != null) return;

            if (_provider == null)
                _provider = new SynthToneProvider(sampleRate: 44100);

            if (_output == null)
            {
                try
                {
                    // Low-latency shared WASAPI, event-driven
                    _output = new WasapiOut(AudioClientShareMode.Shared, true, 8);
                }
                catch
                {
                    // Fallback to WaveOut with low latency
                    _output = new WaveOutEvent
                    {
                        DesiredLatency = 10,
                        NumberOfBuffers = 2
                    };
                }
                _output.Init(_provider);
            }
        }
    }
}