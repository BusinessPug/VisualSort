using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class MergeSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();

            bool completed = await MergeSortRecursive(array, 0, array.Length - 1, uiUpdater);
            if (!completed || uiUpdater.AbortSorting)
            {
                uiUpdater.ClearFocus();
                uiUpdater.ShuffleArray();
                uiUpdater.DrawArray();
                uiUpdater.ResetUI();
                return;
            }

            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }

        private async Task<bool> MergeSortRecursive(int[] array, int left, int right, IUIUpdater uiUpdater)
        {
            if (uiUpdater.AbortSorting) return false;
            if (left >= right) return true;

            int mid = left + (right - left) / 2;

            if (!await MergeSortRecursive(array, left, mid, uiUpdater)) return false;
            if (!await MergeSortRecursive(array, mid + 1, right, uiUpdater)) return false;

            return await Merge(array, left, mid, right, uiUpdater);
        }

        private async Task<bool> Merge(int[] array, int left, int mid, int right, IUIUpdater uiUpdater)
        {
            int n1 = mid - left + 1;
            int n2 = right - mid;

            int[] L = new int[n1];
            int[] R = new int[n2];

            Array.Copy(array, left, L, 0, n1);
            Array.Copy(array, mid + 1, R, 0, n2);

            int i = 0, j = 0, k = left;

            while (i < n1 && j < n2)
            {
                if (uiUpdater.AbortSorting) return false;

                if (L[i] <= R[j])
                {
                    array[k] = L[i++];
                }
                else
                {
                    array[k] = R[j++];
                }

                uiUpdater.SetFocus(k);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);
                k++;
            }

            while (i < n1)
            {
                if (uiUpdater.AbortSorting) return false;

                array[k] = L[i++];
                uiUpdater.SetFocus(k);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);
                k++;
            }

            while (j < n2)
            {
                if (uiUpdater.AbortSorting) return false;

                array[k] = R[j++];
                uiUpdater.SetFocus(k);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);
                k++;
            }

            return true;
        }
    }
}