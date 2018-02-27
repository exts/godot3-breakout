using Godot;
using System;

namespace scripts 
{
    public class Main : Node
    {
        public PackedScene BrickScene;

        public Area2D GameBall;

        public Area2D GamePaddle;
        
        public Vector2 BallDirection;

        public Vector2 WindowSize;

        public override void _Ready()
        {
            WindowSize = GetViewport().GetSize();

            GamePaddle = GetNode("Paddle") as Paddle;

            GameBall = GetNode("Ball") as Ball;

            BallDirection += SetBallDirection();

            BrickScene = ResourceLoader.Load("res://brick.tscn") as PackedScene;
            
            GenerateBrickGrid(); 

            int bricks = 0;
            foreach(var child in GetChildren()) {
                if(child is Brick) {
                    // var brick = child as Brick;
                    ++bricks;
                }
            }

            GD.Print(bricks);
        }
        
        public override void _Process(float delta)
        {
            //Check wall collision
            CheckBallWallCollision();

            //move ball
            var position = new Vector2(BallDirection.x, BallDirection.y);
            position.x += delta * Ball.BALL_SPEED;
            position.y += delta * Ball.BALL_SPEED;
            GameBall.Position += position;

            //move paddle
            var paddle_pos = new Vector2();
            if(Input.IsActionPressed("ui_left")) {
                paddle_pos.x -= delta * Paddle.PADDLE_SPEED + 1;
            }

            if(Input.IsActionPressed("ui_right")) {
                paddle_pos.x += delta * Paddle.PADDLE_SPEED + 1;
            }

            GamePaddle.Position += paddle_pos;
        }

        public Vector2 SetBallDirection()
        {
            var r = new Random();

            int[] x_dir = {-1, 1};
            int[] y_dir = {-1, 1};

            return new Vector2(x_dir[r.Next(0, x_dir.Length)], y_dir[r.Next(0, y_dir.Length)]);
        }

        public void GenerateBrickGrid()
        {
            var window_size = GetViewport().GetSize();
            var bricks_per_row = (int) window_size.x/Brick.Width;
            string[] brick_types = {"green", "blue", "red"};

            int brick_row = 0;
            foreach(var brick_type in brick_types) {
                for(int br = 0; br < bricks_per_row; br++) {
                    var brick = BrickScene.Instance() as Brick;
                    var brick_pos = new Vector2(br * brick.Size.x, brick_row * brick.Size.y);
                    brick.Position = brick_pos;
                    brick.SetBrickType(brick_type);
                    brick.Connect("BrickDestroyed", this, "onBrickCollision");
                    AddChild(brick);
                }
                ++brick_row;
            }
        }

        public void onBrickCollision()
        {
            GD.Print("Collided!");
            ReverseBallDirection();

            var block_break_sound = GetNode("BlockBreakSound") as AudioStreamPlayer;
            var block_break_sound_timer = block_break_sound.GetNode("Timer") as Timer;
            block_break_sound.Play();
            block_break_sound_timer.Start();
        }

        public void onPaddleCollided(object obj)
        {
            // GD.Print("Got hit!");
            // GD.Print($"Ball: {GameBall.Position.x}, Paddle: {GamePaddle.Position.x}");
            var paddle_hit_sound = GetNode("PaddleHitSound") as AudioStreamPlayer;
            var paddle_hit_sound_timer = paddle_hit_sound.GetNode("Timer") as Timer;
            paddle_hit_sound.Play();
            paddle_hit_sound_timer.Start();

            //reverse ball into the other direction
            if(GameBall.Position.x < GamePaddle.Position.x && BallDirection.x > 0) {
                BallDirection = new Vector2(-BallDirection.x, -BallDirection.y);
            } else if(GameBall.Position.x > GamePaddle.Position.x && BallDirection.y > 0 && BallDirection.x < 0) {
                BallDirection = new Vector2(-BallDirection.x, -BallDirection.y);
            } else {
                // GD.Print("Go right");
                BallDirection = new Vector2(BallDirection.x, -BallDirection.y);
            }
        }

        private void CheckBallWallCollision()
        {
            if(GameBall.Position.x < 0 && BallDirection.x < 0) {
                BallDirection.x = -BallDirection.x;
            } else if(GameBall.Position.x > WindowSize.x && BallDirection.x > 0) {
                BallDirection.x = -BallDirection.x;
            }

            if(GameBall.Position.y < 0 && BallDirection.y < 0) {
                BallDirection.y = -BallDirection.y;
            }
        }

        private void ReverseBallDirection()
        {
            BallDirection = new Vector2(BallDirection.x, -BallDirection.y);
        }

        private void onStopPaddleSound()
        {
            var paddle_hit_sound = GetNode("PaddleHitSound") as AudioStreamPlayer;
            var paddle_hit_sound_timer = paddle_hit_sound.GetNode("Timer") as Timer;
            paddle_hit_sound.Stop();
            paddle_hit_sound_timer.Stop();
        }

        private void onStopBlockBreakSound()
        {
            var block_break_sound = GetNode("BlockBreakSound") as AudioStreamPlayer;
            var block_break_sound_timer = block_break_sound.GetNode("Timer") as Timer;
            block_break_sound.Stop();
            block_break_sound_timer.Stop();
        }
    }
}