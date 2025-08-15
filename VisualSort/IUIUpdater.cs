using System.Collections.Generic;
using System.Threading.Tasks;

namespace VisualSort.Helpers
{
    public interface IUIUpdater
    {
        void StartSort();
        void ResetUI();
        void DrawArray();
        void ShuffleArray();

        void SetFocus(params int[] indices);
        void ClearFocus();

        bool AbortSorting { get; }
        Task Delay(int milliseconds);
    }
}