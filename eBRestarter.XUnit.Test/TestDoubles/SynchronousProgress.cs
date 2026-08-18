using System;

namespace eBRestarter.Tests.TestDoubles
{
    /// <summary>
    /// Reports progress updates immediately and synchronously on the invoking thread.
    /// </summary>
    /// <remarks>
    /// Standard <see cref="Progress{T}"/> posts callbacks via <see cref="System.Threading.SynchronizationContext"/>
    /// or the thread pool, making delivery asynchronous by design. In a test environment without a synchronization
    /// context, callbacks execute on background threads and may arrive after assertions have completed. This test
    /// double eliminates timing dependencies and potential test flakiness by invoking callbacks synchronously.
    /// </remarks>
    /// <typeparam name="T">The type of the progress update payload.</typeparam>
    /// <param name="onReport">A callback invoked synchronously whenever a progress report is emitted.</param>
    internal sealed class SynchronousProgress<T>(Action<T> onReport) : IProgress<T>
    {
        private readonly Action<T> _onReport = onReport ?? throw new ArgumentNullException(nameof(onReport));

        /// <inheritdoc />
        public void Report(T value) => _onReport(value);
    }
}
