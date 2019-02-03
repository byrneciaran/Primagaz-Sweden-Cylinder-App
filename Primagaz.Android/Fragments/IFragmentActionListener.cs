using System.Threading.Tasks;
using Android.Support.V4.App;
using Android.Views;

namespace Primagaz.Android
{
    public interface IFragmentActionListener
    {
        void DisplayMenu();
        void Pop();
        void NavigateToSync();
        void RequestSync();
        void PopToRoot();
        void ReplaceFragment(Fragment fragment,string name);
        void NavigateBackToFragment(string name);
        void HideSoftInput();
        void ShowSoftInput(View view);
        void NavigateToFragment(BaseFragment fragment, string name);
        void ToggleDrawer();
        void SetDisplayUsername(string username);
    }
}
