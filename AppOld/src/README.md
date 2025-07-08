public void TaskCriaPedidoErpSgrSgaParaLeadsGanhos()
        {
            Log("TaskCriaPedidoErpSgrSgaParaLeadsGanhos iniciado...");

            try
            {
                MoskitProvider moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");

                ProviderResult providerResult = new ProviderResult();

                //SgrProvider sgrProvider = GetProvider<SgaProvider>("");

                //SgaProvider sgaProvider = GetProvider<SgrProvider>("");

                string TOKEN_SGR = "688472668f481b3efbddb0bfbff99cf6";
                string API_KEY_SGR = "3569";
                string USERNAME_SGR = "moskit";
                string PASSWORD_SGR = "2SI7WG";

                IEnumerable events = List<Event>("Type=0 AND Status='00' DateTime ASC");

                foreach (Event ev in events)
                {
                    //if (!IsRunning)
                    //{
                    //    return;
                    //}

                    try
                    {
                        JObject evento = JObject.Parse(ToString(ev.EventText));

                        JObject data = JObject.Parse(ToString(evento["data"]));

                        string statusDeal = ToString(data["status"]);
                        long dealId = ParseInt(data["id"]); //1496620 160
                        string responsibleId = StringUtilities.ToString(data["responsible"]["id"]);

                        if (!statusDeal.ToLower().Equals("won"))
                        {
                            throw new DiscardedException($"Negocio com status {statusDeal}");
                        }

                        long contatoId = 0;

                        long[] options = null;

                        //if (data["contacts"] != null)
                        //{
                        //    JArray contatos = (JArray)data["contacts"];

                        //    contatoId = contatos.Count > 0 ? NumberUtilities.parseLong(data["contacts"][0]["id"]) : 0;
                        //}

                        //providerResult = moskitProvider.Get<ContactMoskit>(contatoId);

                        #region // Busca dados do contato

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

                        CustomField[] entityCustomFields = null;

                        CustomField customField = new CustomField();

                        #endregion

                        #region Busco negocio e cadastro no SGA e SGR

                        providerResult = moskitProvider.Get<Deal>(dealId);

                        if (providerResult.Status == 200)
                        {
                            Deal deal = (Deal)providerResult.Detail;

                            bool foundDeal = deal != null;

                            //ContactMoskit contact = (ContactMoskit)deal.Contacts;

                            if (deal.Contacts != null)
                            {
                                providerResult = moskitProvider.Get<ContactMoskit>(deal.Contacts[0].Id);

                                if (providerResult.Status == 200)
                                {
                                    Contact contact = (Contact)providerResult.Detail;

                                    if (contact != null)
                                    {
                                        entityCustomFields = contact.CustomFields;

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

                                        cpfCnpj = CustomField.GetCustomFieldById(entityCustomFields, "CF_0WGqoGSKC9zK2qnP")?.TextValue;
                                        rg = CustomField.GetCustomFieldById(entityCustomFields, "CF_6rRmwGSvC6jZKm4X")?.TextValue;
                                        cidade = CustomField.GetCustomFieldById(entityCustomFields, "CF_42AmakSZCwrAPqjl")?.TextValue;
                                        bairro = CustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2oS2Cbj9gDK6")?.TextValue;
                                        cep = CustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYoSeCrBpamQe")?.TextValue;
                                        numero = CustomField.GetCustomFieldById(entityCustomFields, "CF_Rg7Mn4SLCA1XrDvd")?.TextValue;
                                        logradouro = CustomField.GetCustomFieldById(entityCustomFields, "CF_ylAm0KS6C5p91Mvb")?.TextValue;
                                        estado = "";

                                        //long[] 
                                        options = CustomField.GetCustomFieldById(entityCustomFields, "CF_6rRmwGS9i6jZpm4X")?.Options;

                                        if (options != null)
                                        {
                                            providerResult = moskitProvider.Get<CustomField>($"CF_6rRmwGS9i6jZpm4X/options/{options[0]}");

                                            if (providerResult.Status == 500)
                                            {
                                                throw new FailedEventException(providerResult.Message);
                                            }

                                            customField = (CustomField)providerResult.Detail;

                                            estado = StringUtilities.ToString(customField.Label);
                                        }
                                    }
                                }
                            }


                            int codigoVendedorSgr = 0;
                            int codigoVendedorSga = 0;

                            UserVSIntegra usuarioBO = BusinessObjectManager.FindByFilter<UserVSIntegra>($"SYSAJ_COMPANY_ID='{Store}' AND SYSAJ_NUMBER LIKE '%{responsibleId}%'");

                            if (usuarioBO != null)
                            {
                                //JObject codigosIntegracoes = JObject.Parse(usuarioBO.CodeIntegrations);

                                JObject codigosIntegracoes = JObject.Parse(usuarioBO.Number);

                                codigoVendedorSgr = codigosIntegracoes.ToString().Contains("SGR") ? ToInt(codigosIntegracoes["SGR"].ToString()) : 0; //usuarioBO.Code;
                                codigoVendedorSga = codigosIntegracoes.ToString().Contains("SGA") ? ToInt(codigosIntegracoes["SGA"].ToString()) : 0;//ToInt(usuarioBO.CodeIntegrations);
                            }

                            if (codigoVendedorSgr == 0)
                            {
                                throw new Exception("Vendedor não encontrado");
                            }

                            #region //Consultar produto, se produto estiver preenchido associar ao campo Tipo rastreados no sgr

                            string codTipoRastreador = "325";

                            string tipoAssociadoRastreador = "";

                            //long[] 
                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6")?.Options;

                            if (options != null)
                            {
                                providerResult = moskitProvider.Get<CustomField>($"CF_wPVm2VijibjB8mK6/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                codTipoRastreador = StringUtilities.ToString(customField.Label);

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
                                    providerResult = moskitProvider.Get<Product>(deal.DealProducts[0].Product.Id);

                                    Product product = (Product)providerResult.Detail;

                                    bool foundProduct = product != null;

                                    if (!foundProduct)
                                    {
                                        throw new DiscardedEventException($"Produto não informado {deal.Name}");
                                    }

                                    nomeProduto = StringUtilities.ToString(product.Name);
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

                            // Quando o produto do negócio for Plano Light, Plano Plus, Plano Prime o produto no SGR é PROTEÇÃO VEICULAR
                            if (nomeProduto.ToUpper().Equals("RASTREAMENTO") || tipoAssociadoRastreador.ToUpper().Equals("CLIENTE/RASTREADOR"))
                            {
                                tipoProduto = 1;
                            }
                            else if (nomeProduto.ToUpper().Contains("PLANO") || tipoAssociadoRastreador.ToUpper().Equals("ASSOCIADO"))
                            {
                                tipoProduto = 2;
                            }

                            if (tipoProduto == 0)
                            {
                                throw new DiscardedEventException($"Produto não informado {deal.Name}");
                            }
                            #endregion


                            #region Busca campos personalizados

                            nome = string.IsNullOrWhiteSpace(nome) ? deal.Name : nome;

                            entityCustomFields = deal.CustomFields;

                            string email = CustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5")?.TextValue;

                            email = string.IsNullOrWhiteSpace(email) ? emailContato : email;

                            string telefone = CustomField.GetCustomFieldById(entityCustomFields, "CF_075MJ2izS6yRkMaz")?.TextValue;

                            telefone = string.IsNullOrWhiteSpace(telefone) ? telefoneContato : telefone;

                            cpfCnpj = CustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR")?.TextValue;

                            cpfCnpj = cpfCnpj != null ? Regex.Replace(cpfCnpj, "[^0-9,]", "") : null;

                            if (string.IsNullOrWhiteSpace(cpfCnpj))
                            {
                                //throw new Exception($"Cnpj não cadastrado para o negócio: {deal.Name}");
                            }

                            cpfCnpj = StringUtilities.Clear(cpfCnpj, "");

                            string numeroPedido = CustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY")?.TextValue;

                            if (numeroPedido == null)
                            {
                                numeroPedido = "";
                            }

                            if (!string.IsNullOrWhiteSpace(numeroPedido))
                            {
                                throw new DiscardedEventException($"Pedido já cadastrado para o negócio: {deal.Name} - {numero}");
                            }

                            rg = CustomField.GetCustomFieldById(entityCustomFields, "CF_2wpDlkinColgEmvL")?.TextValue;

                            //email = CustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5")?.TextValue;

                            //DateTime dataNascimentoDate = T(CustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA")?.DateValue, "dd/MM/yyyy");

                            string dataNascimento = DateUtilities.format(CustomField.GetCustomFieldById(entityCustomFields, "CF_GwyMgWi0U7gW9MLA")?.DateValue, "yyyy-MM-dd");

                            string profissao = CustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdJQlDO3")?.TextValue;

                            string emails = CustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5")?.TextValue;

                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeidireglqQe")?.Options;

                            if (options != null)
                            {
                                providerResult = moskitProvider.Get<CustomField>($"CF_Pj3qYeidireglqQe/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                estado = StringUtilities.ToString(customField.Label);
                            }

                            cidade = CustomField.GetCustomFieldById(entityCustomFields, "CF_Pj3qYeieCrG2EqQe")?.TextValue;

                            if (!string.IsNullOrWhiteSpace(cidade))
                            {
                                cidade = cidade.Contains(",") ? cidade.Replace(cidade.Substring(cidade.IndexOf(",")), "").Trim() : cidade;
                            }

                            bairro = CustomField.GetCustomFieldById(entityCustomFields, "CF_POEMywieCJrxADdk")?.TextValue;
                            cep = CustomField.GetCustomFieldById(entityCustomFields, "CF_A4wMWNigC68OBqB8")?.TextValue;
                            numero = CustomField.GetCustomFieldById(entityCustomFields, "CF_6rRmweivC6rQ5q4X")?.TextValue;
                            logradouro = CustomField.GetCustomFieldById(entityCustomFields, "CF_3nGqEoirCl8lkmYA")?.TextValue;

                            string observacoes = CustomField.GetCustomFieldById(entityCustomFields, "CF_AE5mpEijCdk0QDO3")?.TextValue;


                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_y5lm56iyiY4L8DwW")?.Options;

                            if (options != null)
                            {
                                //long option = options[0];
                                //JObject optionS = (JObject)SendCommand2($"https://api.moskitcrm.com/v2/customFields/{"CF_y5lm56iyiY4L8DwW"}/options/{option}", "GET", null,
                                //    //ToString(GetCustomParameter("PARAM_0001"))
                                //    "8a3f08a1-d459-4201-9fea-cff64d7696ca"
                                //);

                                //tipoAssociadoRastreador = StringUtilities.ToString(optionS["label"]);

                                providerResult = moskitProvider.Get<CustomField>($"CF_y5lm56iyiY4L8DwW/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                tipoAssociadoRastreador = StringUtilities.ToString(customField.Label);
                            }

                            string sexo = "";

                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_nrLDXoiWikl02mOa")?.Options;

                            if (options != null)
                            {

                                //long option = options[0];

                                //JObject optionS = (JObject)SendCommand2($"https://api.moskitcrm.com/v2/customFields/{"CF_nrLDXoiWikl02mOa"}/options/{option}", "GET", null,
                                //    //ToString(GetCustomParameter("PARAM_0001"))
                                //    "8a3f08a1-d459-4201-9fea-cff64d7696ca"
                                //);

                                //sexo = StringUtilities.ToString(optionS["label"]);

                                providerResult = moskitProvider.Get<CustomField>($"CF_nrLDXoiWikl02mOa/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                sexo = StringUtilities.ToString(customField.Label);

                                sexo = sexo.Equals("Feminino") ? "F" :
                                                sexo.Equals("Masculino") ? "M" : "";

                            }

                            string estadoCivil = "";

                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_nrLDXoiWikl02mOa")?.Options;

                            if (options != null)
                            {
                                //long option = options[0];

                                //// Busco options
                                //JObject optionS = (JObject)SendCommand2($"https://api.moskitcrm.com/v2/customFields/{"CF_nrLDXoiWikl02mOa"}/options/{option}", "GET", null,
                                //    //ToString(GetCustomParameter("PARAM_0001"))
                                //    "8a3f08a1-d459-4201-9fea-cff64d7696ca"
                                //);

                                //estadoCivil = StringUtilities.ToString(optionS["label"]);

                                providerResult = moskitProvider.Get<CustomField>($"CF_nrLDXoiWikl02mOa/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                estadoCivil = StringUtilities.ToString(customField.Label);

                                estadoCivil = estadoCivil.Equals("Casado(a)") ? "CA" :
                                                estadoCivil.Equals("Solteio(a)") ? "SO " :
                                                estadoCivil.Equals("Viuvo(a)") ? "VI" :
                                                estadoCivil.Equals("Divorciado(a)") ? "DI" :
                                                estadoCivil.Equals("Separado(a)") ? "SE" :
                                                estadoCivil.Equals("União estavel") ? "CO" : "SO";
                            }

                            int codIndicacao = 1;

                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_49RM16ixiB7nbmBW")?.Options;

                            if (options != null)
                            {
                                //long option = options[0];

                                //JObject optionS = (JObject)SendCommand2($"https://api.moskitcrm.com/v2/customFields/{"CF_49RM16ixiB7nbmBW"}/options/{option}", "GET", null,
                                //    //ToString(GetCustomParameter("PARAM_0001"))
                                //    "8a3f08a1-d459-4201-9fea-cff64d7696ca"
                                //);

                                //JObject indicacao = BuscaIndicacaoVenda(StringUtilities.ToString(optionS["label"]));

                                providerResult = moskitProvider.Get<CustomField>($"CF_49RM16ixiB7nbmBW/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                IndicacaoOrigemBO indicacaoOrigemBO = BusinessObjectManager.FindByFilter<IndicacaoOrigemBO>($"USUAK_NUMERO='{NumberUtilities.parseInt(customField.Id)}'");

                                if (indicacaoOrigemBO != null)
                                {
                                    codIndicacao = indicacaoOrigemBO.USUAK_CODIGO_SGR;
                                }
                            }

                            string codPontoVenda = "";

                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_QJXmA5iXiJEBpm25")?.Options;

                            if (options != null)
                            {
                                //long option = options[0];

                                //JObject optionS = (JObject)SendCommand2($"https://api.moskitcrm.com/v2/customFields/{"CF_QJXmA5iXiJEBpm25"}/options/{option}", "GET", null,
                                //    //ToString(GetCustomParameter("PARAM_0001"))
                                //    "8a3f08a1-d459-4201-9fea-cff64d7696ca"
                                //);

                                //codPontoVenda = StringUtilities.ToString(optionS["label"]);

                                providerResult = moskitProvider.Get<CustomField>($"CF_QJXmA5iXiJEBpm25/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                codPontoVenda = StringUtilities.ToString(customField.Label);

                                codPontoVenda = codPontoVenda.Equals("PONTO TRACK") ? "1" :
                                                codPontoVenda.Equals("AMERICA CLUBE DE BENEFICIOS") ? "2" :
                                                codPontoVenda.Equals("MARINGA") ? "3" :
                                                codPontoVenda.Equals("ASSOCIACAO MUTUALISTA VIA SUL") ? "4" :
                                                codPontoVenda.Equals("CURITIBA") ? "5" :
                                                codPontoVenda.Equals("LONDRINA") ? "7" : "";
                            }

                            if (string.IsNullOrWhiteSpace(codPontoVenda))
                            {
                                if (deal.LastInteraction < DateTime.Now.AddDays(-1))
                                {
                                    throw new DiscardedEventException($"Ponto de venda não cadastrado: {deal.Name}");
                                }

                                throw new Exception($"Ponto de venda não cadastrado: {deal.Name}");
                            }


                            #region Dados do veículo cada veículo deve-se gerar um pedido no sgr

                            string modeloVeiculo = "";

                            string valorNegocio = StringUtilities.ToString(deal.Price);

                            string renavam = CustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CN0jam6a")?.TextValue;

                            string veiculo1 = CustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2Vi2CbQLOmK6")?.TextValue;

                            string placa = CustomField.GetCustomFieldById(entityCustomFields, "CF_oJZmP1iKCG7oLDgv")?.TextValue;

                            string anoFabricacao = StringUtilities.ToString(CustomField.GetCustomFieldById(entityCustomFields, "CF_0WGqoEioS90o3mnP")?.NumericValue);

                            string anoModelo = StringUtilities.ToString(CustomField.GetCustomFieldById(entityCustomFields, "CF_3nGqEoiPSl8l5mYA")?.NumericValue);

                            string chassi = CustomField.GetCustomFieldById(entityCustomFields, "CF_VrKMbQiaCO72lqZY")?.TextValue;

                            string tamanhoFrota = StringUtilities.ToString(CustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNw8vm6a")?.TextValue);

                            string empresa = StringUtilities.ToString(CustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1Cd93kmWR")?.TextValue);

                            int corVeiculoCod = 640;

                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_gvGm3BiaizpJ0M45")?.Options;

                            if (options != null)
                            {
                                //    long option = options[0];

                                //    // Busco options
                                //    JObject optionS = (JObject)SendCommand2($"https://api.moskitcrm.com/v2/customFields/{"CF_gvGm3BiaizpJ0M45"}/options/{option}", "GET", null,
                                //        //ToString(GetCustomParameter("PARAM_0001"))
                                //        "8a3f08a1-d459-4201-9fea-cff64d7696ca"
                                //    );

                                //    string corVeiculo = StringUtilities.ToString(optionS["label"]);

                                providerResult = moskitProvider.Get<CustomField>($"CF_gvGm3BiaizpJ0M45/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                string corVeiculo = StringUtilities.ToString(customField.Label);

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

                            options = CustomField.GetCustomFieldById(entityCustomFields, "CF_wGrqzpi3ido00mLo")?.Options;

                            if (options != null)
                            {
                                //long option = options[0];

                                //// Busco options
                                //JObject optionS = (JObject)SendCommand2($"https://api.moskitcrm.com/v2/customFields/{"CF_wGrqzpi3ido00mLo"}/options/{option}", "GET", null,
                                //    //ToString(GetCustomParameter("PARAM_0001"))
                                //    "8a3f08a1-d459-4201-9fea-cff64d7696ca"
                                //);

                                //combustivelVeiculo = StringUtilities.ToString(optionS["label"]);

                                providerResult = moskitProvider.Get<CustomField>($"CF_wGrqzpi3ido00mLo/options/{options[0]}");

                                if (providerResult.Status == 500)
                                {
                                    throw new FailedEventException(providerResult.Message);
                                }

                                customField = (CustomField)providerResult.Detail;

                                combustivelVeiculo = StringUtilities.ToString(customField.Label);

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
                            //Modelo do veículo, montar uma lista de veículos porque cada veículo deve-se gerar um pedido.
                            //
                            modeloVeiculo = veiculo1;

                            //modeloVeiculo = "HILUX";

                            //modeloVeiculo = "PALIO";

                            JObject modeloVeiculo1 = BuscaModeloVeiculo(modeloVeiculo);

                            int codModeloVeiculo = 0;
                            int codMontadoraVeiculo = 0;
                            int tipoVeiculo = 0;

                            if (modeloVeiculo != null)
                            {
                                JArray modelos = (JArray)modeloVeiculo1["data"];
                                if (modelos.Count > 0)
                                {
                                    codModeloVeiculo = NumberUtilities.parseInt(modeloVeiculo1["data"][0]["cod_veiculo_modelo"]);

                                    codMontadoraVeiculo = NumberUtilities.parseInt(modeloVeiculo1["data"][0]["cod_veiculo_montadora"]);

                                    tipoVeiculo = NumberUtilities.parseInt(modeloVeiculo1["data"][0]["cod_tipo_veiculo"]);

                                    if (tipoVeiculo == 0 && modelos.Count > 1)
                                    {
                                        tipoVeiculo = NumberUtilities.parseInt(modeloVeiculo1["data"][1]["cod_tipo_veiculo"]);
                                    }

                                    if (tipoVeiculo == 0 && modelos.Count > 2)
                                    {
                                        tipoVeiculo = NumberUtilities.parseInt(modeloVeiculo1["data"][2]["cod_tipo_veiculo"]);
                                    }
                                }
                            }

                            #endregion

                            #endregion


                            #region Gravo cliente no SGA

                            cpfCnpj = cpfCnpj.Length == 14 ? cpfCnpj :
                                         cpfCnpj.Length == 11 ? cpfCnpj : "";

                            ServiceReturn serviceReturn = new ServiceReturn();

                            if (tipoProduto == 2 && !string.IsNullOrWhiteSpace(cpfCnpj))
                            {
                                SgaClient sgaClient = new SgaClient();
                                //sgaClient.Token = "4dbdba0a7dd7958f783f772fcf1fbe533dbd714616259bf1bdb374166e6a5eae5b8c201310d1a518375e36505f7a6ca8a94aa8612057df43999fd43a026ab578bff2a0d06854af12b0d88a2b334ee82ec7e153938eb1bb6a4672cdeab78f5a473c49e3741e2dd4e8df3e9bf03f4eaee920c82289bdcf030bf728a7e09c979e1f48fac0d305e594257eaacd8b5c1ac4a6";

                                sgaClient.Token = ToString(GetCustomParameter("TOKEN_SGA", ""));

                                //sgaClient.Token = ToString(GetCustomParameter("PARAM_0004"));

                                AssociadoService associadoService = new AssociadoService(sgaClient);

                                //ServiceReturn 
                                serviceReturn = associadoService.GetByCpf(cpfCnpj); //35170458886

                                if (serviceReturn.Status == 500)
                                {
                                    if (serviceReturn.Message.Contains("Acesso n\\u00e3o autorizado. Verique seu token de acesso"))
                                    {
                                        //continue;

                                        //
                                        // Gerar novo token que está expirado
                                        //
                                        string tokenSgr = GerarTokenSga();

                                        providerResult = SetCustomParameter("TOKEN_SGA", tokenSgr);

                                        if (providerResult.Status == 500)
                                        {
                                            throw new Exception(providerResult.Message);
                                        }

                                        return;

                                    }

                                    if (!serviceReturn.Message.Contains("Associado n\\u00e3o encontrado ou est\\u00e1 em alguma situa\\u00e7\\u00e3o indispon\\u00edvel para consulta."))
                                    {
                                        throw new Exception(serviceReturn.Message);
                                    }
                                }

                                Associado associado = (Associado)serviceReturn.Detail;

                                bool found = associado != null;

                                if (!found)
                                {
                                    associado = new Associado();
                                    associado.Nome = nome;
                                    associado.Sexo = string.IsNullOrWhiteSpace(sexo) ? null : sexo;
                                    associado.Cpf = cpfCnpj;
                                    associado.Rg = rg == null ? "00.000.000-0" : rg;
                                    associado.DataNascimento = dataNascimento;
                                    associado.Telefone = telefone;
                                    associado.Celular = celular;
                                    associado.Email = string.IsNullOrWhiteSpace(emails) ? email : emails;
                                    associado.Logradouro = logradouro;
                                    associado.Numero = numero;
                                    //associado.Complemento = complemento;
                                    associado.Bairro = bairro;
                                    associado.Cidade = cidade;
                                    associado.Estado = estado;
                                    associado.Cep = cep == null ? "00000-000" : cep;
                                    associado.DiaVencimento = 10;
                                    associado.CodigoVoluntario = codigoVendedorSga == 0 ? 1 : codigoVendedorSga; // 1;
                                    associado.Rg = "00.000.000-0";

                                    serviceReturn = associadoService.Post(associado);

                                    if (serviceReturn.Status == 500)
                                    {
                                        //throw new Exception(serviceReturn.Message);
                                        Log(serviceReturn.Message);
                                    }
                                }
                            }
                            #endregion

                            SgrClient sgrClient = new SgrClient();
                            sgrClient.Token = TOKEN_SGR;
                            sgrClient.ApiKey = API_KEY_SGR;
                            sgrClient.Username = USERNAME_SGR;
                            sgrClient.Password = PASSWORD_SGR;

                            long price = NumberUtilities.parseLong(deal.Price);

                            decimal valorNegocio1 = 0;

                            if (price > 0)
                            {
                                valorNegocio1 = NumberUtilities.parseDecimal(price.ToString().Substring(0, price.ToString().Length - 2) + "," + price.ToString().Substring(price.ToString().Length - 2));
                            }

                            PedidoService pedidoService = new PedidoService(sgrClient);

                            Pedido pedido = new Pedido();

                            //1) PONTO DE VENDA, 
                            pedido.CodPontoVendaVenda = NumberUtilities.parseInt(codPontoVenda);

                            //2) VENDEDOR, 
                            pedido.CodConsultorVenda = codigoVendedorSgr; // 1; //Pega da amarracao MOSKIT x SGR

                            if (!string.IsNullOrWhiteSpace(cep) || !string.IsNullOrWhiteSpace(numero))
                            {
                                Endereco endereco = new Endereco();
                                endereco.Cep = cep;
                                endereco.Logradouro = logradouro;
                                endereco.Numero = numero;
                                endereco.Bairro = bairro;
                                endereco.Cidade = cidade;
                                endereco.Estado = estado;
                                endereco.Complemento = "";

                                pedido.EnderecoVenda = endereco;
                            }

                            if (!string.IsNullOrEmpty(telefone))
                            {
                                VSIntegra.Integration.Provider.Sgr.Model.Telefone telefoneVenda = new VSIntegra.Integration.Provider.Sgr.Model.Telefone();
                                telefoneVenda.Contato = telefone;
                                telefoneVenda.Tipo = "FIXO";
                                telefoneVenda.CodDepartamento = "779"; //Sem informação

                                pedido.TelefoneVenda = telefoneVenda;
                            }

                            if (!string.IsNullOrEmpty(email))
                            {
                                VSIntegra.Integration.Provider.Sgr.Model.Email emailVenda = new VSIntegra.Integration.Provider.Sgr.Model.Email();
                                emailVenda.Contato = email;
                                emailVenda.CodDepartamento = "779"; //Sem informação

                                pedido.EmailVenda = emailVenda;
                            }

                            pedido.NomeClienteVenda = nome;
                            pedido.CpfClienteVenda = cpfCnpj;
                            pedido.RgClienteVenda = rg == null ? "00.000.000-0" : rg;
                            pedido.SexoClienteVenda = sexo;
                            pedido.EstadoCivilClienteVenda = estadoCivil;
                            pedido.ProfissaoClienteVenda = profissao == null ? "" : profissao;
                            pedido.DataNascimentoClienteVenda = dataNascimento;

                            //4) PRODUTO, 
                            pedido.ProdutoVendaCodProduto = tipoProduto; //1;
                            pedido.ValorParcelaAdesaoVenda = valorNegocio1;
                            pedido.ValorParcelaVenda = valorNegocio1;
                            pedido.FipeValorVeiculoVenda = valorNegocio1;
                            pedido.QuantidadeParcelaVenda = 1;
                            pedido.QuantidadeParcelaAdesaoVenda = 1;

                            anoFabricacao = anoFabricacao.Length == 4 ? anoFabricacao : "";
                            //8) DADOS DO VEICULO: MODELO/ANO FABRICAÇÃO E ANO MODELO / MONTADORA / PLACA / CHASSI, 
                            pedido.AnoModVeiculoVenda = anoModelo.Equals("0") ? "" : anoModelo;
                            pedido.AnoFabVeiculoVenda = anoFabricacao.Equals("0") ? "" : anoFabricacao;
                            pedido.PlacaVeiculoVenda = placa;
                            pedido.ChassiVeiculoVenda = chassi;
                            pedido.RenavamVeiculoVenda = renavam;
                            pedido.CodCombustivelVeiculoVenda = NumberUtilities.parseInt(combustivelVeiculo);

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

                            //9) INDICAÇÃO, 
                            pedido.CodIndicacaoVenda = codIndicacao; // 1; // codIndicacao; // 1; // codIndicacao;

                            //10) TIPO DE RASTREADOR
                            pedido.CodTipoRastreadorVenda = NumberUtilities.parseInt(codTipoRastreador); //1;

                            serviceReturn = pedidoService.Post(pedido);

                            if (serviceReturn.Status == 500)
                            {

                                throw new FailedEventException(serviceReturn.Detail.ToString());
                            }

                            ev.StatusDetail = serviceReturn.Message;

                            JObject oEvent = JObject.Parse(serviceReturn.Message);

                            string codigoSgr = StringUtilities.ToString(oEvent["cod_venda"]);

                            //
                            // Atualizar o numero do pedido do sgr no moskit
                            //
                            providerResult = moskitProvider.Get<Deal>(dealId);

                            if (providerResult.Status == 200)
                            {
                                deal = (Deal)providerResult.Detail;

                                entityCustomFields = deal.CustomFields;

                                //string
                                numeroPedido = CustomField.GetCustomFieldById(entityCustomFields, "CF_dN7MGPiGCOLOrmeY")?.TextValue;

                                bool found1 = deal != null;

                                if (String.IsNullOrWhiteSpace(numeroPedido))
                                {
                                    deal.CustomFields = new CustomField[] {
                                                           new CustomField("CF_dN7MGPiGCOLOrmeY", codigoSgr) };


                                    providerResult = moskitProvider.Put<Deal>(deal);
                                }
                            }

                            if (providerResult.Status == 500)
                            {
                                //ev.StatusDetail = oEvent.ToString();
                                throw new FailedEventException(providerResult.Message);
                            }

                        }

                        #endregion
                    }
                    catch (FailedEventException ex1)
                    {
                        ev.Status = "20";
                        ev.StatusReason = ex1.Message;

                        if (ex1.ToString().Contains("Gateway Time-out") || ex1.ToString().Contains("Too Many Requests") || ex1.ToString().Contains("O campo cod_consultor_venda informado"))
                        {
                            ev.Status = "00";
                            ev.StatusReason = ex1.Message;
                        }

                    }
                    catch (DiscardedEventException ex2)
                    {
                        ev.Status = "30";
                        ev.StatusReason = ex2.Message;

                    }
                    catch (Exception ex3)
                    {
                        ev.Status = "00";

                        ev.StatusReason = ex3.Message;
                    }

                    providerResult = Add(ev);

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



















    namespace Entities
    {
        class IndicacaoOrigemBO : IndicacaoOrigemVO
        {

        }

        [Table("6385f344dc11d_usuak")]
        class IndicacaoOrigemVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String USUAK_ID { get; set; }

            [Field]
            public int USUAK_FILIAL { get; set; }

            [Field]
            public int USUAK_CODIGO { get; set; }

            [Field]
            public int USUAK_NUMERO { get; set; }

            [Field]
            public string USUAK_DESCRICAO { get; set; }

            [Field]
            public int USUAK_CODIGO_SGR { get; set; }

            [Field]
            public bool USUAK_INATIVO { get; set; }
        }

    }

























                                //
                                //Cadastro de pedidos
                                //
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
                                request.Headers.Add("X-Auth-Token", "$2y$11$N.zj6OpWG5Iqw8J7dt73TOTYrw..zTffmmSPyH6D8nNAIgCjoVZ3y");
                                request.Headers.Add("Authorization", "708183befb7e49cad4f8f025933f33897d52cc23");
                                request.Method = "POST";
                                request.ContentType = "multipart/form-data";
                                request.Accept = "application/json";


                                request.CookieContainer = cookieContainer;


                                string postData =
                                    $"cliente={3569}&" +
                                    $"cod_combustivel_veiculo_venda={209}&" +
                                    $"cod_forma_pagamento_venda={201}&" +
                                    $"cod_forma_pagamento_adesao_venda={201}&" +
                                    $"cod_grupo_venda={1}&" +
                                    $"cod_grupo_adesao_venda={1002}" +
                                    $"cod_periodo_venda={336}&" +
                                    $"cod_vencimento_venda={281}&" +
                                    $"cod_cliente_venda={1}&" +
                                    $"cpf_cliente_venda={cpfCnpj}&" +
                                    $"rg_cliente_venda={rg}&" +
                                    $"placa_veiculo_venda={placa}&" +
                                    $"placa_veiculo_venda={placa}&" +
                                    $"anofab_veiculo_venda={anofabVeiculoVenda}&" +
                                    $"anomod_veiculo_venda={anoModeloVeiculoVenda}&" +
                                    $"chassi_veiculo_venda={chassiVeiculoVenda}&" +
                                    $"fipe_valor_veiculo_venda={fipeValorVeiculoVenda}&" +
                                    $"renavam_veiculo_vend={renavamVeiculoVenda}&" +
                                    $"data_nascimento_cliente_venda={dataNascimento}" +
                                    $"contato_cliente_venda={contatoClienteVenda}" +
                                    $"profissao_cliente_venda={profissaoClienteVenda}" +
                                    $"interveniente_venda={""}" +
                                    $"quantidade_parcela_venda={quantidadeParcelaVenda}" +
                                    $"quantidade_parcela_adesao_venda={quantidadeParcelaVenda}" +
                                    $"valor_parcela_venda={valorParcelaVenda / quantidadeParcelaVenda}" +
                                    $"valor_parcela_adesao_venda={valorParcelaVenda / quantidadeParcelaVenda}" +
                                    $"entrada_venda={1}" +
                                    $"observacao_venda={obs}" +
                                    $"sexo_cliente_venda={"M"}" +
                                    $"estado_civil_cliente_venda={"SO"}" +
                                    $"nome_cliente_venda={nome}" +
                                    $"produto_venda[cod_produto][0]={1}" +
                                    $"cod_ponto_venda_venda={1}" +
                                    $"cod_departamento={242}" +
                                    $"cod_forma_pagamento_venda={201}";

                                var data = Encoding.ASCII.GetBytes(postData);

                                request.ContentLength = data.Length;

                                using (var stream = request.GetRequestStream())
                                {
                                    stream.Write(data, 0, data.Length);
                                }

                                response = (HttpWebResponse)request.GetResponse();

                                var result = new StreamReader(response.GetResponseStream()).ReadToEnd();











                            //try
                            //{
                            //    string codSituacaoCliente = "1";
                            //    string codMatrizFilialCliente = "4";
                            //    string nomeCliente = "valter batista";
                            //    string cpfCliente = "63847700081";//"63847700081"; //17734222862
                            //    string formatoEnvioTitulo = "198";
                            //    string formatoBoletoCliente = "U";
                            //    string enderecoClienteCep = "18078722";
                            //    string enderecoClienteNumero = "22";
                            //    string enderecoClienteLogradouro = "teste";
                            //    string enderecoClienteBairro = "teste";
                            //    string enderecoClienteCidade = "Sorocaba";
                            //    string enderecoClienteEstado = "SP";

                            //    //Caso o lead seja tratado no moskit como CLIENTE/RASTREADOR deve-se criar o cliente no SGR(Sistema Gerenciamento de Rastreador)
                            //    //https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/inserir_cliente

                            //    

                            //    try
                            //    {
                            //        //CookieContainer cookieContainer = new CookieContainer();
                            //        //System.Net.Cookie cookie1 = new System.Net.Cookie("PHPSESSID", "6vkqushiqds6nvd6mtp0bp2ab5");
                            //        //System.Net.Cookie cookie2 = new System.Net.Cookie("laravel_session", "eyJpdiI6IllFSmVTa0d6R09TWUJUUHRoSEt5dkE9PSIsInZhbHVlIjoiVlBDNlRoaW1KeGYweUJRU1VnQnpnRVpvZ3laeFNFdXpJTkptcVRxNERNMEczYWVqVWRUUnNJcnVmTjZrYkFYVjFDbGY5VWQ2bFQyeFwveHV5YjlTbFwvUT09IiwibWFjIjoiMTc3MTk5MGQ0MDVhMTM5MTY0M2FjMjg3NGUwMWM4NmNjMGNmYWE4ZDM0N2Q1OWY4NTExZWU4YzQwMTU1ZGM1MyJ9");
                            //        //cookie1.Domain = "sgr.hinova.com.br";
                            //        //cookie2.Domain = "sgr.hinova.com.br";
                            //        //cookieContainer.Add(cookie1);
                            //        //cookieContainer.Add(cookie2);

                            //        CookieContainer cookieContainer = new CookieContainer();
                            //        System.Net.Cookie cookie = new System.Net.Cookie("PHPSESSID", "6vkqushiqds6nvd6mtp0bp2ab5", "", "sgr.hinova.com.br");
                            //        cookieContainer.Add(cookie);

                            //        System.Net.Cookie cookie2 = new System.Net.Cookie("laravel_session", "eyJpdiI6IllFSmVTa0d6R09TWUJUUHRoSEt5dkE9PSIsInZhbHVlIjoiVlBDNlRoaW1KeGYweUJRU1VnQnpnRVpvZ3laeFNFdXpJTkptcVRxNERNMEczYWVqVWRUUnNJcnVmTjZrYkFYVjFDbGY5VWQ2bFQyeFwveHV5YjlTbFwvUT09IiwibWFjIjoiMTc3MTk5MGQ0MDVhMTM5MTY0M2FjMjg3NGUwMWM4NmNjMGNmYWE4ZDM0N2Q1OWY4NTExZWU4YzQwMTU1ZGM1MyJ9", "", "sgr.hinova.com.br");
                            //        cookie2.Expired = true;
                            //        cookie2.Expires = DateTime.Now.AddDays(1);
                            //        cookie2.HttpOnly = false;

                            //        cookieContainer.Add(cookie2);

                            //        HttpWebRequest request = null;
                            //        HttpWebResponse response = null;

                            //        request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/inserir_cliente/688472668f481b3efbddb0bfbff99cf6");
                            //        //request.Headers.Add("Accept", "application/json");                            
                            //        request.Headers.Add("X-Auth-Token", "$2y$11$N.zj6OpWG5Iqw8J7dt73TOTYrw..zTffmmSPyH6D8nNAIgCjoVZ3y");
                            //        request.Headers.Add("Authorization", "708183befb7e49cad4f8f025933f33897d52cc23");
                            //        request.Method = "POST";
                            //        //request.ContentType = "application/x-www-form-urlencoded";
                            //        request.ContentType = "multipart/form-data";
                            //        request.Accept = "application/json";
                            //        //request.KeepAlive = false;
                            //        //request.ServicePoint.ConnectionLimit = 10000;

                            //        //request.CookieContainer = new CookieContainer();
                            //        //request.CookieContainer.Add(new Cookie("ConstoCookie", "Chocolate Flavour"));

                            //        request.CookieContainer = cookieContainer;

                            //        //String json =
                            //        //    $"enrollment[registration_id]={registrationId}&" +
                            //        //    $"enrollment[school_product_id]={schoolProductId}&" +
                            //        //    $"enrollment[max_attendance_type]={maxAttendanceType}&" +
                            //        //    $"enrollment[status]={status}";

                            //        string postData =
                            //            $"cliente={3569}&" +
                            //            $"cod_situacao_cliente={codSituacaoCliente}&" +
                            //            $"cod_matriz_filial_cliente={codMatrizFilialCliente}&" +
                            //            $"nome_cliente={nomeCliente}&"
                            //            +
                            //            $"cpf_cliente={cpfCliente}&" +
                            //            $"formato_envio_titulo_cliente[0]=" + "{\"" + formatoEnvioTitulo + "\"}"
                            //            //+
                            //            //$"formato_boleto_cliente={formatoBoletoCliente}&" +
                            //            //$"endereco_cliente[cep]={enderecoClienteCep}&" +
                            //            //$"endereco_cliente[numero]={enderecoClienteNumero}&" +
                            //            //$"endereco_cliente[logradouro]={enderecoClienteLogradouro}&" +
                            //            //$"endereco_cliente[bairro]={enderecoClienteBairro}&" +
                            //            //$"endereco_cliente[cidade]={enderecoClienteCidade}&" +
                            //            //$"endereco_cliente[estado]={enderecoClienteEstado}"
                            //            ;

                            //        var data = Encoding.ASCII.GetBytes(postData);

                            //        request.ContentLength = data.Length;

                            //        using (var stream = request.GetRequestStream())
                            //        {
                            //            stream.Write(data, 0, data.Length);
                            //        }

                            //        //request.ContentLength = postData.Length;

                            //        //using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                            //        //{
                            //        //    streamWriter.Write(postData);
                            //        //}

                            //        response = (HttpWebResponse)request.GetResponse();

                            //        var result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                            //    }
                            //    catch (Exception ex)
                            //    {


                            //    }

                            //    string aqui = "";


                            //    //tipo
                            //}
                            //catch (WebException e)
                            //{
                            //    //serviceReturn.Status = 500;
                            //    //serviceReturn.Message = e.Message;

                            //    if (e.Response != null)
                            //    {
                            //        using (var errorResponse = (HttpWebResponse)e.Response)
                            //        {
                            //            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            //            {
                            //                String resp = reader.ReadToEnd();
                            //                //serviceReturn.Message = resp;
                            //                status = "10";
                            //                motivo = "Falha ao processar";
                            //                eventoRetorno = resp;
                            //            }
                            //        }
                            //    }
                            //}

                            ////Gerar pedido no SGR
                            ////https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/inserir_cliente/inserir_venda

                            ////cod_combustivel_veiculo_venda = 209;
                            ////cod_forma_pagamento_venda = 201;
                            ////cod_forma_pagamento_adesao_venda = 201;
                            ////cod_grupo_venda = 1;
                            ////cod_grupo_adesao_venda = 1002;
                            ////cod_periodo_venda = 336;
                            ////cod_vencimento_venda = 281;
                            ////cod_cliente_venda = 1;
                            ////cpf_cliente_venda = "17734222862";
                            ////rg_cliente_venda = "123";
                            ////placa_veiculo_venda = "fdd-4444";
                            ////anofab_veiculo_venda = "2020";
                            ////anomod_veiculo_venda = "2021";
                            ////chassi_veiculo_venda = 123;
                            ////fipe_valor_veiculo_venda = 999.99;
                            ////renavam_veiculo_vend = 123;
                            ////data_nascimento_cliente_venda = "2020-12-31"; //date
                            ////contato_cliente_venda = "Teste";
                            ////profissao_cliente_venda = "Analista";
                            ////interveniente_venda = "";
                            ////quantidade_parcela_venda = 1;
                            ////quantidade_parcela_adesao_venda = 1;
                            ////valor_parcela_venda = 999.99;
                            ////valor_parcela_adesao_venda = 999.99;
                            ////entrada_venda = 1;
                            ////observacao_venda = "obs";
                            ////sexo_cliente_venda = "M";
                            ////estado_civil_cliente_venda = "SO";                            
                            ////nome_cliente_venda = "Teste";
                            ////produto_venda[0][cod_produto] = 123;
                            ////cod_ponto_venda_venda = 1;
                            ////cod_consultor_venda = 1;
                            ////cod_departamento = 242;

                            ////fipe_codigo_veiculo_venda = "";

                            ////endereco_venda[0]
                            ////telefone_venda[0]
                            ////email_venda[0]
                            ////contato_venda[0]






























        private static Stack<int> userCruzeiro = new Stack<int>();
        private static Stack<int> userAnhanguera = new Stack<int>();
        private static Stack<int> userCruzPoolInterior = new Stack<int>();
        private static DataTable usersDataTable;


            //
            // Primeira coisa a fazer é pegar todos os usuarios e guarda-los separados por FUNIL
            // Caso a colecao userCruzeiro ou userAnhanguera esteja vazia devemos recupera-la da 
            // tabela usuario
            //
            if (userCruzeiro.Count == 0 || userAnhanguera.Count == 0 || userCruzPoolInterior.Count == 0)
            {
                string sql = "SELECT ";
                sql += "SYSAQ_NUMERO, SYSAQ_USUARIO, SYSAQ_FUNIL, SYSAQ_FUNIL_CRUZ, SYSAQ_FUNIL_ANHANG, SYSAQ_FUNIL_CRUZ_POOL ";
                sql += "FROM sysaq ";
                sql += "ORDER BY SYSAQ_NUMERO DESC ";

                usersDataTable = DataAccessManager.ExecuteDataTable(sql, null);

                if (userCruzeiro.Count == 0)
                {
                    foreach (DataRow row in usersDataTable.Rows)
                    {
                        if (BooleanUtilities.parse(row["SYSAQ_FUNIL_CRUZ"]))
                        {
                            userCruzeiro.Push(NumberUtilities.parseInt(row["SYSAQ_NUMERO"]));
                        }
                    }
                }

                if (userAnhanguera.Count == 0)
                {
                    foreach (DataRow row in usersDataTable.Rows)
                    {
                        if (BooleanUtilities.parse(row["SYSAQ_FUNIL_ANHANG"]))
                        {
                            userAnhanguera.Push(NumberUtilities.parseInt(row["SYSAQ_NUMERO"]));
                        }
                    }
                }

                if (userCruzPoolInterior.Count == 0)
                {
                    foreach (DataRow row in usersDataTable.Rows)
                    {
                        if (BooleanUtilities.parse(row["SYSAQ_FUNIL_CRUZ_POOL"]))
                        {
                            userCruzPoolInterior.Push(NumberUtilities.parseInt(row["SYSAQ_NUMERO"]));
                        }
                    }
                }
            }

            //
            // Caso o usuario tenha sido informado na planilha, utilizamos ele, caso contrario pego da lista de usuarios
            //
            int idUser = 0;

            foreach (DataRow row in usersDataTable.Rows)
            {
                if (StringUtilities.ToString(row["SYSAQ_USUARIO"]).Equals(responsavel))
                {
                    idUser = NumberUtilities.parseInt(row["SYSAQ_NUMERO"]);

                    break;
                }
            }

            if (idUser == 0)
            {
                //if (StringUtilities.ToString(IES).Equals("Cruzeiro do Sul", StringComparison.OrdinalIgnoreCase))
                if (funil.Equals("Cruzeiro do Sul", StringComparison.OrdinalIgnoreCase))
                {
                    idUser = userCruzeiro.Pop();
                }
                //else if (StringUtilities.ToString(IES).Equals("Anhanguera", StringComparison.OrdinalIgnoreCase))
                else if (funil.Equals("Anhanguera", StringComparison.OrdinalIgnoreCase))
                {
                    idUser = userAnhanguera.Pop();
                }
                else if (funil.Equals("Cruzeiro do Sul Pool Interior", StringComparison.OrdinalIgnoreCase))
                {
                    idUser = userCruzPoolInterior.Pop();
                }
            }



if (!String.IsNullOrEmpty(curso))
                                {
                                    CustomField customField1 = new CustomField();
                                    customField1.Id = "CF_dVKmQ5i1Cvnl0mWR";
                                    customField1.TextValue = curso;

                                    customFieldList.Add(customField1);
                                }

                                if (!String.IsNullOrEmpty(tipoSIAA))
                                {
                                    CustomField customField2 = new CustomField();
                                    customField2.Id = "CF_y5lm56iNCrO53DwW";
                                    customField2.TextValue = tipoSIAA;

                                    customFieldList.Add(customField2);
                                }

                                if (!String.IsNullOrEmpty(origemLead))
                                {
                                    CustomField customField3 = new CustomField();
                                    customField3.Id = "CF_ylAm0vi6C8bB0qvb";
                                    customField3.TextValue = origemLead;

                                    customFieldList.Add(customField3);
                                }

                                if (!String.IsNullOrEmpty(IES))
                                {
                                    CustomField customField4 = new CustomField();
                                    customField4.Id = "CF_Rg7MnEiLCa76yqvd";
                                    customField4.TextValue = IES;

                                    customFieldList.Add(customField4);
                                }

                                if (!String.IsNullOrEmpty(RGM))
                                {
                                    CustomField customField5 = new CustomField();
                                    customField5.Id = "CF_3NrDZAinCXo26mP5";
                                    customField5.TextValue = RGM;

                                    customFieldList.Add(customField5);
                                }

                                if (!String.IsNullOrEmpty(situacao))
                                {
                                    CustomField customField5 = new CustomField();
                                    customField5.Id = "CF_vG0mR0ikCXoJNqbV";
                                    customField5.TextValue = situacao;

                                    customFieldList.Add(customField5);
                                }

                                if (!String.IsNullOrEmpty(tipoEntrada))
                                {
                                    CustomField customField6 = new CustomField();
                                    customField6.Id = "CF_VrKMbQiaC9ZVPqZY";
                                    customField6.TextValue = tipoEntrada;

                                    customFieldList.Add(customField6);
                                }

                                if (!String.IsNullOrEmpty(unidade))
                                {
                                    CustomField customField7 = new CustomField();
                                    customField7.Id = "CF_2ojMxLiPCWJaXMOE";
                                    customField7.TextValue = unidade;

                                    customFieldList.Add(customField7);
                                }

                                if (!String.IsNullOrEmpty(cpf))
                                {
                                    CustomField customField8 = new CustomField();
                                    customField8.Id = "CF_gPpD7OikC7N4zDvo";
                                    customField8.TextValue = cpf;

                                    customFieldList.Add(customField8);
                                }

                                if (!String.IsNullOrEmpty(cpf))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_gPpD7OikC7N4zDvo";
                                    customField9.TextValue = cpf;

                                    customFieldList.Add(customField9);
                                }

                                if (!String.IsNullOrEmpty(dataInscricao))
                                {
                                    CustomField customField10 = new CustomField();
                                    customField10.Id = "CF_3LvDvEivUBbaBm6a";
                                    customField10.DateValue = dataInscricao;

                                    customFieldList.Add(customField10);
                                }

                                if (!String.IsNullOrEmpty(dataMatricula))
                                {
                                    CustomField customField11 = new CustomField();
                                    customField11.Id = "CF_GwyMgWi0UWrPgMLA";
                                    customField11.DateValue = dataMatricula;

                                    customFieldList.Add(customField11);
                                }

                                if (!String.IsNullOrEmpty(dataProva))
                                {
                                    CustomField customField11 = new CustomField();
                                    customField11.Id = "CF_gPpD7Oi9U75zKDvo";
                                    customField11.DateValue = dataProva;

                                    customFieldList.Add(customField11);
                                }

                                if (!String.IsNullOrEmpty(email))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_wGrqzpiVCjLBwmLo";
                                    customField9.TextValue = email;

                                    customFieldList.Add(customField9);
                                }

                                if (!String.IsNullOrEmpty(tag))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_42AmaJiZCwWn4Djl";
                                    customField9.TextValue = tag;

                                    customFieldList.Add(customField9);
                                }

                                if (!String.IsNullOrEmpty(whatsAppApi))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_dVKmQ5i1CdvBbmWR";
                                    customField9.TextValue = whatsAppApi;

                                    customFieldList.Add(customField9);
                                }
                                if (!String.IsNullOrEmpty(nomeIndicador))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_Pj3qYeieCrlldqQe";
                                    customField9.TextValue = nomeIndicador;

                                    customFieldList.Add(customField9);
                                }

                                if (!String.IsNullOrEmpty(emailIndicador))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_dVKmQ5i1CdJJpmWR";
                                    customField9.TextValue = emailIndicador;

                                    customFieldList.Add(customField9);
                                }
                                if (!String.IsNullOrEmpty(telefoneIndicador))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_A4wMWNigC6wwAqB8";
                                    customField9.TextValue = telefoneIndicador;

                                    customFieldList.Add(customField9);
                                }

                                if (!String.IsNullOrEmpty(pixIndicador))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_YXoDkki3CV44NDGE";
                                    customField9.TextValue = pixIndicador;

                                    customFieldList.Add(customField9);
                                }
                                if (!String.IsNullOrEmpty(codigoIndicador))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_6rRmweivC633gq4X";
                                    customField9.TextValue = codigoIndicador;

                                    customFieldList.Add(customField9);
                                }
                                if (!String.IsNullOrEmpty(telefone))
                                {
                                    CustomField customField9 = new CustomField();
                                    customField9.Id = "CF_E79Mr2ioCj2gNMZJ";
                                    customField9.TextValue = telefone;

                                    customFieldList.Add(customField9);
                                }


//
//24/02/2022
//


        private void Sync4(string name = "")
        {
            object ret = null;

            if (ThreadIsRunning3)
            {
                return;
            }

            ThreadIsRunning3 = true;

            LogApp($"Sync3 {name} iniciado...");

            try
            {

                IEnumerable<NegocioBO> negocioBOList = BusinessObjectManager.GetListByFilter<NegocioBO>($"GERAU_FILIAL={Store} AND GERAU_STATUS='00' ORDER BY GERAU_DT_ATUALIZACAO ASC");

                foreach (NegocioBO negocioEventoBO in negocioBOList)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    try
                    {

                        String id = StringUtilities.ToString(negocioEventoBO.GERAU_ID);
                        String nomeProprio = StringUtilities.ToString(negocioEventoBO.GERAU_NOME_PROPRIO);
                        String email = StringUtilities.ToString(negocioEventoBO.GERAU_EMAIL);
                        String telefonePrincipal = StringUtilities.ToString(negocioEventoBO.GERAU_FONE_PRINCIPAL);
                        String telefone = StringUtilities.ToString(negocioEventoBO.GERAU_FONE);
                        String tipoEntrada = StringUtilities.ToString(negocioEventoBO.GERAU_TP_ENTRADA);
                        String curso = StringUtilities.ToString(negocioEventoBO.GERAU_CURSO);
                        String tipoSIAA = StringUtilities.ToString(negocioEventoBO.GERAU_TP_SIAA);
                        String origemLead = StringUtilities.ToString(negocioEventoBO.GERAU_ORIGEM_LEAD);

                        //if (!nomeProprio.Contains("Eduarda Macedo Maciel"))
                        //{
                        //    continue;
                        //}

                        DateTime? dataProvaDate = (DateTime)negocioEventoBO.GERAU_DT_PROVA;
                        string dataProva = dataProvaDate == null ? "" : dataProvaDate.Value.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();

                        dataProva = dataProva.Equals("0001-01-01T00:00:00.000-03:00") ? "" : dataProva;

                        DateTime? dataInscricaoDate = (DateTime)negocioEventoBO.GERAU_DT_INSCRICAO;
                        string dataInscricao = dataInscricaoDate == null ? "" : dataInscricaoDate.Value.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();

                        dataInscricao = dataInscricao.Equals("0001-01-01T00:00:00.000-03:00") ? "" : dataInscricao;

                        DateTime? dataMatriculaDate = (DateTime)negocioEventoBO.GERAU_DT_MATRICULA;
                        string dataMatricula = dataMatriculaDate == null ? "" : dataMatriculaDate.Value.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
                        dataMatricula = dataMatricula.Equals("0001-01-01T00:00:00.000-03:00") ? "" : dataMatricula;

                        String situacao = StringUtilities.ToString(negocioEventoBO.GERAU_SITUACAO);
                        String unidade = StringUtilities.ToString(negocioEventoBO.GERAU_UNIDADE);
                        String cpf = StringUtilities.ToString(negocioEventoBO.GERAU_CPF);

                        String IES = StringUtilities.ToString(negocioEventoBO.GERAU_IES);
                        String RGM = StringUtilities.ToString(negocioEventoBO.GERAU_RGM);
                        String responsavel = StringUtilities.ToString(negocioEventoBO.GERAU_RESPONSAVEL);
                        String tag = StringUtilities.ToString(negocioEventoBO.GERAU_TAG);

                        String whatsAppApi = StringUtilities.ToString(negocioEventoBO.GERAU_WHATSAPP_API);

                        whatsAppApi = whatsAppApi.Replace(" ", "");

                        if (!Regex.IsMatch(whatsAppApi, @"^https?:\/\/", RegexOptions.IgnoreCase))
                        {
                            whatsAppApi = "";
                        }

                        String nomeIndicador = StringUtilities.ToString(negocioEventoBO.GERAU_INDICADOR_NOME);
                        String emailIndicador = StringUtilities.ToString(negocioEventoBO.GERAU_INDICADOR_EMAIL);
                        String telefoneIndicador = StringUtilities.ToString(negocioEventoBO.GERAU_INDICADOR_TELEFONE);
                        String pixIndicador = StringUtilities.ToString(negocioEventoBO.GERAU_INDICADOR_PIX);
                        String codigoIndicador = StringUtilities.ToString(negocioEventoBO.GERAU_INDICADOR_CODIGO);

                        String nomeNegocio = origemLead + " - " + tipoEntrada + " - " + nomeProprio;

                        if (String.IsNullOrEmpty(nomeProprio))
                        {
                            throw new Exception("Nome é obrigatório");
                        }

                        if (String.IsNullOrEmpty(email))
                        {
                            throw new Exception("Email é obrigatório");
                        }

                        if (String.IsNullOrEmpty(telefonePrincipal))
                        {
                            throw new Exception("Telefone é obrigatório");
                        }

                        //
                        //
                        //
                        MoskitClient moskitClient = new MoskitClient();
                        moskitClient.Token = Parameters["PARAM_0005"];

                        DealService dealService = new DealService(moskitClient);

                        ServiceReturn serviceReturn = new ServiceReturn();

                        //
                        // Grava no MOSKIT, empresa, contato e negocio
                        //

                        String idStage = "";
                        String funil = "";
                        String ganho = "";
                        String status = "OPEN";

                        //UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_NUMERO={responsavelId}");

                        //NegocioBO negocioBO = new NegocioBO().(0, id);

                        if (IES.Equals("Cruzeiro do Sul"))
                        {
                            idStage = "139362";
                            funil = "32158";
                        }
                        else if (IES.Equals("Anhanguera"))
                        {
                            idStage = "144919";
                        }
                        else if (IES.Equals("Cruzeiro do Sul Pool Interior"))
                        {
                            idStage = "173790";
                        }
                        else if (string.IsNullOrEmpty(IES))
                        {

                            if (negocioEventoBO != null)
                            {
                                ret = BusinessObjectManager.UpdateFields(
                                 negocioEventoBO,
                                 new string[] { "GERAU_STATUS" },
                                 new object[] { "99" }
                                );

                                if (ret != null)
                                {
                                    break;
                                }
                            }

                            continue;
                        }

                        if (StringUtilities.ToString(situacao).Equals("Matriculado", StringComparison.OrdinalIgnoreCase))
                        {
                            ganho = "7713602";
                            status = "WON";
                        }

                        //String dataAtual = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString("f0");
                        //long dataInt = NumberUtilities.parseLong(dataAtual);

                        //param = JArray.Parse("[{\"field\": \"phones\",\"expression\": \"like\",\"values\": [\"" + contatoId + "\" ]}]");

                        //JArray contactsSearch = (JArray)SendCommand($"https://api.moskitcrm.com/v2/contacts/search", "POST", param, Parameters["PARAM_0005"]);

                        //
                        // Busca o contato por email
                        //
                        var param = JArray.Parse("[{\"field\": \"emails\",\"expression\": \"like\", \"values\": [\"" + email + "\" ]}]");

                        JArray results = (JArray)SendCommand("https://api.moskitcrm.com/v2/contacts/search", "POST", param, Parameters["PARAM_0005"]);

                        if (results.Count == 0)
                        {
                            param = JArray.Parse("[{\"field\": \"phones\",\"expression\": \"like\", \"values\": [\"" + telefonePrincipal + "\" ]}]");

                            results = (JArray)SendCommand("https://api.moskitcrm.com/v2/contacts/search", "POST", param, Parameters["PARAM_0005"]);
                        }

                        long idNegocio = 0;

                        JObject contact = null;

                        JArray dealContact = null;
                        long contatoId = 0;
                        int idUser = 0;

                        String statusMosk = "";

                        if (results.Count > 0)
                        {
                            //
                            // Se existe contato Atualizo o Contato
                            //
                            contact = (JObject)results[0];

                            contatoId = NumberUtilities.parseLong(contact["id"]);

                            contact["name"] = nomeProprio;
                            if (contact["phones"] != null && ((JArray)contact["phones"]).Count > 0)
                            {
                                //contact["primaryPhone"]["number"] = "55" + telefone;
                                contact["phones"][0]["number"] = "55" + telefonePrincipal;
                            }
                            else
                            {
                                if (contact["phones"] == null)
                                {
                                    contact.Add(new JProperty("phones"));
                                }

                                ((JArray)contact["phones"]).Add(new JObject(
                                        //new JProperty("entity", "Phone"),
                                        new JProperty("number", "55" + telefonePrincipal),
                                        new JProperty("type", new JObject(
                                        new JProperty("entity", "PhoneType"),
                                        new JProperty("name", "Celular")
                                        ))
                                    ));
                            }

                            dealContact = (JArray)contact["deals"];

                            if (dealContact.Count == 0)
                            {
                                idUser = distribuiUsuario(IES, responsavel);
                                contact["responsible"]["id"] = idUser;
                            }


                            try
                            {
                                contact = (JObject)SendCommand($"https://api.moskitcrm.com/v2/contacts/" + contact["id"], "PUT", contact, Parameters["PARAM_0005"]);

                                //idUser = NumberUtilities.parseInt(contact["responsible"]["id"]);
                            }
                            catch (Exception ex)
                            {
                                string mensagem = StringUtilities.ToString(ex.Message);

                                if (mensagem.Contains("Id not found"))
                                {
                                    //contact["deal"].Remove();

                                    //foreach (JObject dealContato in dealContact)
                                    //{

                                    //}

                                    //contact = (JObject)SendCommand($"https://api.moskitcrm.com/v2/contacts/" + contact["id"], "PUT", contact, Parameters["PARAM_0005"]);

                                }

                                LogApp(ex.Message.ToString());

                                continue;
                            }

                            dealContact = (JArray)contact["deals"];

                            //if (dealContact == null)
                            //{
                            //    idUser = NumberUtilities.parseInt(contact["responsible"]["id"]);
                            //}

                        }
                        else
                        {
                            //
                            // Criar contato
                            //
                            Thread.Sleep(1000);

                            idUser = distribuiUsuario(IES, responsavel);

                            if (idUser == 0)
                            {
                                string aqui = "";
                            }

                            //try
                            //{
                            //    JArray filter = JArray.Parse("[{\"field\": \"CF_wGrqzpiVCjLBwmLo\",\"expression\": \"match\",\"values\": [\"" + email + "\" ]}]");

                            //    JArray deals = (JArray)SendCommand($"https://api.moskitcrm.com/v2/deals/search", "POST", filter, Parameters["PARAM_0005"]);

                            //    idNegocio = NumberUtilities.parseLong(deals[0]["id"]);
                            //    idUser = NumberUtilities.parseInt(deals[0]["responsible"]["id"]); //50367;

                            //    if (deals[0]["contacts"] != null)
                            //    {
                            //        //
                            //        //Atualizo o evento
                            //        //
                            //        ret = BusinessObjectManager.UpdateFields(
                            //                   negocioEventoBO,
                            //                   new string[] { "GERAU_STATUS" },
                            //                   new object[] { "10" }
                            //                   );

                            //        if (ret != null)
                            //        {
                            //            break;
                            //        }

                            //        continue;

                            //    }

                            //}
                            //catch (Exception ex)
                            //{

                            //}

                            ContactService contactService = new ContactService(moskitClient);

                            List<CustomField> customFieldListContact = new List<CustomField>();

                            Contact contactNew = new Contact();
                            contactNew.DateCreated = DateTime.Now.AddMinutes(-1);
                            contactNew.Name = nomeProprio;

                            if (!String.IsNullOrEmpty(email))
                            {
                                contactNew.Emails = new Email[] { new Email() { Address = email } };
                            }

                            if (!String.IsNullOrEmpty(telefone))
                            {
                                contactNew.Phones = new Phone[] { new Phone() { Number = telefonePrincipal } };
                            }

                            contactNew.CreatedBy = new Identity() { Id = idUser };
                            contactNew.Responsible = new Identity() { Id = idUser };
                            contactNew.Deals = idNegocio != 0 ? new Identity[] { new Identity() { Id = idNegocio } } : null;

                            //
                            //Campos customizados de contato não está no khronus
                            //
                            if (!String.IsNullOrEmpty(cpf))
                            {
                                CustomField customField1 = new CustomField();
                                customField1.Id = "CF_VRAqdlSdC5zbPqbL";
                                customField1.TextValue = cpf;

                                customFieldListContact.Add(customField1);
                            }

                            serviceReturn = contactService.Post(contactNew);

                            if (serviceReturn.Status == 200)
                            {
                                contatoId = NumberUtilities.parseLong(((Khronus.Framework.Integration.Moskit.Model.Contact)serviceReturn.Detail).Id);

                                ////
                                ////Atualizo o evento
                                ////
                                //ret = BusinessObjectManager.UpdateFields(
                                //           negocioEventoBO,
                                //           new string[] { "GERAU_STATUS" },
                                //           new object[] { "10" }
                                //           );

                                //if (ret != null)
                                //{
                                //    break;
                                //}

                                //continue;

                            }
                            else
                            {
                                continue;
                            }

                        }

                        ////
                        ////Se contato não tem lead
                        ////
                        //if (contact["deals"] == null)
                        //{
                        //    contact.Add(new JProperty("deals"));
                        //}

                        JObject deal = null;  //  <------ ????????? JObject? se vc que o .Status tem que ser Deal

                        String funilName = null;

                        Boolean funilNameDiferente = false;

                        String etapa = "";

                        //if (dealContact != null)
                        //{
                        //    continue;
                        //}

                        //
                        //Se contato tem lead
                        //
                        //if (dealContact.Count != 0)
                        if (dealContact != null)
                        {
                            //
                            //Verifico se o negócio esteja entre os 3 funils Anhanguera, cruzeiro do sul ou Cruzeiro do Sul Pool Interior
                            //
                            foreach (JObject item in ((JArray)contact["deals"]))
                            {
                                idNegocio = NumberUtilities.parseInt(item["id"]);

                                deal = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idNegocio}", "GET", null, Parameters["PARAM_0005"]);

                                long stageDealId = NumberUtilities.parseLong(deal["stage"]["id"]);

                                JObject etapas = (JObject)SendCommand($"https://api.moskitcrm.com/v2/stages/{stageDealId}", "GET", null, Parameters["PARAM_0005"]);

                                int pipeline = NumberUtilities.parseInt(etapas["pipeline"]["id"]);

                                // 32158 - Cruzeiro do Sul
                                // 33151 - Anhanguera
                                // 39043 - Cruzeiro do Sul Pool Interior
                                funilName = pipeline == 32158 ? "Cruzeiro do Sul" : pipeline == 33151 ? "Anhanguera" : pipeline == 39043 ? "Cruzeiro do Sul Pool Interior" : "";

                                //funilName = StringUtilities.ToString(deal["stage"]["pipeline"]["name"]);

                                if (funilName.Equals(IES, StringComparison.OrdinalIgnoreCase))
                                {
                                    funilNameDiferente = true;

                                    break;
                                }
                            }

                            statusMosk = StringUtilities.ToString(deal["status"]);

                            //if (statusMosk.Equals("WON") && !status.Equals("WON")

                            if (statusMosk.Equals("WON"))
                            {
                                //negocioBO = new NegocioBO().Find(0, id);

                                if (negocioEventoBO != null)
                                {
                                    ret = BusinessObjectManager.UpdateFields(
                                        negocioEventoBO,
                                        new string[] { "GERAU_STATUS" },
                                        new object[] { "44" }
                                        );

                                    if (ret != null)
                                    {
                                        break;
                                    }
                                }

                                continue;
                            }

                            etapa = StringUtilities.ToString(deal["stage"]["id"]);

                        }

                        //
                        //  Se funil do negócio encontrador é diferente da planilha insere o negócio ou se não existe negócio para o contato
                        //                                
                        if ((!StringUtilities.ToString(funilName).Equals(IES, StringComparison.OrdinalIgnoreCase) && !funilNameDiferente) || (dealContact.Count == 0))
                        {

                            if (idUser == 0)
                            {
                                idUser = distribuiUsuario(IES, responsavel);
                            }

                            if (contatoId == 0)
                            {

                                continue;
                            }

                            //
                            //Gero negócio
                            //

                            List<CustomField> customFieldList = new List<CustomField>();

                            Deal dealNew = new Deal();
                            dealNew.Name = nomeNegocio;
                            dealNew.Status = "OPEN";
                            dealNew.DateCreated = DateTime.Now.AddMinutes(-1);
                            dealNew.PrevisionCloseDate = DateTime.Now;
                            dealNew.Stage = new Identity() { Id = NumberUtilities.parseLong(idStage) }; //180220 - NOVO
                            dealNew.Contacts = contatoId != 0 ? new Identity[] { new Identity() { Id = contatoId } } : null;
                            dealNew.CreatedBy = new Identity() { Id = idUser };
                            dealNew.Responsible = new Identity() { Id = idUser };

                            if (!String.IsNullOrEmpty(curso))
                            {
                                CustomField customField1 = new CustomField();
                                customField1.Id = "CF_dVKmQ5i1Cvnl0mWR";
                                customField1.TextValue = curso;

                                customFieldList.Add(customField1);
                            }

                            if (!String.IsNullOrEmpty(tipoSIAA))
                            {
                                CustomField customField2 = new CustomField();
                                customField2.Id = "CF_y5lm56iNCrO53DwW";
                                customField2.TextValue = tipoSIAA;

                                customFieldList.Add(customField2);
                            }

                            if (!String.IsNullOrEmpty(origemLead))
                            {
                                CustomField customField3 = new CustomField();
                                customField3.Id = "CF_ylAm0vi6C8bB0qvb";
                                customField3.TextValue = origemLead;

                                customFieldList.Add(customField3);
                            }

                            if (!String.IsNullOrEmpty(IES))
                            {
                                CustomField customField4 = new CustomField();
                                customField4.Id = "CF_Rg7MnEiLCa76yqvd";
                                customField4.TextValue = IES;

                                customFieldList.Add(customField4);
                            }

                            if (!String.IsNullOrEmpty(RGM))
                            {
                                CustomField customField5 = new CustomField();
                                customField5.Id = "CF_3NrDZAinCXo26mP5";
                                customField5.TextValue = RGM;

                                customFieldList.Add(customField5);
                            }

                            if (!String.IsNullOrEmpty(situacao))
                            {
                                CustomField customField6 = new CustomField();
                                customField6.Id = "CF_vG0mR0ikCXoJNqbV";
                                customField6.TextValue = situacao;

                                customFieldList.Add(customField6);
                            }

                            if (!String.IsNullOrEmpty(tipoEntrada))
                            {
                                CustomField customField7 = new CustomField();
                                customField7.Id = "CF_VrKMbQiaC9ZVPqZY";
                                customField7.TextValue = tipoEntrada;

                                customFieldList.Add(customField7);
                            }

                            if (!String.IsNullOrEmpty(unidade))
                            {
                                CustomField customField8 = new CustomField();
                                customField8.Id = "CF_2ojMxLiPCWJaXMOE";
                                customField8.TextValue = unidade;

                                customFieldList.Add(customField8);
                            }

                            if (!String.IsNullOrEmpty(cpf))
                            {
                                CustomField customField9 = new CustomField();
                                customField9.Id = "CF_gPpD7OikC7N4zDvo";
                                customField9.TextValue = cpf;

                                customFieldList.Add(customField9);
                            }

                            if (!String.IsNullOrEmpty(dataInscricao))
                            {
                                CustomField customField10 = new CustomField();
                                customField10.Id = "CF_3LvDvEivUBbaBm6a";
                                //customField10.DateValue = (DateTime)negocioEventoBO.GERAU_DT_INSCRICAO;
                                customField10.DateValue = DateUtilities.Parse(DateUtilities.format(DateUtilities.Parse(negocioEventoBO.GERAU_DT_INSCRICAO), "dd/MM/yyyy") + " " + DateTime.Now.ToString("HH:mm:ss.FFF"));

                                customFieldList.Add(customField10);
                            }

                            if (!String.IsNullOrEmpty(dataMatricula))
                            {
                                CustomField customField11 = new CustomField();
                                customField11.Id = "CF_GwyMgWi0UWrPgMLA";
                                //customField11.DateValue = (DateTime)negocioEventoBO.GERAU_DT_MATRICULA;
                                customField11.DateValue = DateUtilities.Parse(DateUtilities.format(DateUtilities.Parse(negocioEventoBO.GERAU_DT_MATRICULA), "dd/MM/yyyy") + " " + DateTime.Now.ToString("HH:mm:ss.FFF"));

                                customFieldList.Add(customField11);
                            }

                            if (!String.IsNullOrEmpty(dataProva))
                            {
                                CustomField customField12 = new CustomField();
                                customField12.Id = "CF_gPpD7Oi9U75zKDvo";
                                //customField12.DateValue = (DateTime)negocioEventoBO.GERAU_DT_PROVA;
                                customField12.DateValue = DateUtilities.Parse(DateUtilities.format(DateUtilities.Parse(negocioEventoBO.GERAU_DT_PROVA), "dd/MM/yyyy") + " " + DateTime.Now.ToString("HH:mm:ss.FFF"));

                                customFieldList.Add(customField12);
                            }

                            if (!String.IsNullOrEmpty(email))
                            {
                                CustomField customField13 = new CustomField();
                                customField13.Id = "CF_wGrqzpiVCjLBwmLo";
                                customField13.TextValue = email;

                                customFieldList.Add(customField13);
                            }

                            if (!String.IsNullOrEmpty(tag))
                            {
                                CustomField customField14 = new CustomField();
                                customField14.Id = "CF_42AmaJiZCwWn4Djl";
                                customField14.TextValue = tag;

                                customFieldList.Add(customField14);
                            }

                            if (!String.IsNullOrEmpty(whatsAppApi))
                            {
                                CustomField customField15 = new CustomField();
                                customField15.Id = "CF_dVKmQ5i1CdvBbmWR";
                                customField15.TextValue = whatsAppApi;

                                customFieldList.Add(customField15);
                            }
                            if (!String.IsNullOrEmpty(nomeIndicador))
                            {
                                CustomField customField16 = new CustomField();
                                customField16.Id = "CF_Pj3qYeieCrlldqQe";
                                customField16.TextValue = nomeIndicador;

                                customFieldList.Add(customField16);
                            }

                            if (!String.IsNullOrEmpty(emailIndicador))
                            {
                                CustomField customField17 = new CustomField();
                                customField17.Id = "CF_dVKmQ5i1CdJJpmWR";
                                customField17.TextValue = emailIndicador;

                                customFieldList.Add(customField17);
                            }
                            if (!String.IsNullOrEmpty(telefoneIndicador))
                            {
                                CustomField customField18 = new CustomField();
                                customField18.Id = "CF_A4wMWNigC6wwAqB8";
                                customField18.TextValue = telefoneIndicador;

                                customFieldList.Add(customField18);
                            }

                            if (!String.IsNullOrEmpty(pixIndicador))
                            {
                                CustomField customField19 = new CustomField();
                                customField19.Id = "CF_YXoDkki3CV44NDGE";
                                customField19.TextValue = pixIndicador;

                                customFieldList.Add(customField19);
                            }
                            if (!String.IsNullOrEmpty(codigoIndicador))
                            {
                                CustomField customField20 = new CustomField();
                                customField20.Id = "CF_6rRmweivC633gq4X";
                                customField20.TextValue = codigoIndicador;

                                customFieldList.Add(customField20);
                            }
                            if (!String.IsNullOrEmpty(telefone))
                            {
                                CustomField customField21 = new CustomField();
                                customField21.Id = "CF_E79Mr2ioCj2gNMZJ";
                                customField21.TextValue = telefone;

                                customFieldList.Add(customField21);
                            }

                            dealNew.CustomFields = customFieldList.ToArray();

                            if (StringUtilities.ToString(situacao).Equals("Matriculado", StringComparison.OrdinalIgnoreCase))
                            {
                                dealNew.Status = "WON";
                                dealNew.CloseDate = DateTime.Now;
                            }

                            serviceReturn = dealService.Post(dealNew);

                            if (serviceReturn.Status == 200)
                            {
                                Thread.Sleep(2000);

                                JObject dealReturn = JObject.Parse(StringUtilities.ToString(serviceReturn.Message));
                                idNegocio = NumberUtilities.parseLong(((Newtonsoft.Json.Linq.JContainer)dealReturn.First).First);

                                //deal = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idNegocio}", "GET", null, Parameters["PARAM_0005"]);

                                //statusMosk = "OPEN";

                                LogApp($"Novo lead: {idNegocio}");
                            }
                            //else
                            //{
                            //    LogApp($"Novo lead: {idNegocio}");
                            //    continue;
                            //}

                            //if (idNegocio == 0)
                            //{
                            //    LogApp($"Novo lead: {idNegocio}");

                            //    continue;
                            //}

                        }
                        else
                        {
                            //deal = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idNegocio}", "GET", null, Parameters["PARAM_0005"]);

                            //
                            //Se Situação Aprovado ou Reprovado  e na etapa Vestibular ou Processo mudar para a etapa Resultado
                            //
                            if (StringUtilities.ToString(situacao).Equals("aprovado", StringComparison.OrdinalIgnoreCase) || StringUtilities.ToString(situacao).Equals("reprovado", StringComparison.OrdinalIgnoreCase))
                            {
                                //
                                //Stage Cruzeiro do sul: Vestibular - 32158 - 140024 / Processo - 32158 - 140025 = Resultado - 32158 - 140026
                                //
                                if (IES.Equals("Cruzeiro do Sul", StringComparison.OrdinalIgnoreCase) && (etapa.Equals("140024") || etapa.Equals("140025")))
                                {
                                    //deal["stage"]["id"] = "140026";
                                    idStage = "140026";
                                }
                                //
                                //Stage Anhanguera - Vestibular - 33151 - 147975 / Processo - 33151 - 147976 = Resultado - 33151 - 147977
                                //
                                else if (IES.Equals("Anhanguera", StringComparison.OrdinalIgnoreCase) && (etapa.Equals("147975") || etapa.Equals("147976")))
                                {
                                    //deal["stage"]["id"] = "147977";

                                    idStage = "147977";
                                }
                                else if (IES.Equals("Cruzeiro do Sul Pool Interior", StringComparison.OrdinalIgnoreCase) && (etapa.Equals("173799") || etapa.Equals("173800")))
                                {
                                    //deal["stage"]["id"] = "173801";

                                    idStage = "173801";
                                }
                                else
                                {
                                    idStage = StringUtilities.ToString(deal["stage"]["id"]);
                                }
                            }

                            //
                            // Se nao foi ganho, voltar para a Etapa para Contato
                            //
                            if (!statusMosk.Equals("WON", StringComparison.OrdinalIgnoreCase))
                            {
                                //
                                //Conversando - 139363 / Boleto - 140027 / Aceite - 140029
                                //
                                if (IES.Equals("Cruzeiro do Sul", StringComparison.OrdinalIgnoreCase) &&
                                   (!(etapa.Equals("140024") || etapa.Equals("140025") || etapa.Equals("140026") || etapa.Equals("139363") || etapa.Equals("140027") || etapa.Equals("140029") || etapa.Equals("195300") || etapa.Equals("195295"))))
                                {
                                    //deal["stage"]["id"] = "139362"; //contato cruzeiro

                                    idStage = "139362";
                                }

                                //Conversando - 147974 / Boleto - 147978 / Aceito - 147980
                                //Vestibular / Processo / Resultado / B / A
                                else if (IES.Equals("Anhanguera", StringComparison.OrdinalIgnoreCase) && (!(etapa.Equals("147975") || etapa.Equals("147976") || etapa.Equals("147977") || etapa.Equals("147978") || etapa.Equals("147980"))))
                                {
                                    //deal["stage"]["id"] = "144919"; //contato anhanguera

                                    idStage = "144919";
                                }

                                else if (IES.Equals("Cruzeiro do Sul Pool Interior", StringComparison.OrdinalIgnoreCase)
                                    && (!(etapa.Equals("173799") || etapa.Equals("173800") || etapa.Equals("173801") || etapa.Equals("173802") || etapa.Equals("173802") || etapa.Equals("196505") || etapa.Equals("196506"))))
                                {
                                    //deal["stage"]["id"] = "173790"; //contato Cruzeiro do Sul Pool Interior

                                    idStage = "173790";
                                }
                                else
                                {
                                    idStage = StringUtilities.ToString(deal["stage"]["id"]);
                                }
                            }

                            //NegocioBO negocio2BO = new NegocioBO().Find(0, email, null);

                            //if (negocio2BO != null)
                            //{
                            //    if (!negocio2BO.GERAU_ID.Equals(id))
                            //    {
                            //        negocio2BO.GERAU_ETAPA = etapa;

                            //        ret = negocio2BO.Update();
                            //    }
                            //}

                            if (negocioEventoBO != null)
                            {
                                ret = BusinessObjectManager.UpdateFields(
                                    negocioEventoBO,
                                    new string[] { "GERAU_ETAPA" },
                                    new object[] { etapa }
                                    );

                                if (ret != null)
                                {
                                    break;
                                }
                            }

                            //} fim regra


                            //
                            //Atualizo os campos customizados
                            //
                            Thread.Sleep(1000);

                            deal = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idNegocio}", "GET", null, Parameters["PARAM_0005"]);

                            deal["name"] = nomeNegocio;

                            string statusDealAtual = StringUtilities.ToString(deal["status"]);

                            if (!statusDealAtual.Equals("WON"))
                            {

                                if (deal["stage"] == null)
                                {
                                    Thread.Sleep(1000);

                                    deal = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idNegocio}", "GET", null, Parameters["PARAM_0005"]);

                                }

                                if (deal["stage"] == null)
                                {

                                    LogApp("Erro na consulta no moskit");

                                    return;
                                }

                                deal["stage"]["id"] = idStage;

                                JArray entityCustomFields = (JArray)deal["entityCustomFields"];

                                //JArray entityCustomFields = null;

                                entityCustomFields.Clear();

                                if (!String.IsNullOrEmpty(curso))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_dVKmQ5i1Cvnl0mWR"),
                                    new JProperty("textValue", curso))
                                    );
                                }

                                if (!String.IsNullOrEmpty(tipoSIAA))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_y5lm56iNCrO53DwW"),
                                    new JProperty("textValue", tipoSIAA))
                                    );
                                }

                                if (!String.IsNullOrEmpty(origemLead))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_ylAm0vi6C8bB0qvb"),
                                    new JProperty("textValue", origemLead))
                                    );
                                }

                                if (!String.IsNullOrEmpty(IES))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_Rg7MnEiLCa76yqvd"),
                                    new JProperty("textValue", IES))
                                    );
                                }

                                if (!String.IsNullOrEmpty(RGM))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_3NrDZAinCXo26mP5"),
                                    new JProperty("textValue", RGM))
                                    );

                                }

                                if (!String.IsNullOrEmpty(situacao))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_vG0mR0ikCXoJNqbV"),
                                    new JProperty("textValue", situacao))
                                    );
                                }

                                if (!String.IsNullOrEmpty(tipoEntrada))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_VrKMbQiaC9ZVPqZY"),
                                    new JProperty("textValue", tipoEntrada))
                                    );
                                }

                                if (!String.IsNullOrEmpty(unidade))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_2ojMxLiPCWJaXMOE"),
                                    new JProperty("textValue", unidade))
                                    );
                                }

                                if (!String.IsNullOrEmpty(cpf))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_gPpD7OikC7N4zDvo"),
                                    new JProperty("textValue", cpf))
                                    );
                                }

                                if (!String.IsNullOrEmpty(dataInscricao))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_3LvDvEivUBbaBm6a"),
                                    new JProperty("dateValue", dataInscricao))
                                    );
                                }

                                if (!String.IsNullOrEmpty(dataMatricula))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_GwyMgWi0UWrPgMLA"),
                                    new JProperty("dateValue", dataMatricula))
                                    );
                                }

                                if (!String.IsNullOrEmpty(dataProva))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_gPpD7Oi9U75zKDvo"),
                                    new JProperty("dateValue", dataProva))
                                    );
                                }

                                if (!String.IsNullOrEmpty(email))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_wGrqzpiVCjLBwmLo"),
                                    new JProperty("textValue", email))
                                    );
                                }

                                if (!String.IsNullOrEmpty(tag))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_42AmaJiZCwWn4Djl"),
                                    new JProperty("textValue", tag))
                                    );
                                }

                                if (!String.IsNullOrEmpty(whatsAppApi))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_dVKmQ5i1CdvBbmWR"),
                                    new JProperty("textValue", whatsAppApi))
                                    );
                                }

                                if (!String.IsNullOrEmpty(nomeIndicador))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_Pj3qYeieCrlldqQe"),
                                    new JProperty("textValue", nomeIndicador))
                                    );
                                }

                                if (!String.IsNullOrEmpty(emailIndicador))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_dVKmQ5i1CdJJpmWR"),
                                    new JProperty("textValue", emailIndicador))
                                    );
                                }
                                if (!String.IsNullOrEmpty(telefoneIndicador))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_A4wMWNigC6wwAqB8"),
                                    new JProperty("textValue", telefoneIndicador))
                                    );
                                }

                                if (!String.IsNullOrEmpty(pixIndicador))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_YXoDkki3CV44NDGE"),
                                    new JProperty("textValue", pixIndicador))
                                    );
                                }
                                if (!String.IsNullOrEmpty(codigoIndicador))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_6rRmweivC633gq4X"),
                                    new JProperty("textValue", codigoIndicador))
                                    );
                                }
                                if (!String.IsNullOrEmpty(telefone))
                                {
                                    entityCustomFields.Add(new JObject(
                                    new JProperty("id", "CF_E79Mr2ioCj2gNMZJ"),
                                    new JProperty("textValue", telefone))
                                    );
                                }

                                //deal["entityCustomFields"] = entityCustomFields;

                                //
                                //Atualizo o negócio para ganho
                                //

                                if (StringUtilities.ToString(situacao).Equals("Matriculado", StringComparison.OrdinalIgnoreCase))
                                {
                                    ganho = "7713602";
                                    status = "WON";

                                    deal["closeDate"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
                                    deal["status"] = "WON";
                                }

                                if (status.Equals("WON") && !statusMosk.Equals("WON"))
                                {
                                    deal["closeDate"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
                                    deal["status"] = "WON";
                                }

                                if (deal["previsionCloseDate"] != null)
                                {
                                    DateTime? previsionCloseDateData = (DateTime)DateUtilities.parse(deal["previsionCloseDate"]);

                                    //Negocios antigos estão vindo com data incorreta então coloco a data atual
                                    deal["previsionCloseDate"] = previsionCloseDateData == null ? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC()
                                                                                                 : previsionCloseDateData.Value.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
                                }

                                if (deal["closeDate"] != null)
                                {
                                    DateTime? closeDateData = (DateTime)DateUtilities.parse(deal["closeDate"]);

                                    //Negocios antigos estão vindo com data incorreta então coloco a data atual
                                    deal["closeDate"] = closeDateData.Value.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
                                }

                                string statusDeal = statusDealAtual;

                                if (statusDealAtual.Equals("LOST"))
                                {
                                    deal["status"] = "OPEN";

                                    if (IES.Equals("Cruzeiro do Sul"))
                                    {
                                        deal["stage"]["id"] = "139362";
                                    }
                                    else if (IES.Equals("Anhanguera"))
                                    {
                                        deal["stage"]["id"] = "144919";
                                    }
                                    else if (IES.Equals("Cruzeiro do Sul Pool Interior"))
                                    {
                                        deal["stage"]["id"] = "173790";
                                    }
                                }

                                if (deal["price"] == null)
                                {
                                    deal["price"] = 000;
                                }

                                deal = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idNegocio}", "PUT", deal, Parameters["PARAM_0005"]);

                                LogApp($"Negócio atualizado: {idNegocio}");
                            }

                        } //Fim atualização


                        //}

                        //
                        //Atualizo o evento
                        //
                        ret = BusinessObjectManager.UpdateFields(
                                   negocioEventoBO,
                                   new string[] { "GERAU_STATUS" },
                                   new object[] { "10" }
                                   );

                        if (ret != null)
                        {
                            break;
                        }

                    }
                    catch (Exception ex)
                    {
                        LogApp(ex);
                    }

                }

                //if (ret != null)
                //{
                //    throw new Exception(ret.ToString());
                //}
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp($"Sync3 {name} finalizado");

            ThreadIsRunning3 = false;
        }


        //
        //Nova versão vsintegra
        //

        static void Main(string[] args)
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;

            //
            // Verifico se ja não existe uma versão rodando
            //
            bool createdNew;

            Mutex mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Environment.Exit(0);
            }

            string connectionString =
                "server=mysql.vsisolucoes.com.br;" +
                "port=3306;" +
                "database=vsisolucoe153;" +
                "uid=vsisolucoe153;" +
                "pwd=V123456;" +
                "charset=utf8";

            worker = new Worker();
            worker.Name = appName;
            worker.Store = 0;
            worker.ConnectionString = connectionString;
            worker.IsRunning = true;

            workerService = new WorkerService(worker);

            timer1 = new Timer(new TimerCallback(Timer1_Tick), null, 0, 1000 * 60);

            Console.WriteLine(appName);
            Console.ReadKey();
        }



        //
        //Backup 12/05/2022
        //
        using Khronus.Framework.Core.Util;
