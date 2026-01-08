using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Windows;

internal unsafe class D3D11ResourceFactory : IResourceFactory
{
    private readonly ComPtr<ID3D11Device> _device;
    private readonly ComPtr<ID3D11DeviceContext> _context;
    private readonly D3D11 _d3d11;

    public D3D11ResourceFactory(ComPtr<ID3D11Device> device, ComPtr<ID3D11DeviceContext> context, D3D11 d3d11)
    {
        _device = device;
        _context = context;
        _d3d11 = d3d11;
    }

    public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
    {
        var sizeInBytes = (uint)(vertices.Length * Marshal.SizeOf<VertexPositionNormalColor>());
        
        fixed (VertexPositionNormalColor* dataPtr = vertices)
        {
            var bufferDesc = new BufferDesc
            {
                ByteWidth = sizeInBytes,
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0
            };

            var subresourceData = new SubresourceData
            {
                PSysMem = dataPtr
            };

            ComPtr<ID3D11Buffer> buffer = default;
            fixed (ID3D11Buffer** bufferPtr = &buffer.Handle)
            {
                var result = _device.Handle->CreateBuffer(in bufferDesc, in subresourceData, bufferPtr);
                if (result != 0)
                    throw new Exception($"Failed to create vertex buffer. HRESULT: 0x{result:X8}");
            }

            return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
        }
    }

    public IBuffer CreateVertexBuffer(uint sizeInBytes)
    {
        var bufferDesc = new BufferDesc
        {
            ByteWidth = sizeInBytes,
            Usage = Usage.Dynamic,
            BindFlags = (uint)BindFlag.VertexBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0
        };

        ComPtr<ID3D11Buffer> buffer = default;
        fixed (ID3D11Buffer** bufferPtr = &buffer.Handle)
        {
            var result = _device.Handle->CreateBuffer(in bufferDesc, null, bufferPtr);
            if (result != 0)
                throw new Exception($"Failed to create vertex buffer. HRESULT: 0x{result:X8}");
        }

        return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
    }

    public IBuffer CreateIndexBuffer(uint[] indices)
    {
        var sizeInBytes = (uint)(indices.Length * sizeof(uint));
        
        fixed (uint* dataPtr = indices)
        {
            var bufferDesc = new BufferDesc
            {
     

    public IBuffer CreateIndexBuffer(uint sizeInBytes)
    {
        var bufferDesc = new BufferDesc
        {
            ByteWidth = sizeInBytes,
            Usage = Usage.Dynamic,
            BindFlags = (uint)BindFlag.IndexBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0
        };

        ComPtr<ID3D11Buffer> buffer = default;
        fixed (ID3D11Buffer** bufferPtr = &buffer.Handle)
        {
            var result = _device.Handle->CreateBuffer(in bufferDesc, null, bufferPtr);
            if (result != 0)
                throw new Exception($"Failed to create index buffer. HRESULT: 0x{result:X8}");
        }

        return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
    }           ByteWidth = sizeInBytes,
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0
            };

            var subresourceData = new SubresourceData
            {
                PSysMem = dataPtr
            };

            ComPtr<ID3D11Buffer> buffer = default;
            fixed (ID3D11Buffer** bufferPtr = &buffer.Handle)
            {
                var result = _device.Handle->CreateBuffer(in bufferDesc, in subresourceData, bufferPtr);
                if (result != 0)
                    throw new Exception($"Failed to create index buffer. HRESULT: 0x{result:X8}");
            }

            return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
        }
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        // D3D11 constant buffers must be 16-byte aligned
        sizeInBytes = (sizeInBytes + 15) & ~15u;
        
        var bufferDesc = new BufferDesc
        {
            ByteWidth = sizeInBytes,
            Usage = Usage.Dynamic,
            BindFlags = (uint)BindFlag.ConstantBuffer,
            Pipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        // 简化的Pipeline创建 - 返回一个占位符
        Console.WriteLine($"[D3D11] CreatePipeline called with vertexSize={vertexSize}, hasNormals={hasNormals}, hasColors={hasColors}");
        return new D3D11Pipeline(); // 需要创建这个类
    }

    public ICPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0
        };

        ComPtr<ID3D11Buffer> buffer = default;
        fixed (ID3D11Buffer** bufferPtr = &buffer.Handle)
        {
            var result = _device.Handle->CreateBuffer(in bufferDesc, null, bufferPtr);
            if (result != 0)
                throw new Exception($"Failed to create uniform buffer. HRESULT: 0x{result:X8}");
        }

        return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        // 这个方法需要完整的 Pipeline 创建逻辑，包括 Input Layout、Rasterizer State 等
        // 暂时返回一个简单的实现
        throw new NotImplementedException("Pipeline creation will be implemented in full backend");
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        throw new NotImplementedException("Shader creation will be implemented in full backend");
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        throw new NotImplementedException("ResourceSet creation will be implemented in full backend");
    }
}
