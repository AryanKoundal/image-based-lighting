float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec2 IntegrateBRDF(float NdotV, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float VanDerCorput(uint n, uint base);
vec2 HammersleyNoBitOps(uint i, uint N);
vec2 integratedBRDF = IntegrateBRDF(TexCoords.x, TexCoords.y);
FragColor = integratedBRDF;

// storing the pre-computed BRDF's response to each normal and light direction
// combination on varying roughness values in a 2D lookup texture (LUT) known
// as the BRDF integration map
// unsigned int brdfLUTTexture;

// ----------------------------------------------------------------------------
// take both the angle θ and the roughness as input, generate a sample vector
// with importance sampling, process it over the geometry and the derived Fresnel
// term of the BRDF, and output both a scale and a bias to F0 for each sample, averaging them in the end
vec2 IntegrateBRDF(float NdotV, float roughness)
{
  vec3 V;

  V.x = sqrt(1.0 - NdotV * NdotV);
  V.y = 0.0;
  V.z = NdotV;

  float A = 0.0;
  float B = 0.0;

  vec3 N = vec3(0.0, 0.0, 1.0);

  const uint SAMPLE_COUNT = 1024u;
  for (uint i = 0u; i < SAMPLE_COUNT; ++i)
  {
	vec2 Xi = Hammersley(i, SAMPLE_COUNT);
	vec3 H = ImportanceSampleGGX(Xi, N, roughness);
	vec3 L = normalize(2.0 * dot(V, H) * H - V);

	float NdotL = max(L.z, 0.0);
	float NdotH = max(H.z, 0.0);
	float VdotH = max(dot(V, H), 0.0);

	if (NdotL > 0.0)
	{
  	float G = GeometrySmith(N, V, L, roughness);
  	float G_Vis = (G * VdotH) / (NdotH * NdotV);
  	float Fc = pow(1.0 - VdotH, 5.0);

  	A += (1.0 - Fc) * G_Vis;
  	B += Fc * G_Vis;
	}
  }
  A /= float(SAMPLE_COUNT);
  B /= float(SAMPLE_COUNT);
  return vec2(A, B);
}

// ----------------------------------------------------------------------------

float GeometrySchlickGGX(float NdotV, float roughness)
{
  float a = roughness;
  float k = (a * a) / 2.0;

  float nom = NdotV;
  float denom = NdotV * (1.0 - k) + k;

  return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
  float NdotV = max(dot(N, V), 0.0);
  float NdotL = max(dot(N, L), 0.0);
  float ggx2 = GeometrySchlickGGX(NdotV, roughness);
  float ggx1 = GeometrySchlickGGX(NdotL, roughness);

  return ggx1 * ggx2;
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
