using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualSort
{
    public partial class MainWindow : Window
    {
        private int[] array;
        private const int MaxValue = 100; // Maximum value for array elements
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

        }

        private void StartSort()
        {
            startButton.IsEnabled = false;
            abortButton.IsEnabled = true;
            shuffleButton.IsEnabled = false;
            speedSlider.IsEnabled = false;
            sortMethodComboBox.IsEnabled = false;
            abortSorting = false;
        }

        private async void CurrentDelay()
        {
            await Task.Delay(1);
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
                // Add cases for other sorting methods if necessary
                default:
                    MessageBox.Show("Please select a sorting algorithm.");
                    break;
            }
        }

        private async void BubbleSort()
        {

            StartSort();

            int n = array.Length;
            int updateFrequency = Math.Max(1, (int)(n / (speedSlider.Value + 1))); // Example: Update UI after every 5% of the array length

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (abortSorting)
                    {
                        ShuffleArray();
                        DrawArray();
                        ResetUI();
                        return; // Exit the sorting method
                    }

                    if (array[j] > array[j + 1])
                    {
                        // Swap elements
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }

                    // Update UI less frequently
                    if (j % updateFrequency == 0)
                    {
                        Dispatcher.Invoke(() => UpdateCanvas(j, j + 1, resetColor: true));
                        CurrentDelay();
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
                        Dispatcher.Invoke(() => UpdateCanvas(Math.Max(0, index - 1), index, resetColor: true));
                        CurrentDelay();
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
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1, resetColor: true));
                        CurrentDelay();
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
                        Dispatcher.Invoke(() => UpdateCanvas(i, i + 1, resetColor: true));
                        CurrentDelay();
                    }
                }
            } while (swapped);

            if (!abortSorting)
            {
                DrawArray();
                ResetUI();
            }
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