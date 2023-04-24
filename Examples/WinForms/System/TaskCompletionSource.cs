#if NETFRAMEWORK
// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    internal class TaskCompletionSource : TaskCompletionSource<bool>
    {
        internal void SetResult() => SetResult(default);
    }
}
#endif