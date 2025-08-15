using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using VisualSort.Algorithms;
using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IUIUpdater
    {
        private int[] _array;
        private int _arraySize = 256;
        private bool _abortSorting;
        private bool _isSorting;

        private double _speed = 75.0;
        private readonly Stopwatch _paceWatch = Stopwatch.StartNew();

        private readonly HashSet<int> _focused = new HashSet<int>();
        private int _renderRequested;

        private readonly RelayCommand _startCommand;
        private readonly RelayCommand _shuffleCommand;
        private readonly RelayCommand _abortCommand;
        private readonly RelayCommand _setArraySizeCommand;

        public ObservableCollection<string> SortingAlgorithms { get; } = new ObservableCollection<string>
        {
            "Bubble Sort",
            "Cocktail Shaker Sort",
            "Bogo Sort",
            "Radix Sort Base 10",
            "Radix Sort Base 4",
            "Comb Sort",
            "Gnome Sort",
            "Quick Sort",
            "Heap Sort",
            "Shell Sort",
            "Selection Sort",
            "Insertion Sort",
            "Merge Sort"
        };

        public string SelectedAlgorithm { get; set; } = "Bubble Sort";

        public int ArraySize
        {
            get => _arraySize;
            set
            {
                if (_arraySize != value)
                {
                    _arraySize = value;
                    OnPropertyChanged();
                }
            }
        }

        // Speed: 0..100 (0 = slowest, 100 = fastest)
        public double Speed
        {
            get => _speed;
            set
            {
                double v = Math.Clamp(value, 0.0, 100.0);
                if (Math.Abs(_speed - v) > double.Epsilon)
                {
                    _speed = v;
                    OnPropertyChanged();
                }
            }
        }

        // Toggle audio playback
        private bool _audioEnabled = true;
        public bool AudioEnabled
        {
            get => _audioEnabled;
            set
            {
                if (_audioEnabled != value)
                {
                    _audioEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        // Finished sweep progress (0..Array.Length). Bars [0..progress-1] are green.
        private int _finishedProgress;
        public int FinishedProgress
        {
            get => _finishedProgress;
            private set
            {
                if (_finishedProgress != value)
                {
                    _finishedProgress = value;
                    OnPropertyChanged();
                    DrawArray();
                }
            }
        }

        public int[] Array => _array;
        public IReadOnlyCollection<int> FocusedIndices
        {
            get
            {
                lock (_focused)
                {
                    return _focused.ToArray();
                }
            }
        }
        public bool AbortSorting => _abortSorting;

        public ICommand StartCommand => _startCommand;
        public ICommand ShuffleCommand => _shuffleCommand;
        public ICommand AbortCommand => _abortCommand;
        public ICommand SetArraySizeCommand => _setArraySizeCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _startCommand = new RelayCommand(async () => await StartSortTask(), () => !IsSorting);
            _shuffleCommand = new RelayCommand(ShuffleArray, () => !IsSorting);
            _abortCommand = new RelayCommand(() => _abortSorting = true);
            _setArraySizeCommand = new RelayCommand(SetArraySize, () => !IsSorting);

            InitializeArray();
        }

        private void InitializeArray()
        {
            _abortSorting = false;
            lock (_focused) _focused.Clear();
            _array = new int[_arraySize];
            for (int i = 0; i < _arraySize; i++)
                _array[i] = i + 1;

            FinishedProgress = 0;
            ShuffleArray();
        }

        public void ShuffleArray()
        {
            var rnd = new Random();
            for (int i = _array.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (_array[i], _array[j]) = (_array[j], _array[i]);
            }
            FinishedProgress = 0;
            ClearFocus();
            DrawArray();
        }

        // Coalesced render request
        public void DrawArray() => Interlocked.Exchange(ref _renderRequested, 1);
        public bool ConsumeRenderRequest() => Interlocked.Exchange(ref _renderRequested, 0) == 1;

        public bool IsSorting
        {
            get => _isSorting;
            set
            {
                if (_isSorting != value)
                {
                    _isSorting = value;
                    OnPropertyChanged();
                    _startCommand.RaiseCanExecuteChanged();
                    _shuffleCommand.RaiseCanExecuteChanged();
                    _setArraySizeCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public void StartSort() => RunOnUI(() =>
        {
            IsSorting = true;
            FinishedProgress = 0; // reset any previous green overlay
        });

        public void SetFocus(params int[] indices)
        {
            int firstValid = -1;

            lock (_focused)
            {
                _focused.Clear();
                if (indices != null)
                {
                    foreach (var i in indices)
                    {
                        if (i >= 0 && i < _array.Length)
                        {
                            _focused.Add(i);
                            if (firstValid == -1) firstValid = i;
                        }
                    }
                }
            }

            // Update audio immediately (not tied to frame rate)
            if (AudioEnabled && firstValid != -1)
            {
                ToneService.UpdateValue(_array[firstValid], _array.Length);
            }

            DrawArray();
        }

        public void ClearFocus()
        {
            lock (_focused)
            {
                if (_focused.Count == 0) return;
                _focused.Clear();
            }
            DrawArray();
        }

        public void ResetUI() => RunOnUI(() =>
        {
            IsSorting = false;
            _abortSorting = false;
            ClearFocus();
            ToneService.Silence(); // fade out when done
            DrawArray();
        });

        public async Task Delay(int _)
        {
            const double maxDelayUs = 200_000; // 200ms in microseconds
            const double gamma = 3.0;

            double t = Speed / 100.0;               // 0..1
            double delayUs = maxDelayUs * Math.Pow(1 - t, gamma);

            if (delayUs <= 50) return;

            if (delayUs >= 2000)
            {
                int ms = (int)(delayUs / 1000.0);
                await Task.Delay(ms).ConfigureAwait(false);
                return;
            }

            long targetTicks = (long)(delayUs * (Stopwatch.Frequency / 1_000_000.0));
            long start = _paceWatch.ElapsedTicks;

            while (true)
            {
                long elapsed = _paceWatch.ElapsedTicks - start;
                long remaining = targetTicks - elapsed;
                if (remaining <= 0) break;

                if (remaining > Stopwatch.Frequency / 1000)
                    await Task.Delay(0).ConfigureAwait(false);
                else
                    Thread.SpinWait(50);
            }
        }

        private async Task StartSortTask()
        {
            if (string.IsNullOrEmpty(SelectedAlgorithm))
                return;

            StartSort();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ISortingAlgorithm sortingAlgorithm = SelectedAlgorithm switch
            {
                "Bubble Sort" => new BubbleSort(),
                "Cocktail Shaker Sort" => new CocktailShakerSort(),
                "Bogo Sort" => new BogoSort(),
                "Radix Sort Base 10" => new RadixSortBaseTen(),
                "Radix Sort Base 4" => new RadixSortBaseFour(),
                "Comb Sort" => new CombSort(),
                "Gnome Sort" => new GnomeSort(),
                "Quick Sort" => new QuickSort(),
                "Heap Sort" => new HeapSort(),
                "Shell Sort" => new ShellSort(),
                "Selection Sort" => new SelectionSort(),
                "Insertion Sort" => new InsertionSort(),
                "Merge Sort" => new MergeSort(),
                _ => null
            };
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (sortingAlgorithm != null)
            {
                await Task.Run(async () => await sortingAlgorithm.Sort(_array, this).ConfigureAwait(false)).ConfigureAwait(false);
            }

            await RunFinishedSequenceAsync().ConfigureAwait(false);
            ResetUI();
        }

        private void SetArraySize() => InitializeArray();

        public void DelayOnFocusLost()
        {
            if (_focused.Count == 0)
                return;

            ToneService.Silence();
        }

        private async Task RunFinishedSequenceAsync()
        {
            // Clear red focus before sweep
            ClearFocus();

            FinishedProgress = 0;

            const int finishDelayMs = 5; // hardcoded delay, not tied to Speed slider

            for (int i = 0; i < _array.Length; i++)
            {
                if (_abortSorting) break;

                FinishedProgress = i + 1;

                if (AudioEnabled)
                    ToneService.UpdateValue(_array[i], _array.Length);

                await Task.Delay(finishDelayMs).ConfigureAwait(false);
            }

            ToneService.Silence();
        }

        private static void RunOnUI(Action action)
        {
            var disp = Application.Current?.Dispatcher;
            if (disp == null || disp.CheckAccess())
                action();
            else
                disp.Invoke(action);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}