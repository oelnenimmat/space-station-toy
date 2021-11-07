Shader "Custom/PlanetShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _LineColor ("Line Color", color) = (0,0,0,1)
        _LineScale ("LineScale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 position : TEXCOORD0; 
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float _LineScale;
            fixed4 _LineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex * _LineScale);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _LineColor;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normal;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));

            // Albedo comes from a texture tinted by color
            fixed4 colorTexture = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 c = colorTexture * _Color;
            // c = Luminance(colorTexture).rrrr;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = Luminance(colorTexture) * _Glossiness;
            o.Alpha = c.a;

            // o.Albedo = lerp(float3(1,0,0), float3(0,1,0), tex3D(_AmbientOcclusion3D, uv3).r);
            // o.Occlusion = clamp(0, 1, length(IN.worldPos) / 5.0);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
