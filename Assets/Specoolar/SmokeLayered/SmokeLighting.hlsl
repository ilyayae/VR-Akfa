#ifndef SMOKE_LIGHTING_INCLUDED
#define SMOKE_LIGHTING_INCLUDED
 
 //#define SampleTex(value, uv) SAMPLE_TEXTURE2D(value, sampler##value, uv)
 //#define SampleTexLOD0(value, uv) SAMPLE_TEXTURE2D_LOD(value, sampler##value, uv, 0)
 //#define SampleTexLOD(value, uv, level) SAMPLE_TEXTURE2D_LOD(value, sampler##value, uv, level)
 
void SampleFogLights_float(half3 pos, out half3 color)
{
    color = half3(0, 0, 0);
    uint pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; i++)
    {
        Light light = GetAdditionalPerObjectLight(i, pos);
        
        //half3 delta = pos - light.position;
        //half atten = 1 / (dot(delta, delta) * _LightDistanceScale + 1);
        color += light.distanceAttenuation * light.color;
    }
}
#endif