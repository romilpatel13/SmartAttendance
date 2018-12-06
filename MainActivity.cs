using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using SmartAttendance.Model;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using Firebase.Xamarin.Auth;
using Android.Content;
using Android.Speech;

namespace SmartAttendance
{
    [Activity(Label = "Smart Attendance", Icon = "@drawable/app_logo",Theme ="@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        private EditText input_id, input_pwd;
        private ListView list_data;
        private ProgressBar circular_progress;

        private List<Account> list_users = new List<Account>();
        private ListViewAdapter adapter;
        private Account selectedAccount;


        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView textBox;
        private Button recButton;



        private const string FirebaseURL = "https://myfcmexample-98598.firebaseio.com/";
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            isRecording = false;
            SetContentView(Resource.Layout.Main);

            recButton = FindViewById<Button>(Resource.Id.btnRecord);
            textBox = FindViewById<TextView>(Resource.Id.rec_text);
            
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "Smart Attendance";
            SetSupportActionBar(toolbar);

            
            circular_progress = FindViewById<ProgressBar>(Resource.Id.circularProgress);
            input_id = FindViewById<EditText>(Resource.Id.sid);
            input_pwd = FindViewById<EditText>(Resource.Id.status);
            list_data = FindViewById<ListView>(Resource.Id.list_data);
            list_data.ItemClick += (s, e) =>
            {
                
                Account acc = list_users[e.Position];
                selectedAccount = acc;
                input_id.Text = acc.sid;
                input_pwd.Text = acc.status;
            };

            await LoadData();

           
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                
                var alert = new Android.Support.V7.App.AlertDialog.Builder(recButton.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    textBox.Text = "No microphone present";
                    recButton.Enabled = false;
                    return;
                });
                
                alert.Show();
            }
            else
                recButton.Click += delegate
                {
                    
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                        
                        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));

                        
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                       

                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                        StartActivityForResult(voiceIntent, VOICE);
                    }

                };
        }
        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultVal == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        input_id.Text = " ";
                        string textInput = input_id.Text + matches[0];
                        
                        
                        if (textInput.Length > 500)
                           textInput = textInput.Substring(0, 500);
                        input_id.Text = textInput;
                        input_pwd.Text = "Present";
                    }
                    else
                        input_id.Text = "No speech was recognised";
                    
                }
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }
        private async Task LoadData()
        {
            input_id.Text = " ";
            input_pwd.Text = " ";
            circular_progress.Visibility = ViewStates.Visible;
            list_data.Visibility = ViewStates.Invisible;

            var firebase = new FirebaseClient(FirebaseURL);
            var items = await firebase
                .Child("users")
                .OnceAsync<Account>();

            list_users.Clear();
            adapter = null;
            foreach(var item in items)
            {
                Account acc = new Account();
                acc.uid = item.Key;
                acc.sid = item.Object.sid;
                acc.status = item.Object.status;

                list_users.Add(acc);
            }
            adapter = new ListViewAdapter(this, list_users);
            adapter.NotifyDataSetChanged();
            list_data.Adapter = adapter;

            circular_progress.Visibility = ViewStates.Invisible;
            list_data.Visibility = ViewStates.Visible;

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if(id == Resource.Id.menu_add)
            {
                CreateUser();
            }
            else if (id == Resource.Id.menu_save) 
            {
                UpdateUser(selectedAccount.uid, input_id.Text, input_pwd.Text);
            }
            else if (id == Resource.Id.menu_remove)
            {
                DeleteUser(selectedAccount.uid);
            }
            

            return base.OnOptionsItemSelected(item);
        }

        private async void DeleteUser(string uid)
        {
            var firebase = new FirebaseClient(FirebaseURL);
            await firebase.Child("users").Child(uid).DeleteAsync();
       
            await LoadData();
        }

        private async void UpdateUser(string uid, string sid, string status)
        {
            var firebase = new FirebaseClient(FirebaseURL);
            await firebase.Child("users").Child(uid).Child("sid").PutAsync(sid);
            await firebase.Child("users").Child(uid).Child("status").PutAsync(status);

            await LoadData();
        }

        private async void CreateUser()
        {
            Account user = new Account();
            user.uid = String.Empty;
             user.sid = input_id.Text;
           
            user.status = "Present";

            var firebase = new FirebaseClient(FirebaseURL);

            
            var item = await firebase.Child("users").PostAsync<Account>(user);

            await LoadData();
        }
    }
}

