using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Utility;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) verifying Authenticode signatures via the
/// Windows trust provider (<c>WinVerifyTrust</c>).
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Verifies Authenticode signatures of binary executables using native OS cryptography in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortExecutableSignatureVerifier"/>.<br/>
/// </para>
/// </summary>
/// <remarks>
/// Security: Uses WinVerifyTrust instead of X509Certificate.CreateFromSignedFile to validate certificate chain, file hash integrity, and revocation status (WTD_REVOKE_WHOLECHAIN).
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed partial class AdapterWindowsAuthenticodeVerifier : IOutboundPortExecutableSignatureVerifier
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitives & strings ──
    private const string SignatureRejectedLogMessage = "Signature verification failed for {Path} (WinVerifyTrust returned 0x{Status}). The file is unsigned, tampered with, or its certificate chain is not trusted.";
    private const string VerificationErrorLogMessage = "Unexpected error while verifying the code signature of {Path}.";
    private const string WinTrustDllName = "wintrust.dll";
    private const string WinVerifyTrustEntryPoint = "WinVerifyTrust";

    // WINTRUST_DATA / WINTRUST_FILE_INFO flag values (wintrust.h)
    private const uint RevokeWholeChain = 0x00000001;   // WTD_REVOKE_WHOLECHAIN
    private const uint StateActionClose = 0x00000002;   // WTD_STATEACTION_CLOSE
    private const uint StateActionVerify = 0x00000001;  // WTD_STATEACTION_VERIFY
    private const uint TrustProviderFlags = 0x00000100; // WTD_SAFER_FLAG
    private const uint UiChoiceNone = 0x00000002;       // WTD_UI_NONE
    private const uint UiContextExecute = 0x00000000;   // WTD_UICONTEXT_EXECUTE
    private const uint UnionChoiceFile = 0x00000001;    // WTD_CHOICE_FILE
    private const int VerificationSucceeded = 0;        // S_OK / ERROR_SUCCESS

    // ── Block 4: Complex types & collections ──
    /// <summary>WINTRUST_ACTION_GENERIC_VERIFY_V2 – the standard Authenticode policy provider.</summary>
    private static readonly Guid GenericVerifyV2ActionId = new("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");

    /// <summary>Passing INVALID_HANDLE_VALUE as the owner window suppresses any interactive trust dialog.</summary>
    private static readonly IntPtr NoInteractiveWindowHandle = new(-1);


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected dependencies ──
    private readonly ILogger<AdapterWindowsAuthenticodeVerifier> _logger;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterWindowsAuthenticodeVerifier"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public AdapterWindowsAuthenticodeVerifier(ILogger<AdapterWindowsAuthenticodeVerifier> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Verifies whether the specified binary executable carries a trusted Authenticode code signature.
    /// </summary>
    /// <param name="filePath">Target executable file path.</param>
    /// <inheritdoc />
    public bool IsTrustedPublisher(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            int verificationStatus = InvokeWinVerifyTrust(filePath);

            if (verificationStatus == VerificationSucceeded)
            {
                return true;
            }

            _logger.LogError(LogEventIds.Security.SignatureVerificationRejected, SignatureRejectedLogMessage, filePath, verificationStatus.ToString("X8"));

            return false;
        }
        catch (Exception exception)
        {
            // Fail Secure: If verification fails or throws an exception, treat the binary as untrusted.
            _logger.LogError(LogEventIds.Security.SignatureVerificationErrored, exception, VerificationErrorLogMessage, filePath);

            return false;
        }
    }

    /// <summary>
    /// Invokes native WinVerifyTrust Win32 API to evaluate binary Authenticode signature state.
    /// </summary>
    /// <param name="filePath">Target executable file path.</param>
    private static int InvokeWinVerifyTrust(string filePath)
    {
        IntPtr filePathBuffer = IntPtr.Zero;
        IntPtr fileInfoBuffer = IntPtr.Zero;
        IntPtr trustDataBuffer = IntPtr.Zero;

        var actionId = GenericVerifyV2ActionId;

        try
        {
            filePathBuffer = Marshal.StringToCoTaskMemUni(filePath);

            var fileInfo = new WinTrustFileInfo
            {
                StructSize = (uint)Marshal.SizeOf<WinTrustFileInfo>(),
                FilePath = filePathBuffer,
                FileHandle = IntPtr.Zero,
                KnownSubject = IntPtr.Zero
            };

            fileInfoBuffer = Marshal.AllocCoTaskMem(Marshal.SizeOf<WinTrustFileInfo>());
            Marshal.StructureToPtr(fileInfo, fileInfoBuffer, fDeleteOld: false);

            var trustData = new WinTrustData
            {
                StructSize = (uint)Marshal.SizeOf<WinTrustData>(),
                PolicyCallbackData = IntPtr.Zero,
                SipClientData = IntPtr.Zero,
                UIChoice = UiChoiceNone,
                RevocationChecks = RevokeWholeChain,
                UnionChoice = UnionChoiceFile,
                FileInfoPtr = fileInfoBuffer,
                StateAction = StateActionVerify,
                StateData = IntPtr.Zero,
                UrlReference = IntPtr.Zero,
                ProviderFlags = TrustProviderFlags,
                UIContext = UiContextExecute,
                SignatureSettings = IntPtr.Zero
            };

            trustDataBuffer = Marshal.AllocCoTaskMem(Marshal.SizeOf<WinTrustData>());
            Marshal.StructureToPtr(trustData, trustDataBuffer, fDeleteOld: false);

            int verificationStatus = WinVerifyTrust(NoInteractiveWindowHandle, ref actionId, trustDataBuffer);

            // The trust provider allocated state during the VERIFY call; a second call with
            // WTD_STATEACTION_CLOSE is mandatory to release it (documented WinVerifyTrust contract).
            var stateToClose = Marshal.PtrToStructure<WinTrustData>(trustDataBuffer);
            stateToClose.StateAction = StateActionClose;
            Marshal.StructureToPtr(stateToClose, trustDataBuffer, fDeleteOld: false);

            _ = WinVerifyTrust(NoInteractiveWindowHandle, ref actionId, trustDataBuffer);

            return verificationStatus;
        }
        finally
        {
            if (trustDataBuffer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(trustDataBuffer);
            }

            if (fileInfoBuffer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(fileInfoBuffer);
            }

            if (filePathBuffer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(filePathBuffer);
            }
        }
    }

    [LibraryImport(WinTrustDllName, EntryPoint = WinVerifyTrustEntryPoint)]
    private static partial int WinVerifyTrust(IntPtr hwnd, ref Guid actionId, IntPtr trustData);


    // ═══════════════════════════════════════════════════════
    //  9. Nested Types
    // ═══════════════════════════════════════════════════════
    [StructLayout(LayoutKind.Sequential)]
    private struct WinTrustFileInfo
    {
        public uint StructSize;
        public IntPtr FilePath;
        public IntPtr FileHandle;
        public IntPtr KnownSubject;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WinTrustData
    {
        public uint StructSize;
        public IntPtr PolicyCallbackData;
        public IntPtr SipClientData;
        public uint UIChoice;
        public uint RevocationChecks;
        public uint UnionChoice;
        public IntPtr FileInfoPtr;
        public uint StateAction;
        public IntPtr StateData;
        public IntPtr UrlReference;
        public uint ProviderFlags;
        public uint UIContext;
        public IntPtr SignatureSettings;
    }
}
