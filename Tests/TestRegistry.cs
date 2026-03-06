#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.Tests.Framework;

namespace StarGen.Tests
{
	/// <summary>
	/// Shared C# test manifest for the primary headless and interactive harness paths.
	/// </summary>
	public static class TestRegistry
	{
		/// <summary>
		/// Registers the headless-safe C# suites.
		/// </summary>
		public static void RunHeadlessSuites(DotNetTestRunner runner)
		{
			DotNetNativeTestSuite.RunHeadless(runner);
		}

		/// <summary>
		/// Registers the full interactive C# suite set, including scene-only tests.
		/// </summary>
		public static void RunInteractiveSuites(DotNetTestRunner runner)
		{
			DotNetNativeTestSuite.RunInteractive(runner);
		}
	}
}
