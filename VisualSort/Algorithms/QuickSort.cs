using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class QuickSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            await QuickSortRecursive(array, 0, array.Length - 1, uiUpdater);
            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }

        private async Task QuickSortRecursive(int[] array, int low, int high, IUIUpdater uiUpdater)
        {
            if (low < high)
            {
                int pi = await Partition(array, low, high, uiUpdater);
                if (pi == -1) return;

                await QuickSortRecursive(array, low, pi - 1, uiUpdater);
                await QuickSortRecursive(array, pi + 1, high, uiUpdater);
            }
        }

        private async Task<int> Partition(int[] array, int low, int high, IUIUpdater uiUpdater)
        {
            int pivot = array[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (uiUpdater.AbortSorting) return -1;

                uiUpdater.SetFocus(j, high);

                if (array[j] < pivot)
                {
                    i++;
                    (array[i], array[j]) = (array[j], array[i]);
                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);
                }
            }

            (array[i + 1], array[high]) = (array[high], array[i + 1]);
            uiUpdater.SetFocus(i + 1);
            uiUpdater.DrawArray();
            await uiUpdater.Delay(10);

            return i + 1;
        }
    }
}