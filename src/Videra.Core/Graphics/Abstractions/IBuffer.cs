namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// GPU 缓冲区接口
/// </summary>
public interface IBuffer : IDisposable
{
    uint SizeInBytes { get; }
    
    /// <summary>
    /// 更新缓冲区数据
    /// </summary>
    void Update<T>(T data) where T : unmanaged;
    
    /// <summary>
    /// 更新缓冲区数据（数组）
    /// </summary>
    void UpdateArray<T>(T[] data) where T : unmanaged;
    
    /// <summary>
    /// 设置缓冲区数据（带偏移）
    /// </summary>
    void SetData<T>(T data, uint offset) where T : unmanaged;
    
    /// <summary>
    /// 设置缓冲区数据数组（带偏移）
    /// </summary>
    void SetData<T>(T[] data, uint offset) where T : unmanaged;
}
