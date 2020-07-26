Shader "SlidingTiles/World3GridBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Primary Color", Color) = (1,1,1,1)
		_SecondaryColor("Secondary Color", Color) = (0.5,0,0,1)
		_ImageColorAdditionFactor("Addition Factor", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Geometry"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always
			ZWrite On
			Cull Off

            CGPROGRAM	
            #pragma vertex vert
            #pragma fragment frag
			
            #include "UnityCG.cginc"


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
                float4 vertex : SV_POSITION;
				fixed4 primaryColor : COLOR0;
				fixed4 secondaryColor : COLOR1;
            };

            sampler2D _MainTex;
			sampler2D _Noise;
            float4 _MainTex_ST;
			float4 _Color;
			float4 _SecondaryColor;	
			float4 _TertiaryColor;
			float _ImageColorAdditionFactor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.primaryColor = v.color * _Color;
				o.secondaryColor = v.color * _SecondaryColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				float noise = tex2D(_Noise, i.screenPos / 2).r;
				noise *= noise + 0.25;

                // sample the texture
				/*float2 pos = i.screenPos;
				float noise = tex2D(_Noise, (pos / 2) + 0.25).r * 0.85;
				noise += tex2D(_Noise, pos * 3).r * 0.15;*/
				//return float4(smoothstep(0.63, 0.69, noise), 0, 0, 1);


				float4 col = lerp(i.primaryColor, i.secondaryColor, smoothstep(0.2, 0.6, noise));

				float4 main = tex2D(_MainTex, i.uv);
				col.a = main.a;

				col.rgb *= (1 + (main.rgb - 0.5) * 2 * _ImageColorAdditionFactor);

				return col;
            }
            ENDCG
        }
    }
	Fallback "Diffuse"
}
