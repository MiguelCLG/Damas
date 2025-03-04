using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    [Export] public int NumPlayers = 40;

    private List<AudioStreamPlayer> available = new List<AudioStreamPlayer>();
    private List<AudioStreamPlayer> activePlayers = new List<AudioStreamPlayer>();

    private Queue<AudioOptionsResource> queue = new Queue<AudioOptionsResource>(); // Queue now holds AudioOptionsResource

    private Dictionary<Node, AudioStreamPlayer> nodeToPlayer = new Dictionary<Node, AudioStreamPlayer>();

    public override void _Ready()
    {
        // Create the pool of AudioStreamPlayer nodes
        for (int i = 0; i < NumPlayers; i++)
        {
            var player = new AudioStreamPlayer();
            AddChild(player);
            available.Add(player);
            player.Connect("finished", this, nameof(OnStreamFinished), new Godot.Collections.Array() { player });
        }
    }

    private void OnStreamFinished(AudioStreamPlayer player)
    {
        available.Add(player); // Make the player available again
        UnregisterPlayer(player); // Unregister from the active ones
    }

    private void RegisterPlayer(AudioStreamPlayer player)
    {
        if (!activePlayers.Contains(player))
        {
            activePlayers.Add(player);
        }
    }

    public void UnregisterPlayer(AudioStreamPlayer player)
    {
        if (activePlayers.Contains(player))
        {
            activePlayers.Remove(player);
        }
    }

    public void PauseAllSounds()
    {
        foreach (var player in activePlayers)
        {
            GD.Print($"Player {player.Name} - IsPlaying: {player.Playing}");
            if (player.Playing)
            {
                player.StreamPaused = true;
                GD.Print($"Pausing player {player.Name}");
            }
        }
    }
    public void PauseAllSoundsFromBus(string busName)
    {
        foreach (var player in activePlayers)
        {
            GD.Print($"Player {player.Name} - IsPlaying: {player.Playing}");
            if (player.Playing && player.Bus == busName)
            {
                player.StreamPaused = true;
                GD.Print($"Pausing player {player.Name}");
            }
        }
    }

    public void UnpauseAllSounds()
    {
        foreach (var player in activePlayers)
        {
            if (player.StreamPaused)
            {
                player.StreamPaused = false;
            }
        }
    }
    public void Play(AudioOptionsResource options, Node requestingNode)
    {
        if (options != null && options.AudioStream != null)
        {
            queue.Enqueue(options);
            // Track the node and associate the player
            if (!nodeToPlayer.ContainsKey(requestingNode))
            {
                nodeToPlayer.Add(requestingNode, available[0]); // Assign the player to the node
            }
        }
        else
        {
            GD.PrintErr("Invalid AudioOptionsResource provided.");
        }
    }

    public void StopSound(Node requestingNode)
    {
        // Stop the sound and clean up the player reference
        if (nodeToPlayer.ContainsKey(requestingNode))
        {
            var player = nodeToPlayer[requestingNode];
            player.Stop();
            available.Add(player);  // Add the player back to available
            nodeToPlayer.Remove(requestingNode);  // Remove from the dictionary
        }
        else
        {
            GD.PrintErr("No sound playing for the given node.");
        }
    }

    public override void _Process(float delta)
    {
        // Play a queued sound if any players are available
        if (queue.Count > 0 && available.Count > 0)
        {
            var options = queue.Dequeue();
            var player = available[0];
            if (player.Playing)
                player.Stop();

            // Set up player properties from AudioOptionsResource
            player.Stream = options.AudioStream;
            player.Bus = options.BusName;
            player.VolumeDb = options.VolumeDb;
            player.PitchScale = options.PitchScale;
            player.Autoplay = false; // Ensure autoplay is off so manual control works
            player.StreamPaused = options.Mute;

            // Handle 3D sound if specified
            if (options.Is3D)
            {
                var audioPlayer3D = new AudioStreamPlayer3D
                {
                    Stream = options.AudioStream,
                    Position = options.Position,
                    Bus = options.BusName,
                    UnitDb = options.VolumeDb,
                    PitchScale = options.PitchScale,
                    StreamPaused = options.Mute,
                    Autoplay = false,
                };
                AddChild(audioPlayer3D);
                audioPlayer3D.Play();
            }
            else
            {
                player.Play();
                if (options.StartOffset > 0)
                {
                    player.Seek(options.StartOffset);
                }
            }

            // TODO: This fadeout stuff isnt working as intended.
            //? It seems that the fade out duration of the sound resource is setting when the fadeout starts.
            //? I want it to define the time it starts but before the end, if its 20s audio and I set fadeout to 20, I want it to start
            //? fading at 18
            // Handle fade-out duration
            // Ensure audio stream duration is known and fade-out duration is valid
            double streamLength = player.Stream.GetLength();
            float fadeStartTime = (float)(streamLength - options.FadeOutDuration);
            // GD.Print($"Stream length: {streamLength}, Fade starts at: {fadeStartTime}");

            if (fadeStartTime > 0 && options.FadeOutDuration > 0)
            {
                // Create a timer to trigger the fade-out
                Timer fadeOutTimer = new Timer();
                fadeOutTimer.WaitTime = fadeStartTime;
                fadeOutTimer.OneShot = true;
                AddChild(fadeOutTimer);
                void fadeOutAction()
                {
                    SceneTreeTween tween = GetTree().CreateTween();
                    tween.TweenProperty(player, "volume_db", -80, options.FadeOutDuration)
                        .SetTrans(Tween.TransitionType.Linear)
                        .SetEase(Tween.EaseType.InOut);

                    void finishedAction()
                    {
                        player.Stop();
                    }
                    ;
                    tween.Connect("finished", this, nameof(finishedAction));
                }
                ;
                fadeOutTimer.Connect("timeout", this, nameof(fadeOutAction));

                fadeOutTimer.Start();
            }
            RegisterPlayer(player);
            available.RemoveAt(0); // Remove player from available list
        }
    }
}
