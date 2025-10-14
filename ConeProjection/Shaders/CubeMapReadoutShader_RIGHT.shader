Shader "Unlit/CubeMapReadoutShader_RIGHT"
{
    Properties
    {
        _MainTex("NormalMap", 2D) = "white" {}
        _Cube("CubeMap", Cube) = "" {}
        _MousePositionRIGHT("MousePositionRIGHT", Vector) = (0, 0, -3, 0)
        _AlignmentImageRIGHT("AlignmentImageRIGHT", Range(0, 1)) = 0
        _MaxHeight("MaxHeight", Float) = 0
        _MinHeight("MinHeight", Float) = -6
        _fadeOut("Fade out", Range(0, 1)) = 1
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

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
            uniform samplerCUBE _Cube;
            float4 _MainTex_ST;
            float4 _MousePositionRIGHT;
            float _AlignmentImageRIGHT;
            float _MaxHeight;
            float _MinHeight;
            float _fadeOut;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                float4 x = tex2D(_MainTex, i.uv);

                // Position on the cone:
                float3 X = x.xyz;

                // Vector for reading out color from the cubemap
                float3 normal_for_cubemap = normalize(X - _MousePositionRIGHT.xyz);

                // Produce alignment image
                float on_cone = length(X) > 0;
                float z = -X.z;

                float on_proj = (X.z <= _MaxHeight) * (X.z >= _MinHeight);

                float cos_angle = x[3];
                float theta = atan2(X.y, X.x) * 360.0 / 2.0 / 3.141592654;
                float s = 0;
                if (z >= 2.8 & z <= 10.8) {
                    s = 1;
                }

                float3 w = float3((theta / 30.0 + 400) % 1.0, z % 1, s);
                float rad = 2 * 3.141592654 / (360.0);
                float y = cos(70 * rad);

                float c = on_cone * min(y / (abs(cos_angle) + 1e-10), 1);

                float t = -(theta - 5.0) / 10.0;

                float q = 0;
                if (t > 0) {
                    q = 1.0 / (1 + exp(-t));
                }
                else {
                    q = exp(t) / (1 + exp(t));
                }
                float fade_out = q;
                c = _fadeOut*c * fade_out + (1 - _fadeOut)*c;

                float4 alignment_image = float4(c * w.x, c * w.y, c * w.z, 1);


                float a = length(normal_for_cubemap)*c*on_proj;
                float4 col = texCUBE(_Cube, normal_for_cubemap.yzx);
                float4 scene_image =  float4(a*col.x, a*col.y, a*col.z, 1);


                float alph = saturate(_AlignmentImageRIGHT);

                return alph * alignment_image + (1 - alph) * scene_image;



            }
            ENDCG
        }
    }
}
