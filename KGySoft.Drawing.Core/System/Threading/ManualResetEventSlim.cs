#if NET35

// ReSharper disable once CheckNamespace
namespace System.Threading
{
    internal sealed class ManualResetEventSlim : IDisposable
    {
        #region Fields

        private readonly object lockObject = new object();

        private bool isDisposed;

        #endregion

        #region Properties

        internal bool IsSet { get; private set; }

        #endregion

        #region Constructors

        internal ManualResetEventSlim(bool initialState) => IsSet = initialState;

        #endregion

        #region Methods

        #region Public Methods

        public void Dispose()
        {
            lock (lockObject)
            {
                if (isDisposed)
                    return;
                isDisposed = true;
                DoSignal();
                Monitor.PulseAll(lockObject);
            }
        }

        #endregion

        #region Internal Methods

        internal void Set()
        {
            lock (lockObject)
            {
                if (isDisposed)
                    return;
                DoSignal();
                Monitor.PulseAll(lockObject);
            }
        }

        internal void Wait()
        {
            lock (lockObject)
            {
                if (isDisposed)
                    return;
                while (!IsSet)
                    Monitor.Wait(lockObject);
            }
        }

        #endregion

        #region Private Methods

        private void DoSignal()
        {
            // this must be called in lock
            IsSet = true;
        }

        #endregion

        #endregion
    }

}
#endif