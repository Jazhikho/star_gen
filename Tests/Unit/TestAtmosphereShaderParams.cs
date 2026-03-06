#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.Rendering;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for AtmosphereShaderParams.
/// </summary>
public static class TestAtmosphereShaderParams
{
	/// <summary>
	/// Creates a test body with atmosphere.
	/// </summary>
	private static CelestialBody CreateTestBody(
		double pressurePa = 101325.0,
		double scaleHeightM = 8500.0,
		double greenhouse = 1.15,
		Godot.Collections.Dictionary composition = null
	)
	{
		CelestialBody body = new CelestialBody();
		body.Type = CelestialType.Type.Planet;
		body.Name = "Test Planet";

		body.Physical = new PhysicalProps();
		body.Physical.MassKg = 5.972e24;
		body.Physical.RadiusM = 6.371e6;

		if (composition == null || composition.Count == 0)
		{
			composition = new Godot.Collections.Dictionary { { "N2", 0.78 }, { "O2", 0.21 }, { "Ar", 0.01 } };
		}

		body.Atmosphere = new AtmosphereProps(
			pressurePa,
			scaleHeightM,
			composition,
			greenhouse
		);

		return body;
	}

	/// <summary>
	/// Tests earth like @params.
	/// </summary>
	public static void TestEarthLikeParams()
	{
		CelestialBody body = CreateTestBody();
		Godot.Collections.Dictionary @params = AtmosphereShaderParams.GetParams(body);

		if (!@params.ContainsKey("u_atmosphereColor"))
		{
			throw new InvalidOperationException("Should have atmosphere color");
		}
		if (!@params.ContainsKey("u_density"))
		{
			throw new InvalidOperationException("Should have density");
		}
		if (!@params.ContainsKey("u_falloff"))
		{
			throw new InvalidOperationException("Should have falloff");
		}
		if (!@params.ContainsKey("u_scatterStrength"))
		{
			throw new InvalidOperationException("Should have scatter strength");
		}

		double density = @params["u_density"].AsDouble();
		if (density <= 0.5 || density >= 1.5)
		{
			throw new InvalidOperationException("Earth-like density should be moderate");
		}
	}

	/// <summary>
	/// Tests thin atmosphere.
	/// </summary>
	public static void TestThinAtmosphere()
	{
		CelestialBody body = CreateTestBody(610.0, 11000.0, 1.0);
		Godot.Collections.Dictionary @params = AtmosphereShaderParams.GetParams(body);

		double density = @params["u_density"].AsDouble();
		if (density >= 0.1)
		{
			throw new InvalidOperationException("Thin atmosphere should have low density");
		}

		double terminator = @params["u_terminatorSoftness"].AsDouble();
		if (terminator >= 0.15)
		{
			throw new InvalidOperationException("Thin atmosphere should have sharp terminator");
		}
	}

