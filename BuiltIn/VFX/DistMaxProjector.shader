// Made with Amplify Shader Editor v1.9.1.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/DistMaxProjector"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Color1("Color", Color) = (0,0,0,0)
		[Toggle(_BACKFACECULLING_ON)] _BACKFACECULLING("BACKFACECULLING", Float) = 1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend One One, One OneMinusSrcAlpha
		BlendOp Max
		AlphaToMask Off
		Cull Off
		ColorMask RGBA
		ZWrite Off
		ZTest Always
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1


			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _BACKFACECULLING_ON


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
#endif
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform float4 _Color1;
			uniform float4 _Color;

			
			v2f vert ( appdata v )
			{
				v2f o;
				float2 texCoord7 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break101 = texCoord7;
				float2 appendResult103 = (float2(break101.x , ( 1.0 - break101.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch100 = texCoord7;
				#else
				float2 staticSwitch100 = appendResult103;
				#endif
				float4 unityObjectToClipPos38 = UnityObjectToClipPos( v.vertex.xyz );
				float4 appendResult8 = (float4(staticSwitch100 , (unityObjectToClipPos38).zw));
				
				o.ase_texcoord1 = v.vertex;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( appendResult8 * float4( 2,-2,1,0 ) ) + float4( -1,1,0,0 ) ).xyz;
				o.vertex = float4(vertexValue.xyz,1);

#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
#endif
				float3 appendResult116 = (float3(0.0 , 0.0 , 0.5));
				float4 unityObjectToClipPos38 = UnityObjectToClipPos( i.ase_texcoord1.xyz );
				float temp_output_109_0 = ( 1.0 - ( distance( appendResult116 , ( (unityObjectToClipPos38).xyz * float3(0.5,0.5,1) ) ) * 2.0 ) );
				float4 lerpResult111 = lerp( _Color1 , _Color , saturate( temp_output_109_0 ));
				
				
				finalColor = saturate( lerpResult111 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19103
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;64;-682.5044,-56.14828;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;66;-718.7157,-203.077;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;43;-514.2676,-136.6484;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-605.219,227.2467;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SignOpNode;91;-322.1364,-130.7534;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;101;-804.4938,480.4535;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SaturateNode;89;-182.2188,-125.5905;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;102;-662.494,537.4532;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;103;-437.494,506.4532;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexToFragmentNode;49;4.79973,-84.86951;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;259.4811,-161.6906;Inherit;False;FLOAT4;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;85;-945.6664,-153.8007;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;100;-260.9267,476.4027;Inherit;False;Property;UNITY_UV_STARTS_AT_TOP1;UNITY_UV_STARTS_AT_TOP;3;0;Create;False;0;0;0;False;0;False;0;0;0;False;UNITY_UV_STARTS_AT_TOP;Toggle;2;Key0;Key1;Fetch;False;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-55.76405,138.1129;Inherit;False;FLOAT4;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;86;696.9604,-399.0311;Inherit;False;Property;_BACKFACECULLING;BACKFACECULLING;2;0;Create;True;0;0;0;True;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;136.6073,136.7019;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;2,-2,1,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;104;892.4716,-141.6931;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;304.0794,137.444;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;-1,1,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1097.151,-111.8234;Float;False;True;-1;2;ASEMaterialInspector;100;18;Naelstrof/DistMaxProjector;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;True;True;4;1;False;;1;False;;3;1;False;;10;False;;True;5;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.LerpOp;111;137.6448,-556.4791;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;38;-1392.169,-380.4389;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;108;-1149.933,-424.5844;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;125;-1167.276,-340.6078;Inherit;False;Constant;_Vector0;Vector 0;1;0;Create;True;0;0;0;False;0;False;0.5,0.5,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-970.5858,-399.2366;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DistanceOpNode;106;-724.4733,-538.9299;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;116;-963.8008,-580.5916;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-553.9143,-497.9721;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;127;-51.30334,-485.7741;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;109;-400.0145,-496.7726;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;37;-1584.24,-377.2266;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;444.0809,-332.5906;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;112;-171.9562,-385.8956;Inherit;False;Step Antialiasing;-1;;1;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;128;-479.0038,-691.1728;Inherit;False;Property;_Color1;Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;39;-480.0452,-859.3004;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;43;0;66;0
WireConnection;43;1;64;0
WireConnection;91;0;43;0
WireConnection;101;0;7;0
WireConnection;89;0;91;0
WireConnection;102;0;101;1
WireConnection;103;0;101;0
WireConnection;103;1;102;0
WireConnection;49;0;89;0
WireConnection;87;3;49;0
WireConnection;85;0;38;0
WireConnection;100;1;103;0
WireConnection;100;0;7;0
WireConnection;8;0;100;0
WireConnection;8;2;85;0
WireConnection;86;1;111;0
WireConnection;86;0;90;0
WireConnection;9;0;8;0
WireConnection;104;0;111;0
WireConnection;10;0;9;0
WireConnection;42;0;104;0
WireConnection;42;1;10;0
WireConnection;111;0;128;0
WireConnection;111;1;39;0
WireConnection;111;2;127;0
WireConnection;38;0;37;0
WireConnection;108;0;38;0
WireConnection;124;0;108;0
WireConnection;124;1;125;0
WireConnection;106;0;116;0
WireConnection;106;1;124;0
WireConnection;110;0;106;0
WireConnection;127;0;109;0
WireConnection;109;0;110;0
WireConnection;90;0;111;0
WireConnection;90;1;87;0
WireConnection;112;2;109;0
ASEEND*/
//CHKSM=29F1099731FAFE2F9EA17EA19FD2B6E7D8DC0AA0