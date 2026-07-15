#ifndef CUSTOM_FUNCTION_TESSELLATION_SCALE_SAFE_INCLUDED
#define CUSTOM_FUNCTION_TESSELLATION_SCALE_SAFE_INCLUDED

#pragma require tessellation

#pragma hull HullProgram
#pragma domain DomainProgram

#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE) || \
    defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL) || \
    defined(SHADER_API_PSSL)
    #define UNITY_domain domain
    #define UNITY_partitioning partitioning
    #define UNITY_outputtopology outputtopology
    #define UNITY_patchconstantfunc patchconstantfunc
    #define UNITY_outputcontrolpoints outputcontrolpoints
#endif

struct TessellationFactors
{
    float edge[3] : SV_TessFactor;
    float inside  : SV_InsideTessFactor;
};

// Populated from the Shader Graph custom function input.
// Defaults to 1 so tessellation still works if the input is not wired.
float _GraphTessInput = 1.0;

// Converts a clip-space point into pixel coordinates.
// Using clip space makes the tessellation react to Transform scale,
// camera distance, resolution, and perspective automatically.
float2 TessClipToPixel(float4 positionCS)
{
    float safeW = max(abs(positionCS.w), 1e-5);
    float2 ndc = positionCS.xy / safeW;
    return (ndc * 0.5 + 0.5) * _ScreenParams.xy;
}

TessellationFactors PatchConstantFunction(InputPatch<PackedVaryings, 3> patch)
{
    // Use stable, uniform tessellation from graph input to avoid
    // adaptive screen-space distortion artifacts.
    float tessLevel = clamp(_GraphTessInput, 1.0, 64.0);

    TessellationFactors factors;
    factors.edge[0] = tessLevel;
    factors.edge[1] = tessLevel;
    factors.edge[2] = tessLevel;
    factors.inside = tessLevel;
    return factors;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("fractional_odd")]
[UNITY_patchconstantfunc("PatchConstantFunction")]
PackedVaryings HullProgram(
    InputPatch<PackedVaryings, 3> patch,
    uint controlPointId : SV_OutputControlPointID)
{
    return patch[controlPointId];
}

// Add post-tessellation displacement here. Displacement built only in the
// normal Shader Graph Vertex block happens before the new vertices exist.
void ModifyTessellatedVertex(inout PackedVaryings input)
{
}

#define TESSELLATION_INTERPOLATE(fieldName) \
    output.fieldName = \
        patch[0].fieldName * barycentricCoordinates.x + \
        patch[1].fieldName * barycentricCoordinates.y + \
        patch[2].fieldName * barycentricCoordinates.z;

[UNITY_domain("tri")]
PackedVaryings DomainProgram(
    TessellationFactors factors,
    OutputPatch<PackedVaryings, 3> patch,
    float3 barycentricCoordinates : SV_DomainLocation)
{
    PackedVaryings output = patch[0];

    #ifdef VARYINGS_NEED_POSITION_WS
        TESSELLATION_INTERPOLATE(positionWS)
        output.positionCS = TransformWorldToHClip(output.positionWS);
    #else
        TESSELLATION_INTERPOLATE(positionCS)
    #endif

    #ifdef VARYINGS_NEED_NORMAL_WS
        TESSELLATION_INTERPOLATE(normalWS)
    #endif

    #ifdef VARYINGS_NEED_TANGENT_WS
        TESSELLATION_INTERPOLATE(tangentWS)
    #endif

    #ifdef VARYINGS_NEED_COLOR
        TESSELLATION_INTERPOLATE(color)
    #endif

    #ifdef VARYINGS_NEED_TEXCOORD0
        TESSELLATION_INTERPOLATE(texCoord0)
    #endif

    #ifdef VARYINGS_NEED_TEXCOORD1
        TESSELLATION_INTERPOLATE(texCoord1)
    #endif

    #ifdef VARYINGS_NEED_TEXCOORD2
        TESSELLATION_INTERPOLATE(texCoord2)
    #endif

    #ifdef VARYINGS_NEED_TEXCOORD3
        TESSELLATION_INTERPOLATE(texCoord3)
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        TESSELLATION_INTERPOLATE(shadowCoord)
    #endif

    ModifyTessellatedVertex(output);
    return output;
}

void ForceTessDummy_half(out half Out)
{
    Out = 0.0h;
}

void ForceTessDummy_half(half In, out half Out)
{
    _GraphTessInput = max((float)In, 0.01);
    Out = 0.0h;
}

void ForceTessDummy_float(out float Out)
{
    Out = 0.0f;
}

void ForceTessDummy_float(float In, out float Out)
{
    _GraphTessInput = max(In, 0.01);
    Out = 0.0f;
}

void ForceTessDummy_float(float2 In, out float2 Out)
{
    _GraphTessInput = max(In.x, 0.01);
    Out = 0.0f;
}

void ForceTessDummy_float(float3 In, out float3 Out)
{
    _GraphTessInput = max(In.x, 0.01);
    Out = 0.0f;
}

void ForceTessDummy_float(float4 In, out float4 Out)
{
    _GraphTessInput = max(In.x, 0.01);
    Out = 0.0f;
}

#endif
