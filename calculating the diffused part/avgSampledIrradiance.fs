// to generate the irradiance map, we need to convolute the environment's
// lighting as converted to a cubemap.

#version 330 core
out vec4 FragColor;
in vec3 localPos;


// the HDR cubemap converted (can be from an equirectangular environment map.)
uniform samplerCube environmentMap;

const float PI = 3.14159265359;

void main()
{   	 
	// the sample direction equals the hemisphere's orientation
	vec3 normal = normalize(localPos);

	// Discretely sampling the hemisphere given the integral's spherical coordinates translates to the following fragment code:
	vec3 irradiance = vec3(0.0);  

	vec3 up	= vec3(0.0, 1.0, 0.0);
	vec3 right = normalize(cross(up, normal));
	up     	= normalize(cross(normal, right));

	//  We specify a fixed sampleDelta delta value to traverse the hemisphere; decreasing or increasing the sample delta will increase or decrease the accuracy respectively.
	float sampleDelta = 0.025;
	float nrSamples = 0.0;
	for(float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
	{
    	for(float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
    	{
        	// spherical to cartesian (in tangent space)
        	vec3 tangentSample = vec3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
        	// tangent space to world
        	vec3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;
        	// add each sample result to irradiance
        	irradiance += texture(environmentMap, sampleVec).rgb * cos(theta) * sin(theta);
        	nrSamples++;
    	}
	}
	// divide by the total number of samples taken, giving us the average sampled irradiance.
	irradiance = PI * irradiance * (1.0 / float(nrSamples));
 
	FragColor = vec4(irradiance, 1.0);
}