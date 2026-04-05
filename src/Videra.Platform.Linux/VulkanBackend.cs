using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Videra.Core.Exceptions;
using Videra.Core.Graphics.Abstractions;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace Videra.Platform.Linux;

/// <summary>
/// Linux Vulkan graphics backend implementation.
/// Provides hardware-accelerated rendering using the Vulkan API via Silk.NET bindings.
/// Requires a valid X11 window handle on Linux. The constructor accepts an
/// <see cref="ISurfaceCreator"/> (defaulting to <see cref="X11SurfaceCreator"/>) to abstract
/// platform-specific surface creation.
/// </summary>
public unsafe class VulkanBackend : IGraphicsBackend
{
    private static readonly DepthBufferConfiguration DepthConfig = DepthBufferConfiguration.Default;

    private Vk _vk;
    private Instance _instance;
    private PhysicalDevice _physicalDevice;
    private Device _device;
    private Queue _graphicsQueue;
    private Queue _presentQueue;
    
    private KhrSurface _khrSurface;
    private KhrSwapchain _khrSwapchain;
    private SurfaceKHR _surface;
    private SwapchainKHR _swapchain;
    
    private Image[] _swapchainImages;
    private ImageView[] _swapchainImageViews;
    private Framebuffer[] _framebuffers;
    
    private RenderPass _renderPass;
    private CommandPool _commandPool;
    private CommandBuffer[] _commandBuffers;
    
    private Image _depthImage;
    private DeviceMemory _depthImageMemory;
    private ImageView _depthImageView;
    
    private VkSemaphore _imageAvailableSemaphore;
    private VkSemaphore _renderFinishedSemaphore;
    private Fence _inFlightFence;
    
    private Vector4 _clearColor = new(0.1f, 0.1f, 0.15f, 1.0f);
    private int _width;
    private int _height;
    
    private uint _graphicsQueueFamily;
    private uint _presentQueueFamily;
    private uint _currentImageIndex;
    
    private VulkanResourceFactory _resourceFactory;
    private VulkanCommandExecutor _commandExecutor;
    private readonly ISurfaceCreator _surfaceCreator;
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether the backend has been successfully initialized
    /// and is ready for rendering operations.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Creates a VulkanBackend with the default X11 surface creator.
    /// </summary>
    public VulkanBackend() : this(new X11SurfaceCreator()) { }

    /// <summary>
    /// Creates a VulkanBackend with a specific surface creation strategy.
    /// Pass an <see cref="X11SurfaceCreator"/> for X11, or a future WaylandSurfaceCreator for Wayland.
    /// </summary>
    internal VulkanBackend(ISurfaceCreator surfaceCreator)
    {
        _surfaceCreator = surfaceCreator;
    }

    /// <summary>
    /// Initializes the Vulkan backend with the specified window handle and rendering dimensions.
    /// Creates the Vulkan instance, surface, selects a physical device, creates a logical device
    /// and queues, sets up the swap chain, image views, render pass, depth resources, framebuffers,
    /// command pool, command buffers, synchronization objects, resource factory, and command executor.
    /// </summary>
    /// <param name="windowHandle">
    /// A valid X11 window handle for the target surface. Must not be <see cref="IntPtr.Zero"/>.
    /// </param>
    /// <param name="width">The initial width of the rendering surface in pixels. Must be greater than zero.</param>
    /// <param name="height">The initial height of the rendering surface in pixels. Must be greater than zero.</param>
    /// <exception cref="PlatformDependencyException">
    /// Thrown when <paramref name="windowHandle"/> is <see cref="IntPtr.Zero"/> or
    /// when <paramref name="width"/> or <paramref name="height"/> is not positive.
    /// </exception>
    /// <exception cref="GraphicsInitializationException">
    /// Thrown when the Vulkan instance, logical device, or physical device selection fails.
    /// </exception>
    /// <exception cref="ResourceCreationException">
    /// Thrown when the swap chain, image views, render pass, depth resources, framebuffers,
    /// command pool, command buffers, or synchronization objects fail to be created.
    /// </exception>
    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        if (IsInitialized) return;

