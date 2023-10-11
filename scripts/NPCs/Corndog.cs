using Godot;
using System;

public partial class Corndog : NPC
{
    public override void Interact()
    {
        state = "talking";
    }
}
