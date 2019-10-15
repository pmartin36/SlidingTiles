Shader "SlidingTiles/RowWinScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_SecondaryColor("Selection Color", Color) = (0.5,0,0,1)

		_PctDebug("Pct", Range(0,1)) = 0
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
				fixed4 primaryColor : COLOR0;
				fixed4 secondaryColor : COLOR1;
            };

            sampler2D _MainTex;
			sampler2D _Noise;
            float4 _MainTex_ST;
			float4 _Color;
			float4 _SecondaryColor;	

			float _PctDebug;
			float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.primaryColor = v.color * _Color;
				o.secondaryColor = v.color * _SecondaryColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				float boxSize = 150.0;

				float2 offset = fmod(_MainTex_TexelSize.zw, boxSize) / 2.0;

				float row = floor(i.uv.y / boxSize);
				float direction = sign(fmod(row, 2.0) - 0.5);

				float2 uv = i.uv - offset;
				float2 startingPosition = float2(uv.x - direction * _MainTex_TexelSize.z, uv.y);
				//float2 samplePosition = lerp(startingPosition, fragCoord, clamp(_Time.x, 0, 1));
				float2 samplePosition = lerp(startingPosition, uv, _PctDebug);

				// float mm = step(iResolution.x, samplePosition.x);
				float mm = step(_MainTex_TexelSize.z + boxSize / 2.0, samplePosition.x) + (1. - step(-boxSize, samplePosition.x));
				//fragColor = vec4(step(iResolution.x + boxSize/2.0, samplePosition.x), 1. - step(-boxSize, samplePosition.x), 0., 1.);
				//return;
				
				uv = fmod(samplePosition, boxSize.xx) / boxSize;
				uv.x = lerp(uv.x, 1, mm);

				// Time varying pixel color
				float maxVal = smoothstep(0.45, 0.5, max(abs(uv.x - 0.5), abs(uv.y - 0.5)));

				float3 col = saturate(float3(0.75, 0.75, 0.25) - maxVal * 0.05);
				// Output to screen
				return float4(col, 1);
            }
            ENDCG
        }
    }
}
