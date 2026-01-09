using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.DXGI;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Windows;

internal unsafe class D3D11ResourceFactory : IResourceFactory
{
    private readonly ComPtr<ID3D11Device> _device;
    private readonly ComPtr<ID3D11DeviceContext> _context;
    private readonly D3D11 _d3d11;
    private readonly D3DCompiler _compiler;

    public D3D11ResourceFactory(ComPtr<ID3D11Device> device, ComPtr<ID3D11DeviceContext> context, D3D11 d3d11)
    {
        _device = device;
        _context = context;
        _d3d11 = d3d11;
        _compiler = D3DCompiler.GetApi();
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
            var bufferPtr = &buffer.Handle;
            var result = _device.Handle->CreateBuffer(in bufferDesc, in subresourceData, bufferPtr);
            if (result != 0)
                throw new Exception($"Failed to create vertex buffer. HRESULT: 0x{result:X8}");

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
        var bufferPtr = &buffer.Handle;
        var result = _device.Handle->CreateBuffer(in bufferDesc, null, bufferPtr);
        if (result != 0)
            throw new Exception($"Failed to create vertex buffer. HRESULT: 0x{result:X8}");

        return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
    }

    public IBuffer CreateIndexBuffer(uint[] indices)
    {
        var sizeInBytes = (uint)(indices.Length * sizeof(uint));

        fixed (uint* dataPtr = indices)
        {
            var bufferDesc = new BufferDesc
            {
                ByteWidth = sizeInBytes,
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
            var bufferPtr = &buffer.Handle;
            var result = _device.Handle->CreateBuffer(in bufferDesc, in subresourceData, bufferPtr);
            if (result != 0)
                throw new Exception($"Failed to create index buffer. HRESULT: 0x{result:X8}");

            return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
        }
    }

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
        var bufferPtr = &buffer.Handle;
        var result = _device.Handle->CreateBuffer(in bufferDesc, null, bufferPtr);
        if (result != 0)
            throw new Exception($"Failed to create index buffer. HRESULT: 0x{result:X8}");

        return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        sizeInBytes = (sizeInBytes + 15) & ~15u;

        var bufferDesc = new BufferDesc
        {
            ByteWidth = sizeInBytes,
            Usage = Usage.Dynamic,
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0
        };

        ComPtr<ID3D11Buffer> buffer = default;
        var bufferPtr = &buffer.Handle;
        var result = _device.Handle->CreateBuffer(in bufferDesc, null, bufferPtr);
        if (result != 0)
            throw new Exception($"Failed to create uniform buffer. HRESULT: 0x{result:X8}");

        return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        return CreatePipeline(VertexPositionNormalColor.SizeInBytes, true, true);
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        var hlsl = GetShaderSource();
        var vertexShader = CompileShader(hlsl, "main_vs", "vs_5_0");
        var pixelShader = CompileShader(hlsl, "main_ps", "ps_5_0");

        var vsBytecodePtr = vertexShader.GetBufferPointer();
        var vsBytecodeSize = (nuint)vertexShader.GetBufferSize();
        ComPtr<ID3D11VertexShader> vs = default;
        var vsPtr = &vs.Handle;
        {
            var result = _device.Handle->CreateVertexShader(vsBytecodePtr, vsBytecodeSize, null, vsPtr);
            if (result != 0)
                throw new Exception($"Failed to create vertex shader. HRESULT: 0x{result:X8}");
        }

        var psBytecodePtr = pixelShader.GetBufferPointer();
        var psBytecodeSize = (nuint)pixelShader.GetBufferSize();
        ComPtr<ID3D11PixelShader> ps = default;
        var psPtr = &ps.Handle;
        {
            var result = _device.Handle->CreatePixelShader(psBytecodePtr, psBytecodeSize, null, psPtr);
            if (result != 0)
                throw new Exception($"Failed to create pixel shader. HRESULT: 0x{result:X8}");
        }

        var positionPtr = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.UTF8);
        var normalPtr = SilkMarshal.StringToPtr("NORMAL", NativeStringEncoding.UTF8);
        var colorPtr = SilkMarshal.StringToPtr("COLOR", NativeStringEncoding.UTF8);
        ComPtr<ID3D11InputLayout> inputLayout = default;
        try
        {
            var inputElements = stackalloc InputElementDesc[3];
            inputElements[0] = new InputElementDesc
            {
                SemanticName = (byte*)positionPtr,
                SemanticIndex = 0,
                Format = Format.FormatR32G32B32Float,
                InputSlot = 0,
                AlignedByteOffset = 0,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0
            };
            inputElements[1] = new InputElementDesc
            {
                SemanticName = (byte*)normalPtr,
                SemanticIndex = 0,
                Format = Format.FormatR32G32B32Float,
                InputSlot = 0,
                AlignedByteOffset = 12,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0
            };
            inputElements[2] = new InputElementDesc
            {
                SemanticName = (byte*)colorPtr,
                SemanticIndex = 0,
                Format = Format.FormatR32G32B32A32Float,
                InputSlot = 0,
                AlignedByteOffset = 24,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0
            };

            var layoutPtr = &inputLayout.Handle;
            var result = _device.Handle->CreateInputLayout(inputElements, 3, vsBytecodePtr, vsBytecodeSize, layoutPtr);
            if (result != 0)
                throw new Exception($"Failed to create input layout. HRESULT: 0x{result:X8}");
        }
        finally
        {
            SilkMarshal.Free(positionPtr);
            SilkMarshal.Free(normalPtr);
            SilkMarshal.Free(colorPtr);
        }

