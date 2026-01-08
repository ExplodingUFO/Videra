namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// 渲染管线接口
/// </summary>
public interface IPipeline : IDisposable
{
}

/// <summary>
/// Shader 接口
/// </summary>
public interface IShader : IDisposable
{
}

/// <summary>
/// Resource Layout 接口
/// </summary>
public interface IResourceLayout : IDisposable
{
}

/// <summary>
/// Resource Set 接口
/// </summary>
public interface IResourceSet : IDisposable
{
}
