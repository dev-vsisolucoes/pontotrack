using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using VSIntegra.Framework.v6011.Core;
using VSIntegra.Framework.v6011.Provider.Exceptions;
using VSIntegra.Framework.v6011.Model.Crm;
using VSIntegra.Framework.v6011.Model.System;
using VSIntegra.Framework.v6011.Provider;
using VSIntegra.Framework.v6011.Provider.D4Sign;
using VSIntegra.Framework.v6011.Provider.Moskit;
using VSIntegra.Framework.v6011.Provider.Moskit.Model;
using VSIntegra.Framework.v6011.Provider.Movidesk;
using VSIntegra.Framework.v6011.Provider.Movidesk.Model;
using VSIntegra.Framework.v6011.Provider.Sga;
using VSIntegra.Framework.v6011.Provider.Sgr;
using VSIntegra.Framework.v6011.Provider.Sgr.Model;

namespace Pontotrack.Tasks
{
    class Program : TaskBase
    {
        //
        // Para debug
        // 
        private static void Main()
        {
            //const string INTEGRATION_ID = "4e70ceb8-0a0a-45a5-b48e-af880108fab0";
            const string API_KEY = "6385f344dc11d";

            TaskBase task = new Program();
            //task.IntegrationId = INTEGRATION_ID;
            task.ApiKey = API_KEY;
            task.IsRunning = true;
            task.DebugMode = true;

            TaskService taskService = new TaskService(task);
            taskService.Execute();

            Console.WriteLine($"{GetCurrentAssemblyName()} inicializado");
            Console.WriteLine($"{GetCurrentAssemblyName()} pressione qualquer tecla para fechar");

            Console.ReadLine();
        }

        /// <summary>
        /// Monitora os usuarios do Moskit e atualiza o vsintegra 
        /// PUBLICADO
        /// </summary>
        private void TaskMonitoraUsuarioMoskit()
        {
            Log("TaskMonitoraUsuarioMoskit iniciado...");

            try
            {
                ProviderBase moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");

                IEnumerable<MoskitUser> users = moskitProvider.List<MoskitUser>();

                foreach (MoskitUser userMoskit in users)
                {
                    if (!IsRunning)
                    {
                        break;
                    }

                    User userVSIntegra = Find<User>(
                        FormatString("Number LIKE '%{0}%'", userMoskit.Id)
                    );

                    bool found = userVSIntegra != null;

                    if (!found)
                    {
                        userVSIntegra = new User();

                        userVSIntegra.Number = ToString(userMoskit.Id) + ";;";
                        userVSIntegra.RouletteJoin = true;
                        userVSIntegra.RouletteUsed = false;
                        userVSIntegra.Type = "US";
                        userVSIntegra.Password = "123";
                    }

                    userVSIntegra.Name = userMoskit.Name;
                    userVSIntegra.Email = userMoskit.Username;
                    userVSIntegra.Username = userMoskit.Username;
                    userVSIntegra.TeamId = ToString(userMoskit.Team.Id);
                    userVSIntegra.Inactive = !userMoskit.Active;

                    object ret = !found ? Add(userVSIntegra) : Update(userVSIntegra);

                    if (ret != null)
                    {
                        throw new Exception(ToString(ret));
                    }
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskMonitoraUsuarioMoskit finalizado");

            Delay(60);
        }

        /// <summary>
        /// Monitora os negocios do Moskit e cria o pedido no SGR 
        /// PUBLICADO
        /// </summary>
        public void TaskCriaPedidoErpSgrSgaParaLeadsGanhos()
        {
            Log("TaskCriaPedidoErpSgrSgaParaLeadsGanhos iniciado...");

            try
            {
                ProviderBase moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");
                ProviderBase sgaProvider = GetProvider<SgaProvider>("334998e8-7ca5-44db-8270-10d0c5c3f834");
                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");

                bool found = false;

                IEnumerable events = List<Event>(
                    $"Type=0 AND " +
                    $"Status='00'"
                    , "CreateDateTime ASC"
                );

                foreach (Event @event in events)
                {
                    //// Caso o servico de automação não esteja rodando
                    //if (!IsRunning)
                    //{
                    //    throw new ValidationException("Automação foi parada");
                    //}

                    object ret;

                    try
                    {
                        string eventText = ToString(@event.EventText);
                        JObject rawData = ToJObject(ToJObject(eventText)["raw"]);

                        JObject data = ToJObject(rawData["data"]);

                        if (data == null)
                        {
                            data = ToJObject(rawData["after"]);
                        }

                        string status = ToLower(ToString(data["status"]));
                        long dealId = ToInt(data["id"]); //1496620 160
                        string responsibleId = ToString(data["responsible"]["id"]);

                        string nomeNegocio = ToString(data["name"]);

                        if (!status.Equals("won"))
                        {
                            throw new DiscardedException(FormatString("Negocio: {0} {1} com status: {2}", dealId, nomeNegocio, status));
                        }

                        // buscar o negocio
                        ProviderResult providerResult = moskitProvider.Get<MoskitDeal>(dealId);

                        if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                        {
                            throw new Exception(providerResult.Message);
                        }

                        MoskitDeal deal = (MoskitDeal)providerResult.Detail;

                        if (deal == null)
                        {
                            throw new DiscardedException($"Negocio não encontrado para o Id {dealId}");
                        }

                        decimal price = deal.Price;

                        if (price > 0)
                        {
                            price = ToDecimal(price.ToString().Substring(0, price.ToString().Length - 2) + "," + price.ToString().Substring(price.ToString().Length - 2));
                        }

                        decimal priceAmerica = deal.Price;

                        if (priceAmerica > 0)
                        {
                            priceAmerica = ToDecimal(priceAmerica.ToString().Substring(0, priceAmerica.ToString().Length - 2) + "," + priceAmerica.ToString().Substring(priceAmerica.ToString().Length - 2));
                        }

                        MoskitCustomField[] entityCustomFields = deal.CustomFields;

                        //string numeroPedido = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY")?.TextValue;

                        string numeroPedido = null;

                        if (MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY") != null)
                        {
                            numeroPedido = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY").TextValue;

                            throw new DiscardedException(FormatString("Pedido já cadastrado para o negócio: {0} - {1}", deal.Name, numeroPedido));
                        }

                        //
                        // Busco os dados do contato
                        //
                        long[] options = null;
                        string nome = "";
                        string telefoneContato = "";
                        string celular = "";
                        string emailContato = "";
                        string cpfCnpj = "";
                        string rg = "";
                        string cidade = "";
                        string bairro = "";
                        string cep = "";
                        string numero = "";
                        string logradouro = "";
                        string estado = "";
                        string complemento = "";

                        MoskitCustomField customField = null;

                        if (deal.Contacts != null && deal.Contacts.Length > 0)
                        {
                            providerResult = moskitProvider.Get<MoskitContact>(deal.Contacts[0].Id);

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            MoskitContact contact = (MoskitContact)providerResult.Detail;

                            if (contact != null && contact.CustomFields != null)
                            {
                                MoskitCustomField[] entityCustomFieldsContact = contact.CustomFields;

                                nome = contact.Name;

                                if (contact.Phones != null)
                                {
                                    telefoneContato = contact.Phones.Length > 0 ? contact.Phones[0].Number : "";
                                    celular = contact.Phones.Length > 1 ? contact.Phones[1].Number : "";
                                }

                                if (contact.Emails != null)
                                {
                                    emailContato = contact.Emails.Length > 0 ? contact.Emails[0].Address : "";
                                }

                                cpfCnpj = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_0WGqoGSKC9zK2qnP") != null ?
                                          MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_0WGqoGSKC9zK2qnP").TextValue : null;
                                rg = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGSvC6jZKm4X") != null ?
                                     MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGSvC6jZKm4X").TextValue : null;
                                cidade = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_42AmakSZCwrAPqjl") != null ?
                                         MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_42AmakSZCwrAPqjl").TextValue : null;
                                bairro = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_wPVm2oS2Cbj9gDK6") != null ?
                                         MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_wPVm2oS2Cbj9gDK6").TextValue : null;
                                cep = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_Pj3qYoSeCrBpamQe") != null ?
                                      MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_Pj3qYoSeCrBpamQe").TextValue : null;
                                numero = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_Rg7Mn4SLCA1XrDvd") != null ?
                                         MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_Rg7Mn4SLCA1XrDvd").TextValue : null;
                                logradouro = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_ylAm0KS6C5p91Mvb") != null ?
                                             MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_ylAm0KS6C5p91Mvb").TextValue : null;
                                complemento = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_dVKmQAS1CdPoemWR") != null ?
                                              MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_dVKmQAS1CdPoemWR").TextValue : null;
                                options = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGS9i6jZpm4X") != null ?
                                          MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGS9i6jZpm4X").Options : null;

                                if (options != null)
                                {
                                    providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_6rRmwGS9i6jZpm4X/options/{0}", options[0]));

                                    if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                                    {
                                        throw new Exception(providerResult.Message);
                                    }

                                    customField = (MoskitCustomField)providerResult.Detail;

                                    estado = ToString(customField.Label);
                                }
                            }
                        }

                        //int codigoVendedorSgr = 0;
                        //int codigoVendedorSga = 0;

                        //User userVSIntegra = Find<User>(FormatString("Number LIKE '%{0}%'", responsibleId));

                        //found = userVSIntegra != null;

                        //if (!found)
                        //{
                        //    throw new Exception("Usuário não encontrado");
                        //}

                        //string[] c = userVSIntegra.Number.Split(';');

                        //int cMoskit = c.Length > 0 ? ToInt(c[0]) : 0;
                        //codigoVendedorSgr = c.Length > 1 ? ToInt(c[1]) : 0;
                        //codigoVendedorSga = c.Length > 2 ? ToInt(c[2]) : 0;

                        //if (codigoVendedorSgr == 0) // 89475 - Pontotrack
                        //{
                        //    throw new Exception("Vendedor não encontrado");
                        //}

                        //
                        // Consultar produto, se produto estiver preenchido associar ao campo Tipo rastreados no sgr
                        //
                        string codTipoRastreador = "325";
                        string tipoAssociadoRastreador = "";

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_wPVm2VijibjB8mK6/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            codTipoRastreador = ToString(customField.Label);

                            codTipoRastreador =
                                codTipoRastreador.Equals("AVANÇADO COM IDENTIFICADOR DE MOTORISTA") ? "326" :
                                codTipoRastreador.Equals("NÃO INFORMADO") ? "449" :
                                codTipoRastreador.Equals("SATELITAL") ? "797" :
                                codTipoRastreador.Equals("SSX LOGISTICS") ? "800" :
                                codTipoRastreador.Equals("SSX PERFORMACE") ? "799" :
                                codTipoRastreador.Equals("SSX TRACKING") ? "328" :
                                codTipoRastreador.Equals("PORTATIL") ? "735" :
                                "";
                        }

                        string nomeProduto = "";

                        if (deal.DealProducts != null)
                        {
                            if (deal.DealProducts.Length > 0)
                            {
                                providerResult = moskitProvider.Get<MoskitProduct>(deal.DealProducts[0].Product.Id);

                                MoskitProduct product = (MoskitProduct)providerResult.Detail;

                                bool foundProduct = product != null;

                                if (!foundProduct)
                                {
                                    if (DateTime.Now.AddDays(-1) < ToDate(deal.CloseDate))
                                    {
                                        throw new DiscardedException(FormatString("Descartado por não informar o produto {0}", deal.Name));
                                    }

                                    throw new Exception(FormatString("Produto não informado {0}", deal.Name));
                                }

                                nomeProduto = ToString(product.Name);

                                //if (deal.DealProducts[0].FinalPrice > 0 && price <= 0)

                                if (deal.DealProducts[0].FinalPrice > 0)
                                {
                                    decimal valorVenda = deal.DealProducts[0].FinalPrice;

                                    if (valorVenda > 0)
                                    {
                                        price = ToDecimal(deal.DealProducts[0].FinalPrice.ToString().Substring(0, valorVenda.ToString().Length - 2) + "," + valorVenda.ToString().Substring(valorVenda.ToString().Length - 2));

                                        priceAmerica = ToDecimal(deal.DealProducts[0].FinalPrice.ToString().Substring(0, valorVenda.ToString().Length - 2) + "," + valorVenda.ToString().Substring(valorVenda.ToString().Length - 2));
                                    }
                                }
                            }
                        }

                        codTipoRastreador =
                                nomeProduto.Equals("AVANÇADO COM IDENTIFICADOR DE MOTORISTA") ? "326" :
                                nomeProduto.Contains("PLANO") ? "325" :
                                nomeProduto.Contains("RASTREAMENTO") ? "325" :
                                nomeProduto.Equals("SATELITAL") ? "797" :
                                nomeProduto.Equals("SSX LOGISTICS") ? "800" :
                                nomeProduto.Equals("SSX PERFORMACE") ? "799" :
                                nomeProduto.Equals("SSX TRACKING") ? "328" :
                                nomeProduto.Equals("PORTATIL") ? "735" :
                                "449";

                        int tipoProduto = 0;
                        int tipoProduto2 = 0;

                        // Quando o produto do negócio for Plano Light, Plano Plus, Plano Prime o produto no SGR é PROTEÇÃO VEICULAR
                        if (nomeProduto.ToUpper().Contains("RASTREAMENTO") || tipoAssociadoRastreador.ToUpper().Contains("CLIENTE/RASTREADOR"))
                        {
                            tipoProduto = 1;
                        }
                        else if (nomeProduto.ToUpper().Contains("PLANO") || tipoAssociadoRastreador.ToUpper().Contains("ASSOCIADO"))
                        {
                            tipoProduto = 2;
                            tipoProduto = 3;
                        }
                        else if (nomeProduto.ToUpper().Contains("RASTREAMENTO + ASSISTENCIA"))
                        {
                            tipoProduto = 3;
                        }

                        //if (venda.ToUpper().Contains("RASTREAMENTO + ASSISTENCIA"))
                        //{
                        //    tipoProduto = 3;
                        //}

                        if (tipoProduto == 0)
                        {
                            if (DateTime.Now.AddDays(-1) > ToDate(deal.CloseDate))
                            {
                                throw new DiscardedException(FormatString("Descartado por não informar o produto {0}", deal.Name));
                            }

                            throw new Exception(FormatString("Produto não informado {0}", deal.Name));
                        }

                        //
                        // Busca campos personalizados
                        //
                        nome = string.IsNullOrWhiteSpace(nome) ? deal.Name : nome;

                        string email = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5") != null ?
                                       MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5").TextValue : null;

                        email = string.IsNullOrWhiteSpace(email) ? emailContato : email;

                        string telefone1 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_075MJ2izS6yRkMaz") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_075MJ2izS6yRkMaz").TextValue : null;

                        telefone1 = string.IsNullOrWhiteSpace(telefone1) ? telefoneContato : telefone1;

                        string telefone2 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_x1kq6oinCwZd5MzY") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_x1kq6oinCwZd5MzY").TextValue : null;

                        string telefone3 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_KaZmKNiOC08ldMJk") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_KaZmKNiOC08ldMJk").TextValue : null;

                        cpfCnpj = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR").TextValue : null;

                        cpfCnpj = cpfCnpj != null ? StringClear(Regex.Replace(cpfCnpj, "[^0-9,]", "")) : null;

                        rg = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkinColgEmvL") != null ?
                             MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkinColgEmvL").TextValue : null;

                        //string dataNascimento = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA") != null ?
                        //    FormatDate(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA").DateValue, "yyyy-MM-dd hh:MM:ss") : null;

                        DateTime? dataNascimento = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA") != null ?
                            MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA").DateValue : null;

                        string profissao = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdJQlDO3") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdJQlDO3").TextValue : null;

                        string emails = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5") != null ?
                                        MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5").TextValue : null;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeidireglqQe") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeidireglqQe").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_Pj3qYeidireglqQe/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            estado = ToString(customField.Label);
                        }

