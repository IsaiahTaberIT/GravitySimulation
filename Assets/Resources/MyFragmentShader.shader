Shader "Unlit/MyFragmentShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "White" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
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
                
                fixed4 col = fixed4(1,1,1,1);
                fixed4 clear =  fixed4(0,0,0,0);
                fixed4 output =  fixed4(0,0,0,0);

            
              
               
                output = lerp(col, clear, step(0.0,distance(i.uv - 0.5, fixed2(0,0)) - 0.5));
                    
                   
                return output;
                 
            }
               
            
            ENDCG
        }
    }
}


