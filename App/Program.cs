using System.Text;
using VSIntegra.Framework.Core;
using VSIntegra.Framework.Provider;
using VSIntegra.Framework.Model.Crm;
using VSIntegra.Framework.Model.System;
using VSIntegra.Framework.Provider.Moskit;
using VSIntegra.Framework.Provider.Moskit.Model;
using VSIntegra.Framework.Provider.Sga;
using VSIntegra.Framework.Provider.Sgr;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using VSIntegra.Framework.Provider.Sgr.Model;
using System.Reflection;
using VSIntegra.Framework.Provider.D4Sign.Model;
using VSIntegra.Framework.Provider.Download.Model;
using VSIntegra.Framework.Provider.D4Sign;
using VSIntegra.Framework.Provider.Download;
using System.Net;

namespace Sync.Custom
{
    class Program : TaskBase
    {
        //
        // Para debug
        // 
        private static void Main()
        {
            // https://stackoverflow.com/questions/32471058/windows-1252-is-not-supported-encoding-name
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (IsAppRunning())
            {
                Environment.Exit(0);
            }

            string INTEGRATION_ID = "4e70ceb8-0a0a-45a5-b48e-af880108fab0";

            Program worker = new Program();
            worker.IntegrationId = INTEGRATION_ID;
            worker.IsRunning = true;
            worker.DebugMode = true;

            WorkerService workerService = new WorkerService(worker);
            workerService.Execute();

            Console.WriteLine($"{GetAssemblyName()} inicializado");
            Console.WriteLine($"{GetAssemblyName()} pressione qualquer tecla para fechar");

            Console.ReadLine();
        }

        //
        // Monitora os usuarios do Moskit e atualiza o vsintegra
        //
        // ROTINA MIGRADA PARA O VSINTEGRA WEB V6, SE ALTERAR AQUI DEVE ALTERAR LÁ
        //PUBLICADO
        private void TaskMonitoraUsuarioMoskit()
        {
            //Log("TaskMonitoraUsuarioMoskit iniciado...");

            //try
            //{
            //    Provider moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");

            //    IEnumerable<MoskitUser> users = moskitProvider.List<MoskitUser>();

            //    foreach (MoskitUser userMoskit in users)
            //    {
            //        if (!IsRunning)
            //        {
            //            break;
            //        }

            //        ProviderResult providerResult = Find<User>(
            //            FormatString("Number LIKE '%{0}%'", userMoskit.Id)
            //        );

            //        if (providerResult.Status == 500)
            //        {
            //            throw new Exception(providerResult.Message);
            //        }

            //        User userVSIntegra = (User)providerResult.Detail;

            //        bool found = userVSIntegra != null;

            //        if (!found)
            //        {
            //            userVSIntegra = new User();

            //            //userVSIntegra.Store = Store;
            //            //userVSIntegra.Code = GetNumerator(Store, "sysaj");
            //            userVSIntegra.Number = ToString(userMoskit.Id) + ";;";
            //            userVSIntegra.RouletteJoin = true;
            //            userVSIntegra.RouletteUsed = false;
            //            userVSIntegra.Type = "US";
            //            userVSIntegra.Password = "123";
            //        }

            //        userVSIntegra.Name = userMoskit.Name;
            //        userVSIntegra.Email = userMoskit.Username;
            //        userVSIntegra.Username = userMoskit.Username;
            //        userVSIntegra.TeamId = ToString(userMoskit.Team.Id);
            //        userVSIntegra.Inactive = !userMoskit.Active;

            //        providerResult = !found ? Add(userVSIntegra) : Update(userVSIntegra);

            //        if (providerResult.Status == 500)
            //        {
            //            throw new Exception(providerResult.Message);
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log(e);
            //}

            //Log("TaskMonitoraUsuarioMoskit finalizado");
        }

