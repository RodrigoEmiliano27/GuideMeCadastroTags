using GuideMeCadastroTags.ViewModels;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GuideMeCadastroTags.Views
{
    public partial class EstabelecimentoPage : ContentPage
    {
        public EstabelecimentoPage()
        {
            InitializeComponent();
            this.BindingContext = new EstabelecimentoViewModel();
        }
    }
}