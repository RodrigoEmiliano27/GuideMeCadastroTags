using GuideMeCadastroTags.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace GuideMeCadastroTags.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}