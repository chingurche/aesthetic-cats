Shader "Custom/UnderwaterLootInteractive"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        
        [HDR] _RimColor ("Glow Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _RimPower ("Fresnel Power", Range(0.1, 8.0)) = 3.0
        
        _FlashDuration ("Flash Duration (s)", Float) = 0.4
        _Cooldown ("Cooldown / Pause (s)", Float) = 2.0
        _PulseMin ("Pulse Min Intensity", Range(0.0, 1.0)) = 0.1
        
        // --- НОВЫЕ ПАРАМЕТРЫ ДЛЯ СВЯЗИ СО СКРИПТОМ ---
        // Будет меняться от 0 (игрок не смотрит) до 1 (игрок смотрит прямо на предмет)
        _FocusIntensity ("Focus Intensity", Range(0.0, 1.0)) = 0.0
        // Во сколько раз ярче горит предмет при полном фокусе, чем при обычной вспышке
        _FocusMultiplier ("Focus Brightness Multiplier", Float) = 2.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _RimColor;
        float _RimPower;
        
        float _FlashDuration;
        float _Cooldown;
        float _PulseMin;
        
        float _FocusIntensity;
        float _FocusMultiplier;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;

            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));

            // 1. Стандартная логика вспышек (когда игрок НЕ смотрит)
            float totalCycleTime = _FlashDuration + _Cooldown;
            float currentTimeInCycle = fmod(_Time.y, totalCycleTime);
            float pulse = _PulseMin;
            
            if (currentTimeInCycle < _FlashDuration)
            {
                float progress = currentTimeInCycle / _FlashDuration;
                float flashWave = sin(progress * 3.14159265);
                pulse = lerp(_PulseMin, 1.0, flashWave);
            }

            // 2. Логика, когда игрок СМОТРИТ
            // Максимальная яркость при фокусе (1.0 исходный макс * множитель)
            float targetFocusBrightness = 1.0 * _FocusMultiplier; 

            // Плавно смешиваем обычную пульсацию (pulse) и режим фокуса (targetFocusBrightness)
            // на основе значения _FocusIntensity, которое придет из скрипта
            float finalPulse = lerp(pulse, targetFocusBrightness, _FocusIntensity);

            // Выводим финальное свечение
            o.Emission = _RimColor.rgb * pow(rim, _RimPower) * finalPulse;

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}