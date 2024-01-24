Shader "Custom/GameplaySprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
	    [PerRendererData] _Contrast("ContrastValue", Range(-5,5)) = 1.0
        [PerRendererData] _XOffset("XOffset", Float) = 0.0
	    _ContrastModifier("ContrastRange", Range(-5,5)) = 1.0
        
        _Color ("Tint", Color) = (1,1,1,1)
        _Emission ("Emission", Float) = 1.0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0
        
        _BarFill("Bar Fill", Range(0, 5)) = 1
        _BarSecondaryFill("Bar Secondary Fill", Range(0, 5)) = 1
        _BarFillSmoothing("Bar Fill Smoothing", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag

            #ifdef UNITY_INSTANCING_ENABLED

                UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
            
                    // SpriteRenderer.Color while Non-Batched/Instanced.
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
                    // this could be smaller but that's how bit each entry is regardless of type
                    UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
                    // Contrast
                    UNITY_DEFINE_INSTANCED_PROP(float, unity_SpriteContrastArray)
                    // XOffset
                    UNITY_DEFINE_INSTANCED_PROP(float, unity_SpriteXOffsetArray)
                    // FlashAmount
                    UNITY_DEFINE_INSTANCED_PROP(float, unity_SpriteFlashAmountArray)
            
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

                #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
                #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)
                #define _Contrast       UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteContrastArray)
                #define _XOffset        UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteXOffsetArray)
                #define _FlashAmount    UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlashAmountArray)
            
            #endif // instancing

            CBUFFER_START(UnityPerDrawSprite)
            #ifndef UNITY_INSTANCING_ENABLED
                fixed4 _RendererColor;
                fixed2 _Flip;
                float _Contrast;
                float _XOffset;
                fixed _FlashAmount;
            #endif
                float _EnableExternalAlpha;
            CBUFFER_END

            fixed4 _Color;
            float _Emission;
	        float _ContrastModifier;
            float _BarFill;
            float _BarSecondaryFill;
            float _BarFillSmoothing;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

	        half4 AdjustContrast(half4 color, half contrast)
			{
				return saturate(lerp(half4(0.5, 0.5, 0.5, color.a), color, contrast * _ContrastModifier));
			}

			half4 AdjustContrastCurve(half4 color, half contrast)
			{
				return pow(abs(color * 2 - 1), 1 / max(contrast, 0.0001)) * sign(color - _ContrastModifier) + _ContrastModifier;
			}


            inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
            {
                return float4(pos.xy * flip, pos.z, 1.0);
            }
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }
            
            sampler2D _MainTex;
            sampler2D _AlphaTex;

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 color = tex2D (_MainTex, uv);
                
            #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D (_AlphaTex, uv);
                color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
            #endif

                return color;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                float2 coord = IN.texcoord + float2(_XOffset, 0);
                fixed4 c = SampleSpriteTexture (coord) * IN.color;
                
		        c = AdjustContrast(c, _Contrast);
                c.rgb *= c.a;
                c.rgb *= _Emission;

                fixed4 white = (1,1,1,c.a);
                c = lerp(c, white, _FlashAmount);

                float fillValue = 1 - smoothstep(_BarFill, _BarFill + _BarFillSmoothing, IN.texcoord.x);
                c.rgb *= fillValue;
                _BarSecondaryFill = clamp(_BarSecondaryFill, _BarFill+0.0001 + _BarFillSmoothing, 1.0);
                float barFillGap = _BarSecondaryFill - _BarFill;
                c = lerp(c, white, saturate(smoothstep(_BarSecondaryFill, _BarFill, IN.texcoord.x) - fillValue + (barFillGap * 0.5 * fillValue)));
                
                return c;
            }
            
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            //#include "UnitySprites.cginc"
            
        ENDCG
        }
    }
}