using Khronus.Framework.DataAccess.ORM;
using Khronus.Framework.Integration;
using Khronus.Framework.Integration.Moskit;
using Khronus.Framework.Integration.Moskit.Model;
using Khronus.Framework.Integration.Moskit.Services;
using Khronus.Framework.Integration.Sga;
using Khronus.Framework.Integration.Sga.Model;
using Khronus.Framework.Integration.Sga.Services;
using Khronus.Framework.Integration.Sgr;
using Khronus.Framework.Integration.Sgr.Model;
using Khronus.Framework.Integration.Sgr.Services;
using Newtonsoft.Json.Linq;
using Sync.Custom.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using VSIntegra.Sync.Core;

namespace Sync.Custom
{
    class Program : WorkerBase
    {
        private static Timer timer1;
        private static Program worker;
        private static WorkerService workerService;

        private static void Main()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;

            //
            // Verifico se ja não existe uma versão rodando
            //
            bool createdNew;

            Mutex mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Environment.Exit(0);
            }

            string connectionString =
                "server=mysql.vsisolucoes.com.br;" +
                "port=3306;" +
                "database=vsisolucoe153;" +
                "uid=vsisolucoe153;" +
                "pwd=V123456;" +
                "charset=utf8";

            worker = new Program();
            worker.Name = appName;
            worker.Store = "0";
            worker.ConnectionString = connectionString;
            worker.IsRunning = true;

