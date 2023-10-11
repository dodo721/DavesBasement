using Godot;
using System;
using System.Security;

[Tool]
public partial class Joint : Node3D
{

	[Export]
	public float smoked = 1f;
	[Export]
	public float smokeRate = 0.1f;
	[Export]
	public float endOffset;

	private MeshInstance3D _jointMI;
	private Node3D _end;
	private GpuParticles3D _smoke;
	private float _prevSmoked;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_jointMI = GetNode<MeshInstance3D>("Joint");
		_end = GetNode<Node3D>("End");
		_smoke = GetNode<GpuParticles3D>("End/Smoke");
		_prevSmoked = smoked;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// shader
		((ShaderMaterial)_jointMI.GetSurfaceOverrideMaterial(0)).SetShaderParameter("Smoked", smoked);
		// end positioning
		float endZ = endOffset - (_jointMI.GetAabb().GetLongestAxisSize() * smoked * _jointMI.Scale.Z);
		Vector3 endPos = _end.Position;
		endPos.Z = endZ;
		_end.Position = endPos;
		// smoke particles
		if (!Engine.IsEditorHint())
		{
			bool smoking = _prevSmoked != smoked;
			_smoke.Emitting = smoking;
			_prevSmoked = smoked;
		}
	}
}
