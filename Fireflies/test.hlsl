﻿RWTexture2D<float> Output : register(u0);

[numthreads(32, 32, 1)]
void main(uint3 id : SV_DispatchThreadID) {
	Output[id.xy] = float4(1.0, 1.0, 1.0, 1.0);
}