using VisualSort.Helpers;

namespace VisualSort.Sorting
{
    public interface ISortingAlgorithm
    {
        Task Sort(int[] array, IUIUpdater uiUpdater);
    }
}