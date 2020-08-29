Shader "SlidingTiles/Grass"
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
		_WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
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

	// Construct a rotation matrix that rotates around the provided axis, sourced from:
	// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
	}

	struct geometryOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	geometryOutput VertexOutput(float3 pos, float2 uv)
	{
		geometryOutput o;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = uv;
		return o;
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
			#pragma target 4.6

			#include "Lighting.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _PrimaryColor;
			float4 _SecondaryColor;
			sampler2D _Noise;

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


			// Modify the vertex shader.
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.primaryColor = v.color * _PrimaryColor;
				o.secondaryColor = v.color * _SecondaryColor;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float noise = tex2D(_Noise, i.uv).r * 0.85;
				noise += tex2D(_Noise, i.uv * 5).r * 0.25;
				return lerp(i.primaryColor, i.secondaryColor, noise);
			}
			ENDCG
		}

        Pass
        {
			Tags
			{
				"RenderType" = "Transparent"
				"Queue" = "Transparent + 1"
				"LightMode" = "ForwardBase"
			}


            CGPROGRAM
			#pragma geometry geo
            #pragma vertex vert
            #pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
            
			float4 _TopColor;
			float4 _BottomColor;
			float _TranslucentGain;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _BladeWidth;
			float _BladeWidthRandom;
			float _BladeHeight;
			float _BladeHeightRandom;
			float _BendRotationRandom;

			sampler2D _WindDistortionMap;
			float4 _WindDistortionMap_ST;
			float2 _WindFrequency;
			float _WindStrength;

			// Modify the vertex shader.
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.vertex = v.vertex;
				o.normal = v.normal;
				o.tangent = v.tangent;
				return o;
			}

			[maxvertexcount(3)]
			void geo(triangle vertexOutput IN[3], inout TriangleStream<geometryOutput> triStream)
			{
				geometryOutput o;
				float3 pos = IN[0].vertex;
				float3 vNormal = IN[0].normal;
				float4 vTangent = IN[0].tangent;
				float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;

				float3x3 tangentToLocal = float3x3(
					vTangent.x, vBinormal.x, vNormal.x,
					vTangent.y, vBinormal.y, vNormal.y,
					vTangent.z, vBinormal.z, vNormal.z
				);

				float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));
				float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
				
				float2 uv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y / 2;
				float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;
				float3 wind = normalize(float3(windSample.x, windSample.y, 0));
				float3x3 windRotation = AngleAxis3x3(UNITY_PI * windSample, wind);

				float3x3 transformationMatrix = mul(mul(mul(tangentToLocal, windRotation), facingRotationMatrix), bendRotationMatrix);

				// Add to the geometry shader, above the triStream.Append calls.
				float height = (rand(pos.zyx) * 2 - 1) * _BladeHeightRandom + _BladeHeight;
				float width = (rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth;

				triStream.Append(
					VertexOutput(
						pos + mul(transformationMatrix, float3(width, 0, 0)),
						float2(0,0)
					)
				);
				triStream.Append(
					VertexOutput(
						pos + mul(transformationMatrix, float3(-width, 0, 0)),
						float2(1,0)
					)
				);
				triStream.Append(
					VertexOutput(
						pos + mul(transformationMatrix, float3(0, 0, height)),
						float2(0.5,1)
					)
				);
			}

			float4 frag (geometryOutput i, fixed facing : VFACE) : SV_Target
            {	
				return lerp(_BottomColor, _TopColor, i.uv.y);
            }
            ENDCG
        }

		
    }
}