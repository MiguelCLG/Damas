using Godot;
using System;

public class RoomList : Panel
{
    [Export] PackedScene cellScene;
    [Export] PackedScene joinButtonScene;
    public override void _Ready()
    {
        EventSubscriber.SubscribeToEvent("OnRoomCheck", OnDataReceived);
    }

    private void OnDataReceived(object sender, object args)
    {
        GD.Print("[ROOM LIST] - Data Received!");
        if (args is RoomInfoList roomInfoList)
        {
            foreach (RoomInfo room in roomInfoList.room_aggregate)
            {
                // Criar as cells
                var agregateCell = cellScene.Instance();
                var countCell = cellScene.Instance();
                GridContainer container = agregateCell.GetNode<GridContainer>("%GridContainer");
                container.AddChild(agregateCell);
                container.AddChild(countCell);
                agregateCell.GetNode<Label>("%CellLabel").Text = room.agregate_value.ToString();
                countCell.GetNode<Label>("%CellLabel").Text = room.count.ToString();
                AddChild(agregateCell);

                // Criar o botão
                var joinButton = joinButtonScene.Instance();

                //TODO: Connectar join button para um evento que manda o room e da join
                container.AddChild(joinButton);

            }
        }
    }


    public void OnClose()
    {
        Visible = false;
    }

}
