#version 330 core
out vec4 FragColor;
in vec3 localPos;

uniform samplerCube environmentMap;
uniform float roughness;

const float PI = 3.14159265359;

float VanDerCorput(uint bits);
vec2 HammersleyNoBitOps(uint i, uint N);
vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness);
// we don't know the view direction, so Epic Games makes an
// approximation by assuming the view direction (and thus the specular reflection direction)
//  to be equal to the output sample direction ωo(local position).
vec3 N = normalize(localPos);
vec3 R = N;
vec3 V = R;

// pre-filter the environment, based on some input roughness that varies
// over each mipmap level of the pre-filter cubemap (from 0.0 to 1.0),
// and store the result in prefilteredColor
const uint SAMPLE_COUNT = 1024u; // 4096u
float totalWeight = 0.0;
vec3 prefilteredColor = vec3(0.0);
for (uint i = 0u; i < SAMPLE_COUNT; ++i)
{
  vec2 Xi = HammersleyNoBitOps(i, SAMPLE_COUNT);
  vec3 H = ImportanceSampleGGX(Xi, N, roughness);
  vec3 L = normalize(2.0 * dot(V, H) * H - V);

  float NdotL = max(dot(N, L), 0.0);
  if (NdotL > 0.0)
  {
    prefilteredColor += texture(environmentMap, L).rgb * NdotL;
    totalWeight += NdotL;
  }
}
// With the low-discrepancy Hammersley sequence and sample generation defined,
// we finalized the pre-filter convolution shader(pre-filtered environment map)
prefilteredColor = prefilteredColor / totalWeight;

FragColor = vec4(prefilteredColor, 1.0);

// ----------------------------------------------------------------------------
// to build a sample vector, we take GGX NDF in spherical sample vector process
// for orienting and  biasing vector towards the specular lobe of some surface roughness
// This gives us a sample vector somewhat oriented around the expected microsurface's
// halfway vector based on some input roughness and the low-discrepancy sequence value Xi.
vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness){
  float a = roughness * roughness;

  float phi = 2.0 * PI * Xi.x;
  float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
  float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
  // from spherical coordinates to cartesian coordinates
  vec3 H;
  H.x = cos(phi) * sinTheta;
  H.y = sin(phi) * sinTheta;
  H.z = cosTheta;

  // from tangent-space vector to world-space sample vector
  vec3 up = abs(N.z) < 0.999 ? vec3(0.0, 0.0, 1.0) : vec3(1.0, 0.0, 0.0);
  vec3 tangent = normalize(cross(up, N));
  vec3 bitangent = cross(N, tangent);

  vec3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
  return normalize(sampleVec);
}


// ----------------------------------------------------------------------------
float VanDerCorput(uint n, uint base)
{
  float invBase = 1.0 / float(base);
  float denom = 1.0;
  float result = 0.0;

  for (uint i = 0u; i < 32u; ++i)
  {
	if (n > 0u)
	{
  	denom = mod(float(n), 2.0);
  	result += denom * invBase;
  	invBase = invBase / 2.0;
  	n = uint(float(n) / 2.0);
	}
  }

  return result;
}
// ----------------------------------------------------------------------------
vec2 HammersleyNoBitOps(uint i, uint N)
{
  return vec2(float(i) / float(N), VanDerCorput(i, 2u));
}
