using System.Threading.Tasks;

namespace ToyStorage
{
    /// <summary>
    /// Helper class used to encapsulate differences in <see cref="Task"/> between targeted frameworks.
    /// </summary>
    internal class TaskHelper
    {
#if NET45 || NET452
        public static readonly Task CompletedTask = Task.FromResult(true);
#else
        public static readonly Task CompletedTask = Task.CompletedTask;
#endif
    }
}