	/// <summary>
	/// Tests thick atmosphere.
	/// </summary>
	public static void TestThickAtmosphere()
	{
		Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "CO2", 0.96 }, { "N2", 0.03 } };
		CelestialBody body = CreateTestBody(9200000.0, 15000.0, 3.5, composition);
		Godot.Collections.Dictionary @params = AtmosphereShaderParams.GetParams(body);

		double density = @params["u_density"].AsDouble();
		if (density <= 1.5)
		{
			throw new InvalidOperationException("Thick atmosphere should have high density");
		}

		double greenhouse = @params["u_greenhouseIntensity"].AsDouble();
		if (greenhouse <= 0.5)
		{
			throw new InvalidOperationException("Strong greenhouse should have high intensity");
		}

		double terminator = @params["u_terminatorSoftness"].AsDouble();
		if (terminator <= 0.2)
		{
			throw new InvalidOperationException("Thick atmosphere should have soft terminator");
		}
	}

	/// <summary>
	/// Tests greenhouse effect.
	/// </summary>
	public static void TestGreenhouseEffect()
	{
		CelestialBody bodyNoGh = CreateTestBody(101325.0, 8500.0, 1.0);
		Godot.Collections.Dictionary paramsNoGh = AtmosphereShaderParams.GetParams(bodyNoGh);

		CelestialBody bodyGh = CreateTestBody(101325.0, 8500.0, 2.5);
		Godot.Collections.Dictionary paramsGh = AtmosphereShaderParams.GetParams(bodyGh);

		double ghNone = paramsNoGh["u_greenhouseIntensity"].AsDouble();
		double ghStrong = paramsGh["u_greenhouseIntensity"].AsDouble();

		if (ghNone >= 0.1)
		{
			throw new InvalidOperationException("No greenhouse should have low intensity");
		}
		if (ghStrong <= 0.5)
		{
			throw new InvalidOperationException("Strong greenhouse should have high intensity");
		}
	}

	/// <summary>
	/// Tests co2 atmosphere color.
	/// </summary>
	public static void TestCo2AtmosphereColor()
	{
		Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "CO2", 0.95 }, { "N2", 0.05 } };
		CelestialBody body = CreateTestBody(101325.0, 8500.0, 1.5, composition);
		Godot.Collections.Dictionary @params = AtmosphereShaderParams.GetParams(body);

		Color color = @params["u_atmosphereColor"].AsColor();
		if (color.R <= color.B)
		{
			throw new InvalidOperationException("CO2 atmosphere should have warm tones");
		}

		Color ghColor = @params["u_greenhouseColor"].AsColor();
		if (ghColor.R <= ghColor.B)
		{
			throw new InvalidOperationException("CO2 greenhouse glow should be orange-red");
		}
	}

	/// <summary>
	/// Tests methane atmosphere.
	/// </summary>
	public static void TestMethaneAtmosphere()
	{
		Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "N2", 0.95 }, { "CH4", 0.05 } };
		CelestialBody body = CreateTestBody(150000.0, 20000.0, 1.2, composition);
		Godot.Collections.Dictionary @params = AtmosphereShaderParams.GetParams(body);

		Color ghColor = @params["u_greenhouseColor"].AsColor();
		if (ghColor.G <= 0.7)
		{
			throw new InvalidOperationException("Methane greenhouse should have yellow tint");
		}
	}

	/// <summary>
	/// Tests h2 he atmosphere.
	/// </summary>
	public static void TestH2HeAtmosphere()
	{
		Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "H2", 0.86 }, { "He", 0.14 } };
		CelestialBody body = CreateTestBody(100000.0, 27000.0, 1.0, composition);
		Godot.Collections.Dictionary @params = AtmosphereShaderParams.GetParams(body);

		double scatter = @params["u_scatterStrength"].AsDouble();
		if (scatter >= 1.0)
		{
			throw new InvalidOperationException("H2/He atmosphere should scatter less");
		}
	}

	/// <summary>
	/// Tests no atmosphere.
	/// </summary>
	public static void TestNoAtmosphere()
	{
		CelestialBody body = new CelestialBody();
		body.Type = CelestialType.Type.Planet;
		body.Physical = new PhysicalProps();

		Godot.Collections.Dictionary @params = AtmosphereShaderParams.GetParams(body);

		if (!@params.ContainsKey("u_density"))
		{
			throw new InvalidOperationException("Should have default density");
		}
	}

	/// <summary>
	/// Tests should render atmosphere true.
	/// </summary>
	public static void TestShouldRenderAtmosphereTrue()
	{
		CelestialBody body = CreateTestBody();
		if (!AtmosphereShaderParams.ShouldRenderAtmosphere(body))
		{
			throw new InvalidOperationException("Earth-like should render atmosphere");
		}
	}

	/// <summary>
	/// Tests should render atmosphere false no atmo.
	/// </summary>
	public static void TestShouldRenderAtmosphereFalseNoAtmo()
	{
		CelestialBody body = new CelestialBody();
		body.Type = CelestialType.Type.Planet;
		body.Physical = new PhysicalProps();

		if (AtmosphereShaderParams.ShouldRenderAtmosphere(body))
		{
			throw new InvalidOperationException("No atmosphere should not render");
		}
	}

	/// <summary>
	/// Tests should render atmosphere false too thin.
	/// </summary>
	public static void TestShouldRenderAtmosphereFalseTooThin()
	{
		CelestialBody body = CreateTestBody(50.0, 8500.0, 1.0);
		if (AtmosphereShaderParams.ShouldRenderAtmosphere(body))
		{
			throw new InvalidOperationException("Very thin atmosphere should not render");
		}
	}

	/// <summary>
	/// Tests falloff from scale height.
	/// </summary>
	public static void TestFalloffFromScaleHeight()
	{
		CelestialBody bodyLarge = CreateTestBody(100000.0, 27000.0, 1.0);
		bodyLarge.Physical.RadiusM = 69911000.0;

		CelestialBody bodySmall = CreateTestBody(101325.0, 8500.0, 1.0);

		Godot.Collections.Dictionary paramsLarge = AtmosphereShaderParams.GetParams(bodyLarge);
		Godot.Collections.Dictionary paramsSmall = AtmosphereShaderParams.GetParams(bodySmall);

		double falloffLarge = paramsLarge["u_falloff"].AsDouble();
		double falloffSmall = paramsSmall["u_falloff"].AsDouble();

		if (falloffLarge >= falloffSmall)
		{
			throw new InvalidOperationException("Larger scale height should have lower falloff");
		}
	}

	/// <summary>
	/// Tests sun glow strength.
	/// </summary>
	public static void TestSunGlowStrength()
	{
		CelestialBody bodyThin = CreateTestBody(5000.0, 8500.0, 1.0);
		Godot.Collections.Dictionary paramsThin = AtmosphereShaderParams.GetParams(bodyThin);

		CelestialBody bodyThick = CreateTestBody(500000.0, 8500.0, 1.0);
		Godot.Collections.Dictionary paramsThick = AtmosphereShaderParams.GetParams(bodyThick);

		double glowThin = paramsThin["u_sunGlowStrength"].AsDouble();
		double glowThick = paramsThick["u_sunGlowStrength"].AsDouble();

		if (glowThick <= glowThin)
		{
			throw new InvalidOperationException("Thicker atmosphere should have stronger sun glow");
		}
	}
}
