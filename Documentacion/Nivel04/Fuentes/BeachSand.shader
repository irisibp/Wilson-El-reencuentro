Shader "Wilson/BeachSand"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4 _Color;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float2 world:TEXCOORD1; fixed4 color:COLOR; };
            v2f vert(appdata v)
            {
                v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv;
                o.world=mul(unity_ObjectToWorld,v.vertex).xy; o.color=v.color*_Color; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 tex=tex2D(_MainTex,i.uv);
                float2 pixel=floor(i.world*32);
                float grain=frac(sin(dot(pixel,float2(127.1,311.7)))*43758.5453);
                fixed3 sand=lerp(fixed3(.86,.59,.28),fixed3(1,.85,.51),.55+.35*frac(i.world.y));
                sand+=grain>.96 ? .07 : (grain<.035 ? -.09 : 0);
                // Reuse the existing sandy rim; replace blue masonry with dry sand.
                if(tex.r>tex.g*1.08 && tex.r>tex.b*1.4) sand=tex.rgb;
                return fixed4(sand,tex.a)*i.color;
            }
            ENDCG
        }
    }
}
