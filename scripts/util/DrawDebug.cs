using Godot;
using System;

public partial class DrawDebug : Node
{

	public MeshInstance3D DebugPoint (Vector3 position, Color color, float size=0.05f) {
		MeshInstance3D mi = new();
		SphereMesh sphereMesh = new();
		StandardMaterial3D material = new();
		
		mi.Mesh = sphereMesh;
		mi.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
		mi.Position = position;

		sphereMesh.Radius = size;
		sphereMesh.Height = size * 2f;
		sphereMesh.Material = material;

		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.AlbedoColor = color;

		GetTree().Root.AddChild(mi);

		return mi;
	}

	public MeshInstance3D DebugLine (Vector3 pos1, Vector3 pos2, Color color) {
		MeshInstance3D mi = new();
		ImmediateMesh mesh = new();
		StandardMaterial3D material = new();

		mi.Mesh = mesh;
		mi.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
		mesh.SurfaceAddVertex(pos1);
		mesh.SurfaceAddVertex(pos2);
		mesh.SurfaceEnd();

		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.AlbedoColor = color;

		GetTree().Root.AddChild(mi);

		return mi;
	}

}
