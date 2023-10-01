using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GuideMeCadastroTags.Utils
{
    public class LoginResponseTO
    {
        public LoginResponseTO()
        {
            Token = string.Empty;
           
        }

        public string Token { get; set; }
      

    }
}