        //
        // Monitora os negocios do Moskit e cria o pedido no SGR
        // PUBLICADO
        private void TaskCriaPedidoErpSgrSgaParaLeadsGanhos()
        {
            Log("TaskCriaPedidoErpSgrSgaParaLeadsGanhos iniciado...");

            try
            {
                Provider moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");
                Provider sgaProvider = GetProvider<SgaProvider>("334998e8-7ca5-44db-8270-10d0c5c3f834");
                Provider sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");

                bool found = false;
                ProviderResult providerResult = null;
                MoskitCustomField[] entityCustomFields = null;
                MoskitCustomField customField = null;

                //IEnumerable events = List<Event>("Type=0 AND Status='00' AND SYSAT_ID = '642dc96998f77'", "CreateDateTime ASC");

                IEnumerable events = List<Event>("Type=0 AND SYSAT_ID = '642dc96998f77'", "CreateDateTime ASC");

                foreach (Event ev in events)
                {
                    try
                    {
                        JObject evento = JObject.Parse(ToString(ev.EventText));

                        JObject data = JObject.Parse(ToString(evento["data"]));

                        string status = FormatStringToLower(ToString(data["status"]));
                        long dealId = ToInt(data["id"]); //1496620 160
                        string responsibleId = ToString(data["responsible"]["id"]);

                        string nomeNegocio = ToString(data["name"]);

                        if (!status.Equals("won"))
                        {
                            throw new DiscardedException(FormatString("Negocio: {0} {1} com status: {2}", dealId, nomeNegocio, status));
                        }

                        // buscar o negocio
                        providerResult = moskitProvider.Find<MoskitDeal>(dealId);

                        if (providerResult.Status == 500)
                        {
                            throw new Exception(providerResult.Message);
                        }

                        MoskitDeal deal = (MoskitDeal)providerResult.Detail;

                        decimal price = deal.Price;

                        if (price > 0)
                        {
                            price = ToDecimal(price.ToString().Substring(0, price.ToString().Length - 2) + "," + price.ToString().Substring(price.ToString().Length - 2));
                        }

                        entityCustomFields = deal.CustomFields;

                        //string numeroPedido = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY")?.TextValue;

                        string numeroPedido = null;

                        if (MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY") != null)
                        {
                            //numeroPedido = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY").TextValue;

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

                        if (deal.Contacts != null)
                        {
                            providerResult = moskitProvider.Find<MoskitContact>(deal.Contacts[0].Id);

                            if (providerResult.Status == 500)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            MoskitContact contact = (MoskitContact)providerResult.Detail;

                            if (contact != null)
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
                                    providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_6rRmwGS9i6jZpm4X/options/{0}", options[0]));

                                    if (providerResult.Status == 500)
                                    {
                                        throw new Exception(providerResult.Message);
                                    }

                                    customField = (MoskitCustomField)providerResult.Detail;

                                    estado = ToString(customField.Label);
                                }
                            }
                        }

                        int codigoVendedorSgr = 0;
                        int codigoVendedorSga = 0;

                        providerResult = Find<User>(FormatString("Number LIKE '%{0}%'", responsibleId));

                        if (providerResult.Status == 500)
                        {
                            throw new Exception(providerResult.Message);
                        }

                        User userVSIntegra = (User)providerResult.Detail;

                        found = userVSIntegra != null;

                        if (!found)
                        {
                            throw new Exception("Usuário não encontrado");
                        }

                        string[] c = userVSIntegra.Number.Split(';');

                        int cMoskit = c.Length > 0 ? ToInt(c[0]) : 0;
                        codigoVendedorSgr = c.Length > 1 ? ToInt(c[1]) : 0;
                        codigoVendedorSga = c.Length > 2 ? ToInt(c[2]) : 0;

                        if (codigoVendedorSgr == 0) // 89475 - Pontotrack
                        {
                            throw new Exception("Vendedor não encontrado");
                        }

                        //
                        // Consultar produto, se produto estiver preenchido associar ao campo Tipo rastreados no sgr
                        //
                        string codTipoRastreador = "325";
                        string tipoAssociadoRastreador = "";

                        //options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6")?.Options;
                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_wPVm2VijibjB8mK6/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                                providerResult = moskitProvider.Find<MoskitProduct>(deal.DealProducts[0].Product.Id);

                                MoskitProduct product = (MoskitProduct)providerResult.Detail;

                                bool foundProduct = product != null;

                                if (!foundProduct)
                                {
                                    if (DateTime.Now.AddDays(-1) < ToDate(deal.CloseDate))
                                    {
                                        throw new DiscardedException(FormatString("Discartador por não informar o produto {0}", deal.Name));
                                    }

                                    throw new Exception(FormatString("Produto não informado {0}", deal.Name));
                                }

                                nomeProduto = ToString(product.Name);

                                if (deal.DealProducts[0].FinalPrice > 0 && price <= 0)
                                {
                                    decimal valorVenda = deal.DealProducts[0].FinalPrice;

                                    if (valorVenda > 0)
                                    {
                                        price = ToDecimal(deal.DealProducts[0].FinalPrice.ToString().Substring(0, valorVenda.ToString().Length - 2) + "," + valorVenda.ToString().Substring(valorVenda.ToString().Length - 2));
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
                        if (nomeProduto.ToUpper().Equals("RASTREAMENTO") || tipoAssociadoRastreador.ToUpper().Equals("CLIENTE/RASTREADOR"))
                        {
                            tipoProduto = 1;
                        }
                        else if (nomeProduto.ToUpper().Contains("PLANO") || tipoAssociadoRastreador.ToUpper().Equals("ASSOCIADO"))
                        {
                            tipoProduto = 2;
                            tipoProduto = 3;
                        }
                        else if (nomeProduto.ToUpper().Contains("RASTREAMENTO + ASSISTENCIA"))
                        {
                            tipoProduto = 3;
                        }

                        if (tipoProduto == 0)
                        {
                            if (DateTime.Now.AddDays(-1) > ToDate(deal.CloseDate))
                            {
                                throw new DiscardedException(FormatString("Discartado por não informar o produto {0}", deal.Name));
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

                        cpfCnpj = cpfCnpj != null ? ClearString(Regex.Replace(cpfCnpj, "[^0-9,]", "")) : null;

                        rg = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkinColgEmvL") != null ?
                             MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkinColgEmvL").TextValue : null;

                        string dataNascimento = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA") != null ?
                            FormatDate(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA").DateValue, "yyyy-MM-dd") : null;

                        string profissao = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdJQlDO3") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdJQlDO3").TextValue : null;

                        string emails = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5") != null ?
                                        MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5").TextValue : null;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeidireglqQe") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeidireglqQe").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_Pj3qYeidireglqQe/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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

                        cep = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_A4wMWNigC68OBqB8") != null ?
                              MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_A4wMWNigC68OBqB8").TextValue : null;

                        if (cep != null)
                        {
                            cep = ClearString(cep.Replace(",", ""));
                            cep = Regex.Replace(ClearString(cep), "[^0-9,]", "").Length == 8 ? Regex.Replace(cep, "[^0-9,]", "") : null;
                        }

                        numero = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_6rRmweivC6rQ5q4X") != null ?
                                 MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_6rRmweivC6rQ5q4X").TextValue : null;
                        if (numero != null)
                        {
                            numero = ClearString(numero.Replace(",", ""));
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_y5lm56iyiY4L8DwW/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_nrLDXoiWikl02mOa/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_oJZmP1i9iGoX5Dgv/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkieio21BmvL") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkieio21BmvL").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_2wpDlkieio21BmvL/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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

                            price = grupoMensalidadePeriodo.Contains("ANUAL 588,00|588,00") ? 588 :
                                    grupoMensalidadePeriodo.Contains("ANUAL 650,00|650,00") ? 650 :
                                    grupoMensalidadePeriodo.Contains("ANUAL 660,00|660,00") ? 660 :
                                    grupoMensalidadePeriodo.Contains("ANUAL 708,00|708,00") ? 708 :
                                    grupoMensalidadePeriodo.Contains("ANUAL 720,00|720,00") ? 720 :
                                    grupoMensalidadePeriodo.Contains("ANUAL 730,00|730,00") ? 730 :
                                    grupoMensalidadePeriodo.Contains("ANUAL 750,00|750,00") ? 750 :
                                    grupoMensalidadePeriodo.Contains("ANUAL 780,00|780,00") ? 780 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE R$ 149,00|149,00") ? 149 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 49,00|49,00") ? 49 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 50,00|50,00") ? 50 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 55,00|55,00") ? 55 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 59,00|59,00") ? 59 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 59,90 (PRIMEIRA 39,00)|59,90") ? ToDecimal("59.90") :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 60,00|60,00") ? 60 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 64,90|64,90") ? ToDecimal("64,90") :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 65,00|65,00") ? ToDecimal("65") :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 69,00|69,00") ? 69 :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 69,90 (PRIMEIRA 39,90)|69,90") ? ToDecimal("69.90") :
                                    grupoMensalidadePeriodo.Contains("MENSALIDADE 79,00|79,00") ? 79 :
                                    grupoMensalidadePeriodo.Contains("RASTREADOR COM ASSISTENCIA 24 HORAS|94,90") ? ToDecimal("94.90") :
                                    price;

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

                        //CONDIÇÃO DE PAGAMENTO
                        int codCondicaoPagamentoVenda = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_E79Mr2iLipLlQMZJ") != null ?
                            MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_E79Mr2iLipLlQMZJ").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_E79Mr2iLipLlQMZJ/options/{0}", options[0]));

