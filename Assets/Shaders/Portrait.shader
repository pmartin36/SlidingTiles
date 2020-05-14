Shader "SlidingTiles/Portrait"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_OverridePct("Override Percent", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}

        Pass
        {
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

            CGPROGRAM	
            #pragma vertex vert
            #pragma fragment frag
			
            #include "UnityCG.cginc"
			#define CELLSIZE 341

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color: COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float4 _Color;
			float _OverridePct;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv) * i.color;
				col.rgb = lerp(col.rgb, _Color, _OverridePct);

				// convert from 0 to 1 from sprite sheet uv
				float2 normalizedUv = i.uv * _MainTex_TexelSize.zw;
				float2 iuv = floor(normalizedUv / CELLSIZE);
				normalizedUv = frac(normalizedUv / CELLSIZE);

				// convert uv to -1 to 1
				float2 uv = normalizedUv * 2 - 1;
				float len = length(uv);
				float a = smoothstep(_OverridePct * 3, _OverridePct, len) * 0.2;
				
				float diff = step(col.a, a);
				col.rgb += float3(1, 1, 1) * diff;
				col.a = max(col.a, a);

				return col;
            }
            ENDCG
        }
    }
}
