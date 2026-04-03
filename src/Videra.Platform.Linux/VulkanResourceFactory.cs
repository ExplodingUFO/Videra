using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using CorePrimitiveTopology = Videra.Core.Graphics.Abstractions.PrimitiveTopology;

namespace Videra.Platform.Linux;

internal unsafe class VulkanResourceFactory : IResourceFactory
{
    private static readonly DepthBufferConfiguration DepthConfig = DepthBufferConfiguration.Default;

    private readonly Device _device;
    private readonly PhysicalDevice _physicalDevice;
    private readonly Vk _vk;
    private readonly RenderPass _renderPass;

    public VulkanResourceFactory(Device device, PhysicalDevice physicalDevice, Vk vk, RenderPass renderPass)
    {
        _device = device;
        _physicalDevice = physicalDevice;
        _vk = vk;
        _renderPass = renderPass;
    }

    public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
    {
        var sizeInBytes = (uint)(vertices.Length * Marshal.SizeOf<VertexPositionNormalColor>());
        var buffer = CreateBuffer(sizeInBytes, BufferUsageFlags.VertexBufferBit);
        buffer.SetData(vertices, 0);
        return buffer;
    }

    public IBuffer CreateVertexBuffer(uint sizeInBytes)
    {
        return CreateBuffer(sizeInBytes, BufferUsageFlags.VertexBufferBit);
    }

    public IBuffer CreateIndexBuffer(uint[] indices)
    {
        var sizeInBytes = (uint)(indices.Length * sizeof(uint));
        var buffer = CreateBuffer(sizeInBytes, BufferUsageFlags.IndexBufferBit);
        buffer.SetData(indices, 0);
        return buffer;
    }

    public IBuffer CreateIndexBuffer(uint sizeInBytes)
    {
        return CreateBuffer(sizeInBytes, BufferUsageFlags.IndexBufferBit);
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        return CreateBuffer(sizeInBytes, BufferUsageFlags.UniformBufferBit);
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        return CreatePipeline(VertexPositionNormalColor.SizeInBytes, true, true);
    }

