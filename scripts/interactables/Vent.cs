using Godot;
using System;

public partial class Vent : Interactable
{
	public override void Interact()
	{
		GetNode<AnimationTree>("AnimationTree").Set("parameters/trigger/blend_amount", 1f);
	}
}
