using Godot;
using System;

namespace scripts 
{
    public class Brick : Area2D
    {
        [Export] public const int Width = 64;
        [Export] public const int Height = 32;

        // our signal that we emit telling the main scene when a brick breaks
        [Signal] public delegate void BrickDestroyed();

        public Vector2 Size = new Vector2(64, 32);
        
        public override void _Ready()
        {
        }

        // trick to use an animated sprite for multiple enemies or in our case
        // using the animated scene types as individual brick colors :P
        public void SetBrickType(string brick_type)
        {
            var BrickSprite = GetNode("AnimatedSprite") as AnimatedSprite;  
            BrickSprite.Animation = brick_type;
        }

        public void onBrickCollision(object obj) 
        {
            //check if brick was hit by the ball
            var areas = GetOverlappingAreas();
            foreach(var area in areas) {
                if(area is Ball) {
                    GD.Print("Ball hit me!");

                    //emit signal to be used in the main scene
                    EmitSignal("BrickDestroyed");

                    //delete brick
                    QueueFree();
                }
            }
        }
    }
}