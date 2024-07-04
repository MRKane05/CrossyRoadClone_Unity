// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "VitaHOT/DiffuseOverglow" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Color("Main Color", Color) = (1,1,1,1)
    _LightColor("Light Color", Color) = (1,0,0,1)
    _LightMix("Light Mix", Range(0.0, 1.0)) = 1.0
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
    half4 _Color;
    half4 _LightColor;
    float _LightMix;
struct Input {
    float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    c = _Color + c *_LightColor * _LightMix;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
