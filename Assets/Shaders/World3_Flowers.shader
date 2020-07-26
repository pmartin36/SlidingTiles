Shader "SlidingTiles/World3_Flowers"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}

		_Seed("Seed", float) = 1
		_Size("Size", Range(1, 50)) = 20

		[Toggle(DEBUG)]
		_Debug("Debug", Float) = 0

		[Header(Wind)]
		_WindDirectionAngle("Wind Direction", Range(0, 6.28)) = 0
		_WindFrequency("Wind Frequency", float) = 0.05
		_WindStrength("Wind Strength", Float) = 1
    }

    SubShader
    {
		Tags
		{
			"RenderType" = "Transparent"
			"Queue"="Transparent"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature DEBUG

			#include "UnityCG.cginc"
			#include "CommonFunctions.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			sampler2D _CameraDepthTexture;
			float _Seed;
			float _Size;
			float _WindDirectionAngle;
			float _WindFrequency;
			float _WindStrength;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color: COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 screenToTexScale: TEXCOORD1;
				float2 texScale: TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// texScale.y will always == 1, texScale.x will be > 1 if texture height > texture width (true with current background texture)
				o.texScale = _MainTex_TexelSize.y / _MainTex_TexelSize.xy; 

				// get transform x/y/z scale - since background is always scaled evenly across 3 axis, just sample x
				//float scale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)); 
				//o.screenToTexScale = (_MainTex_TexelSize.zw * scale) / _ScreenParams.xy / 16.5;
				//o.screenToTexScale = 1 + ((_MainTex_TexelSize.zw * scale) / _ScreenParams.xy / 100);

				// I would prefer not to use this approach as it hardcodes 1.2 (the size of the background is 1.2x the camera size)
				// but can release with this if it works on all devices.
				o.screenToTexScale = float2(1, (_ScreenParams.x / _ScreenParams.y) / o.texScale.x) * 1.2;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 uv = i.uv * i.texScale;	// square boxes
				uv *= _Size;
				float2 guv = frac(uv) - 0.5;
				float2 id = floor(uv);			

				float random = N21(id * _Seed);

				float2 cellMovement = frac(random * float2(422.4, 156.3)) * 0.5 - 0.25;
				float2 gv_no_wind = guv - cellMovement;

				float2 sampleuv = ((id + 0.5 + cellMovement) / _Size) / i.texScale;
				sampleuv = ((sampleuv * 2 - 1) * i.screenToTexScale + 1) / 2;
				float depth = tex2D(_CameraDepthTexture, sampleuv).r;
				#if defined(UNITY_REVERSED_Z)
					depth = 1.0 - depth;
				#endif

				#if DEBUG
					float4 debug = float4(0, 0, 0, 1);
					if (guv.x > 0.48 || guv.y > 0.48) debug.r = 1;
					debug.r += smoothstep(0.1, 0.0, length(gv_no_wind));
					debug.b = tex2D(_CameraDepthTexture, ((i.uv * 2 - 1) * i.screenToTexScale + 1) / 2).r;
					debug.g = depth;	
					debug.a = 0.5;
					return debug;
				#endif

				// how much will the wind move the flower position
				float2 windDirection = normalize(float2(cos(_WindDirectionAngle), sin(_WindDirectionAngle)));
				float windOffsetTime = dot(-windDirection, id);
				float windStrength = (windDirection * 0.4 + 0.6 * pow(cos(_Time.y * _WindFrequency + windOffsetTime) * cos(_Time.y * _WindFrequency * 2 + windOffsetTime), 2)) * _WindStrength;
				float2 windOffset = windStrength * windDirection;
				// windOffset = -lerp(windOffset, float2(0, 0), (gv_no_wind - windOffset) * 2);
				float2 gv = gv_no_wind+windOffset;

				// what color
				float3 flowerColor = frac(random * float3(267.9, 113.2, 472.1)) * 0.6 + 0.4;

				// make petals
				float theta = atan2(gv.y, gv.x);
				float theta01 = (theta + 3.14159) / 6.282;
				float r = length(gv);

				// how big is the flower
				float sides = 2 + round(random * 3);
				float width = 0.075 + frac(random * 224.3) * 0.05;
				float variation = width * (0.25 + frac(random * 524.3) * 0.25);
				float bigCenterSize = (width - variation);
				float fc = abs(cos(theta*sides + 3.14 / 2.) * variation) + bigCenterSize;

				// how big is the center
				float centerSize = bigCenterSize * (0.5 + frac(random * 325.3) * 0.25);

				// culling due to wind
				float wdg = dot(-windOffset*10, gv);
				float windCull = max(0.7, smoothstep(0.3, 0.0, wdg)) + (0.95 * min(smoothstep(0.0, -0.3, wdg), 0.8));

				// petals
				float3 petalColor = lerp(float3(0.2, 0.2, 0.2), flowerColor, smoothstep(centerSize - 0.01, centerSize + 0.01, r)) * windCull;
				float alpha = smoothstep(r, r + 0.01, fc);

				// stem
				float windOffsetLen = saturate(dot(gv, windOffset) / dot(windOffset, windOffset));
				float distFromStemCenter = length(gv - windOffsetLen * windOffset);
				float stemAlpha = (1 - alpha) * smoothstep(0.02, 0, distFromStemCenter);

				// shadow
				float shadowWidth = width * 2.5;
				float shadowAlpha = smoothstep(shadowWidth, -0.4, r) * (1 - alpha);

				float3 color = petalColor * alpha + float3(0, 0, 0) * shadowAlpha + float3(0.05, 0.41, 0) * stemAlpha;

				float allColor = max(max(alpha, stemAlpha), shadowAlpha);
				float noFlower = step(0.5, frac(random * 321.3)) * step(0.5, depth); // 50% of spaces missing flowers, anything with flower center covered should not show
				float4 col = float4(color, allColor * noFlower);

				// if (guv.x > 0.48 || guv.y > 0.48) col = float4(1, 0, 0, 1);
				return col;
			}
			ENDCG
		}
    }	
}