                        cidade = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeieCrG2EqQe") != null ?
                                 MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeieCrG2EqQe").TextValue : null;

                        if (!string.IsNullOrWhiteSpace(cidade))
                        {
                            cidade = cidade.Contains(",") ? cidade.Replace(cidade.Substring(cidade.IndexOf(",")), "").Trim() : cidade;
                        }

                        bairro = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_POEMywieCJrxADdk") != null ?
                                 MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_POEMywieCJrxADdk").TextValue : null;

                        cep = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_K7Rm8QiRC9L88DbN") != null ?
                              MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_K7Rm8QiRC9L88DbN").TextValue : null;

                        if (cep != null)
                        {
                            cep = StringClear(cep.Replace(",", ""));
                            cep = Regex.Replace(StringClear(cep), "[^0-9,]", "").Length == 8 ? Regex.Replace(cep, "[^0-9,]", "") : null;
                        }

                        numero = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_6rRmweivC6rQ5q4X") != null ?
                                 MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_6rRmweivC6rQ5q4X").TextValue : null;
                        if (numero != null)
                        {
                            numero = StringClear(numero.Replace(",", ""));
                            //numero = Regex.Replace(numero, "[^0-9,]", "").Length != 8 ? null : Regex.Replace(numero, "[^0-9,]", "");
                        }

