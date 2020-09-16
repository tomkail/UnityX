fixed4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten) {
	fixed4 c;
	c.rgb = s.Albedo;
	c.a = s.Alpha;
	return c;
}