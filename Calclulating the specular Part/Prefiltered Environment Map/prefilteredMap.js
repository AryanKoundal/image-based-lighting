// We pre-filter the environment, based on some input roughness that varies over
// each mipmap level of the pre-filter cubemap (from 0.0 to 1.0), and store the
// result in prefilteredColor. The resulting prefilteredColor is divided by the
// total sample weight, where samples with less influence on the final result
// (for small NdotL) contribute less to the final weight.
// Generate the prefiltered map
var prefilteredMap = gl.createTexture();
gl.bindTexture(gl.TEXTURE_CUBE_MAP, prefilteredMap);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_R, gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MAG_FILTER, gl.LINEAR);

// ------------------ Capturing pre-filter mipmap levels --------------- //
// pre-filter the environment map with different roughness values over multiple mipmap levels.
for (let mip = 0; mip < numMips; ++mip) {
 var roughness = mip / (numMips - 1);
 for (let face = 0; face < 6; ++face) {
   // Render the cube map face with the prefiltered shader
   gl.framebufferTexture2D(
     gl.FRAMEBUFFER, 
     gl.COLOR_ATTACHMENT0, 
     gl.TEXTURE_CUBE_MAP_POSITIVE_X + 
     face, 
     prefilteredMap, 
     mip
    );
   gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
   gl.useProgram(prefilteredShaderProgram);
   gl.uniformMatrix4fv(prefilteredShaderProgram.uProjectionMatrix, false, projectionMatrix);
   gl.uniformMatrix4fv(prefilteredShaderProgram.uViewMatrix, false, viewMatrix);
   gl.uniform1f(prefilteredShaderProgram.uRoughness, roughness);
   gl.uniform1i(prefilteredShaderProgram.uEnvironmentMap, 0);
   gl.activeTexture(gl.TEXTURE0);
   gl.bindTexture(gl.TEXTURE_CUBE_MAP, envMapTexture);
   drawCube();
 }
}