## Overview

This are a few classes created by me before to help me with decoupling classes. These are essentially signals with Godot, but I find it better to use the C# features to create an Event-Driven Architecture. These are static classes, so we can call upon them from anywhere and send any object through the events.

## How does it work

Basically we do the following for each event we want to create:

- We register an event with a name;
- We subscribe the objects that need to be warned when that event is called.
- We raise the event warning all objects that have subscribed.

Taking an example from a Checkers game, we can use events to create the gameplay. This means that an event will be called when a checker is pressed and if it is their player's turn, then it will become selected. Same can be said for the tiles on the board, each tile will raise an event when clicked which will then be consumed by the subscriber.

```csharp

#Board.cs
public override void _Ready{
  // Register the event
  EventRegistry.RegisterEvent("<EventName>", FunctionToCall);
  // subscribe to the event
  EventSubscriber.SubscribeToEvent("<EventName>", FunctionToCall);
}

// Function called when the event is Raised
public void FunctionToCall(object sender, object args) {
  if( args is Tile tile)
  {
    // do whatever you need to do with the tile...
  }
 }

#Tile.cs

public void OnTileClicked()
{
  // Raise the event
  EventRegistry.GetEventPublisher("<EventName>").RaiseEvent(this);
}

```

As we can see in the example code, the `Board` will register the event and attach it to `FunctionToCall`, then the Tile will raise the event when the tile its clicked, that will call `FunctionToCall` on the board sending the tile that was clicked.

## Benefits

- Decouples objects that produce events from objects that react to those events.
- Allows for a more modular and scalable system.
- Enables easy addition of new events and subscribers.
