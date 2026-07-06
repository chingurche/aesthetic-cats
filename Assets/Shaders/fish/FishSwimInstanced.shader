Shader "Custom/FishSwimInstanced"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo RGB", 2D) = "white" {}
        
        [Header(Swim Animation)]
        _SwimSpeed ("Swim Speed", Float) = 15.0
        _SwimFrequency ("Wave Frequency", Float) = 3.0
        _SwimAmplitude ("Swim Amplitude", Float) = 0.1
        
        _MaskOffset ("Mask Offset", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _SwimSpeed;
        half _SwimFrequency;
        half _SwimAmplitude;
        half _MaskOffset;
        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v)
        {
            UNITY_SETUP_INSTANCE_ID(v);

            float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            float randomOffset = worldPos.x + worldPos.y + worldPos.z;

            float wave = sin(_Time.y * _SwimSpeed - v.vertex.x * _SwimFrequency + randomOffset);
            
            float mask = abs(v.vertex.x - _MaskOffset);

            v.vertex.z += wave * _SwimAmplitude * mask;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}