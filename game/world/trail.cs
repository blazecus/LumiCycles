using Godot;
using System;

public partial class trail : StaticBody3D
{
	private const int CHUNK_SIZE = 100;
	private MeshInstance3D mesh;
	private ImmediateMesh imesh;
	public Vector3 last_point1;
	public Vector3 last_point2;
	public Godot.Collections.Array<Vector3> points;
	private player player_scene;
	private int polygon_counter = 0;
	private int start_index = 0;

	public void setup(player player_s){
		mesh = GetNode<MeshInstance3D>("mesh");
		imesh = (ImmediateMesh) mesh.Mesh;
		points = new Godot.Collections.Array<Vector3>();
		player_scene = player_s;
	}
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void set_last_points(Vector3 p1, Vector3 p2){
		last_point1 = p1;
		last_point2 = p2;
	}

	public void draw_mesh(int start_index){
		if(points.Count > 4){
			imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
			for(int i = start_index; i < points.Count - 3; i += 2){
				imesh.SurfaceAddVertex(points[i]);
				imesh.SurfaceAddVertex(points[i+1]);
				imesh.SurfaceAddVertex(points[i+2]);

				imesh.SurfaceAddVertex(points[i+2]);
				imesh.SurfaceAddVertex(points[i+1]);
				imesh.SurfaceAddVertex(points[i]);

				imesh.SurfaceAddVertex(points[i+1]);
				imesh.SurfaceAddVertex(points[i+3]);
				imesh.SurfaceAddVertex(points[i+2]);

				imesh.SurfaceAddVertex(points[i+2]);
				imesh.SurfaceAddVertex(points[i+3]);
				imesh.SurfaceAddVertex(points[i+1]);
			}
			imesh.SurfaceEnd();
		}
	}

	public void update_collision(){
		if(points.Count > 16){
			CollisionShape3D p = new CollisionShape3D();
			ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
			Vector3[] pc = new Vector3[6];
			int i = points.Count - 10;
			pc[0] = points[i];
			pc[1] = points[i+1];
			pc[2] = points[i+2];

			pc[3] = points[i+1];
			pc[4] = points[i+3];
			pc[5] = points[i+2];
			shape.Points = pc;
			p.Shape = shape;
			AddChild(p);
		}
	}
	public void add_section(Vector3 p1, Vector3 p2){
		points.Add(p1);
		points.Add(p2);
		update_collision();
		polygon_counter++;
		if(polygon_counter % 200 == 0){
			imesh.ClearSurfaces();
			start_index = points.Count - 2;
			draw_mesh(0);
		}
		else{
			draw_mesh(start_index);
		}
		set_last_points(p1,p2);
	}
}
