Shader "SlidingTiles/World3_Bottom_Border"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		[Toggle(FLOWERS)]
		_ShowFlowers("Show Flowers", Float) = 0

		_Size("Size", Range(1, 50)) = 20

		[Header(Wind)]
		_WindDirectionAngle("Wind Direction", Range(0, 6.28)) = 0
		_WindFrequency("Wind Frequency", float) = 0.05
		_WindStrength("Wind Strength", Float) = 1

		[Header(Background)]
		_BackgroundTex("Background", 2D) = "white" {}
		_SquiggleTex("Squiggle", 2D) = "white" {}
		_PrimaryColor("Primary Background Color", Color) = (1,1,1,1)
		_SecondaryColor("Secondary Background Color", Color) = (1,1,1,1)
	}

		SubShader
		{
			Tags
			{
				"RenderType" = "Transparent"
				"Queue" = "Transparent-10"
			}

			Pass
			{
				ZWrite Off
				ZTest Off
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma shader_feature FLOWERS

				#include "UnityCG.cginc"
				#include "CommonFunctions.cginc"

				float4 _PrimaryColor;
				float4 _SecondaryColor;
				sampler2D _BackgroundTex;
				sampler2D _SquiggleTex;

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;

				sampler2D _CameraDepthTexture;
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
					float2 screenPos: TEXCOORD1;
					float2 texScale: TEXCOORD2;
					float2 screenToTexScale: TEXCOORD3;
					float4 vertex : SV_POSITION;
					fixed4 primaryColor : COLOR0;
					fixed4 secondaryColor : COLOR1;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.primaryColor = v.color * _PrimaryColor;
					o.secondaryColor = v.color * _SecondaryColor;
					o.screenPos = ComputeScreenPos(o.vertex);

					float xScale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
					o.screenToTexScale = (_MainTex_TexelSize.zw * xScale) / _ScreenParams.xy / 3.5;
					o.texScale = _MainTex_TexelSize.y / _MainTex_TexelSize.xy;

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					/*float grass_start = 0.275;

					float n = N21(i.screenPos);
					float darkness = max(n, frac(n*183.2));
					darkness = tex2D(_Noise, i.screenPos).r * 0.7 + 0.3 * darkness;
					float value = lerp(0.4, 0.75, smoothstep(0.35, 1.4, darkness));

					float3 grass_primary = rgb2hsv(i.primaryColor);
					grass_primary.z = value - 0.075;
					grass_primary = hsv2rgb(grass_primary);

					float3 grass_secondary = rgb2hsv(i.secondaryColor);
					grass_secondary.z = value;
					grass_secondary = hsv2rgb(grass_secondary);

					float noise = tex2D(_Noise, i.screenPos).r * 0.6;
					noise += tex2D(_Noise, i.screenPos * 5).r * 0.2;

					float3 tertiary = float3(0.75, 0.63, 0.45);
					float3 darkTertiary = float3(0.35, 0.21, 0.02) * 1.5;
					float3 dirt = lerp(darkTertiary, tertiary, smoothstep(0, grass_start, noise - 0.15));

					float grassLerp = smoothstep(grass_start, 0.7, noise);
					float3 grass = lerp(grass_primary, grass_secondary, grassLerp);

					float3 val = lerp(dirt, grass, smoothstep(grass_start - 0.05, grass_start + 0.05, noise));*/

					float n = N21(i.screenPos);
					float noise = tex2D(_BackgroundTex, i.screenPos / 3).r;
					noise = pow(noise + 0.4, 4);
					float3 val = lerp(i.primaryColor, i.secondaryColor, noise);

					// grass lines
					float2 uv = float2(i.screenPos.x, -i.uv.y) * _ScreenParams.y / _ScreenParams.xy;
					uv.x *= _Size * 2;
					float2 id = floor(uv);
					float random = N21(id);
					float2 guv = frac(uv);// *(0.5 + random * 0.5);

					float2 gv = guv + frac(random * float2(422.4, 156.3)) * 0.5 - 0.25;

					float num_squiggle_rows = 2;
					float num_squiggle_columns = 4;
					float2 squiggle_start_uv = float2(
						floor(frac(random * 242.7) * num_squiggle_columns) / num_squiggle_columns,
						floor(frac(random * 197.4) * num_squiggle_rows) / num_squiggle_rows
					);
					float2 squiggle_uv = squiggle_start_uv + gv / float2(num_squiggle_columns, num_squiggle_rows);
					float4 squiggle = tex2D(_SquiggleTex, squiggle_uv) * 0.6;

					float noGrass = step(0.3, frac(random * 321.3));
					val = lerp(val, squiggle.rgb, squiggle.a * noGrass);

					float4 map = tex2D(_MainTex, i.uv);

					float g = (map.g - 0.5) * 2;
					g -= lerp(0, 0.95, g); // remap only positive numbers from 0 to 0.2
					val.rgb += g;

					float3 shade = float3(0, 0, 0);
					val = lerp(map.rgb, val, map.r);

					#if FLOWERS
						uv = i.uv * i.texScale;	// square boxes
						uv *= _Size;
						guv = frac(uv) - 0.5;
						id = floor(uv);

						random = N21(id);

						float2 cellMovement = frac(random * float2(422.4, 156.3)) * 0.5 - 0.25;
						float2 gv_no_wind = guv - cellMovement;

						float2 sampleuv = ((id + 0.5 + cellMovement) / _Size) / i.texScale;
						sampleuv = ((sampleuv * 2 - 1) * i.screenToTexScale + 1) / 2;
						float depth = tex2D(_CameraDepthTexture, sampleuv).r;

						// float4 debug = float4(0, 0, 0, 1);
						// if (guv.x > 0.48 || guv.y > 0.48) debug.r = 1;
						// debug.r += smoothstep(0.1, 0.0, length(gv_no_wind));
						// debug.b = tex2D(_CameraDepthTexture, ((i.uv * 2 - 1) * i.screenToTexScale + 1) / 2).r;
						// debug.g = depth;	
						// return debug;

						// how much will the wind move the flower position
						float2 windDirection = normalize(float2(cos(_WindDirectionAngle), sin(_WindDirectionAngle)));
						float windOffsetTime = dot(-windDirection, id);
						float windStrength = (windDirection * 0.4 + 0.6 * pow(cos(_Time.y * _WindFrequency + windOffsetTime) * cos(_Time.y * _WindFrequency * 2 + windOffsetTime), 2)) * _WindStrength;
						float2 windOffset = windStrength * windDirection;
						// windOffset = -lerp(windOffset, float2(0, 0), (gv_no_wind - windOffset) * 2);
						gv = gv_no_wind + windOffset;

						// what color
						float3 flowerColor = frac(random * float3(267.9, 113.2, 472.1)) * 0.4 + 0.6;

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
						float wdg = dot(-windOffset * 10, gv);
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
						float noFlower = step(0.5, frac(random * 321.3)) * step(depth, 0.5); // 50% of spaces missing flowers, anything with flower center covered should not show
						val = lerp(val, color, allColor * noFlower);
					#endif

					return float4(val,map.a);
				}
				ENDCG
			}
		}
}