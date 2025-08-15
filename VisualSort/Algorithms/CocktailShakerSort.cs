using VisualSort.Helpers;
using VisualSort.Sorting;

namespace VisualSort.Algorithms
{
    public class CocktailShakerSort : ISortingAlgorithm
    {
        public async Task Sort(int[] array, IUIUpdater uiUpdater)
        {
            uiUpdater.StartSort();
            int n = array.Length;
            bool swapped;

            do
            {
                if (uiUpdater.AbortSorting)
                {
                    uiUpdater.ClearFocus();
                    uiUpdater.ShuffleArray();
                    uiUpdater.DrawArray();
                    uiUpdater.ResetUI();
                    return;
                }

                swapped = false;

                for (int i = 0; i < n - 1; i++)
                {
                    uiUpdater.SetFocus(i + 1);

                    if (array[i] > array[i + 1])
                    {
                        int temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        swapped = true;
                    }

                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);
                }

                if (!swapped) break;

                for (int i = n - 2; i >= 0; i--)
                {
                    uiUpdater.SetFocus(i);

                    if (array[i] > array[i + 1])
                    {
                        int temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        swapped = true;
                    }

                    uiUpdater.DrawArray();
                    await uiUpdater.Delay(10);
                }
            } while (swapped);

            uiUpdater.ClearFocus();
            uiUpdater.DrawArray();
            uiUpdater.ResetUI();
        }
    }
}