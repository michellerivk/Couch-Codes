Shader "UI/CardCuteHighlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _HighlightColor ("Highlight Color", Color) = (1.0, 0.92, 0.60, 1.0)
        _HighlightOn ("Highlight On", Range(0, 1)) = 0
        _OutlineSize ("Outline Size", Range(0, 6)) = 2
        _OutlineStrength ("Outline Strength", Range(0, 3)) = 1.2
        _FillBoost ("Fill Boost", Range(0, 1)) = 0.08

        _PulseSpeed ("Pulse Speed", Range(0, 8)) = 2.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.18
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
            float4 _MainTex_TexelSize;
            fixed4 _Color;

            fixed4 _HighlightColor;
            float _HighlightOn;
            float _OutlineSize;
            float _OutlineStrength;
            float _FillBoost;
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uv) * IN.color;

                float alpha = col.a;
                float2 texel = _MainTex_TexelSize.xy * _OutlineSize;

                float a1 = tex2D(_MainTex, IN.uv + float2( texel.x, 0)).a;
                float a2 = tex2D(_MainTex, IN.uv + float2(-texel.x, 0)).a;
                float a3 = tex2D(_MainTex, IN.uv + float2(0,  texel.y)).a;
                float a4 = tex2D(_MainTex, IN.uv + float2(0, -texel.y)).a;

                float a5 = tex2D(_MainTex, IN.uv + float2( texel.x,  texel.y)).a;
                float a6 = tex2D(_MainTex, IN.uv + float2(-texel.x,  texel.y)).a;
                float a7 = tex2D(_MainTex, IN.uv + float2( texel.x, -texel.y)).a;
                float a8 = tex2D(_MainTex, IN.uv + float2(-texel.x, -texel.y)).a;

                float neighborMax = max(max(a1, a2), max(a3, a4));
                neighborMax = max(neighborMax, max(max(a5, a6), max(a7, a8)));

                float outline = saturate(neighborMax - alpha);

                float pulse01 = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);
                float pulse = lerp(1.0 - _PulseAmount, 1.0 + _PulseAmount, pulse01);

                float highlight = _HighlightOn * pulse;

                // Slight brighten on the card itself
                col.rgb *= 1.0 + (_FillBoost * highlight);

                // Soft outer highlight
                col.rgb += _HighlightColor.rgb * outline * _OutlineStrength * highlight;

                return col;
            }
            ENDCG
        }
    }
}