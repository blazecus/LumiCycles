using Godot;
using System;

public partial class booster : Area3D
{
	private void _on_body_entered(Node3D body){
		if(body.HasMethod("speed_up")){
			player p = (player) body;
			p.speed_up();
		}
	}
}
