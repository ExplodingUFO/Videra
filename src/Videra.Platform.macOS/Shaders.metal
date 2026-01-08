#include <metal_stdlib>
using namespace metal;

// 顶点输入结构
struct VertexIn {
    float3 position [[attribute(0)]];
    float3 normal   [[attribute(1)]];
    float4 color    [[attribute(2)]];
};

// 顶点输出 / 片段输入结构
struct VertexOut {
    float4 position [[position]];
    float3 normal;
    float4 color;
};

// Uniform Buffer 结构
struct Uniforms {
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
    float4x4 worldMatrix;
};

// 顶点着色器
vertex VertexOut vertex_main(
    VertexIn in [[stage_in]],
    constant Uniforms& uniforms [[buffer(1)]])
{
    VertexOut out;
    
    float4 worldPos = uniforms.worldMatrix * float4(in.position, 1.0);
    float4 viewPos = uniforms.viewMatrix * worldPos;
    out.position = uniforms.projectionMatrix * viewPos;
    
    // 简单传递法线和颜色
    out.normal = in.normal;
    out.color = in.color;
    
    return out;
}

// 片段着色器
fragment float4 fragment_main(
    VertexOut in [[stage_in]])
{
    // 简单的光照计算
    float3 lightDir = normalize(float3(0.5, 1.0, 0.3));
    float diffuse = max(dot(normalize(in.normal), lightDir), 0.0);
    float ambient = 0.3;
    
    float3 lighting = float3(ambient + diffuse);
    float3 finalColor = in.color.rgb * lighting;
    
    return float4(finalColor, in.color.a);
}
