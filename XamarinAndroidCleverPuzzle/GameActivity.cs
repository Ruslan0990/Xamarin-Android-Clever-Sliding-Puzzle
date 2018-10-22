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
        int gridSize,  NumberOfMoves, highScore, NumChanges, tileWidth, gameViewWidth;
        string userName;
        int[][] board;
        PuzzleSolver solver = null;
        readonly Random rnd = new Random(Guid.NewGuid().GetHashCode());
        AudioManager audioManager;
        bool effects_enabled;
        TextView movesTextView;
        ISharedPreferences pref;
        ISharedPreferencesEditor editor;

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

            pref = PreferenceManager.GetDefaultSharedPreferences(this);
            editor = pref.Edit();
            effects_enabled = pref.GetBoolean("EffectPreference", true);
            gridSize = pref.GetInt("gridSize", 3);
            highScore = pref.GetInt("highScore", 0);

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
            userName  = pref.GetString("edit_text_preference", ""); 
            usernameTextView.Text = userName;
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
            tileWidth = (int) (gameViewWidth / gridSize) ;

            // Initialize ArrayLists
            tilesArray = new ArrayList();
            coordsArray = PuzzleMixer.GetPuzzleArray((gridSize * gridSize), gridSize, NumChanges, true, rnd);

            board = CreateBoard();

            int tileIndex = 0;
  
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (coordsArray[tileIndex] == 0) // this is the empty tile
                    {
                        emptySpot = new Point(row, col);
                        board[row][col] = gridSize * gridSize - 1;
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

                    NumberOfMoves++;
                    if (CheckCorrect() )   // player won!
                    {
                        bool newHighScore = false;
                        // update high score
                        if (NumberOfMoves < highScore || highScore == 0)
                        {
                            highScore = NumberOfMoves;
                            editor.PutInt("highScore", highScore);
                            editor.Apply();                           
                            newHighScore = true;
                        }
                            var fragmentTrans = FragmentManager.BeginTransaction();
                        fragmentTrans.AddToBackStack(null);
                        var gameOverDialog = GameOverDialogFragment.NewInstance(gridSize, NumberOfMoves, highScore , userName, newHighScore);
                        gameOverDialog.RestartGameEvent += Reset;
                        gameOverDialog.ExitGameEvent += OnExitGame;
                        gameOverDialog.Cancelable = false;
                        gameOverDialog.Show(fragmentTrans, "GameOverDialog");
                    }
                    ResetSolver();
                   
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
            mainLayout.ColumnCount = gridSize;
            mainLayout.RowCount = gridSize;
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
            int[][] board = new int[gridSize][];
            for (int i = 0; i < gridSize; ++i)
            {
                board[i] = new int[gridSize];
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
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
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
        private void OnExitGame(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(intent);
            Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
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