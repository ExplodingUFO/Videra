using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwareCommandExecutor : ICommandExecutor
{
    private const bool DefaultDepthTestEnabled = true;
    private const bool DefaultDepthWriteEnabled = true;

    private readonly SoftwareFrameBuffer _frameBuffer;
    private readonly Dictionary<uint, SoftwareBuffer> _vertexBuffers = new();
    private SoftwareBuffer? _indexBuffer;
    private IPipeline? _pipeline;
    private ViewportState _viewport;
    private bool _depthTestEnabled = DefaultDepthTestEnabled;
    private bool _depthWriteEnabled = DefaultDepthWriteEnabled;

    public SoftwareCommandExecutor(SoftwareFrameBuffer frameBuffer)
    {
        _frameBuffer = frameBuffer;
        _viewport = new ViewportState(0, 0, frameBuffer.Width, frameBuffer.Height, 0f, 1f);
    }

    public void SetPipeline(IPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public void SetVertexBuffer(IBuffer buffer, uint index = 0)
    {
        if (buffer is not SoftwareBuffer softwareBuffer)
            throw new ArgumentException("Buffer must be a SoftwareBuffer.");

        _vertexBuffers[index] = softwareBuffer;
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        if (buffer is not SoftwareBuffer softwareBuffer)
            throw new ArgumentException("Buffer must be a SoftwareBuffer.");

        _indexBuffer = softwareBuffer;
    }

    public void SetResourceSet(uint slot, IResourceSet resourceSet)
    {
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        DrawIndexed(0, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        if (instanceCount == 0 || !_vertexBuffers.TryGetValue(0, out var vertexBuffer) || _indexBuffer == null)
            return;

        var vertices = vertexBuffer.AsSpan<VertexPositionNormalColor>();
        var indices = _indexBuffer.AsSpan<uint>();

        if (firstIndex >= indices.Length || vertices.Length == 0)
            return;

        var available = Math.Min(indexCount, (uint)indices.Length - firstIndex);
        var primitive = ResolvePrimitiveMode(primitiveType);

        var world = ReadMatrixFromSlot(2);
        var (view, projection) = ReadCameraMatrices();
        var mvp = world * view * projection;
        var alphaMask = ReadAlphaMaskState();
        var alphaBlendEnabled = _pipeline is SoftwarePipeline pipeline && pipeline.AlphaBlendEnabled;

        if (primitive == PrimitiveMode.LineList)
        {
            for (uint i = 0; i + 1 < available; i += 2)
            {
                if (!TryGetVertex(indices, firstIndex + i, vertexOffset, vertices, mvp, out var v0))
                    continue;
                if (!TryGetVertex(indices, firstIndex + i + 1, vertexOffset, vertices, mvp, out var v1))
                    continue;

                DrawLine(v0, v1, _depthTestEnabled, _depthWriteEnabled, alphaMask, alphaBlendEnabled);
            }

            return;
        }

        if (primitive == PrimitiveMode.PointList)
        {
            for (uint i = 0; i < available; i++)
            {
                if (!TryGetVertex(indices, firstIndex + i, vertexOffset, vertices, mvp, out var v0))
                    continue;
                DrawPoint(v0, _depthTestEnabled, _depthWriteEnabled, alphaMask, alphaBlendEnabled);
            }

            return;
        }

        for (uint i = 0; i + 2 < available; i += 3)
        {
            if (!TryGetVertex(indices, firstIndex + i, vertexOffset, vertices, mvp, out var v0))
                continue;
            if (!TryGetVertex(indices, firstIndex + i + 1, vertexOffset, vertices, mvp, out var v1))
                continue;
            if (!TryGetVertex(indices, firstIndex + i + 2, vertexOffset, vertices, mvp, out var v2))
                continue;

            DrawTriangle(v0, v1, v2, _depthTestEnabled, _depthWriteEnabled, alphaMask, alphaBlendEnabled);
        }
    }

    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
    }

    public void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f)
    {
        _viewport = new ViewportState(x, y, width, height, minDepth, maxDepth);
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
    }

    public void Clear(float r, float g, float b, float a)
    {
        _frameBuffer.Clear(new Vector4(r, g, b, a));
    }

    public void SetDepthState(bool testEnabled, bool writeEnabled)
    {
        _depthTestEnabled = testEnabled;
        _depthWriteEnabled = writeEnabled;
    }

    public void ResetDepthState()
    {
        _depthTestEnabled = DefaultDepthTestEnabled;
        _depthWriteEnabled = DefaultDepthWriteEnabled;
    }

    private Matrix4x4 ReadMatrixFromSlot(uint slot)
    {
        if (_vertexBuffers.TryGetValue(slot, out var buffer) && buffer.SizeInBytes >= 64)
            return buffer.Read<Matrix4x4>(0);

        return Matrix4x4.Identity;
    }

    private (Matrix4x4 view, Matrix4x4 projection) ReadCameraMatrices()
    {
        if (_vertexBuffers.TryGetValue(1, out var buffer) && buffer.SizeInBytes >= 128)
        {
            var view = buffer.Read<Matrix4x4>(0);
            var projection = buffer.Read<Matrix4x4>(64);
            return (view, projection);
        }

        return (Matrix4x4.Identity, Matrix4x4.Identity);
    }

    private ObjectAlphaMaskUniformData ReadAlphaMaskState()
    {
        if (_vertexBuffers.TryGetValue(RenderBindingSlots.AlphaMask, out var buffer) && buffer.SizeInBytes >= 16)
        {
            return buffer.Read<ObjectAlphaMaskUniformData>(0);
        }

        return ObjectAlphaMaskUniformData.From(Scene.MaterialAlphaSettings.Opaque);
    }

    private bool TryGetVertex(
        ReadOnlySpan<uint> indices,
        uint indexPosition,
        int vertexOffset,
        ReadOnlySpan<VertexPositionNormalColor> vertices,
        Matrix4x4 mvp,
        out VertexOut vertexOut)
    {
        var rawIndex = (int)indices[(int)indexPosition] + vertexOffset;
        if (rawIndex < 0 || rawIndex >= vertices.Length)
        {
            vertexOut = default;
            return false;
        }

        return TryProject(vertices[rawIndex], mvp, out vertexOut);
    }

    private bool TryProject(VertexPositionNormalColor vertex, Matrix4x4 mvp, out VertexOut projected)
    {
        var position = Vector4.Transform(new Vector4(vertex.Position, 1f), mvp);
        if (position.W <= 0.0001f)
        {
            projected = default;
            return false;
        }

        var invW = 1f / position.W;
        var ndcX = position.X * invW;
        var ndcY = position.Y * invW;
        var ndcZ = position.Z * invW;

        var x = _viewport.X + (ndcX + 1f) * 0.5f * _viewport.Width;
        var y = _viewport.Y + (1f - (ndcY + 1f) * 0.5f) * _viewport.Height;
        var z = Math.Clamp(ndcZ, 0f, 1f);

        projected = new VertexOut(
            x,
            y,
            z,
            new Vector4(
                Math.Clamp(vertex.Color.R, 0f, 1f),
                Math.Clamp(vertex.Color.G, 0f, 1f),
                Math.Clamp(vertex.Color.B, 0f, 1f),
                Math.Clamp(vertex.Color.A, 0f, 1f)));
        return true;
    }

    private void DrawPoint(VertexOut v0, bool depthTest, bool depthWrite, ObjectAlphaMaskUniformData alphaMask, bool alphaBlendEnabled)
    {
        if (!TryResolveAlphaMaskedColor(v0.Color, alphaMask, out var maskedColor))
        {
            return;
        }

        DrawPixel((int)MathF.Round(v0.X), (int)MathF.Round(v0.Y), v0.Z, maskedColor, depthWrite, depthTest, alphaBlendEnabled);
    }

    private void DrawLine(VertexOut v0, VertexOut v1, bool depthTest, bool depthWrite, ObjectAlphaMaskUniformData alphaMask, bool alphaBlendEnabled)
    {
        var dx = v1.X - v0.X;
        var dy = v1.Y - v0.Y;
        var steps = (int)MathF.Max(MathF.Abs(dx), MathF.Abs(dy));

        if (steps <= 0)
        {
            DrawPoint(v0, depthTest, depthWrite, alphaMask, alphaBlendEnabled);
            return;
        }

        for (int i = 0; i <= steps; i++)
        {
            var t = i / (float)steps;
            var x = Lerp(v0.X, v1.X, t);
            var y = Lerp(v0.Y, v1.Y, t);
            var z = Lerp(v0.Z, v1.Z, t);
            var color = Vector4.Lerp(v0.Color, v1.Color, t);

            if (!TryResolveAlphaMaskedColor(color, alphaMask, out var maskedColor))
            {
                continue;
            }

            DrawPixel((int)MathF.Round(x), (int)MathF.Round(y), z, maskedColor, depthWrite, depthTest, alphaBlendEnabled);
        }
    }

    private void DrawTriangle(VertexOut v0, VertexOut v1, VertexOut v2, bool depthTest, bool depthWrite, ObjectAlphaMaskUniformData alphaMask, bool alphaBlendEnabled)
    {
        var minX = (int)MathF.Floor(MathF.Min(v0.X, MathF.Min(v1.X, v2.X)));
        var maxX = (int)MathF.Ceiling(MathF.Max(v0.X, MathF.Max(v1.X, v2.X)));
        var minY = (int)MathF.Floor(MathF.Min(v0.Y, MathF.Min(v1.Y, v2.Y)));
        var maxY = (int)MathF.Ceiling(MathF.Max(v0.Y, MathF.Max(v1.Y, v2.Y)));

        minX = Math.Clamp(minX, 0, _frameBuffer.Width - 1);
        maxX = Math.Clamp(maxX, 0, _frameBuffer.Width - 1);
        minY = Math.Clamp(minY, 0, _frameBuffer.Height - 1);
        maxY = Math.Clamp(maxY, 0, _frameBuffer.Height - 1);

        var area = Edge(v0, v1, v2);
        if (MathF.Abs(area) < 0.0001f)
            return;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                var w0 = Edge(v1, v2, p);
                var w1 = Edge(v2, v0, p);
                var w2 = Edge(v0, v1, p);

                if (!IsInside(w0, w1, w2, area))
                    continue;

                var invArea = 1f / area;
                var b0 = w0 * invArea;
                var b1 = w1 * invArea;
                var b2 = w2 * invArea;

                var depth = v0.Z * b0 + v1.Z * b1 + v2.Z * b2;
                var color = v0.Color * b0 + v1.Color * b1 + v2.Color * b2;

                if (!TryResolveAlphaMaskedColor(color, alphaMask, out var maskedColor))
                {
                    continue;
                }

                DrawPixel(x, y, depth, maskedColor, depthWrite, depthTest, alphaBlendEnabled);
            }
        }
    }

    private static bool TryResolveAlphaMaskedColor(Vector4 color, ObjectAlphaMaskUniformData alphaMask, out Vector4 resolvedColor)
    {
        if (alphaMask.MaskEnabled <= 0.5f)
        {
            resolvedColor = color;
            return true;
        }

        if (color.W < alphaMask.Cutoff)
        {
            resolvedColor = default;
            return false;
        }

        if (color.W <= 0f)
        {
            resolvedColor = default;
            return false;
        }

        resolvedColor = new Vector4(color.X, color.Y, color.Z, 1f);
        return true;
    }

    private void DrawPixel(int x, int y, float depth, Vector4 color, bool writeDepth, bool depthTest, bool alphaBlendEnabled)
    {
        if (x < 0 || y < 0 || x >= _frameBuffer.Width || y >= _frameBuffer.Height)
            return;

        var index = y * _frameBuffer.Width + x;
        var depthBuffer = _frameBuffer.DepthBuffer;

        if (depthTest && depth > depthBuffer[index])
            return;

        if (writeDepth)
            depthBuffer[index] = depth;

        var colorBuffer = _frameBuffer.ColorBuffer;
        var offset = index * 4;

        if (!alphaBlendEnabled)
        {
            WriteColor(colorBuffer, offset, color);
            return;
        }

        var srcA = Math.Clamp(color.W, 0f, 1f);
        if (srcA <= 0f)
            return;

        var dst = new Vector4(
            colorBuffer[offset + 2] / 255f,
            colorBuffer[offset + 1] / 255f,
            colorBuffer[offset] / 255f,
            colorBuffer[offset + 3] / 255f);

        var src = new Vector4(color.X * srcA, color.Y * srcA, color.Z * srcA, srcA);
        var outColor = src + dst * (1f - srcA);
        WriteColor(colorBuffer, offset, outColor);
    }

    private static void WriteColor(Span<byte> buffer, int offset, Vector4 color)
    {
        var r = (byte)Math.Round(Math.Clamp(color.X, 0f, 1f) * 255f);
        var g = (byte)Math.Round(Math.Clamp(color.Y, 0f, 1f) * 255f);
        var b = (byte)Math.Round(Math.Clamp(color.Z, 0f, 1f) * 255f);
        var a = (byte)Math.Round(Math.Clamp(color.W, 0f, 1f) * 255f);

        buffer[offset] = b;
        buffer[offset + 1] = g;
        buffer[offset + 2] = r;
        buffer[offset + 3] = a;
    }

    private PrimitiveMode ResolvePrimitiveMode(uint primitiveType)
    {
        return primitiveType switch
        {
            1 => PrimitiveMode.LineList,
            2 => PrimitiveMode.PointList,
            3 => PrimitiveMode.TriangleList,
            _ => _pipeline is SoftwarePipeline pipeline
                ? pipeline.Topology switch
                {
                    PrimitiveTopology.LineList => PrimitiveMode.LineList,
                    PrimitiveTopology.PointList => PrimitiveMode.PointList,
                    _ => PrimitiveMode.TriangleList
                }
                : PrimitiveMode.TriangleList
        };
    }

    private static float Edge(VertexOut a, VertexOut b, Vector2 c)
    {
        return (c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X);
    }

    private static float Edge(VertexOut a, VertexOut b, VertexOut c)
    {
        return (c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X);
    }

    private static bool IsInside(float w0, float w1, float w2, float area)
    {
        return area > 0
            ? w0 >= 0 && w1 >= 0 && w2 >= 0
            : w0 <= 0 && w1 <= 0 && w2 <= 0;
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private readonly struct ViewportState
    {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float MinDepth { get; }
        public float MaxDepth { get; }

        public ViewportState(float x, float y, float width, float height, float minDepth, float maxDepth)
        {
            X = x;
            Y = y;
            Width = MathF.Max(1f, width);
            Height = MathF.Max(1f, height);
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }
    }

    private readonly struct VertexOut
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public Vector4 Color { get; }

        public VertexOut(float x, float y, float z, Vector4 color)
        {
            X = x;
            Y = y;
            Z = z;
            Color = color;
        }
    }

    private enum PrimitiveMode
    {
        TriangleList,
        LineList,
        PointList
    }
}
