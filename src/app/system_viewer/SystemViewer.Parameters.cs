using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Systems;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Spec handoff and validation helpers for the viewer.
/// The System Viewer no longer owns a parameter editor; it only reflects and validates the spec
/// that was used to generate the currently displayed system.
/// </summary>
public partial class SystemViewer
{
    private SolarSystemSpec BuildCurrentSpecFromControls()
    {
        if (_currentSpec != null)
        {
            return SolarSystemSpec.FromDictionary(_currentSpec.ToDictionary());
        }

        return new SolarSystemSpec((int)(GD.Randi() % 1000000), 1, 1);
    }

    private void ApplySpecToControls(SolarSystemSpec spec)
    {
        _currentSpec = SolarSystemSpec.FromDictionary(spec.ToDictionary());
        _currentGenerationIssues = SystemGenerationParameterValidator.Validate(_currentSpec);
    }

    private SolarSystemSpec? ExtractCurrentSpec(SolarSystem? system)
    {
        if (system != null && system.Provenance != null && system.Provenance.SpecSnapshot.Count > 0)
        {
            return SolarSystemSpec.FromDictionary(system.Provenance.SpecSnapshot);
        }

        return _currentSpec;
    }

    private void UpdateGenerationIssuesUi()
    {
    }

    private void ApplyTravellerDefaultsToControls()
    {
    }

    private void RefreshGenerationValidationFromControls()
    {
        _currentGenerationIssues = SystemGenerationParameterValidator.Validate(BuildCurrentSpecFromControls());
    }

    private void UpdatePermissivenessValueLabels()
    {
    }
}
