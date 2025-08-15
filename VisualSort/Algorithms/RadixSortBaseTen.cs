using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class RadixSortBaseTen : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int max = array.Max();

            for (int exp = 1; max / exp > 0; exp *= 10)
            {
                await CountSort(array, exp, uiUpdater);
            }

            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }

        private async Task CountSort(int[] array, int exp, IUIUpdater uiUpdater)
        {
            int n = array.Length;
            int[] output = new int[n];
            int[] count = new int[10];

            for (int i = 0; i < n; i++)
                count[array[i] / exp % 10]++;

            for (int i = 1; i < 10; i++)
                count[i] += count[i - 1];

            for (int i = n - 1; i >= 0; i--)
            {
                output[count[array[i] / exp % 10] - 1] = array[i];
                count[array[i] / exp % 10]--;
            }

            for (int i = 0; i < n; i++)
            {
                array[i] = output[i];
                uiUpdater.SetFocus(i);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);
            }
        }
    }
}