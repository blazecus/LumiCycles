using Godot;
using System;

/*-------------------------------------------
Synchronization plan
Instead of using multiplayersynchronizer, synchronizations will have to be done through rpcs,
since they have to be timed on when the network authority moves to the next chunk. 
chunks should be done fairly often, which means the mesh might have to be copied fairly often
-------------------------------------------*/


public partial class trail : StaticBody3D
{
	private const int CHUNK_SIZE = 100;
	public const float TRAIL_CHECK_INTERVAL = 0.1f;
	public const float TRAIL_LENGTH_INTERVAL = 0.5f;
	private MeshInstance3D mesh;
	private ImmediateMesh imesh;
	public Godot.Collections.Array<Vector3> points;
	public Godot.Collections.Array<Vector3> added_points;
	public int added_count = 0;
	private player parent_player;
	private int polygon_counter = 0;
	private int start_index = 0;
	private int network_authority_id = 0;
	private float trail_timer = 0.0f;
	private Vector3 last_player_pos = Vector3.Zero;

	public void setup(player player_s){
		mesh = GetNode<MeshInstance3D>("mesh");
		imesh = (ImmediateMesh) mesh.Mesh;
		points = new Godot.Collections.Array<Vector3>();
		added_points = new Godot.Collections.Array<Vector3>();
		parent_player = player_s;
		network_authority_id = Int32.Parse(parent_player.Name);
		SetMultiplayerAuthority(network_authority_id);
	}
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		float deltaf = (float) delta;
		trail_timer += deltaf;
		if(trail_timer > TRAIL_CHECK_INTERVAL){
			if((parent_player.Position - last_player_pos).Length() > TRAIL_LENGTH_INTERVAL){
				add_section(parent_player.trailbottom.GlobalPosition, parent_player.trailtop.GlobalPosition);
				last_player_pos = parent_player.Position;
			}
			trail_timer = 0.0f;
		}
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

	public void draw_last_rect(){
		if(added_points.Count >= 4){
			imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

			int i = added_points.Count - 4;

				imesh.SurfaceAddVertex(added_points[i]);
				imesh.SurfaceAddVertex(added_points[i+1]);
				imesh.SurfaceAddVertex(added_points[i+2]);

				imesh.SurfaceAddVertex(added_points[i+2]);
				imesh.SurfaceAddVertex(added_points[i+1]);
				imesh.SurfaceAddVertex(added_points[i]);

				imesh.SurfaceAddVertex(added_points[i+1]);
				imesh.SurfaceAddVertex(added_points[i+3]);
				imesh.SurfaceAddVertex(added_points[i+2]);

				imesh.SurfaceAddVertex(added_points[i+2]);
				imesh.SurfaceAddVertex(added_points[i+3]);
				imesh.SurfaceAddVertex(added_points[i+1]);
		
			imesh.SurfaceEnd();
		}
	}

	public void update_collision(){
		if(added_points.Count > 16){
			CollisionShape3D p = new CollisionShape3D();
			ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
			Vector3[] pc = new Vector3[6];
			int i = added_points.Count - 10;
			pc[0] = added_points[i];
			pc[1] = added_points[i+1];
			pc[2] = added_points[i+2];

			pc[3] = added_points[i+1];
			pc[4] = added_points[i+3];
			pc[5] = added_points[i+2];
			shape.Points = pc;
			p.Shape = shape;
			AddChild(p);
		}
	}

	public void add_section(Vector3 p1, Vector3 p2){
		if(!IsMultiplayerAuthority()){
			added_points.Add(p1);
			added_points.Add(p2);
			update_collision();
			draw_last_rect();
		}
		else{
			//Network authority
			added_points.Add(p1);
			added_points.Add(p2);
			update_collision();
			polygon_counter++;
			if(polygon_counter % CHUNK_SIZE == 0){
				authority_sync();
			}
			else{
				draw_last_rect();
			}
		}
	}

	private void authority_sync(){
		imesh.ClearSurfaces();
		foreach(Vector3 point in added_points){
			points.Add(point);
		}
		Rpc("sync_trail", added_points);
		added_points.Clear();
		draw_mesh(0);
	}
		
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void sync_trail(Godot.Collections.Array<Vector3> sync_points){
		//update collisions - remove client side
		for(int i = 0; i < added_points.Count / 2; i++){
			GetChild<CollisionShape3D>(GetChildCount() - 1).QueueFree();
		}
		//add new from sync_points
		for(int i = 0; i < sync_points.Count - 4; i += 2){
			CollisionShape3D p = new CollisionShape3D();
			ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
			Vector3[] pc = new Vector3[6];
			pc[0] = sync_points[i];
			pc[1] = sync_points[i+1];
			pc[2] = sync_points[i+2];

			pc[3] = sync_points[i+1];
			pc[4] = sync_points[i+3];
			pc[5] = sync_points[i+2];
			shape.Points = pc;
			p.Shape = shape;
			AddChild(p);
		}

		//update points and mesh
		foreach(Vector3 point in sync_points){
			points.Add(point);
		}
		added_points.Clear();
		draw_mesh(0);
	}
}
