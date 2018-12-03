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
        EditText iid;
        EditText pwd;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            
            SetContentView(Resource.Layout.Home);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "Smart Attendance";
            iid = FindViewById<EditText>(Resource.Id.inst_id);
            pwd = FindViewById<EditText>(Resource.Id.inst_pwd);
            var button = FindViewById<Button>(Resource.Id.btn);
            button.Click += Button_Click;
                }

        private void Button_Click(object sender, EventArgs e)
        {
            
            if (pwd.Text == iid.Length().ToString()+"@lambton")
            {
                Toast.MakeText(this, "Login successfully done!", ToastLength.Long).Show();
                StartActivity(typeof(MainActivity));
            }
            else
            {
                  
                Toast.MakeText(this, "Wrong credentials found!", ToastLength.Long).Show();
            }
        }
    }
}