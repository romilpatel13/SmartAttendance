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
            //Add toolbar
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "Smart Attendance";
            SetSupportActionBar(toolbar);

            //View
            circular_progress = FindViewById<ProgressBar>(Resource.Id.circularProgress);
            input_id = FindViewById<EditText>(Resource.Id.name);
            input_pwd = FindViewById<EditText>(Resource.Id.email);
            list_data = FindViewById<ListView>(Resource.Id.list_data);
            list_data.ItemClick += (s, e) =>
            {
                Account acc = list_users[e.Position];
                selectedAccount = acc;
                input_id.Text = acc.name;
                input_pwd.Text = acc.email;
            };

            await LoadData();

            //var authProvider = new FirebaseAuthProvider(new FirebaseConfig(""));
            //var auth = authProvider.CreateUserWithEmailAndPasswordAsync("eddydn@gmail.com", "1234");

            //var resetPass = authProvider.SendPasswordResetEmailAsync("eddydn@gmail.com");
            //var signIn = authProvider.SignInWithEmailAndPasswordAsync("eddydn@gmail.com", "1234");
            //signIn.
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
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
                    // change the text on the button
                    //recButton.Text = "End Recording";
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        // create the intent and start the activity
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                        // put a message on the modal dialog
                        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));

                        // if there is more then 1.5s of silence, consider the speech over
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                        // you can specify other languages recognised here, for example
                        // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
                        // if you wish it to recognise the default Locale language and German
                        // if you do use another locale, regional dialects may not be recognised very well

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
                        
                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                           textInput = textInput.Substring(0, 500);
                        input_id.Text = textInput;
                        input_pwd.Text = "Present";
                    }
                    else
                        input_id.Text = "No speech was recognised";
                    // change the text back on the button
                   // recButton.Text = "Start Recording";
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
                acc.name = item.Object.name;
                acc.email = item.Object.email;

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
            else if (id == Resource.Id.menu_save) // Update
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

        private async void UpdateUser(string uid, string name, string email)
        {
            var firebase = new FirebaseClient(FirebaseURL);
            await firebase.Child("users").Child(uid).Child("name").PutAsync(name);
            await firebase.Child("users").Child(uid).Child("email").PutAsync(email);

            await LoadData();
        }

        private async void CreateUser()
        {
            Account user = new Account();
            user.uid = String.Empty;
             user.name = input_id.Text;
           // user.name = textBox.Text;
            //user.email = input_pwd.Text;
            user.email = "Present";

            var firebase = new FirebaseClient(FirebaseURL);

            //Add item
            var item = await firebase.Child("users").PostAsync<Account>(user);

            await LoadData();
        }
    }
}

