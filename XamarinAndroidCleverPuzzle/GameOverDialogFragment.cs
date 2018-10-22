using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using Android.Support.Graphics.Drawable;
using Android.App;
using Android.Content;
using Android.Graphics;

namespace XamarinAndroidCleverPuzzle
{
    public class GameOverDialogFragment : DialogFragment
    {

        public GameOverDialogFragment()
        {
        }

        public static GameOverDialogFragment NewInstance(int gridSize , int score, int highscore, string userName, bool newHighScore)
        {
            GameOverDialogFragment dialogFragment = new GameOverDialogFragment();
            Bundle args = new Bundle();
            args.PutInt("GridSize", gridSize);
            args.PutInt("Score", score);
            args.PutInt("HighScore", highscore);
            args.PutString("UserName", userName);
            args.PutBoolean("NewHighScore", newHighScore);
            dialogFragment.Arguments = args;
            
            return dialogFragment;                       
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {        
            View view = inflater.Inflate(Resource.Layout.GameOverView, container, false);
            
            var restartBtn = view.FindViewById<Button>(Resource.Id.restartBtn);
            restartBtn.Click += (object sender, EventArgs e) =>
            {
                Dismiss();
                RestartGameEvent?.Invoke(this, EventArgs.Empty);
            };
            var restartIcon = VectorDrawableCompat.Create(Resources, Resource.Drawable.ic_replay, view.Context.Theme);
            restartBtn.SetCompoundDrawablesWithIntrinsicBounds(restartIcon, null, null, null);

            var exitButton = view.FindViewById<Button>(Resource.Id.exitBtn);     
            exitButton.Click += (object sender, EventArgs e) =>
            {
                Dismiss();
                ExitGameEvent?.Invoke(this, EventArgs.Empty);
            };
            var exitIcon = VectorDrawableCompat.Create(Resources, Resource.Drawable.ic_exit, view.Context.Theme);
            exitButton.SetCompoundDrawablesWithIntrinsicBounds(exitIcon, null, null, null);
            
            var score = Arguments.GetInt("Score");
            var highScore = Arguments.GetInt("HighScore");
            var userName = Arguments.GetString("UserName");
            var gridSize = Arguments.GetInt("GridSize");
            var newHighScore = Arguments.GetBoolean("NewHighScore");
            

            var TitleTextView = view.FindViewById<TextView>(Resource.Id.titleTextView);
            TitleTextView.Text = "Congratulations " + userName + "!" ;             

            if (newHighScore)
            {
                var NewHSTextView = view.FindViewById<TextView>(Resource.Id.newHSTextView);
                NewHSTextView.Visibility = ViewStates.Visible;
                NewHSTextView.Text = "New High Score!";
                var podiumIcon = VectorDrawableCompat.Create(Resources, Resource.Drawable.ic_podium, view.Context.Theme);
                NewHSTextView.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, podiumIcon);
            }
            var GridSizeTextView = view.FindViewById<TextView>(Resource.Id.gridSizeTextView);
            GridSizeTextView.Text = "Grid Size: " + gridSize.ToString();

            var ScoreTextView = view.FindViewById<TextView>(Resource.Id.scoreTextView);
            ScoreTextView.Text = "Moves: " + score.ToString();

            var HSCTextView = view.FindViewById<TextView>(Resource.Id.highScoreTextView);
            HSCTextView.Text = "High Score: " + highScore.ToString();

            var shareBtn = view.FindViewById<Button>(Resource.Id.shareBtn);
            var shareIcon = VectorDrawableCompat.Create(Resources, Resource.Drawable.ic_share, view.Context.Theme);
            shareBtn.SetCompoundDrawablesWithIntrinsicBounds(shareIcon, null, null, null);
            shareBtn.Click += (object sender, EventArgs e) =>
            {
                using (var b = BitmapFactory.DecodeResource(Resources, Resource.Drawable.logo))
                {                   
                    using (var shareIntent = new Intent(Intent.ActionSend))
                    {
                        shareIntent.SetType("text/plain");
                        shareIntent.PutExtra(Intent.ExtraSubject, "Awesome game!");
                        shareIntent.PutExtra(Intent.ExtraText, "Hey, I just finished a match of Clever Sliding Puzzle in " + score.ToString() + " moves." );
                        shareIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        StartActivity(Intent.CreateChooser(shareIntent, "Share via"));
                    }
                }
            };
            return view;
        }
        public event EventHandler ExitGameEvent;
        public event EventHandler RestartGameEvent;
    }
}