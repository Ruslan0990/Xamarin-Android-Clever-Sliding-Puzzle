using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using Android.Support.V7.App;
using Android.Widget;

namespace XamarinAndroidCleverPuzzle
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var newGameButton = FindViewById<Button>(Resource.Id.newGameButton);
            var settingsButton = FindViewById<Button>(Resource.Id.settingsButton);
            var highScoresButton = FindViewById<Button>(Resource.Id.highScoresButton);

            newGameButton.Click += (o, e) => {   
                StartActivity(typeof(GameActivity));
                OverridePendingTransition(Resource.Animation.abc_grow_fade_in_from_bottom, Resource.Animation.abc_fade_out);
            };
            highScoresButton.Click += (o, e) => {
                StartActivity(typeof(HighScoresActivity));
                OverridePendingTransition(Resource.Animation.abc_grow_fade_in_from_bottom, Resource.Animation.abc_fade_out);
            };            

            settingsButton.Click += (o, e) => {
                //var intent = new Intent(this, typeof(SettingsActivity));
                //StartActivity(intent);
                StartActivity(typeof(SettingsActivity));
                OverridePendingTransition(Resource.Animation.abc_grow_fade_in_from_bottom, Resource.Animation.abc_fade_out);
            };
        }
    }
        
}

