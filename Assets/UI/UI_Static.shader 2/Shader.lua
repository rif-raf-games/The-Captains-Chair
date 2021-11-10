{
    name = "UI_Static",

    options =
    {
        USE_COLOR = { true },
    },

    properties =
    {
        {"_texture", "texture2D", "Surfaces:Basic Bricks Color", mipmap = true, smooth = true, wrap = 'repeat' },
        {"_time", "float", "0.0" },
    },

    pass =
    {
        base = "Surface",

        blendMode = "normal",
        depthWrite = false,
        depthFunc = "lessEqual",
        renderQueue = "transparent",
        colorMask = {"rgba"},
        cullFace = "back",

        vertex =
        [[
            void vertex(inout Vertex v, out Input o)
            {
            }
        ]],


        surface =
        [[
            uniform mediump sampler2D _texture;
            uniform float _time;
            float mod289(float x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec2 mod289(vec2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec4 mod289(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            
            float permute(float x) { return mod289(((x*34.0)+1.0)*x); }
            vec2 permute(vec2 x) { return mod289(((x*34.0)+1.0)*x); }
            vec3 permute(vec3 x) { return mod289(((x*34.0)+1.0)*x); }
            vec4 permute(vec4 x) { return mod289(((x*34.0)+1.0)*x); }
            
            float taylorInvSqrt(float r) { return 1.79284291400159 - 0.85373472095314 * r; }
            vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
            
            vec2 fade(vec2 t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }
            vec3 fade(vec3 t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }
            vec4 fade(vec4 t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }
            
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
            float pcnoise2D(vec2 P, vec2 rep)
            {
                vec4 Pi = floor(P.xyxy) + vec4(0.0, 0.0, 1.0, 1.0);
                vec4 Pf = fract(P.xyxy) - vec4(0.0, 0.0, 1.0, 1.0);
                Pi = mod(Pi, rep.xyxy); // To create noise with explicit period
                Pi = mod289(Pi);        // To avoid truncation effects in permutation
                vec4 ix = Pi.xzxz;
                vec4 iy = Pi.yyww;
                vec4 fx = Pf.xzxz;
                vec4 fy = Pf.yyww;
            
                vec4 i = permute(permute(ix) + iy);
            
                vec4 gx = fract(i * (1.0 / 41.0)) * 2.0 - 1.0 ;
                vec4 gy = abs(gx) - 0.5 ;
                vec4 tx = floor(gx + 0.5);
                gx = gx - tx;
            
                vec2 g00 = vec2(gx.x,gy.x);
                vec2 g10 = vec2(gx.y,gy.y);
                vec2 g01 = vec2(gx.z,gy.z);
                vec2 g11 = vec2(gx.w,gy.w);
            
                vec4 norm = taylorInvSqrt(vec4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
                g00 *= norm.x;
                g01 *= norm.y;
                g10 *= norm.z;
                g11 *= norm.w;
            
                float n00 = dot(g00, vec2(fx.x, fy.x));
                float n10 = dot(g10, vec2(fx.y, fy.y));
                float n01 = dot(g01, vec2(fx.z, fy.z));
                float n11 = dot(g11, vec2(fx.w, fy.w));
            
                vec2 fade_xy = fade(Pf.xy);
                vec2 n_x = mix(vec2(n00, n01), vec2(n10, n11), fade_xy.x);
                float n_xy = mix(n_x.x, n_x.y, fade_xy.y);
                return 2.3 * n_xy;
            }
            
            
            float fbm_pcnoise2D (in vec2 st, in int octaves, in float lacunarity, in float gain, in vec2 period)
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
            
            vec2 remap(vec2 value, vec2 minA, vec2 maxA, vec2 minB, vec2 maxB)
            {
                return minB + (value - minA) * (maxB - minB) / (maxA - minA);
            }
            
            vec3 remap(vec3 value, vec3 minA, vec3 maxA, vec3 minB, vec3 maxB)
            {
                return minB + (value - minA) * (maxB - minB) / (maxA - minA);
            }
            
            vec4 remap(vec4 value, vec4 minA, vec4 maxA, vec4 minB, vec4 maxB)
            {
                return minB + (value - minA) * (maxB - minB) / (maxA - minA);
            }
            
            vec2 remap(vec2 value, float minA, float maxA, float minB, float maxB)
            {
                return minB + (value - minA) * (maxB - minB) / (maxA - minA);
            }
            
            vec3 remap(vec3 value, float minA, float maxA, float minB, float maxB)
            {
                return minB + (value - minA) * (maxB - minB) / (maxA - minA);
            }
            
            vec4 remap(vec4 value, float minA, float maxA, float minB, float maxB)
            {
                return minB + (value - minA) * (maxB - minB) / (maxA - minA);
            }
            
            
            void surface(in Input IN, inout SurfaceOutput o)
            {
                o.emissive = 1.0;
                float temp_4 = fbm_pcnoise2D(IN.uv * (vec2((_time * 0.01), 0.0)+vec2(0.0, _time)), 5, 100.000000, 0.200000, (vec2((_time * 0.01), 0.0)+vec2(0.0, _time)));
                float remap_5 = remap(normalizeNoise(temp_4), vec2(-1.0, 1.0).x, vec2(-1.0, 1.0).y, vec2(-1.0, 1.5).x, vec2(-1.0, 1.5).y);
                o.diffuse = (texture(_texture, IN.uv).rgb*vec3(remap_5, remap_5, remap_5));
                o.emission = vec3(0.0, 0.0, 0.0);
                o.opacity = 1.0;
            }
        ]]
    }
}
