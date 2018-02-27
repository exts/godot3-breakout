using Godot;
using System;

public class Gameover : Node
{
    // our signal we'll be emitting to other scenes
    [Signal] public delegate void StartGame();

    // disable the hud by default
    public override void _Ready()
    {
        ShowOrHideHud(show: false);
    }

    // simple function to hide/show all the elements in our
    // hud scene so when the game IS over we can quickly show the HUD
    public void ShowOrHideHud(bool show = false)
    {
        //hide button & label by default
        var button = GetNode("Button") as Button;
        var label = GetNode("Label") as Label;

        if(show) {
            label.Show();
            button.Show();
        } else {
            label.Hide();
            button.Hide();
        }
    }

    // overwrite the default text label for our game hub
    // we want to be able to set it to "You lose!" or "You Win!"
    public void SetLabelText(string text)
    {
        var label = GetNode("Label") as Label;
        label.Text = text;
    }

    // when we press the button emit a signal so we can
    // use it in the main scene allowing us to restart our game
    public void OnButtonPress()
    {
        EmitSignal("StartGame");
    }
}
