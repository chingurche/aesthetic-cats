Shader "Custom/SeaweedCollision"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        
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
        
        // Replaces Elasticity. Creates water resistance effect. Recommended: 1.5 - 2.5
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
        };

        void vert (inout appdata_full v)
        {
            float heightMask = saturate((v.vertex.y - _BottomY) / (_TopY - _BottomY));
            float curveMask = heightMask * heightMask;

            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            
            float3 rootWorldPos = mul(unity_ObjectToWorld, float4(0, _BottomY, 0, 1)).xyz;
            float rootWorldY = rootWorldPos.y; 
            
            float camDist = distance(_WorldSpaceCameraPos, rootWorldPos);
            float animFade = saturate(1.0 - (camDist / max(_AnimDistance, 0.1)));

            float3 totalOffset = float3(_BaseBendDir.x, 0, _BaseBendDir.z) * _BendStrength * curveMask;

            if (animFade > 0.001)
            {
                float time = _Time.y * _SwaySpeed;
                float3 swayVector = float3(
                    sin(time + worldPos.x) * _SwayAmount,
                    0, 
                    cos(time + worldPos.z) * _SwayAmount
                );

                float2 diffXZ = rootWorldPos.xz - _UnitPosition.xz;
                float dist = length(diffXZ);

                // --- FIX ROTATION AND "PARTING" EFFECT ---
                // Always bend AWAY from the player first
                float2 pushDir = diffXZ / max(dist, 0.001);
                
                // Decide uniquely for each plant whether to dodge left or right (-1.0 or 1.0)
                float dodgeSide = (frac(sin(dot(rootWorldPos.xz, float2(12.9898, 78.233))) * 43758.5453) > 0.5) ? 1.0 : -1.0;
                
                // Create a tangent vector (perpendicular to the push direction)
                float2 tangentDir = float2(-pushDir.y, pushDir.x) * dodgeSide;
                
                // If the player is very close to the center, smoothly blend in the dodge vector.
                // This makes the seaweed slide gracefully to the side of the player, completely eliminating the 180-spin.
                float partingStrength = smoothstep(1.2, 0.0, dist); 
                pushDir = normalize(pushDir + tangentDir * partingStrength * 1.5);


                // --- FIX RUBBER BAND EFFECT (Thick Water Drag) ---
                float rawStrength = saturate(1.0 - (dist / _CollisionRadius));
                
                // Division by _WaterDrag creates a root effect.
                // This forces the strength to remain high for longer and fade to absolute zero 
                // incredibly smoothly at the very edge of the radius.
                float strength = pow(rawStrength, 1.0 / max(_WaterDrag, 0.01)); 
                
                float3 collisionVector = float3(pushDir.x, 0, pushDir.y) * strength * _PushStrength;

                totalOffset += (swayVector + collisionVector) * heightMask * animFade;

                // Volume preservation (Arc Bending)
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
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            clip(c.a - _Cutoff);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}