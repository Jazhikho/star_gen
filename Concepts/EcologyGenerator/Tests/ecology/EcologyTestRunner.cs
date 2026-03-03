using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StarGen.Tests.Ecology
{
	/// <summary>
	/// Test runner for ecology tests.
	/// Discovers and runs all test methods in registered test classes.
	/// </summary>
	public partial class EcologyTestRunner : Control
	{
		private RichTextLabel _outputLabel = null!;
		private int _passCount;
		private int _failCount;
		private List<string> _failures = new List<string>();

		private static readonly Type[] TestClasses = new Type[]
		{
			typeof(EcologyGeneratorTests),
			typeof(EcologyConstraintsTests),
			typeof(EcologyDeterminismTests)
		};

		public override void _Ready()
		{
			_outputLabel = GetNode<RichTextLabel>("OutputLabel");
			RunAllTests();
		}

		private void RunAllTests()
		{
			_passCount = 0;
			_failCount = 0;
			_failures.Clear();

			Log("[b][color=cyan]═══════════════════════════════════════[/color][/b]");
			Log("[b][color=cyan]     ECOLOGY GENERATOR TEST SUITE      [/color][/b]");
			Log("[b][color=cyan]═══════════════════════════════════════[/color][/b]\n");

			foreach (Type testClass in TestClasses)
			{
				RunTestClass(testClass);
			}

			Log("\n[b]═══════════════════════════════════════[/b]");
			Log("[b]Results: [color=green]" + _passCount + " passed[/color], [color=red]" + _failCount + " failed[/color][/b]");
			Log("[b]═══════════════════════════════════════[/b]");

			if (_failures.Count > 0)
			{
				Log("\n[color=red][b]Failures:[/b][/color]");
				foreach (string failure in _failures)
				{
					Log("  [color=red]• " + failure + "[/color]");
				}
			}
		}

		private void RunTestClass(Type testClass)
		{
			Log("\n[b][color=yellow]▶ " + testClass.Name + "[/color][/b]");

			object? instance = Activator.CreateInstance(testClass);
			if (instance == null)
			{
				_failCount += 1;
				Log("  [color=red]✗ Failed to create test instance[/color]");
				return;
			}
			MethodInfo[] methods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance);

			foreach (MethodInfo method in methods)
			{
				if (method.Name.StartsWith("Test") && method.GetParameters().Length == 0)
				{
					RunTest(instance, method);
				}
			}
		}

		private void RunTest(object instance, MethodInfo method)
		{
			try
			{
				method.Invoke(instance, null);
				_passCount += 1;
				Log("  [color=green]✓[/color] " + method.Name);
			}
			catch (TargetInvocationException ex)
			{
				_failCount += 1;
				Exception innerEx = ex.InnerException ?? ex;
				Log("  [color=red]✗[/color] " + method.Name + ": " + innerEx.Message);
				_failures.Add((method.DeclaringType?.Name ?? "?") + "." + method.Name + ": " + innerEx.Message);
			}
			catch (Exception ex)
			{
				_failCount += 1;
				Log("  [color=red]✗[/color] " + method.Name + ": " + ex.Message);
				_failures.Add((method.DeclaringType?.Name ?? "?") + "." + method.Name + ": " + ex.Message);
			}
		}

		private void Log(string message)
		{
			_outputLabel.Text += message + "\n";
			string printed = message
				.Replace("[b]", string.Empty)
				.Replace("[/b]", string.Empty)
				.Replace("[color=green]", string.Empty)
				.Replace("[color=red]", string.Empty)
				.Replace("[color=yellow]", string.Empty)
				.Replace("[color=cyan]", string.Empty)
				.Replace("[/color]", string.Empty);
			GD.Print(printed);
		}
	}
}
