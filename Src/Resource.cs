using System.Runtime.InteropServices;
namespace SteamAudio;

/// <summary>Owns one native reference. Dependencies remain alive until the final in-flight operation releases its lease.</summary>
/// <remarks>Independent instances may run concurrently. Do not mutate the same native object concurrently.</remarks>
public abstract class Resource : IDisposable
{
    private readonly OwnedHandle _handle;
    private int _disposed;
    /// <summary>Whether this managed owner has been disposed.</summary>
    public bool IsDisposed => Volatile.Read(ref _disposed) != 0;
    /// <summary>Creates an owner and retains each managed dependency for the native object's lifetime.</summary>
    /// <param name="value">Owned native reference.</param>
    /// <param name="release">Exactly-once native release operation.</param>
    /// <param name="dependencies">Native parents retained until release.</param>
    private protected Resource(nint value, Action<nint> release, params Resource[] dependencies)
    {
        if (value == 0) throw new InvalidOperationException("Native creation returned a null resource.");
        try { _handle = new OwnedHandle(value, release, dependencies); }
        catch { release(value); throw; }
    }
    internal Lease Acquire() { Geometry.RayCallbacks.ThrowIfInside(); ObjectDisposedException.ThrowIf(IsDisposed, this); return new(_handle); }
    /// <summary>Returns a borrowed address for advanced interop. It is invalid after the owning lifetime ends.</summary>
    /// <returns>The native address. This method does not retain it.</returns>
    public nint DangerousGetHandle() { ObjectDisposedException.ThrowIf(IsDisposed, this); return _handle.DangerousGetHandle(); }
    /// <summary>Releases this reference after active operations and retained child dependencies finish. Repeated calls are harmless.</summary>
    public void Dispose() { Geometry.RayCallbacks.ThrowIfInside(); if (Interlocked.Exchange(ref _disposed, 1) == 0) _handle.Dispose(); }
    internal readonly ref struct Lease
    {
        private readonly OwnedHandle _owner;
        internal nint Value => _owner.DangerousGetHandle();
        internal Lease(OwnedHandle owner) { var success = false; owner.DangerousAddRef(ref success); _owner = owner; }
        public void Dispose() => _owner.DangerousRelease();
    }
    internal sealed class OwnedHandle : SafeHandle
    {
        private readonly Action<nint> _release;
        private readonly List<OwnedHandle> _parents = [];
        internal OwnedHandle(nint value, Action<nint> release, Resource[] parents) : base(0, true)
        {
            _release = release;
            try
            {
                foreach (var parent in parents)
                {
                    ObjectDisposedException.ThrowIf(parent.IsDisposed, parent);
                    var added = false; parent._handle.DangerousAddRef(ref added);
                    if (added) _parents.Add(parent._handle);
                }
                SetHandle(value);
            }
            catch { foreach (var parent in _parents) parent.DangerousRelease(); throw; }
        }
        public override bool IsInvalid => handle == 0;
        protected override bool ReleaseHandle()
        {
            try { _release(handle); }
            finally { foreach (var parent in _parents) parent.DangerousRelease(); }
            return true;
        }
    }
}
