Shader "Custom/UiOverWrite"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
           // ZTest Always Cull Off ZWrite Off // always render fullscreen on top
            Blend SrcAlpha OneMinusSrcAlpha  // <- important!
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // auto-provided by Unity

            struct appdata {
            float4 vertex : POSITION;
            float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
            return o;
            }


            fixed4 frag(v2f i) : SV_Target {

               float4 col = tex2D(_MainTex, i.uv);
               return col;

            }
            ENDCG
        }
    }
}