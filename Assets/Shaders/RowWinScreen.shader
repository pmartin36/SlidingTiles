Shader "SlidingTiles/RowWinScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_SecondaryColor("Selection Color", Color) = (0.5,0,0,1)

		_AnimationPercent("Animation Percent", Range(0,1)) = 0
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

			float inverseLerp(float a, float b, float v) {
				return (v - a) / (b - a);
			}

			float2 rotate(float2 o, float r) {
				float c = cos(r);
				float s = sin(r);
				return float2(o.x * c - o.y * s, o.x * s + o.y * c);
			}

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
				fixed4 color : COLOR0;
				fixed4 secondaryColor : COLOR1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;
			float4 _SecondaryColor;	

			float _AnimationPercent;
			float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
				o.secondaryColor = v.color * _SecondaryColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				float boxSize = 150.0;		
				float2 dim = _ScreenParams.xy;
				float2 uv = i.uv * dim;

				// if the box size doesn't fit perfectly in each direction, center the boxes
				float2 offset = fmod(dim, boxSize) / 2.0;
				uv -= offset;

				float row = floor(uv.y / boxSize);			

				// adjacent rows should move in opposite directions
				float direction = sign(fmod(row, 2.0) - 0.5);

				float2 startingPosition = float2(uv.x - direction * dim.x, uv.y);
				float2 samplePosition = lerp(startingPosition, uv, _AnimationPercent);

				float inverseAlpha = step(dim.x - offset.x, samplePosition.x) + (1. - step(-offset.x, samplePosition.x));
				samplePosition += boxSize; // fmod doesn't work with numbers < 0 (but we don't want it to effect alpha)
				uv = fmod(samplePosition, boxSize.xx) / boxSize;

				float maxVal = smoothstep(0.3, 0.5, max(abs(uv.x - 0.5), abs(uv.y - 0.5)));

				float2 rowColumn = floor(samplePosition / boxSize);
				float colorinterpolator = fmod(rowColumn.x + rowColumn.y, 2);
				float3 color = lerp(i.color.rgb, i.secondaryColor.rgb, colorinterpolator);

				float3 tex = tex2D(_MainTex, uv).rgb;
				float3 col = saturate(tex * color - maxVal * 0.05);
				// Output to screen
				return float4(col, 1 - inverseAlpha);
            }
            ENDCG
        }
    }
}