        var rasterDesc = new RasterizerDesc
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None,
            FrontCounterClockwise = 0,
            DepthBias = 0,
            DepthBiasClamp = 0,
            SlopeScaledDepthBias = 0,
            DepthClipEnable = 1,
            ScissorEnable = 1,
            MultisampleEnable = 0,
            AntialiasedLineEnable = 1
        };

        ComPtr<ID3D11RasterizerState> rasterizer = default;
        var rasterPtr = &rasterizer.Handle;
        {
            var result = _device.Handle->CreateRasterizerState(in rasterDesc, rasterPtr);
            if (result != 0)
                throw new Exception($"Failed to create rasterizer state. HRESULT: 0x{result:X8}");
        }

        vertexShader.Dispose();
        pixelShader.Dispose();

        return new D3D11Pipeline(vs, ps, inputLayout, rasterizer);
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        throw new NotImplementedException("Shader creation is handled internally for D3D11 backend.");
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        throw new NotImplementedException("ResourceSet creation will be implemented in full backend");
    }

    private ComPtr<ID3D10Blob> CompileShader(string source, string entryPoint, string profile)
    {
        ComPtr<ID3D10Blob> shaderBlob = default;
        ComPtr<ID3D10Blob> errorBlob = default;

        var sourceBytes = System.Text.Encoding.UTF8.GetBytes(source);
        var entryPointPtr = SilkMarshal.StringToPtr(entryPoint, NativeStringEncoding.UTF8);
        var profilePtr = SilkMarshal.StringToPtr(profile, NativeStringEncoding.UTF8);
        try
        {
            fixed (byte* sourcePtr = sourceBytes)
            {
                var shaderBlobPtr = &shaderBlob.Handle;
                var errorBlobPtr = &errorBlob.Handle;
                var result = _compiler.Compile(
                    sourcePtr,
                    (nuint)sourceBytes.Length,
                    (byte*)null,
                    null,
                    null,
                    (byte*)entryPointPtr,
                    (byte*)profilePtr,
                    0,
                    0,
                    shaderBlobPtr,
                    errorBlobPtr);
                if (result != 0)
                {
                    var errorMsg = errorBlob.Handle != null
                        ? Marshal.PtrToStringAnsi((IntPtr)errorBlob.Handle->GetBufferPointer())
                        : "Unknown shader compilation error";
                    errorBlob.Dispose();
                    throw new Exception($"D3D11 shader compilation failed ({entryPoint}/{profile}): {errorMsg}");
                }
            }
        }
        finally
        {
            SilkMarshal.Free(entryPointPtr);
            SilkMarshal.Free(profilePtr);
        }

        errorBlob.Dispose();

        return shaderBlob;
    }

    private string GetShaderSource()
    {
        return @"
cbuffer CameraBuffer : register(b1)
{
    row_major float4x4 viewMatrix;
    row_major float4x4 projectionMatrix;
};

cbuffer WorldBuffer : register(b2)
{
    row_major float4x4 worldMatrix;
};

struct VSInput
{
    float3 position : POSITION;
    float3 normal : NORMAL;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

VSOutput main_vs(VSInput input)
{
    VSOutput output;
    float4 worldPos = mul(float4(input.position, 1.0f), worldMatrix);
    float4 viewPos = mul(worldPos, viewMatrix);
    output.position = mul(viewPos, projectionMatrix);
    output.color = input.color;
    return output;
}

float4 main_ps(VSOutput input) : SV_TARGET
{
    return input.color;
}
";
    }
}
