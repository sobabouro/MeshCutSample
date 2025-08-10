Shader "Custom/FrontBlueBackRedShader"
{
    Properties
    {
        _ColorFront ("Front Color", Color) = (0, 0, 1, 1) // 青
        _ColorBack ("Back Color", Color) = (1, 0, 0, 1)   // 赤
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off // 両面描画

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
            };

            fixed4 _ColorFront;
            fixed4 _ColorBack;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                if (!isFrontFace)
                {
                    return _ColorBack; // 裏面は赤
                }

                // 表面は法線の Z 成分に応じて青強度を固定で調整
                float blue = saturate(i.worldNormal.z);
                return lerp(fixed4(0, 0, 0.2, 1), _ColorFront, blue);
            }
            ENDCG
        }
    }
}