namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Optional advanced seam for executors that cache buffer-bound descriptor or binding state.
/// Callers can notify the executor when a buffer is being retired so stale cache entries do not accumulate.
/// </summary>
public interface IBufferBindingCacheInvalidator
{
    void ReleaseBuffer(IBuffer buffer);
}
