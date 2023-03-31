// to store the BRDF convolution result,  generate a 2D texture of a 512 by 512 resolution
// pre-allocate enough memory for the LUT texture.
// Load the BRDF LUT texture
const brdfLUTTexture = gl.createTexture();
gl.bindTexture(gl.TEXTURE_2D, brdfLUTTexture);
gl.texImage2D(gl.TEXTURE_2D, 0, gl.RG16F, gl.RG, gl.FLOAT, brdfLUT);
gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
  // re-use the same framebuffer object and run this shader over an NDC screen-space quad:
gl.BindFramebuffer(gl.FRAMEBUFFER, captureFBO);
gl.BindRenderbuffer(gl.RENDERBUFFER, captureRBO);
gl.RenderbufferStorage(gl.RENDERBUFFER, gl.DEPTH_COMPONENT24, 512, 512);
gl.FramebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, brdfLUTTexture, 0);

gl.Viewport(0, 0, 512, 512);
brdfShader.use();
glClear(gl.COLOR_BUFFER_BIT |gl.DEPTH_BUFFER_BIT);
RenderQuad();

gl.BindFramebuffer(gl.FRAMEBUFFER, 0);
