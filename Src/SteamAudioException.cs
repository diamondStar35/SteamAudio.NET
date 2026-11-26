namespace SteamAudio;
/// <summary>A Steam Audio operation failed. The native status is preserved for diagnostics.</summary>
public sealed class SteamAudioException : Exception
{
    /// <summary>The status returned by the SDK.</summary>
    public Interop.Error Status { get; }
    /// <summary>Creates a failure with its operation and original SDK status.</summary>
    /// <param name="operation">Operation that failed.</param><param name="status">Native status.</param>
    public SteamAudioException(string operation, Interop.Error status) : base($"{operation}: Steam Audio returned {status}.") => Status = status;
    internal static void Check(Interop.Error status, string operation) { if (status != Interop.Error.Success) throw new SteamAudioException(operation, status); }
}
