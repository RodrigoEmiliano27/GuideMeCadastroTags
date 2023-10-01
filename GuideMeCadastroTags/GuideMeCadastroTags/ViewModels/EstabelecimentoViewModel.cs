using GuideMeCadastroTags.DAO;
using GuideMeCadastroTags.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace GuideMeCadastroTags.ViewModels
{
    public class EstabelecimentoViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public string Nome_Estabelecimento { get; set; }

        public EstabelecimentoViewModel()
        {
            Nome_Estabelecimento = DadosSecao.Estabelecimento.Nome;
        }

       
    }
}
