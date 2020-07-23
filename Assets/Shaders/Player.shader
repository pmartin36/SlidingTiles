Shader "SlidingTiles/Player"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_DistortRadius("Distort Radius", Range(0,1)) = 1
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
			float _DistortRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				//float2 testUv = i.uv;
				//testUv *= (_MainTex_TexelSize.zw / 340) + 1; //6;
				//testUv.y -= 900 / _MainTex_TexelSize.w;
				//return float4(_MainTex_TexelSize.zw/4500, 0, 1);
				//return float4(testUv, 0, 1);

				// convert from 0 to 1 from sprite sheet uv
				float2 normalizedUv = i.uv * _MainTex_TexelSize.zw;
				float2 iuv = floor(normalizedUv / CELLSIZE);
				normalizedUv = frac(normalizedUv / CELLSIZE);

				// convert uv to -1 to 1
				float2 uv = normalizedUv * 2 - 1;

				// robot is taller than wider, scale radius by height proportionately
				float uvyFactor = max(1, 1.5 * smoothstep(1.2, 0.5, _DistortRadius));
				float originalRadius = length(uv);
				float radius = length(float2(uv.x, uv.y * uvyFactor));

				float diff = (radius - _DistortRadius);
				float smushRadius = (1 - _DistortRadius);

				// convert to polar
				float theta = atan2(uv.y, uv.x);

				// squash the radius
				float smushStart = saturate(_DistortRadius - smushRadius);
				float smooth = smoothstep(smushStart, _DistortRadius, radius);
				radius = lerp(radius, 1.5, smooth);

				// convert back to rectangular
				uv = radius * float2(cos(theta), sin(theta));
				
				// convert back to 0 to 1
				uv = (uv + 1) / 2;
				uv = saturate(uv);

				// convert back to sprite sheet uv
				uv = (uv + iuv) * CELLSIZE / _MainTex_TexelSize.zw;

				// sample tex and modify color
				float4 col = tex2D(_MainTex, uv);
				col.rgb = lerp(float3(1, 1, 1), col.rgb, smoothstep(0.6, 1, _DistortRadius));

				// circle growing during smush
				col += float4(1, 1, 1, 1) * smoothstep(smushRadius, smushRadius-0.1, originalRadius+0.05);

				// debug
				//col = lerp(col, float4(1,1,1,1), smoothstep(0.02, 0.0, abs(diff)));

				return col * i.color;
            }
            ENDCG
        }
    }
}
