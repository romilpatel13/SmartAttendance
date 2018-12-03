using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SmartAttendance;

namespace SmartAttendance
{
    [Activity(Label = "Smart Attendance", MainLauncher = true, Icon = "@drawable/app_logo", Theme = "@style/AppTheme")]
    public class Home : Activity
    {
        EditText email;
        EditText password;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Home);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "Smart Attendance";
            email = FindViewById<EditText>(Resource.Id.inst_id);
            password = FindViewById<EditText>(Resource.Id.inst_pwd);
            var button = FindViewById<Button>(Resource.Id.btn);
            button.Click += Button_Click;
                }

        private void Button_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (password.Text == email.Length().ToString()+"@lambton")
            {
                Toast.MakeText(this, "Login successfully done!", ToastLength.Long).Show();
                StartActivity(typeof(MainActivity));
            }
            else
            {
                //Toast.makeText(getActivity(), "Wrong credentials found!", Toast.LENGTH_LONG).show();  
                Toast.MakeText(this, "Wrong credentials found!", ToastLength.Long).Show();
            }
        }
    }
}