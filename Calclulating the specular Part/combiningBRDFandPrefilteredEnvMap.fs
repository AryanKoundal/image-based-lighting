// adding the pre-computed lighting data to the top of our PBR shader:
/// ... other code ... ///

uniform sampler2D brdfLUT;

// shader 
void main()
{
	/// ... other code ... ///

	BDRF();
	// sampling from the BRDF lookup texture given the material's roughness and the angle between the normal and view vector:
	vec2 envBRDF = texture(brdfLUT, vec2(max(dot(N, V), 0.0), roughness)).rg;
	// we combine the scale and bias to F0 from the BRDF lookup texture with the pre-filter portion of the reflectance equation
	vec3 specular = prefilteredColor * (F * envBRDF.x + envBRDF.y);

		/// ... other code ... ///
}

