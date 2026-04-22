using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.DXGI;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
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
                throw new ResourceCreationException(
                    $"Failed to create vertex buffer. HRESULT: 0x{result:X8}",
                    "CreateVertexBuffer");

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
            throw new ResourceCreationException(
                $"Failed to create vertex buffer. HRESULT: 0x{result:X8}",
                "CreateVertexBuffer");

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
                throw new ResourceCreationException(
                    $"Failed to create index buffer. HRESULT: 0x{result:X8}",
                    "CreateIndexBuffer");

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
            throw new ResourceCreationException(
                $"Failed to create index buffer. HRESULT: 0x{result:X8}",
                "CreateIndexBuffer");

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
            throw new ResourceCreationException(
                $"Failed to create uniform buffer. HRESULT: 0x{result:X8}",
                "CreateUniformBuffer");

        return new D3D11Buffer(buffer, sizeInBytes, _context, _d3d11);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        if (IsSurfaceChartScalarPipeline(description))
        {
            return CreatePipelineFromSource(GetSurfaceChartShaderSource(), alphaBlendEnabled: description.AlphaBlendEnabled);
        }

        return CreatePipelineFromSource(GetShaderSource(), alphaBlendEnabled: description.AlphaBlendEnabled);
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        return CreatePipelineFromSource(GetShaderSource(), alphaBlendEnabled: false);
    }

    private IPipeline CreatePipelineFromSource(string hlsl, bool alphaBlendEnabled)
    {
        var vertexShader = CompileShader(hlsl, "main_vs", "vs_5_0");
        var pixelShader = CompileShader(hlsl, "main_ps", "ps_5_0");

        var vsBytecodePtr = vertexShader.GetBufferPointer();
        var vsBytecodeSize = (nuint)vertexShader.GetBufferSize();
        ComPtr<ID3D11VertexShader> vs = default;
        var vsPtr = &vs.Handle;
        {
            var result = _device.Handle->CreateVertexShader(vsBytecodePtr, vsBytecodeSize, null, vsPtr);
            if (result != 0)
                throw new PipelineCreationException(
                    $"Failed to create vertex shader. HRESULT: 0x{result:X8}",
                    "CreatePipeline");
        }

        var psBytecodePtr = pixelShader.GetBufferPointer();
        var psBytecodeSize = (nuint)pixelShader.GetBufferSize();
        ComPtr<ID3D11PixelShader> ps = default;
        var psPtr = &ps.Handle;
        {
            var result = _device.Handle->CreatePixelShader(psBytecodePtr, psBytecodeSize, null, psPtr);
            if (result != 0)
                throw new PipelineCreationException(
                    $"Failed to create pixel shader. HRESULT: 0x{result:X8}",
                    "CreatePipeline");
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
                throw new PipelineCreationException(
                    $"Failed to create input layout. HRESULT: 0x{result:X8}",
                    "CreatePipeline");
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
                throw new PipelineCreationException(
                    $"Failed to create rasterizer state. HRESULT: 0x{result:X8}",
                    "CreatePipeline");
        }

        ComPtr<ID3D11BlendState> blendState = default;
        var blendPtr = &blendState.Handle;
        {
            var blendDesc = new BlendDesc
            {
                AlphaToCoverageEnable = 0,
                IndependentBlendEnable = 0
            };

            blendDesc.RenderTarget[0] = new RenderTargetBlendDesc
            {
                BlendEnable = alphaBlendEnabled ? 1u : 0u,
                SrcBlend = Blend.SrcAlpha,
                DestBlend = Blend.InvSrcAlpha,
                BlendOp = BlendOp.Add,
                SrcBlendAlpha = Blend.One,
                DestBlendAlpha = Blend.InvSrcAlpha,
                BlendOpAlpha = BlendOp.Add,
                RenderTargetWriteMask = (byte)ColorWriteEnable.All
            };

            var result = _device.Handle->CreateBlendState(in blendDesc, blendPtr);
            if (result != 0)
                throw new PipelineCreationException(
                    $"Failed to create blend state. HRESULT: 0x{result:X8}",
                    "CreatePipeline");
        }

        vertexShader.Dispose();
        pixelShader.Dispose();

        return new D3D11Pipeline(vs, ps, inputLayout, rasterizer, blendState);
    }

    private static bool IsSurfaceChartScalarPipeline(PipelineDescription description)
    {
        return description.ResourceLayouts is not null
            && description.ResourceLayouts.Any(static resourceLayout =>
                resourceLayout.Elements is not null
                && resourceLayout.Elements.Any(static element =>
                    element.Binding == RenderBindingSlots.SurfaceColorMap || element.Binding == RenderBindingSlots.SurfaceTileScalars));
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        throw new UnsupportedOperationException(
            "Shader creation is handled internally for the D3D11 backend. Use the pipeline creation methods instead.",
            "CreateShader",
            "Windows");
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        throw new UnsupportedOperationException(
            "ResourceSet creation is not yet supported on the D3D11 backend.",
            "CreateResourceSet",
            "Windows");
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
                    throw new PipelineCreationException(
                        $"D3D11 shader compilation failed ({entryPoint}/{profile}): {errorMsg}",
                        "CompileShader");
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

    private static string GetShaderSource()
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

cbuffer AlphaMaskBuffer : register(b6)
{
    float maskEnabled;
    float alphaCutoff;
    float2 _alphaMaskPad;
};

cbuffer StyleBuffer : register(b3)
{
    // 光照 (offset 0-28, padded to 32)
    float ambientIntensity;
    float diffuseIntensity;
    float specularIntensity;
    float specularPower;
    float3 lightDirection;
    float _pad0;

    // 色彩 (offset 32-56, padded to 64)
    float3 tintColor;
    float saturation;
    float contrast;
    float brightness;
    float2 _pad1;

    // 描边 (offset 64-88, padded to 96)
    float4 outlineColor;
    float outlineWidth;
    int outlineEnabled;
    float2 _pad2;

    // 材质 (offset 96-124, padded to 128)
    float opacity;
    int useVertexColor;
    float2 _pad3;
    float4 overrideColor;
    int wireframeMode;
    float3 _pad4;
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
    float3 worldNormal : NORMAL;
    float4 color : COLOR;
    float3 worldPos : TEXCOORD0;
    float maskEnabled : TEXCOORD1;
    float alphaCutoff : TEXCOORD2;
};

VSOutput main_vs(VSInput input)
{
    VSOutput output;
    float4 worldPos = mul(float4(input.position, 1.0f), worldMatrix);
    float4 viewPos = mul(worldPos, viewMatrix);
    output.position = mul(viewPos, projectionMatrix);
    output.worldNormal = normalize(mul(input.normal, (float3x3)worldMatrix));
    output.color = useVertexColor ? input.color : overrideColor;
    output.worldPos = worldPos.xyz;
    output.maskEnabled = maskEnabled;
    output.alphaCutoff = alphaCutoff;
    return output;
}

float4 main_ps(VSOutput input) : SV_TARGET
{
    float sourceAlpha = input.color.a;
    if (input.maskEnabled > 0.5f)
    {
        clip(sourceAlpha - input.alphaCutoff);
    }

    float3 normal = normalize(input.worldNormal);
    float3 lightDir = normalize(lightDirection);

    // 基础光照
    float ambient = ambientIntensity;
    float diffuse = max(dot(normal, lightDir), 0.0f) * diffuseIntensity;

    // 高光 (Blinn-Phong)
    float3 viewDir = normalize(-input.worldPos);
    float3 halfDir = normalize(lightDir + viewDir);
    float specular = pow(max(dot(normal, halfDir), 0.0f), specularPower) * specularIntensity;

    float lighting = ambient + diffuse + specular;

    // 应用光照
    float3 color = input.color.rgb * lighting;

    // 色调叠加
    color *= tintColor;

    // 饱和度调整
    float grey = dot(color, float3(0.299f, 0.587f, 0.114f));
    color = lerp(float3(grey, grey, grey), color, saturation);

    // 对比度调整
    color = (color - 0.5f) * contrast + 0.5f;

    // 亮度调整
    color += brightness;

    float finalAlpha = input.maskEnabled > 0.5f ? 1.0f : sourceAlpha * opacity;
    return float4(saturate(color), finalAlpha);
}
";
    }

    private static string GetSurfaceChartShaderSource()
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

