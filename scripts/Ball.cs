using Godot;
using System;

namespace scripts
{
    public class Ball : Area2D
    {
        public const int WIDTH = 22;
        public const int HEIGHT = 22;
        public const int BALL_SPEED = 5;
    
        [Signal] public delegate void BallHit();

        public override void _Ready()
        {
        }

        public override void _Process(float delta)
        {
        }
    }
}
