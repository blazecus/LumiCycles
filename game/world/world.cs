using System.Linq;
using Godot;
using System;

public partial class world : Node
{
	const int PORT = 9999;

	private bool host = true;

	private PackedScene player_scene = GD.Load<PackedScene>("res://game/player/player.tscn");
	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private CanvasLayer lobby_menu;

	public override void _EnterTree(){
		lobby_menu = GetNode<CanvasLayer>("CanvasLayer");
	}

	public override void _Ready()
	{
		if(settings.Instance.host){
			start_host();
		}
		else{
			start_client(settings.Instance.connection_address);
		}
	}
	
	public override void _Process(double delta)
	{
		
	}

	public void start_host(){
		peer.CreateServer(PORT);
		Multiplayer.MultiplayerPeer = peer;
		// listen to peer connections, and create new player for them
		Multiplayer.PeerConnected += CreatePlayer;
		// listen to peer disconnections, and destroy their players
		Multiplayer.PeerDisconnected += DestroyPlayer;	

		CreatePlayer(Multiplayer.GetUniqueId());
		upnp_setup();
	}

	public void start_client(string address){
		peer.CreateClient(address, PORT);
		Multiplayer.MultiplayerPeer = peer;
		lobby_menu.GetNode<Button>("start_button").Visible = false;
	}

	private void CreatePlayer(long id){
		player player = (player) player_scene.Instantiate();
		player.Name = id.ToString();
		GetNode<Node3D>("Players").AddChild(player);
		GD.Print(Multiplayer.GetPeers().Count());
	}

	private void DestroyPlayer(long id){
		GetNode<Node3D>("Players").GetNode<player>(id.ToString()).QueueFree();
		GD.Print(Multiplayer.GetPeers().Count());
	}

	private void upnp_setup(){
		Upnp upnp = new Upnp();
		int discover_result = upnp.Discover();
		//Debug.Assert(discover_result != (int) Upnp.UpnpResult.Success);
		//Debug.Assert(upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway());
		int map_result = upnp.AddPortMapping(PORT);
		//Debug.Assert(map_result == (int) Upnp.UpnpResult.Success);

		GD.Print("success, join address: " + upnp.QueryExternalAddress());
	}

	private void start_game(){
		lobby_menu.Visible = false;
		foreach(player p in GetNode<Node3D>("Players").GetChildren()){
			//set up better spawn system later along with map selection
			p.spawn_player(new Vector3(0,3,0));
		}
	}
	
	private void _on_start_button_button_up()
	{
		start_game();
	}
}
