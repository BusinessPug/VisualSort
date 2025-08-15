using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class InsertionSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;

            for (int i = 1; i < n; i++)
            {
                if (uiUpdater.AbortSorting)
                {
                    uiUpdater.ClearFocus();
                    uiUpdater.ShuffleArray();
                    uiUpdater.DrawArray();
                    uiUpdater.ResetUI();
                    return;
                }

                int key = array[i];
                int j = i - 1;

                uiUpdater.SetFocus(i);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);

                while (j >= 0 && array[j] > key)
                {
                    if (uiUpdater.AbortSorting)
                    {
                        uiUpdater.ClearFocus();
                        uiUpdater.ShuffleArray();
                        uiUpdater.DrawArray();
                        uiUpdater.ResetUI();
                        return;
                    }

                    array[j + 1] = array[j];

                    uiUpdater.SetFocus(j, j + 1);
                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);

                    j--;
                }

                array[j + 1] = key;

                uiUpdater.SetFocus(j + 1);
                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);
            }

            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }
    }
}