cbuffer SurfaceColorMapBuffer : register(b4)
{
    float4 colorMapHeader;
    float4 colorMapPalette[256];
};

cbuffer SurfaceTileScalarBuffer : register(b5)
{
    float4 tileScalarGroups[4096];
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
    float3 worldNormal : NORMAL;
    float4 color : COLOR;
    float3 worldPos : TEXCOORD0;
};

float LoadTileScalar(uint vertexId)
{
    uint groupIndex = vertexId / 4;
    uint componentIndex = vertexId % 4;
    float4 group = tileScalarGroups[groupIndex];

    if (componentIndex == 0)
    {
        return group.x;
    }

    if (componentIndex == 1)
    {
        return group.y;
    }

    if (componentIndex == 2)
    {
        return group.z;
    }

    return group.w;
}

float4 MapSurfaceColor(float scalar)
{
    float minimum = colorMapHeader.x;
    float maximum = colorMapHeader.y;
    float paletteCount = colorMapHeader.z;
    float segmentCount = colorMapHeader.w;

    if (paletteCount <= 1.0f || maximum <= minimum)
    {
        return colorMapPalette[0];
    }

    if (scalar <= minimum)
    {
        return colorMapPalette[0];
    }

    uint lastIndex = (uint)(paletteCount - 1.0f);
    if (scalar >= maximum)
    {
        return colorMapPalette[lastIndex];
    }

    float normalized = saturate((scalar - minimum) / (maximum - minimum));
    float scaled = normalized * segmentCount;
    uint lowerIndex = (uint)floor(scaled);
    uint upperIndex = min(lowerIndex + 1, lastIndex);
    float fraction = scaled - lowerIndex;
    return lerp(colorMapPalette[lowerIndex], colorMapPalette[upperIndex], fraction);
}

VSOutput main_vs(VSInput input, uint vertexId : SV_VertexID)
{
    VSOutput output;
    float4 worldPos = mul(float4(input.position, 1.0f), worldMatrix);
    float4 viewPos = mul(worldPos, viewMatrix);
    output.position = mul(viewPos, projectionMatrix);
    output.worldNormal = normalize(mul(input.normal, (float3x3)worldMatrix));
    output.color = MapSurfaceColor(LoadTileScalar(vertexId));
    output.worldPos = worldPos.xyz;
    return output;
}

float4 main_ps(VSOutput input) : SV_TARGET
{
    float3 normal = normalize(input.worldNormal);
    float3 lightDir = normalize(float3(-0.45f, 0.75f, -0.45f));
    float ambient = 0.35f;
    float diffuse = max(dot(normal, lightDir), 0.0f) * 0.65f;
    float lighting = ambient + diffuse;
    float3 color = input.color.rgb * lighting;
    return float4(saturate(color), input.color.a);
}
";
    }
}
