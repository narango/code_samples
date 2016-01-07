//Wind Flailing shader written in GLSL
//to see this shader in windows. Open Unity in OpenGL Mode
//check here http://docs.unity3d.com/Manual/CommandLineArguments.html
Shader "Custom/GLSL_WindFlailing" {

Properties
{
	//declare Material properties
	_MainTex("Texture Image", 2D) = "white" {}
    _WaveSpeed ("Wave Speed", Range(0.0, 300.0)) = 50.0 
    _WaveStrength ("Wave Strength", Range(0.0, 5.0)) = 1.0 
}
	SubShader 
	{
	Pass
	{
		//declare GLSL tag
		GLSLPROGRAM
		#include "UnityCG.glslinc"
		//declare shader in and out variables
		varying vec4 textureCoordinates;
		//declare global varibles that matches Material properties
		uniform float _WaveSpeed;
		uniform float _WaveStrength;
		uniform sampler2D _MainTex;
		//define vertex shader
		#ifdef VERTEX
		void main()
		{
			//get initial data from OpenGL
			vec4 v = vec4(gl_Vertex);
			vec2 uv = gl_MultiTexCoord0;
			
			//wave calculation
			float sinOff = (v.x + v.y + v.z) * _WaveStrength;
			float t = -_Time * _WaveSpeed;
			
			v.x += sin(t * 1.45 + sinOff) * uv.x * 0.5;
			v.y = sin(t*3.12 + sinOff) * uv.x * 0.5 - uv.y * 0.9;
			v.z -= sin(t*2.2 + sinOff) * uv.x * 0.2;
			//pass Mesh UV's to user varibles
			textureCoordinates = gl_MultiTexCoord0;
			//pass calculated vertex data to OpenGL
			gl_Position = gl_ModelViewProjectionMatrix * v;
		}
		#endif
		//define fragment shader
		#ifdef FRAGMENT
		void main()
		{
			//pass texture to OpenGL
			gl_FragColor = texture2D(_MainTex, vec2(textureCoordinates));
		}
		#endif
		ENDGLSL
	}
	}
	//Fallback shader
	FallBack "Diffuse"
}
