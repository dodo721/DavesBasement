using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Wall : MeshInstance3D
{

	[Export]
	public Vector3 UnitScale {
		get {return _unitScale;}
		set {
			_unitScale = value;
			if (Engine.IsEditorHint() && autoUpdateMesh)
				UpdateMesh();
		}
	}
	[Export]
	public Vector3I UnitNumber {
		get {return _unitNumber - Vector3I.One;}
		set {
			_unitNumber = value + Vector3I.One;
			if (Engine.IsEditorHint() && autoUpdateMesh)
				UpdateMesh();
		}
	}

	[Export]
	public bool GenerateMesh {
		get {return false;}
		set {if (value) UpdateMesh();}
	}

	[Export]
	public bool autoUpdateMesh;

	private Vector3 _unitScale;
	private Vector3I _unitNumber;
	private CollisionShape3D _collisionShape;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_collisionShape = GetNode<CollisionShape3D>("RigidBody3D/CollisionShape3D");
		UpdateMesh();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void UpdateMesh () {
		Godot.Collections.Array surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);
		
		List<Vector3> verts = new();
		List<Vector2> uvs = new();
		List<Vector3> normals = new();
		List<int> indices = new();

		// For hard shading, meshes must be generated with separated faces
		GenerateXYFace(verts, uvs, normals, indices, 1, 0);
		GenerateXZFace(verts, uvs, normals, indices, -1, 0);
		GenerateZYFace(verts, uvs, normals, indices, -1, 0);

		GenerateXYFace(verts, uvs, normals, indices, -1, (_unitNumber.Z - 1) * _unitScale.Z);
		GenerateXZFace(verts, uvs, normals, indices, 1, (_unitNumber.Y - 1) * _unitScale.Y);
		GenerateZYFace(verts, uvs, normals, indices, 1, (_unitNumber.X - 1) * _unitScale.X);

		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

		ArrayMesh arrayMesh = Mesh as ArrayMesh;
		arrayMesh?.ClearSurfaces();
		arrayMesh?.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

		_collisionShape ??= GetNode<CollisionShape3D>("RigidBody3D/CollisionShape3D");
		if (_collisionShape?.Shape is BoxShape3D box) {
			Vector3 aabbSize = (_unitNumber - Vector3I.One) * _unitScale;
			box.Size = aabbSize;
			_collisionShape.Position = aabbSize / 2f;
		}
	}

	void GenerateXYFace (List<Vector3> verts, List<Vector2> uvs, List<Vector3> normals, List<int> indices, float direction, float z) {
		// Begin with Z=0 XY face
		int startIdx = verts.Count;
		// Vertices
		for (int x = 0; x < _unitNumber.X; x++) {
			for (int y = 0; y < _unitNumber.Y; y++) {
				verts.Add(new Vector3(x * _unitScale.X, y * _unitScale.Y, z * _unitScale.Z));
				normals.Add(new Vector3(0, 0, -1) * direction);
				uvs.Add(new Vector2(x, y));
			}
		}
		// Triangles
		for (int x = 0; x < _unitNumber.X - 1; x++) {
			for (int y = 0; y < _unitNumber.Y - 1; y++) {
				int bR = (x * _unitNumber.Y) + y + startIdx;
				int bL = ((x + 1) * _unitNumber.Y) + y + startIdx;
				int tR = bR + 1;
				int tL = bL + 1;
				int[] tempIndices = new int[6];
				tempIndices[0] = bR;
				tempIndices[1] = bL;
				tempIndices[2] = tR;
				tempIndices[3] = bL;
				tempIndices[4] = tL;
				tempIndices[5] = tR;
				if (direction < 0) {
					tempIndices = tempIndices.Reverse().ToArray();
				}
				indices.AddRange(tempIndices);
			}
		}
	}

	void GenerateXZFace (List<Vector3> verts, List<Vector2> uvs, List<Vector3> normals, List<int> indices, float direction, float y) {
		// Begin with Z=0 XY face
		int startIdx = verts.Count;
		// Vertices
		for (int x = 0; x < _unitNumber.X; x++) {
			for (int z = 0; z < _unitNumber.Z; z++) {
				verts.Add(new Vector3(x * _unitScale.X, y * _unitScale.Y, z * _unitScale.Z));
				normals.Add(new Vector3(0, 1, 0) * direction);
				uvs.Add(new Vector2(x, z));
			}
		}
		// Triangles
		for (int x = 0; x < _unitNumber.X - 1; x++) {
			for (int z = 0; z < _unitNumber.Z - 1; z++) {
				int bR = (x * _unitNumber.Z) + z + startIdx;
				int bL = ((x + 1) * _unitNumber.Z) + z + startIdx;
				int tR = bR + 1;
				int tL = bL + 1;
				int[] tempIndices = new int[6];
				tempIndices[0] = bR;
				tempIndices[1] = bL;
				tempIndices[2] = tR;
				tempIndices[3] = bL;
				tempIndices[4] = tL;
				tempIndices[5] = tR;
				if (direction < 0) {
					tempIndices = tempIndices.Reverse().ToArray();
				}
				indices.AddRange(tempIndices);
			}
		}
	}

	void GenerateZYFace (List<Vector3> verts, List<Vector2> uvs, List<Vector3> normals, List<int> indices, float direction, float x) {
		// Begin with Z=0 XY face
		int startIdx = verts.Count;
		// Vertices
		for (int z = 0; z < _unitNumber.Z; z++) {
			for (int y = 0; y < _unitNumber.Y; y++) {
				verts.Add(new Vector3(x * _unitScale.X, y * _unitScale.Y, z * _unitScale.Z));
				normals.Add(new Vector3(1, 0, 0) * direction);
				uvs.Add(new Vector2(z, y));
			}
		}
		// Triangles
		for (int z = 0; z < _unitNumber.Z - 1; z++) {
			for (int y = 0; y < _unitNumber.Y - 1; y++) {
				int bR = (z * _unitNumber.Y) + y + startIdx;
				int bL = ((z + 1) * _unitNumber.Y) + y + startIdx;
				int tR = bR + 1;
				int tL = bL + 1;
				int[] tempIndices = new int[6];
				tempIndices[0] = bR;
				tempIndices[1] = bL;
				tempIndices[2] = tR;
				tempIndices[3] = bL;
				tempIndices[4] = tL;
				tempIndices[5] = tR;
				if (direction < 0) {
					tempIndices = tempIndices.Reverse().ToArray();
				}
				indices.AddRange(tempIndices);
			}
		}
	}
}
