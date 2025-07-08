using Khronus.Framework.Core.Util;
using Khronus.Framework.DataAccess.ORM;
using Khronus.Framework.Integration;
using Khronus.Framework.Integration.Moskit;
using Khronus.Framework.Integration.Moskit.Entities;
using Khronus.Framework.Integration.Sga;
using Khronus.Framework.Integration.Sga.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Sync.Custom
{
    public class Worker : WorkerBase
    {
        private void SyncUsuario()
        {
            Object ret = null;
           
            LogApp($"SyncUsuario iniciado...");

            try
            {
                MoskitClient moskitClient = new MoskitClient();
                moskitClient.Token = Parameters["MoskitApiKey"];

                UserService userService = new UserService(moskitClient);

                foreach (ValueObject valueObject in userService.GetList())
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    User user = (User)valueObject;

                    string usuarioId = StringUtilities.ToString(user.Id);
                    string usuario = user.Name;
                    string email = user.Username;
                    bool active = user.Active;

                    Team team = user.Team;

                    TeamService teamService = new TeamService(moskitClient);
                    user.Team = (Team)teamService.Get(user.Team.Id);

                    UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_FILIAL_COD={Store} AND SYSAQ_NUMERO={usuarioId}");

                    bool found = usuarioBO != null;

                    if (!found)
                    {
                        usuarioBO = new UsuarioBO();

                        usuarioBO.SYSAQ_FILIAL_COD = Store;
                        usuarioBO.SYSAQ_CODIGO = GetNumerator(Store, "sysaq");
                        usuarioBO.SYSAQ_NUMERO = usuarioId;
                        usuarioBO.SYSAQ_PARTICIPA_ROLETA = false;
                    }

                    //usuarioBO.SYSAQ_CODIGO = GetNumerator(Store, "sysaq");
                    usuarioBO.SYSAQ_USUARIO = usuario;
                    usuarioBO.SYSAQ_EQUIPE_ID = team.Id;
                    usuarioBO.SYSAQ_EQUIPE_NAME = user.Team.Name;
                    usuarioBO.SYSAQ_EMAIL = email;
                    usuarioBO.SYSAQ_INATIVO = !active;

                    if (!found)
                    {
                        ret = BusinessObjectManager.Insert(usuarioBO);
                    }
                    else
                    {
                        ret = BusinessObjectManager.Update(usuarioBO);
                    }

                    if (ret != null)
                    {
                        break;
                    }

                    Thread.Sleep(1000);
                }

            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp($"SyncUsuario finalizado...");
        }

        private void SyncCriarErpSgrSgaLeadsGanhos()
        {
            object ret = null;            

            LogApp($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {
                IEnumerable<EventoBO> eventoBOList = BusinessObjectManager.GetListByFilter<EventoBO>($"USUAN_FILIAL={Store} AND USUAN_TIPO=0 AND USUAN_STATUS='00'");

                foreach (EventoBO eventoBO in eventoBOList)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    JObject evento = JObject.Parse(StringUtilities.ToString(eventoBO.USUAN_EVENTO));

                    string statusDeal = StringUtilities.ToString(evento["data"]["status"]);
                    long dealId = NumberUtilities.parseInt(evento["data"]["id"]);

                    string status = "10";
                    string motivo = "Processado com sucesso";
                    string eventoRetorno = null;

                    if (statusDeal.Equals("won"))
                    {
                        long contatoId = NumberUtilities.parseLong(evento["data"]["contacts"][0]["id"]);

                        MoskitClient moskitClient = new MoskitClient();
                        //moskitClient.Token = moskitApiKey;
                        moskitClient.Token = Parameters["MoskitApiKey"];

                        ContactService contactService = new ContactService(moskitClient);

                        Contact contact = (Contact)contactService.Get(contatoId);

                        List<Object> entityCustomFieldsContact = new List<Object>(contact.EntityCustomFields);

                        string cpfCnpj = "";

                        //var customFieldsContact = contact.EntityCustomFields;

                        foreach (JObject customField in entityCustomFieldsContact)
                        {
                            if (customField["id"].ToString().Equals("CF_0WGqoGSKC9zK2qnP"))
                            {
                                cpfCnpj = StringUtilities.ToString(customField["textValue"]);

                                break;
                            }
                        }

                        string nome = contact.Name;
                        string sexo = "M";
                        string rg = "400531732"; //String
                        string dataNascimento = "17/11/1987";
                        string telefone = contact.Phones == null ? "" : contact.Phones[0].Number;
                        string celular = contact.Phones.Length > 0 ? "" : contact.Phones[1].Number;
                        string email = contact.Emails == null ? "" : contact.Emails[0].Address;
                        string logradouro = "Teste 222";
                        string numero = "122"; //String
                        string complemento = "teste";
                        string bairro = "teste";
                        string cidade = "Sorocoba";
                        string estado = "SP";
                        string cep = "18.071-095";

                        //JToken customFieldValue = entityCustomFields.SelectToken("$.[?(@.id == 'CF_0WGqoGSKC9zK2qnP')]", true);

                        //string nomeLoja = StringUtilities.ToString(row["line_items"][0]["meta_data"].SelectToken("[?(@.key == '_vendor_id')]")["value"]);

                        //
                        //Campo customizado Lead
                        //
                        List<Object> entityCustomFields = new List<Object>(evento["data"]["customFieldValues"]);

                        string tipoAssociado = "";

                        //var customFieldsContact = contact.EntityCustomFields;

                        foreach (JObject customField in entityCustomFields)
                        {
                            string idCustom = StringUtilities.ToString(customField["customField"]["id_v2"]);

                            if (idCustom.Equals("CF_y5lm56iyiY4L8DwW"))
                            {
                                tipoAssociado = StringUtilities.ToString(customField["label"]);

                                break;
                            }
                        }

                        //
                        // Buscar campo customizado ASSOCIADO ou CLIENTE/RASTREADOR
                        //
                        ServiceReturn serviceReturn = new ServiceReturn();

                        if (tipoAssociado.Equals("ASSOCIADO"))
                        {
                            //Caso o lead seja tratado no moskit como ASSOCIADO deve-se criar o cliente no SGA(Sistema de Gerenciamento de Associado)
                            ///https://api.hinova.com.br/api/sga/v2/doc/#api-Associado-Cadastrar
                            SgaClient sgaClient = new SgaClient();
                            sgaClient.Token = Parameters["SgaToken"];

                            AssociadoService associadoService = new AssociadoService(sgaClient);

                            serviceReturn = associadoService.GetByCpf(cpfCnpj);

                            if (serviceReturn.Status == 500 && serviceReturn.Message.Contains("Associado n\\u00e3o encontrado"))
                            {
                                Associado associado = new Associado();

                                associado.Nome = "TESTE VSI SOLUCOES 500";
                                associado.Sexo = "M";
                                associado.Cpf = cpfCnpj;
                                associado.Rg = "400531732"; //String
                                associado.DataNascimento = "17/11/1987";
                                associado.Telefone = "(15) 3313-2514";
                                associado.Celular = "(15) 998100-5172";
                                associado.Email = "teste@vsisolucoes.com.br";
                                associado.Logradouro = "Teste 222";
                                associado.Numero = "122"; //String
                                associado.Complemento = "teste";
                                associado.Bairro = "teste";
                                associado.Cidade = "Sorocoba";
                                associado.Estado = "SP";
                                associado.Cep = "18.071-095";
                                //associado.CodigoEstadoCivil = 6; //Não informado
                                //associado.DescricaoSituacao = "PENDENTE";


                                //associado.CodigoProfissao = 9;
                                associado.CodigoRegional = 1;
                                //associado.CodigoSituacao = 1;

                                associado.CodigoCooperativa = 1;
                                //associado.CodigoBeneficiario = 1;
                                //associado.CodigoVoluntario = 1;
                                //associado.CodigoComoConheceu = "1"; 
                                //associado.CodigoConta = 2;

                                ////
                                ////Cobrança
                                ////
                                associado.DiaVencimento = 10;
                                //associado.CodigoTipoCobrancaRecorrente = 1;
                                //associado.DescricaoTipoCobrancaRecorrente = "BOLETO / CARNÊ";

                                //associado.SpcSerasa = "NÃO";

                                ////
                                ////Veiculos
                                ////
                                //Veiculo veiculo = new Veiculo();

                                //[veiculos] => Array
                                //(
                                //[0] => Array
                                //(
                                //[codigo_veiculo] => 2053
                                //[placa] => FBP7570
                                //[chassi] => 9BD196271D2041691
                                //[valor_fixo] => 0
                                //[codigo_situacao] => 3
                                //[valor_fipe] => 32297
                                //[situacao] => PENDENTE
                                //[descricao_modelo] => PALIO ATTRACTIVE 1.0 EVO FIRE FLEX 8V 5P
                                //[codigo_modelo] => 8076
                                //[codigo_veiculo_indicador] =>
                                //[placa_veiculo_indicador] =>
                                //[codigo_associado_indicador] =>
                                //[cpf_associado_indicador] =>
                                //[nome_associado_indicador] =>
                                //)
                                //)

                                serviceReturn = associadoService.Post(associado);

                            }
                        }
                        else if (tipoAssociado.Equals("CLIENTE/RASTREADOR"))
                        {
                            try
                            {
                                string codSituacaoCliente = "1";
                                string codMatrizFilialCliente = "4";
                                string nomeCliente = "valter batista";
                                string cpfCliente = "63847700081";//"63847700081"; //17734222862
                                string formatoEnvioTitulo = "198";
                                string formatoBoletoCliente = "U";
                                string enderecoClienteCep = "18078722";
                                string enderecoClienteNumero = "22";
                                string enderecoClienteLogradouro = "teste";
                                string enderecoClienteBairro = "teste";
                                string enderecoClienteCidade = "Sorocaba";
                                string enderecoClienteEstado = "SP";

                                //Caso o lead seja tratado no moskit como CLIENTE/RASTREADOR deve-se criar o cliente no SGR(Sistema Gerenciamento de Rastreador)
                                //https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/inserir_cliente

                                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                                try
                                {
                                    //CookieContainer cookieContainer = new CookieContainer();
                                    //System.Net.Cookie cookie1 = new System.Net.Cookie("PHPSESSID", "6vkqushiqds6nvd6mtp0bp2ab5");
                                    //System.Net.Cookie cookie2 = new System.Net.Cookie("laravel_session", "eyJpdiI6IllFSmVTa0d6R09TWUJUUHRoSEt5dkE9PSIsInZhbHVlIjoiVlBDNlRoaW1KeGYweUJRU1VnQnpnRVpvZ3laeFNFdXpJTkptcVRxNERNMEczYWVqVWRUUnNJcnVmTjZrYkFYVjFDbGY5VWQ2bFQyeFwveHV5YjlTbFwvUT09IiwibWFjIjoiMTc3MTk5MGQ0MDVhMTM5MTY0M2FjMjg3NGUwMWM4NmNjMGNmYWE4ZDM0N2Q1OWY4NTExZWU4YzQwMTU1ZGM1MyJ9");
                                    //cookie1.Domain = "sgr.hinova.com.br";
                                    //cookie2.Domain = "sgr.hinova.com.br";
                                    //cookieContainer.Add(cookie1);
                                    //cookieContainer.Add(cookie2);

                                    CookieContainer cookieContainer = new CookieContainer();
                                    System.Net.Cookie cookie = new System.Net.Cookie("PHPSESSID", "6vkqushiqds6nvd6mtp0bp2ab5", "", "sgr.hinova.com.br");
                                    cookieContainer.Add(cookie);

                                    System.Net.Cookie cookie2 = new System.Net.Cookie("laravel_session", "eyJpdiI6IllFSmVTa0d6R09TWUJUUHRoSEt5dkE9PSIsInZhbHVlIjoiVlBDNlRoaW1KeGYweUJRU1VnQnpnRVpvZ3laeFNFdXpJTkptcVRxNERNMEczYWVqVWRUUnNJcnVmTjZrYkFYVjFDbGY5VWQ2bFQyeFwveHV5YjlTbFwvUT09IiwibWFjIjoiMTc3MTk5MGQ0MDVhMTM5MTY0M2FjMjg3NGUwMWM4NmNjMGNmYWE4ZDM0N2Q1OWY4NTExZWU4YzQwMTU1ZGM1MyJ9", "", "sgr.hinova.com.br");
                                    cookie2.Expired = true;
                                    cookie2.Expires = DateTime.Now.AddDays(1);
                                    cookie2.HttpOnly = false;

                                    cookieContainer.Add(cookie2);

                                    HttpWebRequest request = null;
                                    HttpWebResponse response = null;

                                    request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/inserir_cliente/688472668f481b3efbddb0bfbff99cf6");
                                    //request.Headers.Add("Accept", "application/json");                            
                                    request.Headers.Add("X-Auth-Token", "$2y$11$N.zj6OpWG5Iqw8J7dt73TOTYrw..zTffmmSPyH6D8nNAIgCjoVZ3y");
                                    request.Headers.Add("Authorization", "708183befb7e49cad4f8f025933f33897d52cc23");
                                    request.Method = "POST";
                                    //request.ContentType = "application/x-www-form-urlencoded";
                                    request.ContentType = "multipart/form-data";
                                    request.Accept = "application/json";
                                    //request.KeepAlive = false;
                                    //request.ServicePoint.ConnectionLimit = 10000;

                                    //request.CookieContainer = new CookieContainer();
                                    //request.CookieContainer.Add(new Cookie("ConstoCookie", "Chocolate Flavour"));

                                    request.CookieContainer = cookieContainer;

                                    //String json =
                                    //    $"enrollment[registration_id]={registrationId}&" +
                                    //    $"enrollment[school_product_id]={schoolProductId}&" +
                                    //    $"enrollment[max_attendance_type]={maxAttendanceType}&" +
                                    //    $"enrollment[status]={status}";

                                    string postData =
                                        $"cliente={3569}&" +
                                        $"cod_situacao_cliente={codSituacaoCliente}&" +
                                        $"cod_matriz_filial_cliente={codMatrizFilialCliente}&" +
                                        $"nome_cliente={nomeCliente}&"
                                        +
                                        $"cpf_cliente={cpfCliente}&" +
                                        $"formato_envio_titulo_cliente[0]=" + "{\"" + formatoEnvioTitulo + "\"}"
                                        //+
                                        //$"formato_boleto_cliente={formatoBoletoCliente}&" +
                                        //$"endereco_cliente[cep]={enderecoClienteCep}&" +
                                        //$"endereco_cliente[numero]={enderecoClienteNumero}&" +
                                        //$"endereco_cliente[logradouro]={enderecoClienteLogradouro}&" +
                                        //$"endereco_cliente[bairro]={enderecoClienteBairro}&" +
                                        //$"endereco_cliente[cidade]={enderecoClienteCidade}&" +
                                        //$"endereco_cliente[estado]={enderecoClienteEstado}"
                                        ;

                                    var data = Encoding.ASCII.GetBytes(postData);

                                    request.ContentLength = data.Length;

                                    using (var stream = request.GetRequestStream())
                                    {
                                        stream.Write(data, 0, data.Length);
                                    }

                                    //request.ContentLength = postData.Length;

                                    //using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                                    //{
                                    //    streamWriter.Write(postData);
                                    //}

                                    response = (HttpWebResponse)request.GetResponse();

                                    var result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                                }
                                catch (Exception ex)
                                {

                                    
                                }

                                string aqui = "";
                                

                                //tipo
                            }
                            catch (WebException e)
                            {
                                //serviceReturn.Status = 500;
                                //serviceReturn.Message = e.Message;

                                if (e.Response != null)
                                {
                                    using (var errorResponse = (HttpWebResponse)e.Response)
                                    {
                                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                                        {
                                            String resp = reader.ReadToEnd();
                                            //serviceReturn.Message = resp;
                                            status = "10";
                                            motivo = "Falha ao processar";
                                            eventoRetorno = resp;
                                        }
                                    }
                                }
                            }

                            //Gerar pedido no SGR
                            //https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/inserir_cliente/inserir_venda

                            //cod_combustivel_veiculo_venda = 209;
                            //cod_forma_pagamento_venda = 201;
                            //cod_forma_pagamento_adesao_venda = 201;
                            //cod_grupo_venda = 1;
                            //cod_grupo_adesao_venda = 1002;
                            //cod_periodo_venda = 336;
                            //cod_vencimento_venda = 281;
                            //cod_cliente_venda = 1;
                            //cpf_cliente_venda = "17734222862";
                            //rg_cliente_venda = "123";
                            //placa_veiculo_venda = "fdd-4444";
                            //anofab_veiculo_venda = "2020";
                            //anomod_veiculo_venda = "2021";
                            //chassi_veiculo_venda = 123;
                            //fipe_valor_veiculo_venda = 999.99;
                            //renavam_veiculo_vend = 123;
                            //data_nascimento_cliente_venda = "2020-12-31"; //date
                            //contato_cliente_venda = "Teste";
                            //profissao_cliente_venda = "Analista";
                            //interveniente_venda = "";
                            //quantidade_parcela_venda = 1;
                            //quantidade_parcela_adesao_venda = 1;
                            //valor_parcela_venda = 999.99;
                            //valor_parcela_adesao_venda = 999.99;
                            //entrada_venda = 1;
                            //observacao_venda = "obs";
                            //sexo_cliente_venda = "M";
                            //estado_civil_cliente_venda = "SO";                            
                            //nome_cliente_venda = "Teste";
                            //produto_venda[0][cod_produto] = 123;
                            //cod_ponto_venda_venda = 1;
                            //cod_consultor_venda = 1;
                            //cod_departamento = 242;

                            //fipe_codigo_veiculo_venda = "";

                            //endereco_venda[0]
                            //telefone_venda[0]
                            //email_venda[0]
                            //contato_venda[0]

                        }
                        //else
                        //{
                        //    serviceReturn.Status = 500;
                        //}

                        //status = "10";
                        //motivo = "Falha ao processar";
                        //eventoRetorno = serviceReturn.Message;

                        //if (serviceReturn.Status == 200)
                        //{
                        //    status = "20";
                        //    motivo = "Processado com sucesso";
                        //    eventoRetorno = "";
                        //}

                    }

                    //Atualiza o evento                    
                    ret = BusinessObjectManager.UpdateFields(eventoBO,
                        new string[] { "USUAN_STATUS", "USUAN_STATUS_MOTIVO", "USUAN_EVENTO_RETORNO" },
                        new object[] { status, motivo, eventoRetorno }
                    );

                    if (ret != null)
                    {
                        throw new Exception(ret.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp($"SyncCriarErpSgrSgaLeadsGanhos finalizado...");
        }

        public void SyncMoskitWhatapp()
        {
            object ret = null;

            LogApp($"SyncMoskitWhatapp iniciado...");

            try
            {
                IEnumerable<EventoBO> eventoBOList = BusinessObjectManager.GetListByFilter<EventoBO>("USUAN_FILIAL=0 AND USUAN_TIPO=1 AND USUAN_STATUS='00'");

                foreach (EventoBO eventoBO in eventoBOList)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    string status = "00";
                    string motivo = "Processo não executado.";
                    string eventoRetorno = null;

                    long dealId = 0;

                    try
                    {
                        JObject evento = JObject.Parse(StringUtilities.ToString(eventoBO.USUAN_EVENTO));

                        string idAtendimento = StringUtilities.ToString(evento["att"]["id"]);

                        string idChat = StringUtilities.ToString(evento["msg"]["chat_id"]);

                        string numeroMensagem = StringUtilities.ToString(evento["msg"]["message_number"]);

                        string nome = StringUtilities.ToString(evento["msg"]["sender_name"]);
                        string telefone = StringUtilities.ToString(evento["att"]["number"]);
                        long instanceid = NumberUtilities.parseLong(evento["channel"]["instanceid"]);

                        //string email = "testesPONTOTRACK@gmail.com";
                        string mensagem = "Teste";

                        string plataforma = StringUtilities.ToString(evento["att"]["platform"]);
                        string protocolo = StringUtilities.ToString(evento["att"]["protocolo"]);

                        DateTime dateInicio = (DateTime)evento["att"]["date_start"];

                        string chMeta = StringUtilities.ToString(evento["channel"]["ch_meta"]);

                        //
                        //Buscar responsável
                        //

                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        HttpWebRequest request = null;
                        HttpWebResponse response = null;

                        request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/external/getAtendimentos");
                        request.Headers.Add("Authorization", "Bearer " + Parameters["WHATSAPP_TOKEN"]);
                        request.Method = "POST";
                        request.Accept = "application/json";
                        request.ContentType = "application/json";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            string json = "{" +  $"\"number\":\"{telefone}\",\"instanceid\":\"{instanceid}\"" + "}";

                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }

                        response = (HttpWebResponse)request.GetResponse();

                        string agenteId = "";
                        string agenteNome = "";
                        JArray atendimento = new JArray();

                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();


                            //string r = StringUtilities.ToString(result);

                            //JObject evento2 = JObject.Parse(StringUtilities.ToString(result));

                            atendimento = (JArray)JObject.Parse(StringUtilities.ToString(result))["atendimentos"];

                            foreach (var att in atendimento)
                            {
                                //string agente = StringUtilities.ToString(atendimento.SelectToken("$.[?(@.id == \"21810\")]")["id_agente"]);

                                //JToken customFieldValue = atendimento.SelectToken("$.[?(@.id == '21999')]", true);
                                string getAtendimentoId = StringUtilities.ToString(att["id"]);

                                if (getAtendimentoId.Equals(idAtendimento))
                                {
                                    agenteId = StringUtilities.ToString(att["id_agente"]);

                                    //JObject tags = (JObject)att["tags"];

                                    JObject tags = JObject.Parse(StringUtilities.ToString(att["tags"]));

                                    agenteNome = StringUtilities.ToString(((Newtonsoft.Json.Linq.JProperty)tags.First).Value);


                                    //string agente2 = StringUtilities.ToString(tags.SelectToken("$.[?(@.AGENTE == \"21810\")]")["id_agente"]);
                                    //JObject.Parse(StringUtilities.ToString(eventoBO.USUAN_EVENTO));

                                    break;

                                }
                            }

                            eventoRetorno = "{" + $"\"Id atendimento\": \"{idAtendimento}\", \"Id Agente\":\"{agenteId}\", \"Nome agente\": \"{agenteNome}\" " + "}";

                        }

                        UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_AGENTE_ID = '{agenteId}' OR SYSAQ_USUARIO like '%{agenteNome}%'");



                        bool found = usuarioBO != null;

                        if (found)
                        {
                            status = "20";
                            motivo = "Processo não executado.";

                            if (usuarioBO.SYSAQ_AGENTE_ID == null)
                            {
                                usuarioBO.SYSAQ_AGENTE_ID = agenteId;

                                ret = BusinessObjectManager.Update(usuarioBO);

                                if (ret != null)
                                {
                                    break;
                                }
                            }

                            long usuarioId = NumberUtilities.parseLong(usuarioBO.SYSAQ_NUMERO);

                            //if (usuarioId != 0)
                            //{
                            MoskitClient moskitClient = new MoskitClient();
                            moskitClient.Token = Parameters["MoskitApiKey"];

                            DealService dealService = new DealService(moskitClient);

                            ServiceReturn serviceReturn = new ServiceReturn();

                            //
                            // Criar contato
                            //
                            ContactService contactService = new ContactService(moskitClient);

                            Contact contact = (Contact)contactService.GetByFone(telefone);                            

                            long contactId = 0;
                            
                            if (contact == null)
                            {
                                //RoletaUsuario("TESTE");

                                contact = new Contact();
                                contact.DateCreated = DateTime.Now;
                                contact.Name = nome;

                                //if (!String.IsNullOrEmpty(email))
                                //{
                                //    contact.Emails = new Email[] { new Email() { Address = email } };
                                //}

                                if (!String.IsNullOrEmpty(telefone))
                                {
                                    contact.Phones = new Phone[] { new Phone() { Number = telefone } };
                                }

                                contact.CreatedBy = new Identity() { Id = usuarioId };
                                contact.Responsible = new Identity() { Id = usuarioId };


                                serviceReturn = contactService.Post(contact);

                                if (serviceReturn.Status == 200)
                                {
                                    contactId = NumberUtilities.parseLong(JObject.Parse(serviceReturn.Message.ToString())["id"]);
                                }
                            }
                            else
                            {
                                contactId = NumberUtilities.parseLong(contact.Id);
                            }

                            ////
                            ////Criar Negócio
                            ////
                            //JArray filter = JArray.Parse("[{\"field\": \"CF_lXODObiYCeXZGmaN\",\"expression\": \"match\",\"values\": [\"" + idChat + "\" ]}]");

                            ////JArray deals = (JArray)SendCommand($"https://api.moskitcrm.com/v2/deals/search", "POST", param, Parameters["MoskitApiKey"]);
                            //serviceReturn = dealService.GetByFilter(filter);

                            //if (serviceReturn.Status == 200)
                            //{
                            //    List<Deal> deals = (List<Deal>)serviceReturn.Detail;

                            //    Deal deal = deals.Count == 0 ? new Deal() : deals[0];

                            //    //Deal deal = new Deal();
                            //    deal.Name = nome + " " + telefone;
                            //    deal.Status = "OPEN";
                            //    deal.DateCreated = DateTime.Now;
                            //    deal.PrevisionCloseDate = DateTime.Now;
                            //    deal.Stage = new Identity() { Id = NumberUtilities.parseLong(Parameters["EtapaLead"]) }; //180220 - NOVO
                            //    deal.Contacts = contactId != 0 ? new Identity[] { new Identity() { Id = contactId } } : null;
                            //    deal.CreatedBy = new Identity() { Id = usuarioId };
                            //    deal.Responsible = new Identity() { Id = usuarioId };

                            //    List<CustomField> customFieldList = new List<CustomField>();

                            //    if (!String.IsNullOrEmpty(idAtendimento))
                            //    {
                            //        CustomField customField1 = new CustomField();
                            //        customField1.Id = "CF_YXoDkki3CVLGRDGE";
                            //        customField1.TextValue = idAtendimento;

                            //        customFieldList.Add(customField1);
                            //    }

                            //    if (!String.IsNullOrEmpty(idChat))
                            //    {
                            //        CustomField customField2 = new CustomField();
                            //        customField2.Id = "CF_lXODObiYCeXZGmaN";
                            //        customField2.TextValue = idChat;

                            //        customFieldList.Add(customField2);
                            //    }

                            //    long[] origem = new long[]{ 221451 };

                            //    //if (!String.IsNullOrEmpty(idChat))
                            //    //{
                            //    CustomField customField3 = new CustomField();
                            //    customField3.Id = "CF_49RM16ixiB7nbmBW";
                            //    customField3.Options = origem;

                            //    customFieldList.Add(customField3);
                            //    //}

                            //    deal.CustomFields = customFieldList.ToArray();

                            //    //serviceReturn = dealService.Post(deal);

                            //    if (deals.Count == 0)
                            //    {
                            //        serviceReturn = dealService.Post(deal);
                            //    }
                            //    else
                            //    {
                            //        serviceReturn = dealService.Put(deal);
                            //    }

                            //    if (serviceReturn.Status == 200)
                            //    {
                            //        JObject dealReturn = JObject.Parse(StringUtilities.ToString(serviceReturn.Message));
                            //        dealId = NumberUtilities.parseLong(((Newtonsoft.Json.Linq.JContainer)dealReturn.First).First);

                            //        //
                            //        //Notas
                            //        //
                            //        if (mensagem != null)
                            //        {
                            //            // Buscar conversa
                            //            //https://documenter.getpostman.com/view/7437209/TVewakBz#9c94c58e-7b5f-4255-b2c3-b5addeef0baa


                            //            //Note note = new Note();
                            //            //note.Description = "";
                            //            //note.User = new Identity() { Id = usuarioId };

                            //            //
                            //            //Criar nota com a conversa ";
                            //            //
                            //            string descricaoNota = "Número da mensagem: " + numeroMensagem
                            //                                    + "\nAtendimento: " + idAtendimento
                            //                                    + "\nData de inicio: " + DateUtilities.parse(dateInicio, "dd/MM/yyyy HH:mm:ss")
                            //                                    + "\nProtocolo: " + protocolo
                            //                                    + "\nPlataforma: " + plataforma
                            //                                    + "\nId Agente: " + agenteId
                            //                                    + "\nAgente: " + agenteNome
                            //                                    + "\n Identificador: " + "PONTO TRACK LONDRINA " + "(43) 3017-0227"
                            //                                    ;

                            //            JObject notes = new JObject(
                            //                   new JProperty("description", descricaoNota),
                            //                   new JProperty("user", new JObject(
                            //                       new JProperty("id", usuarioId)
                            //                   ))
                            //               );

                            //            JObject nota = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{dealId}/notes", "POST", notes, Parameters["MoskitApiKey"]);
                            //        }

                            //        //}
                            //    }

                            //    eventoRetorno = serviceReturn.Message.ToString();
                            //    status = "10";
                            //    motivo = "Processo executado com sucesso";
                            //}

                            //}

                        }
                        //}

                        //
                        //Distribuir usuário
                        //
                        //int usuarioId = RoletaUsuario(null);
                        //}
                    }
                    catch (Exception ex)
                    {
                        LogApp(ex);

                        eventoRetorno = ex.ToString();
                    }

                    //Atualiza o evento
                    ret = BusinessObjectManager.UpdateFields(eventoBO, new string[] { "USUAN_STATUS", "USUAN_STATUS_MOTIVO", "USUAN_EVENTO_RETORNO" }, new object[] { status, motivo, eventoRetorno });

                    if (ret != null)
                    {
                        throw new Exception(ret.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp($"SyncMoskitWhatapp finalizado...");

        }

        private void SyncEnviarMensagem()
        {
            object ret = null;

            LogApp($"SyncAtualizaWhatapp iniciado...");

            try
            {
                MoskitClient moskitClient = new MoskitClient();

                moskitClient.Token = Parameters["PARAM_0001"];

                DealService dealService = new DealService(moskitClient);

                JArray filter = JArray.Parse("[{\"field\": \"status\",\"expression\": \"one_of\",\"values\": [\"OPEN\" ]}]");

                ServiceReturn serviceReturn = dealService.GetByFilter(filter);

                if (serviceReturn.Status == 200)
                {             
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/external/getAtendimentos");
                    request.Headers.Add("Authorization", "Bearer " + Parameters["WHATSAPP_TOKEN"]);
                    request.Method = "POST";
                    request.Accept = "application/json";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    var result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    

                    
                }

            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp($"SyncAtualizaWhatapp finalizado...");

        }

        private void SyncAtualizaWhatapp()
        {
            object ret = null;

            LogApp($"SyncAtualizaWhatapp iniciado...");

            try
            {
                IEnumerable<EventoBO> eventoBOList = BusinessObjectManager.GetListByFilter<EventoBO>("USUAN_FILIAL=0 AND USUAN_TIPO=1 AND USUAN_STATUS='10'");

                foreach (EventoBO eventoBO in eventoBOList)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    string status = "20";
                    string motivo = "Processo não executado.";
                    string eventoRetorno = "";

                    long dealId = 14121479;

                    JObject evento = JObject.Parse(StringUtilities.ToString(eventoBO.USUAN_EVENTO));

                    string idAtendimento = StringUtilities.ToString(evento["att"]["id"]);
                    string idChat = StringUtilities.ToString(evento["msg"]["chat_id"]);


                    //https://documenter.getpostman.com/view/7437209/TVewakBz#9c94c58e-7b5f-4255-b2c3-b5addeef0baa

                    //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    HttpWebRequest request = null;
                    HttpWebResponse response = null;

                    //request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/external/getMessages?atendimento_id=21725");
                    request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/session/getChat?chatId=5515997409919@c.us");
                    request.Headers.Add("Authorization", "Bearer " + Parameters["WHATSAPP_TOKEN"]);
                    request.Method = "POST";
                    request.Accept = "application/json";

                    response = (HttpWebResponse)request.GetResponse();

                    var result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    long usuarioId = NumberUtilities.parseLong(Parameters["USUARIO_MOSKIT"]);

                    //
                    //Criar nota com a conversa ";
                    //
                    string descricaoNota = "Teste";

                    JObject notes = new JObject(
                           new JProperty("description", descricaoNota),
                           new JProperty("user", new JObject(
                               new JProperty("id", usuarioId)
                           ))
                       );

                    JObject nota = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{dealId}/notes", "POST", notes, Parameters["MoskitApiKey"]);

                    //Atualiza o evento
                    ret = BusinessObjectManager.UpdateFields(eventoBO, new string[] { "USUAN_STATUS", "USUAN_STATUS_MOTIVO", "USUAN_EVENTO_RETORNO" }, new object[] { status, motivo, eventoRetorno });

                    if (ret != null)
                    {
                        throw new Exception(ret.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp($"SyncAtualizaWhatapp finalizado...");

        }
    }
}
