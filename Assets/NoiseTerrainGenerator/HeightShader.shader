Shader "Custom/HeightShader" {
	Properties {
		_LowColor ("Low Color", Color) = (1,1,1,1)
		_HighColor("High Color", Color) = (1,1,1,1)

		_HighWaterColor("High Water Color", Color) = (0,0,.5,1)
		_LowWaterColor("Low Water Color", Color) = (0,0,1,1)

		_HighVegetationColor("High Vegetation Color", Color) = (0,0.5,0,1)
		_LowVegetationColor("Low Vegetation Color", Color) = (0,1,0,1)

		_HighBeachColor("High Beach Color", Color) = (0,0,0,1)
		_LowBeachColor("Low Beach Color", Color) = (0,0,0,1)

		_HighUnderWaterColor("High Under Water Color", Color) = (0,0,0,1)
		_LowUnderWaterColor("Low Under Water Color", Color) = (0,0,0,1)

		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_VegatationTex("Vegetation Tex (RGB)", 2D) = "white" {}
		_ProjectionHeight ("Projection Height", Float) = 4
		_ProjectionRange("Projection Range", Float) = 0.004

		_CloudTex("Cloud Tex (RGB)", 2D) = "white" {}
		_WaterScrollingTex("Water Scrolling Tex (RGB)", 2D) = "white" {}
		_ScrollXSpeed("X", Range(-10,10)) = 2
		_ScrollYSpeed("Y", Range(-10,10)) = 3

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

		sampler2D _MainTex;
		sampler2D _VegatationTex;
		sampler2D _CloudTex;
		sampler2D _WaterScrollingTex;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_VegatationTex;
			float2 uv_CloudTex;
			float2 uv_WaterScrollingTex;
			float3 worldPos;
		};
				
		fixed4 _LowColor;
		fixed4 _HighColor;

		fixed4 _HighWaterColor;
		fixed4 _LowWaterColor;

		fixed4 _HighVegetationColor;
		fixed4 _LowVegetationColor;

		fixed4 _HighBeachColor;
		fixed4 _LowBeachColor;

		fixed4 _HighUnderWaterColor;
		fixed4 _LowUnderWaterColor;

		float _ProjectionHeight;
		float _ProjectionRange;
		float _ProjectionRadius;
		float _WaterLevel;
		float _VegetationLevel;
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
			bool v = false;
			if (tex2D(_VegatationTex, IN.uv_VegatationTex).r < _VegetationLevel && tex2D(_MainTex, IN.uv_MainTex).r > _WaterLevel && (IN.worldPos.y / _ProjectionHeight) >_WaterLevel) {
				v = true;
			}

			float height = tex2D(_MainTex, IN.uv_MainTex).r * _ProjectionHeight;
			float heightDist = height - IN.worldPos.y;
			
			bool g = false;
			if ((_ProjectionRange - abs(heightDist)) >0 || tex2D(_MainTex, IN.uv_MainTex).r > (IN.worldPos.y / _ProjectionHeight)) {
				g = true;
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
					//o.Emission = lerp(_HighWaterColor, _LowWaterColor, (_WaterLevel - tex2D(_MainTex, IN.uv_MainTex).r)).rgb;
					o.Emission = lerp(_HighWaterColor, _LowWaterColor, (c.rgb)).rgb;
					//o.Emission = c.rgb;
					o.Alpha = lerp(_HighWaterColor, _LowWaterColor, (_WaterLevel - tex2D(_MainTex, IN.uv_MainTex).r)).a;
				}
			}
			else if (uw) {
				o.Emission = lerp(_LowUnderWaterColor, _HighUnderWaterColor, abs(heightDist / _ProjectionHeight)).rgb;
				o.Alpha = 1;
			}
			else if (b) {
				o.Emission = lerp(_LowBeachColor, _HighBeachColor, abs(heightDist / _ProjectionHeight)).rgb;
				o.Alpha = 1; 
			}
			else if (g) {
				o.Emission = lerp(_LowColor, _HighColor, abs(heightDist / _ProjectionHeight)).rgb;
				o.Alpha = 1;
				//o.Emission = lerp(fixed3(0, 0, 0), o.Emission, saturate(c));
			}
			else if (v) {
				o.Emission = lerp(_HighVegetationColor, _LowVegetationColor, abs(heightDist / _ProjectionHeight)).rgb;
				o.Alpha = lerp(_HighVegetationColor, _LowVegetationColor, abs(heightDist / _ProjectionHeight)).a;
			}
			else if (c) {				
				o.Emission = fixed3(1, 1, 1);
				o.Alpha = 0.25;
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