                        logradouro = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3nGqEoirCl8lkmYA") != null ?
                                     MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3nGqEoirCl8lkmYA").TextValue : null;
                        complemento = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_x1kq6oinCwBvRMzY") != null ?
                                      MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_x1kq6oinCwBvRMzY").TextValue : null;
                        string observacoes = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdk0QDO3") != null ?
                                             MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdk0QDO3").TextValue : null;
                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_y5lm56iyiY4L8DwW") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_y5lm56iyiY4L8DwW").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_y5lm56iyiY4L8DwW/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            tipoAssociadoRastreador = ToString(customField.Label);
                        }

                        string sexo = "";

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_nrLDXoiWikl02mOa") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_nrLDXoiWikl02mOa").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_nrLDXoiWikl02mOa/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            sexo = ToString(customField.Label);

                            sexo = sexo.Equals("Feminino") ? "F" : sexo.Equals("Masculino") ? "M" : "";
                        }

                        string estadoCivil = "";

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_oJZmP1i9iGoX5Dgv") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_oJZmP1i9iGoX5Dgv").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_oJZmP1i9iGoX5Dgv/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            estadoCivil = ToString(customField.Label);

                            estadoCivil = estadoCivil.Equals("Casado(a)") ? "CA" :
                                            estadoCivil.Equals("Solteio(a)") ? "SO " :
                                            estadoCivil.Equals("Viuvo(a)") ? "VI" :
                                            estadoCivil.Equals("Divorciado(a)") ? "DI" :
                                            estadoCivil.Equals("Separado(a)") ? "SE" :
                                            estadoCivil.Equals("União estavel") ? "CO" : "SO";
                        }

                        //
                        // Campos customizados Informações adicionais
                        //
                        int codPeriodoVenda = 0;
                        int codGrupoVenda = 0;

                        int quantidadeParcelaVenda = 1;

                        //long codGrupoMensalidade = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkieio21BmvL") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkieio21BmvL").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_2wpDlkieio21BmvL/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string grupoMensalidadePeriodo = ToString(customField.Label);

                            codGrupoVenda = grupoMensalidadePeriodo.Contains("ANUAL 588,00|588,00") ? 1053 :
                                            grupoMensalidadePeriodo.Contains("ANUAL 650,00|650,00") ? 1026 :
                                            grupoMensalidadePeriodo.Contains("ANUAL 660,00|660,00") ? 1039 :
                                            grupoMensalidadePeriodo.Contains("ANUAL 708,00|708,00") ? 1044 :
                                            grupoMensalidadePeriodo.Contains("ANUAL 720,00|720,00") ? 1063 :
                                            grupoMensalidadePeriodo.Contains("ANUAL 730,00|730,00") ? 1054 :
                                            grupoMensalidadePeriodo.Contains("ANUAL 750,00|750,00") ? 1051 :
                                            grupoMensalidadePeriodo.Contains("ANUAL 780,00|780,00") ? 1060 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE R$ 149,00|149,00") ? 1007 :

                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 49,90|49,90") ? 1066 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 55,90|55,90") ? 1067 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 65,90 (CARTÃO DE CREDITO)|65,90") ? 1074 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 69,90 (CARTÃO DE CREDITO)|69,90") ? 1073 :

                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 65,90|65,90") ? 1068 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 75,90|75,90") ? 1070 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 79,90|79,90") ? 1071 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 74,90|74,90") ? 1083 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 84,90|84,90") ? 1084 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 89,90|89,90") ? 1079 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 94,90|94,90") ? 1085 :

                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 95,90|95,90") ? 1087 :

                                            grupoMensalidadePeriodo.Contains("\"MENSALIDADE 59,90 (CARTÃO DE CREDITO)|59,90") ? 1075 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 59,90|59,90") ? 1065 :

                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 49,00|49,00") ? 1 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 50,00|50,00") ? 1038 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 55,00|55,00") ? 1056 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 59,00|59,00") ? 2 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 59,90 (PRIMEIRA 39,00)|59,90") ? 1050 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 60,00|60,00") ? 1015 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 64,90|64,90") ? 1033 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 65,00|65,00") ? 1045 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 69,00|69,00") ? 3 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 69,90 (PRIMEIRA 39,90)|69,90") ? 1048 :
                                            grupoMensalidadePeriodo.Contains("MENSALIDADE 79,00|79,00") ? 1034 :
                                            grupoMensalidadePeriodo.Contains("RASTREADOR COM ASSISTENCIA 24 HORAS|94,90") ? 1052 :
                                            0;

                            codPeriodoVenda = grupoMensalidadePeriodo.Contains("12 MESES") ? 336 :
                                                 grupoMensalidadePeriodo.Contains("36 MESES") ? 644 :
                                                 grupoMensalidadePeriodo.Contains("24 MESES") ? 649 :
                                                 grupoMensalidadePeriodo.Contains("6 MESES") ? 739 : //Ou 741, 759
                                                 grupoMensalidadePeriodo.Contains("7 MESES") ? 820 : 0;

                            quantidadeParcelaVenda = grupoMensalidadePeriodo.Contains("12 MESES") ? 12 :
                                                 grupoMensalidadePeriodo.Contains("36 MESES") ? 36 :
                                                 grupoMensalidadePeriodo.Contains("24 MESES") ? 24 :
                                                 grupoMensalidadePeriodo.Contains("6 MESES") ? 6 : //Ou 741, 759
                                                 grupoMensalidadePeriodo.Contains("7 MESES") ? 7 : 1;

                            price = ToDecimal(Substring(grupoMensalidadePeriodo, "|", "-"));

                            //
                            //Se for anual
                            //
                            if (codGrupoVenda == 1053 ||
                                codGrupoVenda == 1026 ||
                                codGrupoVenda == 1039 ||
                                codGrupoVenda == 1044 ||
                                codGrupoVenda == 1063 ||
                                codGrupoVenda == 1054 ||
                                codGrupoVenda == 1051 ||
                                codGrupoVenda == 1060
                                )
                            {
                                codPeriodoVenda = 0;
                                quantidadeParcelaVenda = 1;
                            }
                        }

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeidijrKrqQe") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeidijrKrqQe").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_Pj3qYeidijrKrqQe/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string grupoMensalidadePeriodo = ToString(customField.Label);

                            int position = grupoMensalidadePeriodo.IndexOf("-");

                            codGrupoVenda = ToInt(grupoMensalidadePeriodo.Substring(0, position));

                            //codGrupoVenda = ToInt(Substring(grupoMensalidadePeriodo, "", "-"));

                            price = ToDecimal(Substring(grupoMensalidadePeriodo, "|", "-"));

                            //codPeriodoVenda = grupoMensalidadePeriodo.Contains("12 MESES") ? 336 :
                            //                     grupoMensalidadePeriodo.Contains("36 MESES") ? 644 :
                            //                     grupoMensalidadePeriodo.Contains("24 MESES") ? 649 :
                            //                     grupoMensalidadePeriodo.Contains("6 MESES") ? 739 : //Ou 741, 759
                            //                     grupoMensalidadePeriodo.Contains("7 MESES") ? 820 : 0;

                            SgrPeriodo sgrPeriodo = sgrProvider.List<SgrPeriodo>()
                                .FirstOrDefault(e => StringClear(grupoMensalidadePeriodo, true, true, true, false).Contains(e.Descricao, StringComparison.CurrentCultureIgnoreCase));

                            if (sgrPeriodo == null)
                            {
                                throw new ValidationException($"Periodo de venda {grupoMensalidadePeriodo} não encontrada no SGR");
                            }

                            codPeriodoVenda = ToInt(sgrPeriodo?.CodPeriodo);

                            quantidadeParcelaVenda = grupoMensalidadePeriodo.Contains("12 MESES") ? 12 :
                                                 grupoMensalidadePeriodo.Contains("36 MESES") ? 36 :
                                                 grupoMensalidadePeriodo.Contains("24 MESES") ? 24 :
                                                 grupoMensalidadePeriodo.Contains("6 MESES") ? 6 : //Ou 741, 759
                                                 grupoMensalidadePeriodo.Contains("7 MESES") ? 7 : 1;

                        }

                        //CONDIÇÃO DE PAGAMENTO
                        int codCondicaoPagamentoVenda = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_E79Mr2iLipLlQMZJ") != null ?
                            MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_E79Mr2iLipLlQMZJ").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_E79Mr2iLipLlQMZJ/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string condicaoPagamento = ToString(customField.Label);

                            //codCondicaoPagamentoVenda = condicaoPagamento.Contains("BOLETO BANCARIO") ? 642 :
                            //                            condicaoPagamento.Contains("CARTAO DE CREDITO") ? 645 :
                            //                            condicaoPagamento.Contains("CARTÃO DÉBITO") ? 793 :
                            //                            condicaoPagamento.Contains("DINHEIRO") ? 347 :
                            //                            condicaoPagamento.Contains("TRANSFERÊNCIA VIA PIX") ? 794 :
                            //                            condicaoPagamento.Contains("ZERO") ? 762 : 0;


                            SgrCondicaoPagamento sgrCondicaoPagamento = sgrProvider.List<SgrCondicaoPagamento>()
                                .FirstOrDefault(e => StringClear(condicaoPagamento, true, true, true, false).Contains(e.Descricao, StringComparison.CurrentCultureIgnoreCase));

                            if (sgrCondicaoPagamento == null)
                            {
                                throw new ValidationException($"Condição de pagamento {condicaoPagamento} não encontrada no SGR");
                            }

                            codCondicaoPagamentoVenda = ToInt(sgrCondicaoPagamento?.CodCondicaoPagamento);
                        }

                        int codVencimentoVenda = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wGrqzpi3id0Y0mLo") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wGrqzpi3id0Y0mLo").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_wGrqzpi3id0Y0mLo/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            int vencimento = ToInt(customField.Label);

                            codVencimentoVenda = vencimento == 10 ? 284 :
                                                 vencimento == 15 ? 281 :
                                                 vencimento == 20 ? 283 :
                                                 vencimento == 25 ? 282 :
                                                 vencimento == 31 ? 581 :
                                                 vencimento == 5 ? 561 : 0;
                        }

                        //CONDIÇÃO DE PAGAMENTO ADESAO
                        int codCondicaoPagamentoAdesao = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5ibidnZVmWR") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5ibidnZVmWR").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_dVKmQ5ibidnZVmWR/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string condicaoPagamento = ToString(customField.Label);

                            //codCondicaoPagamentoAdesao = condicaoPagamento.Contains("BOLETO BANCARIO") ? 642 :
                            //                            condicaoPagamento.Contains("CARTAO DE CREDITO") ? 645 :
                            //                            condicaoPagamento.Contains("CARTÃO DÉBITO") ? 793 :
                            //                            condicaoPagamento.Contains("DINHEIRO") ? 347 :
                            //                            condicaoPagamento.Contains("TRANSFERÊNCIA VIA PIX") ? 794 :
                            //                            condicaoPagamento.Contains("ZERO") ? 762 : 0;

                            SgrCondicaoPagamento sgrCondicaoPagamento = sgrProvider.List<SgrCondicaoPagamento>()
                                .FirstOrDefault(e => StringClear(condicaoPagamento, true, true, true, false).Contains(e.Descricao, StringComparison.CurrentCultureIgnoreCase));

                            if (sgrCondicaoPagamento == null)
                            {
                                throw new ValidationException($"Condição de pagamento da adesão {condicaoPagamento} não encontrada no SGR");
                            }

                            codCondicaoPagamentoAdesao = ToInt(sgrCondicaoPagamento?.CodCondicaoPagamento);
                        }

                        //
                        //Grupo de adesão
                        //
                        int codGrupoAdesao = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_A4wMWNiLi69NQqB8") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_A4wMWNiLi69NQqB8").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_A4wMWNiLi69NQqB8/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string adesao = ToString(customField.Label);

                            //codGrupoAdesao = adesao.Contains("SEM TAXA") ? 1043 : adesao.Contains("INSTALACAO") ? 1002 : 0;
                            codGrupoAdesao = adesao.Contains("SEM TAXA DE INSTALAÇÃO") ? 1211 : adesao.Contains("TAXA DE INSTALAÇÃO") ? 1212 : 0;
                        }

                        decimal valorAdesao = 0;

                        if (codGrupoAdesao == 1002)
                        {
                            valorAdesao = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNbalm6a") != null ?
                                          ToDecimal(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNbalm6a").TextValue) : 0;
                        }

                        //
                        // Recupera a Indicação no SGR
                        //
                        int codIndicacao = 1;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_49RM16ixiB7nbmBW") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_49RM16ixiB7nbmBW").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_49RM16ixiB7nbmBW/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            Source source = Find<Source>($"CRMAD_DESCRIPTION='{customField.Label}'");

                            if (source != null)
                            {
                                codIndicacao = ToInt(source.Number);
                            }
                        }

                        //
                        //
                        //
                        string codPontoVenda = "";

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_QJXmA5iXiJEBpm25") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_QJXmA5iXiJEBpm25").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_QJXmA5iXiJEBpm25/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            codPontoVenda = ToString(customField.Label);

                            codPontoVenda = codPontoVenda.Equals("PONTO TRACK") ? "1" :
                                            codPontoVenda.Equals("AMERICA CLUBE DE BENEFICIOS") ? "2" :
                                            codPontoVenda.Equals("MARINGA") ? "3" :
                                            codPontoVenda.Equals("ASSOCIACAO MUTUALISTA VIA SUL") ? "4" :
                                            codPontoVenda.Equals("CURITIBA") ? "5" :
                                            codPontoVenda.Equals("LONDRINA") ? "7" : "";
                        }

                        if (IsNullOrEmpty(codPontoVenda) || codPontoVenda.Equals("1"))
                        {
                            throw new DiscardedException(FormatString("Ponto de venda não cadastrado: {0}", deal.Name));
                        }

                        //
                        // Dados do veículo cada veículo deve-se gerar um pedido no sgr
                        //
                        string modeloVeiculo = "";

                        string valorNegocio = ToString(deal.Price);

                        //string valorNegocioAmerica = "";

                        string renavam = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CN0jam6a") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CN0jam6a").TextValue : null;

                        string veiculo1 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2Vi2CbQLOmK6") != null ?
                                          MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2Vi2CbQLOmK6").TextValue : null;

                        string placa = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_oJZmP1iKCG7oLDgv") != null ?
                                       MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_oJZmP1iKCG7oLDgv").TextValue : null;

                        string anoFabricacao = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_0WGqoEioS90o3mnP") != null ?
                                               ToString(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_0WGqoEioS90o3mnP").NumericValue) : null;

                        anoFabricacao = !string.IsNullOrEmpty(anoFabricacao) ? anoFabricacao.Length != 4 ? null : anoFabricacao : null;

                        string anoModelo = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3nGqEoiPSl8l5mYA") != null ?
                                           ToString(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3nGqEoiPSl8l5mYA").NumericValue) : null;

                        anoModelo = !string.IsNullOrEmpty(anoModelo) ? anoModelo.Length != 4 ? null : anoModelo : null;

                        string chassi = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_VrKMbQiaCO72lqZY") != null ?
                                        MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_VrKMbQiaCO72lqZY").TextValue : null;

                        string tamanhoFrota = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNw8vm6a") != null ?
                                              ToString(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNw8vm6a").TextValue) : null;

                        string empresa = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1Cd93kmWR") != null ?
                                         ToString(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1Cd93kmWR").TextValue) : null;

                        int corVeiculoCod = 640;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_gvGm3BiaizpJ0M45") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_gvGm3BiaizpJ0M45").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_gvGm3BiaizpJ0M45/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string corVeiculo = ToString(customField.Label);

                            corVeiculoCod =
                                    corVeiculo.Equals("Amarelo") ? 216 :
                                                corVeiculo.Equals("Branco") ? 217 :
                                                corVeiculo.Equals("Bege") ? 218 :
                                                corVeiculo.Equals("Preto") ? 219 :
                                                corVeiculo.Equals("Vermelho") ? 220 :
                                                corVeiculo.Equals("Azul") ? 221 :
                                                corVeiculo.Equals("Prata") ? 222 :
                                                corVeiculo.Equals("Verde") ? 223 :
                                                corVeiculo.Equals("Cinza") ? 224 :
                                                corVeiculo.Equals("Laranja") ? 225 :
                                                corVeiculo.Equals("Dourado") ? 226 :
                                                corVeiculo.Equals("Marrom") ? 227 :
                                                corVeiculo.Equals("Não Informado") ? 640 :
                                                corVeiculo.Equals("Roxo") ? 738 :
                                                corVeiculo.Equals("Rosa") ? 760 :
                                                corVeiculo.Equals("Fantasia") ? 761 :
                                                 640;
                        }

                        string combustivelVeiculo = "335";

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wGrqzpi3ido00mLo") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wGrqzpi3ido00mLo").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_wGrqzpi3ido00mLo/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            combustivelVeiculo = ToString(customField.Label);

                            combustivelVeiculo = combustivelVeiculo.Equals("Alcool") ? "210" :
                                            combustivelVeiculo.Equals("Alcool/Gasolina") ? "796" :
                                            combustivelVeiculo.Equals("Flex") ? "212" :
                                            combustivelVeiculo.Equals("Diesel") ? "211" :
                                            combustivelVeiculo.Equals("Gasolina") ? "209" :
                                            combustivelVeiculo.Equals("GNV") ? "214" :
                                            combustivelVeiculo.Equals("Não Informado") ? "335" :
                                            combustivelVeiculo.Equals("Tetra Full") ? "213" : "335";
                        }

                        //
                        //Tipo do veiculo
                        //
                        int tipoVeiculo = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_oJZmP1i9iGwywDgv") != null ?
                                 MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_oJZmP1i9iGwywDgv").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Get<MoskitCustomField>(FormatString("CF_oJZmP1i9iGwywDgv/options/{0}", options[0]));

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string label = ToString(customField.Label);

                            tipoVeiculo = label.Equals("AUTOMOVEL") ? 2 :
                                          label.Equals("CAMINHAO") ? 3 :
                                          label.Equals("MOTOCICLETA") ? 4 :
                                          label.Equals("VAN/UTILITARIO") ? 5 :
                                          label.Equals("PICK-UP") ? 6 :
                                          label.Equals("CAMINHONETE") ? 7 :
                                          label.Equals("ÔNIBUS") ? 8 :
                                          label.Equals("ESCAVADEIRA") ? 10 :
                                          label.Equals("MAQUINAS") ? 11 :
                                          label.Equals("SEMI REBOQUE") ? 12 :
                                          label.Equals("-") ? 13 :
                                          label.Equals("BICICLETA") ? 663 :
                                          label.Equals("CICLOMOTOR") ? 664 :
                                          label.Equals("MOTONETA") ? 665 :
                                          label.Equals("TRICICLO") ? 666 :
                                          label.Equals("MICRO-ÔNIBUS") ? 667 :
                                          label.Equals("BONDE") ? 668 :
                                          label.Equals("REBOQUE") ? 669 :
                                          label.Equals("CHARRETE") ? 670 :
                                          label.Equals("CAMINHONETA") ? 671 :
                                          label.Equals("CARROÇA") ? 672 :
                                          label.Equals("CARRO DE MÃO") ? 673 :
                                          label.Equals("CAMINHÃO TRATOR") ? 674 :
                                          label.Equals("TRATOR DE RODAS") ? 675 :
                                          label.Equals("TRATOR ESTEIRAS") ? 676 :
                                          label.Equals("TRATOR MISTO") ? 677 :
                                          label.Equals("QUADRICICLO ") ? 678 :
                                          label.Equals("CHASSI/PLATAFORMA") ? 679 :
                                          label.Equals("SIDECAR") ? 680 :
                                          label.Equals("CAMIONETE") ? 720 :
                                          label.Equals("MALETA") ? 736 :
                                          label.Equals("NÃO IDENTIFICADO") ? 743 :
                                          label.Equals("SEMIRREBOQUE") ? 744 :
                                          label.Equals("CAMIONETA") ? 745 :
                                          label.Equals("CAMINHÃO") ? 746 :
                                          label.Equals("TRATOR DE ESTEIRAS") ? 747 :
                                          label.Equals("QUADRICICLO") ? 748 :
                                          label.Equals("UTILITÁRIO") ? 749 :
                                          label.Equals("DESCONHECIDO") ? 750 :
                                          label.Equals("AVIÃO") ? 751 :
                                          label.Equals("CAIXA") ? 752 :
                                          label.Equals("HELICÓPTERO") ? 753 :
                                          label.Equals("ANIMAL") ? 754 :
                                          label.Equals("MALA") ? 755 :
                                          label.Equals("PESSOA") ? 756 :
                                          label.Equals("OUTROS") ? 757 :
                                          label.Equals("JET SKI") ? 758 :
                                          label.Equals("LANCHA") ? 798 :
                                          label.Equals("INVERSOR ELÉTRICO") ? 832 : 0;
                        }

                        //
                        // Modelo do veículo, montar uma lista de veículos porque cada veículo deve-se gerar um pedido.
                        //
                        modeloVeiculo = veiculo1;

                        int codModeloVeiculo = 0;
                        int codMontadoraVeiculo = 0;

                        if (!IsNullOrEmpty(modeloVeiculo))
                        {
                            SgrProviderParam sgrProviderParam = new SgrProviderParam();
                            sgrProviderParam.VeiculoModeloDescricao = modeloVeiculo;

                            List<SgrVeiculoModelo> modelos = sgrProvider.List<SgrVeiculoModelo>(sgrProviderParam).ToList();

                            //int codModeloVeiculo = 0;
                            //int codMontadoraVeiculo = 0;

                            if (modelos.Count > 0)
                            {
                                int i = 0;
                                codModeloVeiculo = ToInt(modelos[i].CodVeiculoModelo);
                                codMontadoraVeiculo = ToInt(modelos[i].CodVeiculoMontadora);

                                if (tipoVeiculo == 0)
                                {
                                    do
                                    {
                                        try
                                        {
                                            tipoVeiculo = ToInt(modelos[i].CodTipoVeiculo);

                                            i++;
                                        }
                                        catch (Exception e)
                                        {
                                            break;
                                        }
                                    } while (tipoVeiculo == 0);
                                }
                            }
                        }

                        //
                        // 
                        //
                        //int codigoVendedorSga = 0;

                        User userVSIntegra = Find<User>($"Number LIKE '%{responsibleId}%'");

                        found = userVSIntegra != null;

                        if (!found)
                        {
                            throw new Exception("Usuário não encontrado");
                        }

                        //string[] c = userVSIntegra.Number.Split(';');

                        //int cMoskit = c.Length > 0 ? ToInt(c[0]) : 0;
                        //codigoVendedorSgr = c.Length > 1 ? ToInt(c[1]) : 0;
                        //codigoVendedorSga = c.Length > 2 ? ToInt(c[2]) : 0;

                        SgrVendedor vendedor = sgrProvider.List<SgrVendedor>(
                            new SgrProviderParam() { CodPontoVenda = ToLong(codPontoVenda) }
                        ).FirstOrDefault(e => ToString(e.Nome).Equals(userVSIntegra.Name));

                        long codigoVendedorSgr = ToLong(vendedor?.CodVendedor);

                        if (codigoVendedorSgr == 0) // 89475 - Pontotrack
                        {
                            throw new Exception("Vendedor não encontrado");
                        }

                        //
                        // Geração do pedido no SGR
                        //
                        SgrPedido pedido = new SgrPedido();

                        //1) PONTO DE VENDA, 
                        pedido.CodPontoVendaVenda = ToInt(codPontoVenda);

                        //2) VENDEDOR, 
                        pedido.CodConsultorVenda = ToInt(codigoVendedorSgr); // 1; // Pega da amarracao MOSKIT x SGR

                        if (!IsNullOrEmpty(cep) || !IsNullOrEmpty(numero))
                        {
                            SgrEndereco endereco = new SgrEndereco();
                            endereco.Cep = cep;
                            endereco.Logradouro = logradouro;
                            endereco.Numero = numero;
                            endereco.Bairro = bairro;
                            endereco.Cidade = cidade;
                            endereco.Estado = estado;
                            endereco.Complemento = complemento;

                            pedido.EnderecoVenda = endereco;
                        }

                        List<SgrTelefone> telefones = new List<SgrTelefone>();

                        if (!IsNullOrEmpty(celular))
                        {
                            SgrTelefone telefoneVenda = new SgrTelefone();
                            telefoneVenda.Contato = celular;
                            telefoneVenda.Tipo = "CELULAR";
                            telefoneVenda.CodDepartamento = "779"; //Sem informação

                            telefones.Add(telefoneVenda);
                        }

                        if (!IsNullOrEmpty(telefone1))
                        {
                            SgrTelefone telefoneVenda = new SgrTelefone();
                            telefoneVenda.Contato = telefone1;
                            telefoneVenda.Tipo = "FIXO";
                            telefoneVenda.CodDepartamento = "779"; //Sem informação
                            telefones.Add(telefoneVenda);
                        }

                        List<SgrContato> contatos = new List<SgrContato>();

                        if (!IsNullOrEmpty(telefone2))
                        {
                            SgrContato contatoVenda = new SgrContato();
                            contatoVenda.Contato = telefone2;
                            contatoVenda.Nome = "Contato 1";
                            contatoVenda.Tipo = "CELULAR";
                            contatoVenda.CodVinculo = "245";
                            contatos.Add(contatoVenda);

                        }

                        if (!IsNullOrEmpty(telefone3))
                        {
                            SgrContato contatoVenda2 = new SgrContato();
                            contatoVenda2.Contato = telefone3;
                            contatoVenda2.Nome = "Contato 2";
                            contatoVenda2.Tipo = "COMERCIAL";
                            contatoVenda2.CodVinculo = "245";
                            contatos.Add(contatoVenda2);
                        }

                        pedido.ContatosVenda = contatos.Count > 0 ? contatos.ToArray() : null;

                        pedido.TelefoneVenda = telefones.ToArray();

                        if (!IsNullOrEmpty(email))
                        {
                            SgrEmail emailVenda = new SgrEmail();
                            emailVenda.Contato = email;
                            emailVenda.CodDepartamento = "779"; //Sem informação

                            pedido.EmailVenda = emailVenda;
                        }

                        pedido.NomeClienteVenda = nomeNegocio; // nome;
                        pedido.CpfClienteVenda = cpfCnpj;
                        pedido.RgClienteVenda = rg == null ? "00.000.000-0" : rg;
                        pedido.SexoClienteVenda = sexo;
                        pedido.EstadoCivilClienteVenda = estadoCivil;
                        pedido.ProfissaoClienteVenda = profissao == null ? "" : profissao;
                        pedido.DataNascimentoClienteVenda = !IsNullOrEmpty(dataNascimento) ? FormatDate(dataNascimento.Value.AddHours(3), "yyyy-MM-dd") : "";

                        //string dataNascimento = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA") != null ?
                        //    FormatDate(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA").DateValue, "yyyy-MM-dd hh:MM:ss") : null;

                        pedido.ValorParcelaVenda = price;
                        pedido.ValorParcelaVenda = price;
                        pedido.FipeValorVeiculoVenda = price;

                        //4) PRODUTO                        
                        pedido.ProdutoVendaCodProdutos = new int[] { tipoProduto };

                        if (tipoProduto2 > 0)
                        {
                            pedido.ProdutoVendaCodProdutos = new int[] { tipoProduto, tipoProduto2 };
                        }

                        if (codPontoVenda.Equals("2"))
                        {
                            pedido.ProdutoVendaCodProdutos = new int[] { 1, 2 };
                            pedido.ValorParcelaVenda = priceAmerica;
                            pedido.ValorParcelaVenda = priceAmerica;
                            pedido.FipeValorVeiculoVenda = priceAmerica;
                        }


                        pedido.QuantidadeParcelaVenda = quantidadeParcelaVenda;
                        pedido.QuantidadeParcelaAdesaoVenda = 1;
                        pedido.AnoModVeiculoVenda = anoModelo;
                        pedido.AnoFabVeiculoVenda = anoFabricacao;
                        pedido.PlacaVeiculoVenda = placa;
                        pedido.ChassiVeiculoVenda = chassi;
                        pedido.RenavamVeiculoVenda = StringClear(renavam);
                        pedido.CodCombustivelVeiculoVenda = ToInt(combustivelVeiculo);

                        //Informações adicionais
                        if (codVencimentoVenda != 0)
                        {
                            pedido.CodVencimentoVenda = codVencimentoVenda;
                        }

                        if (codGrupoVenda != 0)
                        {
                            pedido.CodGrupoVenda = codGrupoVenda;
                        }

                        if (codPeriodoVenda != 0)
                        {
                            pedido.CodPeriodoVenda = codPeriodoVenda;
                        }

                        if (codCondicaoPagamentoVenda != 0)
                        {
                            pedido.CodFormaPagamentoVenda = codCondicaoPagamentoVenda; //Verificar se é esse campo
                        }

                        pedido.QuantidadeParcelaAdesaoVenda = 1;

                        if (codGrupoAdesao != 0)
                        {
                            pedido.CodGrupoAdesaoVenda = codGrupoAdesao;
                        }

                        if (valorAdesao != 0)
                        {
                            pedido.ValorParcelaAdesaoVenda = valorAdesao;
                        }

                        if (codCondicaoPagamentoAdesao != 0)
                        {
                            pedido.CodFormaPagamentoAdesaoVenda = codCondicaoPagamentoAdesao;
                        }

                        pedido.ObservacaoVenda = observacoes;

                        if (codModeloVeiculo != 0)
                        {
                            pedido.CodModeloVeiculoVenda = codModeloVeiculo;
                        }

                        if (codMontadoraVeiculo != 0)
                        {
                            pedido.CodMarcaVeiculoVenda = codMontadoraVeiculo;
                        }

                        if (corVeiculoCod != 0)
                        {
                            pedido.CodCorVeiculoVenda = corVeiculoCod;
                        }

                        if (tipoVeiculo != 0)
                        {
                            pedido.CodTipoVeiculoVenda = tipoVeiculo; //cod_tipo_veiculo_venda
                        }

                        ////9) INDICAÇÃO, 
                        pedido.CodIndicacaoVenda = codIndicacao; // 1; // codIndicacao; // 1; // codIndicacao;

                        //10) TIPO DE RASTREADOR
                        pedido.CodTipoRastreadorVenda = ToInt(codTipoRastreador); //1;

                        providerResult = sgrProvider.Post(pedido);

                        if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                        {
                            throw new Exception(providerResult.Message);
                        }

                        @event.StatusDetail = providerResult.Message;

                        //
                        //Pedido da venda
                        //
                        SgrPedido pedidoRetorno = (SgrPedido)providerResult.Detail;

                        pedido.CodVenda = pedidoRetorno.CodVenda;

                        //
                        // Atualizar o numero do pedido do sgr no moskit
                        //
                        deal.CustomFields = new MoskitCustomField[] { new MoskitCustomField("CF_dN7MGPiGCOLOrmeY", ToString(pedidoRetorno.CodVenda)) };

                        providerResult = moskitProvider.Put(deal);

                        if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                        {
                            //throw new Exception(providerResult.Message); // se der throw vai interferir na rotina.
                            @event.StatusDetail = providerResult.Message;
                        }

                        ////
                        //// Gero um evento do pedido para assinatura
                        ////
                        //Event eventBO = new Event();
                        //eventBO.Type = 1;
                        //eventBO.CreateDateTime = DateTime.Now;
                        //eventBO.EventText = ToJson(pedido);
                        //eventBO.Hash = ToHash(pedido);
                        //eventBO.Status = "00";
                        //eventBO.StatusReason = "Aguardando processamento";
                        //eventBO.StatusDetail = null;

                        //ret = Add(eventBO);

                        //if (ret != null)
                        //{
                        //    throw new Exception(ToString(ret));
                        //}
                        @event.Number = ToString(pedido.CodVenda);

                        //@event.Status = VSIntegra.Framework.v6011.Model.System.ValuesObject.EventVO.Success;
                        @event.Status = Event.Success;
                        @event.StatusReason = "Processado com sucesso";
                        @event.StatusDetail = null;

                        Log(pedido.CodVenda);
                    }
                    //catch (DiscardedException e)
                    //{
                    //    @event.Status = "10";
                    //    @event.StatusReason = "Processado com sucesso, evento descartado";
                    //    @event.StatusDetail = e.Message;
                    //}
                    //catch (FailedException e)
                    //{
                    //    @event.Status = "20";
                    //    @event.StatusReason = "Falha ao processar o evento, verifique e envie novamente";
                    //    @event.StatusDetail = e.Message;
                    //}
                    catch (Exception e)
                    {
                        //@event.Status = "00";
                        //@event.StatusReason = "Falha ao processar o evento";
                        //@event.StatusDetail = e.Message;
                        HandleEventException(e, @event);
                    }

                    ret = Update(@event);

                    if (ret != null)
                    {
                        throw new GeneralException(ret);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskCriaPedidoErpSgrSgaParaLeadsGanhos finalizado...");

            Delay(60);
        }

        /// <summary>
        /// Monitorar os pedidos do SGR e sempre que o vendedor alterar o situacao(status) para AGUARDANDO ASSINATURA deve-se gerar o pedido e o contrato em PDF, e enviá-lo para o D4SIGN
        ///  - Fazer o upload do pedido de venda(principal) e o contrato(anexo) no d4sign
        ///  - POST /documents/{UUID-SAFE}/upload
        ///  - POST/documents/{UUID-DOC-PRINCIPAL}/ uploadslave
        ///  - Cadastrar os signatarios do documento no d4sign
        ///  - POST/documents/{UUID-DOCUMENT}/ createlist
        ///  - Enviar os documentos para a assinatura
        ///  - POST/documents/{UUID-DOCUMENT}/ sendtosigner
        /// PUBLICADO
        /// </summary>
        private void TaskMonitorarPedidosSgrAguardandoAssinaturaParaD4Sign()
        {
            Log("TaskMonitorarPedidosSgrAguardandoAssinaturaParaD4Sign iniciado...");

            try
            {
                ProviderBase moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");

                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                ProviderBase d4signProvider = GetProvider<D4SignProvider>("464998e8-7ca5-44db-8270-10d0c5c3f849");
                //DownloadProvider downloadProvider = new DownloadProvider();

                IEnumerable events = List<Event>(
                    "Type=1 " +
                    "AND Status='00'",
                    "CreateDateTime ASC"
                );

                foreach (Event eventt in events)
                {
                    ProviderResult result = null;

                    if (!IsRunning)
                    {
                        break;
                    }

                    try
                    {
                        //            JObject data = ToJSonObject(eventt.EventText);
                        //            string codVenda = ToString(data["cod_venda"]);

                        //            string dataEmissao = eventt.CreateDateTime.ToString("dd/MM/yyyy");
                        //            string horaEmissao = eventt.CreateDateTime.ToString("hh:mm:ss");

                        //            //Cliente
                        //            string clienteNome = ToString(data["nome_cliente_venda"]);
                        //            string clienteCpf = ToString(data["cpf_cliente_venda"]);
                        //            string clienteLogradouro = ToString(data["endereco_venda"]["logradouro"]);
                        //            string clienteBairro = ToString(data["endereco_venda"]["bairro"]);
                        //            string clienteCidade = ToString(data["endereco_venda"]["cidade"]);
                        //            string clienteEstado = ToString(data["endereco_venda"]["estado"]);
                        //            string clienteComplemento = ToString(data["endereco_venda"]["complemento"]);
                        //            string clienteNumero = ToString(data["endereco_venda"]["numero"]);
                        //            string clienteEmail = ToString(data["email_venda"]["contato"]);


                        //            string clienteTelefone = "";


                        //            if (data["telefone_venda"] != null)
                        //            {
                        //                JArray telefones = (JArray)data["telefone_venda"];
                        //                clienteTelefone = telefones.Count > 0 ? "+55" + ToString(telefones[0]["contato"]) : null;
                        //            }

                        //            string telefone1 = "";
                        //            string telefone2 = "";

                        //            //JObject contatos1 = JObject.Parse("data[\"contato_venda\"]")

                        //            if (data["contato_venda"] != null)
                        //            {
                        //                JArray contatos = (JArray)data["contato_venda"];
                        //                telefone1 = contatos.Count > 0 ? "+55" + ToString(contatos[0]["contato"]) : null;
                        //                telefone2 = contatos.Count > 1 ? "+55" + ToString(contatos[1]["contato"]) : null;
                        //            }


                        //            //Veiculo
                        //            string placaVeiculo = ToString(data["placa_veiculo_venda"]);
                        //            string anoFabricacao = ToString(data["anofab_veiculo_venda"]);
                        //            string anoModelo = ToString(data["anomod_veiculo_venda"]);
                        //            string renavam = ToString(data["renavam_veiculo_vend"]);
                        //            string valorParcela = ToString(data["valor_parcela_venda"]);
                        //            string valorParcelaAdesao = ToString(data["valor_parcela_adesao_venda"]);

                        //            string observacaoVenda = ToString(data["observacao_venda"]);


                        //            //Dados Padrão da pontotrack
                        //            string telefone = "(43)3017-0227"; // ToString(data["nome_cliente_venda"]);
                        //            string email = "atendimento@pontotrack.com";// ToString(data["nome_cliente_venda"]);

                        //            //SgrProviderParam sgrProviderParam = new SgrProviderParam();

                        //            //sgrProviderParam.VeiculoModeloDescricao = codModeloVeiculo;

                        //            //List<SgrVeiculoModelo> modelos = sgrProvider.Find<SgrVeiculoModelo>(codModeloVeiculo);

                        //            JObject resultSgr = null;
                        //            bool found = false;

                        //            //
                        //            // Gero o token para não ocorrer erro no cookies
                        //            //
                        //            CookieContainer CookieContainer = new CookieContainer();
                        //            CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                        //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                        //                $"cliente={"3569"}&" +
                        //                $"nome={"moskit"}&" +
                        //                $"senha={"2SI7WG"}");

                        //            request.Method = "POST";
                        //            request.Accept = "application/json";
                        //            request.CookieContainer = CookieContainer;

                        //            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        //            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        //            JObject jObj = JObject.Parse(json);

                        //            string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
                        //            string Authorization = jObj["Headers"]["Authorization"].ToString();

                        //            //
                        //            // Busca Ponto de venda
                        //            //
                        //            //string pontoVenda = null;

                        //            //long codPontoVenda = ToLong(data["cod_ponto_venda_venda"]);

                        //            //if (codPontoVenda > 0)
                        //            //{
                        //            //    //No sgr está errado a forma de pagamento é a condição de pagamento
                        //            //    resultSgr = BuscaSgr($"get_ponto_venda/688472668f481b3efbddb0bfbff99cf6?cod_ponto_venda={codPontoVenda}",
                        //            //                        XAuthToken,
                        //            //                        Authorization,
                        //            //                        CookieContainer);

                        //            //    found = resultSgr != null;

                        //            //    if (found)
                        //            //    {
                        //            //        pontoVenda = ToString(resultSgr["data"][0]["nome"]);
                        //            //    }
                        //            //}

                        //            string pontoVenda = ToString(data["cod_ponto_venda_venda"]);

                        //            SgrProviderParam sgrProviderParam = new SgrProviderParam();
                        //            sgrProviderParam.CodPontoVenda = pontoVenda;

                        //            ProviderResult providerResult = sgrProvider.Get<SgrPontoVenda>(sgrProviderParam);

                        //            if (providerResult.Status == 500)
                        //            {
                        //                throw new Exception(providerResult.Message);
                        //            }

                        //            SgrPontoVenda sgrPontoVenda = (SgrPontoVenda)providerResult.Detail;

                        //            if (sgrPontoVenda != null)
                        //            {
                        //                pontoVenda = sgrPontoVenda.Nome;
                        //            }

                        //            ////
                        //            //// Busca produtos 1
                        //            ////
                        //            //string produtos = null;

                        //            //string descProduto1 = null;

                        //            //JArray produtosSgr = (JArray)(data["ProdutoVendaCodProdutos"]);

                        //            //if (produtosSgr.Count > 0)
                        //            //{
                        //            //    //long produto1 = ToLong(data["produto_venda[0][cod_produto]"]);

                        //            //    long produto1 = ToLong(produtosSgr[0]);

                        //            //    if (produto1 > 0)
                        //            //    {
                        //            //        resultSgr = BuscaSgr($"get_produtos/688472668f481b3efbddb0bfbff99cf6?cod_produto={produto1}",
                        //            //                    XAuthToken,
                        //            //                    Authorization,
                        //            //                    CookieContainer);

                        //            //        found = resultSgr != null;

                        //            //        if (found)
                        //            //        {
                        //            //            descProduto1 = ToString(resultSgr["data"][0]["descricao"]);
                        //            //        }

                        //            //        produtos = descProduto1;
                        //            //    }
                        //            //}

                        //            ////
                        //            //// Busca produtos 2
                        //            ////    
                        //            //string descProduto2 = null;

                        //            //if (produtosSgr.Count > 1)
                        //            //{
                        //            //    long produto2 = ToLong(produtosSgr[1]);

                        //            //    if (produto2 > 0)
                        //            //    {
                        //            //        resultSgr = BuscaSgr($"get_produto/688472668f481b3efbddb0bfbff99cf6?cod_produto={produto2}",
                        //            //                    XAuthToken,
                        //            //                    Authorization,
                        //            //                    CookieContainer);

                        //            //        found = resultSgr != null;

                        //            //        if (found)
                        //            //        {
                        //            //            descProduto2 = ToString(resultSgr["data"][0]["descricao"]);
                        //            //        }

                        //            //        produtos = $"{descProduto1}, {descProduto2}";
                        //            //    }
                        //            //}

                        //            string produtos = null;

                        //            JArray codProdutos = (JArray)(data["ProdutoVendaCodProdutos"]);

                        //            foreach (JValue codProduto in codProdutos)
                        //            {
                        //                long produto = ToLong(codProduto);

                        //                sgrProviderParam = new SgrProviderParam();
                        //                sgrProviderParam.CodProduto = produto;

                        //                providerResult = sgrProvider.Get<SgrProduto>(sgrProviderParam);

                        //                if (providerResult.Status == 500)
                        //                {
                        //                    throw new Exception(providerResult.Message);
                        //                }

                        //                SgrProduto sgrProduto = (SgrProduto)providerResult.Detail;

                        //                if (sgrProduto != null)
                        //                {
                        //                    produtos = $"{sgrProduto.Descricao}, ";
                        //                }
                        //            }

                        //            if (!IsNullOrEmpty(produtos))
                        //            {
                        //                produtos = produtos.Substring(0, produtos.Length - 2);
                        //            }

                        //            //
                        //            // Busca Origem/Indicação
                        //            //    
                        //            //string origem = null;

                        //            //long codOrigem = ToLong(data["cod_indicacao_venda"]);

                        //            //if (codOrigem > 0)
                        //            //{
                        //            //    resultSgr = BuscaSgr($"get_indicacao/688472668f481b3efbddb0bfbff99cf6?cod_indicacao={codOrigem}",
                        //            //                XAuthToken,
                        //            //                Authorization,
                        //            //                CookieContainer);

                        //            //    found = resultSgr != null;

                        //            //    if (found)
                        //            //    {
                        //            //        origem = ToString(resultSgr["data"][0]["nome"].ToString());
                        //            //    }
                        //            //}

                        //            //
                        //            // Busca consultor de venda
                        //            //    
                        //            string vendedor = null;
                        //            string emailConsultor = null;
                        //            string telefoneConsultor = null;

                        //            long codConsultorVenda = ToLong(data["cod_consultor_venda"]);

                        //            if (codConsultorVenda > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_vendedor/688472668f481b3efbddb0bfbff99cf6?cod_ponto_venda={sgrPontoVenda.CodPontoVenda}&cod_vendedor={codConsultorVenda}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    vendedor = ToString(resultSgr["data"][0]["nome"]);
                        //                }
                        //            }
                        //            //
                        //            //Busca forma de pagamento de venda
                        //            //No sgr está errado a forma de pagamento é a condição de pagamento
                        //            string formaPagamentoVenda = null;
                        //            long codFormaPagamentoVenda = ToLong(data["cod_forma_pagamento_venda"]);

                        //            if (codFormaPagamentoVenda > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_condicao_pagamento/688472668f481b3efbddb0bfbff99cf6?cod_condicao_pagamento={codFormaPagamentoVenda}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    formaPagamentoVenda = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca forma de pagamento de venda
                        //            //No sgr está errado a forma de pagamento é a condição de pagamento
                        //            string formaPagamentoAdesao = null;
                        //            long codFormaPagamentoAdesao = ToLong(data["cod_forma_pagamento_adesao_venda"]);

                        //            if (codFormaPagamentoAdesao > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_condicao_pagamento/688472668f481b3efbddb0bfbff99cf6?cod_condicao_pagamento={codFormaPagamentoAdesao}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    formaPagamentoAdesao = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca modelo do veiculo
                        //            //
                        //            string modeloVeiculo = null;
                        //            long codModeloVeiculo = ToLong(data["cod_modelo_veiculo_venda"]);

                        //            if (codModeloVeiculo > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_veiculo_modelo/688472668f481b3efbddb0bfbff99cf6?cod_veiculo_modelo={codModeloVeiculo}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    modeloVeiculo = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca marca
                        //            //
                        //            string marcaVeiculo = null;
                        //            long codMarcaVeiculo = ToLong(data["cod_marca_veiculo_venda"]);

                        //            if (codMarcaVeiculo > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_marca_veiculo/688472668f481b3efbddb0bfbff99cf6?cod_marca_veiculo={codMarcaVeiculo}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    marcaVeiculo = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca tipo rastreador
                        //            //
                        //            string tipoRastreador = null;
                        //            long codTipoRastreador = ToLong(data["cod_tipo_rastreador_venda"]);

                        //            if (codTipoRastreador > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_tipo_rastreador/688472668f481b3efbddb0bfbff99cf6?cod_tipo_rastreador={codTipoRastreador}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    tipoRastreador = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca indicação
                        //            //
                        //            //string indicacao = null;

                        //            //long codIndicacao = ToLong(data["cod_indicacao_venda"]);

                        //            //if (codIndicacao > 0)
                        //            //{
                        //            //    resultSgr = BuscaSgr($"get_indicacao/688472668f481b3efbddb0bfbff99cf6?cod_indicacao={codIndicacao}",
                        //            //                XAuthToken,
                        //            //                Authorization,
                        //            //                CookieContainer);

                        //            //    found = resultSgr != null;

                        //            //    if (found)
                        //            //    {
                        //            //        indicacao = ToString(resultSgr["data"][0]["nome"]);
                        //            //    }
                        //            //}
                        //            string indicacao = ToString(data["cod_indicacao_venda"]);

                        //            sgrProviderParam = new SgrProviderParam();
                        //            sgrProviderParam.CodIndicacao = indicacao;

                        //            providerResult = sgrProvider.Get<SgrIndicacao>(sgrProviderParam);

                        //            if (providerResult.Status == 500)
                        //            {
                        //                throw new Exception(providerResult.Message);
                        //            }

                        //            SgrIndicacao sgrIndicacao = (SgrIndicacao)providerResult.Detail;

                        //            if (sgrIndicacao != null)
                        //            {
                        //                indicacao = $"{sgrIndicacao.Nome}";
                        //            }

                        //            //
                        //            //Busca cor do veiculo
                        //            //
                        //            string corVeiculo = null;
                        //            long codCorVeiculo = ToLong(data["cod_cor_veiculo_venda"]);

                        //            if (codCorVeiculo > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_cor/688472668f481b3efbddb0bfbff99cf6?cod_cor={codCorVeiculo}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    corVeiculo = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca combustivel do veiculo
                        //            //
                        //            string combustivelVeiculo = null;
                        //            long codCombustivelVeiculo = ToLong(data["cod_combustivel_veiculo_venda"]);

                        //            if (codCombustivelVeiculo > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_combustivel/688472668f481b3efbddb0bfbff99cf6?cod_combustivel={codCombustivelVeiculo}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    combustivelVeiculo = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca grupo de venda
                        //            //
                        //            string grupoMensalidade = null;
                        //            long codGrupoMensalidade = ToLong(data["cod_grupo_venda"]);

                        //            if (codGrupoMensalidade > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_grupo_mensalidade/688472668f481b3efbddb0bfbff99cf6?cod_grupo_mensalidade={codGrupoMensalidade}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    grupoMensalidade = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca grupo de adesão
                        //            //
                        //            string grupoAdesao = null;

                        //            long codGrupoAdesao = ToLong(data["cod_grupo_adesao_venda"]);

                        //            if (codGrupoAdesao > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_grupo_adesao/688472668f481b3efbddb0bfbff99cf6?cod_grupo_adesao={codGrupoAdesao}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    grupoAdesao = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca periodo
                        //            //
                        //            string periodo = null;

                        //            long codPeriodo = ToLong(data["cod_periodo_venda"]);

                        //            if (codPeriodo > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_periodo/688472668f481b3efbddb0bfbff99cf6?cod_periodo={codPeriodo}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    periodo = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //Busca vencimento
                        //            //
                        //            string vencimento = null;

                        //            long codVencimento = ToLong(data["cod_vencimento_venda"]);

                        //            if (codVencimento > 0)
                        //            {
                        //                resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"get_vencimento/688472668f481b3efbddb0bfbff99cf6?cod_vencimento={codVencimento}",
                        //                            XAuthToken,
                        //                            Authorization,
                        //                            CookieContainer);

                        //                found = resultSgr != null;

                        //                if (found)
                        //                {
                        //                    vencimento = ToString(resultSgr["data"][0]["descricao"]);
                        //                }
                        //            }

                        //            //
                        //            //  - Fazer o upload do pedido de venda(principal) e o contrato(anexo) no d4sign
                        //            //

                        //            //
                        //            // Principal: Pedido
                        //            //  - POST /documents/{UUID-SAFE}/upload
                        //            //

                        //            //
                        //            // Criar o arquivo pdf do pedido com base no parametro
                        //            // nesse PEDIDO_TEMPLATE irá conter o html do pedido com os pares {} com cada campo que vai ser substituido
                        //            //

                        //            // Versão anterior
                        //            //string filename = ToString(GetCustomParameter("PEDIDO_TESTE"));

                        //            //result = downloadProvider.Find<DownloadDocument>(filename);

                        //            //if (result.Status == 500)
                        //            //{
                        //            //    throw new Exception(result.Message);
                        //            //}

                        //            //DownloadDocument downloadDocument = (DownloadDocument)result.Detail;

                        //            //byte[] bytes = downloadDocument.Document;

                        //            //if (bytes.Length == 0)
                        //            //{
                        //            //    throw new FailedException("O arquivo de pedido informado é invalido");
                        //            //}

                        //            // Nova versão
                        //            string template = ToString(GetCustomParameter("PEDIDO_TEMPLATE"));

                        //            var args = new string[,] {
                        //                {"{NUMERO}", codVenda},
                        //                {"{EMISSAO_DATA}",dataEmissao},
                        //                {"{EMISSAO_HORA}", horaEmissao},
                        //                {"{VENDEDOR_TELEFONE}",telefone},
                        //                {"{VENDEDOR_EMAIL}", email},
                        //                {"{VENDEDOR}", vendedor},
                        //                {"{CLIENTE}", clienteNome},
                        //                {"{CLIENTE_CPF}", clienteCpf},
                        //                {"{PONTO_VENDA}", pontoVenda},
                        //                {"{CLIENTE_LOGRADOURO}", clienteLogradouro},
                        //                {"{CLIENTE_NUMERO}", clienteNumero},
                        //                {"{CLIENTE_COMPLEMENTO}", clienteComplemento},
                        //                {"{CLIENTE_BAIRRO}", clienteBairro},
                        //                {"{CLIENTE_CIDADE}", clienteCidade},
                        //                {"{CLIENTE_ESTADO}", clienteEstado},
                        //                {"{CLIENTE_TELEFONE}", clienteTelefone},
                        //                {"{CLIENTE_CELULAR}", telefone1},
                        //                {"{CLIENTE_CELULAR2}", telefone2},
                        //                {"{CLIENTE_EMAIL}", clienteEmail},
                        //                //{"{ORIGEM}", origem},
                        //                {"{INDICACAO}", indicacao},
                        //                {"{VEICULO_PLACA}", placaVeiculo},
                        //                {"{VEICULO_RENAVAM}", renavam},
                        //                {"{VEICULO_MODELO}", modeloVeiculo},
                        //                {"{VEICULO_FABRICACAO}", anoFabricacao},
                        //                {"{VEICULO_MODELO}", anoModelo},
                        //                {"{VEICULO_MARCA}", marcaVeiculo},
                        //                {"{VEICULO_COR}", corVeiculo},
                        //                {"{PASIVEL_BLOQUEIO}", "NÃO"},//SIM OU NÃO
                        //                {"{PRODUTOS_CONTRATADOS}",produtos },
                        //                {"{DESCONTO_VENCIMENTO}", "Desconto de 10,00 para pagamentos até o vencimento"},
                        //                {"{FORMA_PAGAMENTO_VENDA}", formaPagamentoVenda},
                        //                {"{FORMA_PAGAMENTO_ADESAO}", formaPagamentoAdesao},
                        //                {"{TAXA_INSTALACAO}", valorParcelaAdesao},
                        //                {"{VALOR_MENSAL}", valorParcela},
                        //                {"{MODALIDADE}", "COMODATO"},
                        //                {"{PRAZO_CONTRATUAL}", periodo},
                        //                {"{DATA_VENCIMENTO}", $"DIA {vencimento}"},
                        //                {"{MES_VENCIMENTO}", ""},//janeiro...

                        //                {"{OBSERVACOES}", observacaoVenda},
                        //                {"{CIDADE}", "LONDRINA"},
                        //                {"{DATA}", DateTime.Today.ToString("dd/MM/yyyy")}
                        //            };

                        //            byte[] bytes = ToPDF(template, args);

                        //            //
                        //            // Esse trecho é usado apenas para degugar e ver como esta o PDF
                        //            //
                        //            var uuid = Guid.NewGuid();

                        //            string filename = @$"tmp\{uuid}.pdf";

                        //            var assemblyPath = Assembly.GetExecutingAssembly().Location;

                        //            string startUpDirectory = new FileInfo(assemblyPath).Directory.FullName;

                        //            string fullFilename = Path.Combine(startUpDirectory, filename);

                        //            System.IO.File.WriteAllBytes(fullFilename, bytes);

                        //            //
                        //            // fim
                        //            //

                        //            //
                        //            string bytesInBase64 = ToBase64(bytes);

                        //            D4SignDocument d4SignDocument = new D4SignDocument();
                        //            d4SignDocument.Base64BinaryFile = bytesInBase64;
                        //            //d4SignDocument.MimeType = file.MimeType;
                        //            d4SignDocument.MimeType = "application/pdf";

                        //            // Para teste
                        //            //d4SignDocument.Name = FormatString("{0}", clienteNome);
                        //            d4SignDocument.Name = FormatString("{0}", "INTEGRACAO TESTE - PEDIDO");

                        //            //d4SignDocument.UuidSafe = "0145b22e-0c2e-4ce0-b2c8-9182897f7c78";

                        //            string safe = ToString(GetCustomParameter("D4SIGN_COFRE_OUTROS", "eb5481b1-bb8c-4a16-b4f2-65435435d0f7"));

                        //            if (pontoVenda.ToUpper().Contains("AMERICA"))
                        //            {
                        //                safe = ToString(GetCustomParameter("D4SIGN_COFRE_AMERICA", "19b18ca9-d221-4e72-92ca-30e0e78405ea"));
                        //            }

                        //            d4SignDocument.UuidSafe = safe;

                        //            result = d4signProvider.Post(d4SignDocument);

                        //            if (result.Status == 500)
                        //            {
                        //                throw new Exception(result.Message);
                        //            }

                        //            d4SignDocument = (D4SignDocument)result.Detail;

                        //            string uuidDoc = d4SignDocument.UuidDoc;

                        //            //
                        //            //Anexo do moskit
                        //            //
                        //            //
                        //            //busca deal no moskit
                        //            //  
                        //            providerResult = moskitProvider.Get<MoskitDeal>(
                        //                new MoskitProviderParam("CF_dN7MGPiGCOLOrmeY", "match", new object[] { ToString(codVenda) })
                        //            );

                        //            MoskitDeal deal = (MoskitDeal)providerResult.Detail;

                        //            bool foundDeal = deal != null;

                        //            if (foundDeal)
                        //            {

                        //                //
                        //                //Busco os anexos.
                        //                //

                        //                //
                        //                //primeiro request

                        //                //string parans = "";//ToJson(new { name = "" });

                        //                //string uri = $"https://api.moskitcrm.com/v2/deals/19383862/attachments/";

                        //                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                        //                //HttpClient client = new HttpClient();

                        //                //var request2 = new HttpRequestMessage
                        //                //{
                        //                //    Method = HttpMethod.Post,
                        //                //    RequestUri = new Uri(uri),
                        //                //    Content = new StringContent(parans, Encoding.UTF8, "application/json")
                        //                //};

                        //                //request.Headers.Add("apikey", $"8a3f08a1-d459-4201-9fea-cff64d7696ca");

                        //                //var response2 = client.SendAsync(request2).ConfigureAwait(false);

                        //                //var responseInfo = response2.GetAwaiter().GetResult();

                        //                //string result2 = responseInfo.Content.ReadAsStringAsync().Result;


                        //                //
                        //                //segundo request
                        //                //
                        //                //string parans = ""; //ToJson(new { name = name, phone = phone });

                        //                //string uri = $"https://api.moskitcrm.com/v2/deals/19383862/attachments";

                        //                //providerResult = GetHttpClient(
                        //                //    uri,
                        //                //    null,
                        //                //    //new StringContent(null, Encoding.UTF8, "application/json"),
                        //                //    HttpMethod.Get
                        //                //);                            

                        //                //if (providerResult.Status == 500)
                        //                //{
                        //                //    throw new Exception(providerResult.Message);
                        //                //}

                        //                //
                        //                // Busca anexos
                        //                //
                        //                try
                        //                {
                        //                    request = (HttpWebRequest)WebRequest.Create($"https://api.moskitcrm.com/v2/deals/19383862/attachments");
                        //                    request.Headers.Add("apikey", "8a3f08a1-d459-4201-9fea-cff64d7696ca");
                        //                    request.Method = "GET";

                        //                    response = (HttpWebResponse)request.GetResponse();

                        //                    if (response.StatusCode != HttpStatusCode.OK)
                        //                    {
                        //                        //throw new Exception(response.StatusDescription);
                        //                    }

                        //                    json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        //                    //var results = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());

                        //                    JArray anexos = JArray.Parse(json);

                        //                    foreach (JObject anexo in anexos)
                        //                    {
                        //                        filename = ToString(anexo["url"]);

                        //                        result = downloadProvider.Get<DownloadDocument>(filename);

                        //                        if (result.Status == 500)
                        //                        {
                        //                            throw new Exception(result.Message);
                        //                        }

                        //                        DownloadDocument downloadDocument2 = (DownloadDocument)result.Detail;

                        //                        bytes = downloadDocument2.Document;

                        //                        if (bytes.Length == 0)
                        //                        {
                        //                            throw new FailedException("O arquivo de contrato informado é invalido");
                        //                        }

                        //                        bytesInBase64 = ToBase64(bytes);

                        //                        D4SignDocumentSlave attachment2 = new D4SignDocumentSlave();
                        //                        attachment2.Base64BinaryFile = bytesInBase64;
                        //                        attachment2.MimeType = "application/pdf";

                        //                        attachment2.Name = FormatString("{0}", "INTEGRACAO TESTE 2 - CONTRATO");

                        //                        attachment2.UuidDocMaster = uuidDoc;

                        //                        result = d4signProvider.Post(attachment2);

                        //                        if (result.Status == 500)
                        //                        {
                        //                            throw new Exception(result.Message);
                        //                        }

                        //                    }

                        //                    object results = JsonConvert.DeserializeObject<object>(json);
                        //                }
                        //                catch (Exception e)
                        //                {
                        //                    Log(e);
                        //                }
                        //            }


                        //            //
                        //            // Principal: Anexo 1
                        //            //  - POST/documents/{UUID-DOC-PRINCIPAL}/uploadslave

                        //            //string
                        //            filename = ToString(GetCustomParameter("D4SIGN_CONTRATO_PADRAO"));

                        //            result = downloadProvider.Get<DownloadDocument>(filename);

                        //            if (result.Status == 500)
                        //            {
                        //                throw new Exception(result.Message);
                        //            }

                        //            DownloadDocument downloadDocument = (DownloadDocument)result.Detail;

                        //            bytes = downloadDocument.Document;

                        //            if (bytes.Length == 0)
                        //            {
                        //                throw new FailedException("O arquivo de contrato informado é invalido");
                        //            }

                        //            bytesInBase64 = ToBase64(bytes);

                        //            D4SignDocumentSlave attachment1 = new D4SignDocumentSlave();
                        //            attachment1.Base64BinaryFile = bytesInBase64;
                        //            //attachment1.MimeType = file.MimeType;
                        //            attachment1.MimeType = "application/pdf";

                        //            // Para teste
                        //            //attachment1.Name = file.Name;
                        //            //attachment1.Name = FormatString("{0}", clienteNome);
                        //            attachment1.Name = FormatString("{0}", "INTEGRACAO TESTE 2 - CONTRATO");

                        //            attachment1.UuidDocMaster = uuidDoc;

                        //            result = d4signProvider.Post(attachment1);

                        //            if (result.Status == 500)
                        //            {
                        //                throw new Exception(result.Message);
                        //            }

                        //            //
                        //            //Contrato America
                        //            //
                        //            if (pontoVenda.ToUpper().Contains("AMERICA"))
                        //            {
                        //                filename = ToString(GetCustomParameter("D4SIGN_REGULAMENTO_INTERNO_PADRAO"));

                        //                result = downloadProvider.Get<DownloadDocument>(filename);

                        //                if (result.Status == 500)
                        //                {
                        //                    throw new Exception(result.Message);
                        //                }

                        //                downloadDocument = (DownloadDocument)result.Detail;

                        //                bytes = downloadDocument.Document;

                        //                if (bytes.Length == 0)
                        //                {
                        //                    throw new FailedException("O arquivo de contrato informado é invalido");
                        //                }

                        //                bytesInBase64 = ToBase64(bytes);

                        //                D4SignDocumentSlave attachment3 = new D4SignDocumentSlave();
                        //                attachment3.Base64BinaryFile = bytesInBase64;
                        //                //attachment1.MimeType = file.MimeType;
                        //                attachment3.MimeType = "application/pdf";
                        //                attachment3.Name = FormatString("{0}", "INTEGRACAO TESTE 2 - CONTRATO");

                        //                attachment3.UuidDocMaster = uuidDoc;

                        //                result = d4signProvider.Post(attachment3);

                        //                if (result.Status == 500)
                        //                {
                        //                    throw new Exception(result.Message);
                        //                }
                        //            }

                        //            //  - Cadastrar os signatarios do documento no d4sign
                        //            //  - POST/documents/{UUID-DOCUMENT}/createlist
                        //            D4SignSigners signers = new D4SignSigners();
                        //            signers.UuidDoc = uuidDoc;

                        //            // Cliente
                        //            D4SignSigner signer1 = new D4SignSigner();

                        //            //
                        //            // Caso exista o email, mandar por email,
                        //            // caso não exista deve ser enviar por telefone
                        //            //

                        //            // Para teste
                        //            clienteEmail = "dev@vsisolucoes.com.br";
                        //            clienteTelefone = "+554391865049";

                        //            if (IsNullOrEmpty(clienteEmail))
                        //            {
                        //                signer1.WhatsappNumber = clienteTelefone;
                        //            }
                        //            else
                        //            {
                        //                signer1.Email = clienteEmail;
                        //            }

                        //            D4SignSigner signer2 = null;

                        //            D4SignSigner signer3 = null;

                        //            D4SignSigner signer4 = null;

                        //            D4SignSigner signer5 = null;

                        //            signers.Signers = new D4SignSigner[] { signer1 };

                        //            if (pontoVenda.ToUpper().Contains("AMERICA"))
                        //            {
                        //                signer2 = new D4SignSigner();
                        //                signer2.Email = "contratosamericapv@gmail.com";

                        //                signers.Signers = new D4SignSigner[] { signer1, signer2 };

                        //            }
                        //            else
                        //            {
                        //                //signer2 = new D4SignSigner();
                        //                //signer2.Email = "eder@pontotrack.com";

                        //                //// 
                        //                //signer3 = new D4SignSigner();
                        //                //signer3.Email = "juridico@pontotrack.com";

                        //                ////
                        //                //signer4 = new D4SignSigner();
                        //                //signer4.Email = "contratospontotrack@gmail.com";

                        //                //// Para teste
                        //                //signer5 = new D4SignSigner();
                        //                //signer5.Email = "dev@vsisolucoes.com.br";

                        //                //signers.Signers = new D4SignSigner[] { signer1, signer2, signer3, signer4, signer5 };

                        //            }
                        //            // 


                        //            //signers.Signers = new D4SignSigner[] { signer1, signer2, signer3, signer4, signer5 };

                        //            result = d4signProvider.Post(signers);

                        //            if (result.Status == 500)
                        //            {
                        //                throw new Exception(result.Message);
                        //            }

                        //            //  - Enviar os documentos para a assinatura
                        //            //  - POST/documents/{UUID-DOCUMENT}/sendtosigner
                        //            D4SignSender sender = new D4SignSender();
                        //            sender.UuidDoc = uuidDoc;
                        //            //sender.Message = "Documentos estão disponíveis para assinatura";
                        //            sender.Message = ToString(GetCustomParameter("D4SIGN_MENSAGEM_PADRAO", ""));
                        //            sender.SkipEmail = 0;
                        //            sender.Workflow = 1;

                        //            result = d4signProvider.Post(sender);

                        //            if (result.Status == 500)
                        //            {
                        //                throw new Exception(result.Message);
                        //            }

                        eventt.Status = "10";
                        eventt.StatusReason = "Processado com sucesso";
                        eventt.StatusDetail = null;
                    }
                    //catch (DiscardedException e)
                    //{
                    //    eventt.Status = "10";
                    //    eventt.StatusReason = "Processado com sucesso, evento descartado";
                    //    eventt.StatusDetail = e.Message;
                    //}
                    //catch (FailedException e)
                    //{
                    //    eventt.Status = "20";
                    //    eventt.StatusReason = "Falha ao processar o evento, verifique e envie novamente";
                    //    eventt.StatusDetail = e.Message;
                    //}
                    catch (Exception e)
                    {
                        //eventt.Status = "00";
                        //eventt.StatusReason = "Falha ao processar o evento";
                        //eventt.StatusDetail = e.Message;
                        HandleEventException(e, eventt);
                    }

                    object ret = Update(eventt);

                    if (ret != null)
                    {
                        throw new Exception(ToString(ret));
                    }
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskMonitorarPedidosSgrAguardandoAssinaturaParaD4Sign finalizado");

            Delay(60);
        }

        /// <summary>
        /// Sempre que ocorrer o cadastro ou alteração de um cliente
        /// no SGR deve-se enviá-lo ao MOVIDESK como CLIENTE
        /// PUBLICADO
        /// </summary>
        private void TaskMonitorarClienteSgrParaMovidesk()
        {
            Log("TaskMonitorarClienteSgrParaMovidesk iniciado...");

            try
            {
                object ret;

                DateTime now = DateTime.Now;

                DateTime lastRun = ToDate(GetCustomParameter("TaskMonitorarClienteSgrParaMovidesk_LastRun", $"{now.ToString("dd/MM/yyyy HH:mm:ss")}"));

                ProviderBase sgrProvider = GetProvider<SgrProvider>();
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>();

                SgrProviderParam sgrProviderParam = new SgrProviderParam();
                sgrProviderParam.ClienteUltimaAtualizacao = lastRun;

                IEnumerable<SgrCliente> clientes = sgrProvider.List<SgrCliente>(sgrProviderParam);

                foreach (SgrCliente cliente in clientes)
                {
                    // Caso o servico de automação não esteja rodando
                    if (!IsRunning)
                    {
                        throw new ValidationException("Automação foi parada");
                    }

                    string nome = cliente.NomeCliente;
                    string hash = ToHash(cliente.CodCliente);

                    Event @event = Find<Event>($"Hash='{hash}'");

                    if (@event == null)
                    {
                        @event = new Event();
                        @event.Number = ToString(cliente.CodCliente);
                        @event.Type = 2;
                        @event.EventText = ToJson(cliente);
                        @event.Hash = hash;
                        @event.Status = Event.Unprocessed;

                        ret = Add(@event);

                        if (ret != null)
                        {
                            throw new GeneralException(ret);
                        }

                        Log(nome);
                    }
                }

                lastRun = now.AddMinutes(-10);

                ret = SetCustomParameter("TaskMonitorarClienteSgrParaMovidesk_LastRun", $"{lastRun.ToString("dd/MM/yyyy HH:mm:ss")}");

                if (ret != null)
                {
                    throw new GeneralException(ret);
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskMonitorarClienteSgrParaMovidesk finalizado");

            Delay(60);
        }

        /// <summary>
        /// PUBLICADO
        /// </summary>
        private void TaskProcessarEventosClienteSgrParaMovidesk()
        {
            Log("TaskProcessarEventosClienteSgrParaMovidesk iniciado...");

            try
            {
                ProviderBase sgrProvider = GetProvider<SgrProvider>();
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>();

                IEnumerable events = List<Event>(
                    $"Type=2 " +
                    $"AND Status='{Event.Unprocessed}'",
                    "CreateDateTime Asc"
                );

                foreach (Event @event in events)
                {
                    try
                    {
                        // Caso o servico de automação não esteja rodando
                        if (!IsRunning)
                        {
                            throw new ValidationException("Automação foi parada");
                        }

                        SgrCliente cliente = ToModel<SgrCliente>(@event.EventText);

                        string nome = cliente.NomeCliente;
                        SgrEmail[] emails = cliente.Emails;
                        SgrTelefone[] phones = cliente.Telefones;
                        string cpfCnpj = StringClear(cliente.CpfCliente);
                        string situacao = cliente.Situacao.Descricao;
                        string cep = cliente.Endereco.Cep;
                        string logradouro = cliente.Endereco.Logradouro;
                        string numero = cliente.Endereco.Numero;
                        string complemento = cliente.Endereco.Complemento;
                        string bairro = cliente.Endereco.Bairro;
                        string cidade = cliente.Endereco.Cidade;
                        string uf = cliente.Endereco.Uf;
                        int tipoPessoa = StringClear(cpfCnpj).Length == 14 ? 2 : 1; // Pessoa Fisica

                        //
                        // Busca no movidesk
                        //
                        MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();
                        moviDeskProviderParam.Filter = $"cpfCnpj eq '{cpfCnpj}'";

                        ProviderResult providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

                        if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                        {
                            throw new GeneralException(providerResult.Message);
                        }

                        MovideskPerson person = (MovideskPerson)providerResult.Detail;

                        bool found = person != null;

                        if (!found)
                        {
                            // Se pessoa fisica é obrigatório colocar um relacionamento tipo empresa
                            string idEmpresa = null;

                            if (tipoPessoa == 1)
                            {
                                person = new MovideskPerson();
                                person.BusinessName = nome;
                                person.PersonType = 2; // 1 - Fisica, 2 - Juridica,
                                person.ProfileType = 2;
                                person.IsActive = true; // !situacao.Equals("CANCELADO") ? true : false;

                                providerResult = movideskProvider.Post(person);

                                if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                                {
                                    throw new GeneralException(providerResult.Message);
                                }

                                person = (MovideskPerson)providerResult.Detail;

                                found = person != null;

                                if (!found)
                                {
                                    throw new GeneralException("Falha ao criar o relacionamento quando é uma pessoa fisica");
                                }

                                idEmpresa = person.Id;
                            }

                            person = new MovideskPerson();
                            person.CpfCnpj = cpfCnpj;
                            person.BusinessName = nome;
                            person.PersonType = tipoPessoa; // 1 - Fisica, 2 - Juridica,
                            person.ProfileType = 2;
                            person.IsActive = !situacao.Equals("CANCELADO") ? true : false;

                            MovideskAddress movideskAddress = new MovideskAddress();
                            movideskAddress.AddressType = "Comercial";
                            movideskAddress.PostalCode = cep;
                            movideskAddress.Street = logradouro;
                            movideskAddress.Number = numero;
                            movideskAddress.Complement = complemento;
                            movideskAddress.District = bairro;
                            movideskAddress.City = cidade;
                            movideskAddress.State = uf;
                            movideskAddress.Country = "BRASIL";
                            movideskAddress.CountryId = "1058";
                            movideskAddress.IsDefault = true;

                            person.Addresses = new MovideskAddress[] { movideskAddress };

                            MovideskPersonRelationship relationships = new MovideskPersonRelationship();
                            relationships.Id = idEmpresa;

                            person.Relationships = new MovideskPersonRelationship[] { relationships };

                            //
                            //
                            //
                            List<MovideskEmail> movideskEmails = new List<MovideskEmail>();

                            ////validar email
                            ////string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                            //Regex regExpEmail = new Regex(@"^[a-zA-Z]+(([\'\,\.\- ][a-zA-Z ])?[a-zA-Z]*)*\s+<(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})>$|^(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})$");
                            ////bool emailValido = true;

                            foreach (SgrEmail email in emails)
                            {
                                //Match match = regExpEmail.Match(email.Email);

                                //if (match.Success)
                                if (IsValidEmail(email.Email))
                                {
                                    MovideskEmail movideskEmail = new MovideskEmail();
                                    movideskEmail.EmailType = "Pessoal";
                                    movideskEmail.Email = email.Email;
                                    //movideskEmail.IsDefault = true;

                                    movideskEmails.Add(movideskEmail);
                                }
                            }

                            person.Emails = movideskEmails.ToArray();

                            //
                            //
                            //
                            List<MovideskPhone> movideskPhones = new List<MovideskPhone>();

                            foreach (SgrTelefone phone in phones)
                            {
                                MovideskPhone movideskPhone = new MovideskPhone();
                                movideskPhone.ContactType = "Celular";
                                movideskPhone.Contact = phone.Contato;
                                //movideskPhone.IsDefault = true;

                                movideskPhones.Add(movideskPhone);
                            }

                            person.Phones = movideskPhones.ToArray();

                            //
                            //
                            //
                            providerResult = movideskProvider.Post(person);

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }
                        }

                        Log(nome);

                        @event.Status = Event.Success;
                        @event.StatusReason = "Processado com sucesso";
                        @event.StatusDetail = null;
                    }
                    catch (Exception e)
                    {
                        HandleEventException(e, @event);
                    }

                    object ret = Update(@event);

                    if (ret != null)
                    {
                        throw new GeneralException(ret);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskProcessarEventosClienteSgrParaMovidesk finalizado");

            Delay(60);
        }

        //  Sempre que ocorrer um agendamento no SGR deve-se enviá-lo ao MOVIDESK como TICKET
        //      - Solicitante = Cliente
        //      - Responsável = Pessoa que abriu o agendamento
        /// <summary>
        /// 
        /// </summary>
        private void TaskMonitorarAgendamentosAbertosSgrParaMovidesk()
        {
            Log("TaskMonitorarAgendamentosAbertosSgrParaMovidesk iniciado...");

            try
            {
                int previewDays = -30;

                string dataInicio = DateTime.Today.AddDays(previewDays).ToString("yyyy-MM-dd");

                //string dataFim = DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd");
                string dataFim = DateTime.Today.ToString("yyyy-MM-dd");

                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");

                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

                string tokenMovidesk = movideskProvider.Token;

                CookieContainer CookieContainer = new CookieContainer();
                CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                    $"cliente={"3569"}&" +
                    $"nome={"moskit"}&" +
                    $"senha={"2SI7WG"}");

                request.Method = "POST";
                request.Accept = "application/json";
                request.CookieContainer = CookieContainer;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
                string json = StreamToString(response.GetResponseStream());

                JObject jObj = JObject.Parse(json);

                string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
                string Authorization = jObj["Headers"]["Authorization"].ToString();

                //dataInicio = "2023-10-16";
                //dataFim = "2023-10-17";

                string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}&data_cadastro_fim={dataFim}&indice=0&total=200";

                //string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}&indice=0&total=200";


                //string contratanteBusca = "BRUNO ESTEVAM";
                //url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}&data_cadastro_fim={dataFim}&contratante_agendamento={contratanteBusca}";

                //url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}&contratante_agendamento={contratanteBusca}";

                //long numeroAgendamento = 40409;
                //string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?cod_agendamento={numeroAgendamento}";

                //url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio=2023-07-13&contratante_agendamento=EVERSON AUGUSTO GARCIA DE SOUZA";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                httpWebRequest.Headers.Add("Authorization", Authorization);
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.CookieContainer = CookieContainer;
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                //string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                string json2 = StreamToString(httpWebResponse.GetResponseStream());

                JObject detail = JObject.Parse(json2);
                JArray data = JArray.Parse(detail["data"].ToString());

                foreach (JObject agendamento in data)
                {
                    string contratanteCliente = ToString(agendamento["contratante"]);

                    string codAgendamento = ToString(agendamento["cod_agendamento"]);

                    string subject = $"{ToString(agendamento["cod_agendamento"])} - {contratanteCliente}";

                    try
                    {
                        ProviderResult result = new ProviderResult();

                        string situacao = ToString(agendamento["situacao"]["descricao"]);

                        DateTime dataInicial = ToDate($"{ToString(agendamento["data_inicial"])} {ToString(agendamento["horario_inicial"])}");
                        DateTime dataFinal = ToDate($"{ToString(agendamento["data_final"])} {ToString(agendamento["horario_final"])}");

                        string dataCriacao = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");

                        //if (dataInicial < DateTime.Now.AddHours(3))
                        //{
                        //    dataCriacao = dataInicial.ToString("yyyy-MM-dd HH:mm:ss");
                        //}

                        string status = "Novo";
                        string baseStatus = "New";
                        string justificativa = null;
                        string dataResolvido = null;

                        if (situacao.ToLower().Equals("cancelado"))
                        {
                            status = "Cancelado";
                            baseStatus = "Canceled";
                        }
                        else if (situacao.ToLower().Equals("parado"))
                        {
                            status = "Aguardando";
                            baseStatus = "Stopped";
                            justificativa = "Retorno do cliente";
                        }
                        else if (situacao.ToLower().Equals("concluído"))
                        {
                            status = "Resolvido";
                            baseStatus = "Resolved";
                            dataResolvido = ToDate(ToString(agendamento["data_conclusao"])).ToString("yyyy-MM-dd HH:mm:ss"); //DateTime.Now.ToString();

                            Log($"Data de conclusao: {ToString(agendamento["data_conclusao"])} - {dataResolvido}");

                            if (ToDate(ToString(agendamento["data_conclusao"])) < ToDate(dataCriacao))
                            //DateTime.Now)
                            {
                                dataResolvido = dataCriacao;// DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");
                                dataCriacao = dataResolvido;
                            }
                        }

                        string observacao = ToString(agendamento["observacao"]);

                        string descricao = ToString(agendamento["descricao"]);

                        string usuario = ToString(agendamento["usuario_original"]["nome"]);

                        string pontoVenda = ToString(agendamento["ponto"]["nome"]);

                        string descServico = ToString(agendamento["servico"]["descricao"]);

                        string servico = descServico;

                        //switch (descServico.ToUpper())
                        //{
                        //    //case "INSTALACAO": servico = "Instalação"; break;
                        //    case "MANUTENÇÃO": servico = "MANUTENÇÃO"; break;
                        //}

                        string dadosVeiculo = $"Veículo: " +
                            $"{ToString(agendamento["modelo"])} -" +
                            $"{ToString(agendamento["placa"])} - " +
                            $"{ToString(agendamento["marca"])} - " +
                            $"{ToString(agendamento["anofab"])} -" +
                            $"{ToString(agendamento["anomod"])} -";

                        string localInstalacao = ToString(agendamento["local_instalacao_modulo"]);

                        var res = "";

                        bool found = false;
                        ProviderResult providerResult = null;


                        //
                        //Alguns clientes estão com erro no retorno do campo data de criação então não da para consultar pelo objeto MovideskPerson
                        //Estou fazendo pelo request
                        //
                        //MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();
                        //moviDeskProviderParam.Filter = $"businessName eq '{contratanteCliente}'";

                        //providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

                        //if (providerResult.Status == 500)
                        //{
                        //    Log($"Contratante não cadastrado {contratanteCliente}");
                        //    throw new Exception(providerResult.Message);
                        //}

                        //MovideskPerson movideskPerson = (MovideskPerson)providerResult.Detail;

                        //found = movideskPerson != null;

                        //if (contratanteCliente.Equals("VALDEMIR RIBEIRO DOS SANTOS"))
                        //{
                        //    string d = null;

                        //}

                        string filter = $"businessName eq '{UrlEncode(contratanteCliente)}'";

                        JObject parans = null;

                        HttpClient client = new HttpClient();
                        HttpRequestMessage
                        httpRequestMessage = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={tokenMovidesk}&$filter={filter}"),
                            Content = new StringContent("", null, "application/json")
                            //null
                        };

                        var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                        var responseInfo = response1.GetAwaiter().GetResult();

                        string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                        if (!responseInfo.IsSuccessStatusCode)
                        {
                            if (responseInfo.StatusCode == HttpStatusCode.BadRequest)
                            {
                                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                            }
                            else if (responseInfo.StatusCode == HttpStatusCode.NotFound)
                            {
                                result1 = null;
                            }
                            else
                            {
                                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                            }
                        }

                        JArray cliente = JArray.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                        //found = !result1.Equals("[]");

                        found = cliente.Count > 0;

                        if (!found)
                        {
                            Log($"Cliente não cadastrado {contratanteCliente}");
                        }

                        //
                        //Se encontrar o cliente cadastrado
                        //
                        if (found)
                        {
                            string idCliente = Substring(result1, "id\":\"", "\"").Trim();

                            //
                            //Buscar agente 
                            //
                            string idUsuario = "df562909-3e5d-4406-"; //df562909-3e5d-4406-

                            string timeUsuario = "Administrativo Suporte";

                            if (pontoVenda.ToUpper().Equals("MARINGA"))
                            {
                                idUsuario = "1844aa69-53d0-4cbf-";

                                timeUsuario = "Atendimento N1 Maringá";
                            }

                            client = new HttpClient();

                            //try
                            //{

                            //
                            //Delay pq estou usando o mesmo endpoint para buscar cliente e usuários
                            //
                            Delay(3);

                            //
                            //profileType = 2 é cadastro de clientes
                            //
                            filter = $"(businessName eq '{usuario}') and (profileType ne 2)";
                            httpRequestMessage = new HttpRequestMessage
                            {
                                Method = HttpMethod.Get,
                                RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={movideskProvider.Token}&$filter={filter}"),
                                Content = new StringContent("", null, "application/json")
                                //null
                            };

                            response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                            responseInfo = response1.GetAwaiter().GetResult();

                            result1 = responseInfo.Content.ReadAsStringAsync().Result;

                            if (!responseInfo.IsSuccessStatusCode)
                            {
                                if (responseInfo.StatusCode == HttpStatusCode.BadRequest)
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                                else if (responseInfo.StatusCode == HttpStatusCode.NotFound)
                                {
                                    result1 = null;
                                }
                                else
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                            }

                            JArray usuarios = JArray.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                            //found = !result1.Equals("[]");

                            found = usuarios.Count > 0;

                            if (found)
                            {
                                idUsuario = Substring(result1, "id\":\"", "\"").Trim();

                                timeUsuario = Substring(result1, "\"teams\":[\"", "\"").Trim();
                            }

                            //}
                            //catch (Exception e)
                            //{
                            //    throw new Exception(e.Message);
                            //    //Log(e);
                            //}

                            //
                            //Consultar o serviço para pegar o id
                            //
                            string idServico = "MANUTENCAO";

                            url = $"https://api.movidesk.com/public/v1/services?token=78113255-a9e4-4824-b651-95f664927d9f&$filter=name eq '{servico}'&$select=name,id";

                            client = new HttpClient();
                            httpRequestMessage = new HttpRequestMessage
                            {
                                Method = HttpMethod.Get,
                                RequestUri = new Uri(url),
                                Content = new StringContent("", null, "application/json")
                                //null
                            };

                            response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                            responseInfo = response1.GetAwaiter().GetResult();

                            result1 = responseInfo.Content.ReadAsStringAsync().Result;

                            if (!responseInfo.IsSuccessStatusCode)
                            {
                                if (responseInfo.StatusCode == HttpStatusCode.BadRequest)
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                                else if (responseInfo.StatusCode == HttpStatusCode.NotFound)
                                {
                                    result1 = null;
                                }
                                else
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                            }

                            JArray resultado1 = JArray.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                            idServico = resultado1.Count > 0 ? ToString(resultado1[0]["id"]) : "MANUTENCAO";

                            //providerResult = GetHttpClient(
                            //  $"https://api.movidesk.com/public/v1/services?token=78113255-a9e4-4824-b651-95f664927d9f&$filter=name eq '{servico}'&$select=name,id",
                            //  null,
                            //  HttpMethod.Get
                            //);

                            //if (providerResult.Status == 500)
                            //{
                            //    throw new Exception(providerResult.Message);
                            //}

                            //res = providerResult.Message.ToString().Replace("[", "").Replace("]", "").Replace("},", "};");

                            //if (!IsNullOrEmpty(res))
                            //{
                            //    string[] serv = res.Split(";");

                            //    foreach (string ret in serv)
                            //    {
                            //        JObject servicos = JObject.Parse(ret);
                            //        idServico = ToString(servicos["id"]);

                            //        break;
                            //    }
                            //}

                            //
                            //Buscar o ticket se já foi cadastrado
                            //
                            filter = $"subject eq '{subject}'";

                            string campos = "id,subject,createdDate";

                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            bool foundTicket = false;
                            //JObject agendament = null;

                            url = $"https://api.movidesk.com/public/v1/tickets?token={tokenMovidesk}&$filter={filter}&$select={campos}";

                            client = new HttpClient();
                            httpRequestMessage = new HttpRequestMessage
                            {
                                Method = HttpMethod.Get,
                                RequestUri = new Uri(url),
                                Content = new StringContent("", null, "application/json")
                                //null
                            };

                            response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                            responseInfo = response1.GetAwaiter().GetResult();

                            result1 = responseInfo.Content.ReadAsStringAsync().Result;

                            //resultado = JObject.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                            if (!responseInfo.IsSuccessStatusCode)
                            {
                                if (responseInfo.StatusCode == HttpStatusCode.BadRequest)
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                                else if (responseInfo.StatusCode == HttpStatusCode.NotFound)
                                {
                                    result1 = null;
                                }
                                else
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                            }

                            JArray ticketMovidesk = JArray.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                            foundTicket = ticketMovidesk.Count > 0;

                            //providerResult = GetHttpClient(
                            //  $"https://api.movidesk.com/public/v1/tickets?token={tokenMovidesk}&$filter={filter}&$select={campos}",
                            //  null,
                            //  HttpMethod.Get
                            //);

                            //if (providerResult.Status == 500)
                            //{
                            //    throw new Exception(providerResult.Message);
                            //}

                            //res = providerResult.Message.ToString().Replace("[", "").Replace("]", "").Replace("},", "};");

                            //if (!IsNullOrEmpty(res))
                            //{
                            //    string[] agend = res.Split(";");

                            //    foreach (string ret in agend)
                            //    {
                            //        agendament = JObject.Parse(ret);
                            //        foundTicket = true;
                            //    }
                            //}

                            if (!foundTicket)
                            {
                                Delay(5);
                                parans = new JObject(
                                           new JProperty("type", "2"),
                                           new JProperty("status", status),
                                           new JProperty("createdDate", dataCriacao), //VERIFICAR A DATA DE CONCLUSÃO
                                           new JProperty("justification", justificativa),
                                           new JProperty("baseStatus", baseStatus),
                                           new JProperty("resolvedIn", dataResolvido),
                                           //new JProperty("closedIn", dataConcluido)
                                           new JProperty("subject", subject),
                                           new JProperty("category", "Solicitação de serviço"),
                                           new JProperty("serviceFirstLevelId", idServico), // 792031
                                           new JProperty("urgency", "Média"),
                                           new JProperty("ownerTeam", timeUsuario),
                                           new JProperty("createdBy", new JObject(new JProperty("id", idUsuario))), //"938628769"

                                           new JProperty("clients", new JArray(new JObject(new JProperty("id", idCliente)))), //"ca943566-44f2-4241-"
                                           new JProperty("actions", new JArray(new JObject(new JProperty("type", 1),
                                                                               new JProperty("description", $"{descricao} \n\n {observacao} \n\n{dadosVeiculo} \n\nPonto de Venda: {pontoVenda} \n\n Data Agendamento Inicio: {dataInicial} \\n\\n Data Agendamento Final: {dataFinal}"))
                                                                               )),
                                           new JProperty("owner", new JObject(new JProperty("id", idUsuario)
                                                                 )));

                                //HttpClient
                                client = new HttpClient();
                                //HttpRequestMessage
                                httpRequestMessage = new HttpRequestMessage
                                {
                                    Method = HttpMethod.Post,
                                    RequestUri = new Uri($"https://api.movidesk.com/public/v1/tickets?token={tokenMovidesk}"),
                                    Content = new StringContent(parans.ToString(), null, "application/json")
                                };
                            }

                            //var 
                            response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                            //var
                            responseInfo = response1.GetAwaiter().GetResult();

                            //string
                            result1 = responseInfo.Content.ReadAsStringAsync().Result;

                            if (!responseInfo.IsSuccessStatusCode)
                            {
                                if (responseInfo.StatusCode == HttpStatusCode.BadRequest)
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                                else if (responseInfo.StatusCode == HttpStatusCode.NotFound)
                                {
                                    result1 = null;
                                }
                                else
                                {
                                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                }
                            }

                            Log($"MonitorarAgendamentoSgrParaMovidesk {subject}");

                        }

                    }
                    catch (Exception e)
                    {
                        Log($"MonitorarAgendamentoSgrParaMovidesk - Falha no envio: {subject}");

                        Log("MonitorarAgendamentoSgrParaMovidesk - " + e.Message);
                    }

                    Delay(3);
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskMonitorarAgendamentosAbertosSgrParaMovidesk finalizado");

            Delay(60);
        }

        //
        // Monitorar todos os agendamento no SGR e ir atualizando o status dos tickets no MOVIDESK 
        // 
        private void TaskMonitorarAgendamentosFechadosSgrParaMovidesk()
        {
            Log("TaskMonitorarAgendamentosFechadosSgrParaMovidesk iniciado...");

            try
            {
                int previewDays = -30;

                string dataInicio = DateTime.Today.AddDays(previewDays).ToString("yyyy-MM-dd");

                string dataFim = DateTime.Today.ToString("yyyy-MM-dd");

                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

                string tokenMovidesk = movideskProvider.Token;

                CookieContainer CookieContainer = new CookieContainer();
                CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                    $"cliente={"3569"}&" +
                    $"nome={"moskit"}&" +
                    $"senha={"2SI7WG"}");

                request.Method = "POST";
                request.Accept = "application/json";
                request.CookieContainer = CookieContainer;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
                string json = StreamToString(response.GetResponseStream());

                JObject jObj = JObject.Parse(json);

                string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
                string Authorization = jObj["Headers"]["Authorization"].ToString();

                //string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}";

                //dataInicio = "2023-09-19";

                string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_conclusao_inicio={dataInicio}&indice=200&total=200";

                //string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_inicial_inicio={dataInicio}";

                //string 
                //url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?cod_agendamento=41245";

                //string contratanteBusca = "NADIR DONIZETE TESTA";
                //url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}&data_cadastro_fim={dataFim}&contratante_agendamento={contratanteBusca}";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                httpWebRequest.Headers.Add("Authorization", Authorization);
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.CookieContainer = CookieContainer;
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                //string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                string json2 = StreamToString(httpWebResponse.GetResponseStream());

                JObject detail = JObject.Parse(json2);
                JArray data = JArray.Parse(detail["data"].ToString());

                foreach (JObject agendamento in data)
                {
                    try
                    {
                        string situacao = ToString(agendamento["situacao"]["descricao"]);

                        string contratanteCliente = ToString(agendamento["contratante"]);

                        string codAgendamento = ToString(agendamento["cod_agendamento"]);

                        //string subject = $"{codAgendamento} - {contratanteCliente}(TESTE VSI)";

                        string subject = $"{ToString(agendamento["cod_agendamento"])} - {contratanteCliente}";

                        //
                        //Situação do ticket
                        //
                        //New, InAttendance,Stopped,Canceled,Resolved,Closed
                        string status = null;
                        string justificativa = null;
                        string baseStatus = "New";
                        string dataResolvido = null;
                        string dataConcluido = null;

                        if (situacao.ToLower().Equals("aberto"))
                        {
                            continue;
                        }

                        //switch (situacao.ToLower())
                        //{
                        //    case "cancelado": status = "Cancelado", baseStatus = "Canceled"; break;
                        //}
                        if (situacao.ToLower().Equals("cancelado"))
                        {
                            status = "Cancelado";
                            baseStatus = "Canceled";
                        }
                        else if (situacao.ToLower().Equals("parado"))
                        {
                            status = "Aguardando";
                            baseStatus = "Stopped";
                            justificativa = "Retorno do cliente";
                        }
                        else if (situacao.ToLower().Equals("concluído"))
                        {
                            //status = "Fechado";
                            //baseStatus = "Closed";
                            ////justificativa = "Retorno do cliente";
                            //dataConcluido = ToDate(ToString(agendamento["data_conclusao"]) + " 00:00:00").ToString(); //DateTime.Now.ToString();

                            status = "Resolvido";
                            baseStatus = "Resolved";
                            //justificativa = "Retorno do cliente";
                            //dataConcluido = ToDate(ToString(agendamento["data_conclusao"]) + " 00:00:00").ToString(); //DateTime.Now.ToString();

                            dataResolvido = ToDate(ToString(agendamento["data_conclusao"])).ToString("yyyy-MM-dd HH:mm:ss"); //DateTime.Now.ToString();

                        }
                        else
                        {
                            //Resolvido - Resolved
                            //Em atendimento - InAttendance
                            //
                            //No SGR - Aguardando agendamento, Agendado

                            Log($"Verificar: {subject} -  {situacao}");
                            continue;
                        }

                        //
                        //Buscar o ticket se já foi cadastrado
                        //                       
                        string filter = $"subject eq '{UrlEncode(subject)}'";

                        string campos = "id,subject,createdDate,status,baseStatus,justification,origin,createdDate,resolvedIn,closedIn";

                        bool foundTicket = false;

                        url = $"https://api.movidesk.com/public/v1/tickets?token={tokenMovidesk}&$filter={filter}&$select={campos}";

                        HttpClient client = new HttpClient();
                        HttpRequestMessage httpRequestMessage = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(url),
                            Content = new StringContent("", null, "application/json")
                            //null
                        };

                        var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                        var responseInfo = response1.GetAwaiter().GetResult();

                        string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                        //resultado = JObject.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                        if (!responseInfo.IsSuccessStatusCode)
                        {
                            if (responseInfo.StatusCode == HttpStatusCode.BadRequest)
                            {
                                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                            }
                            else if (responseInfo.StatusCode == HttpStatusCode.NotFound)
                            {
                                result1 = null;
                            }
                            else
                            {
                                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                            }
                        }

                        JArray ticketMovidesk = JArray.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                        foundTicket = ticketMovidesk.Count > 0;

                        if (foundTicket)
                        {
                            string statusNoMovidesk = ToString(ticketMovidesk[0]["baseStatus"]).ToLower();

                            //if ((statusNoMovidesk.ToLower().Equals("resolved") || statusNoMovidesk.ToLower().Equals("closed")) && !status.ToLower().Equals("aberto"))
                            //{
                            //    continue;
                            //}

                            //if (statusNoMovidesk.ToLower().Equals("canceled") && status.ToLower().Equals("cancelado"))
                            //{
                            //    continue;
                            //}

                            if (statusNoMovidesk.ToLower().Equals("stopped") && status.ToLower().Equals("aberto"))
                            {
                                continue;
                            }

                            if (!statusNoMovidesk.Equals(baseStatus.ToLower()))
                            {
                                JObject parans = new JObject(
                                      new JProperty("status", status),
                                      new JProperty("justification", justificativa),
                                      new JProperty("baseStatus", baseStatus),
                                      new JProperty("resolvedIn", dataResolvido),
                                      new JProperty("closedIn", dataConcluido)

                                      );

                                client = new HttpClient();

                                url = $"https://api.movidesk.com/public/v1/tickets?id={ToString(ticketMovidesk[0]["id"])}&token={tokenMovidesk}";

                                httpRequestMessage = new HttpRequestMessage
                                {
                                    Method = HttpMethod.Patch,
                                    RequestUri = new Uri(url),
                                    Content = new StringContent(parans.ToString(), null, "application/json")
                                };

                                response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                                responseInfo = response1.GetAwaiter().GetResult();

                                result1 = responseInfo.Content.ReadAsStringAsync().Result;

                                if (!responseInfo.IsSuccessStatusCode)
                                {
                                    if (responseInfo.StatusCode == HttpStatusCode.BadRequest)
                                    {
                                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                    }
                                    else if (responseInfo.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        result1 = null;
                                    }
                                    else
                                    {
                                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                                    }
                                }

                                Log($"{subject} - Status movidesk: {statusNoMovidesk} - Status SGR: {status} -  Situação SGR: {situacao}");
                            }
                            else
                            {
                                Log($"{subject} Situação: {situacao} Data conclusão: {dataResolvido} Situação sgr: {situacao}");
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Log("MonitorarAgendamentosFechadosSgrParaMovidesk" + e.Message);
                    }

                    Delay(30);
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskMonitorarAgendamentosFechadosSgrParaMovidesk finalizado");

            Delay(60);
        }
    }
}