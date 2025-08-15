using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class HeapSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;

            for (int i = n / 2 - 1; i >= 0; i--)
            {
                if (!await Heapify(array, n, i, uiUpdater))
                {
                    uiUpdater.ClearFocus();
                    uiUpdater.ShuffleArray();
                    uiUpdater.DrawArray();
                    uiUpdater.ResetUI();
                    return;
                }
            }

            for (int i = n - 1; i > 0; i--)
            {
                if (uiUpdater.AbortSorting)
                {
                    uiUpdater.ClearFocus();
                    uiUpdater.ShuffleArray();
                    uiUpdater.DrawArray();
                    uiUpdater.ResetUI();
                    return;
                }

                (array[0], array[i]) = (array[i], array[0]);

                uiUpdater.SetFocus(0, i);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);

                if (!await Heapify(array, i, 0, uiUpdater))
                {
                    uiUpdater.ClearFocus();
                    uiUpdater.ShuffleArray();
                    uiUpdater.DrawArray();
                    uiUpdater.ResetUI();
                    return;
                }
            }

            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }

        private async Task<bool> Heapify(int[] array, int heapSize, int root, IUIUpdater uiUpdater)
        {
            while (true)
            {
                if (uiUpdater.AbortSorting) return false;

                int largest = root;
                int left = 2 * root + 1;
                int right = 2 * root + 2;

                if (left < heapSize)
                {
                    uiUpdater.SetFocus(root, left);
                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);
                    if (array[left] > array[largest]) largest = left;
                }

                if (right < heapSize)
                {
                    uiUpdater.SetFocus(largest, right);
                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);
                    if (array[right] > array[largest]) largest = right;
                }

                if (largest == root) break;

                (array[root], array[largest]) = (array[largest], array[root]);

                uiUpdater.SetFocus(root, largest);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);

                root = largest;
            }

            return true;
        }
    }
}