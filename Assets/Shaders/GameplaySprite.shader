Shader "Custom/GameplaySprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
	    [PerRendererData] _Contrast("ContrastValue", Range(-5,5)) = 1.0
        [PerRendererData] _XOffset("XOffset", Float) = 0.0
        
        _Color ("Tint", Color) = (1,1,1,1)
        _ContrastModifier("ContrastRange", Range(-5,5)) = 1.0
        _Emission ("Emission", Float) = 1.0
        _EmissionModifier("EmissionModifier", Float) = 1.0
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0
        _GradientAmount ("Gradient Amount",Range(0, 5)) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineOpacity ("Outline Opacity", Range (0, 1)) = 0.5
        _OutlineThickness ("Outline Thickness", Range(0, 20)) = 1.0
        
        [Space][Space]
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        [Space][Space]
        _DoBarFill("Do Bar Fill", int) = 0
        _BarFill("Bar Fill", Range(0, 5)) = 1
        _BarSecondaryFill("Bar Secondary Fill", Range(0, 5)) = 1
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
        Blend SrcAlpha OneMinusSrcAlpha

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
                    // Emission
                    UNITY_DEFINE_INSTANCED_PROP(float, unity_SpriteEmissionArray)
                    // EmissionModifier
                    UNITY_DEFINE_INSTANCED_PROP(float, unity_SpriteEmissionModifierArray)
                    // XOffset
                    UNITY_DEFINE_INSTANCED_PROP(float, unity_SpriteXOffsetArray)
                    // FlashAmount
                    UNITY_DEFINE_INSTANCED_PROP(float, unity_SpriteFlashAmountArray)
            
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

                #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
                #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)
                #define _Contrast       UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteContrastArray)
                #define _Emission     UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteEmissionArray)
                #define _EmissionModifier      UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteEmissionModifierArray)
                #define _XOffset        UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteXOffsetArray)
                #define _FlashAmount    UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlashAmountArray)
            
            #endif // instancing

            CBUFFER_START(UnityPerDrawSprite)
            #ifndef UNITY_INSTANCING_ENABLED
                fixed4 _RendererColor;
                fixed2 _Flip;
                float _Contrast;
                float _Emission;
                float _EmissionModifier;
                float _XOffset;
                fixed _FlashAmount;
            #endif
                float _EnableExternalAlpha;
            CBUFFER_END

            fixed4 _Color;
	        float _ContrastModifier;
            float _GradientAmount;

            fixed4 _OutlineColor;
            float _OutlineOpacity;
            float _OutlineThickness;
            
            int _DoBarFill;
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

	        half4 AdjustContrast(half4 color, const half contrast)
			{
				return saturate(lerp(half4(0.5, 0.5, 0.5, color.a), color, contrast * _ContrastModifier));
			}

			half4 AdjustContrastCurve(const half4 color, const half contrast)
			{
				return pow(abs(color * 2 - 1), 1 / max(contrast, 0.0001)) * sign(color - _ContrastModifier) + _ContrastModifier;
			}


            inline float4 UnityFlipSprite(in float3 pos, in const fixed2 flip)
            {
                return float4(pos.xy * flip, pos.z, 1.0);
            }
            
            v2f vert(const appdata_t IN)
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
            float4 _MainTex_TexelSize;
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

            float DetermineOutlineAmount(fixed4 selfColor, float2 coord)
            {
                float2 size = _OutlineThickness * _MainTex_TexelSize.xy;
                float outline = SampleSpriteTexture(float2(coord.x + size.x, coord.y)).a;
                outline += SampleSpriteTexture(float2(coord.x, coord.y + size.y)).a;
                outline += SampleSpriteTexture(float2(coord.x - size.x, coord.y)).a;
                outline += SampleSpriteTexture(float2(coord.x, coord.y - size.y)).a;
                outline += SampleSpriteTexture(float2(coord.x + size.x, coord.y + size.y)).a;
                outline += SampleSpriteTexture(float2(coord.x - size.x, coord.y - size.y)).a;
                outline += SampleSpriteTexture(float2(coord.x + size.x, coord.y - size.y)).a;
                outline += SampleSpriteTexture(float2(coord.x - size.x, coord.y + size.y)).a;
                outline = min(outline, 1.0);
                outline = min(outline, step(0.1, _OutlineThickness));
                outline = min(outline, step(selfColor.a, 0));
                return outline;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                //Texture sampling
                const float2 coord = IN.texcoord + float2(_XOffset, 0);
                fixed4 c = SampleSpriteTexture (coord) * IN.color;

                //Contrast adjustment
		        c = AdjustContrast(c, _Contrast);
                c.rgb *= c.a;

                //Emission
                float emissionDifference = (_Emission - 1);
                float finalEmission = 1 + (emissionDifference * _EmissionModifier);
                c.rgb *= finalEmission;

                //Flashing (pure white)
                const fixed4 white = (1,1,1,c.a);
                c = lerp(c, white, _FlashAmount);

                //Left-Right darkening gradient
                float gradientOutcome = 1 - (1 - IN.texcoord.x) * _GradientAmount;
                c.rgb *= gradientOutcome;

                //Bar Fill (Health bars etc.)
                if (_DoBarFill)
                {
                    const float fillValue = 1 - step(_BarFill, IN.texcoord.x);
                    
                    _BarSecondaryFill = clamp(_BarSecondaryFill, _BarFill+0.0001, 1.0);
                    const float barFillGap = _BarSecondaryFill - _BarFill;
                    c.rgb *= fillValue;
                    c.rgb *= 1 - barFillGap;
                    c = lerp(c, white * gradientOutcome, saturate(step(IN.texcoord.x, _BarSecondaryFill) - fillValue));
                }

                //Outline
                const float outlineAmount = DetermineOutlineAmount(c, coord);
                c.rgb = lerp(c.rgb, _OutlineColor.rgb, outlineAmount);
                c.a = lerp(c.a, _OutlineOpacity, outlineAmount);

                //End
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
