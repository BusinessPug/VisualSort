using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class ShellSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;

            for (int gap = n / 2; gap > 0; gap /= 2)
            {
                for (int i = gap; i < n; i++)
                {
                    if (uiUpdater.AbortSorting)
                    {
                        uiUpdater.ClearFocus();
                        uiUpdater.ShuffleArray();
                        uiUpdater.DrawArray();
                        uiUpdater.ResetUI();
                        return;
                    }

                    int temp = array[i];
                    int j = i;

                    uiUpdater.SetFocus(i);
                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);

                    while (j >= gap && array[j - gap] > temp)
                    {
                        if (uiUpdater.AbortSorting)
                        {
                            uiUpdater.ClearFocus();
                            uiUpdater.ShuffleArray();
                            uiUpdater.DrawArray();
                            uiUpdater.ResetUI();
                            return;
                        }

                        array[j] = array[j - gap];

                        uiUpdater.SetFocus(j, j - gap);
                        uiUpdater.DrawArray();
                        await uiUpdater.Delay(10);

                        j -= gap;
                    }

                    array[j] = temp;

                    uiUpdater.SetFocus(j, i);
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