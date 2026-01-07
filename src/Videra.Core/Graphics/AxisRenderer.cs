using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Videra.Core.Cameras;
// 注意：不再依赖外部的 VertexPositionNormalColor，避免混淆

namespace Videra.Core.Graphics;

public class AxisRenderer : IDisposable
{
    // --- 私有结构体：专门用于坐标轴，确保内存对齐绝对正确 ---
    private struct AxisVertex
    {
        public Vector3 Position;
        public RgbaFloat Color;

        // Vector3(12) + RgbaFloat(16) = 28 bytes
        public const uint SizeInBytes = 28;

        public AxisVertex(Vector3 pos, RgbaFloat col)
        {
            Position = pos;
            Color = col;
        }
    }
    // -------------------------------------------------------

    private DeviceBuffer _vertexBuffer;
    private DeviceBuffer _indexBuffer;
    private DeviceBuffer _uniformBuffer;
    private Pipeline _pipeline;
    private ResourceSet _resourceSet;
    private GraphicsDevice _gd;

    private const float AxisLength = 1.0f;
    private const int GizmoSize = 100;
    private const float Margin = 20;

    public RgbaFloat XColor { get; set; } = RgbaFloat.Red;
    public RgbaFloat YColor { get; set; } = RgbaFloat.Green;
    public RgbaFloat ZColor { get; set; } = RgbaFloat.Blue;

    public bool IsVisible { get; set; } = true;

    public void Initialize(GraphicsDevice gd, OutputDescription outputDesc)
    {
        _gd = gd;
        var factory = gd.ResourceFactory;

        // 1. 创建 Buffer (使用新的 AxisVertex 大小)
        _vertexBuffer = factory.CreateBuffer(new BufferDescription(6 * AxisVertex.SizeInBytes, BufferUsage.VertexBuffer));
        _indexBuffer = factory.CreateBuffer(new BufferDescription(6 * sizeof(ushort), BufferUsage.IndexBuffer));
        _uniformBuffer = factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        // 填充数据
        UpdateGeometry();

        // 2. Shader (极简版：去掉了法线输入)
        // 显卡直接把 layout(location=1) 的数据当做颜色
        string vsCode = @"
#version 450
layout(location=0) in vec3 Pos; 
layout(location=1) in vec4 Col; // 注意：这里 location=1 直接是颜色

layout(location=0) out vec4 vCol;

layout(set=0, binding=0) uniform VP { mat4 V; mat4 P; };

void main() { 
    gl_Position = P * V * vec4(Pos, 1); 
    vCol = Col; 
}";

        string fsCode = @"
#version 450
layout(location=0) in vec4 vCol; 
layout(location=0) out vec4 Out;
void main() { Out = vCol; }";

        var vDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vsCode), "main");
        var fDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fsCode), "main");
        var shaders = factory.CreateFromSpirv(vDesc, fDesc);

        // 3. Layout (必须与 AxisVertex 结构严格对应)
        var vertexLayoutDesc = new VertexLayoutDescription(
            new VertexElementDescription("Pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3), // 12 bytes
            new VertexElementDescription("Col", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)  // 16 bytes
        );

        // 4. Pipeline
        var resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("VP", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(resourceLayout, _uniformBuffer));

        var pd = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = DepthStencilStateDescription.Disabled, // 禁用深度测试
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.LineList,
            ResourceLayouts = new[] { resourceLayout },
            ShaderSet = new ShaderSetDescription(
                new[] { vertexLayoutDesc },
                shaders),
            Outputs = outputDesc
        };

        _pipeline = factory.CreateGraphicsPipeline(pd);
    }

    public void UpdateGeometry()
    {
        if (_gd == null) return;

        // 使用简化的 AxisVertex 填充数据
        var vertices = new[]
        {
            new AxisVertex(Vector3.Zero, XColor),                   // 0
            new AxisVertex(Vector3.Zero, YColor),                   // 1
            new AxisVertex(Vector3.Zero, ZColor),                   // 2
            new AxisVertex(Vector3.UnitX * AxisLength, XColor),     // 3
            new AxisVertex(Vector3.UnitY * AxisLength, YColor),     // 4
            new AxisVertex(Vector3.UnitZ * AxisLength, ZColor),     // 5
        };
        var indices = new ushort[] { 0, 3, 1, 4, 2, 5 };

        _gd.UpdateBuffer(_vertexBuffer, 0, vertices);
        _gd.UpdateBuffer(_indexBuffer, 0, indices);
    }

    public void Draw(CommandList cl, OrbitCamera mainCamera, float windowWidth, float windowHeight)
    {
        if (!IsVisible) return;

        cl.SetViewport(0, new Viewport(Margin, windowHeight - GizmoSize - Margin, GizmoSize, GizmoSize, 0, 1));

        // 保持旋转，锁定位置
        var rotation = Quaternion.CreateFromYawPitchRoll(mainCamera.Yaw, mainCamera.Pitch, 0);
        var eyePos = Vector3.Transform(new Vector3(0, 0, 2.5f), rotation);
        var viewMatrix = Matrix4x4.CreateLookAt(eyePos, Vector3.Zero, Vector3.UnitY);
        var projMatrix = Matrix4x4.CreatePerspectiveFieldOfView(0.8f, 1.0f, 0.1f, 10f);

        cl.UpdateBuffer(_uniformBuffer, 0, viewMatrix);
        cl.UpdateBuffer(_uniformBuffer, 64, projMatrix);

        cl.SetPipeline(_pipeline);
        cl.SetGraphicsResourceSet(0, _resourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer);
        cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        cl.DrawIndexed(6, 1, 0, 0, 0);

        cl.SetViewport(0, new Viewport(0, 0, windowWidth, windowHeight, 0, 1));
    }

    public void Dispose()
    {
        _pipeline?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _uniformBuffer?.Dispose();
        _resourceSet?.Dispose();
    }
}