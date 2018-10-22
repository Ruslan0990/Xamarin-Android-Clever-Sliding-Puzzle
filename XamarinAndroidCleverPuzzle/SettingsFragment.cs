using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Support.V7.Preferences;

namespace XamarinAndroidCleverPuzzle
{
    public class SettingsFragment : PreferenceFragmentCompat
    {
        Context context;
        Toast toast;
        Preference effPref, gridSize;

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.Preferences);
            context = Activity.ApplicationContext;
            
            effPref = FindPreference("EffectPreference");
            gridSize = FindPreference("gridSize");

            effPref.PreferenceClick += OnSettingsClicked;
            gridSize.PreferenceChange += OnSettingsChanged;
        }
        public override void OnPause()
        {
            base.OnPause();
            effPref.PreferenceClick -= OnSettingsClicked;
            gridSize.PreferenceChange -= OnSettingsChanged;
        }

        void OnSettingsChanged(object sender, Preference.PreferenceChangeEventArgs e)
        {
            toast = Toast.MakeText(context, Resource.String.changedSettings, ToastLength.Short);
            toast.Show();
        }

        void OnSettingsClicked(object sender, Preference.PreferenceClickEventArgs e)
        {
                toast = Toast.MakeText(context, Resource.String.changedSettings, ToastLength.Short);
                toast.Show();
        }
    }
}