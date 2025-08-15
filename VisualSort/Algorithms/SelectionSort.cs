using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class SelectionSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;

            for (int i = 0; i < n - 1; i++)
            {
                if (uiUpdater.AbortSorting)
                {
                    uiUpdater.ClearFocus();
                    uiUpdater.ShuffleArray();
                    uiUpdater.DrawArray();
                    uiUpdater.ResetUI();
                    return;
                }

                int minIndex = i;

                for (int j = i + 1; j < n; j++)
                {
                    if (uiUpdater.AbortSorting)
                    {
                        uiUpdater.ClearFocus();
                        uiUpdater.ShuffleArray();
                        uiUpdater.DrawArray();
                        uiUpdater.ResetUI();
                        return;
                    }

                    uiUpdater.SetFocus(j, minIndex);
                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);

                    if (array[j] < array[minIndex])
                    {
                        minIndex = j;
                    }
                }

                if (minIndex != i)
                {
                    (array[i], array[minIndex]) = (array[minIndex], array[i]);

                    uiUpdater.SetFocus(i, minIndex);
                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);
                }
            }

            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }
    }
}