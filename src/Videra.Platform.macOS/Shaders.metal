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
    float3 worldNormal;
    float4 color;
    float3 worldPos;
};

// Camera Uniform Buffer 结构
struct CameraUniforms {
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
};

// World Uniform Buffer 结构
struct WorldUniforms {
    float4x4 worldMatrix;
};

// Style Uniform Buffer 结构 (128 bytes, matching C# StyleUniformData)
struct StyleParams {
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

// 顶点着色器
vertex VertexOut vertex_main(
    VertexIn in [[stage_in]],
    constant CameraUniforms& camera [[buffer(1)]],
    constant WorldUniforms& world [[buffer(2)]],
    constant StyleParams& style [[buffer(3)]])
{
    VertexOut out;

    float4 worldPos = world.worldMatrix * float4(in.position, 1.0);
    float4 viewPos = camera.viewMatrix * worldPos;
    out.position = camera.projectionMatrix * viewPos;

    // Transform normal to world space
    out.worldNormal = normalize((world.worldMatrix * float4(in.normal, 0.0)).xyz);

    // Use vertex color or override color based on style setting
    out.color = style.useVertexColor ? in.color : style.overrideColor;
    out.worldPos = worldPos.xyz;

    return out;
}

// 片段着色器
fragment float4 fragment_main(
    VertexOut in [[stage_in]],
    constant StyleParams& style [[buffer(3)]])
{
    float3 normal = normalize(in.worldNormal);
    float3 lightDir = normalize(style.lightDirection);

    // 基础光照
    float ambient = style.ambientIntensity;
    float diffuse = max(dot(normal, lightDir), 0.0) * style.diffuseIntensity;

    // 高光 (Blinn-Phong)
    float3 viewDir = normalize(-in.worldPos);
    float3 halfDir = normalize(lightDir + viewDir);
    float specular = pow(max(dot(normal, halfDir), 0.0), style.specularPower) * style.specularIntensity;

    float lighting = ambient + diffuse + specular;

    // 应用光照
    float3 color = in.color.rgb * lighting;

    // 色调叠加
    color *= style.tintColor;

    // 饱和度调整
    float grey = dot(color, float3(0.299, 0.587, 0.114));
    color = mix(float3(grey), color, style.saturation);

    // 对比度调整
    color = (color - 0.5) * style.contrast + 0.5;

    // 亮度调整
    color += style.brightness;

    return float4(saturate(color), in.color.a * style.opacity);
}
