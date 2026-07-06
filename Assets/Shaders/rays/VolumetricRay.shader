Shader "Custom/VolumetricRay"
{
    Properties
    {
        [HDR] _Color ("Top Color (Script Controlled)", Color) = (0.5, 0.8, 1.0, 0.2)
        [HDR] _DeepColor ("Deep Water Color", Color) = (0.05, 0.2, 0.5, 0.0)
        
        _Speed ("Ripple Speed", Float) = 1.2
        _Stripes ("Stripe Density", Float) = 12.0
        _Sharpness ("Caustic Sharpness", Range(1.0, 10.0)) = 4.0
        
        _RandomSeed ("Random Seed", Float) = 0.0
        
        _DepthFade ("Depth Softness", Range(0.01, 5.0)) = 2.0
        _CamFadeDistance ("Camera Fade Distance", Float) = 3.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog 
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD1;
                float4 projPos : TEXCOORD2; 
                UNITY_FOG_COORDS(3) 
            };

            float4 _Color;
            float4 _DeepColor;
            float _Speed;
            float _Stripes;
            float _Sharpness;
            float _RandomSeed;
            float _DepthFade;
            float _CamFadeDistance;
            
            sampler2D_float _CameraDepthTexture; 

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                o.projPos = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float uniqueOffset = _RandomSeed + i.worldPos.x * 0.2 + i.worldPos.z * 0.2;

                float centeredX = i.uv.x * 2.0 - 1.0;
                float volumeThickness = sqrt(max(0.0, 1.0 - centeredX * centeredX));
                float edgeFade = pow(volumeThickness, 1.5); 

                float topFade = smoothstep(1.0, 0.8, i.uv.y);

                float ripple1 = abs(sin(i.uv.x * _Stripes + _Time.y * _Speed + uniqueOffset));
                float ripple2 = abs(sin(i.uv.x * _Stripes * 0.6 - i.uv.y * 4.0 + _Time.y * _Speed * 0.8 - uniqueOffset));
                
                float stripes = 1.0 - (ripple1 * ripple2);
                stripes = pow(stripes, _Sharpness);

                float macroPulse = sin(i.uv.x * 3.0 - _Time.y * 0.5 + uniqueOffset) * 0.5 + 0.5;
                stripes *= lerp(0.4, 1.0, macroPulse);

                float4 rayGradientColor = lerp(_DeepColor, _Color, i.uv.y);

                float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float depthFade = saturate((sceneZ - i.projPos.z) / _DepthFade);
                float camFade = smoothstep(0.0, _CamFadeDistance, i.projPos.z);

                float alpha = edgeFade * topFade * stripes * depthFade * camFade * i.color.a;
                fixed4 finalColor = rayGradientColor * alpha;

                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalColor, fixed4(0,0,0,0));

                return finalColor;
            }
            ENDCG
        }
    }
}