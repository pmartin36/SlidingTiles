Shader "SlidingTiles/TileSide"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Y("Y", float) = 1
		_RightX("Right X", float) = 1
		_LeftX("Left X", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 color: COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Y;
			float _RightX;
			float _LeftX;

            v2f vert (appdata v)
            {
				// vertex range = -(image size in pixels / pixels per unit / 2) to (image size in pixels / pixels per unit / 2)
				// vertex.x = -0.5 to 0.5
				// vertex.y = -0.03125 to 0.03125
                v2f o;
				float4 vd = v.vertex;
				if (vd.y < 0.0) {
					vd.y += (1-_Y) * 0.09375;
					if (vd.x < 0) {
						vd.x *= _LeftX;
					}
					else {
						vd.x *= _RightX;
					}
				}
                o.vertex = UnityObjectToClipPos(vd);
				o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                return col * i.color;
            }
            ENDCG
        }
    }
}
