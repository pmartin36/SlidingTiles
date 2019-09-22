Shader "SlidingTiles/TutorialArrow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Target("Target", Range(0, 2)) = 0
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

            CGPROGRAM	
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"	

			float inverseLerp(float a, float b, float v) {
				return (v - a) / (b - a);
			}

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Target;
			float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				
				float val = (1 - (_Target - col.r)) * step(0, _Target - col.r);
				val = saturate(val - 0.5 + col.b / 4);
				float alpha = inverseLerp(0, 0.35, val) * col.a;
				val = saturate(val - 0.3);
				float4 color = lerp(float4(1, 1, 1, 1), _Color, inverseLerp(0, 0.2, val));
				return color * alpha;
            }
            ENDCG
        }
    }
}
