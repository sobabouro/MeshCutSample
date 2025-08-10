Shader "Custom/Wireframe" {
	Properties{
		_WireFrameColor("WireFrame Color", Color) = (0,0,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader{
		Tags{ "Queue" = "Transparent" }
		
		// Pass1
		Pass{
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#pragma target 5.0

			sampler2D _MainTex;
			float4 _WireFrameColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
			};

			v2g vert(appdata v)
			{
				v2g o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = _WireFrameColor;

				return o;
			}

			[maxvertexcount(6)]
			void geom(triangle v2g input[3], inout LineStream<g2f> outStream)
			{
				v2g p0 = input[0];
				v2g p1 = input[1];
				v2g p2 = input[2];


				g2f out0;
				out0.pos = p0.vertex;
				out0.uv = p0.uv;
				out0.color = p0.color;

				g2f out1;
				out1.pos = p1.vertex;
				out1.uv = p1.uv;
				out1.color = p1.color;

				g2f out2;
				out2.pos = p2.vertex;
				out2.uv = p2.uv;
				out2.color = p2.color;

				outStream.Append(out0);
				outStream.Append(out1);
				outStream.RestartStrip();

				outStream.Append(out1);
				outStream.Append(out2);
				outStream.RestartStrip();

				outStream.Append(out2);
				outStream.Append(out0);
				outStream.RestartStrip();
			}

			fixed4 frag(g2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}

		// Pass2
		Cull Back
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	}
}
