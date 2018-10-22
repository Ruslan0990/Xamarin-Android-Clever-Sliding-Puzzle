
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;

namespace XamarinAndroidCleverPuzzle
{
    [Activity(Label = "HighScoresActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class HighScoresActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.highScores_layout);

            int gameViewWidth = Resources.DisplayMetrics.WidthPixels;
            int colWidth = (int)(gameViewWidth / 2);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "High Scores";

            var pref = PreferenceManager.GetDefaultSharedPreferences(this);
            var gridLayout = FindViewById<GridLayout>(Resource.Id.HighScoresGridId);

            TextView headertextView1 = new TextView(this)
            {
                LayoutParameters = GetTileLayoutParams(0, 0, colWidth),
                Text = "Grid Size",
                Gravity = GravityFlags.Center,
                TextSize = 20
            };               
            gridLayout.AddView(headertextView1);

            TextView headertextView2 = new TextView(this)
            {
                LayoutParameters = GetTileLayoutParams(0, 1, colWidth),
                Text = "High Score",
                Gravity = GravityFlags.Center,
                TextSize = 20
            };
            gridLayout.AddView(headertextView2);

            for (int grid = 2; grid < 8; grid++)
            {
                string highscoreString = "highScore" + grid.ToString() + "Grids";
                int highScore = pref.GetInt(highscoreString, 0);
              
                if(highScore==0)
                {
                    highscoreString = "none";
                }
                else
                {
                    highscoreString = highScore.ToString();
                }              
                var textView1 = new TextView(this)
                {
                    LayoutParameters = GetTileLayoutParams(grid - 1, 0, colWidth),
                    Text = grid.ToString(),
                    Gravity = GravityFlags.Center,
                    TextSize = 20
                };
                gridLayout.AddView(textView1);

                var textView2 = new TextView(this)
                {
                    LayoutParameters = GetTileLayoutParams(grid - 1, 1, colWidth),
                    Text = highscoreString,
                    Gravity = GravityFlags.Center,
                    TextSize = 20
                };
                gridLayout.AddView(textView2);
            }     
        }

        private GridLayout.LayoutParams GetTileLayoutParams(int x, int y, int colWidth)
        {
            GridLayout.Spec rowSpec = GridLayout.InvokeSpec(x);
            GridLayout.Spec colSpec = GridLayout.InvokeSpec(y);
            GridLayout.LayoutParams tileLayoutParams = new GridLayout.LayoutParams(rowSpec, colSpec)
            {
                Width = colWidth - 20,
                Height = ViewGroup.LayoutParams.WrapContent
            };
            tileLayoutParams.SetMargins(5, 5, 5, 5);
            return tileLayoutParams;
        }
    }

}