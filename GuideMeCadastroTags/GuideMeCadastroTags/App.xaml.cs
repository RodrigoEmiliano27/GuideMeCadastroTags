using GuideMeCadastroTags.Bengala;
using GuideMeCadastroTags.DAO;
using GuideMeCadastroTags.Interfaces;
using GuideMeCadastroTags.Services;
using GuideMeCadastroTags.Views;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GuideMeCadastroTags
{
    public partial class App : Application
    {
        private bool varTeste = false;
        public PermissionStatus PermissaoBLE { get; set; } = PermissionStatus.Unknown;
        public PermissionStatus PermissaoBLEAndroid12 { get; set; } = PermissionStatus.Unknown;
        private IAndroidBluetoothService _bluetoothService;
        IDevice _device;
        private string _versaoDoAndroid;
        private ConcurrentBag<RequisicaoBase> FilaRequsicoesBengala = new ConcurrentBag<RequisicaoBase>();
        private bool _threadMensagensBengala = false;
        public EventHandler onAlgumaCoisa;


        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();

            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();

            _versaoDoAndroid = _bluetoothService.ObterVersaoDoAndroid();



        }


        protected override void OnStart()
        {
            base.OnStart();
            if (_device == null)
                VerificaCondicoesBluetooth();

            
        }

        protected override void OnSleep()
        {
            
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_device == null)
                VerificaCondicoesBluetooth();

        }
        private async Task<bool> ObterPermissaoBluetoothLEAndroid12Async()
        {
            PermissaoBLEAndroid12 = await _bluetoothService.ObtemPermissaoBluetoothLE();

            if (PermissaoBLEAndroid12 == PermissionStatus.Denied || PermissaoBLEAndroid12 == PermissionStatus.Disabled)
            {
                DependencyService.Get<IMessage>().ShortAlert("Não é possível usar o app sem o Bluetooth.");
                return false;
            }
            return true;
        }
        private async void ObterPermissaoLocalizacaoParaBluetoothLE()
        {
            PermissaoBLE = await _bluetoothService.ObtemPermissaoLocalizacao();

            // Talkback depois
            if (PermissaoBLE == PermissionStatus.Denied || PermissaoBLE == PermissionStatus.Disabled)
                DependencyService.Get<IMessage>().ShortAlert("Não é possível usar o app sem o Bluetooth.");

            else
            {
                bool oBluetoothTaAtivado = _bluetoothService.VerificaSeOBluetoothEstaAtivado();

                if (!oBluetoothTaAtivado)
                {
                    
                     bool decisao = await App.Current.MainPage.DisplayAlert("O Bluetooth do dispositivo está desativado", "Para o GuideMe funcionar, é necessário que o Bluetooth esteja ativado." +
                         "\nDeseja ativar o Bluetooth?", "Yes", "No");

                     if (decisao)
                         _bluetoothService.AbreTelaConfiguracoes();
                }

                else if (!string.IsNullOrEmpty(StorageDAO.NomeBengalaBluetooth) && !_threadMensagensBengala)
                    ConectarNaBengala();
                else
                {
                    ProcurarDispositivo();
                }

            }
        }
        private async void ProcurarDispositivo()
        {
            if (_bluetoothService != null)
            {
                DependencyService.Get<IMessage>().ShortAlert("Procurando dispositivo..");
                if (_bluetoothService is IAndroidBluetoothService)
                {
                    (_bluetoothService as IAndroidBluetoothService).OnBluetoothScanTerminado -= MainPage_OnBluetoothScanTerminado;
                    (_bluetoothService as IAndroidBluetoothService).OnBluetoothScanTerminado += MainPage_OnBluetoothScanTerminado;
                    _ = _bluetoothService.EscanearDispositivosAsync();
                    DependencyService.Get<IMessage>().ShortAlert("Procurando dispositivo..");
                }


            }

        }
        private async void MainPage_OnBluetoothScanTerminado()
        {
            List<IDevice> dispositivos = new List<IDevice>();
            if (_bluetoothService is IAndroidBluetoothService)
                dispositivos = new List<IDevice>((_bluetoothService as IAndroidBluetoothService)._dispositivosEscaneados);

            IDevice deviceMaiorRSSI = null;
            if (dispositivos != null && dispositivos.Count > 0)
            {
                (_bluetoothService as IAndroidBluetoothService).OnBluetoothScanTerminado -= MainPage_OnBluetoothScanTerminado;

                foreach (IDevice device in dispositivos)
                {
                    if (deviceMaiorRSSI == null || deviceMaiorRSSI.Rssi > device.Rssi)
                        deviceMaiorRSSI = device;

                }

                if (_device != null)
                    _device.Dispose();

                if (deviceMaiorRSSI != null)
                {
                    DependencyService.Get<IMessage>().ShortAlert($"Dispositivo encontrado! {deviceMaiorRSSI.Name}");
                    if (await StorageDAO.SalvaConfiguracoesNomeBengala(deviceMaiorRSSI.Name))
                        ConectarNaBengala();

                }
                else
                    DependencyService.Get<IMessage>().ShortAlert($"Nenhum dispositivo encontrado!");


            }
        }
        private void RequisitaLeiturasTags()
        {
            try
            {
                while (_threadMensagensBengala && _device != null)
                {
                    FilaRequsicoesBengala.Add(new RequisicaoBase() { Tipo = Enum.EnumTipoRequisicaoBengala.LeituraTag });
                    Thread.Sleep(350);
                }
            }
            catch (Exception err)
            {
                _ = Task.Factory.StartNew(_ => RequisitaLeiturasTags(), TaskCreationOptions.LongRunning);
            }
        }
        private async Task<FrameLeituraTag> LeituraTagsBengala(string instanteMillis)
        {
            FrameLeituraTag frame = null;
            try
            {
                byte[] dadoRFID = await _bluetoothService.LeDadosRFIDAsync(_device);

                if (dadoRFID != null)
                {
                    string leitura = "";
                    foreach (byte b in dadoRFID)
                        leitura += ((char)b).ToString();

                    string[] tokens = leitura.Split(' ');
                    leitura = "";
                    foreach (string s in tokens)
                    {
                        string aux = s;

                        if (aux.Length == 1)
                            aux = "0" + s;

                        leitura += aux + " ";
                    }
                    //leitura = ConvertHex(leitura);

                    //leitura = System.Text.Encoding.ASCII.GetString(dadoRFID);

                    leitura = leitura.ToUpper().Trim();
                    string[] tokensFinais = leitura.Split('-');
                    if (tokensFinais.Length == 2)
                    {

                        var frameLido = ParserAntena.ParseData(tokensFinais[0]);
                        if (frameLido != null)
                        {
                            if (frameLido.TipoFrame == TrataFrames.LeituraTag)
                            {
                                if (instanteMillis == null || instanteMillis.Trim() != tokensFinais[1].Trim())
                                {
                                    //TOdo IMPLEMENTAR TAMBÉM A VERIFICAÇÃO DE DATA E HORA PELO APP PRA EVITAR MULTIPLOS DISPAROS EM UM MESMO INSTANTE

                                    frame = (frameLido 
                                        as FrameLeituraTag);
                                    frame.IDMensagem = tokensFinais[1];

                                    if (!string.IsNullOrEmpty(frame.TagID))
                                    {
                                        Dispatcher.BeginInvokeOnMainThread(() => {
                                            DependencyService.Get<IMessage>().CustomAlert($"Tag lida: {frame.TagID} ", 800);
                                            DadosSecao.UltimaTagLida = frame.TagID;
                                            MessagingCenter.Send<App>(this, "tag");

                                        });
                                    }

                                   

                                   
                                }



                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {

            }
            return frame == null ? null : (FrameLeituraTag)frame.Clone();
        }
        private async void MensagensBengala()
        {
            FrameLeituraTag ultimoFrameLido = null;
            try
            {
                while (_threadMensagensBengala && _device != null)
                {
                    if (FilaRequsicoesBengala != null && FilaRequsicoesBengala.Count > 0)
                    {
                        RequisicaoBase requisicao = null;
                        if (FilaRequsicoesBengala.TryTake(out requisicao))
                        {
                            switch (requisicao.Tipo)
                            {
                                case Enum.EnumTipoRequisicaoBengala.LeituraTag:
                                    FrameLeituraTag frameAux = await LeituraTagsBengala(ultimoFrameLido == null ? null : ultimoFrameLido.IDMensagem);
                                    if (frameAux != null)
                                        ultimoFrameLido = frameAux;
                                    break;
                                case Enum.EnumTipoRequisicaoBengala.AcionarMotor:
                                    if (requisicao is RequisicaoMotor)
                                    {
                                        if (!await VibrarMotor(2))
                                            DependencyService.Get<IMessage>().ShortAlert("Comando erro");
                                    }

                                    break;
                            }
                        }


                    }
                    Thread.Sleep(350);
                }
            }
            catch (Exception err)
            {
                _ = Task.Factory.StartNew(_ => MensagensBengala(), TaskCreationOptions.LongRunning);
            }
        }
        private async Task<bool> VibrarMotor(int qtVibracao)
        {
            return await _bluetoothService.AcionarVibracaoBengala(_device, qtVibracao);
        }
        private void InicializaControleBengala()
        {
            _ = Task.Factory.StartNew(_ => MensagensBengala(), TaskCreationOptions.LongRunning);
            _ = Task.Factory.StartNew(_ => RequisitaLeiturasTags(), TaskCreationOptions.LongRunning);


        }

        private async void ConectarNaBengala()
        {
            try
            {

                _device = await /*Task.Run(*/_bluetoothService.EscanearDispositivosEConectarAoESP32Async(StorageDAO.NomeBengalaBluetooth);/*)*/
                if (_device != null)
                {
                    _threadMensagensBengala = true;
                    bool apagouMsg = await _bluetoothService.ApagaUltimaTagLida(_device);
                    await _bluetoothService.AcionarVibracaoBengala(_device, 2);
                    InicializaControleBengala();

                }

            }

            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exceção", "Deu a exceção do Java Security pedindo o bluetooth connection.", "Ok");
            }
        }
        private async void VerificaCondicoesBluetooth()
        {
            try
            {
                //DependencyService.Get<IMessage>().ShortAlert("Só um teste");
                if (_bluetoothService.BluetoothLEEhSuportado())
                {
                    if (Convert.ToInt32(_versaoDoAndroid) >= 12)
                    {
                        bool permissaoBLEAndroid12Concedida = await ObterPermissaoBluetoothLEAndroid12Async();

                        if (!permissaoBLEAndroid12Concedida)
                            return;
                    }

                    ObterPermissaoLocalizacaoParaBluetoothLE();
                }

                else
                {
                    DependencyService.Get<IMessage>().ShortAlert("O seu dispositivo não possui suporte ao BluetoothLE.");
                    return;
                }
            }
            catch (Plugin.BLE.Abstractions.Exceptions.DeviceDiscoverException ex)
            {
                // TALKBACK
                DependencyService.Get<IMessage>().ShortAlert("Ocorreu um erro ao escanear dispositivos Bluetooth.");
                return;
            }
            catch (Plugin.BLE.Abstractions.Exceptions.DeviceConnectionException ex)
            {
                // TALKBACK
                DependencyService.Get<IMessage>().ShortAlert("Ocorreu uma falha para conectar ao ESP32 por Bluetooth.");
                return;
            }
            catch (Plugin.BLE.Abstractions.Exceptions.CharacteristicReadException ex)
            {
                // TALKBACK
                DependencyService.Get<IMessage>().ShortAlert("Não foi possível ler dados retornados pelo ESP32.");
                return;
            }
            catch (Exception ex)
            {
                // TALKBACK
                _bluetoothService.ReiniciarOAppAposFalha();
                return;
            }
        }
    }
}
