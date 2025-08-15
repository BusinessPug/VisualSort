using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VisualSort.Helpers;
using VisualSort.ViewModels;

namespace VisualSort
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;
        private readonly Path _barsPath = new() { Fill = Brushes.DodgerBlue, SnapsToDevicePixels = true };
        private readonly Path _finishedPath = new() { Fill = Brushes.LimeGreen, SnapsToDevicePixels = true };
        private readonly Path _focusedPath = new() { Fill = Brushes.Red, SnapsToDevicePixels = true };
        private bool _renderOnce;

        public MainWindow()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Loaded += (_, __) =>
            {
                HookViewModel();

                arrayCanvas.Children.Clear();
                arrayCanvas.Children.Add(_barsPath);
                arrayCanvas.Children.Add(_finishedPath); // green overlay between blue and red
                arrayCanvas.Children.Add(_focusedPath);

                _renderOnce = true;
                RenderArray();
            };

            arrayCanvas.SizeChanged += (_, __) =>
            {
                _renderOnce = true;
                RenderArray();
            };

            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object sender, System.EventArgs e)
        {
            if (_vm == null) return;

            if (_vm.IsSorting || _vm.ConsumeRenderRequest() || _renderOnce)
            {
                _renderOnce = false;
                RenderArray();
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnhookViewModel(e.OldValue as MainViewModel);
            HookViewModel();
            _renderOnce = true;
            RenderArray();
        }

        private void HookViewModel()
        {
            _vm = DataContext as MainViewModel;
            if (_vm != null)
            {
                _vm.PropertyChanged += OnViewModelPropertyChanged;
                ToneService.SetEnabled(_vm.AudioEnabled);
            }
        }

        private void UnhookViewModel(MainViewModel oldVm)
        {
            if (oldVm != null)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsSorting))
            {
                _renderOnce = true;
            }
            else if (e.PropertyName == nameof(MainViewModel.AudioEnabled))
            {
                ToneService.SetEnabled(_vm.AudioEnabled);
            }
        }

        private void RenderArray()
        {
            if (_vm == null || _vm.Array == null || _vm.Array.Length == 0)
            {
                _barsPath.Data = null;
                _finishedPath.Data = null;
                _focusedPath.Data = null;
                return;
            }

            double width = arrayCanvas.ActualWidth;
            double height = arrayCanvas.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                _barsPath.Data = null;
                _finishedPath.Data = null;
                _focusedPath.Data = null;
                return;
            }

            var data = _vm.Array;
            int n = data.Length;
            int maxVal = data.Max();
            if (maxVal <= 0)
            {
                _barsPath.Data = null;
                _finishedPath.Data = null;
                _focusedPath.Data = null;
                return;
            }

            double barWidth = width / n;
            if (barWidth < 1) barWidth = 1;

            var blueGeom = new StreamGeometry();
            var greenGeom = new StreamGeometry();
            var redGeom = new StreamGeometry();

            using (var bctx = blueGeom.Open())
            using (var gctx = greenGeom.Open())
            using (var rctx = redGeom.Open())
            {
                var focused = _vm.FocusedIndices.ToHashSet();
                int finishedCount = _vm.FinishedProgress;

                for (int i = 0; i < n; i++)
                {
                    double barHeight = (data[i] / (double)maxVal) * height;
                    if (barHeight < 1) barHeight = 1;

                    double x = i * barWidth;
                    double y = height - barHeight;

                    StreamGeometryContext targetCtx;
                    if (focused.Contains(i))
                        targetCtx = rctx;
                    else if (i < finishedCount)
                        targetCtx = gctx;
                    else
                        targetCtx = bctx;

                    targetCtx.BeginFigure(new Point(x, y), isFilled: true, isClosed: true);
                    targetCtx.LineTo(new Point(x + barWidth, y), true, false);
                    targetCtx.LineTo(new Point(x + barWidth, y + barHeight), true, false);
                    targetCtx.LineTo(new Point(x, y + barHeight), true, false);
                }
            }

            blueGeom.Freeze();
            greenGeom.Freeze();
            redGeom.Freeze();

            _barsPath.Data = blueGeom;
            _finishedPath.Data = greenGeom;
            _focusedPath.Data = redGeom;
        }
    }
}