            workerService = new WorkerService(worker);

            worker.Logger.Log("inicializado");
            worker.Logger.Log("pressione qualquer tecla para fechar");

            timer1 = new Timer(new TimerCallback(Timer1_Tick), null, 0, 1000 * 60);

            Console.ReadLine();
        }

        private static void Timer1_Tick(object sender)
        {
            try
            {
                //
                // Injetar os parametros
                //
                Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();

                IEnumerable<ParametroBO> parametroBOList = worker.BusinessObjectManager.GetListByFilter<ParametroBO>($"SYSAH_FILIAL_COD={worker.Store}");
                foreach (ParametroBO parametroBO in parametroBOList)
                {
                    parameters.Add(parametroBO.SYSAH_NOME, parametroBO.SYSAH_VALOR);
                }

                worker.Parameters = parameters;

                workerService.Execute();
            }
            catch (Exception ex)
            {
                worker.Logger.Log(ex);
            }
        }

        private void SyncUsuario()
        {
            Object ret = null;

            Log($"SyncUsuario iniciado...");

            try
            {
                MoskitClient moskitClient = new MoskitClient();
                moskitClient.Token = Parameters["PARAM_0001"];

                UserService userService = new UserService(moskitClient);

                IEnumerable<User> users = userService.List();

                foreach (User user in users)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    string usuarioId = StringUtilities.ToString(user.Id);
                    string usuario = user.Name;
                    string email = user.Username;
                    bool active = user.Active;

                    Team team = user.Team;

                    TeamService teamService = new TeamService(moskitClient);
                    user.Team = (Team)teamService.Get(user.Team.Id);

                    UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_FILIAL_COD='{Store}' AND SYSAQ_NUMERO={usuarioId}");

                    bool found = usuarioBO != null;

                    if (!found)
                    {
                        usuarioBO = new UsuarioBO();

                        usuarioBO.SYSAQ_FILIAL_COD = NumberUtilities.parseInt(Store);
                        usuarioBO.SYSAQ_CODIGO = GetNumerator(Store, "sysaq");
                        usuarioBO.SYSAQ_NUMERO = usuarioId;
                        usuarioBO.SYSAQ_PARTICIPA_ROLETA = false;
                    }

                    //usuarioBO.SYSAQ_CODIGO = GetNumerator(Store, "sysaq");
                    usuarioBO.SYSAQ_USUARIO = usuario;
                    //usuarioBO.SYSAQ_EQUIPE_ID = team.Id;
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

                    //Thread.Sleep(1000);
                }

            }
            catch (Exception ex)
            {
                Log(ex);
            }

            Log($"SyncUsuario finalizado...");
        }

        private void SyncSinconizarIndicacao()
        {

            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {
                int quantity = 50;
                int start = 0;

                //
                // Busca origem do leads e fazer um de para com a indicação
                //
                do
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    JArray customFields = (JArray)SendCommand($"https://api.moskitcrm.com/v2/customFields/CF_49RM16ixiB7nbmBW/options?start={start}&quantity={quantity}", "GET", null, Parameters["PARAM_0001"]);

                    //JObject campos = (JObject)SendCommand("https://api.moskitcrm.com/v2/customFields/CF_49RM16ixiB7nbmBW/options", "GET", null, Parameters["PARAM_0001"]);

                    if (customFields.Count == 0)
                    {
                        return;
                    }

                    foreach (JObject customField in customFields)
                    {
                        if (!IsRunning)
                        {
                            return;
                        }

                        int id = NumberUtilities.parseInt(customField["id"]);
                        String descricao = StringUtilities.ToString(customField["label"]);

                        IndicacaoOrigemBO indicacaoOrigemBO = BusinessObjectManager.FindByFilter<IndicacaoOrigemBO>($"USUAK_FILIAL={Store} AND USUAK_NUMERO={id}");

                        bool found = indicacaoOrigemBO != null;

                        if (!found)
                        {
                            indicacaoOrigemBO = new IndicacaoOrigemBO();

                            indicacaoOrigemBO.USUAK_FILIAL = NumberUtilities.parseInt(Store);
                            indicacaoOrigemBO.USUAK_CODIGO = GetNumerator(Store, "usuak");
                        }

                        indicacaoOrigemBO.USUAK_NUMERO = id;
                        indicacaoOrigemBO.USUAK_DESCRICAO = descricao;
                        indicacaoOrigemBO.USUAK_CODIGO_SGR = 0;

                        object ret = null;

                        if (!found)
                        {
                            ret = BusinessObjectManager.Insert(indicacaoOrigemBO);
                        }
                        else
                        {
                            ret = BusinessObjectManager.Update(indicacaoOrigemBO);
                        }

                        if (ret != null)
                        {
                            throw new Exception(StringUtilities.ToString(ret));
                        }

                        //registro++;
                        Thread.Sleep(3000);
                    }
                    start++;

                } while (true);

                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_indicacao/688472668f481b3efbddb0bfbff99cf6");

                ////request.Headers.Add("Authorization", "Bearer " + token);

                ////request.Headers.Add("Accept", "application/json");                            
                //request.Headers.Add("X-Access-Token", "$2y$11$tHCYAIVEyGWUgKnNFFstvOG3OvnuIFX544pUguS/aCdr3K8RZQ5zq");
                //request.Headers.Add("Authorization", "8fc135598e7d0e87e7d1af222ab215c29ec94fcc");

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //if (response.StatusCode != HttpStatusCode.OK)
                //{
                //    throw new Exception(response.StatusDescription);
                //}
                //var resultActivities = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
            }
            catch (Exception ex)
            {

            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado");

        }

        private void SyncSinconizarPontoVenda()
        {

            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {

                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_ponto_venda/688472668f481b3efbddb0bfbff99cf6");

                //request.Headers.Add("Authorization", "Bearer " + token);

                //request.Headers.Add("Accept", "application/json");                            
                request.Headers.Add("X-Access-Token", "$2y$11$tHCYAIVEyGWUgKnNFFstvOG3OvnuIFX544pUguS/aCdr3K8RZQ5zq");
                request.Headers.Add("Authorization", "8fc135598e7d0e87e7d1af222ab215c29ec94fcc");
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(response.StatusDescription);
                }
                var resultActivities = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
            }
            catch (Exception ex)
            {

            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado");

        }

        public void SyncCriarErpSgrSgaLeadsGanhos()
        {
            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

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
                    object eventoRetorno = null;

                    if (statusDeal.Equals("won"))
                    {
                        //
                        //Busca codigo do vendedor
                        //
                        int codigoVendedor = 0;

                        UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_FILIAL_COD='{Store}' AND SYSAQ_NUMERO={evento["responsible"]["id"]}");

                        bool found = usuarioBO != null;

                        if (!found)
                        {
                            codigoVendedor = usuarioBO.SYSAQ_CODIGO_VENDEDOR;
                        }

                        if (codigoVendedor == 0)
                        {
                            continue;
                        }

                        //
                        //Busca dados do contato
                        //
                        long contatoId = NumberUtilities.parseLong(evento["data"]["contacts"][0]["id"]);

                        MoskitClient moskitClient = new MoskitClient();
                        moskitClient.Token = Parameters["PARAM_0001"];

                        ContactService contactService = new ContactService(moskitClient);

                        Contact contact = (Contact)contactService.Get(contatoId);

                        string nome = contact.Name;
                        string telefone = contact.Phones == null ? "" : contact.Phones[0].Number;
                        string celular = contact.Phones.Length > 0 ? "" : contact.Phones[1].Number;
                        string email = contact.Emails == null ? "" : contact.Emails[0].Address;

                        //List<Object> entityCustomFields = new List<Object>(contact.EntityCustomFields);
                        //string cpfCnpj = "";

                        ////var customFieldsContact = contact.EntityCustomFields;

                        //foreach (JObject customField in entityCustomFieldsContact)
                        //{
                        //    if (customField["id"].ToString().Equals("CF_0WGqoGSKC9zK2qnP"))
                        //    {
                        //        cpfCnpj = StringUtilities.ToString(customField["textValue"]);

                        //        break;
                        //    }
                        //}

                        //JToken customFieldValue = entityCustomFields.SelectToken("$.[?(@.id == 'CF_0WGqoGSKC9zK2qnP')]", true);

                        List<EntityCustomFields> entityCustomFields = new List<EntityCustomFields>(contact.EntityCustomFields);

                        string origem = "";
                        int codPontoVenda = 0;
                        string profissaoClienteVenda = "";
                        string sexo = "N";
                        string rg = "400531732"; //String
                        string dataNascimento = "17/11/1987";
                        string logradouro = "Teste 222";
                        string numero = "122"; //String
                        string complemento = "teste";
                        string bairro = "teste";
                        string cidade = "Sorocoba";
                        string estado = "SP";
                        string cep = "18.071-095";
                        string cpfCnpj = "";
                        string placa = "fdd-4444";
                        string anofabVeiculoVenda = "2020";
                        string anoModeloVeiculoVenda = "2021";
                        string chassiVeiculoVenda = "123";
                        decimal fipeValorVeiculoVenda = 123;
                        decimal valorParcelaVenda = 123;
                        string renavamVeiculoVenda = "123";
                        string contatoClienteVenda = "";
                        int quantidadeParcelaVenda = 1;
                        string obs = "";

                        EntityCustomFields customField = entityCustomFields.Find(e => e.Id == "CF_0WGqoGSKC9zK2qnP");

                        if (customField != null)
                        {
                            cpfCnpj = entityCustomFields.Find(e => e.Id == "CF_0WGqoGSKC9zK2qnP").TextValue;
                        }

                        customField = entityCustomFields.Find(e => e.Id == "CF_6rRmwGSvC6jZKm4X");

                        if (customField != null)
                        {
                            rg = entityCustomFields.Find(e => e.Id == "CF_6rRmwGSvC6jZKm4X").TextValue;
                        }

                        customField = entityCustomFields.Find(e => e.Id == "CF_42AmakSZCwrAPqjl");

                        if (customField != null)
                        {
                            cidade = entityCustomFields.Find(e => e.Id == "CF_42AmakSZCwrAPqjl").TextValue;
                        }

                        customField = entityCustomFields.Find(e => e.Id == "CF_wPVm2oS2Cbj9gDK6");

                        if (customField != null)
                        {
                            bairro = entityCustomFields.Find(e => e.Id == "CF_wPVm2oS2Cbj9gDK6").TextValue;
                        }

                        customField = entityCustomFields.Find(e => e.Id == "CF_Pj3qYoSeCrBpamQe");

                        if (customField != null)
                        {
                            cep = entityCustomFields.Find(e => e.Id == "CF_Pj3qYoSeCrBpamQe").TextValue;
                        }

                        customField = entityCustomFields.Find(e => e.Id == "CF_Rg7Mn4SLCA1XrDvd");

                        if (customField != null)
                        {
                            numero = entityCustomFields.Find(e => e.Id == "CF_Rg7Mn4SLCA1XrDvd").TextValue;
                        }

                        customField = entityCustomFields.Find(e => e.Id == "CF_6rRmwGS9i6jZpm4X");

                        if (customField != null)
                        {
                            logradouro = entityCustomFields.Find(e => e.Id == "CF_ylAm0KS6C5p91Mvb").TextValue;
                        }

                        customField = entityCustomFields.Find(e => e.Id == "CF_6rRmwGS9i6jZpm4X");

                        if (customField != null)
                        {
                            long option = 0;
                            option = entityCustomFields.Find(e => e.Id == "CF_6rRmwGS9i6jZpm4X").Options[0];

                            //
                            //Busco options de estados
                            //

                            JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_6rRmwGS9i6jZpm4X"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                            estado = StringUtilities.ToString(optionS["label"]);
                        }

                        //customField = entityCustomFields.Find(e => e.Id == "CF_Rg7Mn4SLCA1XrDvd");

                        //if (customField != null)
                        //{
                        //    numero = entityCustomFields.Find(e => e.Id == "CF_Rg7Mn4SLCA1XrDvd").TextValue;
                        //}

                        //JToken customFieldValue = entityCustomFields.SelectToken("$.[?(@.id == 'CF_0WGqoGSKC9zK2qnP')]", true);

                        //string nomeLoja = StringUtilities.ToString(row["line_items"][0]["meta_data"].SelectToken("[?(@.key == '_vendor_id')]")["value"]);

                        //
                        //Campo customizado Lead
                        //
                        //List<Object> entityCustomFields2 = new List<Object>(evento["data"]["customFieldValues"]);

                        ////
                        //// Buscar campo customizado ASSOCIADO ou CLIENTE/RASTREADOR
                        ////
                        //JArray entityCustomFields2 = JArray.Parse(evento["data"]["customFieldValues"].ToString());

                        //string tipoAssociado = "";

                        //foreach (JObject customField2 in entityCustomFields2)
                        //{
                        //    string idCustom = StringUtilities.ToString(customField2["customField"]["id_v2"]);

                        //    if (idCustom.Equals("CF_y5lm56iyiY4L8DwW"))
                        //    {
                        //        tipoAssociado = StringUtilities.ToString(customField2["label"]);

                        //        break;
                        //    }
                        //}

                        //
                        //Campo customizado Lead
                        //
                        //List<Object> entityCustomFields2 = new List<Object>(evento["data"]["customFieldValues"]);

                        //
                        // Buscar campo customizado ASSOCIADO ou CLIENTE/RASTREADOR
                        //
                        JArray entityCustomFields2 = JArray.Parse(evento["data"]["customFieldValues"].ToString());

                        string tipoAssociado = "";

                        long codIndicacao = 0;

                        foreach (JObject customField2 in entityCustomFields2)
                        {
                            string idCustom = StringUtilities.ToString(customField2["customField"]["id_v2"]);

                            if (idCustom.Equals("CF_49RM16ixiB7nbmBW"))
                            {
                                //codIndicacao = NumberUtilities.parseInt(customField2["label"]);

                                //
                                //buscar indicação do lead
                                //
                                IndicacaoOrigemBO indicacaoOrigemBO = BusinessObjectManager.FindByFilter<IndicacaoOrigemBO>($"USUAK_NUMERO={NumberUtilities.parseInt(customField2["value"])}");

                                //bool 
                                found = indicacaoOrigemBO != null;

                                if (found)
                                {
                                    codIndicacao = indicacaoOrigemBO.USUAK_CODIGO_SGR;
                                }

                                break;
                            }
                            else if (idCustom.Equals("CF_dVKmQ5i1CdPXwmWR"))
                            {
                                cpfCnpj = StringUtilities.ToString(customField2["value"]);

                                break;
                            }
                            else if (idCustom.Equals("CF_dVKmQ5i1CdPXwmWR"))
                            {
                                cep = StringUtilities.ToString(customField2["value"]);

                                break;
                            }
                            else if (idCustom.Equals("CF_wPVm2VijibjB8mK6"))
                            {
                                long option = NumberUtilities.parseInt(customField2["value"]);

                                //
                                //Busco options de tipo rastreador
                                //
                                JObject options = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_wPVm2VijibjB8mK6"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                string descTipoRastreados = StringUtilities.ToString(options["label"]);

                                //NumberUtilities.parseInt(options["label"]);

                                break;
                            }
                            else if (idCustom.Equals("CF_QJXmA5iXiJEBpm25"))
                            {
                                long option = NumberUtilities.parseInt(customField2["value"]);

                                //
                                //Busco options de ponto de venda
                                //
                                JObject options = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_6rRmwGS9i6jZpm4X"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                string nomePontoVenda = StringUtilities.ToString(options["label"]);

                                //NumberUtilities.parseInt(options["label"]);

                                codPontoVenda = nomePontoVenda.Equals("PONTO TRACK") ? 1 :
                                                nomePontoVenda.Equals("AMERICA CLUBE DE BENEFICIOS") ? 2 :
                                                nomePontoVenda.Equals("MARINGA") ? 3 :
                                                nomePontoVenda.Equals("ASSOCIACAO MUTUALISTA VIA SUL") ? 4 :
                                                nomePontoVenda.Equals("CURITIBA") ? 5 : 0;
                                break;
                            }



                        }

                        //
                        //Consultar produto
                        //
                        JArray products = (JArray)evento["data"]["productDeals"];
                        string nomeProduto = "";

                        foreach (JObject product in products)
                        {
                            nomeProduto = StringUtilities.ToString(product["product"][0]["name"]);
                        }

                        //DealService dealService = new DealService(moskitClient);

                        ////JArray deal1 = (JArray) $"[{dealId}]";

                        //JArray filter = JArray.Parse($"[{dealId}]");

                        //IEnumerable<Deal> deals = dealService.List(filter);

                        //foreach (Deal deal in deals)
                        //{
                        //    //Deal deal = (Deal)dealService.List(filter);


                        //    //
                        //    //Campos customizados do negócio
                        //    //
                        //    List<EntityCustomFields> entityCustomFieldsDeal = new List<EntityCustomFields>(deal.EntityCustomFields);

                        //    customField = entityCustomFields.Find(e => e.Id == "CF_dVKmQ5i1CdPXwmWR");

                        //    if (customField != null)
                        //    {
                        //        cpfCnpj = entityCustomFields.Find(e => e.Id == "CF_dVKmQ5i1CdPXwmWR").TextValue;
                        //    }

                        //    customField = entityCustomFields.Find(e => e.Id == "CF_49RM16ixiB7nbmBW");

                        //    long codIndicacao = 0;

                        //    if (customField != null)
                        //    {

                        //        long option = entityCustomFields.Find(e => e.Id == "CF_49RM16ixiB7nbmBW").Options[0];

                        //        //
                        //        //Busco options de estados
                        //        //
                        //        JObject options = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_6rRmwGS9i6jZpm4X"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                        //        origem = StringUtilities.ToString(options["label"]);

                        //        //
                        //        //buscar indicação do lead
                        //        //
                        //        IndicacaoOrigemBO indicacaoOrigemBO = BusinessObjectManager.FindByFilter<IndicacaoOrigemBO>($"USUAK_NUMERO={option}");

                        //        bool found = indicacaoOrigemBO != null;

                        //        if (!found)
                        //        {
                        //            codIndicacao = indicacaoOrigemBO.USUAK_CODIGO_SGR;
                        //        }
                        //    }

                        //}



                        ServiceReturn serviceReturn = new ServiceReturn();

                        //if (tipoAssociado.Equals("ASSOCIADO"))
                        if (nomeProduto.Contains("Plano"))
                        {
                            SgaClient sgaClient = new SgaClient();
                            sgaClient.Token = Parameters["PARAM_0004"];

                            AssociadoService associadoService = new AssociadoService(sgaClient);

                            Associado associado = associadoService.GetByCpf(cpfCnpj);

                            if (associado == null)
                            {
                                associado = new Associado();

                                associado.Nome = nome;
                                associado.Sexo = "M";
                                associado.Cpf = cpfCnpj;
                                associado.Rg = rg;
                                associado.DataNascimento = dataNascimento;
                                associado.Telefone = telefone;
                                associado.Celular = celular;
                                associado.Email = email;
                                associado.Logradouro = logradouro;
                                associado.Numero = numero; //String
                                associado.Complemento = complemento;
                                associado.Bairro = bairro;
                                associado.Cidade = cidade;
                                associado.Estado = estado;
                                associado.Cep = cep;

                                //associado.Nome = "TESTE VSI SOLUCOES 500";
                                //associado.Sexo = "M";
                                //associado.Cpf = cpfCnpj;
                                //associado.Rg = "400531732"; //String
                                //associado.DataNascimento = "17/11/1987";
                                //associado.Telefone = "(15) 3313-2514";
                                //associado.Celular = "(15) 998100-5172";
                                //associado.Email = "teste@vsisolucoes.com.br";
                                //associado.Logradouro = "Teste 222";
                                //associado.Numero = "122"; //String
                                //associado.Complemento = "teste";
                                //associado.Bairro = "teste";
                                //associado.Cidade = "Sorocoba";
                                //associado.Estado = "SP";
                                //associado.Cep = "18.071-095";

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

                                if (serviceReturn.Status == 500)
                                {
                                    status = "10";
                                    motivo = serviceReturn.Message;
                                    eventoRetorno = serviceReturn.Detail;
                                }
                            }
                        }
                        //else if (tipoAssociado.Equals("CLIENTE/RASTREADOR"))
                        //else if (nomeProduto.Equals("Rastreamento"))    <---- isso nao da certo
                        else if (nomeProduto.ToUpper().Equals("RASTREAMENTO"))
                        {
                            string cCliente = null;

                            cpfCnpj = StringUtilities.Clear(cpfCnpj, ""); // <-- precisa passar sem os tracos

                            //
                            //Cadastro de cliente
                            //
                            SgrClient sgrClient = new SgrClient();
                            sgrClient.Token = Parameters["PARAM_0009"]; //;"688472668f481b3efbddb0bfbff99cf6"; // 
                            sgrClient.ApiKey = Parameters["PARAM_0010"]; //;"3569"; //
                            sgrClient.Username = Parameters["PARAM_0011"]; // ; "moskit"; // 
                            sgrClient.Password = Parameters["PARAM_0012"]; //;"2SI7WG"; // 

                            ClienteService clienteService = new ClienteService(sgrClient);

                            Cliente cliente = clienteService.Get(cpfCnpj);
                            if (cliente == null)
                            {
                                cliente = new Cliente();
                                cliente.NomeCliente = nome;
                                cliente.CpfCliente = cpfCnpj;
                                cliente.EnderecoClienteLogradouro = logradouro;
                                cliente.EnderecoClienteNumero = numero;
                                cliente.EnderecoClienteBairro = bairro;
                                cliente.EnderecoClienteCidade = cidade;
                                cliente.EnderecoClienteEstado = estado;
                                cliente.EnderecoClienteCep = cep;
                                cliente.CodSituacaoCliente = 1;
                                cliente.CodMatrizFilialCliente = 4;
                                cliente.FormatoEnvioTituloCliente = 198;
                                cliente.FormatoBoletoCliente = "U";

                                serviceReturn = clienteService.Post(cliente);

                                if (serviceReturn.Status == 200)
                                {
                                    cCliente = JObject.Parse(serviceReturn.Message)["cod_cliente"].ToString();
                                }
                                else
                                {
                                    status = "20";
                                    motivo = serviceReturn.Message;
                                    eventoRetorno = serviceReturn.Detail;
                                }
                            }
                            else
                            {
                                cCliente = cliente.CodCliente;
                            }

                            if (cCliente != null)
                            {

                                PedidoService pedidoService = new PedidoService(sgrClient);

                                Pedido pedido = new Pedido();
                                //pedido.CodIndicacaoVenda = codIndicacao;
                                //pedido.CodCombustivelVeiculoVenda = 209;
                                //pedido.CodFormaPagamentoVenda = 201;
                                //pedido.CodFormaPagamentoAdesaoVenda = 201;
                                //pedido.CodGrupoVenda = 1;
                                //pedido.CodGrupoAdesaoVenda = 1002;
                                //pedido.CodPeriodoVenda = 336;
                                //pedido.CodVencimentoVenda = 281;
                                pedido.CodClienteVenda = NumberUtilities.parseInt(cCliente);
                                pedido.CpfClienteVenda = cpfCnpj;
                                //pedido.RgClienteVenda = rg;
                                //pedido.PlacaVeiculoVenda = placa;
                                //pedido.AnoFabVeiculoVenda = anofabVeiculoVenda;
                                //pedido.AnomodVeiculoVenda = anoModeloVeiculoVenda;
                                //pedido.ChassiVeiculoVenda = chassiVeiculoVenda;
                                //pedido.FipeValorVeiculoVenda = fipeValorVeiculoVenda;
                                //pedido.RenavamVeiculoVend = renavamVeiculoVenda;
                                //pedido.DataNascimentoClienteVenda = dataNascimento;
                                //pedido.ContatoClienteVenda = contatoClienteVenda;
                                //pedido.ProfissaoClienteVenda = profissaoClienteVenda;
                                ////pedido.interveniente_venda =;
                                //pedido.QuantidadeParcelaVenda = quantidadeParcelaVenda;
                                //pedido.QuantidadeParcelaAdesaoVenda = quantidadeParcelaVenda;
                                //pedido.ValorParcelaVenda = valorParcelaVenda / quantidadeParcelaVenda;
                                //pedido.ValorParcelaAdesaoVenda = valorParcelaVenda / quantidadeParcelaVenda;
                                //pedido.EntradaVenda = 1;
                                //pedido.ObservacaoVenda = obs;
                                //pedido.SexoClienteVenda = "M";
                                //pedido.EstadoCivilClienteVenda = "SO";
                                pedido.NomeClienteVenda = nome;
                                //pedido.ProdutoVendaCodProduto = 1;
                                //pedido.CodPontoVendaVenda = 1;
                                //pedido.CodDepartamento = 242;
                                //pedido.CodFormaPagamentoVenda = 201;

                                pedido.CodPontoVendaVenda = codPontoVenda; //1; //Sera informado no moskit
                                pedido.CodConsultorVenda = codigoVendedor; // 1; //Pega da amarracao MOSKIT x SGR

                                serviceReturn = pedidoService.Post(pedido);

                                if (serviceReturn.Status == 500)
                                {
                                    status = "20";
                                }

                                motivo = serviceReturn.Message;
                                eventoRetorno = serviceReturn.Detail;
                            }
                        }
                    }

                    //Atualiza o evento   
                    object ret = BusinessObjectManager.UpdateFields(eventoBO,
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
                Log(ex);
            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado...");
        }

        private void SyncMoskitWhatapp()
        {
            object ret = null;

            Log($"SyncMoskitWhatapp iniciado...");

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
                        request.Headers.Add("Authorization", "Bearer " + Parameters["PARAM_0005"]);
                        request.Method = "POST";
                        request.Accept = "application/json";
                        request.ContentType = "application/json";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            string json = "{" + $"\"number\":\"{telefone}\",\"instanceid\":\"{instanceid}\"" + "}";

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

                            atendimento = (JArray)JObject.Parse(StringUtilities.ToString(result))["atendimentos"];

                            foreach (var att in atendimento)
                            {

                                string getAtendimentoId = StringUtilities.ToString(att["id"]);

                                if (getAtendimentoId.Equals(idAtendimento))
                                {
                                    agenteId = StringUtilities.ToString(att["id_agente"]);

                                    JObject tags = JObject.Parse(StringUtilities.ToString(att["tags"]));

                                    agenteNome = StringUtilities.ToString(((Newtonsoft.Json.Linq.JProperty)tags.First).Value);

                                    //string agente2 = StringUtilities.ToString(tags.SelectToken("$.[?(@.AGENTE == \"21810\")]")["id_agente"]);
                                    //JObject.Parse(StringUtilities.ToString(eventoBO.USUAN_EVENTO));

                                    break;

                                }
                            }

                            eventoRetorno = "{" + $"\"Id atendimento\": \"{idAtendimento}\", \"Id Agente\":\"{agenteId}\", \"Nome agente\": \"{agenteNome}\" " + "}";

                        }

                        long usuarioId = NumberUtilities.parseLong(Parameters["PARAM_0003"]);

                        UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_AGENTE_ID = '{agenteId}' OR SYSAQ_USUARIO like '%{agenteNome}%'");

                        bool found = usuarioBO != null;

                        if (found)
                        {
                            //status = "20";
                            //motivo = "Processo não executado.";

                            if (usuarioBO.SYSAQ_AGENTE_ID == null)
                            {
                                usuarioBO.SYSAQ_AGENTE_ID = agenteId;

                                ret = BusinessObjectManager.Update(usuarioBO);

                                if (ret != null)
                                {
                                    break;
                                }
                            }

                            usuarioId = NumberUtilities.parseLong(usuarioBO.SYSAQ_NUMERO);
                        }

                        MoskitClient moskitClient = new MoskitClient();
                        moskitClient.Token = Parameters["PARAM_0001"];

                        //DealService dealService = new DealService(moskitClient);

                        ServiceReturn serviceReturn = new ServiceReturn();

                        //
                        // Criar contato
                        //
                        ContactService contactService = new ContactService(moskitClient);

                        Contact contact = (Contact)contactService.GetByFone(telefone);

                        long contactId = 0;

                        if (contact == null)
                        {
                            //telefone = telefone.Substring(2);

                            contact = (Contact)contactService.GetByFone(telefone.Substring(2));
                        }

                        //string telefoneNovo = telefone.Substring(2);

                        if (contact == null)
                        {
                            //if (String.IsNullOrWhiteSpace(nome))
                            //{
                            //    nome = telefone;
                            //}

                            contact = new Contact();
                            contact.DateCreated = DateTime.Now;
                            contact.Name = String.IsNullOrWhiteSpace(nome) ? telefone : nome;

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
                            string origem = contact.Origin;

                            if (origem.Equals("API V2"))
                            {
                                var param = JArray.Parse("[{\"field\": \"phones\",\"expression\": \"like\", \"values\": [\"" + telefone + "\" ]}]");

                                JArray results = (JArray)SendCommand("https://api.moskitcrm.com/v2/contacts/search", "POST", param, Parameters["PARAM_0001"]);

                                if (results.Count > 0)
                                {
                                    //
                                    // Se existe contato Atualizo o Contato
                                    //
                                    JObject contactUpdate = (JObject)results[0];

                                    //string origem = StringUtilities.ToString(contactUpdate["origin"]);
                                    //contactId = NumberUtilities.parseLong(contact.Id);


                                    contactId = NumberUtilities.parseLong(contactUpdate["id"]);

                                    contactUpdate["name"] = String.IsNullOrWhiteSpace(nome) ? contactUpdate["name"] : nome;
                                    contactUpdate["responsible"]["id"] = usuarioId;

                                    contactUpdate = (JObject)SendCommand($"https://api.moskitcrm.com/v2/contacts/{contactId}", "PUT", contactUpdate, Parameters["PARAM_0001"]);
                                }

                            }

                        }

                        status = "10";
                        motivo = "Processado";

                        ////
                        ////Criar Negócio
                        ////
                        //JArray filter = JArray.Parse("[{\"field\": \"CF_lXODObiYCeXZGmaN\",\"expression\": \"match\",\"values\": [\"" + idChat + "\" ]}]");

                        ////JArray deals = (JArray)SendCommand($"https://api.moskitcrm.com/v2/deals/search", "POST", param, Parameters["PARAM_0001"]);
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
                        //    deal.Stage = new Identity() { Id = NumberUtilities.parseLong(Parameters["PARAM_0002"]) }; //180220 - NOVO
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

                        //            JObject nota = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{dealId}/notes", "POST", notes, Parameters["PARAM_0001"]);
                        //        }

                        //        //}
                        //    }

                        //    eventoRetorno = serviceReturn.Message.ToString();
                        //    status = "10";
                        //    motivo = "Processo executado com sucesso";
                        //}

                        //}

                        //}
                        //}

                        //
                        //Distribuir usuário
                        //
                        //int usuarioId = RoletaUsuario(null);
                        //}
                    }
                    catch (Exception ex)
                    {
                        status = "20";
                        motivo = "Processo com falhas";

                        Log(ex);

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
                Log(ex);
            }

            Log($"SyncMoskitWhatapp finalizado...");

        }

        private void SyncEnviarMensagem()
        {
            object ret = null;

            Log($"SyncAtualizaWhatapp iniciado...");

            try
            {
                MoskitClient moskitClient = new MoskitClient();

                moskitClient.Token = Parameters["PARAM_0001"];

                DealService dealService = new DealService(moskitClient);

                JArray filter = JArray.Parse("[{\"field\": \"status\",\"expression\": \"one_of\",\"values\": [\"OPEN\" ]}]");

                ServiceReturn serviceReturn = dealService.GetByFilter(filter);

                if (serviceReturn.Status == 200)
                {
                    List<Deal> deals = (List<Deal>)serviceReturn.Detail;

                    //Deal deal = deals.Count == 0 ? new Deal() : deals[0];

                    foreach (Deal deal in deals)
                    {
                        string idContact = StringUtilities.ToString(deal.Contacts[0].Id);

                        DateTime dataInteracao = (DateTime)deal.DateCreated;

                        DateTime dataAtual = DateTime.Now;

                        int ultimaInteracao = NumberUtilities.parseInt(dataAtual.Day - dataInteracao.Day);

                        ContactService contactService = new ContactService(moskitClient);
                        //ServiceReturn serviceReturn = new ServiceReturn();

                        Contact contact = (Contact)contactService.Get(idContact);

                        string nomeContato = StringUtilities.ToString(contact.Name);

                        string telefoneContato = StringUtilities.ToString(contact.Phones[0].Number);

                        string telefone = "5515997409919";

                        string mensagem = "";

                        if (ultimaInteracao == 2)
                        {
                            mensagem = Parameters["PARAM_0006"];
                        }
                        else if (ultimaInteracao == 10)
                        {
                            mensagem = Parameters["PARAM_0007"];
                        }
                        else if (ultimaInteracao == 30)
                        {
                            mensagem = Parameters["PARAM_0008"];
                        }

                        mensagem = mensagem.Replace("[nome-cliente]", nomeContato);

                        HttpWebRequest request = null;

                        //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/external/getAtendimentos");
                        //request.Headers.Add("Authorization", "Bearer " + Parameters["WHATSAPP_TOKEN"]);
                        //request.Method = "POST";
                        //request.ContentType = "application/json";

                        //var data = @"{""number"": ""5515997409919"",""instanceid"": ""12""}";

                        //using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        //{
                        //    streamWriter.Write(data);
                        //}

                        //var httpResponse = (HttpWebResponse)request.GetResponse();

                        //int numeroAtendimento = 0;
                        //DateTime? dateStartAtend = null;

                        //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        //{
                        //    var result = streamReader.ReadToEnd();

                        //    JArray atendimento = (JArray)JObject.Parse(StringUtilities.ToString(result))["atendimentos"];

                        //    foreach (var att in atendimento)
                        //    {
                        //    }

                        //    dateStartAtend = (DateTime)atendimento[0]["date_start"];

                        //    numeroAtendimento = NumberUtilities.parseInt(atendimento[0]["id"]);
                        //}

                        var url = "https://pontotrack.ipsolutiontelecom.com.br:5001/external/sendMessage";

                        //
                        //Cria um atendimento em Orçamento
                        //
                        request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/external/createAtendimento");

                        request.Method = "POST";
                        request.Headers.Add("Authorization", "Bearer " + Parameters["WHATSAPP_TOKEN"]);
                        request.ContentType = "application/json";

                        var data = @"{""numero"": """ + telefone + @""",""instanceid"": """ + "12" + @""",""id_bot"": ""16"",""id_agente"": ""1178"",""id_queue"": ""65""}";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(data);
                        }

                        //var data = "{" + $"\"number\":\"{telefone}\",\"message\":\"{mensagem}\",\"canal_id\": \"8\", \"id_atendimento:\"21725" + "}";

                        var
                        httpResponse = (HttpWebResponse)request.GetResponse();

                        string idAtendimento = "37017";
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            //JObject atendimentos = JObject.Parse(StringUtilities.ToString(result));

                            //idAtendimento = StringUtilities.ToString(atendimentos["atendimento"]["id"]);

                            idAtendimento = StringUtilities.ToString(JObject.Parse(StringUtilities.ToString(result))["atendimento"]["id"]);
                        }


                        //HttpWebRequest 
                        request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/external/sendMessage");

                        //var request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "POST";

                        //request.Headers["Authorization"] = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6NDcsImxvZ2luIjoiaXBzb2x1dGlvbiIsImVtYWlsIjoiIiwiY2hhbm5lbCI6e30sImlhdCI6MTY0NDQ0OTQwMCwiZXhwIjoxNjc1OTg1NDAwfQ.r4bfbt-D-T79HuCYT1Dkt512VbzieLEdnSMclDwUA50";
                        request.Headers.Add("Authorization", "Bearer " + Parameters["WHATSAPP_TOKEN"]);
                        request.ContentType = "application/json";

                        //string mensagem = Parameters["PARAM_0001"];
                        //var 
                        data = @"{""number"": """ + telefone + @""",""message"": """ + mensagem + @""",""canal_id"": ""8"",""id_atendimento"": """ + idAtendimento + @"""}";

                        //var json = "{" + $"\"number\":\"{telefone}\",\"message\":\"{mensagem}\",\"canal_id\": \"8\", \"id_atendimento:\"21725" + "}";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(data);
                        }

                        //var data = "{" + $"\"number\":\"{telefone}\",\"message\":\"{mensagem}\",\"canal_id\": \"8\", \"id_atendimento:\"21725" + "}";

                        //var 
                        httpResponse = (HttpWebResponse)request.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                        }

                        //
                        //Enviar imagem e video
                        //
                        request = (HttpWebRequest)WebRequest.Create($"https://pontotrack.ipsolutiontelecom.com.br:5001/external/sendFile");

                        //var request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "POST";

                        //request.Headers["Authorization"] = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6NDcsImxvZ2luIjoiaXBzb2x1dGlvbiIsImVtYWlsIjoiIiwiY2hhbm5lbCI6e30sImlhdCI6MTY0NDQ0OTQwMCwiZXhwIjoxNjc1OTg1NDAwfQ.r4bfbt-D-T79HuCYT1Dkt512VbzieLEdnSMclDwUA50";
                        request.Headers.Add("Authorization", "Bearer " + Parameters["WHATSAPP_TOKEN"]);
                        request.ContentType = "application/json";

                        //string mensagem = Parameters["PARAM_0001"];
                        //var 
                        data = @"{""number"": """ + telefone + @""",""caption"": ""Orçamento"",""fileUrl"": ""https://vsisolucoes.com.br/clientes/pontotrack/img/pontotrack.jpeg"",""id_atendimento"":" + idAtendimento + @",""filename"": ""Pontotrack"",""canal_id"": 8}";

                        //var json = "{" + $"\"number\":\"{telefone}\",\"message\":\"{mensagem}\",\"canal_id\": \"8\", \"id_atendimento:\"21725" + "}";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(data);
                        }

                        //var data = "{" + $"\"number\":\"{telefone}\",\"message\":\"{mensagem}\",\"canal_id\": \"8\", \"id_atendimento:\"21725" + "}";

                        //var 
                        httpResponse = (HttpWebResponse)request.GetResponse();

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                        }

                    }


                }

            }
            catch (Exception ex)
            {
                Log(ex);
            }

            Log($"SyncAtualizaWhatapp finalizado...");

        }

        private void SyncAtualizaWhatapp()
        {
            object ret = null;

            Log($"SyncAtualizaWhatapp iniciado...");

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

                    JObject nota = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{dealId}/notes", "POST", notes, Parameters["PARAM_0001"]);

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
                Log(ex);
            }

            Log($"SyncAtualizaWhatapp finalizado...");

        }
    }

    namespace Entities
    {
        class EventoBO : EventoVO
        {
            //public override Object Insert()
            //{
            //    if(this.USUAN_CODIGO < 1)
            //    {
            //        this.USUAN_CODIGO = Worker.GetInstance().GetNumerator(this.USUAN_FILIAL, this.GetType().Name);
            //    }            

            //    Object ret = DataAccessManager.Insert(this);

            //    return ret;
            //}

            //public override Object Update()
            //{
            //    return Worker.GetInstance().DataAccessManager.Update(this);
            //}

            //public IntegracaoBO Find(int param1, String param2)
            //{
            //    return (IntegracaoBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_ID", "'" + param2 + "'" } });
            //}

            //public IntegracaoBO FindByNumero(int param1, String param2)
            //{
            //    return (IntegracaoBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_NUMERO", "'" + param2 + "'" } });
            //}

            //public IntegracaoBO FindByEvento(int param1, int param2, String param3)
            //{
            //    return (IntegracaoBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_TIPO", param2 }, { "USUAN_EVENTO", "'" + param2 + "'" } });
            //}


            //public DataTable GetList(int param1, String param2)
            //{
            //    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_STATUS", "'" + param2 + "'" } });
            //}

            //public DataTable GetList(int param1, int param2, String param3)
            //{
            //    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_TIPO", param2 }, { "USUAN_STATUS", "'" + param3 + "'" } });
            //}
        }

        [Table("usuan")]
        class EventoVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String USUAN_ID { get; set; }

            [Field]
            public int USUAN_FILIAL { get; set; }

            [Field]
            public int USUAN_CODIGO { get; set; }

            [Field]
            public int USUAN_TIPO { get; set; }

            [Field]
            public DateTime USUAN_DATA { get; set; }

            [Field]
            public String USUAN_EVENTO { get; set; }

            [Field]
            public String USUAN_STATUS { get; set; }

            [Field]
            public String USUAN_STATUS_MOTIVO { get; set; }

            [Field]
            public String USUAN_EVENTO_RETORNO { get; set; }
        }

        class IndicacaoOrigemBO : IndicacaoOrigemVO
        {

        }

        [Table("usuak")]
        class IndicacaoOrigemVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String USUAK_ID { get; set; }

            [Field]
            public int USUAK_FILIAL { get; set; }

            [Field]
            public int USUAK_CODIGO { get; set; }

            [Field]
            public int USUAK_NUMERO { get; set; }

            [Field]
            public string USUAK_DESCRICAO { get; set; }

            [Field]
            public int USUAK_CODIGO_SGR { get; set; }

            [Field]
            public bool USUAK_INATIVO { get; set; }
        }

        class NegocioBO : NegocioVO
        {
            //public override Object Insert()
            //{
            //    //if (this.USUAM_CODIGO < 1)
            //    //{
            //    //    this.USUAM_CODIGO = Worker.GetInstance().GetNumerator(this.USUAM_FILIAL, this.GetType().Name);
            //    //}

            //    //Object ret = Worker.GetInstance().DataAccessManager.Insert(this);
            //    Object ret = null;
            //    return ret;
            //}

            //public override Object Update()
            //{
            //    return Worker.GetInstance().DataAccessManager.Update(this);
            //}

            //public LojaBO Find(int param1, String param2)
            //{
            //    return (LojaBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAM_FILIAL", param1 }, { "USUAM_ID", "'" + param2 + "'" } });
            //}

            //public LojaBO FindByNome(int param1, String param2)
            //{
            //    return (LojaBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAM_FILIAL", param1 }, { "USUAM_NOME", "'" + param2 + "'" } });
            //}

            //public DataTable GetList(int param1, bool param2)
            //{
            //    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "USUAM_FILIAL", param1 }, { "USUAM_INATIVO", param2 } });
            //}

        }

        [Table("gerau")]
        class NegocioVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String GERAU_ID { get; set; }

            [Field]
            public int GERAU_FILIAL { get; set; }

            [Field]
            public int GERAU_CODIGO { get; set; }

            [Field]
            public String GERAU_NOME_PROPRIO { get; set; }

            [Field]
            public String GERAU_EMAIL { get; set; }

            [Field]
            public String GERAU_FONE_PRINCIPAL { get; set; }

            [Field]
            public String GERAU_FONE { get; set; }

            [Field]
            public String GERAU_TP_ENTRADA { get; set; }

            [Field]
            public String GERAU_CURSO { get; set; }

            [Field]
            public String GERAU_TP_SIAA { get; set; }

            [Field]
            public String GERAU_ORIGEM_LEAD { get; set; }
            [Field]
            public DateTime GERAU_DT_PROVA { get; set; }

            [Field]
            public DateTime GERAU_DT_INSCRICAO { get; set; }

            [Field]
            public String GERAU_SITUACAO { get; set; }

            [Field]
            public String GERAU_UNIDADE { get; set; }

            [Field]
            public String GERAU_CPF { get; set; }

            [Field]
            public DateTime GERAU_DT_MATRICULA { get; set; }

            [Field]
            public String GERAU_IES { get; set; }

            [Field]
            public String GERAU_RGM { get; set; }

            [Field]
            public String GERAU_RESPONSAVEL { get; set; }

            [Field]
            public String GERAU_ETAPA { get; set; }

            [Field]
            public String GERAU_STATUS { get; set; }

            [Field]
            public String GERAU_TAG { get; set; }

            [Field]
            public String GERAU_WHATSAPP_API { get; set; }

            [Field]
            public String GERAU_INDICADOR_NOME { get; set; }

            [Field]
            public String GERAU_INDICADOR_EMAIL { get; set; }

            [Field]
            public String GERAU_INDICADOR_TELEFONE { get; set; }

            [Field]
            public String GERAU_INDICADOR_PIX { get; set; }

            [Field]
            public String GERAU_INDICADOR_CODIGO { get; set; }

            [Field]
            public String GERAU_HASH { get; set; }

            [Field]
            public DateTime GERAU_DT_ATUALIZACAO { get; set; }
        }

        class ParametroBO : ParametroVO
        {
            //public override Object Insert()
            //{
            //    if (this.SYSAH_CODIGO < 1)
            //    {
            //        this.SYSAH_CODIGO = Worker.GetInstance().GetNumerator(this.SYSAH_FILIAL_COD, this.GetType().Name);
            //    }

            //    //Object ret = Worker.GetInstance().DataAccessManager.Insert(this);

            //    return DataAccessManager.Insert(this);
            //}

            //public override Object Update()
            //{
            //    //return Worker.GetInstance().DataAccessManager.Update(this);
            //    return DataAccessManager.Update(this);
            //}

            //public UsuarioBO Find(int param1, String param2)
            //{
            //    //return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_ID", "'" + param2 + "'" } });
            //    return (UsuarioBO)DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_ID", "'" + param2 + "'" } });
            //}

            //public UsuarioBO FindByCodigo(int param1, int param2)
            //{
            //    //return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_NUMERO", "" + param2 + "" } });
            //    return (UsuarioBO)DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_NUMERO", "" + param2 + "" } });
            //}

            //public DataTable GetList(int param1)
            //{
            //    //return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 } });
            //    return DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 } });
            //}

            //public DataTable GetList(int param1, String param2)
            //{
            //    //return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_CODIGO", "'" + param2 + "'" } });
            //    return DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_CODIGO", "'" + param2 + "'" } });
            //}
        }

        [Table("sysah")]
        class ParametroVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String SYSAH_ID { get; set; }

            [Field]
            public int SYSAH_FILIAL_COD { get; set; }

            [Field]
            public int SYSAH_CODIGO { get; set; }

            [Field]
            public String SYSAH_NOME { get; set; }

            [Field]
            public String SYSAH_VALOR { get; set; }

            [Field]
            public String SYSAH_DESCRICAO { get; set; }
        }

        class UsuarioBO : UsuarioVO
        {
            //public override Object Insert()
            //{
            //    if (this.SYSAQ_CODIGO < 1)
            //    {
            //        this.SYSAQ_CODIGO = Worker.GetInstance().GetNumerator(this.SYSAQ_FILIAL_COD, this.GetType().Name);
            //    }

            //    Object ret = Worker.GetInstance().DataAccessManager.Insert(this);

            //    return ret;
            //}

            //public override Object Update()
            //{
            //    return Worker.GetInstance().DataAccessManager.Update(this);
            //}

            //public UsuarioBO Find(int param1, String param2)
            //{
            //    return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_ID", "'" + param2 + "'" } });
            //}

            //public UsuarioBO FindByNumero(int param1, String param2, String param3)
            //{
            //    return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_NUMERO", "'" + param2 + "'" } });
            //}

            ////public DataTable GetList(int param1, String param2)
            ////{
            ////    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_CODIGO", "'" + param2 + "'" } });
            ////}

            //public DataTable GetList(int param1, bool param2)
            //{
            //    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_INTEGRADO", param2 } });
            //}
        }

        [Table("sysaq")]
        class UsuarioVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public string SYSAQ_ID { get; set; }

            [Field]
            public int SYSAQ_FILIAL_COD { get; set; }

            [Field]
            public int SYSAQ_CODIGO { get; set; }

            [Field]
            public string SYSAQ_NUMERO { get; set; }

            [Field]
            public string SYSAQ_USUARIO { get; set; }

            [Field]
            public string SYSAQ_EMAIL { get; set; }


            [Field]
            public string SYSAQ_SENHA { get; set; }

            [Field]
            public string SYSAQ_FUNIL { get; set; }

            [Field]
            public string SYSAQ_AGENTE_ID { get; set; }

            [Field]
            public string SYSAQ_TIPO { get; set; }


            [Field]
            public int SYSAQ_EQUIPE_ID { get; set; }

            [Field]
            public string SYSAQ_EQUIPE_NAME { get; set; }

            [Field]
            public bool SYSAQ_PARTICIPA_ROLETA { get; set; }

            [Field]
            public bool SYSAQ_USADO_ROLETA { get; set; }

            [Field]
            public bool SYSAQ_INATIVO { get; set; }

            [Field]
            public int SYSAQ_CODIGO_VENDEDOR { get; set; }

            [Field]
            public DateTime SYSAQ_DATA_ATUALIZACAO { get; set; }

        }
    }
}



//
//01/09/2022
//

//private static Timer timer1;
        //private static Program worker;
        //private static WorkerService workerService;

        //private static void Main()
        //{
        //    //string appName = Assembly.GetExecutingAssembly().GetName().Name;

        //    ////
        //    //// Verifico se ja não existe uma versão rodando
        //    ////
        //    //bool createdNew;

        //    //Mutex mutex = new Mutex(true, appName, out createdNew);

        //    //if (!createdNew)
        //    //{
        //    //    Environment.Exit(0);
        //    //}

        //    //string connectionString =
        //    //    "server=mysql.vsisolucoes.com.br;" +
        //    //    "port=3306;" +
        //    //    "database=vsisolucoe153;" +
        //    //    "uid=vsisolucoe153;" +
        //    //    "pwd=V123456;" +
        //    //    "charset=utf8";

        //    //worker = new Program();
        //    //worker.IntegrationId = INTEGRATION_ID;
        //    ////worker.Name = appName;
        //    ////worker.Store = "0";
        //    ////worker.ConnectionString = connectionString;
        //    //worker.IsRunning = true;

        //    //workerService = new WorkerService(worker);

        //    //worker.Logger.Log("inicializado");
        //    //worker.Logger.Log("pressione qualquer tecla para fechar");

        //    //timer1 = new Timer(new TimerCallback(Timer1_Tick), null, 0, 1000 * 60);

        //    //Console.ReadLine();

        //    Program worker = new Program();
        //    worker.IntegrationId = INTEGRATION_ID;
        //    worker.IsRunning = true;
        //    worker.DebugMode = true;

        //    WorkerService workerService = new WorkerService(worker);
        //    workerService.Execute();

        //    worker.Logger.Log("inicializado");
        //    worker.Logger.Log("pressione qualquer tecla para fechar");

        //    Console.ReadLine();
        //}

        //private static void Timer1_Tick(object sender)
        //{
        //    try
        //    {
        //        //
        //        // Injetar os parametros
        //        //
        //        Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();

        //        IEnumerable<ParametroBO> parametroBOList = worker.BusinessObjectManager.GetListByFilter<ParametroBO>($"SYSAH_FILIAL_COD={worker.Store}");
        //        foreach (ParametroBO parametroBO in parametroBOList)
        //        {
        //            parameters.Add(parametroBO.SYSAH_NOME, parametroBO.SYSAH_VALOR);
        //        }

        //        worker.Parameters = parameters;

        //        workerService.Execute();
        //    }
        //    catch (Exception ex)
        //    {
        //        worker.Logger.Log(ex);
        //    }
        //}


        //
        //
        //

         //SgrClient sgrClient = new SgrClient();

                //sgrClient.Token = "688472668f481b3efbddb0bfbff99cf6";// Parameters["PARAM_0009"]; //;"688472668f481b3efbddb0bfbff99cf6"; // 
                //sgrClient.ApiKey = "3569"; //Parameters["PARAM_0010"]; //;"3569"; //
                //                           //sgrClient.Logger = "3569";
                //                           //sgrClient. = "$2y$11$VsK54jZEuuauBgimeiSkreMPalWalHWJg4fs82vIvOw8uNsHyMksG";

                //sgrClient.Username = "moskit"; //Parameters["PARAM_0011"]; // ; "moskit"; // 
                //sgrClient.Password = "2SI7WG"; // Parameters["PARAM_0012"]; //;"2SI7WG"; // 


                //sgrClient.Auth();

                //CookieContainer CookieContainer = new CookieContainer();

                ////CookieContainer.GetCookies(new Cookie());
                //CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));
                //CookieContainer.Add(new Cookie("PHPSESSID", "laravel_session"));



                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUri}servicos/headers_authorization?" +
                //       $"cliente={ApiKey}&" +
                //       $"nome={Username}&" +
                //       $"senha={Password}");

                //request.Method = "POST";
                //request.Accept = "application/json";
                //request.CookieContainer = CookieContainer;


                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_ponto_venda/688472668f481b3efbddb0bfbff99cf6");

                ////request.Headers.Add("Authorization", "Bearer " + token);

                ////request.Headers.Add("Accept", "application/json");
                //request.Headers.Add("X-Access-Token", "$2y$11$VsK54jZEuuauBgimeiSkreMPalWalHWJg4fs82vIvOw8uNsHyMksG");
                //request.Headers.Add("Authorization", "37f82f6bbd836751ce1f58d0cd76861bc8b5f00f");
                //request.Method = "GET";

                //request.Accept = "application/json";
                //request.KeepAlive = true;
                //request.ContentType = "application/x-www-form-urlencoded";
                //request.CookieContainer = CookieContainer;

                ////request.Method = "POST";
                ////request.Accept = "application/json";
                ////request.Headers.Add("X-Auth-Token", client.XAuthToken);
                ////request.Headers.Add("Authorization", client.Authorization);
                ////request.KeepAlive = true;
                //////request.ServicePoint.ConnectionLimit = 10000;
                //////request.ContentType = "multipart/form-data";
                ////request.ContentType = "application/x-www-form-urlencoded";
                ////request.CookieContainer = client.CookieContainer;




                //
                //Veiculos 12/09/2022
                //

                //
                            //veiculo 2
                            //
                            string veiculo2 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_E79Mr2ioCpdYdMZJ").TextValue;

                            string renavam2 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_KaZmKNiOC0QnGMJk").TextValue;

                            string placa2 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_Pj3qYeieCreJKqQe").TextValue;

                            string anoFabricacao2 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_Pj3qYeiVSreJgqQe").NumericValue);

                            string anoModelo2 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_VRAqd1iGSElY5MbL").NumericValue);

                            string chassi2 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_YXoDkki3CVJrjDGE").TextValue;

                            string corVeiculo2 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_Pj3qYeidire8AqQe").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_Pj3qYeidire8AqQe"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                corVeiculo2 = StringUtilities.ToString(optionS["label"]);

                                corVeiculo2 = corVeiculo2.Equals("Azul") ? "221" :
                                                corVeiculo2.Equals("Amarelo") ? "216" :
                                                corVeiculo2.Equals("Branco") ? "217" :
                                                corVeiculo2.Equals("Bege") ? "218" :
                                                corVeiculo2.Equals("Cinza") ? "224" :
                                                corVeiculo2.Equals("Dourado") ? "226" :
                                                corVeiculo2.Equals("Fantasia") ? "761" :
                                                corVeiculo2.Equals("Laranja") ? "225" :
                                                corVeiculo2.Equals("Marrom") ? "227" :
                                                corVeiculo2.Equals("Não Informado") ? "640" :
                                                corVeiculo2.Equals("Preto") ? "219" :
                                                corVeiculo2.Equals("Prata") ? "222" :
                                                corVeiculo2.Equals("Roxo") ? "738" :
                                                corVeiculo2.Equals("Rosa") ? "760" :
                                                corVeiculo2.Equals("Verde") ? "223" :
                                                corVeiculo2.Equals("Vermelho") ? "220" : "640";
                            }

                            string combustivelVeiculo2 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_E79Mr2iLipBLQMZJ").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_E79Mr2iLipBLQMZJ"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                combustivelVeiculo2 = StringUtilities.ToString(optionS["label"]);

                                combustivelVeiculo2 = combustivelVeiculo2.Equals("Alcool") ? "210" :
                                                combustivelVeiculo2.Equals("Alcool/Gasolina") ? "796" :
                                                combustivelVeiculo2.Equals("Flex") ? "212" :
                                                combustivelVeiculo2.Equals("Diesel") ? "211" :
                                                combustivelVeiculo2.Equals("Gasolina") ? "209" :
                                                combustivelVeiculo2.Equals("GNV") ? "214" :
                                                combustivelVeiculo2.Equals("Não Informado") ? "335" :
                                                combustivelVeiculo2.Equals("Tetra Full") ? "213" : "335";
                            }
                            //
                            //veiculo 3
                            //
                            string veiculo3 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_x1kq6oinCw02nMzY").TextValue;

                            string renavam3 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_E79Mr2ioCpBwkMZJ").TextValue;

                            string placa3 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_A4wMWNigC60YKqB8").TextValue;

                            string anoFabricacao3 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_A4wMWNizS60YZqB8").NumericValue);

                            string anoModelo3 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_VrKMbQi4SO2N8qZY").NumericValue);

                            string chassi3 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_YXoDkki3CVJrjDGE").TextValue;

                            string corVeiculo3 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_YXoDkkiPiVJkQDGE").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_YXoDkkiPiVJkQDGE"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                corVeiculo3 = StringUtilities.ToString(optionS["label"]);

                                corVeiculo3 = corVeiculo3.Equals("Azul") ? "221" :
                                                corVeiculo3.Equals("Amarelo") ? "216" :
                                                corVeiculo3.Equals("Branco") ? "217" :
                                                corVeiculo3.Equals("Bege") ? "218" :
                                                corVeiculo3.Equals("Cinza") ? "224" :
                                                corVeiculo3.Equals("Dourado") ? "226" :
                                                corVeiculo3.Equals("Fantasia") ? "761" :
                                                corVeiculo3.Equals("Laranja") ? "225" :
                                                corVeiculo3.Equals("Marrom") ? "227" :
                                                corVeiculo3.Equals("Não Informado") ? "640" :
                                                corVeiculo3.Equals("Preto") ? "219" :
                                                corVeiculo3.Equals("Prata") ? "222" :
                                                corVeiculo3.Equals("Roxo") ? "738" :
                                                corVeiculo3.Equals("Rosa") ? "760" :
                                                corVeiculo3.Equals("Verde") ? "223" :
                                                corVeiculo3.Equals("Vermelho") ? "220" : "640";
                            }

                            string combustivelVeiculo3 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_8P5q4Vi6iBgvXmRJ").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_8P5q4Vi6iBgvXmRJ"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                combustivelVeiculo3 = StringUtilities.ToString(optionS["label"]);

                                combustivelVeiculo3 = combustivelVeiculo3.Equals("Alcool") ? "210" :
                                                combustivelVeiculo3.Equals("Alcool/Gasolina") ? "796" :
                                                combustivelVeiculo3.Equals("Flex") ? "212" :
                                                combustivelVeiculo3.Equals("Diesel") ? "211" :
                                                combustivelVeiculo3.Equals("Gasolina") ? "209" :
                                                combustivelVeiculo3.Equals("GNV") ? "214" :
                                                combustivelVeiculo3.Equals("Não Informado") ? "335" :
                                                combustivelVeiculo3.Equals("Tetra Full") ? "213" : "335";
                            }
                            //
                            //veiculo 4
                            //
                            string veiculo4 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_rpGmBPioCRlPKqeR").TextValue;

                            string placa4 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_2ojMxLiPCZxRAMOE").TextValue;

                            string renavam4 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_G5rMe5iNC3kAWMyj").TextValue;

                            string anoFabricacao4 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_2ojMxLi8SZxReMOE").NumericValue);

                            string anoModelo4 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_0WGqoEioS9oy9mnP").NumericValue);

                            string chassi4 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_K7Rm8QiRCKlzkDbN").TextValue;

                            string corVeiculo4 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_dVKmQ5ibid9e1mWR").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_dVKmQ5ibid9e1mWR"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                corVeiculo4 = StringUtilities.ToString(optionS["label"]);

                                corVeiculo4 = corVeiculo4.Equals("Azul") ? "221" :
                                                corVeiculo4.Equals("Amarelo") ? "216" :
                                                corVeiculo4.Equals("Branco") ? "217" :
                                                corVeiculo4.Equals("Bege") ? "218" :
                                                corVeiculo4.Equals("Cinza") ? "224" :
                                                corVeiculo4.Equals("Dourado") ? "226" :
                                                corVeiculo4.Equals("Fantasia") ? "761" :
                                                corVeiculo4.Equals("Laranja") ? "225" :
                                                corVeiculo4.Equals("Marrom") ? "227" :
                                                corVeiculo4.Equals("Não Informado") ? "640" :
                                                corVeiculo4.Equals("Preto") ? "219" :
                                                corVeiculo4.Equals("Prata") ? "222" :
                                                corVeiculo4.Equals("Roxo") ? "738" :
                                                corVeiculo4.Equals("Rosa") ? "760" :
                                                corVeiculo4.Equals("Verde") ? "223" :
                                                corVeiculo4.Equals("Vermelho") ? "220" : "640";
                            }

                            string combustivelVeiculo4 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_K7Rm8QiBiKleVDbN").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_K7Rm8QiBiKleVDbN"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                combustivelVeiculo4 = StringUtilities.ToString(optionS["label"]);

                                combustivelVeiculo4 = combustivelVeiculo4.Equals("Alcool") ? "210" :
                                                combustivelVeiculo4.Equals("Alcool/Gasolina") ? "796" :
                                                combustivelVeiculo4.Equals("Flex") ? "212" :
                                                combustivelVeiculo4.Equals("Diesel") ? "211" :
                                                combustivelVeiculo4.Equals("Gasolina") ? "209" :
                                                combustivelVeiculo4.Equals("GNV") ? "214" :
                                                combustivelVeiculo4.Equals("Não Informado") ? "335" :
                                                combustivelVeiculo4.Equals("Tetra Full") ? "213" : "335";
                            }
                            //
                            //veiculo 4
                            //
                            string veiculo5 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_R7PDNQiGCZ1xxmWJ").TextValue;

                            string renavam5 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_eZYm9BiyCR0pjD47").TextValue;

                            string placa5 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_POEMywieCJrxRDdk").TextValue;

                            string anoFabricacao5 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_POEMywiYSJrxXDdk").NumericValue);

                            string anoModelo5 = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_oJZmP1ibSGo0VDgv").NumericValue);

                            string chassi5 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_3LvDvEi4CN0RZm6a").TextValue;
                            string corVeiculo5 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_A4wMWNiLi60QXqB8").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_A4wMWNiLi60QXqB8"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                corVeiculo5 = StringUtilities.ToString(optionS["label"]);

                                corVeiculo5 = corVeiculo5.Equals("Azul") ? "221" :
                                                corVeiculo5.Equals("Amarelo") ? "216" :
                                                corVeiculo5.Equals("Branco") ? "217" :
                                                corVeiculo5.Equals("Bege") ? "218" :
                                                corVeiculo5.Equals("Cinza") ? "224" :
                                                corVeiculo5.Equals("Dourado") ? "226" :
                                                corVeiculo5.Equals("Fantasia") ? "761" :
                                                corVeiculo5.Equals("Laranja") ? "225" :
                                                corVeiculo5.Equals("Marrom") ? "227" :
                                                corVeiculo5.Equals("Não Informado") ? "640" :
                                                corVeiculo5.Equals("Preto") ? "219" :
                                                corVeiculo5.Equals("Prata") ? "222" :
                                                corVeiculo5.Equals("Roxo") ? "738" :
                                                corVeiculo5.Equals("Rosa") ? "760" :
                                                corVeiculo5.Equals("Verde") ? "223" :
                                                corVeiculo5.Equals("Vermelho") ? "220" : "640";
                            }

                            string combustivelVeiculo5 = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_gvGm3BiaizpoQM45").Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_gvGm3BiaizpoQM45"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                combustivelVeiculo5 = StringUtilities.ToString(optionS["label"]);

                                combustivelVeiculo5 = combustivelVeiculo5.Equals("Alcool") ? "210" :
                                                combustivelVeiculo5.Equals("Alcool/Gasolina") ? "796" :
                                                combustivelVeiculo5.Equals("Flex") ? "212" :
                                                combustivelVeiculo5.Equals("Diesel") ? "211" :
                                                combustivelVeiculo5.Equals("Gasolina") ? "209" :
                                                combustivelVeiculo5.Equals("GNV") ? "214" :
                                                combustivelVeiculo5.Equals("Não Informado") ? "335" :
                                                combustivelVeiculo5.Equals("Tetra Full") ? "213" : "335";
                            }


                            //
                            //Backup 21/09/2022
                            //


                            using Khronus.Framework.Core.Util;
using Khronus.Framework.DataAccess.ORM;
using Newtonsoft.Json.Linq;
using Sync.Custom.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using VSIntegra.Core.Model;
using VSIntegra.Integration;
using VSIntegra.Integration.Moskit;
using VSIntegra.Integration.Moskit.Model;
using VSIntegra.Integration.Moskit.Services;
using VSIntegra.Integration.Sga;
using VSIntegra.Integration.Sga.Model;
using VSIntegra.Integration.Sga.Services;
using VSIntegra.Integration.Sgr;
using VSIntegra.Integration.Sgr.Model;
using VSIntegra.Integration.Sgr.Services;

namespace Sync.Custom
{
    class Program : WorkerBase
    {
        private const string INTEGRATION_ID = "55s8c52635331";
        private const string API_KEY_MOSKIT = "8a3f08a1-d459-4201-9fea-cff64d7696ca";
        private const string TOKEN_SGA = "916104032ec4fb57216c03ac3f1a50e9b3c1c97c1c029c8fee34e164de5a8ec47aa3389c508fef4895d420766c54e443385691f8dc1223e5527d71f8597a5e9cb1b6d360ca1f8c3d480fdb209a8bd27d37820fb3bb31af485b7c0b022ab9d5a458825573627a27320fc2e7b8f858304f4d40cbdb42f7d8e923c4eb16d378c1acc9c56b13d72ac7175ee38197e719de76";
        private const string TOKEN_SGR = "688472668f481b3efbddb0bfbff99cf6";
        private const string API_KEY_SGR = "3569";
        private const string USERNAME_SGR = "moskit";
        private const string PASSWORD_SGR = "2SI7WG";

        private static void Main()
        {
            Program worker = new Program();
            worker.IntegrationId = INTEGRATION_ID;
            worker.IsRunning = true;
            worker.DebugMode = true;

            WorkerService workerService = new WorkerService(worker);
            workerService.Execute();

            worker.Logger.Log("inicializado");
            worker.Logger.Log("pressione qualquer tecla para fechar");

            Console.ReadLine();
        }

        private void SyncUsuario()
        {
            Object ret = null;

            Log($"SyncUsuario iniciado...");

            try
            {
                MoskitClient moskitClient = new MoskitClient();
                moskitClient.Token = Parameters["PARAM_0001"];

                UserService userService = new UserService(moskitClient);

                IEnumerable<User> users = userService.List();

                foreach (User user in users)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    string usuarioId = StringUtilities.ToString(user.Id);
                    string usuario = user.Name;
                    string email = user.Username;
                    bool active = user.Active;

                    Team team = user.Team;

                    TeamService teamService = new TeamService(moskitClient);


                    //user.Team = (Team)teamService.Get(user.Team.Id);

                    ServiceReturn serviceReturn = teamService.Get(user.Team.Id);

                    string equipeName = "";


                    if (serviceReturn.Status == 200)
                    {
                        team = (Team)serviceReturn.Detail;

                        equipeName = team.Name;

                    }

                    UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_FILIAL_COD='{Store}' AND SYSAQ_NUMERO={usuarioId}");

                    bool found = usuarioBO != null;

                    if (!found)
                    {
                        usuarioBO = new UsuarioBO();

                        usuarioBO.SYSAQ_FILIAL_COD = NumberUtilities.parseInt(Store);
                        usuarioBO.SYSAQ_CODIGO = GetNumerator(Store, "sysaq");
                        usuarioBO.SYSAQ_NUMERO = usuarioId;
                        usuarioBO.SYSAQ_PARTICIPA_ROLETA = false;
                    }

                    //usuarioBO.SYSAQ_CODIGO = GetNumerator(Store, "sysaq");
                    usuarioBO.SYSAQ_USUARIO = usuario;
                    //usuarioBO.SYSAQ_EQUIPE_ID = team.Id;
                    usuarioBO.SYSAQ_EQUIPE_NAME = equipeName;
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

                    //Thread.Sleep(1000);
                }

            }
            catch (Exception ex)
            {
                Log(ex);
            }

            Log($"SyncUsuario finalizado");
        }

        private void SyncSinconizarIndicacao()
        {

            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {
                int quantity = 50;
                int start = 0;

                //
                // Busca origem do leads e fazer um de para com a indicação
                //
                do
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    JArray customFields = (JArray)SendCommand($"https://api.moskitcrm.com/v2/customFields/CF_49RM16ixiB7nbmBW/options?start={start}&quantity={quantity}", "GET", null, Parameters["PARAM_0001"]);

                    //JObject campos = (JObject)SendCommand("https://api.moskitcrm.com/v2/customFields/CF_49RM16ixiB7nbmBW/options", "GET", null, Parameters["PARAM_0001"]);

                    if (customFields.Count == 0)
                    {
                        return;
                    }

                    foreach (JObject customField in customFields)
                    {
                        if (!IsRunning)
                        {
                            return;
                        }

                        int id = NumberUtilities.parseInt(customField["id"]);
                        String descricao = StringUtilities.ToString(customField["label"]);

                        IndicacaoOrigemBO indicacaoOrigemBO = BusinessObjectManager.FindByFilter<IndicacaoOrigemBO>($"USUAK_FILIAL={Store} AND USUAK_NUMERO={id}");

                        bool found = indicacaoOrigemBO != null;

                        if (!found)
                        {
                            indicacaoOrigemBO = new IndicacaoOrigemBO();

                            indicacaoOrigemBO.USUAK_FILIAL = NumberUtilities.parseInt(Store);
                            indicacaoOrigemBO.USUAK_CODIGO = GetNumerator(Store, "usuak");
                        }

                        indicacaoOrigemBO.USUAK_NUMERO = id;
                        indicacaoOrigemBO.USUAK_DESCRICAO = descricao;
                        indicacaoOrigemBO.USUAK_CODIGO_SGR = 0;

                        object ret = null;

                        if (!found)
                        {
                            ret = BusinessObjectManager.Insert(indicacaoOrigemBO);
                        }
                        else
                        {
                            ret = BusinessObjectManager.Update(indicacaoOrigemBO);
                        }

                        if (ret != null)
                        {
                            throw new Exception(StringUtilities.ToString(ret));
                        }

                        //registro++;
                        Thread.Sleep(3000);
                    }
                    start++;

                } while (true);

                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_indicacao/688472668f481b3efbddb0bfbff99cf6");

                ////request.Headers.Add("Authorization", "Bearer " + token);

                ////request.Headers.Add("Accept", "application/json");                            
                //request.Headers.Add("X-Access-Token", "$2y$11$tHCYAIVEyGWUgKnNFFstvOG3OvnuIFX544pUguS/aCdr3K8RZQ5zq");
                //request.Headers.Add("Authorization", "8fc135598e7d0e87e7d1af222ab215c29ec94fcc");

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //if (response.StatusCode != HttpStatusCode.OK)
                //{
                //    throw new Exception(response.StatusDescription);
                //}
                //var resultActivities = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
            }
            catch (Exception ex)
            {

            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado");
        }

        private void SyncSinconizarPontoVenda()
        {

            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {

                //if (CookieContainer == null || CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"))["laravel_session"].Expires < DateTime.Now)
                //{
                //}

                CookieContainer CookieContainer = new CookieContainer();
                //CookieContainer.GetCookies(new Cookie());
                CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));


                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUri}servicos/headers_authorization?cliente={3569}&nome={"moskit"}&senha={"2SI7WG"}");
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

                //SgrClient sgrClient = new SgrClient();
                ////sgrClient.ApiUri = "https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_ponto_venda/688472668f481b3efbddb0bfbff99cf6";
                //sgrClient.Token = "688472668f481b3efbddb0bfbff99cf6";
                //sgrClient.ApiKey = "3569"; 
                //sgrClient.Username = "moskit";
                //sgrClient.Password = "2SI7WG";

                ///
                ///
                //HttpWebRequest 
                request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_ponto_venda/688472668f481b3efbddb0bfbff99cf6");
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

                ////HttpWebResponse
                //response = (HttpWebResponse)request.GetResponse();

                ////string 
                //    json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //JObject detail = JObject.Parse(json);
                //JArray detail2 = JArray.Parse(detail["data"].ToString());

                //if (response.StatusCode != HttpStatusCode.OK)
                //{
                //    throw new Exception(response.StatusDescription);
                //}
                //var resultActivities = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
            }
            catch (Exception ex)
            {

            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado");

        }

        public void SyncCriarErpSgrSgaLeadsGanhos()
        {
            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {
                MoskitClient moskitClient = new MoskitClient();
                //moskitClient.Token = Parameters["PARAM_0001"];

                moskitClient.Token = API_KEY_MOSKIT;

                DealService dealService = new DealService(moskitClient);

                ServiceReturn serviceReturn = new ServiceReturn();


                ContactService contactService = new ContactService(moskitClient);

                IEnumerable<EventoBO> eventoBOList = BusinessObjectManager.GetListByFilter<EventoBO>($"USUAN_FILIAL={Store} AND USUAN_TIPO=0 AND USUAN_STATUS='00'");

                foreach (EventoBO eventoBO in eventoBOList)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    JObject evento = JObject.Parse(StringUtilities.ToString(eventoBO.USUAN_EVENTO));

                    JObject data = JObject.Parse(StringUtilities.ToString(evento["data"]));

                    string statusDeal = StringUtilities.ToString(data["status"]);
                    long dealId = NumberUtilities.parseInt(data["id"]); //1496620 160
                    string responsibleId = StringUtilities.ToString(data["responsible"]["id"]);

                    long contatoId = 0;



                    if (data["contacts"] != null)
                    {
                        JArray contatos = (JArray)data["contacts"];


                        contatoId = contatos.Count > 0 ? NumberUtilities.parseLong(data["contacts"][0]["id"]) : 0;

                    }

                    JArray products = (JArray)data["productDeals"];

                    string status = "10";
                    string motivo = "Processado com sucesso";
                    object eventoRetorno = null;

                    if (statusDeal.Equals("won"))
                    {

                        serviceReturn = contactService.Get(contatoId);

                        // Busca dados do contato
                        //Contact contact = (Contact)contactService.Get(contatoId);

                        string nome = "";
                        string telefone = "";
                        string celular = "";
                        string email = "";
                        string cpfCnpj = "";
                        string rg = "";
                        string cidade = "";
                        string bairro = "";
                        string cep = "";
                        string numero = "";
                        string logradouro = "";
                        string estado = "";
                        //string profissao = "";
                        //string estadoCivil = "";
                        //string sexo = "";
                        //string renavam = "";

                        //long[] options = new long[];
                        List<EntityCustomField> entityCustomFields = new List<EntityCustomField>();

                        if (serviceReturn.Status == 200)
                        {
                            Contact contact = (Contact)serviceReturn.Detail;

                            if (contact != null)
                            {
                                entityCustomFields = new List<EntityCustomField>(contact.EntityCustomFields);

                                nome = contact.Name;
                                telefone = contact.Phones == null ? "" : contact.Phones[0].Number;
                                celular = contact.Phones.Length > 0 ? "" : contact.Phones[1].Number;
                                email = contact.Emails == null ? "" : contact.Emails[0].Address;
                                cpfCnpj = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_0WGqoGSKC9zK2qnP").TextValue;
                                rg = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_6rRmwGSvC6jZKm4X").TextValue;
                                cidade = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_42AmakSZCwrAPqjl").TextValue;
                                bairro = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_wPVm2oS2Cbj9gDK6").TextValue;
                                cep = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_Pj3qYoSeCrBpamQe").TextValue;
                                numero = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_Rg7Mn4SLCA1XrDvd").TextValue;
                                logradouro = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_ylAm0KS6C5p91Mvb").TextValue;
                                estado = "";

                                long[] options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_6rRmwGS9i6jZpm4X").Options;

                                if (options != null)
                                {
                                    long option = options[0];

                                    // Busco options
                                    JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_6rRmwGS9i6jZpm4X"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                    estado = StringUtilities.ToString(optionS["label"]);
                                }

                            }
                        }

                        // Buscar os dados do negócio

                        serviceReturn = dealService.Get(dealId);


                        if (serviceReturn.Status == 200)
                        {
                            //Deal deal = (Deal)dealService.Get(dealId);

                            Deal deal = (Deal)serviceReturn.Detail;


                            // Busca codigo do vendedor
                            int codigoVendedorSgr = 27;
                            int codigoVendedorSga = 1;

                            UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_FILIAL_COD='{Store}' AND SYSAQ_NUMERO='{responsibleId}'");

                            //found = usuarioBO != null;

                            if (usuarioBO != null)
                            {
                                codigoVendedorSgr = usuarioBO.SYSAQ_CODIGO_VENDEDOR_SGR;
                                codigoVendedorSga = usuarioBO.SYSAQ_CODIGO_VENDEDOR_SGA;
                            }

                            if (codigoVendedorSgr == 0)
                            {
                                continue;
                            }

                            if (codigoVendedorSga == 0)
                            {
                                codigoVendedorSga = 1;
                            }

                            entityCustomFields = new List<EntityCustomField>(deal.EntityCustomFields);

                            //
                            //Dados do cliente
                            //

                            //
                            //Cnpj
                            //
                            cpfCnpj = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR")?.TextValue;


                            //
                            //RG
                            //
                            rg = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_2wpDlkinColgEmvL")?.TextValue;

                            //
                            //Sexo
                            //
                            //string sexo = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_nrLDXoiWikl02mOa")?.TextValue;


                            //
                            //Estado civil
                            //
                            //string estadoCivil = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR")?.TextValue;

                            //
                            //Profissão
                            //
                            string profissao = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_AE5mpEijCdJQlDO3")?.TextValue;

                            //
                            //Email
                            //
                            string emails = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5")?.TextValue;

                            //
                            //Endereço
                            //
                            long[] options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_Pj3qYeidireglqQe")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_Pj3qYeidireglqQe"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                estado = StringUtilities.ToString(optionS["label"]);
                            }

                            cidade = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_Pj3qYeieCrG2EqQe")?.TextValue;

                            if (string.IsNullOrWhiteSpace(cidade))
                            {
                                cidade = cidade.Contains(",") ? cidade.Replace(cidade.Substring(cidade.IndexOf(",")), "").Trim() : cidade;
                            }


                            //pos = valorSubstring.IndexOf(finish);

                            //valorSubstring = pos > 0 ? valorSubstring.Substring(0, pos) : "";

                            //valorSubstring = valorSubstring.Trim();

                            //cidade = cidade.Substring(cidade.IndexOf(","));
                            bairro = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_POEMywieCJrxADdk")?.TextValue;
                            cep = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_A4wMWNigC68OBqB8")?.TextValue;
                            numero = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_6rRmweivC6rQ5q4X")?.TextValue;
                            logradouro = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_3nGqEoirCl8lkmYA")?.TextValue;

                            string tipoAssociadoRastreador = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_y5lm56iyiY4L8DwW")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_y5lm56iyiY4L8DwW"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                tipoAssociadoRastreador = StringUtilities.ToString(optionS["label"]);
                            }

                            string sexo = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_nrLDXoiWikl02mOa")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_nrLDXoiWikl02mOa"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                sexo = StringUtilities.ToString(optionS["label"]);

                                sexo = sexo.Equals("Feminino") ? "F" :
                                                sexo.Equals("Masculino") ? "M" : "";

                            }

                            string estadoCivil = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_nrLDXoiWikl02mOa")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_nrLDXoiWikl02mOa"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                estadoCivil = StringUtilities.ToString(optionS["label"]);

                                estadoCivil = estadoCivil.Equals("Casado(a)") ? "CA" :
                                                estadoCivil.Equals("Solteio(a)") ? "SO " :
                                                estadoCivil.Equals("Viuvo(a)") ? "VI" :
                                                estadoCivil.Equals("Divorciado(a)") ? "DI" :
                                                estadoCivil.Equals("Separado(a)") ? "SE" :
                                                estadoCivil.Equals("União estavel") ? "CO" : "SO";
                            }

                            int codIndicacao = 1;

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_49RM16ixiB7nbmBW")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_49RM16ixiB7nbmBW"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                //codIndicacao = NumberUtilities.parseInt(optionS["id"]);

                                // buscar indicação do lead
                                IndicacaoOrigemBO indicacaoOrigemBO = BusinessObjectManager.FindByFilter<IndicacaoOrigemBO>($"USUAK_NUMERO='{NumberUtilities.parseInt(optionS["id"])}'");

                                //bool 
                                //found = indicacaoOrigemBO != null;

                                if (indicacaoOrigemBO != null)
                                {
                                    codIndicacao = indicacaoOrigemBO.USUAK_CODIGO_SGR;
                                }
                            }

                            string codPontoVenda = "";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_QJXmA5iXiJEBpm25")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_QJXmA5iXiJEBpm25"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                codPontoVenda = StringUtilities.ToString(optionS["label"]);

                                codPontoVenda = codPontoVenda.Equals("PONTO TRACK") ? "1" :
                                                codPontoVenda.Equals("AMERICA CLUBE DE BENEFICIOS") ? "2" :
                                                codPontoVenda.Equals("MARINGA") ? "3" :
                                                codPontoVenda.Equals("ASSOCIACAO MUTUALISTA VIA SUL") ? "4" :
                                                codPontoVenda.Equals("CURITIBA") ? "5" : "0";
                            }

                            string codTipoRastreador = "325";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options de tipo rastreados se Funil PJ "id": 38814,"name": "Funil PF"
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_wPVm2VijibjB8mK6"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                codTipoRastreador = StringUtilities.ToString(optionS["label"]);

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

                            #region Dados do veículo cada veículo deve-se gerar um pedido no sgr
                            //
                            //Dados do veiculo
                            //

                            string modeloVeiculo = "";

                            string valorNegocio = StringUtilities.ToString(deal.Price);

                            string renavam = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_3LvDvEi4CN0jam6a")?.TextValue;

                            string veiculo1 = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_wPVm2Vi2CbQLOmK6")?.TextValue;

                            string placa = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_oJZmP1iKCG7oLDgv")?.TextValue;

                            string anoFabricacao = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_0WGqoEioS90o3mnP")?.NumericValue);

                            string anoModelo = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_3nGqEoiPSl8l5mYA")?.NumericValue);

                            string chassi = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_VrKMbQiaCO72lqZY")?.TextValue;

                            string tamanhoFrota = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNw8vm6a")?.TextValue);

                            string empresa = StringUtilities.ToString(EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_dVKmQ5i1Cd93kmWR")?.TextValue);

                            int corVeiculoCod = 640;

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_gvGm3BiaizpJ0M45")?.Options;
                            // combustivel veiculo 1

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_gvGm3BiaizpJ0M45"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                string corVeiculo = StringUtilities.ToString(optionS["label"]);

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

                                                //AMARELA: "216"
                                                //BRANCA: "217"
                                                //BEGE: "218"
                                                //PRETA: "219"
                                                //VERMELHA: "220"
                                                //AZUL: "221"
                                                //PRATA: "222"
                                                //VERDE: "223"
                                                //CINZA: "224"
                                                //LARANJA: "225"
                                                //DOURADA: "226"
                                                //MARROM: "227"
                                                //NAO INFORMADO: "640"
                                                //ROXA: "738"
                                                //ROSA: "760"
                                                //FANTASIA: "761"
                            }

                            string combustivelVeiculo = "335";

                            options = EntityCustomField.GetEntityCustomFieldById(entityCustomFields, "CF_wGrqzpi3ido00mLo")?.Options;

                            if (options != null)
                            {
                                long option = options[0];

                                // Busco options
                                JObject optionS = (JObject)SendCommand($"https://api.moskitcrm.com/v2/customFields/{"CF_wGrqzpi3ido00mLo"}/options/{option}", "GET", null, Parameters["PARAM_0001"]);

                                combustivelVeiculo = StringUtilities.ToString(optionS["label"]);

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
                            //Modelo do veículo, montar uma lista de veículos porque cada veículo deve-se gerar um pedido.
                            //
                            modeloVeiculo = veiculo1;

                            modeloVeiculo = "PALIO";

                            JObject modeloVeiculo1 = BuscaModeloVeiculo(modeloVeiculo);

                            int codModeloVeiculo = 0;
                            int codMontadoraVeiculo = 0;

                            if (modeloVeiculo != null)
                            {
                                JArray modelos = (JArray)modeloVeiculo1["data"];
                                if (modelos.Count > 0)
                                {
                                    codModeloVeiculo = NumberUtilities.parseInt(modeloVeiculo1["data"][0]["cod_veiculo_modelo"]);

                                    codMontadoraVeiculo = NumberUtilities.parseInt(modeloVeiculo1["data"][0]["cod_veiculo_montadora"]);
                                }                                
                            }

                            #endregion

                            #region //Consultar produto, se produto estiver preenchido associar ao campo Tipo rastreados no sgr
                            // Consultar produto

                            string nomeProduto = "";

                            if (products.Count > 0)
                            {
                                nomeProduto = StringUtilities.ToString(products[0]["product"][0]["name"]);
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

                            // Quando o produto do negócio for Plano Light, Plano Plus, Plano Prime o produto no SGR é PROTEÇÃO VEICULAR
                            if (nomeProduto.ToUpper().Equals("RASTREAMENTO") || tipoAssociadoRastreador.ToUpper().Equals("CLIENTE/RASTREADOR"))
                            {
                                tipoProduto = 1;
                            }
                            else if (nomeProduto.ToUpper().Contains("PLANO") || tipoAssociadoRastreador.ToUpper().Equals("ASSOCIADO"))
                            {
                                tipoProduto = 2;
                            }
                            #endregion

                            cpfCnpj = StringUtilities.Clear(cpfCnpj, "");

                            //ServiceReturn serviceReturn = new ServiceReturn();

                            #region Gravo cliente no SGA
                            if (tipoProduto == 2)
                            {
                                SgaClient sgaClient = new SgaClient();
                                //sgaClient.Token = "339de8a7370943867aaa693e966cfa061083679b5caf157636c1619496258fb208f5571ddd3b47a4cea8867175ed18f9ebc2ef60089660054204ecbd994a093d641885a6a31338e6bd52ca21a4ddba52d08ad265568c48f43cde3d9c10af1e989a51569e8f317a64e51488320bb1a40640a41af6521b9628b01f1c93dad46242406a2d7abdfd18338002e8910f2c1942"; // Parameters["PARAM_0004"];
                                sgaClient.Token = TOKEN_SGA;

                                AssociadoService associadoService = new AssociadoService(sgaClient);

                                serviceReturn = associadoService.GetByCpf(cpfCnpj); //35170458886

                                if (serviceReturn.Status == 500)
                                {
                                    throw new Exception(serviceReturn.Message);
                                }

                                Associado associado = (Associado)serviceReturn.Detail;

                                bool found = associado != null;

                                if (!found)
                                {
                                    associado = new Associado();
                                    associado.Nome = nome;
                                    associado.Sexo = "M";
                                    associado.Cpf = cpfCnpj;
                                    associado.Rg = rg;
                                    //associado.DataNascimento = dataNascimento;
                                    associado.Telefone = telefone;
                                    associado.Celular = celular;
                                    associado.Email = email;
                                    associado.Logradouro = logradouro;
                                    associado.Numero = numero; //String
                                                               //associado.Complemento = complemento;
                                    associado.Bairro = bairro;
                                    associado.Cidade = cidade;
                                    associado.Estado = estado;
                                    associado.Cep = cep;

                                    associado.DiaVencimento = 10;

                                    associado.CodigoVoluntario = codigoVendedorSga; // 1;
                                    
                                    //associado.CodigoVoluntario = 1;

                                    //associado.CodigoProfissao = 9;
                                    //associado.CodigoEstadoCivil = 6; //Não informado

                                    //associado.CodigoRegional = 1;
                                    //associado.CodigoCooperativa = 1;


                                    //associado.Nome = "TESTE VSI SOLUCOES 500";
                                    //associado.Sexo = "M";
                                    //associado.Cpf = cpfCnpj;
                                    //associado.Rg = "400531732"; //String
                                    //associado.DataNascimento = "17/11/1987";
                                    //associado.Telefone = "(15) 3313-2514";
                                    //associado.Celular = "(15) 998100-5172";
                                    //associado.Email = "teste@vsisolucoes.com.br";
                                    //associado.Logradouro = "Teste 222";
                                    //associado.Numero = "122"; //String
                                    //associado.Complemento = "teste";
                                    //associado.Bairro = "teste";
                                    //associado.Cidade = "Sorocoba";
                                    //associado.Estado = "SP";
                                    //associado.Cep = "18.071-095";

                                    //
                                    //associado.DescricaoSituacao = "PENDENTE";

                                    //

                                    //associado.CodigoSituacao = 1;


                                    //associado.CodigoBeneficiario = 1;

                                    //associado.CodigoComoConheceu = "1"; 
                                    //associado.CodigoConta = 2;

                                    ////
                                    ////Cobrança
                                    ////

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

                                    if (serviceReturn.Status == 500)
                                    {
                                        status = "20";
                                        motivo = "Processado com falha";
                                        eventoRetorno = serviceReturn.Message;
                                    }
                                }
                            }
                            #endregion

                            //if (tipoAssociadoRastreador.ToUpper().Equals("CLIENTE/RASTREADOR"))
                            //{

                            if (serviceReturn.Status == 200 && tipoProduto > 0)
                            {

                                string cCliente = null;

                                //cpfCnpj = StringUtilities.Clear(cpfCnpj, ""); // <-- precisa passar sem os tracos
                                //
                                //Cadastro de cliente
                                //
                                SgrClient sgrClient = new SgrClient();
                                sgrClient.Token = TOKEN_SGR;// Parameters["PARAM_0009"]; //;"688472668f481b3efbddb0bfbff99cf6"; // 
                                sgrClient.ApiKey = API_KEY_SGR; //"3569"; //Parameters["PARAM_0010"]; //;"3569"; //
                                sgrClient.Username = USERNAME_SGR; //"moskit"; //Parameters["PARAM_0011"]; // ; "moskit"; // 
                                sgrClient.Password = PASSWORD_SGR; //"2SI7WG"; // Parameters["PARAM_0012"]; //;"2SI7WG"; // 

                                ClienteService clienteService = null;

                                Cliente cliente = null;

                                clienteService = new ClienteService(sgrClient);

                                cliente = clienteService.Get(cpfCnpj);
                                //Cliente cliente = clienteService.Get("31563730014");
                                //Cliente cliente = clienteService.Get(cpfCnpj);
                                if (cliente == null)
                                {
                                    cliente = new Cliente();
                                    cliente.NomeCliente = nome;
                                    cliente.CpfCliente = cpfCnpj; // "31563730014"; // cpfCnpj;
                                    cliente.EnderecoClienteLogradouro = logradouro;
                                    cliente.EnderecoClienteNumero = numero;
                                    cliente.EnderecoClienteBairro = bairro;
                                    cliente.EnderecoClienteCidade = cidade;
                                    cliente.EnderecoClienteEstado = estado;
                                    cliente.EnderecoClienteCep = cep;
                                    cliente.CodSituacaoCliente = 1; //1-Ativo e 3-Pendente
                                    cliente.CodMatrizFilialCliente = 1;
                                    cliente.FormatoEnvioTituloCliente = 198;
                                    cliente.FormatoBoletoCliente = "U";
                                    //cliente.Rg = rg;

                                    serviceReturn = clienteService.Post(cliente);

                                    if (serviceReturn.Status == 200)
                                    {
                                        cCliente = JObject.Parse(serviceReturn.Message)["cod_cliente"].ToString();
                                    }
                                    else
                                    {
                                        status = "20";
                                        motivo = "Processado com falha";
                                        eventoRetorno = serviceReturn.Message;
                                    }
                                }
                                else
                                {
                                    cCliente = cliente.CodCliente;
                                }

                                if (cCliente != null)
                                {

                                    long price = NumberUtilities.parseLong(deal.Price);

                                    decimal valorNegocio1 = 0;

                                    if (price > 0)
                                    {
                                        valorNegocio1 = NumberUtilities.parseDecimal(price.ToString().Substring(0, price.ToString().Length - 2) + "," + price.ToString().Substring(price.ToString().Length - 2));
                                    }


                                    PedidoService pedidoService = new PedidoService(sgrClient);

                                    Pedido pedido = new Pedido();

                                    //1) PONTO DE VENDA, 
                                    pedido.CodPontoVendaVenda = NumberUtilities.parseInt(codPontoVenda);

                                    //2) VENDEDOR, 
                                    pedido.CodConsultorVenda = codigoVendedorSgr; // 1; //Pega da amarracao MOSKIT x SGR

                                    //3) DADOS DO CLIENTE: NOME / RAZÃO SOCIAL e CPF / CNPJ, 
                                    //5) TELEFONES, 
                                    //6) E-MAIL, 
                                    //7) DADOS ENDEREÇO(VALIDAR COM EDER), 
                                    pedido.CodClienteVenda = NumberUtilities.parseInt(cCliente);
                                    pedido.NomeClienteVenda = nome;
                                    pedido.CpfClienteVenda = cpfCnpj;
                                    pedido.RgClienteVenda = rg;
                                    pedido.SexoClienteVenda = sexo;
                                    pedido.EstadoCivilClienteVenda = estadoCivil;
                                    pedido.ProfissaoClienteVenda = profissao;

                                    //pedido.RgClienteVenda = rg;
                                    //pedido.SexoClienteVenda = cliente.Sexo;
                                    //pedido.EstadoCivilClienteVenda = cliente.EstadoCivil;
                                    //pedido.ProfissaoClienteVenda = cliente.Profissao;

                                    //4) PRODUTO, 
                                    pedido.ProdutoVendaCodProduto = tipoProduto; //1;
                                    pedido.ValorParcelaAdesaoVenda = valorNegocio1;
                                    pedido.ValorParcelaVenda = valorNegocio1;
                                    pedido.FipeValorVeiculoVenda = valorNegocio1;
                                    pedido.QuantidadeParcelaVenda = 1;
                                    pedido.QuantidadeParcelaAdesaoVenda = 1;

                                    //8) DADOS DO VEICULO: MODELO/ANO FABRICAÇÃO E ANO MODELO / MONTADORA / PLACA / CHASSI, 
                                    pedido.AnomodVeiculoVenda = anoModelo;
                                    pedido.AnoFabVeiculoVenda = anoFabricacao;
                                    pedido.PlacaVeiculoVenda = "vsd45f5"; // placa;
                                    pedido.ChassiVeiculoVenda = "9BGRX08908G100105"; // chassi;


                                    pedido.RenavamVeiculoVend = "00920955398";// renavam;
                                    pedido.CodCombustivelVeiculoVenda = NumberUtilities.parseInt(335);

                                    pedido.CodModeloVeiculoVenda = codModeloVeiculo;
                                    pedido.CodMarcaVeiculoVenda = codMontadoraVeiculo;

                                    //pedido.CodFormaPagamentoVenda = null;
                                    //pedido.CodFormaPagamentoAdesaoVenda = null;
                                    //pedido.CodGrupoVenda = null;
                                    //pedido.CodGrupoAdesaoVenda = null;

                                    //pedido.EmailVenda = "";
                                    //pedido.TempoContrato = "";

                                    //9) INDICAÇÃO, 
                                    pedido.CodIndicacaoVenda = codIndicacao; // 1; // codIndicacao;

                                    //10) TIPO DE RASTREADOR
                                    pedido.CodTipoRastreadorVenda = NumberUtilities.parseInt(codTipoRastreador); //1;
                                    //pedido.CodMarcaVeiculoVenda = codMontadoraVeiculo;

                                    //pedido.ValorParcelaVenda = valorParcelaVenda / quantidadeParcelaVenda;
                                    //pedido.ValorParcelaAdesaoVenda = valorParcelaVenda / quantidadeParcelaVenda;

                                    //pedido.CodCombustivelVeiculoVenda = 209;
                                    //pedido.CodFormaPagamentoVenda = 201;
                                    //pedido.CodFormaPagamentoAdesaoVenda = 201;
                                    //pedido.CodGrupoVenda = 1;
                                    //pedido.CodGrupoAdesaoVenda = 1002;
                                    //pedido.CodPeriodoVenda = 336;
                                    //pedido.CodVencimentoVenda = 281;
                                    //pedido.RgClienteVenda = rg;
                                    //pedido.FipeValorVeiculoVenda = fipeValorVeiculoVenda;
                                    //pedido.RenavamVeiculoVend = renavamVeiculoVenda;
                                    //pedido.DataNascimentoClienteVenda = dataNascimento;
                                    //pedido.ContatoClienteVenda = contatoClienteVenda;
                                    //pedido.ProfissaoClienteVenda = profissaoClienteVenda;
                                    ////pedido.interveniente_venda =;
                                    //pedido.QuantidadeParcelaVenda = quantidadeParcelaVenda;
                                    //pedido.QuantidadeParcelaAdesaoVenda = quantidadeParcelaVenda;
                                    //pedido.ValorParcelaVenda = valorParcelaVenda / quantidadeParcelaVenda;
                                    //pedido.ValorParcelaAdesaoVenda = valorParcelaVenda / quantidadeParcelaVenda;
                                    //pedido.EntradaVenda = 1;
                                    //pedido.ObservacaoVenda = obs;
                                    //pedido.SexoClienteVenda = "M";
                                    //pedido.EstadoCivilClienteVenda = "SO";
                                    //pedido.CodDepartamento = 242;
                                    //pedido.CodFormaPagamentoVenda = 201;

                                    serviceReturn = pedidoService.Post(pedido);

                                    if (serviceReturn.Status == 500)
                                    {
                                        status = "20";
                                        motivo = "Processado com falha";
                                        eventoRetorno = serviceReturn.Message;
                                    }

                                    eventoRetorno = serviceReturn.Message;
                                }
                                //}

                            }
                        }
                    }

                    //Atualiza o evento   
                    object ret = BusinessObjectManager.UpdateFields(eventoBO,
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
                Log(ex);
            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado...");
        }

        private void ValidacaoToken()
        {
            JObject codigoModelo = BuscaModeloVeiculo("PALIO");


            //CookieContainer CookieContainer = new CookieContainer();

            ////HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUri}servicos/headers_authorization?cliente={3569}&nome={"moskit"}&senha={"2SI7WG"}");
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/}servicos/headers_authorization?" +
            //    $"cliente={"3569"}&" +
            //    $"nome={"moskit"}&" +
            //    $"senha={"2SI7WG"}");

            //request.Method = "POST";
            //request.Accept = "application/json";
            //request.CookieContainer = CookieContainer;

            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            //JObject ret = JObject.Parse(json);

            //string XAuthToken = ret["Headers"]["X-Auth-Token"].ToString();
            //string Authorization = ret["Headers"]["Authorization"].ToString();

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_veiculo_modelo/688472668f481b3efbddb0bfbff99cf6?descricao=PALIO");
            //request.Method = "GET";
            //request.Accept = "application/json";
            //request.Headers.Add("X-Auth-Token", "$2y$11$VsK54jZEuuauBgimeiSkreMPalWalHWJg4fs82vIvOw8uNsHyMksG");
            //request.Headers.Add("Authorization", "37f82f6bbd836751ce1f58d0cd76861bc8b5f00f");
            //request.KeepAlive = true;
            ////request.ServicePoint.ConnectionLimit = 10000;
            ////request.ContentType = "multipart/form-data";
            //request.ContentType = "application/x-www-form-urlencoded";
            ////request.CookieContainer = client.CookieContainer;

            //string json =
            //    $"nome_cliente={vo.NomeCliente}&" +
            //    $"cpf_cliente={vo.CpfCliente}&" +
            //    $"endereco_cliente[logradouro]={vo.EnderecoClienteLogradouro}&" +
            //    $"endereco_cliente[numero]={vo.EnderecoClienteNumero}&" +
            //    $"endereco_cliente[bairro]={vo.EnderecoClienteBairro}&" +
            //    $"endereco_cliente[cidade]={vo.EnderecoClienteCidade}&" +
            //    $"endereco_cliente[estado]={vo.EnderecoClienteEstado}&" +
            //    $"endereco_cliente[cep]={vo.EnderecoClienteCep}&" +
            //    //$"cliente={client.ApiKey}&" +
            //    $"cod_situacao_cliente={vo.CodSituacaoCliente}&" +
            //    $"cod_matriz_filial_cliente={vo.CodMatrizFilialCliente}&" +
            //    $"formato_envio_titulo_cliente[0]={vo.FormatoEnvioTituloCliente}&" +
            //    $"formato_boleto_cliente={vo.FormatoBoletoCliente}";

            //
            //Cadastro de cliente
            //
            SgrClient sgrClient = new SgrClient();
            //sgrClient.ApiUri = "https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_ponto_venda/688472668f481b3efbddb0bfbff99cf6";
            sgrClient.Token = "688472668f481b3efbddb0bfbff99cf6";// Parameters["PARAM_0009"]; //;"688472668f481b3efbddb0bfbff99cf6"; // 
            sgrClient.ApiKey = "3569"; //Parameters["PARAM_0010"]; //;"3569"; //
            //sgrClient.Logger = "3569";
            //sgrClient. = "$2y$11$VsK54jZEuuauBgimeiSkreMPalWalHWJg4fs82vIvOw8uNsHyMksG";

            sgrClient.Username = "moskit"; //Parameters["PARAM_0011"]; // ; "moskit"; // 
            sgrClient.Password = "2SI7WG"; // Parameters["PARAM_0012"]; //;"2SI7WG"; // 


            ClienteService clienteService = null;

            Cliente cliente = null;

            try
            {
                clienteService = new ClienteService(sgrClient);

                cliente = clienteService.Get("35170458886");
            }
            catch (Exception ex)
            {


            }

        }

        private JObject BuscaModeloVeiculo(string descricaoModelo)
        {
            int codigoModelo = 0;

            //if (CookieContainer == null || CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"))["laravel_session"].Expires < DateTime.Now)
            //{
            //}

            CookieContainer CookieContainer = new CookieContainer();
            //CookieContainer.GetCookies(new Cookie());
            CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));


            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUri}servicos/headers_authorization?cliente={3569}&nome={"moskit"}&senha={"2SI7WG"}");
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

            ///
            ///
            //HttpWebRequest 
            request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_veiculo_modelo/688472668f481b3efbddb0bfbff99cf6?descricao={descricaoModelo}");
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

            //codigoModelo = NumberUtilities.parseInt(ret["data"][0]["cod_veiculo_modelo"]);


            return ret; // codigoModelo;

        }

        private void SyncMoskitWhatapp()
        {
            object ret = null;

            Log($"SyncMoskitWhatapp iniciado...");

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
                        request.Headers.Add("Authorization", "Bearer " + Parameters["PARAM_0005"]);
                        request.Method = "POST";
                        request.Accept = "application/json";
                        request.ContentType = "application/json";

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            string json = "{" + $"\"number\":\"{telefone}\",\"instanceid\":\"{instanceid}\"" + "}";

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

                            atendimento = (JArray)JObject.Parse(StringUtilities.ToString(result))["atendimentos"];

                            foreach (var att in atendimento)
                            {

                                string getAtendimentoId = StringUtilities.ToString(att["id"]);

                                if (getAtendimentoId.Equals(idAtendimento))
                                {
                                    agenteId = StringUtilities.ToString(att["id_agente"]);

                                    JObject tags = JObject.Parse(StringUtilities.ToString(att["tags"]));

                                    agenteNome = StringUtilities.ToString(((Newtonsoft.Json.Linq.JProperty)tags.First).Value);

                                    break;
                                }
                            }

                            eventoRetorno = "{" + $"\"Id atendimento\": \"{idAtendimento}\", \"Id Agente\":\"{agenteId}\", \"Nome agente\": \"{agenteNome}\" " + "}";

                        }

                        long usuarioId = NumberUtilities.parseLong(Parameters["PARAM_0003"]);

                        UsuarioBO usuarioBO = BusinessObjectManager.FindByFilter<UsuarioBO>($"SYSAQ_AGENTE_ID = '{agenteId}' OR SYSAQ_USUARIO like '%{agenteNome}%'");

                        bool found = usuarioBO != null;

                        if (found)
                        {
                            if (usuarioBO.SYSAQ_AGENTE_ID == null)
                            {
                                usuarioBO.SYSAQ_AGENTE_ID = agenteId;

                                ret = BusinessObjectManager.Update(usuarioBO);

                                if (ret != null)
                                {
                                    break;
                                }
                            }

                            usuarioId = NumberUtilities.parseLong(usuarioBO.SYSAQ_NUMERO);
                        }

                        MoskitClient moskitClient = new MoskitClient();
                        moskitClient.Token = Parameters["PARAM_0001"];

                        ServiceReturn serviceReturn = new ServiceReturn();

                        //
                        // Criar contato
                        //
                        ContactService contactService = new ContactService(moskitClient);

                        Contact contact = (Contact)contactService.GetByFone(telefone);

                        long contactId = 0;

                        if (contact == null)
                        {
                            contact = (Contact)contactService.GetByFone(telefone.Substring(2));
                        }

                        if (contact == null)
                        {
                            contact = new Contact();
                            contact.DateCreated = DateTime.Now;
                            contact.Name = String.IsNullOrWhiteSpace(nome) ? telefone : nome;

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
                            string origem = contact.Origin;

                            if (origem.Equals("API V2"))
                            {
                                var param = JArray.Parse("[{\"field\": \"phones\",\"expression\": \"like\", \"values\": [\"" + telefone + "\" ]}]");

                                JArray results = (JArray)SendCommand("https://api.moskitcrm.com/v2/contacts/search", "POST", param, Parameters["PARAM_0001"]);

                                if (results.Count > 0)
                                {
                                    //
                                    // Se existe contato Atualizo o Contato
                                    //
                                    JObject contactUpdate = (JObject)results[0];

                                    contactId = NumberUtilities.parseLong(contactUpdate["id"]);

                                    contactUpdate["name"] = String.IsNullOrWhiteSpace(nome) ? contactUpdate["name"] : nome;
                                    contactUpdate["responsible"]["id"] = usuarioId;

                                    contactUpdate = (JObject)SendCommand($"https://api.moskitcrm.com/v2/contacts/{contactId}", "PUT", contactUpdate, Parameters["PARAM_0001"]);
                                }

                            }
                        }

                        status = "10";
                        motivo = "Processado";

                    }
                    catch (Exception ex)
                    {
                        status = "20";
                        motivo = "Processo com falhas";

                        Log(ex);

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
                Log(ex);
            }

            Log($"SyncMoskitWhatapp finalizado...");

        }
    }

    namespace Entities
    {
        class EventoBO : EventoVO
        {

        }

        [Table("usuan")]
        class EventoVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String USUAN_ID { get; set; }

            [Field]
            public int USUAN_FILIAL { get; set; }

            [Field]
            public int USUAN_CODIGO { get; set; }

            [Field]
            public int USUAN_TIPO { get; set; }

            [Field]
            public DateTime USUAN_DATA { get; set; }

            [Field]
            public String USUAN_EVENTO { get; set; }

            [Field]
            public String USUAN_STATUS { get; set; }

            [Field]
            public String USUAN_STATUS_MOTIVO { get; set; }

            [Field]
            public String USUAN_EVENTO_RETORNO { get; set; }
        }

        class IndicacaoOrigemBO : IndicacaoOrigemVO
        {

        }

        [Table("usuak")]
        class IndicacaoOrigemVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String USUAK_ID { get; set; }

            [Field]
            public int USUAK_FILIAL { get; set; }

            [Field]
            public int USUAK_CODIGO { get; set; }

            [Field]
            public int USUAK_NUMERO { get; set; }

            [Field]
            public string USUAK_DESCRICAO { get; set; }

            [Field]
            public int USUAK_CODIGO_SGR { get; set; }

            [Field]
            public bool USUAK_INATIVO { get; set; }
        }

        class ParametroBO : ParametroVO
        {

        }

        [Table("sysah")]
        class ParametroVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String SYSAH_ID { get; set; }

            [Field]
            public int SYSAH_FILIAL_COD { get; set; }

            [Field]
            public int SYSAH_CODIGO { get; set; }

            [Field]
            public String SYSAH_NOME { get; set; }

            [Field]
            public String SYSAH_VALOR { get; set; }

            [Field]
            public String SYSAH_DESCRICAO { get; set; }
        }

        class UsuarioBO : UsuarioVO
        {

        }

        [Table("sysaq")]
        class UsuarioVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public string SYSAQ_ID { get; set; }

            [Field]
            public int SYSAQ_FILIAL_COD { get; set; }

            [Field]
            public int SYSAQ_CODIGO { get; set; }

            [Field]
            public string SYSAQ_NUMERO { get; set; }

            [Field]
            public string SYSAQ_USUARIO { get; set; }

            [Field]
            public string SYSAQ_EMAIL { get; set; }


            [Field]
            public string SYSAQ_SENHA { get; set; }

            [Field]
            public string SYSAQ_FUNIL { get; set; }

            [Field]
            public string SYSAQ_AGENTE_ID { get; set; }

            [Field]
            public string SYSAQ_TIPO { get; set; }


            [Field]
            public int SYSAQ_EQUIPE_ID { get; set; }

            [Field]
            public string SYSAQ_EQUIPE_NAME { get; set; }

            [Field]
            public bool SYSAQ_PARTICIPA_ROLETA { get; set; }

            [Field]
            public bool SYSAQ_USADO_ROLETA { get; set; }

            [Field]
            public bool SYSAQ_INATIVO { get; set; }

            [Field]
            public int SYSAQ_CODIGO_VENDEDOR_SGR { get; set; }

            [Field]
            public int SYSAQ_CODIGO_VENDEDOR_SGA { get; set; }

            [Field]
            public DateTime SYSAQ_DATA_ATUALIZACAO { get; set; }

        }
    }


}


//
//Backup 29/11/2022
//

 private void SyncSinconizarIndicacao()
        {

            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {
                int quantity = 50;
                int start = 0;

                //string apiMoskit = ToString(GetCustomParameter("PARAM_0001"));

                //
                // Busca origem do leads e fazer um de para com a indicação
                //
                do
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    JArray customFields = (JArray)SendCommand2($"https://api.moskitcrm.com/v2/customFields/CF_49RM16ixiB7nbmBW/options?start={start}&quantity={quantity}", "GET", null, ToString(GetCustomParameter("PARAM_0001")));

                    //JObject campos = (JObject)SendCommand2("https://api.moskitcrm.com/v2/customFields/CF_49RM16ixiB7nbmBW/options", "GET", null, ToString(GetCustomParameter("PARAM_0001"));

                    if (customFields.Count == 0)
                    {
                        return;
                    }

                    foreach (JObject customField in customFields)
                    {
                        if (!IsRunning)
                        {
                            return;
                        }

                        int id = NumberUtilities.parseInt(customField["id"]);
                        String descricao = StringUtilities.ToString(customField["label"]);

                        IndicacaoOrigemBO indicacaoOrigemBO = BusinessObjectManager.FindByFilter<IndicacaoOrigemBO>($"USUAK_FILIAL={"0"} AND USUAK_NUMERO={id}");

                        bool found = indicacaoOrigemBO != null;

                        if (!found)
                        {
                            indicacaoOrigemBO = new IndicacaoOrigemBO();

                            indicacaoOrigemBO.USUAK_FILIAL = NumberUtilities.parseInt("0");
                            indicacaoOrigemBO.USUAK_CODIGO = GetNumerator("0", "usuak");
                        }

                        indicacaoOrigemBO.USUAK_NUMERO = id;
                        indicacaoOrigemBO.USUAK_DESCRICAO = descricao;
                        indicacaoOrigemBO.USUAK_CODIGO_SGR = 0;

                        object ret = null;

                        if (!found)
                        {
                            ret = BusinessObjectManager.Insert(indicacaoOrigemBO);
                        }
                        else
                        {
                            ret = BusinessObjectManager.Update(indicacaoOrigemBO);
                        }

                        if (ret != null)
                        {
                            throw new Exception(StringUtilities.ToString(ret));
                        }

                        //registro++;
                        Thread.Sleep(3000);
                    }
                    start++;

                } while (true);
            }
            catch (Exception ex)
            {

            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado");
        }

        private void SyncSinconizarPontoVenda()
        {

            Log($"SyncCriarErpSgrSgaLeadsGanhos iniciado...");

            try
            {

                //if (CookieContainer == null || CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"))["laravel_session"].Expires < DateTime.Now)
                //{
                //}

                CookieContainer CookieContainer = new CookieContainer();
                //CookieContainer.GetCookies(new Cookie());
                CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));


                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUri}servicos/headers_authorization?cliente={3569}&nome={"moskit"}&senha={"2SI7WG"}");
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

                ///
                ///
                //HttpWebRequest 
                request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_ponto_venda/688472668f481b3efbddb0bfbff99cf6");
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
            }
            catch (Exception ex)
            {

            }

            Log($"SyncCriarErpSgrSgaLeadsGanhos finalizado");
        }


          private string GerarTokenSga()
        {
            string tokenSga = "";

            try
            {
                HttpWebRequest
                request = (HttpWebRequest)WebRequest.Create($"https://api.hinova.com.br/api/sga/v2/usuario/autenticar");
                request.Method = "POST";
                request.Accept = "application/json";
                request.Headers.Add("Authorization", "Bearer " + "0b638bf287c8a6683375f6bd044f616009761bbba8e4cae10bd580d509eeee2bd19dd6cf87b22f6a16c489d24660b0b2d855365863d16cc36c591369689fb3b70f0012bc5c7bb8f1c4b9916e1b96ecc7e8a615c89cd7038039396b6125e504c6");
                //request.Headers.Add("X-Access-Token", "0b638bf287c8a6683375f6bd044f616009761bbba8e4cae10bd580d509eeee2bd19dd6cf87b22f6a16c489d24660b0b2d855365863d16cc36c591369689fb3b70f0012bc5c7bb8f1c4b9916e1b96ecc7e8a615c89cd7038039396b6125e504c6");
                //request.Headers.Add("Authorization", Authorization);
                request.KeepAlive = true;
                //request.ServicePoint.ConnectionLimit = 10000;
                //request.ContentType = "multipart/form-data";
                //request.ContentType = "application/x-www-form-urlencoded";
                //request.CookieContainer = CookieContainer;

                JObject note = new JObject(
                                               new JProperty("usuario", "moskit"),
                                               new JProperty("senha", "2SI7WG#")
                                              );

                string json = JsonConvert.SerializeObject(note, Formatting.Indented);


                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var result = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());

                //var results = (JObject)result["token_usuario"];


                tokenSga = ToString(result["token_usuario"]);

                //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //JObject ret = JObject.Parse(json);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            return tokenSga;
        }


        // Monitora os usuarios do Moskit
        private void SyncMonitoraUsuarioMoskit()
        {

            Log("SyncMonitoraUsuarioMoskit iniciado...");

            try
            {
                MoskitProvider moskitProvider = GetProvider<MoskitProvider>("194998e8-7ca5-44db-8270-10d0c5c3f819");

                IEnumerable<UserMoskit> users = moskitProvider.List<UserMoskit>();

                foreach (UserMoskit user in users)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    ProviderResult providerResult = Get<UsuarioBO>(
                        $"SYSAQ_FILIAL_COD='{"0"}' AND " +
                        $"SYSAQ_NUMERO={user.Id}"
                    );

                    UsuarioBO userVSIntegra = (UsuarioBO)providerResult.Detail;

                    bool found = userVSIntegra != null;

                    if (!found)
                    {
                        userVSIntegra = new UsuarioBO();

                        userVSIntegra.SYSAQ_FILIAL_COD = 0;
                        userVSIntegra.SYSAQ_CODIGO = 0;
                        userVSIntegra.SYSAQ_NUMERO = user.Id.ToString(); ;

                        userVSIntegra.SYSAQ_PARTICIPA_ROLETA = false;
                        userVSIntegra.SYSAQ_USADO_ROLETA = false;
                        userVSIntegra.SYSAQ_INATIVO = !user.Active;
                    }

                    providerResult = moskitProvider.Get<TeamMoskit>(user.Team.Id);

                    TeamMoskit teamMoskit = (TeamMoskit)providerResult.Detail;

                    userVSIntegra.SYSAQ_USUARIO = user.Username;
                    userVSIntegra.SYSAQ_EQUIPE_ID = ToInt(user.Team.Id);
                    userVSIntegra.SYSAQ_EQUIPE_NAME = teamMoskit.Name;
                    userVSIntegra.SYSAQ_EMAIL = user.Username;

                    if (!userVSIntegra.SYSAQ_INATIVO.Equals(!user.Active))
                    {
                        userVSIntegra.SYSAQ_INATIVO = !user.Active;
                    }

                    providerResult = !found ? Post(userVSIntegra) : Put(userVSIntegra);

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

            Log("SyncMonitoraUsuarioMoskit finalizado");
        }


         namespace Entities
    {

        class UserBO : UserVO
        {

        }

        [Table("sysaj")]

        class UserVO
        {
            [Field("SYSAJ_CALENDLY_KEY")]
            public string CalendlyKey { get; set; }
            [Field("SYSAJ_INACTIVE")]
            public bool Inactive { get; set; }
            [Field("SYSAJ_LOG")]
            public bool Logged { get; set; }
            [Field("SYSAJ_ROULETTE_USED")]
            public bool RouletteUsed { get; set; }
            [Field("SYSAJ_ROULETTE_JOIN")]
            public bool RouletteJoin { get; set; }
            [Field("SYSAJ_TEAM_ID")]
            public string TeamId { get; set; }
            [Field("SYSAJ_PHONE_NUMBER")]
            public string PhoneNumber { get; set; }
            [Field("SYSAJ_CALENDLY_TOKEN")]
            public string CalendlyToken { get; set; }
            [Field("SYSAJ_PASSWORD")]
            public string Password { get; set; }
            [Field("SYSAJ_TYPE")]
            public string Type { get; set; }
            [Field("SYSAJ_EMAIL")]
            public string Email { get; set; }
            [Field("SYSAJ_NAME")]
            public string Name { get; set; }
            [Field("SYSAJ_NUMBER")]
            public string Number { get; set; }
            [Field("SYSAJ_CODE")]
            public int Code { get; set; }
            [Field("SYSAJ_COMPANY_ID")]
            [IsStoreField]
            public string Store { get; set; }
            [Field("SYSAJ_ID")]
            [IsKey]
            [IsUUID]
            public string Id { get; set; }
            [Field("SYSAJ_USERNAME")]
            public string Username { get; set; }

            [Field("SYSAJ_CALENDLY_ORGANIZATION")]
            public string CalendlyOrganization { get; set; }

            [Field("SYSAJ_CODE_INTEGRATIONS")]
            public string CodeIntegrations { get; set; }
        }



        class EventoBO : EventoVO
        {

        }

        [Table("usuan")]
        class EventoVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String USUAN_ID { get; set; }

            [Field]
            public int USUAN_FILIAL { get; set; }

            [Field]
            public int USUAN_CODIGO { get; set; }

            [Field]
            public int USUAN_TIPO { get; set; }

            [Field]
            public DateTime USUAN_DATA { get; set; }

            [Field]
            public String USUAN_EVENTO { get; set; }

            [Field]
            public String USUAN_STATUS { get; set; }

            [Field]
            public String USUAN_STATUS_MOTIVO { get; set; }

            [Field]
            public String USUAN_EVENTO_RETORNO { get; set; }
        }

        class IndicacaoOrigemBO : IndicacaoOrigemVO
        {

        }

        [Table("6385f344dc11d_usuak")]
        class IndicacaoOrigemVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String USUAK_ID { get; set; }

            [Field]
            public int USUAK_FILIAL { get; set; }

            [Field]
            public int USUAK_CODIGO { get; set; }

            [Field]
            public int USUAK_NUMERO { get; set; }

            [Field]
            public string USUAK_DESCRICAO { get; set; }

            [Field]
            public int USUAK_CODIGO_SGR { get; set; }

            [Field]
            public bool USUAK_INATIVO { get; set; }
        }

        class ParametroBO : ParametroVO
        {

        }

        [Table("sysah")]
        class ParametroVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public String SYSAH_ID { get; set; }

            [Field]
            public int SYSAH_FILIAL_COD { get; set; }

            [Field]
            public int SYSAH_CODIGO { get; set; }

            [Field]
            public String SYSAH_NOME { get; set; }

            [Field]
            public String SYSAH_VALOR { get; set; }

            [Field]
            public String SYSAH_DESCRICAO { get; set; }
        }

        class UsuarioBO : UsuarioVO
        {

        }

        [Table("sysaq")]
        class UsuarioVO : BusinessObjectBase
        {
            [Field]
            [IsKey]
            [IsUUID]
            public string SYSAQ_ID { get; set; }

            [Field]
            public int SYSAQ_FILIAL_COD { get; set; }

            [Field]
            public int SYSAQ_CODIGO { get; set; }

            [Field]
            public string SYSAQ_NUMERO { get; set; }

            [Field]
            public string SYSAQ_USUARIO { get; set; }

            [Field]
            public string SYSAQ_EMAIL { get; set; }


            [Field]
            public string SYSAQ_SENHA { get; set; }

            [Field]
            public string SYSAQ_FUNIL { get; set; }

            [Field]
            public string SYSAQ_AGENTE_ID { get; set; }

            [Field]
            public string SYSAQ_TIPO { get; set; }


            [Field]
            public int SYSAQ_EQUIPE_ID { get; set; }

            [Field]
            public string SYSAQ_EQUIPE_NAME { get; set; }

            [Field]
            public bool SYSAQ_PARTICIPA_ROLETA { get; set; }

            [Field]
            public bool SYSAQ_USADO_ROLETA { get; set; }

            [Field]
            public bool SYSAQ_INATIVO { get; set; }

            [Field]
            public int SYSAQ_CODIGO_VENDEDOR_SGR { get; set; }

            [Field]
            public int SYSAQ_CODIGO_VENDEDOR_SGA { get; set; }

            //[Field]
            //public DateTime SYSAQ_DATA_ATUALIZACAO { get; set; }

        }


        class AtividadesBO
        {
            public Dictionary<int, string[]> Atividades { get; set; } = new Dictionary<int, string[]>();

            public AtividadesBO()
            {
                Atividades.Add(85, new string[] { "1", "Academia Gin/Dança" });

            }
        }
    }


    //
    //06/02/2023
    //
     //                        if (!IsNullOrEmpty(email))
                        //                        {
                        //                            SgrEmail emailVenda = new SgrEmail();
                        //                            emailVenda.Contato = email;
                        //                            emailVenda.CodDepartamento = "779"; //Sem informação

                        //                            pedido.EmailVenda = emailVenda;
                        //                        }

                        //                        if (!IsNullOrEmpty(telefone))
                        //                        {
                        //                            SgrTelefone telefoneVenda = new SgrTelefone();
                        //                            telefoneVenda.Contato = telefone;
                        //                            telefoneVenda.Tipo = "FIXO";
                        //                            telefoneVenda.CodDepartamento = "779"; //Sem informação

                        //                            pedido.TelefoneVenda = telefoneVenda;
                        //                        }

                        //                        pedido.NomeClienteVenda = nome;


                        //                        //pedido = (Pedido)providerResult.Detail;
                        //                        //
                        //                        // Gero um evento do pedido
                        //                        //
                        Event eventBO = new Event();
//                        //<<<<<<< HEAD
//                        eventBO.Store = Store;
//                        eventBO.Code = GetNumerator(Store, "sysat");
//                        eventBO.Number = ToString(pedido.CodVenda);
//                        eventBO.Type = 1;
//                        eventBO.CreateDateTime = DateTime.Now;
//                        eventBO.EventText = JsonConvert.SerializeObject(pedido);
//                        //throw new DiscardedException(FormatString("Negocio: {0} {1} com status: {2}", dealId, nomeNegocio, status));
//                        //eventBO.EventText = JsonConvert.SerializeObject(FormatString("{ \"codVenda\":\"{0}\", \"emailContato\":\"{1}\", \"nome\":\"{2}\"}", pedido.CodVenda, email, nome));
//                        eventBO.Hash = CryptoUtilities.GetMD5Hash(pedido.CodVenda);
//=======
                        //eventBO.Store = Store; ??????????????????????????? isso nao precisa mais
                        //eventBO.Code = GetNumerator(Store, "sysat"); ??????????????????????????? isso nao precisa mais
                        //eventBO.Number = ToString(pedido.CodVenda); ??????????????????????????? isso nao precisa mais
                        eventBO.Type = 2;
                        eventBO.CreateDateTime = DateTime.Now;
                        //eventBO.EventText = JsonConvert.SerializeObject(""); ??????????????????????????? 
                        eventBO.EventText = ToJson(pedido);
                        //eventBO.Hash = CryptoUtilities.GetMD5Hash(pedido); ??????????????????????????? 
                        eventBO.Hash = ToHash(pedido);
//>>>>>>> 878fafc7d775d35db38a5a7c562263434fb8279e
                        eventBO.Status = "00";
                        eventBO.StatusReason = "Aguardando processamento";
                        eventBO.StatusDetail = null;

                        providerResult = Add(eventBO);

                        if (providerResult.Status == 500)
                        {
                            throw new Exception(providerResult.Message);
                        }


                        //
                        //Backup 14/02/2023
                        //

                        using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VSIntegra.Framework.Core;
using VSIntegra.Framework.Model.Crm;
using VSIntegra.Framework.Model.System;
using VSIntegra.Framework.Provider;
using VSIntegra.Framework.Provider.D4Sign;
using VSIntegra.Framework.Provider.D4Sign.Model;
using VSIntegra.Framework.Provider.Download;
using VSIntegra.Framework.Provider.Download.Model;
using VSIntegra.Framework.Provider.Moskit;
using VSIntegra.Framework.Provider.Moskit.Model;
using VSIntegra.Framework.Provider.Sga;
using VSIntegra.Framework.Provider.Sga.Model;
using VSIntegra.Framework.Provider.Sgr;
using VSIntegra.Framework.Provider.Sgr.Model;

namespace Sync.Custom
{
    class Program : TaskBase
    {
        //
        // Para debug
        //
        private static void Main()
        {
            string INTEGRATION_ID = "4e70ceb8-0a0a-45a5-b48e-af880108fab0";

            Program worker = new Program();
            worker.IntegrationId = INTEGRATION_ID;
            worker.IsRunning = true;
            worker.DebugMode = true;

            WorkerService workerService = new WorkerService(worker);
            workerService.Execute();

            Console.WriteLine("inicializado");
            Console.WriteLine("pressione qualquer tecla para fechar");

            Console.ReadLine();
        }

        //
        // Monitora os usuarios do Moskit e atualiza o vsintegra
        //
        // ROTINA MIGRADA PARA O VSINTEGRA WEB V6, SE ALTERAR AQUI DEVE ALTERAR LÁ
        private void TaskMonitoraUsuarioMoskit()
        {
            Log("TaskMonitoraUsuarioMoskit iniciado...");

            try
            {
                Provider moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");

                IEnumerable<MoskitUser> users = moskitProvider.List<MoskitUser>();

                foreach (MoskitUser userMoskit in users)
                {
                    //if (!IsRunning)
                    //{
                    //    return;
                    //}

                    ProviderResult providerResult = Find<User>(
                        FormatString("Number LIKE '%{0}%'", userMoskit.Id)
                    );

                    if (providerResult.Status == 500)
                    {
                        throw new Exception(providerResult.Message);
                    }

                    User userVSIntegra = (User)providerResult.Detail;

                    bool found = userVSIntegra != null;

                    if (!found)
                    {
                        userVSIntegra = new User();

                        userVSIntegra.Store = Store;
                        userVSIntegra.Code = GetNumerator(Store, "sysaj");
                        userVSIntegra.Number = ToString(userMoskit.Id);
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

                    providerResult = !found ? Add(userVSIntegra) : Update(userVSIntegra);

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

            Log("TaskMonitoraUsuarioMoskit finalizado");
        }

        //
        // Monitora os negocios do Moskit e cria o pedido no SGR
        //
        public void TaskCriaPedidoErpSgrSgaParaLeadsGanhos()
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
                MoskitCustomField customField = null; ;

                IEnumerable events = List<Event>("Type=0 AND Status='00'", "CreateDateTime ASC");

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

                        //if (!IsNullOrEmpty(numeroPedido))
                        //{
                        //    throw new DiscardedException(FormatString("Pedido já cadastrado para o negócio: {0} - {1}", deal.Name, numeroPedido));
                        //}

                        //
                        // Busco os dados do contato
                        //
                        //long contatoId = 0;
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

                                //cpfCnpj =  MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_0WGqoGSKC9zK2qnP")?.TextValue;
                                //rg = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGSvC6jZKm4X")?.TextValue;
                                //cidade = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_42AmakSZCwrAPqjl")?.TextValue;
                                //bairro = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_wPVm2oS2Cbj9gDK6")?.TextValue;
                                //cep = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_Pj3qYoSeCrBpamQe")?.TextValue;
                                //numero = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_Rg7Mn4SLCA1XrDvd")?.TextValue;
                                //logradouro = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_ylAm0KS6C5p91Mvb")?.TextValue;
                                //complemento = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_dVKmQAS1CdPoemWR")?.TextValue;
                                //options = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGS9i6jZpm4X")?.Options;

                                cpfCnpj = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_0WGqoGSKC9zK2qnP") != null ? 
                                          MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_0WGqoGSKC9zK2qnP").TextValue : null;
                                rg = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGSvC6jZKm4X") != null?
                                     MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_6rRmwGSvC6jZKm4X").TextValue : null;
                                cidade = MoskitCustomField.GetCustomFieldById(entityCustomFieldsContact, "CF_42AmakSZCwrAPqjl") != null?
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

                        //
                        //
                        //
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

                        //JObject codigosIntegracoes = JObject.Parse(userVSIntegra.Number);
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
                        options = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_wPVm2VijibjB8mK6") != null?
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
                                    throw new Exception(FormatString("Produto não informado {0}", deal.Name));
                                }

                                nomeProduto = ToString(product.Name);
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

                        // Quando o produto do negócio for Plano Light, Plano Plus, Plano Prime o produto no SGR é PROTEÇÃO VEICULAR
                        if (nomeProduto.ToUpper().Equals("RASTREAMENTO") || tipoAssociadoRastreador.ToUpper().Equals("CLIENTE/RASTREADOR"))
                        {
                            tipoProduto = 1;
                        }
                        else if (nomeProduto.ToUpper().Contains("PLANO") || tipoAssociadoRastreador.ToUpper().Equals("ASSOCIADO"))
                        {
                            tipoProduto = 2;
                        }
                        else if (nomeProduto.ToUpper().Contains("RASTREAMENTO + ASSISTENCIA"))
                        {
                            tipoProduto = 3;
                        }

                        if (tipoProduto == 0)
                        {
                            throw new Exception(FormatString("Produto não informado {0}", deal.Name));
                        }

                        //
                        // Busca campos personalizados
                        //
                        nome = string.IsNullOrWhiteSpace(nome) ? deal.Name : nome;

                        //string email = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5")?.TextValue;

                        //email = string.IsNullOrWhiteSpace(email) ? emailContato : email;

                        //string telefone1 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_075MJ2izS6yRkMaz")?.TextValue;

                        //telefone1 = string.IsNullOrWhiteSpace(telefone1) ? telefoneContato : telefone1;

                        //string telefone2 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_x1kq6oinCwZd5MzY")?.TextValue;

                        //string telefone3 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_KaZmKNiOC08ldMJk")?.TextValue;

                        //cpfCnpj = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR")?.TextValue;

                        string email = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5") != null ?
                                       MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3NrDZAinCJAzemP5").TextValue : null;

                        email = string.IsNullOrWhiteSpace(email) ? emailContato : email;

                        string telefone1 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_075MJ2izS6yRkMaz") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_075MJ2izS6yRkMaz").TextValue: null;

                        telefone1 = string.IsNullOrWhiteSpace(telefone1) ? telefoneContato : telefone1;

                        string telefone2 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_x1kq6oinCwZd5MzY") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_x1kq6oinCwZd5MzY").TextValue : null;

                        string telefone3 = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_KaZmKNiOC08ldMJk") != null ?
                                           MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_KaZmKNiOC08ldMJk").TextValue : null;

                        cpfCnpj = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR") != null ?
                                  MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_dVKmQ5i1CdPXwmWR").TextValue : null;

                        cpfCnpj = cpfCnpj != null ? Regex.Replace(cpfCnpj, "[^0-9,]", "") : null;

                        cpfCnpj = ClearString(cpfCnpj);

                        if (string.IsNullOrWhiteSpace(cpfCnpj))
                        {
                            //throw new Exception($"Cnpj não cadastrado para o negócio: {deal.Name}");
                        }

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

                        //long cepNumero = ToLong(cep);
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
                            numero = Regex.Replace(numero, "[^0-9,]", "").Length != 8 ? null : Regex.Replace(numero, "[^0-9,]", "");
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

                        //GRUPO MENSALIDADE + PERÍODO, coloquei em um mesmo campo customizado no moskit
                        //porque o período está relacionado com a escolha do grupo de mensalidade.
                        int codPeriodoVenda = 0;
                        int codGrupoVenda = 0;

                        //Verificar se o campos quantidade, valor parcela e valor mensalidade atualiza sozinho
                        //Na tela ao selecionar o grupo e periodo esses campos são automáticos.
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
                        //Verificar se é no campo cod_forma_pagamento_venda que grava a condição no pedido

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
                        //Verificar se é no campo cod_forma_pagamento_venda que grava a condição no pedido

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
                            valorAdesao = MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNbalm6a") != null?
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
                                              ToString(MoskitCustomField.GetCustomFieldById(entityCustomFields, "CF_3LvDvEi4CNw8vm6a").TextValue): null;

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
                        // Modelo do veículo, montar uma lista de veículos porque cada veículo deve-se gerar um pedido.
                        //
                        modeloVeiculo = veiculo1;

                        SgrProviderParam sgrProviderParam = new SgrProviderParam();
                        sgrProviderParam.VeiculoModeloDescricao = modeloVeiculo;

                        List<SgrVeiculoModelo> modelos = sgrProvider.List<SgrVeiculoModelo>(sgrProviderParam).ToList();

                        int codModeloVeiculo = 0;
                        int codMontadoraVeiculo = 0;
                        int tipoVeiculo = 0;

                        if (modelos.Count > 0)
                        {
                            int i = 0;

                            codModeloVeiculo = ToInt(modelos[i].CodVeiculoModelo);
                            codMontadoraVeiculo = ToInt(modelos[i].CodVeiculoMontadora);

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

                        //
                        // Gravo cliente no SGA
                        //
                        cpfCnpj = cpfCnpj.Length == 14 ? cpfCnpj : cpfCnpj.Length == 11 ? cpfCnpj : "";

                        if (tipoProduto == 2 && !IsNullOrEmpty(cpfCnpj))
                        {
                            SgaProviderParam param = new SgaProviderParam();
                            param.ClienteCnpjCpf = cpfCnpj;

                            providerResult = sgaProvider.Find<Associado>(param);

                            if (providerResult.Status == 500)
                            {
                                throw new Exception(providerResult.Message);
                            }

                            Associado associado = (Associado)providerResult.Detail;

                            found = associado != null;

                            if (!found)
                            {
                                associado = new Associado();
                                associado.Nome = nome;
                                associado.Sexo = IsNullOrEmpty(sexo) ? null : sexo;
                                associado.Cpf = cpfCnpj;
                                associado.Rg = rg == null ? "00.000.000-0" : rg;
                                associado.DataNascimento = FormatDate(dataNascimento, "dd/MM/yyyy");
                                associado.Telefone = telefone1;
                                associado.Celular = celular;
                                associado.Email = IsNullOrEmpty(emails) ? email : emails;
                                associado.Logradouro = logradouro;
                                associado.Numero = numero;
                                associado.Complemento = complemento;
                                associado.Bairro = bairro;
                                associado.Cidade = cidade;
                                associado.Estado = estado;
                                associado.Cep = cep == null ? "00000-000" : cep;
                                associado.DiaVencimento = 10;
                                associado.CodigoVoluntario = codigoVendedorSga == 0 ? 1 : codigoVendedorSga; // 1;
                                associado.Rg = "00.000.000-0";

                                providerResult = sgaProvider.Add(associado);

                                if (providerResult.Status == 500)
                                {
                                    throw new Exception(providerResult.Message);
                                }

                                associado = (Associado)providerResult.Detail;
                            }
                        }

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

                        //
                        //
                        //
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

                            // pedido.TelefoneVenda = telefoneVenda;
                            telefones.Add(telefoneVenda);
                        }

                        if (!IsNullOrEmpty(telefone2))
                        {
                            SgrTelefone telefoneVenda = new SgrTelefone();
                            telefoneVenda.Contato = telefone2;
                            telefoneVenda.Tipo = "FIXO";
                            telefoneVenda.CodDepartamento = "779"; //Sem informação

                            // pedido.TelefoneVenda = telefoneVenda;
                            telefones.Add(telefoneVenda);
                        }

                        if (!IsNullOrEmpty(telefone3))
                        {
                            SgrTelefone telefoneVenda = new SgrTelefone();
                            telefoneVenda.Contato = telefone3;
                            telefoneVenda.Tipo = "FIXO";
                            telefoneVenda.CodDepartamento = "779"; //Sem informação

                            // pedido.TelefoneVenda = telefoneVenda;
                            telefones.Add(telefoneVenda);
                        }

                        pedido.TelefoneVenda = telefones.ToArray();

                        //
                        //
                        //
                        if (!IsNullOrEmpty(email))
                        {
                            SgrEmail emailVenda = new SgrEmail();
                            emailVenda.Contato = email;
                            emailVenda.CodDepartamento = "779"; //Sem informação

                            pedido.EmailVenda = emailVenda;
                        }

                        pedido.NomeClienteVenda = nome;
                        pedido.CpfClienteVenda = cpfCnpj;
                        pedido.RgClienteVenda = rg == null ? "00.000.000-0" : rg;
                        pedido.SexoClienteVenda = sexo;
                        pedido.EstadoCivilClienteVenda = estadoCivil;
                        pedido.ProfissaoClienteVenda = profissao == null ? "" : profissao;
                        pedido.DataNascimentoClienteVenda = dataNascimento;

                        //4) PRODUTO, 
                        pedido.ProdutoVendaCodProduto = tipoProduto; //1;

                        //if (codGrupoVenda == 0)
                        //{
                        pedido.ValorParcelaAdesaoVenda = price;
                        pedido.ValorParcelaVenda = price;
                        pedido.FipeValorVeiculoVenda = price;
                        //}

                        pedido.QuantidadeParcelaVenda = quantidadeParcelaVenda;
                        pedido.QuantidadeParcelaAdesaoVenda = 1;

                        //anoFabricacao = anoFabricacao.Length == 4 ? anoFabricacao : "";

                        //8) DADOS DO VEICULO: MODELO/ANO FABRICAÇÃO E ANO MODELO / MONTADORA / PLACA / CHASSI, 
                        //pedido.AnoModVeiculoVenda = anoModelo.Equals("0") ? "" : anoModelo;
                        //pedido.AnoFabVeiculoVenda = anoFabricacao.Equals("0") ? "" : anoFabricacao;

                        pedido.AnoModVeiculoVenda = anoModelo;
                        pedido.AnoFabVeiculoVenda = anoFabricacao;
                        pedido.PlacaVeiculoVenda = placa;
                        pedido.ChassiVeiculoVenda = chassi;
                        pedido.RenavamVeiculoVenda = renavam;
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
                            //pedido.ValorParcelaAdesaoVenda = valorAdesao;
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

                         //ev.StatusDetail = FormatString("Venda: {0}", pedido.CodVenda);

                        ev.StatusDetail = providerResult.Message;

                        //
                        //Pedido da venda
                        //
                        SgrPedido pedidoRetorno = (SgrPedido)providerResult.Detail;

                        pedido.CodVenda = pedidoRetorno.CodVenda;

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
        private void MonitorarPedidosSgrAguardandoAssinaturaParaD4Sign()
        {
            Log("MonitorarPedidosSgrAguardandoAssinaturaParaD4Sign iniciado...");

            try
            {
                ProviderResult result = null;

                Provider sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                Provider d4signProvider = GetProvider<D4SignProvider>("464998e8-7ca5-44db-8270-10d0c5c3f849");
                DownloadProvider downloadProvider = new DownloadProvider();

                IEnumerable events = List<Event>("Type=1 AND Status='00'", "CreateDateTime ASC");

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
                        string filename = ToString(GetCustomParameter("PEDIDO_TESTE"));

                        result = downloadProvider.Find<DownloadDocument>(filename);

                        if (result.Status == 500)
                        {
                            throw new Exception(result.Message);
                        }

                        DownloadDocument downloadDocument = (DownloadDocument)result.Detail;

                        byte[] bytes = downloadDocument.Document;

                        if (bytes.Length == 0)
                        {
                            throw new FailedException("O arquivo de pedido informado é invalido");
                        }

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
                        filename = ToString(GetCustomParameter("D4SIGN_CONTRATO_PADRAO"));

                        result = downloadProvider.Find<DownloadDocument>(filename);

                        if (result.Status == 500)
                        {
                            throw new Exception(result.Message);
                        }

                        downloadDocument = (DownloadDocument)result.Detail;

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
    }
}


//
//Backup 09/03/2023
//

//
                        //Não integrar mais com o SGA segundo o Eder.
                        //

                        //if (tipoProduto == 2 && !IsNullOrEmpty(cpfCnpj))
                        //{
                        //    SgaProviderParam param = new SgaProviderParam();
                        //    param.ClienteCnpjCpf = cpfCnpj;

                        //    providerResult = sgaProvider.Find<Associado>(param);

                        //    if (providerResult.Status == 500)
                        //    {
                        //        throw new Exception(providerResult.Message);
                        //    }

                        //    Associado associado = (Associado)providerResult.Detail;

                        //    found = associado != null;

                        //    if (!found)
                        //    {
                        //        associado = new Associado();
                        //        associado.Nome = nome;
                        //        associado.Sexo = IsNullOrEmpty(sexo) ? null : sexo;
                        //        associado.Cpf = cpfCnpj;
                        //        associado.Rg = rg == null ? "00.000.000-0" : rg;
                        //        associado.DataNascimento = FormatDate(dataNascimento, "dd/MM/yyyy");
                        //        associado.Telefone = telefone1;
                        //        associado.Celular = celular;
                        //        associado.Email = IsNullOrEmpty(emails) ? email : emails;
                        //        associado.Logradouro = logradouro;
                        //        associado.Numero = numero == null ? "s/n" : numero;
                        //        associado.Complemento = complemento;
                        //        associado.Bairro = bairro;
                        //        associado.Cidade = cidade;
                        //        associado.Estado = estado;
                        //        associado.Cep = cep == null ? "00000-000" : cep;
                        //        associado.DiaVencimento = 10;
                        //        associado.CodigoVoluntario = codigoVendedorSga == 0 ? 1 : codigoVendedorSga; // 1;
                        //        associado.Rg = "00.000.000-0";

                        //        providerResult = sgaProvider.Add(associado);

                        //        if (providerResult.Status == 500)
                        //        {
                        //            throw new Exception(providerResult.Message);
                        //        }
                        //        associado = (Associado)providerResult.Detail;
                        //    }
                        //}

