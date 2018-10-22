using Android.App;
using Android.OS;
using Android.Content.PM;
using Android.Support.V7.App;
using Android.Views;

namespace XamarinAndroidCleverPuzzle
{
    [Activity(Label = "@string/settings",  ScreenOrientation = ScreenOrientation.Portrait)]
    public class SettingsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SettingsLayout);

            Android.Support.V4.App.Fragment preferenceFragment = new SettingsFragment();
            Android.Support.V4.App.FragmentTransaction ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.pref_container, preferenceFragment);
            ft.Commit();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}