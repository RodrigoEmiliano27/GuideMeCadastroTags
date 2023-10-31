using GuideMeCadastroTags.DAO;
using GuideMeCadastroTags.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace GuideMeCadastroTags.ViewModels
{
    public class CadastroTagViewModel : BaseViewModel
    {
        public Command SalvarCommand { get; }
        public string Nome { get; set; }

        private string _TagID ;  // Backing store

        public string TagID

        {
            get => _TagID;

            set
            {
                _TagID = value.Replace(" ", "");
            }
        }

        public CadastroTagViewModel()
        {
            SalvarCommand = new Command(OnSalvarClickec);

            

        }

        public void OnApearring()
        {
            MessagingCenter.Subscribe<App>((App)Application.Current, "tag", (variable) => {
                TagID = DadosSecao.UltimaTagLida;
                Console.WriteLine($"CadastroTagViewModel: {TagID}");
                OnPropertyChanged(nameof(TagID));
            });
        }

        public void OnLeaving()
        {
            MessagingCenter.Unsubscribe<App>((App)Application.Current, "tag");
        }

        private async void OnSalvarClickec(object obj)
        {
            TagTO tag = new TagTO();
            tag.TagID = TagID;
            tag.TagName = Nome;
            tag.IdEstabelecimento = DadosSecao.Estabelecimento.Id;
            var retorno = APIHelper.CadastrarAlterarTAG(tag);
            
            await App.Current.MainPage.DisplayAlert(retorno ?"Sucesso":"Falha", retorno ? "Sucesso no cadastro!" : "Falha no cadastro","Ok");

        }

    }
}
