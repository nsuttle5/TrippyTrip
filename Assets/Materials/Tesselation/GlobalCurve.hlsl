//Make sure its only included once!
#ifndef CUSTOM_COLOR_INCLUDED
#define CUSTOM_COLOR_INCLUDED

float3 _My_Global_Curve;

void getGlobalCurve_float(out float3 curve)
{
    curve = _My_Global_Curve;
}
#endif