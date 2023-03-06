using Godot;
using System;

public partial class trail : StaticBody3D
{
	private MeshInstance3D mesh;
	private ImmediateMesh imesh;
	public Vector3 last_point1;
	public Vector3 last_point2;
	public Godot.Collections.Array<Vector3> points;
	private player player_scene;

	public void setup(player player_s){
		mesh = GetNode<MeshInstance3D>("mesh");
		imesh = (ImmediateMesh) mesh.Mesh;
		points = new Godot.Collections.Array<Vector3>();
		player_scene = player_s;
		// imesh.SurfaceAddVertex(new Vector3(0,0,0));
		// imesh.SurfaceAddVertex(new Vector3(0,1,0));
		// imesh.SurfaceAddVertex(new Vector3(1,0,0));
		// imesh.SurfaceEnd();
		// imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
		// imesh.SurfaceAddVertex(new Vector3(0,1,0));
		// imesh.SurfaceAddVertex(new Vector3(1,1,0));
		// imesh.SurfaceAddVertex(new Vector3(1,0,0));
		// imesh.SurfaceEnd();
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

	public void draw_mesh(){
		imesh.ClearSurfaces();
		if(points.Count > 4){
			imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
			for(int i = 0; i < points.Count - 3; i += 2){
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
			// imesh.SurfaceAddVertex(points[points.Count - 2]);
			// imesh.SurfaceAddVertex(points[points.Count - 1]);
			// imesh.SurfaceAddVertex(player_scene.trailbottom.GlobalPosition);

			// imesh.SurfaceAddVertex(player_scene.trailbottom.GlobalPosition);
			// imesh.SurfaceAddVertex(points[points.Count - 1]);
			// imesh.SurfaceAddVertex(points[points.Count - 2]);

			// imesh.SurfaceAddVertex(points[points.Count - 1]);
			// imesh.SurfaceAddVertex(player_scene.trailtop.GlobalPosition);
			// imesh.SurfaceAddVertex(player_scene.trailbottom.GlobalPosition);

			// imesh.SurfaceAddVertex(player_scene.trailbottom.GlobalPosition);
			// imesh.SurfaceAddVertex(player_scene.trailtop.GlobalPosition);
			// imesh.SurfaceAddVertex(points[points.Count - 1]);

			imesh.SurfaceEnd();
		}
	}
	public void add_section(Vector3 p1, Vector3 p2){
		/*CollisionPolygon3D next_collision1 = new CollisionPolygon3D();
		CollisionPolygon3D next_collision2 = new CollisionPolygon3D();
		next_collision1.Depth = 0.1f;
		next_collision2.Depth = 0.1f;
		Vector2[] poly1 = {last_point1, last_point2, p1};
		next_collision1.Polygon.*/
				// imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
		// imesh.SurfaceAddVertex(last_point1);
		// imesh.SurfaceAddVertex(last_point2);
		// imesh.SurfaceAddVertex(p1);

		// imesh.SurfaceAddVertex(last_point2);
		// imesh.SurfaceAddVertex(p2);
		// imesh.SurfaceAddVertex(p1);
		// imesh.SurfaceEnd();
		//points.Add(last_point1);
		//points.Add(last_point2);
		//points.Add(p1);

		//points.Add(last_point2);
		//points.Add(p2);
		//points.Add(p1);
		//points.Resize((int) Mesh.ArrayType.Max);
		//imesh.ClearSurfaces();
		//imesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, points);
		points.Add(p1);
		points.Add(p2);
		//GD.Print(points.Count);
		draw_mesh();
		set_last_points(p1,p2);
	}
}
