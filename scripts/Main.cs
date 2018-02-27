using Godot;
using System;

// created a scripts namespace so our vs code would stop complaining classes didn't exist when they did.
namespace scripts 
{
    public class Main : Node
    {
        public Random rand = new Random();

        public Area2D GameBall;

        public Area2D GamePaddle;
        
        public Vector2 WindowSize;

        public Vector2 BallDirection;
        
        public PackedScene BrickScene;

        public Gameover GameOverHud;

        public bool game_running = true;

        public override void _Ready()
        {
            WindowSize = GetViewport().GetSize();

            //get gameball & ball nodes
            GameBall = GetNode("Ball") as Ball;
            GamePaddle = GetNode("Paddle") as Paddle;

            //setup game over signal
            GameOverHud = GetNode("GameOverHud") as Gameover;
            GameOverHud.Connect("StartGame", this, "StartGame");

            BallDirection += SetBallDirection();

            BrickScene = ResourceLoader.Load("res://brick.tscn") as PackedScene;
            
            GenerateBrickGrid(); 
        }
        
        public override void _Process(float delta)
        {
            if(game_running && IsGameOver()) {
                game_running = false;
                ShowGameOverHud();
            }

            if(game_running) {
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
        }

        public bool IsGameOver()
        {
            var bricks = GetBrickCount();
            if(bricks == 0) {
                return true;
            }

            if(GameBall.Position.y - Ball.HEIGHT/2 > WindowSize.y) {
                return true;
            }

            return false;
        }

        public void StartGame()
        {
            //reset ball position to center
            GameBall.Position = new Vector2(WindowSize.x / 2, 338);

            //set ball direction
            BallDirection = SetBallDirection();

            //reset paddle position
            GamePaddle.Position = new Vector2(WindowSize.x / 2, 456);

            //regenerate bricks
            GenerateBrickGrid();

            //show ball & paddle
            GameBall.Show();
            GamePaddle.Show();

            //hide hud
            GameOverHud.ShowOrHideHud(show: false);

            //tell the script the game's running
            game_running = true;
        }

        public void ShowGameOverHud()
        {
            //clean up bricks
            RemoveRemainingBricksFromGrid();

            //hide ball & paddle
            GameBall.Hide();
            GamePaddle.Hide();

            //game over or naw?
            var bricks = GetBrickCount();
            if(bricks > 0) {
                GameOverHud.SetLabelText("You lose!");
            } else {
                GameOverHud.SetLabelText("You Win!");
            }

            GameOverHud.ShowOrHideHud(show: true);
        }

        // here I wanted to make it so randomly we tell the ball
        // which direction to go so essentially left/right (x) or up/down (y)
        public Vector2 SetBallDirection()
        {
            int[] x_dir = {-1, 1};
            int[] y_dir = {-1, 1};

            return new Vector2(x_dir[rand.Next(0, x_dir.Length)], y_dir[rand.Next(0, y_dir.Length)]);
        }

        // here we regenerate our brick grid, we use a trick with the
        // animated sprite to switch between brick colors instead of actually
        // animating saving us a bit of time.
        public void GenerateBrickGrid()
        {
            var window_size = GetViewport().GetSize();
            var bricks_per_row = (int) window_size.x/Brick.Width;
            string[] brick_types = {"green", "blue", "red"};

            int brick_row = 0;
            foreach(var brick_type in brick_types) {
                for(int br = 0; br < bricks_per_row; br++) {
                    
                    // here we create a new instance of the scene, set its position
                    var brick = BrickScene.Instance() as Brick;
                    var brick_pos = new Vector2(br * brick.Size.x, brick_row * brick.Size.y);
                    brick.Position = brick_pos;

                    //this is where we switch the animation/brick color
                    brick.SetBrickType(brick_type);

                    //this is when we connect our brick collision signal with a local
                    // function inside of the Main class Main::onBrickCollision
                    brick.Connect("BrickDestroyed", this, "onBrickCollision");

                    //append brick to the scene
                    AddChild(brick);
                }
                ++brick_row;
            }
        }

        // we need to free all bricks from the scene generated by us
        // when the game is over and restarting
        public void RemoveRemainingBricksFromGrid()
        {
            foreach(var child in GetChildren()) {
                if(child is Brick) {
                    var node = child as Node;
                    node.QueueFree();
                }
            }
        }

        // this code tells us our ball collided with the ball
        // which is essentially responding to the collision
        // signal from the brick we originally created
        public void onBrickCollision()
        {
            ReverseBallDirection();

            var block_break_sound = GetNode("BlockBreakSound") as AudioStreamPlayer;
            var block_break_sound_timer = block_break_sound.GetNode("Timer") as Timer;
            block_break_sound.Play();
            block_break_sound_timer.Start();
        }

        public void onPaddleCollided(object obj)
        {
            // we want to play a sound, but we also want to start the timer so it knows when
            // to stop playing the paddle hit sound ;)
            var paddle_hit_sound = GetNode("PaddleHitSound") as AudioStreamPlayer;
            var paddle_hit_sound_timer = paddle_hit_sound.GetNode("Timer") as Timer;
            paddle_hit_sound.Play();
            paddle_hit_sound_timer.Start();

            //is the ball on the left side of the paddle and it's direction is positive reverse the X & Y directions
            // else if the ball is on the right side of the paddle and the ball is essentialling negatively increasing
            // then reverse iit in the opposite direction else just normally reverse the ball in a general verticle direction
            if(GameBall.Position.x < GamePaddle.Position.x && BallDirection.x > 0) {
                BallDirection = new Vector2(-BallDirection.x, -BallDirection.y);
            } else if(GameBall.Position.x > GamePaddle.Position.x && BallDirection.y > 0 && BallDirection.x < 0) {
                BallDirection = new Vector2(-BallDirection.x, -BallDirection.y);
            } else {
                BallDirection = new Vector2(BallDirection.x, -BallDirection.y);
            }
        }

        // some code to make sure the ball isn't going off the sceen
        // but we want to allow the ball to go off the height of the screen (or the bottom)
        // it's weird because the grid starts at the top left instead of the bottom left
        // essentially in reverse of what I'm used to.
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

        // we need to stop the paddle sound, so a trick I did was to call this after the timer
        // hits the length of the audio file. so essentially once the timer matches the audio clip
        // it stops, this applies to both timer signals that we connected.
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

        // count all the bricks in the scene that we spawned
        private int GetBrickCount()
        {
            int bricks = 0;
            foreach(var child in GetChildren()) {
                if(child is Brick) {
                    ++bricks;
                }
            }
            return bricks;
        }
    }
}