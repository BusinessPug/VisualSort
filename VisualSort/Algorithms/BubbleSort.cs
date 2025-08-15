using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class BubbleSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (uiUpdater.AbortSorting)
                    {
                        uiUpdater.ClearFocus();
                        uiUpdater.ShuffleArray();
                        uiUpdater.DrawArray();
                        uiUpdater.ResetUI();
                        return;
                    }

                    // Focus the element being dragged to the right
                    uiUpdater.SetFocus(j + 1);

                    if (array[j] > array[j + 1])
                    {
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }

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