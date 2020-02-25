Shader "SlidingTiles/Debug/Fingerprint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Percent("Percent", Range(-0.2,1.2)) = 0
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

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
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;
			float _Percent;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				
				float2 gv = i.uv * 2 - 1;
				float dist = length(gv);
				float angle = frac(atan2(gv.y, gv.x) + _Time.y) * 0.2 - 0.2;

				float fullalpha = smoothstep(1, 0.9, col.a);

				float pd = (_Percent - dist);
				float3 inner_col = hsv2rgb(float3(sin(pd*6.28), 0.2, 1));

				float lVal = smoothstep(0.2, 0.0, pd);
				float3 color = lerp(inner_col, float3(0.5, 0.5, 0.5),  lVal * (1-fullalpha) );
				col.rgb = color;
				col.a *= step(dist + angle, _Percent);

				/*float dpa = dist + angle;
				float smo = smoothstep(_Percent + 0.1, _Percent, dpa);
				col.rgb = lerp(float3(1, 1, 1), color , smo);
				col.a *= smo;*/
				
				return col;
            }
            ENDCG
        }
    }
}