        if (windowHandle == IntPtr.Zero)
            throw new PlatformDependencyException(
                "A valid X11 window handle is required for Vulkan initialization.",
                "Initialize",
                "Linux");

        if (width <= 0 || height <= 0)
            throw new PlatformDependencyException(
                $"Invalid dimensions for Vulkan initialization: {width}x{height}. Both width and height must be positive.",
                "Initialize",
                "Linux");

        _width = width;
        _height = height;

        _vk = Vk.GetApi();

        try
        {
            // 创建 Vulkan Instance
            CreateInstance();

            // 创建 Surface (需要平台特定代码)
            CreateSurface(windowHandle);

            // 选择物理设备
            PickPhysicalDevice();

            // 创建逻辑设备
            CreateLogicalDevice();

            // 创建 Swapchain
            CreateSwapchain();

            // 创建 Image Views
            CreateImageViews();

            // 创建 Render Pass
            CreateRenderPass();

            // 创建深度资源
            CreateDepthResources();

            // 创建 Framebuffers
            CreateFramebuffers();

            // 创建 Command Pool
            CreateCommandPool();

            // 创建 Command Buffers
            CreateCommandBuffers();

            // 创建同步对象
            CreateSyncObjects();

            // 创建工厂和命令执行器
            _resourceFactory = new VulkanResourceFactory(_device, _physicalDevice, _vk, _renderPass);
            _commandExecutor = new VulkanCommandExecutor(_device, _commandBuffers[0], _vk);
        }
        catch
        {
            Dispose();
            throw;
        }

