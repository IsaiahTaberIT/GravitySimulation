Shader "UI/Invert"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "blue" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
      

    }
    SubShader
    {
        // No culling or depth
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always

          GrabPass { "_GrabTexture" } // Captures the screen into _GrabTexture
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

         
            fixed4  _Color; 
       
            sampler2D _GrabTexture;
            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            { 
                
                float2 p = i.uv;
                p = float2(p.x,1 - p.y);
                p/= 32;
                p += 0.484375;
                fixed4 col = tex2D(_GrabTexture, p.xy);
                fixed4 textcol = tex2D(_MainTex, i.uv);
                textcol *= _Color;
                col.rgb = textcol.rgb - col.rgb;
                col.a = textcol.a;
                return col;
            }
            ENDCG
        }
    }
}
