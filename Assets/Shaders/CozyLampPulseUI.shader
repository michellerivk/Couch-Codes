Shader "UI/CozyLampPulseUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _GlowCenter ("Glow Center (UV)", Vector) = (0.10, 0.78, 0, 0)

        _LampRadius ("Lamp Radius", Range(0.001, 0.5)) = 0.05
        _LampSoftness ("Lamp Softness", Range(0.001, 0.5)) = 0.05
        _LampStrength ("Lamp Strength", Range(0, 1)) = 0.12

        _HaloRadius ("Halo Radius", Range(0.001, 1.0)) = 0.22
        _HaloSoftness ("Halo Softness", Range(0.001, 1.0)) = 0.15
        _HaloStrength ("Halo Strength", Range(0, 1)) = 0.06

        _GlowTint ("Glow Tint", Color) = (1.0, 0.72, 0.35, 1)

        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 0.8
        _PulseAmount ("Pulse Amount", Range(0, 0.5)) = 0.03

        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 1.2
        _FlickerAmount ("Flicker Amount", Range(0, 0.2)) = 0.008

        _ConeOrigin ("Cone Origin (UV)", Vector) = (0.105, 0.69, 0, 0)
        _ConeLength ("Cone Length", Range(0.01, 1.0)) = 0.55
        _ConeStartWidth ("Cone Start Width", Range(0.001, 0.5)) = 0.025
        _ConeEndWidth ("Cone End Width", Range(0.001, 1.0)) = 0.22
        _ConeSoftness ("Cone Softness", Range(0.001, 0.2)) = 0.04
        _ConeStrength ("Cone Strength", Range(0, 1)) = 0.08

    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float4 _GlowCenter;

            float _LampRadius;
            float _LampSoftness;
            float _LampStrength;

            float _HaloRadius;
            float _HaloSoftness;
            float _HaloStrength;

            fixed4 _GlowTint;

            float _PulseSpeed;
            float _PulseAmount;

            float _FlickerSpeed;
            float _FlickerAmount;

            float4 _ConeOrigin;
            float _ConeLength;
            float _ConeStartWidth;
            float _ConeEndWidth;
            float _ConeSoftness;
            float _ConeStrength;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float softCircle(float2 uv, float2 center, float radius, float softness)
            {
                float d = distance(uv, center);
                return 1.0 - smoothstep(radius, radius + softness, d);
            }

            float softCone(float2 uv, float2 origin, float length, float startWidth, float endWidth, float softness)
            {
                float dy = origin.y - uv.y;

                if (dy < 0.0)
                    return 0.0;

                float t = saturate(dy / length);

                float halfWidth = lerp(startWidth, endWidth, t);
                float xDist = abs(uv.x - origin.x);

                float horizontal = 1.0 - smoothstep(halfWidth, halfWidth + softness, xDist);
                float vertical = 1.0 - smoothstep(length, length + softness, dy);

                return horizontal * vertical;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uv) * IN.color;

                float lampMask = softCircle(IN.uv, _GlowCenter.xy, _LampRadius, _LampSoftness);
                float haloMask = softCircle(IN.uv, _GlowCenter.xy, _HaloRadius, _HaloSoftness);

                float t = _Time.y;

                float pulse =
                    1.0 +
                    sin(t * _PulseSpeed) * _PulseAmount;

                float flicker =
                    (sin(t * _FlickerSpeed * 1.73) * 0.5 +
                     sin(t * _FlickerSpeed * 2.41 + 1.7) * 0.35 +
                     sin(t * _FlickerSpeed * 3.87 + 0.9) * 0.15) * _FlickerAmount;

                float coneMask = softCone(
                    IN.uv,
                    _ConeOrigin.xy,
                    _ConeLength,
                    _ConeStartWidth,
                    _ConeEndWidth,
                    _ConeSoftness
                );

                float glow =
                    (lampMask * _LampStrength +
                     haloMask * _HaloStrength +
                     coneMask * _ConeStrength) * (pulse + flicker);

                // Slightly brighten the actual lamp area
                col.rgb *= 1.0 + lampMask * 0.12 * pulse;

                // Warm halo on surrounding wall/floor
                col.rgb += _GlowTint.rgb * glow;

                // Brighten base image insite the cone
                col.rgb *= 1.0 + coneMask * 0.06 * pulse;

                return col;
            }
            ENDCG
        }
    }
}