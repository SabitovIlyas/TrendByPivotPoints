using System.Diagnostics;
using System.Text;

namespace ProjectsManager
{
    public class ConsoleProject
    {
        public string Name { get; init; }
        public string CsprojPath { get; init; }
        public string AssemblyName { get; init; }
        public string Description { get; init; }
        public bool IsSdkStyle { get; init; }
        public int DefaultEncodingIndex { get; init; }

        public string Args = string.Empty;
        public int EncodingIndex = -1;

        public readonly StringBuilder Output = new();
        public readonly StringBuilder Pending = new();
        public readonly object SyncRoot = new();

        /// <summary>Текущие значения прогона оптимизатора, разобранные из вывода.
        /// У остальных проектов остаётся пустым.</summary>
        public readonly OptimizatorStatus Status = new();

        public Process Process;
        public Task StdoutTask;
        public Task StderrTask;
        public bool IsBuilding;
        public ListViewItem Item;

        public bool IsBusy
        {
            get
            {
                var process = Process;
                if (process == null)
                    return false;
                try
                {
                    return !process.HasExited;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
