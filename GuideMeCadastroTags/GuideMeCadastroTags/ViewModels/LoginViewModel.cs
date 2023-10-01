using GuideMeCadastroTags.DAO;
using GuideMeCadastroTags.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace GuideMeCadastroTags.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public string Login { get; set; }
        public string Senha { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            var login = Login;
            var senha = Senha;

            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            var estabelecimento = Utils.APIHelper.GetEstabelecimentoInfo(new Utils.LoginRequestTO(login, senha));
            if (estabelecimento != null)
            {
                DadosSecao.Estabelecimento = estabelecimento;
                await Shell.Current.GoToAsync($"//{nameof(EstabelecimentoPage)}");
            }
            else
                await App.Current.MainPage.DisplayAlert("Falha", "Login ou usuário inválidos!", "Ok");

        }
    }
}
