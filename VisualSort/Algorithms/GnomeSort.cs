using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class GnomeSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;
            int index = 0;

            while (index < n)
            {
                if (uiUpdater.AbortSorting)
                {
                    uiUpdater.ClearFocus();
                    uiUpdater.ShuffleArray();
                    uiUpdater.DrawArray();
                    uiUpdater.ResetUI();
                    return;
                }

                uiUpdater.SetFocus(index);

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
                }

                uiUpdater.DrawArray();
                await uiUpdater.Delay(10);
            }

            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }
    }
}