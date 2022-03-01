Shader "Custom/PaintedHeightShader" {
	Properties {
		_LowColor ("Low Color", Color) = (1,1,1,1)
		_HighColor("High Color", Color) = (1,1,1,1)

		_HighWaterColor("High Water Color", Color) = (0,0,.5,1)
		_LowWaterColor("Low Water Color", Color) = (0,0,1,1)

		_HighBeachColor("High Beach Color", Color) = (0,0,0,1)
		_LowBeachColor("Low Beach Color", Color) = (0,0,0,1)

		_HighUnderWaterColor("High Under Water Color", Color) = (0,0,0,1)
		_LowUnderWaterColor("Low Under Water Color", Color) = (0,0,0,1)
		
		_TreeWoodColour("Tree Wood Colour", Color) = (0,0,0,1)
		_TreeLeavesColour("Tree Leaves Colour", Color) = (0,0,0,1)

		_MainTex("Terrain Tex (RGB)", 2D) = "white" {}
		_ProjectionHeight ("Projection Height", Float) = 4
		_ProjectionRange("Projection Range", Float) = 0.004

		_CloudTex("Cloud Tex (RGB)", 2D) = "white" {}
		_WaterScrollingTex("Water Scrolling Tex (RGB)", 2D) = "white" {}
		_ScrollXSpeed("X", Range(-10,10)) = 2
		_ScrollYSpeed("Y", Range(-10,10)) = 3


		_ColourTex("Colour Tex (RGB)", 2D) = "white" {}
		[Toggle]_LayeredGround("is Layered", Float) = 0



			_CellSize("Cell Size", Range(0, 10)) = 2
			_TimeScale("Scrolling Speed", Range(0, 2)) = 1
			_BorderColor("Border Color", Color) = (0,0,0,1)
			_BorderSize("Border Size", Range(0.000, 0.1)) = 0.05


			_ForestCellSize("Forest Cell Size", Range(0, 100)) = 50
			_ForestCellDensity("Forest Cell Density", Range(0, 1)) = 0.25
			_ForestDensity("Forest Density", Range(0, 1)) = .1
		//_ProjectionRadius("Projection Radius", Float) = 9
		//_WaterLevel("Water Level",Range(0, 1)) = 0.25
	}
	SubShader {
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite off
			//Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }	
			//pass {
		//	Blend SrcAlpha OneMinusSrcAlpha
		//	ZWrite Off
		//}
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "Random.cginc"

		sampler2D _MainTex;
		sampler2D _CloudTex;
		sampler2D _ColourTex;
		sampler2D _WaterScrollingTex;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_CloudTex;
			float2 uv_ColourTex;
			float2 uv_WaterScrollingTex;
			float3 worldPos;
		};
				
		fixed4 _LowColor;
		fixed4 _HighColor;

		fixed4 _HighWaterColor;
		fixed4 _LowWaterColor;

		fixed4 _HighBeachColor;
		fixed4 _LowBeachColor;

		fixed4 _HighUnderWaterColor;
		fixed4 _LowUnderWaterColor;

		fixed4 _TreeWoodColour;
		fixed4 _TreeLeavesColour;

		float _ProjectionHeight;
		float _ProjectionRange;
		float _ProjectionRadius;
		float _WaterLevel;
		float _CloudLevel;
		float _CloudHeight;


		float _MapHeight;

		float4 _PointA;
		int _PointCount = 0;
		float4 _Point[1000];
		float _PointRadius[1000];


		fixed _ScrollXSpeed;
		fixed _ScrollYSpeed;

		sampler2D _WaterScrollingTexDuplicate;

		bool _LayeredGround;


		float _CellSize;
		
		float _TimeScale;
		float3 _BorderColor;
		float _BorderSize;

		float _ForestCellSize;
		float _ForestCellDensity;
		float _ForestDensity;




		float2 voronoiNoise2D(float2 value) {
			float2 baseCell = floor(value);

			float minDistToCell = 10;
			float2 closestCell;
			[unroll]
			for (int x = -1; x <= 1; x++) {
				[unroll]
				for (int y = -1; y <= 1; y++) {
					float2 cell = baseCell + float2(x, y);
					float2 cellPosition = cell + rand2dTo2d(cell);
					float2 toCell = cellPosition - value;
					float distToCell = length(toCell);
					if (distToCell < minDistToCell) {
						minDistToCell = distToCell;
						closestCell = cell;
					}
				}
			}
			float random = rand2dTo1d(closestCell);
			return float2(minDistToCell, random);
		}
		float3 voronoiNoise3D(float3 value) {
			float3 baseCell = floor(value);

			//first pass to find the closest cell
			float minDistToCell = 10;
			float3 toClosestCell;
			float3 closestCell;
			[unroll]
			for (int x1 = -1; x1 <= 1; x1++) {
				[unroll]
				for (int y1 = -1; y1 <= 1; y1++) {
					[unroll]
					for (int z1 = -1; z1 <= 1; z1++) {
						float3 cell = baseCell + float3(x1, y1, z1);
						float3 cellPosition = cell + rand3dTo3d(cell);
						float3 toCell = cellPosition - value;
						float distToCell = length(toCell);
						if (distToCell < minDistToCell) {
							minDistToCell = distToCell;
							closestCell = cell;
							toClosestCell = toCell;
						}
					}
				}
			}

			//second pass to find the distance to the closest edge
			float minEdgeDistance = 10;
			[unroll]
			for (int x2 = -1; x2 <= 1; x2++) {
				[unroll]
				for (int y2 = -1; y2 <= 1; y2++) {
					[unroll]
					for (int z2 = -1; z2 <= 1; z2++) {
						float3 cell = baseCell + float3(x2, y2, z2);
						float3 cellPosition = cell + rand3dTo3d(cell);
						float3 toCell = cellPosition - value;

						float3 diffToClosestCell = abs(closestCell - cell);
						bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
						if (!isClosestCell) {
							float3 toCenter = (toClosestCell + toCell) * 0.5;
							float3 cellDifference = normalize(toCell - toClosestCell);
							float edgeDistance = dot(toCenter, cellDifference);
							minEdgeDistance = min(minEdgeDistance, edgeDistance);
						}
					}
				}
			}

			float random = rand3dTo1d(closestCell);
			return float3(minDistToCell, random, minEdgeDistance);
		}
		void surf (Input IN, inout SurfaceOutputStandard o) {			
			// checks if within distant of the ponts
			// if not clips it
			float dist = 0;
			//float a = 0;
			//float c = a;
			bool d = false;
			for (int i = 0; i < _PointCount; i++) {
				dist = distance(IN.worldPos, _Point[i].xyz);
				if (dist < _PointRadius[i]) {
					d = true;
				}
			}
			clip(d ? 1 : -1);

			// checks if the pixel is water 
			
			bool w = false;
			if (tex2D(_MainTex, IN.uv_MainTex).r < _WaterLevel && (IN.worldPos.y / _ProjectionHeight) <_WaterLevel && (IN.worldPos.y / _ProjectionHeight) >_WaterLevel - .05) {
				w = true;
			}	

			float height = tex2D(_MainTex, IN.uv_MainTex).r * _ProjectionHeight;
			float heightDist = height - IN.worldPos.y;
			
			bool g = false;
			if ((_ProjectionRange - abs(heightDist)) >0) {
				g = true;
			}
			if (_LayeredGround) {
				if (tex2D(_MainTex, IN.uv_MainTex).r > (IN.worldPos.y / _ProjectionHeight)) {
					g = true;
				}
			}

			bool c = false;
			if (tex2D(_CloudTex, IN.uv_CloudTex).r < _CloudLevel && (IN.worldPos.y / _MapHeight) >= _CloudHeight -.05 && (IN.worldPos.y / _MapHeight) < _CloudHeight) {
				c = true;
			}


			bool b = false;
			if (tex2D(_MainTex, IN.uv_MainTex).r >= _WaterLevel -.1 &&tex2D(_MainTex, IN.uv_MainTex).r <= _WaterLevel + .05 && (IN.worldPos.y / _ProjectionHeight) >=_WaterLevel -.1 && (IN.worldPos.y / _ProjectionHeight) <=_WaterLevel + .05) {
				if (g) {
					b = true;
					g = false;
				}
			}
			bool uw = false;
			if (tex2D(_MainTex, IN.uv_MainTex).r < _WaterLevel && (IN.worldPos.y / _ProjectionHeight) <_WaterLevel) {
				if (g) {
					uw = true;
					g = false;
				}
			}
			
			bool tt = false;
			bool tl = false;
			
			
			float2 value = IN.worldPos.xz / _ForestCellSize;
			float noise = voronoiNoise2D(value).y;
			if (noise < _ForestCellDensity) {
				value = IN.worldPos.xz / 1;
				noise = voronoiNoise2D(value).y;
				if (noise < _ForestDensity && tex2D(_MainTex, IN.uv_MainTex).r < (IN.worldPos.y / _ProjectionHeight) && ((IN.worldPos.y / _ProjectionHeight) - tex2D(_MainTex, IN.uv_MainTex).r) < 0.1 && tex2D(_MainTex, IN.uv_MainTex).r >= _WaterLevel) {
					if (((IN.worldPos.y / _ProjectionHeight) - tex2D(_MainTex, IN.uv_MainTex).r) > .05) {
						tl = true;
					}
					else {
						tt = true;
					}
				}
			}

			


			///else {
				//clip(_ProjectionRange - abs(heightDist));
			//}
			
			//float dist = distance(IN.worldPos, _PointA.xyz);
			//clip(_ProjectionRadius - abs(dist));
			
			
			/*
			dist = 0;
			for (int i = 0; i < _PointCount; i++) {
				dist += distance(IN.worldPos, _Point[i].xyz);
				a = _PointRadius[i];
				a -= dist;
				a = a / 25;
			}
			*/
			//clip(_ProjectionRadius - abs(a));
						
			
			if (w && g) {
				b = true;
				w = false;
				g = false;
			}
			else if (w &&b) {
				w = false;
			}

			heightDist = 0 - IN.worldPos.y;

			o.Albedo = float3(0,0,0);	
			if (w) {
				/*
				float3 value = IN.worldPos.xyz / _CellSize;
				value.y += _Time.y * _TimeScale;
				float noise = voronoiNoise2D(value);*/

			//	float2 uv = IN.uv_WaterScrollingTex + (fixed2(_ScrollXSpeed, _ScrollYSpeed) * _Time);
				
				
				// aimation
				fixed2 scrolledUV = ((IN.uv_WaterScrollingTex -0.5) * 0.5 + 0.5);
				fixed2 scrolledUVDuplicate = scrolledUV;
				//fixed2 scrolledUV = IN.uv_CloudTex;


				fixed xScrollValue = _ScrollXSpeed * _Time;
				//fixed xScrollValue = 0;
				fixed yScrollValue = _ScrollYSpeed * _Time;
				//fixed yScrollValue = 0;

				fixed2 uvOffset = fixed2(xScrollValue, yScrollValue);
				
				

				scrolledUV += uvOffset;
				scrolledUVDuplicate -= uvOffset;

				fixed2 scrolledUVAdd = scrolledUV + scrolledUVDuplicate;

				
				_WaterScrollingTexDuplicate = _WaterScrollingTex;


				half4 c = tex2D(_WaterScrollingTex, scrolledUV);
				half4 cd = tex2D(_WaterScrollingTexDuplicate, scrolledUVDuplicate);

				half4 cO = (c + cd) /2;

				fixed2 staticUV = IN.uv_WaterScrollingTex;
				
				half4 cS = tex2D(_WaterScrollingTex, staticUV);

				half4 cM = (cO + cS) / 2;

				c = cM;

				c = (c + tex2D(_WaterScrollingTex, ((IN.uv_WaterScrollingTex - 0.5) * 0.5 + 0.5)).w)/2;
				c = (c + tex2D(_WaterScrollingTex, ((IN.uv_WaterScrollingTex - 0.5) * 0.5 + 0.5)).z)/2;
				 

				c = c * cO;


				//o.uv = _WaterScrollingTex.xy + _WaterScrollingTex.zw;



				
				
				// colour
				if (c.r <= 0.1) {
					o.Emission = fixed3(0.75, 0.75, 0.75);
					//o.Emission += c.rgb;
					o.Alpha = 0.75;
				}
				else {
					
					/*
					//o.Emission = lerp(_HighWaterColor, _LowWaterColor, (_WaterLevel - tex2D(_MainTex, IN.uv_MainTex).r)).rgb;
					o.Emission = lerp(_HighWaterColor, _LowWaterColor, (c.rgb)).rgb;
					//o.Emission = c.rgb;
					o.Alpha = lerp(_HighWaterColor, _LowWaterColor, (_WaterLevel - tex2D(_MainTex, IN.uv_MainTex).r)).a;
					*/

					// voronoi 
					float3 value = IN.worldPos.xyz / _CellSize;
					value.y += _Time.y * _TimeScale;
					float3 noise = voronoiNoise3D(value);

					float3 cellColor = rand1dTo3d(noise.y);
					float valueChange = fwidth(value.z) * 0.5;
					float isBorder = 1 - smoothstep(_BorderSize - valueChange, _BorderSize + valueChange, noise.z);
					//float3 color = lerp(_HighWaterColor, _LowWaterColor, isBorder).rgb;
					float3 color = lerp(_HighWaterColor, _LowWaterColor, cellColor.r).rgb;
					o.Emission = color;
					o.Alpha = lerp(_HighWaterColor, _LowWaterColor, (_WaterLevel - tex2D(_MainTex, IN.uv_MainTex).r)).a;


				}

				
			}
			else if (uw) {
				o.Emission = lerp(_LowUnderWaterColor, _HighUnderWaterColor, abs(heightDist / _ProjectionHeight)).rgb;
				if (tex2D(_ColourTex, IN.uv_ColourTex).a > 0.7) {
					o.Emission = tex2D(_ColourTex, IN.uv_ColourTex).rgb;
				}
				o.Alpha = 1;
			}
			else if (b) {
				float2 value = IN.worldPos.xz / _CellSize;
				float noise = voronoiNoise2D(value).y;
				o.Emission = lerp(_LowBeachColor, _HighBeachColor, abs(heightDist / _ProjectionHeight)).rgb  + (noise/10);
				if (tex2D(_ColourTex, IN.uv_ColourTex).a > 0.7) {
					o.Emission = tex2D(_ColourTex, IN.uv_ColourTex).rgb;
				}
				o.Alpha = 1; 
			}
			else if (g) {
				float2 value = IN.worldPos.xz / _CellSize;				
				float noise = voronoiNoise2D(value).y;
				o.Emission = lerp(_LowColor, _HighColor, abs(abs(heightDist / _ProjectionHeight) - _WaterLevel )).rgb + (noise / 10);
				if (tex2D(_ColourTex, IN.uv_ColourTex).a > 0.7) {
					o.Emission = tex2D(_ColourTex, IN.uv_ColourTex).rgb + (noise / 10);
				}

				// shadows
				// get direction of sun 
				// get every pixel in that direction and if value higher make shadow
				// get angle of sun
				// get a small length of pixels in the direction and if value higher make shadow
				// 

				// foliage
				// noise texture
				// if noise if above an amount 
				// have colour
				// the folliage height is based on the noise value


				o.Alpha = 1;
				//o.Emission = lerp(fixed3(0, 0, 0), o.Emission, saturate(c));
			}
			else if (c) {				
				o.Emission = fixed3(1, 1, 1);
				o.Alpha = 0.25;
			}
			else if (tt) {
				o.Emission = _TreeWoodColour;
				o.Alpha = 1.0;
			}
			else if (tl) {
				o.Emission = _TreeLeavesColour;
				o.Alpha = 1.0;
			}
			else{
				//clip(0);
				//o.Emission = fixed3(1, 0, 0);	
				//o.Alpha = 1;
			}
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}
