using Godot;
using System;

public partial class JointLight : OmniLight3D
{

	public bool smoking;
	[ExportGroup("Smooth")]
	[Export]
	public float smokeLightUpSmooth;
	[Export]
	public float smokeLightDownSmooth;
	[ExportGroup("LightEnergy")]
	[Export]
	public float smokeLightEnergy;
	[Export]
	public float smokeLightEnergyMin;
	[ExportGroup("LightRange")]
	[Export]
	public float smokeLightRange;
	[Export]
	public float smokeLightRangeMin;

	public bool hitLightLimit = false;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		LightEnergy = Mathf.Max(Mathf.Lerp(LightEnergy, smoking ? smokeLightEnergy : 0f, (smoking ? smokeLightUpSmooth : smokeLightDownSmooth) * (float)delta), smokeLightEnergyMin);
		OmniRange = Mathf.Max(Mathf.Lerp(OmniRange, smoking ? smokeLightRange : 0f, (smoking ? smokeLightUpSmooth : smokeLightDownSmooth) * (float)delta), smokeLightRangeMin);
		hitLightLimit = LightEnergy >= smokeLightEnergy;
	}
}
