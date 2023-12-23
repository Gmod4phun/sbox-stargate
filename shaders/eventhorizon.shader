
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	float g_flIllumbrightness < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 1, 10 ); >;
	bool g_bGrayscale < UiGroup( ",0/,0/0" ); Default( 0 ); >;
	float g_flInMin < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flInMax < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flOutMin < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flOutMax < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float4 g_vSaturation < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float l_0 = g_flIllumbrightness;
		float l_1 = l_0 * 1.5;
		float2 l_2 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_3 = g_flTime * 1;
		float l_4 = VoronoiNoise( i.vTextureCoords.xy, l_3, 15 );
		float l_5 = saturate( ( l_4 - 0 ) / ( 1 - 0 ) ) * ( 0.525 - 0.5 ) + 0.5;
		float2 l_6 = l_2 + float2( l_5, l_5 );
		float2 l_7 = l_6 + float2( 0.49, 0.49 );
		float4 l_8 = Tex2DS( g_tColor, g_sSampler0, l_7 );
		float l_9 = l_8.x;
		float l_10 = l_9 * 0.299;
		float l_11 = l_8.y;
		float l_12 = l_11 * 0.587;
		float l_13 = l_10 + l_12;
		float l_14 = l_8.z;
		float l_15 = l_14 * 0.114;
		float l_16 = l_13 + l_15;
		float4 l_17 = float4( l_16, l_16, l_16, 0 );
		float4 l_18 = g_bGrayscale ? l_17 : l_8;
		float l_19 = l_18.x;
		float l_20 = l_19 * 0.299;
		float l_21 = l_18.y;
		float l_22 = l_21 * 0.587;
		float l_23 = l_20 + l_22;
		float l_24 = l_18.z;
		float l_25 = l_24 * 0.114;
		float l_26 = l_23 + l_25;
		float4 l_27 = float4( l_26, l_26, l_26, 0 );
		float l_28 = g_flInMin;
		float l_29 = g_flInMax;
		float l_30 = g_flOutMin;
		float l_31 = g_flOutMax;
		float4 l_32 = saturate( ( l_27 - float4( l_28, l_28, l_28, l_28 ) ) / ( float4( l_29, l_29, l_29, l_29 ) - float4( l_28, l_28, l_28, l_28 ) ) ) * ( float4( l_31, l_31, l_31, l_31 ) - float4( l_30, l_30, l_30, l_30 ) ) + float4( l_30, l_30, l_30, l_30 );
		float l_33 = l_32.x;
		float l_34 = l_33 * 1;
		float l_35 = l_32.y;
		float l_36 = l_35 * 1;
		float l_37 = l_34 + l_36;
		float l_38 = l_32.z;
		float l_39 = l_38 * 4.6;
		float l_40 = l_37 + l_39;
		float4 l_41 = float4( l_40, l_40, l_40, 0 );
		float4 l_42 = l_18 * l_41;
		float l_43 = l_42.x;
		float4 l_44 = g_vSaturation;
		float l_45 = l_43 * l_44.r;
		float l_46 = l_42.y;
		float l_47 = l_46 * l_44.g;
		float l_48 = l_42.z;
		float l_49 = l_48 * l_44.b;
		float4 l_50 = float4( l_45, l_47, l_49, 0 );
		float l_51 = l_0 * 10;
		float4 l_52 = l_50 * float4( l_51, l_51, l_51, l_51 );
		float4 l_53 = saturate( ( l_52 - float4( 0, 0, 0, 0 ) ) / ( float4( 10, 10, 10, 10 ) - float4( 0, 0, 0, 0 ) ) ) * ( float4( 1, 1, 1, 1 ) - float4( 0, 0, 0, 0 ) ) + float4( 0, 0, 0, 0 );
		float4 l_54 = float4( l_1, l_1, l_1, l_1 ) * l_53;
		float l_55 = saturate( ( l_0 - 1.5 ) / ( 10 - 1.5 ) ) * ( 10 - 0 ) + 0;
		float4 l_56 = l_52 * float4( l_55, l_55, l_55, l_55 );
		float4 l_57 = l_54 + l_56;
		
		m.Albedo = l_57.xyz;
		m.Emission = l_57.xyz;
		m.Opacity = 1;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );

		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
        m.TextureCoords = i.vTextureCoords.xy;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
