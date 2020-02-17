Shader "Terrain Grid System/Unlit Single Color Cell Thin Line" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", float) = -0.01  
    _NearClip ("Near Clip", Range(0, 1000.0)) = 25.0
    _FallOff ("FallOff", Range(1, 1000.0)) = 50.0
    _ZWrite("ZWrite", Int) = 0
    _SrcBlend("Src Blend", Int) = 5
    _DstBlend("Dst Blend", Int) = 10
}
 
SubShader {
    Tags {
      "Queue"="Geometry+206" // microsplat renders in Geometry+100 so we force the grid to render afterwards
      "RenderType"="Opaque"
  	}
    Blend [_SrcBlend] [_DstBlend]
  	ZWrite [_ZWrite]
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag	
		#pragma multi_compile __ NEAR_CLIP_FADE
		#include "UnityCG.cginc"			

		float _Offset;
		fixed4 _Color;
		float _NearClip;
		float _FallOff;

		struct AppData {
			float4 vertex : POSITION;
		};

		struct VertexToFragment {
			fixed4 pos : SV_POSITION;	
			fixed4 color: COLOR;
		};
		
		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
				o.pos.z -= _Offset;
			#else
				o.pos.z += _Offset;
			#endif
			#if NEAR_CLIP_FADE
			if (UNITY_MATRIX_P[3][3]==1.0) {	// Orthographic camera
				o.color = _Color;
			} else {
				#if UNITY_REVERSED_Z
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z + _NearClip)/_FallOff));
				#else
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z - _NearClip)/_FallOff));
				#endif
			}
			#else
				o.color = _Color;
			#endif
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return i.color;
		}
			
		ENDCG
    }
    
}
}
