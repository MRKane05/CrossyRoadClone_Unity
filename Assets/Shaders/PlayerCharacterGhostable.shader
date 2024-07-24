Shader "VitaHOT/PlayerCharacterGhostable" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _GhostCol("Ghost Color", Color) = (0.8, 1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _Alpha("Alpha", Range(0.0, 1.0)) = 1.0
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }
        //Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 200
        //ZWrite Always

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _GhostCol;
        fixed _Alpha;

        struct Input {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = lerp(_GhostCol.rgb, c.rgb, _Alpha);
            o.Alpha = 1.0;
        }
        ENDCG

    }
    Fallback "Legacy Shaders/Mobile/Diffuse"
}
