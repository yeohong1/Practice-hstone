Shader "Custom/GrayscaleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GraysclaeAmount("Gray Scale Amount", Range(0,1))=1.0
        _DarkAmount("Dark Amount",Range(0,1)) = 1.0
    }
    SubShader
    {
       Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            

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
            float _GrayscaleAmount;
            float _DarkAmount;
            
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = lerp(col.rgb,dot(col.rgb,float3(0.3,0.59,0.11)),_GrayscaleAmount);
                col.rgb -= _DarkAmount;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
