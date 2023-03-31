// —---------------- Generate the irradiance map —--------------------//
// As the irradiance map averages all surrounding radiance uniformly it doesn't have a lot of high frequency details, so we can store the map at a low resolution (32x32)
const irradianceMap = gl.createTexture();

gl.genTextures(1, irradianceMap);
g.bindTexture(gl.TEXTURE_CUBE_MAP, irradianceMap);

for (let face = 0; face < 6; ++face) {
 gl.TexImage2D(GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, 0, GL_RGB16F, 32, 32, 0,GL_RGB, GL_FLOAT, nullptr);
}
gl.bindTexture(gl.TEXTURE_CUBE_MAP, irradianceMap);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_R, gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MAG_FILTER, gl.LINEAR);

// Create your shader program
const program = createShaderProgram(
 gl,
 vertexShaderSource,
 fragmentShaderSource
);
gl.useProgram(program);

// Set up your uniforms
const projectionMatrix = calculateProjectionMatrix();
gl.uniformMatrix4fv(
 gl.getUniformLocation(program, "uProjectionMatrix"),
 false,
 projectionMatrix
);


// re-scale the capture framebuffer to the new resolution
// Convolve the irradiance map
gl.bindTexture(gl.TEXTURE_CUBE_MAP, irradianceMap);
gl.generateMipmap(gl.TEXTURE_CUBE_MAP);
gl.RenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, 32, 32);

// Render each face of the cubemap texture
gl.bindFramebuffer(gl.FRAMEBUFFER, framebuffer);
// don't forget to configure the viewport to capture dimensions.
gl.viewport(0, 0, cubemapSize, cubemapSize);
for (let i = 0; i < 6; i++) {
 gl.uniformMatrix4fv(
   gl.getUniformLocation(program, "uViewMatrix"),
   false,
   cubemapViews[i]
 );
 gl.framebufferTexture2D(
   gl.FRAMEBUFFER,
   gl.COLOR_ATTACHMENT0,
   gl.TEXTURE_CUBE_MAP_POSITIVE_X + i,
   cubemapTexture,
   0
 );
 gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
 render();
}