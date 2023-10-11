using Godot;
using System;

public partial class NPC : AnimatedSprite3D
{

	public string state;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		state = "idle";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Play(state);
	}

	public virtual void Interact () {}

}
