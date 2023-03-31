// adding the pre-computed lighting data to the top of our PBR shader:
uniform samplerCube prefilterMap;
uniform sampler2D brdfLUT;


// shader 
void main()
{
	vec3 F = FresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);

	vec3 kS = F;
	vec3 kD = 1.0 - kS;
	kD *= 1.0 - metallic;

	vec3 irradiance = texture(irradianceMap, N).rgb;
	vec3 diffuse = irradiance * albedo;

	generatePreFilteredEnvMap();
	// getting the indirect specular reflections of the surface by sampling the pre-filtered environment map using the reflection vector
	vec3 R = reflect(-V, N);
	// using a variable to ensure we don't sample a mip level where there's no data.
	const float MAX_REFLECTION_LOD = 4.0;
	vec3 prefilteredColor = textureLod(prefilterMap, R, roughness * MAX_REFLECTION_LOD).rgb;

	BDRF();
	// sampling from the BRDF lookup texture given the material's roughness and the angle between the normal and view vector:
	vec2 envBRDF = texture(brdfLUT, vec2(max(dot(N, V), 0.0), roughness)).rg;
	// we combine the scale and bias to F0 from the BRDF lookup texture with the pre-filter portion of the reflectance equation
	vec3 specular = prefilteredColor * (F * envBRDF.x + envBRDF.y);

	// combining the indirect specular part with the diffuse IBL part
	// we don't multiply specular by kS as we already have a Fresnel multiplication in there.
	vec3 ambient = (kD * diffuse + specular) * ao;
}