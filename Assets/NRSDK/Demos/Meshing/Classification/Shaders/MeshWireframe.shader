Shader "Unlit/MeshWireframe"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _LineWidth("LineWidth", Range(0,1))= 0.5
    }
    SubShader
    {
        Tags 
        {        
            "RenderType" = "Opaque"
        }
        
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color: COLOR;
                UNITY_FOG_COORDS(1)
                float3 worldPos : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };
            
            fixed4 _Color;            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _LineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed edgeFactor(fixed3 vbc) {
                fixed3 d = fwidth(vbc);                
                fixed3 f = step(d*_LineWidth, vbc);
                return min(min(f.x,f.y),f.z);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = max(1-edgeFactor(i.color.xyz),_Color);                             
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
