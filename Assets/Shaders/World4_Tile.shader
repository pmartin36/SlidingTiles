Shader "SlidingTiles/ChalkTile"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ChalkTexture("Chalk Texture", 2D) = "white" {}
		_ChalkPower("Chalk Power", float) = 2
		_ChalkSub("Chalk Sub", float) = 0

		_MobileColor("Color", Color) = (1,1,1,1)
		_ImmobileColor("Immobile Color", Color) = (1,1,1,1)

		_Mobile("Mobile", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent-100"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off

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
				float4 screenPos: TEXTCOORD1;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _ChalkTexture;
			float _ChalkPower;
			float _ChalkSub;
            float4 _MainTex_ST;

			float4 _MobileColor;
			float4 _ImmobileColor;
			float _Mobile;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * lerp(_ImmobileColor, _MobileColor, _Mobile);
				o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				fixed4 col = tex2D(_MainTex, i.uv);
				float4 chalk = tex2D(_ChalkTexture, i.screenPos * 10);

				float alpha = col.a;
				alpha *= lerp(pow(saturate(chalk.r-_ChalkSub), _ChalkPower), 0.8, min(0.5, col.r));
				return float4(i.color.rgb * col.b, alpha);
            }
            ENDCG
        }
    }
}
