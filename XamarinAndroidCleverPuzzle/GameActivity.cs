using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Views;
using System.Collections;
using System;
using Android.Content;
using XamarinAndroidCleverPuzzle.Solver;
using System.Collections.Generic;
using Android.Animation;
using Android.Preferences;
using Android.Media;
using Android.Content.PM;

namespace XamarinAndroidCleverPuzzle
{

    [Activity(Label = "GameActivity" ,  ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameActivity : Activity
    {
        GridLayout mainLayout;
        ArrayList tilesArray;
        int[] coordsArray;
        Point emptySpot;
        int nx, ny, NumberOfMoves, NumChanges, tileWidth, gameViewWidth;
        int[][] board;
        PuzzleSolver solver = null;
        readonly Random rnd = new Random(Guid.NewGuid().GetHashCode());
        AudioManager audioManager;
        bool effects_enabled;
        TextView movesTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.game_layout);

            var resetButton = FindViewById<Button>(Resource.Id.resetButtonId);
            resetButton.Click += Reset;

            var helpButton = FindViewById<Button>(Resource.Id.helpButtonId);
            helpButton.Click += ShowHelp;           

            mainLayout = FindViewById<GridLayout>(Resource.Id.gameGridLayoutId);
            gameViewWidth = Resources.DisplayMetrics.WidthPixels;
            NumChanges = 5;

            var pref = PreferenceManager.GetDefaultSharedPreferences(this);
            effects_enabled = pref.GetBoolean("EffectPreference", true);
            nx = pref.GetInt("gridSize", 3);  ny = nx;

            audioManager = (AudioManager)GetSystemService(AudioService);
            if (effects_enabled)
            {                
                int max_volume = audioManager.GetStreamMaxVolume(Stream.System);
                audioManager.SetStreamVolume(Stream.System, max_volume, 0);
            }
            else
            {
                audioManager.SetStreamVolume(Stream.System, 0, 0);
            }
            var usernameTextView = FindViewById<TextView>(Resource.Id.UsernameTextId);
            usernameTextView.Text = pref.GetString("edit_text_preference", "" );
            movesTextView = FindViewById<TextView>(Resource.Id.movesTextId);
            movesTextView.Text = string.Format("Moves: {0}", 0);

            SetGameView();
            MakeTiles();
        }

        private void MakeTiles()
        {
            if (effects_enabled)
            {
                audioManager.PlaySoundEffect(SoundEffect.Standard);
            }
            // Clean up the GridLayout
            mainLayout.RemoveAllViews();

            // Calculate the width/height of the tiles
            tileWidth = (int) (gameViewWidth / nx) ;

            // Initialize ArrayLists
            tilesArray = new ArrayList();
            coordsArray = PuzzleMixer.GetPuzzleArray((nx * ny), ny, NumChanges, true, rnd);

            board = CreateBoard();

            int tileIndex = 0;
  
            for (int row = 0; row < nx; row++)
            {
                for (int col = 0; col < ny; col++)
                {
                    if (coordsArray[tileIndex] == 0) // this is the empty tile
                    {
                        emptySpot = new Point(row, col);
                        board[row][col] = ny * nx - 1;
                        tileIndex++;
                        continue;
                    }
                    board[ row][col] = coordsArray[tileIndex] - 1;

                    // Create the tile (TextTileView)
                    TextTileView textTile = new TextTileView(this);

                    // Set the tile with the layout parameter
                    textTile.LayoutParameters = GetTileLayoutParams(row, col);

                    textTile.SetBackgroundColor(Color.Green);
                    textTile.Text = coordsArray[tileIndex].ToString();
                    textTile.SetTextColor(Color.Black);
                    textTile.TextSize = 40;
                    textTile.Gravity = GravityFlags.Center;
                    textTile.PositionX = row;
                    textTile.PositionY = col;

                    // Subscribe to the tile's Touch event handler
                    textTile.Touch += TileTouched;

                    // Add the tile to the game's grid layout
                    mainLayout.AddView(textTile);

                    // Keep the tile and its coordenate in the arrays
                    tilesArray.Add(textTile);
                   
                    tileIndex++;
                }
            }
            ResetSolver();
            NumberOfMoves = 0;
            movesTextView.Text = string.Format("Moves: {0}", 0);
        }

        private void TileTouched(object sender, View.TouchEventArgs e)
        {            
            // Check if the touch has finished
            if (e.Event.Action == MotionEventActions.Up)
            {
                TextTileView view = (TextTileView)sender;
                // Calculate the distance between the touched tile and the empty spot
                double yDiff = (view.PositionX - emptySpot.X);
                double xDiff = (view.PositionY - emptySpot.Y);
  
                // If they're adjacent, move the tile
                if ( (Math.Abs(xDiff) == 1 && yDiff == 0) || (xDiff == 0 && Math.Abs(yDiff) == 1) )
                {
                    int emptyX = emptySpot.X;
                    int emptyY = emptySpot.Y;
                    int textPosX = view.PositionX;
                    int textPosY = view.PositionY;

                    view.LayoutParameters = GetTileLayoutParams(emptyX, emptyY);
        
                    emptySpot.X = textPosX;
                    emptySpot.Y = textPosY;
                    view.PositionX = emptyX;
                    view.PositionY = emptyY;

                    int tmp = board[textPosX][textPosY];
                    board[textPosX][textPosY] = board[emptyX][emptyY];
                    board[emptyX][emptyY] = tmp;


                    if (CheckCorrect() )   // player won!
                    {
                         MakeTiles();
                    }
                    ResetSolver();
                    NumberOfMoves++;
                    movesTextView.Text = string.Format("Moves: {0}", NumberOfMoves.ToString());
                    if (effects_enabled)
                    {
                        audioManager.PlaySoundEffect(SoundEffect.KeyClick);
                    }
                }
            }
        }
        
        private GridLayout.LayoutParams GetTileLayoutParams(int x, int y)
        {
            // Create the specifications that establish in which row and column the tile is going to be rendered
            GridLayout.Spec rowSpec = GridLayout.InvokeSpec(x);
            GridLayout.Spec colSpec = GridLayout.InvokeSpec(y);

            // Create a new layout parameter object for the tile using the previous specs
            GridLayout.LayoutParams tileLayoutParams = new GridLayout.LayoutParams(rowSpec, colSpec);
            tileLayoutParams.Width = tileWidth - 10;
            tileLayoutParams.Height = tileWidth - 10;
            tileLayoutParams.SetMargins(5, 5, 5, 5);
            return tileLayoutParams;
        }

        private void SetGameView()
        {
            mainLayout.ColumnCount = nx;
            mainLayout.RowCount = ny;
            mainLayout.LayoutParameters = new LinearLayout.LayoutParams(gameViewWidth, gameViewWidth);
            mainLayout.SetBackgroundColor(Color.Gray);
        }

        private void Reset(object sender, EventArgs e)
        {
            MakeTiles();
        }

        private void ResetSolver()
        {
            solver = new PuzzleSolver(board);
            solver.run();
        }

        private int[][] CreateBoard()
        {
            int[][] board = new int[nx][];
            for (int i = 0; i < nx; ++i)
            {
                board[i] = new int[ny];
            }
            return board;
        }

        private void ShowHelp(object sender, EventArgs e)
        {
            if (solver == null)
            {
                return;
            }

            List<SolutionMove> solution = solver.get_solution();
            if (solution.Count > 0)
            {
                SolutionMove hintMove = solution[0];
                int node_id = board[hintMove.move_y][hintMove.move_x] + 1;

                // find the correct textTile and make it blink
                TextTileView textTile = (TextTileView)tilesArray[ Array.IndexOf(coordsArray, node_id)];
 
                ValueAnimator colorAnim = ObjectAnimator.OfInt(textTile, "BackgroundColor", Color.DarkGreen, Color.Green);
                colorAnim.SetDuration(350);
                colorAnim.SetEvaluator(new ArgbEvaluator());
                colorAnim.RepeatCount = 2;
                colorAnim.RepeatMode = ValueAnimatorRepeatMode.Reverse;
                colorAnim.Start();
            }
        }

        //check if board is sorted

        private bool CheckCorrect()
        {
            int tileIndex=0;
            for (int row = 0; row < nx; row++)
            {
                for (int col = 0; col < ny; col++)
                {
                    if (board[row][col] != tileIndex)
                    {
                        return false;
                    }
                    tileIndex++;
                }
            }
            return true;
        }

        private class TextTileView : TextView
        {
            public int PositionX { get; set; }
            public int PositionY { get; set; }

            public TextTileView(Context context) : base(context)
            {
            }
        }

    }
}