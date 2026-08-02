using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

/// <summary>
/// Infrastructure Wrapper: Encapsulates System.Diagnostics.Process instance into an IProcess abstraction.
/// </summary>
public sealed class ProcessAdapter(
    Process process)
    : IProcess
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 4: Komplexe Typen & Kollektionen (alphabetisch) ──
    private readonly Process _process = process ?? throw new ArgumentNullException(nameof(process));


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    public string ProcessName => _process.ProcessName;

    // ── Block 4: Komplexe Typen & Kollektionen (alphabetisch) ──
    public IntPtr MainWindowHandle => _process.MainWindowHandle;
    public StreamReader StandardError => _process.StandardError;
    public StreamReader StandardOutput => _process.StandardOutput;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public void Dispose() => _process.Dispose();

    public void WaitForExit() => _process.WaitForExit();

    public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

    public Task WaitForExitAsync() => _process.WaitForExitAsync();
}
