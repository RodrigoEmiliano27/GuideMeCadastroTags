using GuideMeCadastroTags.ViewModels;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace GuideMeCadastroTags.Views
{
    public partial class CadastroTagPage : ContentPage
    {

        public CadastroTagPage()
        {
            InitializeComponent();
            BindingContext = new CadastroTagViewModel();
        }

        protected override void OnDisappearing()
        {
            (BindingContext as CadastroTagViewModel).OnLeaving();
            base.OnDisappearing();
           
        }
        protected override void OnAppearing()
        {
            (BindingContext as CadastroTagViewModel).OnApearring();
            base.OnAppearing();
        }
    }
}