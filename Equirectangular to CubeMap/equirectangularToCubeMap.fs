// fragment shader
// In the fragment shader we need to use a samplerCube instead of a sampler2D
// and use textureCube instead of texture2D. textureCube takes a vec3 direction 
// so we pass the normalized normal. Since the normal is a varying and will be 
// interpolated we need to normalize it again


precision mediump float;
    
// Passed in from the vertex shader.
varying vec3 v_normal;
    
// The texture.
uniform samplerCube u_texture;
    
void main() {
    gl_FragColor = textureCube(u_texture, normalize(v_normal));
}