        IsInitialized = true;
    }

    private void CreateInstance()
    {
        var appName = Marshal.StringToHGlobalAnsi("Videra");
        var engineName = Marshal.StringToHGlobalAnsi("Videra Engine");
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)appName,
            ApplicationVersion = Vk.MakeVersion(1, 0, 0),
            PEngineName = (byte*)engineName,
            EngineVersion = Vk.MakeVersion(1, 0, 0),
            ApiVersion = Vk.Version12
        };

        var extSurface = Marshal.StringToHGlobalAnsi("VK_KHR_surface");
        var extPlatform = Marshal.StringToHGlobalAnsi(_surfaceCreator.RequiredExtensionName);
        var extensions = stackalloc IntPtr[]
        {
            extSurface,
            extPlatform
        };

        var createInfo = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = 2,
            PpEnabledExtensionNames = (byte**)extensions
        };

        try
        {
            fixed (Instance* instance = &_instance)
            {
                if (_vk.CreateInstance(in createInfo, null, instance) != Result.Success)
                    throw new GraphicsInitializationException(
                        "Failed to create Vulkan instance.",
                        "CreateInstance");
            }

            // 获取 KHR Surface 扩展
            _vk.TryGetInstanceExtension(_instance, out _khrSurface);
        }
        finally
        {
            Marshal.FreeHGlobal(appName);
            Marshal.FreeHGlobal(engineName);
            Marshal.FreeHGlobal(extSurface);
            Marshal.FreeHGlobal(extPlatform);
        }
    }

    private static Format MapDepthFormat(DepthBufferFormat format)
    {
        return format switch
        {
            DepthBufferFormat.Depth24UnormStencil8 => Format.D24UnormS8Uint,
            _ => Format.D32Sfloat
        };
    }

    private void CreateSurface(IntPtr windowHandle)
    {
        _surface = _surfaceCreator.CreateSurface(_vk, _instance, windowHandle);
    }

    private void PickPhysicalDevice()
    {
        uint deviceCount = 0;
        _vk.EnumeratePhysicalDevices(_instance, &deviceCount, null);
        
        if (deviceCount == 0)
            throw new GraphicsInitializationException(
                "Failed to find GPUs with Vulkan support.",
                "PickPhysicalDevice");

        var devices = stackalloc PhysicalDevice[(int)deviceCount];
        _vk.EnumeratePhysicalDevices(_instance, &deviceCount, devices);

        // 简单选择第一个设备
        _physicalDevice = devices[0];

        // 找到图形队列族
        FindQueueFamilies();
    }

    private void FindQueueFamilies()
    {
        uint queueFamilyCount = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, null);

        var queueFamilies = stackalloc QueueFamilyProperties[(int)queueFamilyCount];
        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, queueFamilies);

        for (uint i = 0; i < queueFamilyCount; i++)
        {
            if ((queueFamilies[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
            {
                _graphicsQueueFamily = i;
                
                // 检查是否支持 Present
                Silk.NET.Core.Bool32 presentSupport = false;
                _khrSurface.GetPhysicalDeviceSurfaceSupport(_physicalDevice, i, _surface, &presentSupport);
                
                if (presentSupport)
                {
                    _presentQueueFamily = i;
                    break;
                }
            }
        }
    }

    private void CreateLogicalDevice()
    {
        var queuePriority = 1.0f;

        var queueCreateInfo = new DeviceQueueCreateInfo
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = _graphicsQueueFamily,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };

        var deviceFeatures = new PhysicalDeviceFeatures();

        var extSwapchain = Marshal.StringToHGlobalAnsi("VK_KHR_swapchain");
        var extensions = stackalloc IntPtr[]
        {
            extSwapchain
        };

        var createInfo = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreateInfo,
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = 1,
            PpEnabledExtensionNames = (byte**)extensions
        };

        try
        {
            fixed (Device* device = &_device)
            {
                if (_vk.CreateDevice(_physicalDevice, in createInfo, null, device) != Result.Success)
                    throw new GraphicsInitializationException(
                        "Failed to create logical device.",
                        "CreateLogicalDevice");
            }

            // 获取队列
            fixed (Queue* queue = &_graphicsQueue)
            {
                _vk.GetDeviceQueue(_device, _graphicsQueueFamily, 0, queue);
            }

            fixed (Queue* queue = &_presentQueue)
            {
                _vk.GetDeviceQueue(_device, _presentQueueFamily, 0, queue);
            }

            // 获取 Swapchain 扩展
            _vk.TryGetDeviceExtension(_instance, _device, out _khrSwapchain);
        }
        finally
        {
            Marshal.FreeHGlobal(extSwapchain);
        }
    }

    private void CreateSwapchain()
    {
        // 查询 Swapchain 支持
        SurfaceCapabilitiesKHR capabilities;
        _khrSurface.GetPhysicalDeviceSurfaceCapabilities(_physicalDevice, _surface, &capabilities);

        // 选择 Surface Format
        uint formatCount;
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &formatCount, null);
        var formats = stackalloc SurfaceFormatKHR[(int)formatCount];
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &formatCount, formats);

        var surfaceFormat = formats[0]; // 简化：选择第一个

        // 选择 Present Mode
        uint presentModeCount;
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, &presentModeCount, null);
        var presentModes = stackalloc PresentModeKHR[(int)presentModeCount];
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, &presentModeCount, presentModes);

        var presentMode = PresentModeKHR.FifoKhr; // VSync

        // 创建 Swapchain
        var extent = new Extent2D((uint)_width, (uint)_height);
        
        var createInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,
            MinImageCount = capabilities.MinImageCount + 1,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true
        };

        fixed (SwapchainKHR* swapchain = &_swapchain)
        {
            if (_khrSwapchain.CreateSwapchain(_device, in createInfo, null, swapchain) != Result.Success)
                throw new ResourceCreationException(
                    "Failed to create swapchain.",
                    "CreateSwapchain");
        }

        // 获取 Swapchain Images
        uint imageCount = 0;
        _khrSwapchain.GetSwapchainImages(_device, _swapchain, &imageCount, null);
        _swapchainImages = new Image[imageCount];
        
        fixed (Image* images = _swapchainImages)
        {
            _khrSwapchain.GetSwapchainImages(_device, _swapchain, &imageCount, images);
        }
    }

    private void CreateImageViews()
    {
        _swapchainImageViews = new ImageView[_swapchainImages.Length];

        for (int i = 0; i < _swapchainImages.Length; i++)
        {
            var createInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _swapchainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = Format.B8G8R8A8Srgb,
                Components = new ComponentMapping
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                },
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            fixed (ImageView* imageView = &_swapchainImageViews[i])
            {
                if (_vk.CreateImageView(_device, in createInfo, null, imageView) != Result.Success)
                    throw new ResourceCreationException(
                        "Failed to create image view.",
                        "CreateImageViews");
            }
        }
    }

    private void CreateRenderPass()
    {
        // 颜色附件
        var colorAttachment = new AttachmentDescription
        {
            Format = Format.B8G8R8A8Srgb,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };

        var colorAttachmentRef = new AttachmentReference
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal
        };

        // 深度附件
        var depthAttachment = new AttachmentDescription
        {
            Format = MapDepthFormat(DepthConfig.DepthFormat),
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
        };

        var depthAttachmentRef = new AttachmentReference
        {
            Attachment = 1,
            Layout = ImageLayout.DepthStencilAttachmentOptimal
        };

        var subpass = new SubpassDescription
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
            PDepthStencilAttachment = &depthAttachmentRef
        };

        var attachments = stackalloc AttachmentDescription[] { colorAttachment, depthAttachment };

        var dependency = new SubpassDependency
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            SrcAccessMask = 0,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
        };

        var renderPassInfo = new RenderPassCreateInfo
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 2,
            PAttachments = attachments,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency
        };

        fixed (RenderPass* renderPass = &_renderPass)
        {
            if (_vk.CreateRenderPass(_device, in renderPassInfo, null, renderPass) != Result.Success)
                throw new ResourceCreationException(
                    "Failed to create render pass.",
                    "CreateRenderPass");
        }
    }

    private void CreateDepthResources()
    {
        var depthFormat = MapDepthFormat(DepthConfig.DepthFormat);

        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new Extent3D((uint)_width, (uint)_height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Format = depthFormat,
            Tiling = ImageTiling.Optimal,
            InitialLayout = ImageLayout.Undefined,
            Usage = ImageUsageFlags.DepthStencilAttachmentBit,
            SharingMode = SharingMode.Exclusive,
            Samples = SampleCountFlags.Count1Bit
        };

        fixed (Image* image = &_depthImage)
        {
            if (_vk.CreateImage(_device, in imageInfo, null, image) != Result.Success)
                throw new ResourceCreationException(
                    "Failed to create depth image.",
                    "CreateDepthResources");
        }

        MemoryRequirements memRequirements;
        _vk.GetImageMemoryRequirements(_device, _depthImage, out memRequirements);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
        };

        fixed (DeviceMemory* memory = &_depthImageMemory)
        {
            if (_vk.AllocateMemory(_device, in allocInfo, null, memory) != Result.Success)
                throw new ResourceCreationException(
                    "Failed to allocate depth memory.",
                    "CreateDepthResources");
        }

        _vk.BindImageMemory(_device, _depthImage, _depthImageMemory, 0);

        var viewInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = _depthImage,
            ViewType = ImageViewType.Type2D,
            Format = depthFormat,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.DepthBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        fixed (ImageView* view = &_depthImageView)
        {
            if (_vk.CreateImageView(_device, in viewInfo, null, view) != Result.Success)
                throw new ResourceCreationException(
                    "Failed to create depth image view.",
                    "CreateDepthResources");
        }
    }

    private void CreateFramebuffers()
    {
        _framebuffers = new Framebuffer[_swapchainImageViews.Length];

        for (int i = 0; i < _swapchainImageViews.Length; i++)
        {
            var attachments = stackalloc ImageView[] { _swapchainImageViews[i], _depthImageView };

            var framebufferInfo = new FramebufferCreateInfo
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = _renderPass,
                AttachmentCount = 2,
                PAttachments = attachments,
                Width = (uint)_width,
                Height = (uint)_height,
                Layers = 1
            };

            fixed (Framebuffer* framebuffer = &_framebuffers[i])
            {
                if (_vk.CreateFramebuffer(_device, in framebufferInfo, null, framebuffer) != Result.Success)
                    throw new ResourceCreationException(
                        "Failed to create framebuffer.",
                        "CreateFramebuffers");
            }
        }
    }

    private void CreateCommandPool()
    {
        var poolInfo = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = _graphicsQueueFamily
        };

        fixed (CommandPool* commandPool = &_commandPool)
        {
            if (_vk.CreateCommandPool(_device, in poolInfo, null, commandPool) != Result.Success)
                throw new ResourceCreationException(
                    "Failed to create command pool.",
                    "CreateCommandPool");
        }
    }

    private void CreateCommandBuffers()
    {
        _commandBuffers = new CommandBuffer[1];

        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        fixed (CommandBuffer* commandBuffers = _commandBuffers)
        {
            if (_vk.AllocateCommandBuffers(_device, in allocInfo, commandBuffers) != Result.Success)
                throw new ResourceCreationException(
                    "Failed to allocate command buffers.",
                    "CreateCommandBuffers");
        }
    }

    private void CreateSyncObjects()
    {
        var semaphoreInfo = new SemaphoreCreateInfo
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        var fenceInfo = new FenceCreateInfo
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        fixed (VkSemaphore* imageAvailable = &_imageAvailableSemaphore)
        fixed (VkSemaphore* renderFinished = &_renderFinishedSemaphore)
        fixed (Fence* inFlight = &_inFlightFence)
        {
            if (_vk.CreateSemaphore(_device, in semaphoreInfo, null, imageAvailable) != Result.Success ||
                _vk.CreateSemaphore(_device, in semaphoreInfo, null, renderFinished) != Result.Success ||
                _vk.CreateFence(_device, in fenceInfo, null, inFlight) != Result.Success)
            {
                throw new ResourceCreationException(
                    "Failed to create sync objects.",
                    "CreateSyncObjects");
            }
        }
    }

    /// <summary>
    /// Resizes the swap chain and recreates dependent resources (image views, depth resources,
    /// framebuffers) to match the new dimensions. Waits for the device to become idle before
    /// performing the resize. If either dimension is not positive, the call is silently ignored.
    /// </summary>
    /// <param name="width">The new width of the rendering surface in pixels.</param>
    /// <param name="height">The new height of the rendering surface in pixels.</param>
    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        _width = width;
        _height = height;

        // 等待设备空闲
        _vk.DeviceWaitIdle(_device);

        // 重新创建 Swapchain
        CleanupSwapchain();
        CreateSwapchain();
        CreateImageViews();
        CleanupDepthResources();
        CreateDepthResources();
        CreateFramebuffers();
    }

    private void CleanupSwapchain()
    {
        if (_framebuffers is not null)
        {
            foreach (var framebuffer in _framebuffers)
                _vk.DestroyFramebuffer(_device, framebuffer, null);
        }

        if (_swapchainImageViews is not null)
        {
            foreach (var imageView in _swapchainImageViews)
                _vk.DestroyImageView(_device, imageView, null);
        }

        if (_swapchain.Handle != 0)
            _khrSwapchain.DestroySwapchain(_device, _swapchain, null);
    }

    private void CleanupDepthResources()
    {
        if (_depthImageView.Handle != 0)
            _vk.DestroyImageView(_device, _depthImageView, null);
        if (_depthImage.Handle != 0)
            _vk.DestroyImage(_device, _depthImage, null);
        if (_depthImageMemory.Handle != 0)
            _vk.FreeMemory(_device, _depthImageMemory, null);
    }

    /// <summary>
    /// Begins a new rendering frame. Waits for the previous frame's fence, acquires the next swap chain image,
    /// resets the command buffer, begins command recording, starts the render pass with the current clear color,
    /// and sets the viewport and scissor rect to the current dimensions.
    /// </summary>
    public void BeginFrame()
    {
        // 等待上一帧完成
        ThrowIfFailed(
            _vk.WaitForFences(_device, 1, in _inFlightFence, true, ulong.MaxValue),
            "BeginFrame",
            "Failed while waiting for the previous Vulkan frame fence.");
        ThrowIfFailed(
            _vk.ResetFences(_device, 1, in _inFlightFence),
            "BeginFrame",
            "Failed to reset the Vulkan in-flight fence.");

        // 获取下一个图像
        Result acquireResult;
        fixed (uint* imageIndex = &_currentImageIndex)
        {
            acquireResult = _khrSwapchain.AcquireNextImage(_device, _swapchain, ulong.MaxValue, _imageAvailableSemaphore, default, imageIndex);
        }
        if (acquireResult == Result.ErrorOutOfDateKhr || acquireResult == Result.SuboptimalKhr)
        {
            Resize(_width, _height);
            throw new GraphicsInitializationException(
                $"Vulkan swapchain became invalid during AcquireNextImage. Result: {acquireResult}.",
                "BeginFrame");
        }

        ThrowIfFailed(acquireResult, "BeginFrame", "Failed to acquire the next Vulkan swapchain image.");

        // 开始记录命令
        ThrowIfFailed(
            _vk.ResetCommandBuffer(_commandBuffers[0], 0),
            "BeginFrame",
            "Failed to reset the Vulkan command buffer.");

        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo
        };

        ThrowIfFailed(
            _vk.BeginCommandBuffer(_commandBuffers[0], in beginInfo),
            "BeginFrame",
            "Failed to begin recording the Vulkan command buffer.");

        // 开始 Render Pass
        var clearValues = stackalloc ClearValue[]
        {
            new ClearValue { Color = new ClearColorValue(_clearColor.X, _clearColor.Y, _clearColor.Z, _clearColor.W) },
            new ClearValue { DepthStencil = new ClearDepthStencilValue(DepthConfig.ClearDepthValue, (uint)DepthConfig.ClearStencilValue) }
        };

        var renderPassInfo = new RenderPassBeginInfo
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass,
            Framebuffer = _framebuffers[_currentImageIndex],
            RenderArea = new Rect2D
            {
                Offset = new Offset2D(0, 0),
                Extent = new Extent2D((uint)_width, (uint)_height)
            },
            ClearValueCount = 2,
            PClearValues = clearValues
        };

        _vk.CmdBeginRenderPass(_commandBuffers[0], in renderPassInfo, SubpassContents.Inline);

        // 设置 Viewport 和 Scissor
        var viewport = new Viewport
        {
            X = 0,
            Y = 0,
            Width = _width,
            Height = _height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f
        };
        _vk.CmdSetViewport(_commandBuffers[0], 0, 1, in viewport);

        var scissor = new Rect2D
        {
            Offset = new Offset2D(0, 0),
            Extent = new Extent2D((uint)_width, (uint)_height)
        };
        _vk.CmdSetScissor(_commandBuffers[0], 0, 1, in scissor);
    }

    /// <summary>
    /// Ends the current rendering frame by finishing the render pass, ending command buffer recording,
    /// submitting the command buffer to the graphics queue with proper semaphore synchronization,
    /// and presenting the swap chain image to the presentation queue.
    /// </summary>
    public void EndFrame()
    {
        // 结束 Render Pass
        _vk.CmdEndRenderPass(_commandBuffers[0]);

        // 结束命令记录
        ThrowIfFailed(
            _vk.EndCommandBuffer(_commandBuffers[0]),
            "EndFrame",
            "Failed to end Vulkan command buffer recording.");

        // 提交命令
        var waitStages = stackalloc PipelineStageFlags[] { PipelineStageFlags.ColorAttachmentOutputBit };
        var commandBuffer = _commandBuffers[0];
        var imageAvailSem = _imageAvailableSemaphore;
        var renderFinishSem = _renderFinishedSemaphore;

        var submitInfo = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &imageAvailSem,
            PWaitDstStageMask = waitStages,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer,
            SignalSemaphoreCount = 1,
            PSignalSemaphores = &renderFinishSem
        };

        ThrowIfFailed(
            _vk.QueueSubmit(_graphicsQueue, 1, in submitInfo, _inFlightFence),
            "EndFrame",
            "Failed to submit Vulkan work to the graphics queue.");

        // 呈现
        var swapchain = _swapchain;
        var imageIndex = _currentImageIndex;
        var presentInfo = new PresentInfoKHR
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &renderFinishSem,
            SwapchainCount = 1,
            PSwapchains = &swapchain,
            PImageIndices = &imageIndex
        };

        var presentResult = _khrSwapchain.QueuePresent(_presentQueue, in presentInfo);
        if (presentResult == Result.ErrorOutOfDateKhr || presentResult == Result.SuboptimalKhr)
        {
            Resize(_width, _height);
            return;
        }

        ThrowIfFailed(presentResult, "EndFrame", "Failed to present the Vulkan swapchain image.");
    }

    /// <summary>
    /// Sets the color used to clear the render target at the beginning of each frame.
    /// </summary>
    /// <param name="color">The clear color as a <see cref="Vector4"/> with RGBA components in the range [0, 1].</param>
    public void SetClearColor(Vector4 color)
    {
        _clearColor = color;
    }

    /// <summary>
    /// Gets the Vulkan resource factory used to create GPU resources such as buffers, textures, and shaders.
    /// </summary>
    /// <returns>The <see cref="IResourceFactory"/> implementation for this backend.</returns>
    public IResourceFactory GetResourceFactory() => _resourceFactory;

    /// <summary>
    /// Gets the Vulkan command executor used to issue rendering commands to the GPU.
    /// </summary>
    /// <returns>The <see cref="ICommandExecutor"/> implementation for this backend.</returns>
    public ICommandExecutor GetCommandExecutor() => _commandExecutor;

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

    /// <summary>
    /// Releases all Vulkan resources including synchronization objects, command pool, swap chain,
    /// render pass, depth resources, logical device, surface, instance, and the underlying Vulkan API.
    /// Waits for the device to become idle before destroying resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        IsInitialized = false;

        if (_device.Handle != 0)
            _vk.DeviceWaitIdle(_device);

        _vk.DestroySemaphore(_device, _imageAvailableSemaphore, null);
        _vk.DestroySemaphore(_device, _renderFinishedSemaphore, null);
        _vk.DestroyFence(_device, _inFlightFence, null);

        _vk.DestroyCommandPool(_device, _commandPool, null);

        CleanupSwapchain();

        _vk.DestroyRenderPass(_device, _renderPass, null);
        CleanupDepthResources();

        _vk.DestroyDevice(_device, null);
        _khrSurface.DestroySurface(_instance, _surface, null);
        _vk.DestroyInstance(_instance, null);

        _surfaceCreator.Cleanup();

        _vk?.Dispose();
        GC.SuppressFinalize(this);
    }

    private static void ThrowIfFailed(Result result, string operation, string message)
    {
        if (result != Result.Success)
            throw new GraphicsInitializationException(
                $"{message} Result: {result}.",
                operation);
    }
}
