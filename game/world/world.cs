using System.Security.Cryptography;
using System.Reflection.Metadata;
using System.Linq;
using Godot;
using System;

public partial class world : Node
{
	const int PORT = 8888;

	private PackedScene start_screen = GD.Load<PackedScene>("res://game/UI/start_screen.tscn");
	private PackedScene player_scene = GD.Load<PackedScene>("res://game/player/player.tscn");

	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private CanvasLayer lobby_menu;
	private Label info_label;
	private bool host = true;

	public override void _EnterTree(){
		lobby_menu = GetNode<CanvasLayer>("CanvasLayer");
		info_label = lobby_menu.GetNode<Label>("Label");
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
		lobby_menu.GetNode<Label>("debug").Text = GetNode<Node3D>("Players").GetChildren().Count().ToString();
		lobby_menu.GetNode<Label>("numplayers").Text = Multiplayer.GetPeers().Count().ToString();
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
		lobby_menu.GetNode<Button>("start_button").Visible = false;
		Error connection = peer.CreateClient(address, PORT);
		if(connection != Error.Ok){
			lobby_menu.GetNode<Label>("Label").Text = "connection failed!";
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		lobby_menu.GetNode<Label>("label").Text = "lobby " + peer.Host.ToString();
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
		//Debug.Assert(discover_result == (int) Upnp.UpnpResult.Success);
		if(discover_result != (int) Upnp.UpnpResult.Success){
			throw new ApplicationException("discovery failed!");
		}
		//Debug.Assert(upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway());
		if(!(upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway())){
			throw new ApplicationException("gateway invalid!");
		}
		int map_result = upnp.AddPortMapping(PORT);
		//Debug.Assert(map_result == (int) Upnp.UpnpResult.Success);
		if(map_result != (int) Upnp.UpnpResult.Success){
			throw new ApplicationException("port mapping failed!");
		}
		GD.Print("success, join address: " + upnp.QueryExternalAddress());
		info_label.Text = "Address: " + upnp.QueryExternalAddress();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	private void start_game(){
		lobby_menu.Visible = false;
		foreach(player p in GetNode<Node3D>("Players").GetChildren()){
			//set up better spawn system later along with map selection
			p.spawn_player(new Vector3(0,3,0));
		}
	}
	
	private void _on_start_button_button_up()
	{
		Rpc("start_game");
		start_game();
	}
	
	private void _on_back_button_button_up()
	{
		GetTree().ChangeSceneToPacked(start_screen);
	}

	private void _on_multiplayer_spawner_spawned(){
		lobby_menu.GetNode<Label>("debug").Text = Multiplayer.GetPeers().Count().ToString();
	}
}

