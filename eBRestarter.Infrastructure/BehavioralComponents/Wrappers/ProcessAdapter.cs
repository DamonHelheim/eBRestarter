using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

/// <summary>
/// Infrastructure Wrapper: Encapsulates <see cref="Process"/> instance into an <see cref="IProcess"/> abstraction.
/// </summary>
/// <param name="process">The underlying <see cref="Process"/> instance to wrap.</param>
public sealed class ProcessAdapter(
    Process process)
    : IProcess
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 4: Complex Types & Collections (alphabetical) ──
    private readonly Process _process = process ?? throw new ArgumentNullException(nameof(process));


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    /// <inheritdoc />
    public string ProcessName => _process.ProcessName;

    // ── Block 4: Complex Types & Collections (alphabetical) ──
    /// <inheritdoc />
    public IntPtr MainWindowHandle => _process.MainWindowHandle;

    /// <inheritdoc />
    public StreamReader StandardError => _process.StandardError;

    /// <inheritdoc />
    public StreamReader StandardOutput => _process.StandardOutput;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public void Dispose() => _process.Dispose();

    /// <inheritdoc />
    public void WaitForExit() => _process.WaitForExit();

    /// <inheritdoc />
    public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

    /// <inheritdoc />
    public Task WaitForExitAsync() => _process.WaitForExitAsync();
}
