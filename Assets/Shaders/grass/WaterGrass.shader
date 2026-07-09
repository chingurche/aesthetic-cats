Shader "Custom/WaterGrass"
{
    Properties
    {
        _BaseColor ("Base Color (Roots)", Color) = (0.2, 0.4, 0.2, 1)
        _TipColor ("Tip Color (Ends)", Color) = (0.4, 0.7, 0.3, 1)
        
        _Density ("Strand Density", Range(1.0, 20.0)) = 5.0
        _Thickness ("Base Thickness", Range(0.01, 0.5)) = 0.2
        _LengthVariation ("Length Variation", Range(0.0, 1.0)) = 0.5
        
        _SwaySpeed ("Sway Speed", Range(0.0, 5.0)) = 2.0
        _Waviness ("Waviness Amount", Range(0.0, 10.0)) = 3.0
        
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
        LOD 200
        
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard alphatest:_Cutoff addshadow vertex:vert
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _BaseColor;
        fixed4 _TipColor;
        float _Density;
        float _Thickness;
        float _LengthVariation;
        float _SwaySpeed;
        float _Waviness;

        float random(float2 st)
        {
            return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
        }

        void vert (inout appdata_full v)
        {
            float globalSway = sin(_Time.y * _SwaySpeed + v.vertex.x * _Waviness);
            v.vertex.x += globalSway * 0.05 * v.texcoord.y;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            
            float strandID = floor(uv.x * _Density);
            float localX = frac(uv.x * _Density);

            float rnd = random(float2(strandID, 0.0));
            
            localX += sin(uv.y * (_Waviness + rnd * 5.0) + _Time.y * (_SwaySpeed * 1.5) + rnd * 10.0) * 0.15 * uv.y;

            float currentThickness = _Thickness * (1.0 - uv.y) * (0.5 + rnd * 0.5);

            float mask = 1.0 - step(currentThickness, abs(localX - 0.5));
            
            float maxLength = 1.0 - (rnd * _LengthVariation);
            mask *= step(uv.y, maxLength);

            fixed4 c = lerp(_BaseColor, _TipColor, uv.y / maxLength);
            
            o.Albedo = c.rgb;
            o.Alpha = mask; 
            
            o.Metallic = 0.0;
            o.Smoothness = 0.1;
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/VertexLit"
}