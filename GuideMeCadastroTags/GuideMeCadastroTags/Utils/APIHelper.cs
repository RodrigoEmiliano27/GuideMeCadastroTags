using GuideMeCadastroTags.DAO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GuideMeCadastroTags.Utils
{

    static public class APIHelper
    {
        //private const string SiteURL = "https://guideme.azurewebsites.net";
        private const string SiteURL = "http://192.168.1.16:5254";
        private const string LoginApi = "/api/Login/v1/login";
        private const string GetEstabelecimentoInfoAPI = "/api/Estabelecimento/v1/EstabInfo";
        private const string SalvarTagInfoAPI = "/api/Tag/v1/SalvarTag";
        private static string tokenAPI = "";
        private static HttpClient client = null;
        private static HttpClientHandler httpClientHandler = null;
        private static LoginRequestTO _loginRequest;

        private static HttpClient VerificaHttpClient()
        {
            if (client != null)
                return client;

            if (Debugger.IsAttached)
            {
                httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => { return true; };
                client = new HttpClient(httpClientHandler);
            }
            else
            {
                client = new HttpClient();
                return client;
            }

            return client;
        }

        public static EstabelecimentoTO GetEstabelecimentoInfo(LoginRequestTO loginRequest)
        {
            EstabelecimentoTO retorno = null;

            try
            {
                _loginRequest = loginRequest;
                var retornoAPI = ExecGetAPI(MontaLinkAPI(GetEstabelecimentoInfoAPI), null, 3,anonymous:false);

                retorno = JsonConvert.DeserializeObject<EstabelecimentoTO>(retornoAPI.RetornoObj.ToString());

            }
            catch (Exception err)
            { 

            }

            return retorno;

        }

        public static bool CadastrarAlterarTAG(TagTO tagto)
        {
            bool retorno = false;


            try
            {
                var retornoAPI = ExecPostAPI(MontaLinkAPI(SalvarTagInfoAPI), tagto, 3, anonymous: false);
                retorno = retornoAPI.Sucesso;

            }
            catch (Exception err)
            {

            }


            return retorno;
        }

        private static string MontaLinkAPI(string api)
        {
            return SiteURL + api;
        }
        public static LoginResponseTO GetAuthenticationToken(LoginRequestTO loginRequest)
        {
            LoginResponseTO response = new LoginResponseTO();
            ResultCallApi resultado = new ResultCallApi();
            try
            {
                DadosSecao.LoginRequestTO = loginRequest; 
                resultado = ExecPostAPI(MontaLinkAPI(LoginApi), loginRequest, 3);

                if (resultado != null && resultado.Sucesso)
                {
                    var teste = JsonConvert.DeserializeObject<LoginResponseRoot>(resultado.Retorno);
                    tokenAPI = teste.loginResponse.Token;
                    response = teste.loginResponse;
                }
            }
            catch (Exception e)
            { 
            }
           
               
            return response;
        }
        public static ResultCallApi ExecPostAPI(string api, object objParaEnviar, int tentativas, int timeOutEmSegundos = 100,
            bool anonymous=true)
        {
            ResultCallApi retorno = new ResultCallApi();
            retorno.Retorno = string.Empty;

            try
            {
                LoginResponseTO token = new LoginResponseTO();
                Stopwatch sw = new Stopwatch();
                Random gerador = new Random();
                int handleTentativa = gerador.Next(1, 9999999);
                for (int n = 1; n <= tentativas; n++)
                {
                    sw.Restart();
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, api))
                    {
                        if (!anonymous)
                        {
                            if (string.IsNullOrEmpty(tokenAPI))
                            {
                                if (_loginRequest == null)
                                    return retorno;

                                var autenticacao = GetAuthenticationToken(_loginRequest);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;

                            }
                            else
                            {
                                VerificaHttpClient().DefaultRequestHeaders.Clear();
                                VerificaHttpClient().DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAPI);
                            }
                        }
                        if (!anonymous &&!string.IsNullOrEmpty(tokenAPI) || anonymous)
                        {
                            string dadoSerializado = JsonConvert.SerializeObject(objParaEnviar);

                            request.Content = new StringContent(dadoSerializado, Encoding.UTF8, "application/json");

                            var response = VerificaHttpClient().SendAsync(request).Result;


                            if (response.IsSuccessStatusCode)
                            {
                                retorno.Sucesso = true;
                                retorno.Retorno = response.Content.ReadAsStringAsync().Result;
                                //retorno.Retorno = TrataRespostaApi(retorno.Retorno);
                                break;
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !anonymous && !string.IsNullOrEmpty(tokenAPI))
                            {
                                var autenticacao = GetAuthenticationToken(DadosSecao.LoginRequestTO);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;
                            }

                        }

                       

                       
                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Erro = ex;
            }

            return retorno; ;
        }



        public static ResultCallApi ExecGetAPI(string api, object objParaEnviar, int tentativas, int timeOutEmSegundos = 100,
            bool anonymous = true)
        {
            ResultCallApi retorno = new ResultCallApi();
            retorno.Retorno = string.Empty;

            try
            {
                LoginResponseTO token = new LoginResponseTO();
                Stopwatch sw = new Stopwatch();
                Random gerador = new Random();
                int handleTentativa = gerador.Next(1, 9999999);
                for (int n = 1; n <= tentativas; n++)
                {
                    sw.Restart();
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, api))
                    {
                        if (!anonymous)
                        {
                            if (string.IsNullOrEmpty(tokenAPI))
                            {
                                if (_loginRequest == null)
                                    return retorno;

                                var autenticacao = GetAuthenticationToken(_loginRequest);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;

                            }
                            else
                            {
                                VerificaHttpClient().DefaultRequestHeaders.Clear();
                                VerificaHttpClient().DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAPI);
                            }
                        }

                        if (!anonymous && !string.IsNullOrEmpty(tokenAPI) || anonymous)
                        {

                            string dadoSerializado = JsonConvert.SerializeObject(objParaEnviar);

                            request.Content = new StringContent(dadoSerializado, Encoding.UTF8, "application/json");

                            var response = VerificaHttpClient().SendAsync(request).Result;

                            if (response.IsSuccessStatusCode)
                            {


                                ResultCallApi result = new ResultCallApi();
                                result.Sucesso = true;
                                result.RetornoObj = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                                retorno.Sucesso = true;
                                retorno.Retorno = result.Retorno;
                                retorno.RetornoObj = result.RetornoObj;
                                //retorno.Retorno = TrataRespostaApi(retorno.Retorno);
                                break;
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !anonymous && !string.IsNullOrEmpty(tokenAPI))
                            {
                                var autenticacao = GetAuthenticationToken(_loginRequest);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Erro = ex;
            }

            return retorno; ;
        }
    }
}