    public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
    {
        var vertexShaderBytes = CompileShader(GetVertexShaderSource(), ShaderKind.GlslVertexShader);
        var fragmentShaderBytes = CompileShader(GetFragmentShaderSource(), ShaderKind.GlslFragmentShader);

        var vertexModule = CreateShaderModule(vertexShaderBytes);
        var fragmentModule = CreateShaderModule(fragmentShaderBytes);

        var shaderStages = stackalloc PipelineShaderStageCreateInfo[2];
        shaderStages[0] = new PipelineShaderStageCreateInfo
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.VertexBit,
            Module = vertexModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };
        shaderStages[1] = new PipelineShaderStageCreateInfo
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = fragmentModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };

        var bindingDescription = new VertexInputBindingDescription
        {
            Binding = 0,
            Stride = vertexSize,
            InputRate = VertexInputRate.Vertex
        };

        var attributeDescriptions = stackalloc VertexInputAttributeDescription[3];
        attributeDescriptions[0] = new VertexInputAttributeDescription
        {
            Location = 0,
            Binding = 0,
            Format = Format.R32G32B32Sfloat,
            Offset = 0
        };
        attributeDescriptions[1] = new VertexInputAttributeDescription
        {
            Location = 1,
            Binding = 0,
            Format = Format.R32G32B32Sfloat,
            Offset = 12
        };
        attributeDescriptions[2] = new VertexInputAttributeDescription
        {
            Location = 2,
            Binding = 0,
            Format = Format.R32G32B32A32Sfloat,
            Offset = 24
        };

        var vertexInputInfo = new PipelineVertexInputStateCreateInfo
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            VertexBindingDescriptionCount = 1,
            PVertexBindingDescriptions = &bindingDescription,
            VertexAttributeDescriptionCount = 3,
            PVertexAttributeDescriptions = attributeDescriptions
        };

        var inputAssemblyTriangle = new PipelineInputAssemblyStateCreateInfo
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = Silk.NET.Vulkan.PrimitiveTopology.TriangleList,
            PrimitiveRestartEnable = false
        };

        var inputAssemblyLine = new PipelineInputAssemblyStateCreateInfo
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = Silk.NET.Vulkan.PrimitiveTopology.LineList,
            PrimitiveRestartEnable = false
        };

        var inputAssemblyPoint = new PipelineInputAssemblyStateCreateInfo
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = Silk.NET.Vulkan.PrimitiveTopology.PointList,
            PrimitiveRestartEnable = false
        };

        var viewportState = new PipelineViewportStateCreateInfo
        {
            SType = StructureType.PipelineViewportStateCreateInfo,
            ViewportCount = 1,
            ScissorCount = 1
        };

        var rasterizer = new PipelineRasterizationStateCreateInfo
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            DepthClampEnable = false,
            RasterizerDiscardEnable = false,
            PolygonMode = PolygonMode.Fill,
            LineWidth = 1.0f,
            CullMode = CullModeFlags.None,
            FrontFace = FrontFace.CounterClockwise,
            DepthBiasEnable = false
        };

        var multisampling = new PipelineMultisampleStateCreateInfo
        {
            SType = StructureType.PipelineMultisampleStateCreateInfo,
            SampleShadingEnable = false,
            RasterizationSamples = SampleCountFlags.Count1Bit
        };

        var colorBlendAttachment = new PipelineColorBlendAttachmentState
        {
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            BlendEnable = false
        };

        var colorBlending = new PipelineColorBlendStateCreateInfo
        {
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            LogicOpEnable = false,
            AttachmentCount = 1,
            PAttachments = &colorBlendAttachment
        };

        var depthStencil = new PipelineDepthStencilStateCreateInfo
        {
            SType = StructureType.PipelineDepthStencilStateCreateInfo,
            DepthTestEnable = true,
            DepthWriteEnable = true,
            DepthCompareOp = MapDepthComparison(DepthConfig.DepthComparison),
            DepthBoundsTestEnable = false,
            StencilTestEnable = false
        };

        var dynamicStates = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
        var dynamicState = new PipelineDynamicStateCreateInfo
        {
            SType = StructureType.PipelineDynamicStateCreateInfo,
            DynamicStateCount = 2,
            PDynamicStates = dynamicStates
        };

        var bindings = stackalloc DescriptorSetLayoutBinding[2];
        bindings[0] = new DescriptorSetLayoutBinding
        {
            Binding = 0,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.VertexBit
        };
        bindings[1] = new DescriptorSetLayoutBinding
        {
            Binding = 1,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.VertexBit
        };

        var descriptorSetLayoutInfo = new DescriptorSetLayoutCreateInfo
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 2,
            PBindings = bindings
        };

        DescriptorSetLayout descriptorSetLayout;
        if (_vk.CreateDescriptorSetLayout(_device, in descriptorSetLayoutInfo, null, out descriptorSetLayout) != Result.Success)
            throw new PipelineCreationException(
                "Failed to create descriptor set layout.",
                "CreatePipeline");

        var pipelineLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 1,
            PSetLayouts = &descriptorSetLayout
        };

        PipelineLayout pipelineLayout;
        if (_vk.CreatePipelineLayout(_device, in pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            throw new PipelineCreationException(
                "Failed to create pipeline layout.",
                "CreatePipeline");

        var pipelineInfo = new GraphicsPipelineCreateInfo
        {
            SType = StructureType.GraphicsPipelineCreateInfo,
            StageCount = 2,
            PStages = shaderStages,
            PVertexInputState = &vertexInputInfo,
            PViewportState = &viewportState,
            PRasterizationState = &rasterizer,
            PMultisampleState = &multisampling,
            PDepthStencilState = &depthStencil,
            PColorBlendState = &colorBlending,
            PDynamicState = &dynamicState,
            Layout = pipelineLayout,
            RenderPass = _renderPass,
            Subpass = 0
        };

        Pipeline trianglePipeline;
        pipelineInfo.PInputAssemblyState = &inputAssemblyTriangle;
        if (_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out trianglePipeline) != Result.Success)
            throw new PipelineCreationException(
                "Failed to create triangle pipeline.",
                "CreatePipeline");

        Pipeline linePipeline;
        pipelineInfo.PInputAssemblyState = &inputAssemblyLine;
        if (_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out linePipeline) != Result.Success)
            throw new PipelineCreationException(
                "Failed to create line pipeline.",
                "CreatePipeline");

        Pipeline pointPipeline;
        pipelineInfo.PInputAssemblyState = &inputAssemblyPoint;
        if (_vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out pointPipeline) != Result.Success)
            throw new PipelineCreationException(
                "Failed to create point pipeline.",
                "CreatePipeline");

        var poolSizes = stackalloc DescriptorPoolSize[1];
        poolSizes[0] = new DescriptorPoolSize(DescriptorType.UniformBuffer, 2);

        var descriptorPoolInfo = new DescriptorPoolCreateInfo
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            PoolSizeCount = 1,
            PPoolSizes = poolSizes,
            MaxSets = 1
        };

        DescriptorPool descriptorPool;
        if (_vk.CreateDescriptorPool(_device, in descriptorPoolInfo, null, out descriptorPool) != Result.Success)
            throw new PipelineCreationException(
                "Failed to create descriptor pool.",
                "CreatePipeline");

        var allocateInfo = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = descriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &descriptorSetLayout
        };

        DescriptorSet descriptorSet;
        if (_vk.AllocateDescriptorSets(_device, in allocateInfo, out descriptorSet) != Result.Success)
            throw new PipelineCreationException(
                "Failed to allocate descriptor set.",
                "CreatePipeline");

        SilkMarshal.Free((nint)shaderStages[0].PName);
        SilkMarshal.Free((nint)shaderStages[1].PName);

        _vk.DestroyShaderModule(_device, vertexModule, null);
        _vk.DestroyShaderModule(_device, fragmentModule, null);

        return new VulkanPipeline(_vk, _device, trianglePipeline, linePipeline, pointPipeline, pipelineLayout, descriptorSetLayout, descriptorPool, descriptorSet);
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        var module = CreateShaderModule(bytecode);
        return new VulkanShader(_vk, _device, module);
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        throw new UnsupportedOperationException(
            "Resource set creation is handled internally for the Vulkan backend. Use pipeline-level resource binding instead.",
            "CreateResourceSet",
            "Linux");
    }

    private static CompareOp MapDepthComparison(DepthComparisonFunction comparison)
    {
        return comparison switch
        {
            DepthComparisonFunction.Never => CompareOp.Never,
            DepthComparisonFunction.Less => CompareOp.Less,
            DepthComparisonFunction.Equal => CompareOp.Equal,
            DepthComparisonFunction.LessEqual => CompareOp.LessOrEqual,
            DepthComparisonFunction.Greater => CompareOp.Greater,
            DepthComparisonFunction.NotEqual => CompareOp.NotEqual,
            DepthComparisonFunction.GreaterEqual => CompareOp.GreaterOrEqual,
            _ => CompareOp.Always
        };
    }

    private VulkanBuffer CreateBuffer(uint sizeInBytes, BufferUsageFlags usage)
    {
        var bufferInfo = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = sizeInBytes,
            Usage = usage,
            SharingMode = SharingMode.Exclusive
        };

        VkBuffer buffer;
        if (_vk.CreateBuffer(_device, in bufferInfo, null, out buffer) != Result.Success)
            throw new ResourceCreationException(
                "Failed to create buffer.",
                "CreateBuffer");

        MemoryRequirements memRequirements;
        _vk.GetBufferMemoryRequirements(_device, buffer, out memRequirements);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
        };

        DeviceMemory bufferMemory;
        if (_vk.AllocateMemory(_device, in allocInfo, null, out bufferMemory) != Result.Success)
            throw new ResourceCreationException(
                "Failed to allocate buffer memory.",
                "CreateBuffer");

        var bindResult = _vk.BindBufferMemory(_device, buffer, bufferMemory, 0);
        if (bindResult != Result.Success)
        {
            _vk.DestroyBuffer(_device, buffer, null);
            _vk.FreeMemory(_device, bufferMemory, null);
            throw new ResourceCreationException(
                $"Failed to bind Vulkan buffer memory. Result: {bindResult}.",
                "CreateBuffer");
        }

        return new VulkanBuffer(_vk, _device, buffer, bufferMemory, sizeInBytes);
    }

    private uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
    {
        PhysicalDeviceMemoryProperties memProperties;
        _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out memProperties);

        for (uint i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1u << (int)i)) != 0 && (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
                return i;
        }

        throw new ResourceCreationException(
            "Failed to find suitable memory type.",
            "FindMemoryType");
    }

    private ShaderModule CreateShaderModule(byte[] bytecode)
    {
        fixed (byte* codePtr = bytecode)
        {
            var createInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)bytecode.Length,
                PCode = (uint*)codePtr
            };

            ShaderModule shaderModule;
            if (_vk.CreateShaderModule(_device, in createInfo, null, out shaderModule) != Result.Success)
                throw new PipelineCreationException(
                    "Failed to create shader module.",
                    "CreateShaderModule");

            return shaderModule;
        }
    }

    private byte[] CompileShader(string source, ShaderKind kind)
    {
        var shaderc = Shaderc.GetApi();
        var compiler = shaderc.CompilerInitialize();
        var options = shaderc.CompileOptionsInitialize();

        var result = shaderc.CompileIntoSpv(compiler, source, (nuint)source.Length, kind, "shader.glsl", "main", options);
        var status = shaderc.ResultGetCompilationStatus(result);
        if (status != CompilationStatus.Success)
        {
            var message = shaderc.ResultGetErrorMessageS(result);
            shaderc.ResultRelease(result);
            shaderc.CompileOptionsRelease(options);
            shaderc.CompilerRelease(compiler);
            throw new PipelineCreationException(
                $"Vulkan shader compilation failed: {message}",
                "CompileShader");
        }

        var length = (int)shaderc.ResultGetLength(result);
        var bytes = new byte[length];
        var dataPtr = shaderc.ResultGetBytes(result);
        Marshal.Copy((IntPtr)dataPtr, bytes, 0, length);

        shaderc.ResultRelease(result);
        shaderc.CompileOptionsRelease(options);
        shaderc.CompilerRelease(compiler);

        return bytes;
    }

    private string GetVertexShaderSource()
    {
        return @"#version 450
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inNormal;
layout(location = 2) in vec4 inColor;

layout(set = 0, binding = 0) uniform CameraBuffer
{
    mat4 viewMatrix;
    mat4 projectionMatrix;
} camera;

layout(set = 0, binding = 1) uniform WorldBuffer
{
    mat4 worldMatrix;
} world;

layout(location = 0) out vec4 fragColor;

void main()
{
    vec4 worldPos = world.worldMatrix * vec4(inPosition, 1.0);
    vec4 viewPos = camera.viewMatrix * worldPos;
    gl_Position = camera.projectionMatrix * viewPos;
    fragColor = inColor;
}
";
    }

    private string GetFragmentShaderSource()
    {
        return @"#version 450
layout(location = 0) in vec4 fragColor;
layout(location = 0) out vec4 outColor;

void main()
{
    outColor = fragColor;
}
";
    }
}
