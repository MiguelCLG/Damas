using Godot;
using System;

public class RoomList : Panel
{
	[Export] PackedScene cellScene;
	[Export] PackedScene joinButtonScene;
	public override void _Ready()
	{
		//EventSubscriber.SubscribeToEvent("OnRoomCheck", OnDataReceived);
	}

	private void OnDataReceived(object sender, object args)
	{
		GD.Print("[ROOM LIST] - Data Received!");
		if (args is RoomInfoList roomInfoList)
		{
			foreach (RoomInfo room in roomInfoList.room_aggregate)
			{
				GridContainer container = GetNode<GridContainer>("%GridContainer");
				if (container.HasNode($"Room-{room.aggregate_value}"))
				{
					container.GetNode<PanelContainer>($"Room-{room.aggregate_value}").GetNode<Label>("%CellLabel").Text = room.aggregate_value.ToString();
					container.GetNode<PanelContainer>($"Cell-{room.aggregate_value}").GetNode<Label>("%CellLabel").Text = room.count.ToString();
				}
				else
				{
					// Criar as cells
					var agregateCell = cellScene.Instance<PanelContainer>();
					var countCell = cellScene.Instance<PanelContainer>();
					agregateCell.Name = $"Room-{room.aggregate_value}";
					countCell.Name = $"Cell-{room.aggregate_value}";
					container.AddChild(agregateCell);
					container.AddChild(countCell);
					agregateCell.GetNode<Label>("%CellLabel").Text = room.aggregate_value.ToString();
					countCell.GetNode<Label>("%CellLabel").Text = room.count.ToString();
					// Criar o botão
					var joinButton = joinButtonScene.Instance();
					//TODO: Connectar join button para um evento que manda o room e da join
					container.AddChild(joinButton);
					joinButton.Connect("pressed", this, nameof(OnJoinRoom), new Godot.Collections.Array() { room.aggregate_value });
				}


			}
		}
	}

	public void OnJoinRoom(int bidValue)
	{

		EventRegistry.GetEventPublisher("OnJoinRoom").RaiseEvent(bidValue);
	}

	public void OnClose()
	{
		Visible = false;
	}

}
