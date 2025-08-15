using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class CombSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;
            int gap = n;
            bool swapped = true;

            while (gap != 1 || swapped)
            {
                gap = (int)(gap / 1.3);
                if (gap < 1) gap = 1;

                swapped = false;

                for (int i = 0; i < n - gap; i++)
                {
                    if (uiUpdater.AbortSorting)
                    {
                        uiUpdater.ClearFocus();
                        uiUpdater.ShuffleArray();
                        uiUpdater.DrawArray();
                        uiUpdater.ResetUI();
                        return;
                    }

                    uiUpdater.SetFocus(i + gap);

                    if (array[i] > array[i + gap])
                    {
                        int temp = array[i];
                        array[i] = array[i + gap];
                        array[i + gap] = temp;
                        swapped = true;
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