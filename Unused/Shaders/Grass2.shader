Shader "SlidingTiles/Grass2"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}

		[Header(Shading)]
        _TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5

		[Header(Blade Info)]
		_BladeWidth("Blade Width", Float) = 0.005
		_BladeWidthRandom("Blade Width Random", Float) = 0.002
		_BladeHeight("Blade Height", Float) = 0.5
		_BladeHeightRandom("Blade Height Random", Float) = 0.3
		_BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2

		[Header(Tesselation)]
		_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1

		[Header(Wind)]
		_WindTex("Wind Distortion Map", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1

		[Header(Background)]
		_Noise("Noise", 2D) = "white" {}
		_PrimaryColor("Primary Background Color", Color) = (1,1,1,1)
		_SecondaryColor("Secondary Background Color", Color) = (1,1,1,1)
    }

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"
	#include "CustomTessellation.cginc"

	// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
	// Extended discussion on this function can be found at the following link:
	// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
	// Returns a number in the 0...1 range.
	float rand(float3 co)
	{
		return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
	}
	ENDCG

    SubShader
    {
		Cull Off
		ZTest Off

		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			sampler2D _WindTex;

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
			};


			float N21(float2 p) {
				p = frac(p*float2(123.34, 456.21));
				p += dot(p, p + 45.32);
				return frac(p.x*p.y);
			}

			float3 grass(float2 uv, float wind_strength, float2x2 rotation) {
				float2 gv = (frac(uv) - 0.5) * 2.;
				float2 id = floor(uv);

				float n = N21(id);
				gv += float2(n, frac(n*34.13))*1.4 - 0.7;

				float2 rgv = mul(rotation, gv);
				rgv.y = abs(rgv.y);

				float width = lerp(0.3, 0.6, frac(n * 274.3));
				float len = lerp(0.01, 0.2, frac(n * 239.2)); // default length
				len = min(0.5, len + wind_strength);

				float tri = 0.05*(len - rgv.x) - rgv.y*len / (width);
				float val = smoothstep(-0.01, 0.01, tri) * step(0., rgv.x);
				return float3(fmod(id.x, 2.), fmod(id.y, 2.), 1.) * val;
			}

			// Modify the vertex shader.
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				/*float noise = tex2D(_Noise, i.uv).r * 0.85;
				noise += tex2D(_Noise, i.uv * 5).r * 0.25;*/
				
				// Normalized pixel coordinates (from 0 to 1)
				float2 uv = ((i.uv - 0.5) / _MainTex_TexelSize.xy) * _MainTex_TexelSize.y;		
				uv *= 3.;
				uv += 0.5;
				float2 gv = (frac(uv) - 0.5);		

				float4 wind_direction = tex2D(_WindTex, _Time.x / float2(10.3, 12.2));
				float angle = wind_direction.r * 21.28;
				float c = cos(angle);
				float s = sin(angle);
				float2x2 rotation = float2x2(c, -s, s, c);

				float t = _Time.x;
				float wind_strength = abs(sin(3 * t) * pow(sin(t), 6) * 0.5);

				float3 col = float3(0, 0, 0);
				for (float i = 0.; i < 1.; i += 1. / 10.) {
					float2 iuv = uv + 453.2*i;
					col += grass(iuv, wind_strength, rotation);
				}

				
				if(gv.x > 0.48 || gv.y > 0.48) col.rgb = float3(1,0,0);
				//else if(abs(gv.y) < 0.005) col.rgb = float3(0,1,0);
				//else if(abs(gv.x) < 0.005) col.rgb = float3(0,0,1);

				// Output to screen
				return float4(col, 1.0);

			}
			ENDCG
		}
    }
}