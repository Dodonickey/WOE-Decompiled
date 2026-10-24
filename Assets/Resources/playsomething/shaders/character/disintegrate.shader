Shader "Character/Disintegrate Diffuse"
{
    Properties
    {
        _MainTex ("Texture (RGB)", 2D) = "white" {}
        _NoiseTex ("Effect Map (RGB)", 2D) = "white" {}
        _DisintegrateAmount ("Effect Amount", Range(0,1.01)) = 0
        _DissolveColor ("Edge Color", Color) = (1,0.5,0.2,0)
        _EdgeEmission ("Edge Emission", Color) = (0,0,0,0)
        _DissolveEdge ("Edge Range", Range(0,0.1)) = 0.01
        _TileFactor ("Tile Factor", Range(0,4)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // =========================================================================
        // PASS 1: FORWARD BASE
        // =========================================================================
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" "RenderType"="Opaque" }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_baseDisintegrate
            #pragma fragment frag_baseDisintegrate
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata_baseDisintegrate
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f_baseDisintegrate
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 vLight : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };

            fixed4 _EdgeEmission;
            float _TileFactor;
            float _DissolveEdge;
            fixed4 _DissolveColor;
            float _DisintegrateAmount;
            sampler2D _NoiseTex;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f_baseDisintegrate vert_baseDisintegrate(appdata_baseDisintegrate v)
            {
                v2f_baseDisintegrate o;
                UNITY_INITIALIZE_OUTPUT(v2f_baseDisintegrate, o);

                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldNormal = worldNormal;

                float3 vLight = ShadeSH9(half4(worldNormal, 1.0));
                #ifdef VERTEXLIGHT_ON
                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    vLight += Shade4PointLights(
                        unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                        unity_LightColor[0].rgb, unity_LightColor[1].rgb,
                        unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                        unity_4LightAtten0, worldPos, worldNormal);
                #endif
                o.vLight = vLight;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag_baseDisintegrate(v2f_baseDisintegrate i) : SV_Target
            {
                fixed3 albedo = fixed3(0,0,0);
                fixed3 emission = fixed3(0,0,0);

                fixed4 noise = tex2D(_NoiseTex, i.uv * _TileFactor);
                float edgeVal = noise.r - _DisintegrateAmount;
                clip(edgeVal);

                if (edgeVal < _DissolveEdge && _DisintegrateAmount > 0.0)
                {
                    emission = _EdgeEmission.rgb;
                    albedo = _DissolveColor.rgb;
                }
                else
                {
                    albedo = tex2D(_MainTex, i.uv).rgb;
                }

                fixed atten = LIGHT_ATTENUATION(i);
                fixed3 diff = albedo * _LightColor0.rgb *
                    (max(0, dot(i.worldNormal, _WorldSpaceLightPos0.xyz)) * atten);

                fixed4 col;
                col.rgb = diff + albedo * i.vLight;
                col.rgb += emission;
                col.a = 0;
                return col;
            }
            ENDCG
        }

        // =========================================================================
        // PASS 2: FORWARD ADD
        // =========================================================================
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardAdd" "RenderType"="Opaque" }
            ZWrite Off
            Fog { Color (0,0,0,0) }
            Blend One One

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_addDisintegrate
            #pragma fragment frag_addDisintegrate
            #pragma multi_compile_fwdadd
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata_addDisintegrate
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f_addDisintegrate
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };

            float _TileFactor;
            float _DissolveEdge;
            fixed4 _DissolveColor;
            float _DisintegrateAmount;
            sampler2D _NoiseTex;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f_addDisintegrate vert_addDisintegrate(appdata_addDisintegrate v)
            {
                v2f_addDisintegrate o;
                UNITY_INITIALIZE_OUTPUT(v2f_addDisintegrate, o);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldNormal = worldNormal;

                #ifdef USING_DIRECTIONAL_LIGHT
                    o.lightDir = _WorldSpaceLightPos0.xyz;
                #else
                    o.lightDir = _WorldSpaceLightPos0.xyz - worldPos;
                #endif

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag_addDisintegrate(v2f_addDisintegrate i) : SV_Target
            {
                fixed4 noise = tex2D(_NoiseTex, i.uv * _TileFactor);
                float edgeVal = noise.r - _DisintegrateAmount;
                clip(edgeVal);

                fixed3 albedo;
                if (edgeVal < _DissolveEdge && _DisintegrateAmount > 0.0)
                    albedo = _DissolveColor.rgb;
                else
                    albedo = tex2D(_MainTex, i.uv).rgb;

                fixed3 lightDir = normalize(i.lightDir);
                fixed atten = LIGHT_ATTENUATION(i);

                fixed4 col;
                col.rgb = albedo * _LightColor0.rgb *
                    (max(0, dot(i.worldNormal, lightDir)) * atten);
                col.a = 0;
                return col;
            }
            ENDCG
        }

        // =========================================================================
        // PASS 3: PREPASS BASE (DEFERRED)
        // =========================================================================
        Pass
        {
            Name "PREPASS"
            Tags { "LightMode"="PrePassBase" "RenderType"="Opaque" }
            Fog { Mode Off }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_prepassBaseDisintegrate
            #pragma fragment frag_prepassBaseDisintegrate
            #include "UnityCG.cginc"

            struct appdata_prepassBaseDisintegrate
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f_prepassBaseDisintegrate
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float4 _MainTex_ST;
            float _TileFactor;
            float _DisintegrateAmount;
            sampler2D _NoiseTex;

            v2f_prepassBaseDisintegrate vert_prepassBaseDisintegrate(appdata_prepassBaseDisintegrate v)
            {
                v2f_prepassBaseDisintegrate o;
                UNITY_INITIALIZE_OUTPUT(v2f_prepassBaseDisintegrate, o);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            fixed4 frag_prepassBaseDisintegrate(v2f_prepassBaseDisintegrate i) : SV_Target
            {
                fixed4 noise = tex2D(_NoiseTex, i.uv * _TileFactor);
                clip(noise.r - _DisintegrateAmount);

                fixed4 res;
                res.rgb = i.worldNormal * 0.5 + 0.5;
                res.a = 0;
                return res;
            }
            ENDCG
        }

        // =========================================================================
        // PASS 4: PREPASS FINAL (DEFERRED)
        // =========================================================================
        Pass
        {
            Name "PREPASS"
            Tags { "LightMode"="PrePassFinal" "RenderType"="Opaque" }
            ZWrite Off

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_prepassFinalDisintegrate
            #pragma fragment frag_prepassFinalDisintegrate
            #pragma multi_compile_prepassfinal
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata_prepassFinalDisintegrate
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f_prepassFinalDisintegrate
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 projPos : TEXCOORD1;
                float3 shAmbient : TEXCOORD2;
            };

            fixed4 _EdgeEmission;
            float _TileFactor;
            float _DissolveEdge;
            fixed4 _DissolveColor;
            float _DisintegrateAmount;
            sampler2D _NoiseTex;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _LightBuffer;

            v2f_prepassFinalDisintegrate vert_prepassFinalDisintegrate(appdata_prepassFinalDisintegrate v)
            {
                v2f_prepassFinalDisintegrate o;
                UNITY_INITIALIZE_OUTPUT(v2f_prepassFinalDisintegrate, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.projPos = ComputeScreenPos(o.pos);
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.shAmbient = ShadeSH9(half4(worldNormal, 1.0));

                return o;
            }

            fixed4 frag_prepassFinalDisintegrate(v2f_prepassFinalDisintegrate i) : SV_Target
            {
                fixed3 albedo = fixed3(0,0,0);
                fixed3 emission = fixed3(0,0,0);

                fixed4 noise = tex2D(_NoiseTex, i.uv * _TileFactor);
                float edgeVal = noise.r - _DisintegrateAmount;
                clip(edgeVal);

                if (edgeVal < _DissolveEdge && _DisintegrateAmount > 0.0)
                {
                    emission = _EdgeEmission.rgb;
                    albedo = _DissolveColor.rgb;
                }
                else
                {
                    albedo = tex2D(_MainTex, i.uv).rgb;
                }

                half4 light = tex2Dproj(_LightBuffer, UNITY_PROJ_COORD(i.projPos));
                #ifdef HDR_LIGHT_PREPASS_ON
                    light.rgb = max(light.rgb, 0.001h);
                #else
                    light.rgb = -log2(max(light.rgb, 0.001h));
                #endif
                light.rgb += i.shAmbient;

                fixed4 col;
                col.rgb = albedo * light.rgb;
                col.rgb += emission;
                col.a = 0;
                return col;
            }
            ENDCG
        }

        // =========================================================================
        // PASS 5: SHADOW CASTER
        // =========================================================================
        Pass
        {
            Name "SHADOWCASTER"
            Tags { "LightMode"="ShadowCaster" "RenderType"="Opaque" }
            Cull Off
            Fog { Mode Off }
            Offset 1, 1

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_shadowCasterDisintegrate
            #pragma fragment frag_shadowCasterDisintegrate
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct appdata_shadowCasterDisintegrate
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f_shadowCasterDisintegrate
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };

            float4 _MainTex_ST;
            float _TileFactor;
            float _DisintegrateAmount;
            sampler2D _NoiseTex;

            v2f_shadowCasterDisintegrate vert_shadowCasterDisintegrate(appdata_shadowCasterDisintegrate v)
            {
                v2f_shadowCasterDisintegrate o;
                UNITY_INITIALIZE_OUTPUT(v2f_shadowCasterDisintegrate, o);
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }

            fixed4 frag_shadowCasterDisintegrate(v2f_shadowCasterDisintegrate i) : SV_Target
            {
                fixed4 noise = tex2D(_NoiseTex, i.uv * _TileFactor);
                clip(noise.r - _DisintegrateAmount);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

        // =========================================================================
        // PASS 6: SHADOW COLLECTOR
        // =========================================================================
        Pass
        {
            Name "SHADOWCOLLECTOR"
            Tags { "LightMode"="ShadowCollector" "RenderType"="Opaque" }
            Fog { Mode Off }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_shadowCollectorDisintegrate
            #pragma fragment frag_shadowCollectorDisintegrate
            #pragma multi_compile_shadowcollector
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            sampler2D _ShadowMapTexture;

            struct appdata_shadowCollectorDisintegrate
            {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f_shadowCollectorDisintegrate
            {
                float4 pos : SV_POSITION;
                float3 shadowCoord0 : TEXCOORD0;
                float3 shadowCoord1 : TEXCOORD1;
                float3 shadowCoord2 : TEXCOORD2;
                float3 shadowCoord3 : TEXCOORD3;
                float4 worldPosViewZ : TEXCOORD4;
                float2 uv : TEXCOORD5;
            };

            float4 _MainTex_ST;
            float _TileFactor;
            float _DisintegrateAmount;
            sampler2D _NoiseTex;

            v2f_shadowCollectorDisintegrate vert_shadowCollectorDisintegrate(
                appdata_shadowCollectorDisintegrate v)
            {
                v2f_shadowCollectorDisintegrate o;
                UNITY_INITIALIZE_OUTPUT(v2f_shadowCollectorDisintegrate, o);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPosViewZ.xyz = worldPos.xyz;
                o.worldPosViewZ.w = -UnityObjectToViewPos(v.vertex).z;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.shadowCoord0 = mul(unity_WorldToShadow[0], worldPos).xyz;
                o.shadowCoord1 = mul(unity_WorldToShadow[1], worldPos).xyz;
                o.shadowCoord2 = mul(unity_WorldToShadow[2], worldPos).xyz;
                o.shadowCoord3 = mul(unity_WorldToShadow[3], worldPos).xyz;

                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

            fixed4 frag_shadowCollectorDisintegrate(
                v2f_shadowCollectorDisintegrate i) : SV_Target
            {
                fixed4 noise = tex2D(_NoiseTex, i.uv * _TileFactor);
                clip(noise.r - _DisintegrateAmount);

                // NOTE: bool -> float conversions below use full-precision float4.
                // Using fixed4/half4 here makes the D3D11 compiler emit a bool->min16float
                // conversion, which triggers "Incompatible min precision type" (X8000).

                // --- Cascade Selection ---
                float4 cascadeWeights;
                float fade;
                #if defined(SHADOWS_SPLIT_SPHERES)
                    float3 fromCenter0 = i.worldPosViewZ.xyz - unity_ShadowSplitSpheres[0].xyz;
                    float3 fromCenter1 = i.worldPosViewZ.xyz - unity_ShadowSplitSpheres[1].xyz;
                    float3 fromCenter2 = i.worldPosViewZ.xyz - unity_ShadowSplitSpheres[2].xyz;
                    float3 fromCenter3 = i.worldPosViewZ.xyz - unity_ShadowSplitSpheres[3].xyz;
                    float4 distSq = float4(
                        dot(fromCenter0, fromCenter0),
                        dot(fromCenter1, fromCenter1),
                        dot(fromCenter2, fromCenter2),
                        dot(fromCenter3, fromCenter3));
                    cascadeWeights = (distSq < unity_ShadowSplitSqRadii) ? float4(1,1,1,1) : float4(0,0,0,0);
                    cascadeWeights.yzw = saturate(cascadeWeights.yzw - cascadeWeights.xyz);
                    float3 fadeVec = i.worldPosViewZ.xyz - unity_ShadowFadeCenterAndType.xyz;
                    fade = saturate(
                        sqrt(dot(fadeVec, fadeVec)) * _LightShadowData.z + _LightShadowData.w);
                #else
                    float4 viewZ = i.worldPosViewZ.wwww;
                    float4 zNear = (viewZ >= _LightSplitsNear) ? float4(1,1,1,1) : float4(0,0,0,0);
                    float4 zFar  = (viewZ <  _LightSplitsFar)  ? float4(1,1,1,1) : float4(0,0,0,0);
                    cascadeWeights = zNear * zFar;
                    fade = saturate(
                        i.worldPosViewZ.w * _LightShadowData.z + _LightShadowData.w);
                #endif

                // --- Blend Cascade Coordinates ---
                float4 samplePos;
                samplePos.w = 1.0;
                samplePos.xyz = i.shadowCoord0 * cascadeWeights.x
                              + i.shadowCoord1 * cascadeWeights.y
                              + i.shadowCoord2 * cascadeWeights.z
                              + i.shadowCoord3 * cascadeWeights.w;

                // --- Shadow Sampling ---
                float shadowAtten;
                #if defined(SHADOWS_NATIVE)
                    float rawShadow = tex2Dproj(
                        _ShadowMapTexture,
                        float4(samplePos.xy, 0.0, samplePos.z)
                    ).r;
                    shadowAtten = _LightShadowData.x
                                + rawShadow * (1.0 - _LightShadowData.x);
                #else
                    float rawShadow = tex2D(_ShadowMapTexture, samplePos.xy).x;
                    shadowAtten = max(
                        (rawShadow > samplePos.z) ? 1.0 : 0.0,
                        _LightShadowData.x);
                #endif

                // --- Output: shadow + fade in R, 1 in G, encoded depth in BA ---
                fixed4 res;
                res.x = saturate(shadowAtten + fade);
                res.y = 1.0;
                float2 enc = frac(
                    float2(1.0, 255.0)
                    * (1.0 - i.worldPosViewZ.w * _ProjectionParams.w));
                res.zw = float2(enc.x - enc.y * 0.00392157, enc.y);
                return res;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
