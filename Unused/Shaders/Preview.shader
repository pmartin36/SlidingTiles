Shader "SlidingTiles/Preview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_PreviewWorldPosition("Preview World Space Position", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent+1"
		}

		GrabPass
		{
			"_BackgroundTexture"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

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
				float4 grabPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _BackgroundTexture;
			float4 _Color;
			float4 _PreviewWorldPosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float4 scale = float4(
					length(UNITY_MATRIX_M._m00_m01_m02),
					length(UNITY_MATRIX_M._m10_m11_m12),
					length(UNITY_MATRIX_M._m20_m21_m22),
					1 / v.vertex.w
				);

				float4 vScaled = v.vertex * scale;
				float4 samplePosition = _PreviewWorldPosition + float4(35,0,0,0) + vScaled; // no idea why we have to add 35

				o.grabPos = mul(UNITY_MATRIX_VP, samplePosition);
				o.grabPos = ComputeGrabScreenPos(o.grabPos);

				o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				fixed4 gp = tex2D(_BackgroundTexture, i.grabPos.xy);
				gp *= i.color;
				return gp;
            }
            ENDCG
        }
    }
}
