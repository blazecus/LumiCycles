using Godot;
using System;

public partial class map : Node3D
{
	const int GRID_SIZE = 20;
	const float GRID_CELL_SIZE = 40.0f;
	const float GRID_EDGE_WIDTH = 5.0f;
	private Material grid_material = GD.Load<Material>("res://assets/materials/grid_material.tres");
	private MeshInstance3D mesh;
	private ImmediateMesh imesh;

	public override void _Ready()
	{
		mesh = GetNode<MeshInstance3D>("grid");
		imesh = new ImmediateMesh();
		mesh.Mesh = imesh;
		set_up_grid();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void set_up_grid(){
		imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, grid_material);

		for(int x = -GRID_SIZE + 1; x < GRID_SIZE; x++){
			for(int y = -GRID_SIZE + 1; y < GRID_SIZE; y++){
				draw_grid_line(generate_grid_line(new Vector3(x * GRID_CELL_SIZE, y * GRID_CELL_SIZE, -GRID_SIZE * GRID_CELL_SIZE), new Vector3(0,0,1)));
			}
		}

		for(int y = -GRID_SIZE + 1; y < GRID_SIZE; y++){
			for(int z = -GRID_SIZE + 1; z < GRID_SIZE; z++){
				draw_grid_line(generate_grid_line(new Vector3(-GRID_SIZE * GRID_CELL_SIZE, y * GRID_CELL_SIZE, z * GRID_CELL_SIZE), new Vector3(1,0,0)));
			}
		}

		for(int x = -GRID_SIZE + 1; x < GRID_SIZE; x++){
			for(int z = -GRID_SIZE + 1; z < GRID_SIZE; z++){
				draw_grid_line(generate_grid_line(new Vector3(x * GRID_CELL_SIZE, -GRID_SIZE * GRID_CELL_SIZE, z * GRID_CELL_SIZE), new Vector3(0,1,0)));
			}
		}

		imesh.SurfaceEnd();
	}

	private Godot.Collections.Array<Vector3> generate_grid_line(Vector3 start_pos, Vector3 dir){
		float total_grid_size = GRID_SIZE * GRID_CELL_SIZE * 2;

		Godot.Collections.Array<Vector3> points = new Godot.Collections.Array<Vector3>();
		
		points.Add(start_pos);

		if(dir.X == 1){
			points.Add(start_pos + new Vector3(total_grid_size,0,0));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,0));
			points.Add(start_pos + new Vector3(total_grid_size,GRID_EDGE_WIDTH,0));

			points.Add(start_pos + new Vector3(0,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(total_grid_size,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(total_grid_size,GRID_EDGE_WIDTH,GRID_EDGE_WIDTH));
		}
		else if(dir.Y == 1){
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,0));
			points.Add(start_pos + new Vector3(0,total_grid_size,0));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,total_grid_size,0));

			points.Add(start_pos + new Vector3(0,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(0,total_grid_size,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,total_grid_size,GRID_EDGE_WIDTH));
		}
		else{
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,0));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,0));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,GRID_EDGE_WIDTH,0));

			points.Add(start_pos + new Vector3(0,0,total_grid_size));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,total_grid_size));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,total_grid_size));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,GRID_EDGE_WIDTH,total_grid_size));
		}

		return points;
	}

	private void draw_grid_line(Godot.Collections.Array<Vector3> points){
		//bottom face

		imesh.SurfaceAddVertex(points[0]);
		imesh.SurfaceAddVertex(points[4]);
		imesh.SurfaceAddVertex(points[1]);

		imesh.SurfaceAddVertex(points[5]);
		imesh.SurfaceAddVertex(points[1]);
		imesh.SurfaceAddVertex(points[4]);

		//side faces
		imesh.SurfaceAddVertex(points[0]);
		imesh.SurfaceAddVertex(points[2]);
		imesh.SurfaceAddVertex(points[4]);

		imesh.SurfaceAddVertex(points[6]);
		imesh.SurfaceAddVertex(points[4]);
		imesh.SurfaceAddVertex(points[2]);


		imesh.SurfaceAddVertex(points[4]);
		imesh.SurfaceAddVertex(points[6]);
		imesh.SurfaceAddVertex(points[5]);

		imesh.SurfaceAddVertex(points[7]);
		imesh.SurfaceAddVertex(points[5]);
		imesh.SurfaceAddVertex(points[6]);	


		imesh.SurfaceAddVertex(points[5]);
		imesh.SurfaceAddVertex(points[7]);
		imesh.SurfaceAddVertex(points[1]);

		imesh.SurfaceAddVertex(points[3]);
		imesh.SurfaceAddVertex(points[1]);
		imesh.SurfaceAddVertex(points[7]);
		
		imesh.SurfaceAddVertex(points[1]);
		imesh.SurfaceAddVertex(points[3]);
		imesh.SurfaceAddVertex(points[0]);

		imesh.SurfaceAddVertex(points[2]);
		imesh.SurfaceAddVertex(points[0]);
		imesh.SurfaceAddVertex(points[3]);

		//top face
		imesh.SurfaceAddVertex(points[6]);
		imesh.SurfaceAddVertex(points[2]);
		imesh.SurfaceAddVertex(points[7]);

		imesh.SurfaceAddVertex(points[3]);
		imesh.SurfaceAddVertex(points[7]);
		imesh.SurfaceAddVertex(points[2]);
	}
}
