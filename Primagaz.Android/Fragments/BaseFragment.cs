using System;
using System.Collections.Generic;
using Android.Content;

using Fragment = Android.Support.V4.App.Fragment;
using Activity = Android.Support.V4.App.FragmentActivity;

namespace Primagaz.Android
{
    public class BaseFragment : Fragment
    {
        protected IFragmentActionListener _fragmentActionListener;
        protected Dictionary<string, object> _values;


        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            InitialiseFragmentActionListener(Activity);
        }


        /// <summary>
        /// Initialises the fragment action listener.
        /// </summary>
        /// <param name="activity">Activity.</param>
        void InitialiseFragmentActionListener(Activity activity)
        {
            try
            {
                _fragmentActionListener = (IFragmentActionListener)activity;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException(String.Format("{0} must implement IFragmentActionListener", activity));
            }
        }
    }
}