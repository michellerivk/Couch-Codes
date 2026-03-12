Shader "UI/LaptopScreenAliveUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _ScreenCenter ("Screen Center (UV)", Vector) = (0.092, 0.838, 0, 0)
        _ScreenSize ("Screen Size (UV)", Vector) = (0.11, 0.10, 0, 0)
        _ScreenRotation ("Screen Rotation (Degrees)", Range(-45, 45)) = -6
        _ScreenSoftness ("Screen Softness", Range(0.001, 0.05)) = 0.01

        _ScreenTint ("Screen Tint", Color) = (0.82, 0.93, 1.0, 1.0)

        _EmissionStrength ("Emission Strength", Range(0, 1)) = 0.08
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 0.9
        _PulseAmount ("Pulse Amount", Range(0, 0.5)) = 0.03

        _ScanlineStrength ("Scanline Strength", Range(0, 0.2)) = 0.025
        _ScanlineDensity ("Scanline Density", Range(10, 400)) = 140
        _ScanlineSpeed ("Scanline Speed", Range(0, 5)) = 0.4

        _NoiseStrength ("Noise Strength", Range(0, 0.2)) = 0.012
        _NoiseScale ("Noise Scale", Range(10, 300)) = 90
        _NoiseSpeed ("Noise Speed", Range(0, 5)) = 0.6
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

            float4 _ScreenCenter;
            float4 _ScreenSize;
            float _ScreenRotation;
            float _ScreenSoftness;

            fixed4 _ScreenTint;

            float _EmissionStrength;
            float _PulseSpeed;
            float _PulseAmount;

            float _ScanlineStrength;
            float _ScanlineDensity;
            float _ScanlineSpeed;

            float _NoiseStrength;
            float _NoiseScale;
            float _NoiseSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float2 Rotate2D(float2 p, float degrees)
            {
                float r = radians(degrees);
                float s = sin(r);
                float c = cos(r);
                return float2(
                    p.x * c - p.y * s,
                    p.x * s + p.y * c
                );
            }

            float SoftRectMask(float2 uv, float2 center, float2 size, float rotationDeg, float softness)
            {
                float2 local = uv - center;
                local = Rotate2D(local, rotationDeg);

                float2 halfSize = size * 0.5;
                float2 d = abs(local) - halfSize;

                float outside = max(d.x, d.y);
                return 1.0 - smoothstep(0.0, softness, outside);
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.45);
                return frac(p.x * p.y);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uv) * IN.color;

                float screenMask = SoftRectMask(
                    IN.uv,
                    _ScreenCenter.xy,
                    _ScreenSize.xy,
                    _ScreenRotation,
                    _ScreenSoftness
                );

                float t = _Time.y;

                float pulse = 1.0 + sin(t * _PulseSpeed) * _PulseAmount;

                float2 local = Rotate2D(IN.uv - _ScreenCenter.xy, _ScreenRotation);

                float scan = 0.5 + 0.5 * sin((local.y + t * _ScanlineSpeed) * _ScanlineDensity);
                scan *= _ScanlineStrength;

                float noise = Hash21(floor((IN.uv + t * _NoiseSpeed) * _NoiseScale));
                noise = (noise - 0.5) * 2.0 * _NoiseStrength;

                float brightness = _EmissionStrength * pulse + scan + noise;

                col.rgb *= 1.0 + screenMask * 0.10 * pulse;
                col.rgb += _ScreenTint.rgb * screenMask * brightness;

                return col;
            }
            ENDCG
        }
    }
}