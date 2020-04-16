Shader "SlidingTiles/SelectedTile"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_HighlightTex("Highlight Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)	
		_SelectionColor("Selection Color", Color) = (0.5,0,0,1)
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
			#include "./CommonFunctions.cginc"

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
			sampler2D _Noise;
			sampler2D _HighlightTex;
            float4 _MainTex_ST;
			float4 _Color;
			float4 _SelectionColor;	

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*fixed4 col = tex2D(_MainTex, i.uv);
				float len = length((i.uv) * 2 - 1);
				fixed4 noise = (tex2D(_Noise, i.uv) * 2 - 1) ;
				col.rgb = lerp(
					col.rgb, 
					max(_SelectionColor.rgb, col.rgb),
					smoothstep(
						0.9, 
						1.4, 
						len + sin(_Time.y * 2) * 0.2 + noise * 0.2
					)
				);*/
				float4 highlightTex = tex2D(_HighlightTex, i.uv);
				float4 col = highlightTex;

				float2 uv = i.uv * 2 - 1;
				float theta = (atan2(uv.y, uv.x) / 3.1415 + 1) / 2; //0 to 1
				theta = frac(theta * 4);
				float target = frac(-_Time.y*2);

				float len = length(uv) * 2;
				float diffFromTarget = abs(theta - target);
				diffFromTarget = min(1 - diffFromTarget, diffFromTarget);
				float s = smoothstep(0.25, 0.025, diffFromTarget / len);
				//col.a = min(col.a, s);
				//col.a = max(col.a, highlightTex.a);

				col.rgb = lerp(_SelectionColor, _Color, smoothstep(0, 1, s));
				
				float notAlreadyFilledAlpha = 0.09;
				float notAlreadyFilled = 1 - step(0, col.a - notAlreadyFilledAlpha);
				col.rgb = lerp(
					col.rgb, 
					float3(1, 1, 1), 
					notAlreadyFilled
				);
				
				fixed maintexAlpha = tex2D(_MainTex, i.uv).a;
				col.a += maintexAlpha * notAlreadyFilledAlpha * notAlreadyFilled;
				return col;
            }
            ENDCG
        }
    }
}
