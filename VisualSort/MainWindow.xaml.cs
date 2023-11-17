using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VisualSort
{
    public partial class MainWindow : Window
    {
        private int[] array;
        private int ArraySize = 256;
        private bool abortSorting = false;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ArraySize = 256; // Set the initial size of the array
            InitializeArray(); // Initialize the array with 256 elements
            DrawArray(); // Draw the array on the canvas
        }


        private void SetArraySize_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(arraySizeTextBox.Text, out int newSize) && newSize > 0)
            {
                ArraySize = newSize;
                InitializeArray();   // Re-initialize the array with consecutive values
                DrawArray();         // Redraw the array
            }
            else
            {
                MessageBox.Show("Please enter a valid array size.");
            }
        }


        private void InitializeArray()
        {
            array = new int[ArraySize];
            for (int i = 0; i < ArraySize; i++)
            {
                array[i] = i + 1; // Fill the array with consecutive numbers
            }
            ShuffleArray(); // Shuffle the array to randomize the order
        }

        private void ShuffleArray()
        {
            Random rnd = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }


        private void DrawArray()
        {
            arrayCanvas.Children.Clear();
            double canvasWidth = arrayCanvas.ActualWidth; // Use the actual width of the canvas
            double canvasHeight = arrayCanvas.ActualHeight; // Use the actual height of the canvas
            double barWidth = canvasWidth / ArraySize; // Calculate bar width based on array size

            for (int i = 0; i < ArraySize; i++)
            {
                Rectangle rect = new Rectangle
                {
                    Width = barWidth,
                    Height = (array[i] / (double)ArraySize) * canvasHeight, // Scale height to fit canvas
                    Fill = Brushes.Blue
                };

                Canvas.SetLeft(rect, i * barWidth);
                Canvas.SetBottom(rect, 0);
                arrayCanvas.Children.Add(rect);
            }
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleArray();
            DrawArray();
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            abortSorting = true;
        }

        private void ResetUI()
        {
            startButton.IsEnabled = true;
            abortButton.IsEnabled = false;
            shuffleButton.IsEnabled = true;
            sortMethodComboBox.IsEnabled = true;
            speedSlider.IsEnabled = true;
            setsize.IsEnabled = true;
            arraySizeTextBox.IsEnabled = true;
        }

        private void StartSort()
        {
            startButton.IsEnabled = false;
            abortButton.IsEnabled = true;
            shuffleButton.IsEnabled = false;
            speedSlider.IsEnabled = false;
            sortMethodComboBox.IsEnabled = false;
            abortSorting = false;
            setsize.IsEnabled = false;
            arraySizeTextBox.IsEnabled = false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedItem = ((ComboBoxItem)sortMethodComboBox.SelectedItem);
            string selectedAlgorithm = selectedItem.Content.ToString();
            switch (selectedAlgorithm)
            {
                case "Bubble Sort":
                    BubbleSort();
                    break;
                case "Gnome Sort":
                    GnomeSort();
                    break;
                case "Cocktail Shaker Sort":
                    CocktailShakerSort();
                    break;
                case "Radix Sort":
                    RadixSort();
                    break;
                case "Comb Sort":
                    CombSort();
                    break;
                case "Bogo Sort":
                    BogoSort();
                    break;
                default:
                    MessageBox.Show("Please select a sorting algorithm.");
                    break;
            }
        }

        private async void BubbleSort()
        {

            StartSort();
            int n = array.Length;
            int updateFrequency = Math.Max(1, (int)(n / (speedSlider.Value + 1)));

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (abortSorting)
                    {
                        ShuffleArray();
                        DrawArray();
                        ResetUI();
                        return;
                    }

                    if (array[j] > array[j + 1])
                    {

                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }

                    if (j % updateFrequency == 0)
                    {
                        Dispatcher.Invoke(() => UpdateCanvas(j, j + 1));
                        await Task.Delay((int)speedSlider.Value / 2);
                        Dispatcher.Invoke(() => UpdateCanvas(j, j + 1, resetColor: true));
                    }
                }
            }
            if (!abortSorting)
            {
                DrawArray();
                ResetUI();
            }
        }

        private async void GnomeSort()
        {
            StartSort();

            int n = array.Length;
            int updateFrequency = Math.Max(1, (int)(n / (speedSlider.Value + 1)));

            int index = 0;
            while (index < n)
            {
                if (abortSorting)
                {
                    ShuffleArray();
                    DrawArray();
                    ResetUI();
                    return;
                }

                if (index == 0 || array[index] >= array[index - 1])
                {
                    index++;
                }
                else
                {

                    int temp = array[index];
                    array[index] = array[index - 1];
                    array[index - 1] = temp;

                    index--;

                    if (index % updateFrequency == 0 || index == 0)
                    {
                        Dispatcher.Invoke(() => UpdateCanvas(Math.Max(0, index - 1), index));
                        await Task.Delay((int)speedSlider.Value / 2);
                        Dispatcher.Invoke(() => UpdateCanvas(Math.Max(0, index - 1), index, resetColor: true));
                    }
                }

            }
            if (!abortSorting)
            {
                DrawArray();
                ResetUI();
            }
        }

        private async void CocktailShakerSort()
        {
            StartSort();
            int n = array.Length;
            int updateFrequency = Math.Max(1, (int)(n / (speedSlider.Value + 1)));
            bool swapped;

            do
            {
                if (abortSorting)
                {
                    ShuffleArray();
                    DrawArray();
                    ResetUI();
                    return;
                }

                swapped = false;
                // Forward pass
                for (int i = 0; i < n - 1; i++)
                {
                    if (array[i] > array[i + 1])
                    {
                        int temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        swapped = true;
                    }

                    if (i % updateFrequency == 0 || i == n - 2)
                    {
                        if (abortSorting)
                        {
                            ShuffleArray();
                            DrawArray();
                            ResetUI();
                            return;
                        }
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1));
                        await Task.Delay((int)speedSlider.Value / 2);
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1, resetColor: true));
                    }
                }

                if (!swapped) break;

                // Backward pass
                for (int i = n - 2; i >= 0; i--)
                {
                    if (array[i] > array[i + 1])
                    {
                        int temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        swapped = true;
                    }

                    if (i % updateFrequency == 0 || i == 0)
                    {
                        if (abortSorting)
                        {
                            ShuffleArray();
                            DrawArray();
                            ResetUI();
                            return;
                        }
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1));
                        await Task.Delay((int)speedSlider.Value / 2);
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1, resetColor: true));
                    }
                }
            } while (swapped);

            if (!abortSorting)
            {
                DrawArray();
                ResetUI();
            }
        }

        private int GetMax(int[] arr)
        {

            return arr.Max();
        }

        private async void RadixSort()
        {
            StartSort();

            int n = array.Length;
            int m = GetMax(array);

            for (int exp = 1; m / exp > 0; exp *= 10)
            {
                await CountSortVisualization(array, n, exp);

                if (abortSorting)
                {
                    ShuffleArray();
                    DrawArray();
                    ResetUI();
                    return;
                }
            }

            if (!abortSorting)
            {
                DrawArray();
                ResetUI();
            }
        }

        private async Task CountSortVisualization(int[] arr, int n, int exp)
        {
            int[] output = new int[n];
            int[] count = new int[10];

            for (int i = 0; i < n; i++)
            {
                int index = (arr[i] / exp) % 10;
                count[index]++;
            }

            for (int i = 1; i < 10; i++)
                count[i] += count[i - 1];

            for (int i = n - 1; i >= 0; i--)
            {
                int index = (arr[i] / exp) % 10;
                output[count[index] - 1] = arr[i];
                count[index]--;

                Dispatcher.Invoke(() => UpdateCanvas(i, count[index]));
                await Task.Delay((int)speedSlider.Value / 2);
            }

            for (int i = 0; i < n; i++)
            {
                if (arr[i] != output[i])
                {
                    int targetIndex = Array.IndexOf(arr, output[i], i); 
                                                                        
                    int temp = arr[i];
                    arr[i] = arr[targetIndex];
                    arr[targetIndex] = temp;

                    Dispatcher.Invoke(() => UpdateCanvas(i, targetIndex));
                    await Task.Delay((int)speedSlider.Value / 2);
                }
            }
        }

        private async void CombSort()
        {
            StartSort();

            int n = array.Length;
            int updateFrequency = Math.Max(1, (int)(n / (speedSlider.Value + 100)));
            int operationCount = 0;
            float shrinkFactor = 1.3f;
            int gap = n;
            bool swapped = true;

            while (gap != 1 || swapped)
            {
                gap = (int)(gap / shrinkFactor);
                if (gap < 1)
                    gap = 1;

                swapped = false;
                for (int i = 0; i < n - gap; i++)
                {
                    if (array[i] > array[i + gap])
                    {
                        int temp = array[i];
                        array[i] = array[i + gap];
                        array[i + gap] = temp;
                        swapped = true;

                        operationCount++;
                        if (operationCount % updateFrequency == 0)
                        {
                            Dispatcher.Invoke(() => UpdateCanvas(i, i + gap));
                            await Task.Delay((int)speedSlider.Value / 2);
                            Dispatcher.Invoke(() => UpdateCanvas(i, i + gap, resetColor: true));
                        }
                    }
                }

                if (abortSorting)
                {
                    ShuffleArray();
                    DrawArray();
                    ResetUI();
                    return;
                }
            }

            if (!abortSorting)
            {
                DrawArray();
                ResetUI();
            }
        }
        private async void BogoSort()
        {
            StartSort();

            int n = array.Length;
            int updateFrequency = Math.Max(1, (int)(n / (speedSlider.Value + 1)));
            int operationCount = 0;

            Random rnd = new Random();

            while (!IsSorted(array))
            {
                if (abortSorting)
                {
                    ShuffleArray();
                    DrawArray();
                    ResetUI();
                    return;
                }

                for (int i = 0; i < n; i++)
                {
                    int j = rnd.Next(i, n);
                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;

                    operationCount++;
                    if (operationCount % updateFrequency == 0)
                    {
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1));
                        await Task.Delay((int)speedSlider.Value / 2);
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1, resetColor: true));
                    }
                }
            }

            if (!abortSorting)
            {
                DrawArray();
                ResetUI();
            }
        }

        private bool IsSorted(int[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i - 1] > arr[i])
                    return false;
            }
            return true;
        }

        private void UpdateCanvas(int index1, int index2, bool resetColor = false)
        {
            double barWidth = arrayCanvas.ActualWidth / ArraySize;
            double canvasHeight = arrayCanvas.ActualHeight;

            for (int i = 0; i < ArraySize; i++)
            {
                Rectangle rect = (Rectangle)arrayCanvas.Children[i];
                rect.Width = barWidth;
                rect.Height = (array[i] / (double)ArraySize) * canvasHeight;

                if ((i == index1 || i == index2) && !resetColor)
                {
                    rect.Fill = Brushes.Red;
                }
                else
                {
                    rect.Fill = Brushes.Blue;
                }
            }
        }
    }
}