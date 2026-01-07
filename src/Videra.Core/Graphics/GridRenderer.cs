using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Videra.Core.Cameras;

namespace Videra.Core.Graphics;

public class GridRenderer : IDisposable
{
    private DeviceBuffer _vertexBuffer;
    private DeviceBuffer _uniformBuffer; // 存放 View, Proj, World, Color
    private Pipeline _pipeline;
    private ResourceSet _resourceSet;
    private GraphicsDevice _gd;

    // 网格配置
    private const int GridSize = 20; // 网格范围 +/- 20
    private const int GridStep = 1;  // 网格间距

    // --- 公开属性 ---
    public bool IsVisible { get; set; } = true;

    // 网格高度 (Y轴偏移)
    private float _height = 0.0f;
    public float Height
    {
        get => _height;
        set { _height = value; UpdateUniforms(); } // 改变高度时更新 Uniform
    }

    // 网格颜色
    private RgbaFloat _gridColor = new RgbaFloat(0.4f, 0.4f, 0.4f, 0.5f); // 默认半透明灰
    public RgbaFloat GridColor
    {
        get => _gridColor;
        set { _gridColor = value; UpdateUniforms(); }
    }

    // 缓存相机引用以更新 View/Proj
    private OrbitCamera _camera;

    public void Initialize(GraphicsDevice gd, OutputDescription outputDesc)
    {
        _gd = gd;
        var factory = gd.ResourceFactory;

        // 1. 生成网格顶点 (在 Y=0 平面生成)
        var vertices = new List<Vector3>();
        // 平行于 Z 轴的线 (沿 X 轴排列)
        for (int x = -GridSize; x <= GridSize; x += GridStep)
        {
            vertices.Add(new Vector3(x, 0, -GridSize));
            vertices.Add(new Vector3(x, 0, GridSize));
        }
        // 平行于 X 轴的线 (沿 Z 轴排列)
        for (int z = -GridSize; z <= GridSize; z += GridStep)
        {
            vertices.Add(new Vector3(-GridSize, 0, z));
            vertices.Add(new Vector3(GridSize, 0, z));
        }

        _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(vertices.Count * 12), BufferUsage.VertexBuffer));
        gd.UpdateBuffer(_vertexBuffer, 0, vertices.ToArray());

        // 2. Uniform Buffer 布局:
        // 0-64: View
        // 64-128: Proj
        // 128-192: World (用于高度平移)
        // 192-208: Color
        _uniformBuffer = factory.CreateBuffer(new BufferDescription(256, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        // 3. Shader
        string vsCode = @"
#version 450
layout(location=0) in vec3 Pos;
layout(set=0, binding=0) uniform Uniforms { 
    mat4 V; 
    mat4 P; 
    mat4 W; 
    vec4 Color; 
};
void main() { 
    gl_Position = P * V * W * vec4(Pos, 1); 
}";

        string fsCode = @"
#version 450
layout(set=0, binding=0) uniform Uniforms { 
    mat4 V; 
    mat4 P; 
    mat4 W; 
    vec4 Color; 
};
layout(location=0) out vec4 Out;
void main() { Out = Color; }";

        var vDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vsCode), "main");
        var fDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fsCode), "main");
        var shaders = factory.CreateFromSpirv(vDesc, fDesc);

        // 4. Pipeline & Layout
        var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Uniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

        _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(layout, _uniformBuffer));

        var pd = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleAlphaBlend, // 支持半透明
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual, // 正常的深度测试
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.LineList,
            ResourceLayouts = new[] { layout },
            ShaderSet = new ShaderSetDescription(
                new[] { new VertexLayoutDescription(new VertexElementDescription("Pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)) },
                shaders),
            Outputs = outputDesc
        };

        _pipeline = factory.CreateGraphicsPipeline(pd);
    }

    private void UpdateUniforms()
    {
        if (_gd == null || _camera == null) return;

        // 更新 World 矩阵 (处理高度) 和 颜色
        var world = Matrix4x4.CreateTranslation(0, _height, 0);

        _gd.UpdateBuffer(_uniformBuffer, 0, _camera.ViewMatrix);
        _gd.UpdateBuffer(_uniformBuffer, 64, _camera.ProjectionMatrix);
        _gd.UpdateBuffer(_uniformBuffer, 128, world);
        _gd.UpdateBuffer(_uniformBuffer, 192, _gridColor);
    }

    public void Draw(CommandList cl, OrbitCamera camera)
    {
        if (!IsVisible) return;
        _camera = camera;

        // 更新所有矩阵
        UpdateUniforms();

        cl.SetPipeline(_pipeline);
        cl.SetGraphicsResourceSet(0, _resourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer);
        // 线段数量 = 顶点数
        //cl.DrawIndexed((uint)(_vertexBuffer.SizeInBytes / 12), 1, 0, 0, 0);
        // 注意：这里其实用了 DrawIndexed 的参数但不需要 IndexBuffer，
        // 对于 LineList 且没有 IndexBuffer，应该用 cl.Draw(...)。
        // 修正如下：
        cl.Draw((uint)(_vertexBuffer.SizeInBytes / 12), 1, 0, 0);
    }

    public void Dispose()
    {
        _pipeline?.Dispose();
        _vertexBuffer?.Dispose();
        _uniformBuffer?.Dispose();
        _resourceSet?.Dispose();
    }
}