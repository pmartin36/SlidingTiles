// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "SlidingTiles/DepthTest"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
       
        Pass
        {
			 Tags {
				"RenderType" = "Transparent"
				"Queue" = "Opaque"
			}


			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest Off

            CGPROGRAM	
            #pragma vertex vert
            #pragma fragment frag
			
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color: COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 screenPos: TEXCOORD1;
				float2 scale: TEXCOORD2;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
				o.screenPos = ComputeScreenPos(o.vertex);
				o.scale = _MainTex_TexelSize.zw / _ScreenParams.xy;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(0,0,0,0);
			}
			
            ENDCG
        }

		Pass
		{
			Tags {
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
			}


			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../CommonFunctions.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color: COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 screenPos: TEXCOORD1;
				float2 scale: TEXCOORD2;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;
			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
				o.screenPos = ComputeScreenPos(o.vertex);
				o.scale = _MainTex_TexelSize.zw / _ScreenParams.xy;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv)) , 0, 0, 1);
				//return float4(i.screenPos.xy-i.uv, 0, 1);
				//return float4(tex2D(_CameraDepthTexture, i.uv).rgb, 1);
				float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture, float4(i.uv, 0, 1)).r);
				float depthDiff = i.screenPos.z / depth;
				return float4(depth,depth,depth,1);
			}

			
			ENDCG
		}
    }
}