                            if (providerResult.Status == 500)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string condicaoPagamento = ToString(customField.Label);

                            codCondicaoPagamentoVenda = condicaoPagamento.Contains("BOLETO BANCARIO") ? 642 :
                                                        condicaoPagamento.Contains("CARTAO DE CREDITO") ? 645 :
                                                        condicaoPagamento.Contains("CARTÃO DÉBITO") ? 793 :
                                                        condicaoPagamento.Contains("DINHEIRO") ? 347 :
                                                        condicaoPagamento.Contains("TRANSFERÊNCIA VIA PIX") ? 794 :
                                                        condicaoPagamento.Contains("ZERO") ? 762 : 0;
                        }

                        int codVencimentoVenda = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wGrqzpi3id0Y0mLo") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wGrqzpi3id0Y0mLo").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_wGrqzpi3id0Y0mLo/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_dVKmQ5ibidnZVmWR/options/{0}", options[0]));

                            if (providerResult.Status == 500)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string condicaoPagamento = ToString(customField.Label);

                            codCondicaoPagamentoAdesao = condicaoPagamento.Contains("BOLETO BANCARIO") ? 642 :
                                                        condicaoPagamento.Contains("CARTAO DE CREDITO") ? 645 :
                                                        condicaoPagamento.Contains("CARTÃO DÉBITO") ? 793 :
                                                        condicaoPagamento.Contains("DINHEIRO") ? 347 :
                                                        condicaoPagamento.Contains("TRANSFERÊNCIA VIA PIX") ? 794 :
                                                        condicaoPagamento.Contains("ZERO") ? 762 : 0;
                        }

                        //
                        //Grupo de adesão
                        //
                        int codGrupoAdesao = 0;

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_A4wMWNiLi69NQqB8") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_A4wMWNiLi69NQqB8").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_A4wMWNiLi69NQqB8/options/{0}", options[0]));

                            if (providerResult.Status == 500)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            string adesao = ToString(customField.Label);

                            codGrupoAdesao = adesao.Contains("SEM TAXA") ? 1043 :
                                                 adesao.Contains("INSTALACAO") ? 1002 : 0;
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_49RM16ixiB7nbmBW/options/{0}", options[0]));

                            if (providerResult.Status == 500)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            customField = (MoskitCustomField)providerResult.Detail;

                            providerResult = Find<Source>(FormatString("CRMAD_DESCRIPTION='{0}'", customField.Label));

                            if (providerResult.Status == 500)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            Source source = (Source)providerResult.Detail;

                            if (source != null)
                            {
                                codIndicacao = ToInt(source.Number);
                            }
                        }

                        string codPontoVenda = "";

                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_QJXmA5iXiJEBpm25") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_QJXmA5iXiJEBpm25").Options : null;

                        if (options != null)
                        {
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_QJXmA5iXiJEBpm25/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_gvGm3BiaizpJ0M45/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_wGrqzpi3ido00mLo/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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
                            providerResult = moskitProvider.Find<MoskitCustomField>(FormatString("CF_oJZmP1i9iGwywDgv/options/{0}", options[0]));

                            if (providerResult.Status == 500)
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

                        SgrProviderParam sgrProviderParam = new SgrProviderParam();
                        sgrProviderParam.VeiculoModeloDescricao = modeloVeiculo;

                        List<SgrVeiculoModelo> modelos = sgrProvider.List<SgrVeiculoModelo>(sgrProviderParam).ToList();

                        int codModeloVeiculo = 0;
                        int codMontadoraVeiculo = 0;

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

                        //
                        // Gravo cliente no SGA
                        //
                        //if (cpfCnpj != null)
                        //{
                        //    cpfCnpj = cpfCnpj.Length == 14 ? cpfCnpj : cpfCnpj.Length == 11 ? cpfCnpj : "";
                        //}

                        //
                        // Geração do pedido no SGR
                        //
                        SgrPedido pedido = new SgrPedido();

                        //1) PONTO DE VENDA, 
                        pedido.CodPontoVendaVenda = ToInt(codPontoVenda);

                        //2) VENDEDOR, 
                        pedido.CodConsultorVenda = codigoVendedorSgr; // 1; // Pega da amarracao MOSKIT x SGR

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

                        if (!IsNullOrEmpty(telefone2))
                        {
                            SgrTelefone telefoneVenda = new SgrTelefone();
                            telefoneVenda.Contato = telefone2;
                            telefoneVenda.Tipo = "FIXO";
                            telefoneVenda.CodDepartamento = "779"; //Sem informação
                            telefones.Add(telefoneVenda);
                        }

                        if (!IsNullOrEmpty(telefone3))
                        {
                            SgrTelefone telefoneVenda = new SgrTelefone();
                            telefoneVenda.Contato = telefone3;
                            telefoneVenda.Tipo = "FIXO";
                            telefoneVenda.CodDepartamento = "779"; //Sem informação
                            telefones.Add(telefoneVenda);
                        }

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
                        pedido.DataNascimentoClienteVenda = dataNascimento;

                        //4) PRODUTO                        
                        pedido.ProdutoVendaCodProdutos = new int[] { tipoProduto };

                        if (tipoProduto2 > 0)
                        {
                            pedido.ProdutoVendaCodProdutos = new int[] { tipoProduto, tipoProduto2 };
                        }

                        pedido.ValorParcelaVenda = price;
                        pedido.ValorParcelaVenda = price;
                        pedido.FipeValorVeiculoVenda = price;
                        pedido.QuantidadeParcelaVenda = quantidadeParcelaVenda;
                        pedido.QuantidadeParcelaAdesaoVenda = 1;
                        pedido.AnoModVeiculoVenda = anoModelo;
                        pedido.AnoFabVeiculoVenda = anoFabricacao;
                        pedido.PlacaVeiculoVenda = placa;
                        pedido.ChassiVeiculoVenda = chassi;
                        pedido.RenavamVeiculoVenda = ClearString(renavam);
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

                        providerResult = sgrProvider.Add(pedido);

                        if (providerResult.Status == 500)
                        {
                            throw new Exception(providerResult.Message);
                        }

                        ev.StatusDetail = providerResult.Message;

                        //
                        //Pedido da venda
                        //
                        SgrPedido pedidoRetorno = (SgrPedido)providerResult.Detail;

                        pedido.CodVenda = pedidoRetorno.CodVenda;

                        Log(pedido.CodVenda);

                        //
                        // Atualizar o numero do pedido do sgr no moskit
                        //
                        deal.CustomFields = new MoskitCustomField[] { new MoskitCustomField("CF_dN7MGPiGCOLOrmeY", ToString(pedidoRetorno.CodVenda)) };

                        providerResult = moskitProvider.Update(deal);

                        if (providerResult.Status == 500)
                        {
                            //throw new Exception(providerResult.Message); // se der throw vai interferir na rotina.
                            ev.StatusDetail = providerResult.Message;
                        }

                        //
                        // Gero um evento do pedido
                        //
                        Event eventBO = new Event();
                        eventBO.Type = 2;
                        eventBO.CreateDateTime = DateTime.Now;
                        eventBO.EventText = ToJson(pedido);
                        eventBO.Hash = ToHash(pedido);
                        eventBO.Status = "00";
                        eventBO.StatusReason = "Aguardando processamento";
                        eventBO.StatusDetail = null;

                        providerResult = Add(eventBO);

                        if (providerResult.Status == 500)
                        {
                            throw new Exception(providerResult.Message);
                        }

                        ev.Status = "10";
                        ev.StatusReason = "Processado com sucesso";
                    }
                    catch (DiscardedException e)
                    {
                        ev.Status = "10";
                        ev.StatusReason = "Processado com sucesso, evento descartado";
                        ev.StatusDetail = e.Message;
                    }
                    catch (FailedException e)
                    {
                        ev.Status = "20";
                        ev.StatusReason = "Falha ao processar o evento, verifique e envie novamente";
                        ev.StatusDetail = e.Message;
                    }
                    catch (Exception e)
                    {
                        ev.Status = "00";
                        ev.StatusReason = "Falha ao processar o evento";
                        ev.StatusDetail = e.Message;
                    }

                    providerResult = Update(ev);

                    if (providerResult.Status == 500)
                    {
                        throw new Exception(providerResult.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("TaskCriaPedidoErpSgrSgaParaLeadsGanhos finalizado...");
        }

        //Monitorar os pedidos do SGR e sempre que o vendedor alterar o situacao(status) para AGUARDANDO ASSINATURA deve-se gerar o pedido e o contrato em PDF, e enviá-lo para o D4SIGN
        //  - Fazer o upload do pedido de venda(principal) e o contrato(anexo) no d4sign
        //  - POST /documents/{UUID-SAFE}/upload
        //  - POST/documents/{UUID-DOC-PRINCIPAL}/ uploadslave
        //  - Cadastrar os signatarios do documento no d4sign
        //  - POST/documents/{UUID-DOCUMENT}/ createlist
        //  - Enviar os documentos para a assinatura
        //  - POST/documents/{UUID-DOCUMENT}/ sendtosigner
        public void MonitorarPedidosSgrAguardandoAssinaturaParaD4Sign()
        {
            Log("MonitorarPedidosSgrAguardandoAssinaturaParaD4Sign iniciado...");

            try
            {
                ProviderResult result = null;

                Provider sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                Provider d4signProvider = GetProvider<D4SignProvider>("464998e8-7ca5-44db-8270-10d0c5c3f849");
                DownloadProvider downloadProvider = new DownloadProvider();

                IEnumerable events = List<Event>("" +
                    "Type=2 AND " +
                    "Status='00'", 
                    "CreateDateTime ASC"
                );

                foreach (Event eventt in events)
                {
                    try
                    {
                        JObject data = ToJSonObject(eventt.EventText);
                        int codVenda = ToInt(data["codVenda"]);
                        string emailContato = ToString(data["emailContato"]);
                        string phoneContato = "+55" + ToString(data["phoneContato"]);
                        string nomeCliente = "valter batista maciel";

                        //
                        //  - Fazer o upload do pedido de venda(principal) e o contrato(anexo) no d4sign
                        //  - POST /documents/{UUID-SAFE}/upload
                        //  - POST/documents/{UUID-DOC-PRINCIPAL}/uploadslave
                        //

                        //
                        // Principal: Pedido
                        //

                        //
                        // Criar o arquivo pdf aqui
                        //
                        Stream template = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sync.Custom.LabelTemplate.html");

                        var args = new string[,] {
                            {"{CLIENTE}", ""},
                            {"{POLIMEC_COD}", ""},
                            {"{CLIENTE_COD}", ""}
                        };

                        //var pdf = new PdfGenerator(new WkHtmlToPdfBinaryCustom());
                        ////var bytes = await pdf.Generate(html);
                        //byte[] bytes = Task.Run(async () => await pdf.Generate(html)).Result;
                        byte[] bytes = ToPDF(template, args);

                        var uuid = Guid.NewGuid();

                        string filename = @$"tmp\{uuid}.pdf";

                        var assemblyPath = Assembly.GetExecutingAssembly().Location;

                        string startUpDirectory = new FileInfo(assemblyPath).Directory.FullName;

                        string fullFilename = Path.Combine(startUpDirectory, "frontend\\build", filename);

                        System.IO.File.WriteAllBytes(fullFilename, bytes);


                        //string filename = ToString(GetCustomParameter("PEDIDO_TESTE"));

                        //result = downloadProvider.Find<DownloadDocument>(filename);

                        //if (result.Status == 500)
                        //{
                        //    throw new Exception(result.Message);
                        //}

                        //DownloadDocument downloadDocument = (DownloadDocument)result.Detail;

                        //byte[] bytes = downloadDocument.Document;

                        //if (bytes.Length == 0)
                        //{
                        //    throw new FailedException("O arquivo de pedido informado é invalido");
                        //}

                        string bytesInBase64 = ToBase64(bytes);

                        D4SignDocument d4SignDocument = new D4SignDocument();
                        d4SignDocument.Base64BinaryFile = bytesInBase64;
                        //d4SignDocument.MimeType = file.MimeType;
                        d4SignDocument.MimeType = "application/pdf";
                        d4SignDocument.Name = FormatString("{0}", nomeCliente);
                        //d4SignDocument.UuidSafe = "0145b22e-0c2e-4ce0-b2c8-9182897f7c78";
                        d4SignDocument.UuidSafe = ToString(GetCustomParameter("D4SIGN_COFRE"));

                        result = d4signProvider.Add(d4SignDocument);

                        if (result.Status == 500)
                        {
                            throw new Exception(result.Message);
                        }

                        d4SignDocument = (D4SignDocument)result.Detail;

                        string uuidDoc = d4SignDocument.UuidDoc;

                        //
                        // Principal: Anexo 1
                        //
                        //string
                        filename = ToString(GetCustomParameter("D4SIGN_CONTRATO_PADRAO"));

                        result = downloadProvider.Find<DownloadDocument>(filename);

                        if (result.Status == 500)
                        {
                            throw new Exception(result.Message);
                        }

                        DownloadDocument downloadDocument = (DownloadDocument)result.Detail;

                        bytes = downloadDocument.Document;

                        if (bytes.Length == 0)
                        {
                            throw new FailedException("O arquivo de contrato informado é invalido");
                        }

                        bytesInBase64 = ToBase64(bytes);

                        D4SignDocumentSlave attachment1 = new D4SignDocumentSlave();
                        attachment1.Base64BinaryFile = bytesInBase64;
                        //attachment1.MimeType = file.MimeType;
                        attachment1.MimeType = "application/pdf";
                        //attachment1.Name = file.Name;
                        attachment1.Name = FormatString("{0}", nomeCliente);
                        attachment1.UuidDocMaster = uuidDoc;

                        result = d4signProvider.Add(attachment1);

                        if (result.Status == 500)
                        {
                            throw new Exception(result.Message);
                        }

                        //  - Cadastrar os signatarios do documento no d4sign
                        //  - POST/documents/{UUID-DOCUMENT}/createlist

                        D4SignSigner signer = new D4SignSigner();

                        //
                        // Caso exista o email, mandar por email, caso não exista deve ser enviar por telefone
                        //
                        if (IsNullOrEmpty(emailContato))
                        {
                            signer.WhatsappNumber = phoneContato;
                        }
                        else
                        {
                            signer.Email = emailContato;
                        }

                        D4SignSigners signers = new D4SignSigners();
                        signers.UuidDoc = uuidDoc;
                        signers.Signers = new D4SignSigner[] { signer };

                        result = d4signProvider.Add(signers);

                        if (result.Status == 500)
                        {
                            throw new Exception(result.Message);
                        }

                        //  - Enviar os documentos para a assinatura
                        //  - POST/documents/{UUID-DOCUMENT}/sendtosigner
                        D4SignSender sender = new D4SignSender();
                        sender.UuidDoc = uuidDoc;
                        //sender.Message = "Documentos estão disponíveis para assinatura";
                        sender.Message = ToString(GetCustomParameter("D4SIGN_MENSAGEM_PADRAO", ""));
                        sender.SkipEmail = "0";
                        sender.Workflow = "0";
                        sender.TokenAPI = null;

                        result = d4signProvider.Add(sender);

                        if (result.Status == 500)
                        {
                            throw new Exception(result.Message);
                        }

                        eventt.Status = "10";
                        eventt.StatusReason = "Processado com sucesso";
                    }
                    catch (DiscardedException e)
                    {
                        eventt.Status = "10";
                        eventt.StatusReason = "Processado com sucesso, evento descartado";
                        eventt.StatusDetail = e.Message;
                    }
                    catch (FailedException e)
                    {
                        eventt.Status = "20";
                        eventt.StatusReason = "Falha ao processar o evento, verifique e envie novamente";
                        eventt.StatusDetail = e.Message;
                    }
                    catch (Exception e)
                    {
                        eventt.Status = "00";
                        eventt.StatusReason = "Falha ao processar o evento";
                        eventt.StatusDetail = e.Message;
                    }

                    result = Update(eventt);

                    if (result.Status == 500)
                    {
                        throw new Exception(result.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("MonitorarPedidosSgrAguardandoAssinaturaParaD4Sign finalizado");
        }

        private JObject BuscaMensalidade(string descricaoModelo)
        {
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

            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            JObject ret = JObject.Parse(json);

            string XAuthToken = ret["Headers"]["X-Auth-Token"].ToString();
            string Authorization = ret["Headers"]["Authorization"].ToString();

            request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_grupo_mensalidade/688472668f481b3efbddb0bfbff99cf6?descricao={descricaoModelo}");
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("X-Access-Token", XAuthToken);
            request.Headers.Add("Authorization", Authorization);
            request.KeepAlive = true;
            //request.ServicePoint.ConnectionLimit = 10000;
            //request.ContentType = "multipart/form-data";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = CookieContainer;

            response = (HttpWebResponse)request.GetResponse();

            json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            ret = JObject.Parse(json);

            return ret;

            //return null;
        }
    }
}