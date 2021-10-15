Shader "Unlit/SpaceSphereShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DotOffset ("Dot Offset", range(-1, 1)) = 0
        _DotScale ("Dot Scale", float) = 1
        _BottomColor("Bottom Color", color) = (0,0,0,0)
        _TopColor("Top Color", color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // Blend One One

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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _DotOffset;
            float _DotScale;

            float4 _BottomColor;
            float4 _TopColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = dot(i.normal, float3(0,1,0));

                d *= _DotScale;
                d += _DotOffset;

                d = max(0, d);
                d = 1 - d;


                // sample the texture
                fixed4 col = pow(lerp(_BottomColor, _TopColor, d), 1.1);

                float3 starTexture = tex2D(_MainTex, i.uv);
                float stars = Luminance(starTexture.rgb);
                // stars = starTexture.r;
                stars *= 500;
                // stars = step(0.005, stars);
                // stars = smoothstep(0.003, 0.008, stars);
                col.rgb += stars.rrr;
   
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
