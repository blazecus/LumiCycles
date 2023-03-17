using Godot;
using System;

public partial class start_screen : Node2D
{
	private PackedScene game_scene = ResourceLoader.Load<PackedScene>("res://game/world/world.tscn");
	private PackedScene join_scene = ResourceLoader.Load<PackedScene>("res://game/UI/join_screen.tscn"); 

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	private void _on_join_game_button_up()
	{
		GetTree().ChangeSceneToPacked(join_scene);
	}

	private void _on_host_game_button_up()
	{
		settings.Instance.host = true;
		GetTree().ChangeSceneToPacked(game_scene);
	}
}



