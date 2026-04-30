using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
using Videra.SurfaceCharts.Rendering.Software;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public sealed class SurfaceChartGpuRenderBackendTests
{
    [Fact]
    public void HandleBoundInputs_WithAvailableGpuBackend_SelectsGpu()
    {
        var graphicsBackend = new FakeGraphicsBackend();
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));

        host.UpdateInputs(CreateInputs(handleBound: true));

        host.Snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.Snapshot.UsesNativeSurface.Should().BeTrue();
        host.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.RenderingStatus.UsesNativeSurface.Should().BeTrue();
        host.SoftwareScene.Should().BeNull();
        graphicsBackend.InitializeCallCount.Should().Be(1);
        graphicsBackend.BeginFrameCallCount.Should().Be(1);
    }

    [Fact]
    public void HandleBoundInputs_WithSameShapeTiles_ReusesSharedIndexBuffer()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 8, height: 8);
        var leftTile = CreateTile(metadata, new SurfaceTileKey(1, 1, 0, 0), tileValue: 12f);
        var rightTile = CreateTile(metadata, new SurfaceTileKey(1, 1, 1, 0), tileValue: 24f);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [leftTile, rightTile],
            ColorMap = CreateColorMap(metadata),
        });

        resourceFactory.IndexBufferCreationCount.Should().Be(1);
        resourceFactory.VertexBufferCreationCount.Should().Be(2);
    }

    [Fact]
    public void ColorMapChanges_UpdatePaletteUniformWithoutRewritingResidentVertexBuffers()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 18f);
        var initialColorMap = CreateColorMap(metadata);
        var replacementColorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF804020u, 0xFF20C0F0u));

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = initialColorMap,
        });

        resourceFactory.VertexBufferCreationCount.Should().Be(1);
        resourceFactory.IndexBufferCreationCount.Should().Be(1);
        resourceFactory.VertexBufferUpdateCount.Should().Be(0);
        resourceFactory.UniformBuffers.Should().Contain(buffer => buffer.SizeInBytes > 32);
        var paletteBuffer = resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 4112);
        paletteBuffer.LatestRawData.Should().NotBeNull();
        var initialPaletteUpdateCount = paletteBuffer.UpdateCount;
        var initialPaletteBytes = paletteBuffer.LatestRawData!.ToArray();

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = replacementColorMap,
        });

        resourceFactory.VertexBufferCreationCount.Should().Be(1);
        resourceFactory.IndexBufferCreationCount.Should().Be(1);
        resourceFactory.VertexBufferUpdateCount.Should().Be(0);
        paletteBuffer.UpdateCount.Should().Be(initialPaletteUpdateCount + 1);
        paletteBuffer.LatestRawData.Should().NotBeNull();
        paletteBuffer.LatestRawData.Should().NotEqual(initialPaletteBytes);
        graphicsBackend.CommandExecutor.BoundVertexBufferSlots.Should().Contain(RenderBindingSlots.SurfaceColorMap);
        graphicsBackend.CommandExecutor.BoundVertexBufferSlots.Should().Contain(RenderBindingSlots.SurfaceTileScalars);
    }

    [Fact]
    public void IndependentColorField_DrivesTileScalarUniformAndPaletteRecolor()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 2, height: 2);
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 2, 2),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 10f, 20f, 30f, 40f },
                range: new SurfaceValueRange(0d, 100d)),
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[] { 40f, 30f, 20f, 10f },
                range: new SurfaceValueRange(0d, 100d)));
        var initialColorMap = CreateColorMap(metadata);
        var replacementColorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF804020u, 0xFF20C0F0u));

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = initialColorMap,
        });

        resourceFactory.VertexBuffers.Should().ContainSingle();
        resourceFactory.VertexBuffers[0].LatestVertexData.Should().NotBeNull();
        var scalarBuffer = resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 65536);
        scalarBuffer.LatestRawData.Should().NotBeNull();
        ReadScalarPayloadValue(scalarBuffer.LatestRawData!, 0).Should().Be(40f);
        ReadScalarPayloadValue(scalarBuffer.LatestRawData!, 1).Should().Be(30f);
        ReadScalarPayloadValue(scalarBuffer.LatestRawData!, 2).Should().Be(20f);
        ReadScalarPayloadValue(scalarBuffer.LatestRawData!, 3).Should().Be(10f);

        var paletteBuffer = resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 4112);
        paletteBuffer.LatestRawData.Should().NotBeNull();
        var initialPaletteBytes = paletteBuffer.LatestRawData!.ToArray();
        var initialPaletteUpdateCount = paletteBuffer.UpdateCount;
        var initialScalarBytes = scalarBuffer.LatestRawData!.ToArray();

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = replacementColorMap,
        });

        resourceFactory.VertexBuffers.Should().ContainSingle();
        resourceFactory.VertexBuffers[0].LatestVertexData.Should().NotBeNull();
        resourceFactory.VertexBufferUpdateCount.Should().Be(0);
        scalarBuffer.LatestRawData.Should().NotBeNull();
        scalarBuffer.LatestRawData.Should().Equal(initialScalarBytes);
        scalarBuffer.UpdateCount.Should().Be(1);
        paletteBuffer.UpdateCount.Should().Be(initialPaletteUpdateCount + 1);
        paletteBuffer.LatestRawData.Should().NotBeNull();
        paletteBuffer.LatestRawData.Should().NotEqual(initialPaletteBytes);
        graphicsBackend.CommandExecutor.BoundVertexBufferSlots.Should().Contain(RenderBindingSlots.SurfaceColorMap);
        graphicsBackend.CommandExecutor.BoundVertexBufferSlots.Should().Contain(RenderBindingSlots.SurfaceTileScalars);
    }

    [Fact]
    public void TileScalarUniforms_ReserveShaderDeclaredBufferRange()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 2, height: 2);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 25f);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = CreateColorMap(metadata),
        });

        resourceFactory.UniformBuffers.Should().Contain(buffer => buffer.SizeInBytes == 65536);
        var scalarBuffer = resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 65536);
        scalarBuffer.LatestRawData.Should().NotBeNull();
        scalarBuffer.LatestRawData!.Length.Should().Be(16);
    }

    [Fact]
    public void LargePalettes_AreResampledIntoGpuLutInsteadOfThrowing()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 42f);
        var oversizedPalette = Enumerable.Range(0, 300)
            .Select(index => 0xFF000000u | ((uint)index << 8) | (uint)(299 - index))
            .ToArray();
        var colorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(oversizedPalette));

        var act = () => host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = colorMap,
        });

        act.Should().NotThrow();
        var paletteBuffer = resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 4112);
        paletteBuffer.SizeInBytes.Should().Be(4112);
        paletteBuffer.LatestRawData.Should().NotBeNull();
    }

    [Fact]
    public void SlopedSurface_UsesDerivedNormalsInsteadOfUnitYPlaceholders()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 3, height: 3);
        var tile = CreateTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 3, 3),
            3,
            3,
            new float[]
            {
                0f, 1f, 2f,
                1f, 2f, 3f,
                2f, 3f, 4f,
            },
            metadata.ValueRange);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = CreateColorMap(metadata),
            ViewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, 3d, 3d)),
        });

        var expectedNormal = Vector3.Normalize(new Vector3(-1f, 1f, -1f));
        var centerNormal = resourceFactory.VertexBuffers.Single().LatestVertexData![4].Normal;
        centerNormal.Should().NotBe(Vector3.UnitY);
        centerNormal.X.Should().BeApproximately(expectedNormal.X, 0.0001f);
        centerNormal.Y.Should().BeApproximately(expectedNormal.Y, 0.0001f);
        centerNormal.Z.Should().BeApproximately(expectedNormal.Z, 0.0001f);
    }

    [Fact]
    public void AdjacentTiles_WithNewNeighborArrival_RebuildBoundaryNormalsUsingCrossTileGradients()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 4, height: 3);
        var leftTile = CreateTile(
            new SurfaceTileKey(1, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 2, 3),
            2,
            3,
            new float[]
            {
                0f, 1f,
                1f, 2f,
                2f, 3f,
            },
            metadata.ValueRange);
        var rightTile = CreateTile(
            new SurfaceTileKey(1, 0, 1, 0),
            new SurfaceTileBounds(2, 0, 2, 3),
            2,
            3,
            new float[]
            {
                4f, 9f,
                5f, 10f,
                6f, 11f,
            },
            metadata.ValueRange);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [leftTile],
            ColorMap = CreateColorMap(metadata),
            ViewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, 4d, 3d)),
        });

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [leftTile, rightTile],
            ColorMap = CreateColorMap(metadata),
            ViewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, 4d, 3d)),
        });

        var expectedLeftBoundaryNormal = Vector3.Normalize(new Vector3(-2f, 1f, -1f));
        var expectedRightBoundaryNormal = Vector3.Normalize(new Vector3(-4f, 1f, -1f));
        var leftVertices = resourceFactory.VertexBuffers.Last(buffer => MathF.Abs(buffer.LatestVertexData![0].Position.X) < 0.0001f).LatestVertexData!;
        var rightVertices = resourceFactory.VertexBuffers.Last(buffer => MathF.Abs(buffer.LatestVertexData![0].Position.X - 2f) < 0.0001f).LatestVertexData!;

        for (var row = 0; row < 3; row++)
        {
            var leftBoundary = leftVertices[(row * 2) + 1].Normal;
            var rightBoundary = rightVertices[row * 2].Normal;

            leftBoundary.X.Should().BeApproximately(expectedLeftBoundaryNormal.X, 0.0001f);
            leftBoundary.Y.Should().BeApproximately(expectedLeftBoundaryNormal.Y, 0.0001f);
            leftBoundary.Z.Should().BeApproximately(expectedLeftBoundaryNormal.Z, 0.0001f);
            rightBoundary.X.Should().BeApproximately(expectedRightBoundaryNormal.X, 0.0001f);
            rightBoundary.Y.Should().BeApproximately(expectedRightBoundaryNormal.Y, 0.0001f);
            rightBoundary.Z.Should().BeApproximately(expectedRightBoundaryNormal.Z, 0.0001f);
        }
    }

    [Fact]
    public void GpuInitializationFailure_ReportsGpuNotReadyWithoutSoftwareFallback()
    {
        var graphicsBackend = new FakeGraphicsBackend(
            initializeException: new InvalidOperationException("gpu init failed"));
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));

        host.UpdateInputs(CreateInputs(handleBound: true));

        host.Snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.Snapshot.IsReady.Should().BeFalse();
        host.Snapshot.IsFallback.Should().BeFalse();
        host.Snapshot.UsesNativeSurface.Should().BeTrue();
        host.Snapshot.FallbackReason.Should().Contain("gpu init failed");
        host.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.RenderingStatus.IsFallback.Should().BeFalse();
        host.SoftwareScene.Should().BeNull();
    }

    [Fact]
    public void GpuFramePreparationFailure_ReportsGpuNotReadyDiagnostic()
    {
        var graphicsBackend = new FakeGraphicsBackend(
            beginFrameException: new InvalidOperationException("gpu frame prep failed"));
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));

        host.UpdateInputs(CreateInputs(handleBound: true));

        host.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.RenderingStatus.IsReady.Should().BeFalse();
        host.RenderingStatus.IsFallback.Should().BeFalse();
        host.RenderingStatus.UsesNativeSurface.Should().BeTrue();
        host.RenderingStatus.FallbackReason.Should().Contain("gpu frame prep failed");
        host.Snapshot.FallbackReason.Should().Contain("gpu frame prep failed");
    }

    [Fact]
    public void HandleBindingAfterSoftwareFrame_RehydratesGpuTileResources()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var inputs = CreateInputs(handleBound: false);

        host.UpdateInputs(inputs);

        host.Snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
        resourceFactory.VertexBufferCreationCount.Should().Be(0);

        host.UpdateInputs(inputs with
        {
            NativeHandle = new IntPtr(0x1234),
            HandleBound = true,
        });

        host.Snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.Snapshot.IsReady.Should().BeTrue();
        resourceFactory.VertexBufferCreationCount.Should().Be(1);
        resourceFactory.IndexBufferCreationCount.Should().Be(1);
        resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 4112).LatestRawData.Should().NotBeNull();
    }

    [Fact]
    public void TileResidencyChurn_ReleasesStaleScalarBuffersFromExecutorCache()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 4, height: 4);
        var firstTile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var secondTile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 36f);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [firstTile],
            ColorMap = CreateColorMap(metadata),
        });

        var firstScalarBuffer = resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 65536);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [secondTile],
            ColorMap = CreateColorMap(metadata),
        });

        graphicsBackend.CommandExecutor.ReleasedScalarBuffers.Should().Contain(buffer => ReferenceEquals(buffer, firstScalarBuffer));
        graphicsBackend.CommandExecutor.ReleaseBufferBeginFrameCounts.Should().ContainSingle().Which.Should().Be(2);
        firstScalarBuffer.DisposeCount.Should().Be(1);
        firstScalarBuffer.DisposeBeginFrameCounts.Should().ContainSingle().Which.Should().Be(2);
    }

    [Fact]
    public void ColorMapChangesDuringSoftwareInterval_ReuploadsPaletteWhenGpuReturns()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend));
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var initialColorMap = CreateColorMap(metadata);
        var replacementColorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF804020u, 0xFF20C0F0u));

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = initialColorMap,
        });

        var paletteBuffer = resourceFactory.UniformBuffers.Single(buffer => buffer.SizeInBytes == 4112);
        var initialPaletteBytes = paletteBuffer.LatestRawData!.ToArray();

        host.UpdateInputs(CreateInputs(handleBound: false) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = replacementColorMap,
        });

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = replacementColorMap,
        });

        paletteBuffer.LatestRawData.Should().NotBeNull();
        paletteBuffer.LatestRawData.Should().NotEqual(initialPaletteBytes);
    }

    private static SurfaceChartRenderInputs CreateInputs(bool handleBound)
    {
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
        var viewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));

        return new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = CreateColorMap(metadata),
            ViewState = viewState,
            ViewWidth = 320d,
            ViewHeight = 180d,
            NativeHandle = handleBound ? new IntPtr(0x1234) : IntPtr.Zero,
            HandleBound = handleBound,
            RenderScale = 1f,
        };
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0d, maximum: width - 1d),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0d, maximum: height - 1d),
            new SurfaceValueRange(0d, 100d));
    }

    private static SurfaceTile CreateTile(SurfaceMetadata metadata, SurfaceTileKey key, float tileValue)
    {
        var tileCountX = 1 << key.LevelX;
        var tileCountY = 1 << key.LevelY;
        var startX = (metadata.Width * key.TileX) / tileCountX;
        var endX = (metadata.Width * (key.TileX + 1)) / tileCountX;
        var startY = (metadata.Height * key.TileY) / tileCountY;
        var endY = (metadata.Height * (key.TileY + 1)) / tileCountY;
        var width = endX - startX;
        var height = endY - startY;
        var bounds = new SurfaceTileBounds(startX, startY, width, height);
        var values = new float[width * height];
        Array.Fill(values, tileValue);
        return new SurfaceTile(key, width, height, bounds, values, metadata.ValueRange);
    }

    private static SurfaceTile CreateTile(
        SurfaceTileKey key,
        SurfaceTileBounds bounds,
        int width,
        int height,
        float[] values,
        SurfaceValueRange range)
    {
        return new SurfaceTile(key, width, height, bounds, values, range);
    }

    private static SurfaceColorMap CreateColorMap(SurfaceMetadata metadata)
    {
        return new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF203040u, 0xFFE0F0FFu));
    }

    private static float ReadScalarPayloadValue(byte[] payload, int scalarIndex)
    {
        var offset = scalarIndex * sizeof(float);
        return BitConverter.ToSingle(payload, offset);
    }

    private sealed class FakeGraphicsBackend : IGraphicsBackend
    {
        private readonly Exception? _initializeException;
        private readonly Exception? _beginFrameException;
        private readonly FakeResourceFactory _resourceFactory;

        public FakeGraphicsBackend(
            Exception? initializeException = null,
            Exception? beginFrameException = null,
            FakeResourceFactory? resourceFactory = null)
        {
            _initializeException = initializeException;
            _beginFrameException = beginFrameException;
            _resourceFactory = resourceFactory ?? new FakeResourceFactory();
            _resourceFactory.Owner = this;
            CommandExecutor = new FakeCommandExecutor(this);
        }

        public bool IsInitialized { get; private set; }

        public int InitializeCallCount { get; private set; }

        public int BeginFrameCallCount { get; private set; }

        public IResourceFactory GetResourceFactory()
        {
            return _resourceFactory;
        }

        public ICommandExecutor GetCommandExecutor()
        {
            return CommandExecutor;
        }

        public FakeCommandExecutor CommandExecutor { get; }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            InitializeCallCount++;
            if (_initializeException is not null)
            {
                throw _initializeException;
            }

            IsInitialized = true;
        }

        public void Resize(int width, int height)
        {
        }

        public void BeginFrame()
        {
            BeginFrameCallCount++;
            if (_beginFrameException is not null)
            {
                throw _beginFrameException;
            }
        }

        public void EndFrame()
        {
        }

        public void SetClearColor(Vector4 color)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeResourceFactory : IResourceFactory
    {
        public FakeGraphicsBackend? Owner { get; set; }

        public int VertexBufferCreationCount { get; private set; }

        public int IndexBufferCreationCount { get; private set; }

        public int VertexBufferUpdateCount { get; private set; }

        public List<FakeBuffer> VertexBuffers { get; } = [];

        public List<FakeBuffer> UniformBuffers { get; } = [];

        public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
        {
            VertexBufferCreationCount++;
            var buffer = new FakeBuffer((uint)(vertices.Length * 40), this, trackVertexUpdates: true);
            buffer.RecordVertexData(vertices);
            VertexBuffers.Add(buffer);
            return buffer;
        }

        public IBuffer CreateVertexBuffer(uint sizeInBytes)
        {
            VertexBufferCreationCount++;
            var buffer = new FakeBuffer(sizeInBytes, this, trackVertexUpdates: true);
            VertexBuffers.Add(buffer);
            return buffer;
        }

        public IBuffer CreateIndexBuffer(uint[] indices)
        {
            IndexBufferCreationCount++;
            return new FakeBuffer((uint)(indices.Length * sizeof(uint)), this, trackVertexUpdates: false);
        }

        public IBuffer CreateIndexBuffer(uint sizeInBytes)
        {
            IndexBufferCreationCount++;
            return new FakeBuffer(sizeInBytes, this, trackVertexUpdates: false);
        }

        public IBuffer CreateUniformBuffer(uint sizeInBytes)
        {
            var buffer = new FakeBuffer(sizeInBytes, this, trackVertexUpdates: false, trackUniformUpdates: true);
            UniformBuffers.Add(buffer);
            return buffer;
        }

        public IPipeline CreatePipeline(PipelineDescription description)
        {
            return new FakePipeline();
        }

        public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
        {
            return new FakePipeline();
        }

        public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
        {
            return new FakeShader();
        }

        public IResourceSet CreateResourceSet(ResourceSetDescription description)
        {
            return new FakeResourceSet();
        }

        public void RecordVertexBufferUpdate()
        {
            VertexBufferUpdateCount++;
        }
    }

    private sealed class FakeCommandExecutor : ICommandExecutor, IBufferBindingCacheInvalidator
    {
        private readonly FakeGraphicsBackend? _owner;

        public FakeCommandExecutor(FakeGraphicsBackend? owner = null)
        {
            _owner = owner;
        }

        public HashSet<uint> BoundVertexBufferSlots { get; } = [];

        public List<IBuffer> ReleasedScalarBuffers { get; } = [];

        public List<int> ReleaseBufferBeginFrameCounts { get; } = [];

        public void SetPipeline(IPipeline pipeline)
        {
        }

        public void SetVertexBuffer(IBuffer buffer, uint index = 0)
        {
            BoundVertexBufferSlots.Add(index);
        }

        public void SetIndexBuffer(IBuffer buffer)
        {
        }

        public void SetResourceSet(uint slot, IResourceSet resourceSet)
        {
        }

        public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
        }

        public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
        }

        public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
        {
        }

        public void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f)
        {
        }

        public void SetScissorRect(int x, int y, int width, int height)
        {
        }

        public void Clear(float r, float g, float b, float a)
        {
        }

        public void SetDepthState(bool testEnabled, bool writeEnabled)
        {
        }

        public void ResetDepthState()
        {
        }

        public void ReleaseBuffer(IBuffer buffer)
        {
            ReleasedScalarBuffers.Add(buffer);
            ReleaseBufferBeginFrameCounts.Add(_owner?.BeginFrameCallCount ?? 0);
        }
    }

    private sealed class FakeBuffer : IBuffer
    {
        private readonly FakeResourceFactory _owner;
        private readonly bool _trackVertexUpdates;
        private readonly bool _trackUniformUpdates;

        public FakeBuffer(uint sizeInBytes, FakeResourceFactory owner, bool trackVertexUpdates, bool trackUniformUpdates = false)
        {
            SizeInBytes = sizeInBytes;
            _owner = owner;
            _trackVertexUpdates = trackVertexUpdates;
            _trackUniformUpdates = trackUniformUpdates;
        }

        public uint SizeInBytes { get; }

        public VertexPositionNormalColor[]? LatestVertexData { get; private set; }

        public byte[]? LatestRawData { get; private set; }

        public int UpdateCount { get; private set; }

        public int DisposeCount { get; private set; }

        public List<int> DisposeBeginFrameCounts { get; } = [];

        public void Update<T>(T data)
            where T : unmanaged
        {
            LatestRawData = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref data, 1)).ToArray();
            if (_trackUniformUpdates)
            {
                UpdateCount++;
            }

            if (_trackVertexUpdates)
            {
                _owner.RecordVertexBufferUpdate();
            }
        }

        public void UpdateArray<T>(T[] data)
            where T : unmanaged
        {
            LatestRawData = MemoryMarshal.AsBytes(data.AsSpan()).ToArray();

            if (data is VertexPositionNormalColor[] vertices)
            {
                RecordVertexData(vertices);
            }

            if (_trackUniformUpdates)
            {
                UpdateCount++;
            }

            if (_trackVertexUpdates)
            {
                _owner.RecordVertexBufferUpdate();
            }
        }

        public void SetData<T>(T data, uint offset)
            where T : unmanaged
        {
        }

        public void SetData<T>(T[] data, uint offset)
            where T : unmanaged
        {
        }

        public void Dispose()
        {
            DisposeCount++;
            DisposeBeginFrameCounts.Add(_owner.Owner?.BeginFrameCallCount ?? 0);
        }

        public void RecordVertexData(VertexPositionNormalColor[] vertices)
        {
            LatestVertexData = (VertexPositionNormalColor[])vertices.Clone();
        }
    }

    private sealed class FakePipeline : IPipeline
    {
        public void Dispose()
        {
        }
    }

    private sealed class FakeShader : IShader
    {
        public void Dispose()
        {
        }
    }

    private sealed class FakeResourceSet : IResourceSet
    {
        public void Dispose()
        {
        }
    }
}
