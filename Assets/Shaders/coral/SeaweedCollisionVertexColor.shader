Shader "Custom/SeaweedCollisionVertexColor"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        
        [Header(Surface Settings)]
        _Smoothness ("Base Smoothness", Range(0,1)) = 0.1
        
        [Header(Procedural Texture (Noise))]
        _NoiseScale ("Noise Scale", Float) = 25.0
        _NoiseStrength ("Noise Intensity", Range(0,1)) = 0.6
        _NoiseColorBlend ("Noise Darken Tint", Color) = (0.4, 0.4, 0.4, 1.0)

        [Header(Mesh Setup Root Fix)]
        _BottomY ("Root Local Y", Float) = 0.0
        _TopY ("Top Local Y", Float) = 2.0
        
        [Header(Sway Settings)]
        _SwaySpeed ("Sway Speed", Float) = 2.0
        _SwayAmount ("Sway Amount", Float) = 0.2
        
        [Header(Base Curve Settings)]
        _BaseBendDir ("Base Bend Direction (X,Z)", Vector) = (1, 0, 0, 0)
        _BendStrength ("Curve Strength", Float) = 0.5
        
        [Header(Collision Settings)]
        _CollisionRadius ("Collision Radius", Float) = 2.0
        _PushStrength ("Push Strength", Float) = 1.5
        
        _WaterDrag ("Water Drag", Range(0.1, 4.0)) = 1.8
        
        [Header(Optimization)]
        _AnimDistance ("Max Anim Distance", Float) = 40.0
    }
    SubShader
    {
        Cull Off
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0
        #pragma multi_compile_instancing

        sampler2D _MainTex;
        fixed4 _Color;
        half _Cutoff;
        
        float _Smoothness;
        
        float _NoiseScale;
        float _NoiseStrength;
        fixed4 _NoiseColorBlend;
        
        float _BottomY;
        float _TopY;
        
        float _SwaySpeed;
        float _SwayAmount;
        
        float4 _BaseBendDir;
        float _BendStrength;
        
        float _CollisionRadius;
        float _PushStrength;
        float _WaterDrag; 
        
        float4 _UnitPosition;
        float _AnimDistance;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR; 
            float3 staticWorldPos; 
        };

        float hash(float3 p) {
            return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * 43758.5453);
        }

        float noise3D(float3 x) {
            float3 p = floor(x);
            float3 f = frac(x);
            f = f * f * (3.0 - 2.0 * f);

            float h000 = hash(p + float3(0,0,0));
            float h100 = hash(p + float3(1,0,0));
            float h010 = hash(p + float3(0,1,0));
            float h110 = hash(p + float3(1,1,0));
            float h001 = hash(p + float3(0,0,1));
            float h101 = hash(p + float3(1,0,1));
            float h011 = hash(p + float3(0,1,1));
            float h111 = hash(p + float3(1,1,1));

            float lerp00 = lerp(h000, h100, f.x);
            float lerp01 = lerp(h010, h110, f.x);
            float lerp10 = lerp(h001, h101, f.x);
            float lerp11 = lerp(h011, h111, f.x);

            float lerp0 = lerp(lerp00, lerp01, f.y);
            float lerp1 = lerp(lerp10, lerp11, f.y);

            return lerp(lerp0, lerp1, f.z);
        }

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            o.staticWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

            float scaleXZ = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));

            float heightMask = saturate((v.vertex.y - _BottomY) / (_TopY - _BottomY));
            float curveMask = heightMask * heightMask;

            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            
            float3 rootWorldPos = mul(unity_ObjectToWorld, float4(0, _BottomY, 0, 1)).xyz;
            float rootWorldY = rootWorldPos.y; 
            
            float camDist = distance(_WorldSpaceCameraPos, rootWorldPos);
            float animFade = saturate(1.0 - (camDist / max(_AnimDistance, 0.1)));

            float3 totalOffset = float3(_BaseBendDir.x, 0, _BaseBendDir.z) * (_BendStrength * scaleXZ) * curveMask;

            if (animFade > 0.001)
            {
                float time = _Time.y * _SwaySpeed;
                
                float actualSway = _SwayAmount * scaleXZ;
                float3 swayVector = float3(
                    sin(time + worldPos.x) * actualSway,
                    0, 
                    cos(time + worldPos.z) * actualSway
                );

                float2 diffXZ = rootWorldPos.xz - _UnitPosition.xz;
                float dist = length(diffXZ);

                float2 pushDir = diffXZ / max(dist, 0.001);
                
                float dodgeSide = (frac(sin(dot(rootWorldPos.xz, float2(12.9898, 78.233))) * 43758.5453) > 0.5) ? 1.0 : -1.0;
                
                float2 tangentDir = float2(-pushDir.y, pushDir.x) * dodgeSide;
                
                float partingStrength = smoothstep(1.2 * scaleXZ, 0.0, dist); 
                pushDir = normalize(pushDir + tangentDir * partingStrength * 1.5);

                float actualRadius = _CollisionRadius * scaleXZ;
                float rawStrength = saturate(1.0 - (dist / max(actualRadius, 0.001)));
                
                float strength = pow(rawStrength, 1.0 / max(_WaterDrag, 0.01)); 
                
                float actualPush = _PushStrength * scaleXZ;
                float3 collisionVector = float3(pushDir.x, 0, pushDir.y) * strength * actualPush;

                totalOffset += (swayVector + collisionVector) * heightMask * animFade;

                float h = max(0.001, worldPos.y - rootWorldY); 
                float pushAmount = length(totalOffset.xz);
                
                float2 pushDirXZ = totalOffset.xz / max(pushAmount, 0.0001);
                float angle = min(pushAmount / h, 1.57079);
                
                float newRadius = h * sin(angle);
                totalOffset.xz = pushDirXZ * newRadius;
                
                float newH = h * cos(angle);
                totalOffset.y -= (h - newH); 
            }

            worldPos.xyz += totalOffset;
            v.vertex = mul(unity_WorldToObject, worldPos);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color * IN.color;
            clip(c.a - _Cutoff);
            
            float noiseVal = noise3D(IN.staticWorldPos * _NoiseScale);
            
            c.rgb = lerp(c.rgb, c.rgb * _NoiseColorBlend.rgb, noiseVal * _NoiseStrength);
            
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            
            o.Smoothness = lerp(_Smoothness, 0.0, noiseVal * _NoiseStrength);
        }
        ENDCG
    }
    FallBack "Diffuse"
}