Shader "Shade/UI_Static"
{
    // Made with Shade Pro by Two Lives Left
    Properties
    {
        [NoScaleOffset] _texture  ("Texture", 2D) = "white" {}
    }

    SubShader
    {

        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 200

        CGPROGRAM

        #pragma target 4.0
        uniform sampler2D _texture;

        struct Input
        {
            float2 texcoord : TEXCOORD0;
            float4 color : COLOR;
        };
        float mod289(float x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
        float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
        float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
        float4 mod289(float4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
        
        float permute(float x) { return mod289(((x*34.0)+1.0)*x); }
        float2 permute(float2 x) { return mod289(((x*34.0)+1.0)*x); }
        float3 permute(float3 x) { return mod289(((x*34.0)+1.0)*x); }
        float4 permute(float4 x) { return mod289(((x*34.0)+1.0)*x); }
        
        float taylorInvSqrt(float r) { return 1.79284291400159 - 0.85373472095314 * r; }
        float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
        
        float2 fade(float2 t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }
        float3 fade(float3 t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }
        float4 fade(float4 t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }
        
        //
        // GLSL textureless classic 2D noise "cnoise",
        // with an RSL-style periodic variant "pcnoise2D".
        // Author:  Stefan Gustavson (stefan.gustavson@liu.se)
        // Version: 2011-08-22
        //
        // Many thanks to Ian McEwan of Ashima Arts for the
        // ideas for permutation and gradient selection.
        //
        // Copyright (c) 2011 Stefan Gustavson. All rights reserved.
        // Distributed under the MIT license. See LICENSE file.
        // https://github.com/ashima/webgl-noise
        //
        
        // Classic Perlin noise, periodic variant
        float pcnoise2D(float2 P, float2 rep)
        {
            float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
            float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
            Pi = fmod(Pi, rep.xyxy); // To create noise with explicit period
            Pi = mod289(Pi);        // To avoid truncation effects in permutation
            float4 ix = Pi.xzxz;
            float4 iy = Pi.yyww;
            float4 fx = Pf.xzxz;
            float4 fy = Pf.yyww;
        
            float4 i = permute(permute(ix) + iy);
        
            float4 gx = frac(i * (1.0 / 41.0)) * 2.0 - 1.0 ;
            float4 gy = abs(gx) - 0.5 ;
            float4 tx = floor(gx + 0.5);
            gx = gx - tx;
        
            float2 g00 = float2(gx.x,gy.x);
            float2 g10 = float2(gx.y,gy.y);
            float2 g01 = float2(gx.z,gy.z);
            float2 g11 = float2(gx.w,gy.w);
        
            float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
            g00 *= norm.x;
            g01 *= norm.y;
            g10 *= norm.z;
            g11 *= norm.w;
        
            float n00 = dot(g00, float2(fx.x, fy.x));
            float n10 = dot(g10, float2(fx.y, fy.y));
            float n01 = dot(g01, float2(fx.z, fy.z));
            float n11 = dot(g11, float2(fx.w, fy.w));
        
            float2 fade_xy = fade(Pf.xy);
            float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
            float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
            return 2.3 * n_xy;
        }
        
        
        float fbm_pcnoise2D (in float2 st, in int octaves, in float lacunarity, in float gain, in float2 period)
        {
            // Initial values
            float value = 0.0;
            float amplitude = .5;
            float frequency = 0.;
            //
            // Loop of octaves
            for (int i = 0; i < octaves; i++) {
                value += amplitude * pcnoise2D(st, period);
                st *= lacunarity;
                period *= lacunarity;
                amplitude *= gain;
            }
            return value;
        }
        float normalizeNoise(float n)
        {
            // return noise in [0, 1]
            return (n + 1.0) * 0.5;
        }
        float remap(float value, float minA, float maxA, float minB, float maxB)
        {
            return minB + (value - minA) * (maxB - minB) / (maxA - minA);
        }
        
        float2 remap(float2 value, float2 minA, float2 maxA, float2 minB, float2 maxB)
        {
            return minB + (value - minA) * (maxB - minB) / (maxA - minA);
        }
        
        float3 remap(float3 value, float3 minA, float3 maxA, float3 minB, float3 maxB)
        {
            return minB + (value - minA) * (maxB - minB) / (maxA - minA);
        }
        
        float4 remap(float4 value, float4 minA, float4 maxA, float4 minB, float4 maxB)
        {
            return minB + (value - minA) * (maxB - minB) / (maxA - minA);
        }
        
        float2 remap(float2 value, float minA, float maxA, float minB, float maxB)
        {
            return minB + (value - minA) * (maxB - minB) / (maxA - minA);
        }
        
        float3 remap(float3 value, float minA, float maxA, float minB, float maxB)
        {
            return minB + (value - minA) * (maxB - minB) / (maxA - minA);
        }
        
        float4 remap(float4 value, float minA, float maxA, float minB, float maxB)
        {
            return minB + (value - minA) * (maxB - minB) / (maxA - minA);
        }
        
        

        // Unlit model
        #pragma surface surf NoLighting vertex:vert noforwardadd addshadow

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            fixed4 c;
            c.rgb = s.Albedo + s.Emission.rgb;
            c.a = s.Alpha;
            return c;
        }

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.texcoord = v.texcoord;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Emission = float3(0.0, 0.0, 0.0);
            float temp_4 = fbm_pcnoise2D(IN.texcoord * (float2((_Time.y * 0.01), 0.0)+float2(0.0, _Time.y)), 5, 100.000000, 0.200000, (float2((_Time.y * 0.01), 0.0)+float2(0.0, _Time.y)));
            float remap_5 = remap(normalizeNoise(temp_4), float2(-1.0, 1.0).x, float2(-1.0, 1.0).y, float2(-1.0, 1.5).x, float2(-1.0, 1.5).y);
            o.Albedo = (tex2D(_texture, IN.texcoord).rgb*float3(remap_5, remap_5, remap_5));
            o.Alpha = 1.0;
            
        }
        ENDCG
    }
}
