
        private void TestarHttpRequest()
        {
            Log("MonitorarClienteSgrParaMovidesk iniciado...");

            try
            {
                string dataInicio = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");

                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

                SgrProviderParam sgrProviderParam = new SgrProviderParam();
                sgrProviderParam.ClienteUltimaAtualizacao = DateTime.Now;

                List<SgrCliente> modelos = sgrProvider.List<SgrCliente>(sgrProviderParam).ToList();

                CookieContainer CookieContainer = new CookieContainer();
                CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                string url = $"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                    $"cliente={"3569"}&" +
                    $"nome={"moskit"}&" +
                    $"senha={"2SI7WG"}";

                //request.CookieContainer = CookieContainer;

                HttpClient client = new HttpClient();
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Content = new StringContent("", null, "application/json")
                    //null
                };

                var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                var responseInfo = response1.GetAwaiter().GetResult();

                string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                JObject result = JObject.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                if (!responseInfo.IsSuccessStatusCode)
                {
                    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        result1 = null;
                    }
                    else
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                }

                string XAuthToken = ToString(result["Headers"]["X-Auth-Token"]);
                string Authorization = ToString(result["Headers"]["Authorization"]);

                int indice = 0;


                //CookieContainer CookieContainer = new CookieContainer();
                //CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                //
                //Buscar cadastros de clientes
                //
                url = $"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/buscar_cliente/688472668f481b3efbddb0bfbff99cf6?ultima_atualizacao=2023-08-01 00:30:53&indice=0&total=200";


                //request.CookieContainer = CookieContainer;
                //httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                //httpWebRequest.Headers.Add("Authorization", Authorization);

                client = new HttpClient();
                httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Content = new StringContent("", null, "application/json"),
                    //Headers = new HttpRequestHeaders(Authorization, "Basic QVJNSVpFQ09NRVJDSU9fUFJEOnJDcDZWSzRSM3VOV1dzNFA=")
                    //null
                };

                httpRequestMessage.Headers.Add("X-Access-Token", $"{XAuthToken}");
                httpRequestMessage.Headers.Add("Authorization", $"{Authorization}");

                response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                responseInfo = response1.GetAwaiter().GetResult();

                result1 = responseInfo.Content.ReadAsStringAsync().Result;

                result = JObject.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                if (!responseInfo.IsSuccessStatusCode)
                {
                    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        result1 = null;
                    }
                    else
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                }




                //string tokenMovidesk = movideskProvider.Token;



                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                //    $"cliente={"3569"}&" +
                //    $"nome={"moskit"}&" +
                //    $"senha={"2SI7WG"}");

                //request.Method = "POST";
                //request.Accept = "application/json";
                //request.CookieContainer = CookieContainer;

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //JObject jObj = JObject.Parse(json);

                //string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
                //string Authorization = jObj["Headers"]["Authorization"].ToString();

                //int indice = 0;

                //do
                //{
                url = $"buscar_cliente/688472668f481b3efbddb0bfbff99cf6?ultima_atualizacao={dataInicio}&indice={indice}&total=200";

                //    //url = $"buscar_cliente/688472668f481b3efbddb0bfbff99cf6?cpf_cliente=04216151000132&indice=0&total=200";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                httpWebRequest.Headers.Add("Authorization", Authorization);
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.CookieContainer = CookieContainer;
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();

                JObject detail = JObject.Parse(json2);

                //    JArray data = JArray.Parse(detail["data"].ToString());

                //    //string data2 = ToString(data);

                //    if (data.Count <= 0)
                //    {
                //        break;
                //    }

                //} while (true);
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("MonitorarClienteSgrParaMovidesk finalizado");

            Delay(60);
        }

        private void TestarBuscaClientRequest()
        {
            try
            {
                //    IEnumerable events = List<Event>(
                //        "Type=1 " +
                //        "AND Status='00'",
                //        "CreateDateTime ASC"
                //    );

                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");
                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");

                SgrProviderParam param = new SgrProviderParam();
                param.ClienteUltimaAtualizacao = DateTime.Now.AddDays(-10);

                //SgrProviderParam filter = new SgrProviderParam();
                //filter.ClienteUltimaAtualizacao = DateTime.Today.AddDays(-1);
                //IEnumerable agendamentos = sgrProvider.List<SgrAgendamento>(filter);

                List < SgrCliente> sgrClientes = sgrProvider.List<SgrCliente>(param).ToList();

                foreach (SgrCliente clientes in sgrClientes)
                {
                    Log(ToString(clientes));
                    //try
                    //{
                    //    //
                    //    //Alguns clientes estão com erro no retorno do campo data de criação então não da para consultar pelo objeto MovideskPerson
                    //    MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();
                    //    moviDeskProviderParam.Filter = $"businessName eq '{"C.HENRIQUE BODEMEIER & CIA LTDA"}'";

                    //    moviDeskProviderParam.Filter = $"cpfCnpj eq '{StringClear(clientes.CpfCliente)}'";

                    //    ProviderResult providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

                    //    if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                    //    {
                    //        //Log($"Contratante não cadastrado {contratanteCliente}");
                    //        throw new Exception(providerResult.Message);
                    //    }

                    //    MovideskPerson movideskPerson = (MovideskPerson)providerResult.Detail;
                    //    string cpf = null;

                    //    cpf = cpf.StartsWith("00000000000") ? null : cpf;
                        

                    //}
                    //catch (Exception e)
                    //{
                    //    Log(e);
                    //}        

                }


                //string contratante = UrlEncode("C.HENRIQUE BODEMEIER & CIA LTDA");

                //string filter = $"businessName eq '{contratante}'";

                //JObject parans = null;

                //HttpClient client = new HttpClient();
                //HttpRequestMessage
                //httpRequestMessage = new HttpRequestMessage
                //{
                //    Method = HttpMethod.Get,
                //    RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={movideskProvider.Token}&$filter={filter}"),
                //    Content = new StringContent("", null, "application/json")
                //    //null
                //};

                //var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                //var responseInfo = response1.GetAwaiter().GetResult();

                //string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                //if (!responseInfo.IsSuccessStatusCode)
                //{
                //    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //    {
                //        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                //    }
                //    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                //    {
                //        result1 = null;
                //    }
                //    else
                //    {
                //        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                //    }
                //}

            }
            catch (Exception e)
            {

                throw;
            }
        }









        private void TaskCriaGrupodeMensalidade()
        {
            //Log("TaskCriaGrupodeMensalidade iniciando...");

            //try
            //{
            //    ProviderBase moskitProvider = GetProvider<MoskitProvider>("204998e8-7ca5-44db-8270-10d0c5c3f819");

            //    CookieContainer CookieContainer = new CookieContainer();
            //    CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
            //        $"cliente={"3569"}&" +
            //        $"nome={"moskit"}&" +
            //        $"senha={"2SI7WG"}");

            //    request.Method = "POST";
            //    request.Accept = "application/json";
            //    request.CookieContainer = CookieContainer;

            //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            //    JObject jObj = JObject.Parse(json);

            //    string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
            //    string Authorization = jObj["Headers"]["Authorization"].ToString();

            //    request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/get_grupo_mensalidade/688472668f481b3efbddb0bfbff99cf6?");
            //    request.Method = "GET";
            //    request.Accept = "application/json";
            //    request.Headers.Add("X-Access-Token", XAuthToken);
            //    request.Headers.Add("Authorization", Authorization);
            //    request.KeepAlive = true;
            //    //request.ServicePoint.ConnectionLimit = 10000;
            //    //request.ContentType = "multipart/form-data";
            //    request.ContentType = "application/x-www-form-urlencoded";
            //    request.CookieContainer = CookieContainer;

            //    response = (HttpWebResponse)request.GetResponse();

            //    json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            //    string grupoMensalidadeSgr = json[0].ToString();

            //    JObject detail = JObject.Parse(json);
            //    JArray data = JArray.Parse(detail["data"].ToString());

            //    foreach (JObject mensalidade in data)
            //    {
            //        decimal valor = ToDecimal(mensalidade["valor"]);

            //        if (valor == 0)
            //        {
            //            continue;
            //        }

            //        string descMensalidade = ToString(mensalidade["descricao"]);

            //        int codMensalidade = ToInt(mensalidade["cod_grupo_mensalidade"]);

            //        string descOptions = $"({codMensalidade}) {descMensalidade}|{ToString(mensalidade["valor"])}";

            //        //
            //        //Buscar options
            //        //
            //        int registro = 0;

            //        bool found = false;

            //        do
            //        {
            //            request = (HttpWebRequest)WebRequest.Create($"https://api.moskitcrm.com/v2/customFields/CF_Pj3qYeidijrKrqQe/options?quantity=50&start={registro}");
            //            request.Method = "GET";
            //            request.Accept = "application/json";
            //            request.Headers.Add("Apikey", "8a3f08a1-d459-4201-9fea-cff64d7696ca");
            //            request.KeepAlive = true;

            //            response = (HttpWebResponse)request.GetResponse();

            //            json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            //            if (json.Equals("[]"))
            //            {
            //                break;
            //            }

            //            string grupoMensalidades = json.ToString();

            //            if (grupoMensalidades.Contains(descOptions))
            //            {

            //                found = true;
            //                break;
            //            }

            //            registro = registro + 50;

            //        } while (true);

            //        try
            //        {
            //            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //            request = (HttpWebRequest)WebRequest.Create($"https://api.moskitcrm.com/v2/customFields/CF_Pj3qYeidijrKrqQe/options");

            //            request.Method = "PUT";

            //            if (found)
            //            {
            //                request.Method = "POST";
            //            }

            //            request.ContentType = "application/json";
            //            request.KeepAlive = false;
            //            request.ServicePoint.ConnectionLimit = 10000;

            //            request.Headers.Add("apikey", "8a3f08a1-d459-4201-9fea-cff64d7696ca");

            //            JObject model = new JObject(
            //                           //new JProperty("id", 0),
            //                           new JProperty("label", descOptions));

            //            json = JsonConvert.SerializeObject(model);

            //            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            //            {
            //                streamWriter.Write(json);
            //            }

            //            using (response = (HttpWebResponse)request.GetResponse())
            //            {

            //                json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            //                //result.Message = json;
            //                //result.Detail = ToModel<T>(json);

            //                Thread.Sleep(5000);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            string mensagem = ToString(((System.Net.HttpWebResponse)((System.Net.WebException)ex).Response).Headers);

            //            Log(mensagem);

            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log(e);
            //}

            //Log("TaskCriaGrupodeMensalidade finalizado");
        }










        private void TestarHttpRequest()
        {
            Log("MonitorarClienteSgrParaMovidesk iniciado...");

            try
            {
                string dataInicio = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");

                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

                SgrProviderParam sgrProviderParam = new SgrProviderParam();
                sgrProviderParam.ClienteUltimaAtualizacao = DateTime.Now;

                List<SgrCliente> modelos = sgrProvider.List<SgrCliente>(sgrProviderParam).ToList();

                CookieContainer CookieContainer = new CookieContainer();
                CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                string url = $"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                    $"cliente={"3569"}&" +
                    $"nome={"moskit"}&" +
                    $"senha={"2SI7WG"}";

                //request.CookieContainer = CookieContainer;

                HttpClient client = new HttpClient();
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Content = new StringContent("", null, "application/json")
                    //null
                };

                var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                var responseInfo = response1.GetAwaiter().GetResult();

                string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                JObject result = JObject.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                if (!responseInfo.IsSuccessStatusCode)
                {
                    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        result1 = null;
                    }
                    else
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                }

                string XAuthToken = ToString(result["Headers"]["X-Auth-Token"]);
                string Authorization = ToString(result["Headers"]["Authorization"]);

                int indice = 0;


                //CookieContainer CookieContainer = new CookieContainer();
                //CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                //
                //Buscar cadastros de clientes
                //
                url = $"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/buscar_cliente/688472668f481b3efbddb0bfbff99cf6?ultima_atualizacao=2023-08-01 00:30:53&indice=0&total=200";


                //request.CookieContainer = CookieContainer;
                //httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                //httpWebRequest.Headers.Add("Authorization", Authorization);

                client = new HttpClient();
                httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Content = new StringContent("", null, "application/json"),
                    //Headers = new HttpRequestHeaders(Authorization, "Basic QVJNSVpFQ09NRVJDSU9fUFJEOnJDcDZWSzRSM3VOV1dzNFA=")
                    //null
                };

                httpRequestMessage.Headers.Add("X-Access-Token", $"{XAuthToken}");
                httpRequestMessage.Headers.Add("Authorization", $"{Authorization}");

                response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                responseInfo = response1.GetAwaiter().GetResult();

                result1 = responseInfo.Content.ReadAsStringAsync().Result;

                result = JObject.Parse(responseInfo.Content.ReadAsStringAsync().Result);

                if (!responseInfo.IsSuccessStatusCode)
                {
                    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        result1 = null;
                    }
                    else
                    {
                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    }
                }




                //string tokenMovidesk = movideskProvider.Token;



                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                //    $"cliente={"3569"}&" +
                //    $"nome={"moskit"}&" +
                //    $"senha={"2SI7WG"}");

                //request.Method = "POST";
                //request.Accept = "application/json";
                //request.CookieContainer = CookieContainer;

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //JObject jObj = JObject.Parse(json);

                //string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
                //string Authorization = jObj["Headers"]["Authorization"].ToString();

                //int indice = 0;

                //do
                //{
                url = $"buscar_cliente/688472668f481b3efbddb0bfbff99cf6?ultima_atualizacao={dataInicio}&indice={indice}&total=200";

                //    //url = $"buscar_cliente/688472668f481b3efbddb0bfbff99cf6?cpf_cliente=04216151000132&indice=0&total=200";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                httpWebRequest.Headers.Add("Authorization", Authorization);
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.CookieContainer = CookieContainer;
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();

                JObject detail = JObject.Parse(json2);

                //    JArray data = JArray.Parse(detail["data"].ToString());

                //    //string data2 = ToString(data);

                //    if (data.Count <= 0)
                //    {
                //        break;
                //    }

                //} while (true);
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("MonitorarClienteSgrParaMovidesk finalizado");

            Delay(60);
        }

        private void TestarBuscaClientRequest()
        {
            try
            {
                //    IEnumerable events = List<Event>(
                //        "Type=1 " +
                //        "AND Status='00'",
                //        "CreateDateTime ASC"
                //    );

                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");
                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");

                SgrProviderParam param = new SgrProviderParam();
                param.ClienteUltimaAtualizacao = DateTime.Now.AddDays(-3);

                //SgrProviderParam filter = new SgrProviderParam();
                //filter.ClienteUltimaAtualizacao = DateTime.Today.AddDays(-1);
                //IEnumerable agendamentos = sgrProvider.List<SgrAgendamento>(filter);

                //List <SgrCliente> sgrClientes = sgrProvider.List<SgrCliente>(param).ToList();
                IEnumerable<SgrCliente> sgrClientes = sgrProvider.List<SgrCliente>(param);

                foreach (SgrCliente clientes in sgrClientes)
                {
                    try
                    {
                        string businessName = StringEncode("C.HENRIQUE BODEMEIER & CIA LTDA");

                        //
                        //Alguns clientes estão com erro no retorno do campo data de criação então não da para consultar pelo objeto MovideskPerson
                        MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();
                        //moviDeskProviderParam.Filter = $"cpfCnpj eq '{StringClear(clientes.CpfCliente)}'";
                        moviDeskProviderParam.Filter = $"businessName eq '{businessName}'";

                        ProviderResult providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

                        if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                        {
                            //Log($"Contratante não cadastrado {contratanteCliente}");
                            throw new Exception(providerResult.Message);
                        }

                        MovideskPerson movideskPerson = (MovideskPerson)providerResult.Detail;

                    }
                    catch (Exception e)
                    {
                        Log(e);
                    }

                }


                //string contratante = UrlEncode("C.HENRIQUE BODEMEIER & CIA LTDA");

                //string filter = $"businessName eq '{contratante}'";

                //JObject parans = null;

                //HttpClient client = new HttpClient();
                //HttpRequestMessage
                //httpRequestMessage = new HttpRequestMessage
                //{
                //    Method = HttpMethod.Get,
                //    RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={movideskProvider.Token}&$filter={filter}"),
                //    Content = new StringContent("", null, "application/json")
                //    //null
                //};

                //var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                //var responseInfo = response1.GetAwaiter().GetResult();

                //string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                //if (!responseInfo.IsSuccessStatusCode)
                //{
                //    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //    {
                //        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                //    }
                //    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                //    {
                //        result1 = null;
                //    }
                //    else
                //    {
                //        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                //    }
                //}

            }
            catch (Exception e)
            {

                throw;
            }
        }









public void MonitorarClienteSgrParaMovidesk()
        {
            Log("MonitorarClienteSgrParaMovidesk iniciado...");

            try
            {
                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

                //string dataInicio = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");

                //if (DateTime.Now.Hour > 20)
                //{
                //    dataInicio = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss");
                //}
                DateTime dataInicio = DateTime.Today.AddDays(-1);

                ////string dataFim = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");

                //string tokenMovidesk = movideskProvider.Token;

                //CookieContainer CookieContainer = new CookieContainer();
                //CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
                //    $"cliente={"3569"}&" +
                //    $"nome={"moskit"}&" +
                //    $"senha={"2SI7WG"}");

                //request.Method = "POST";
                //request.Accept = "application/json";
                //request.CookieContainer = CookieContainer;

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //JObject jObj = JObject.Parse(json);

                //string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
                //string Authorization = jObj["Headers"]["Authorization"].ToString();

                //int indice = 0;

                //do
                //{
                //string url = $"buscar_cliente/688472668f481b3efbddb0bfbff99cf6?ultima_atualizacao={dataInicio}&indice={indice}&total=200";

                ////url = $"buscar_cliente/688472668f481b3efbddb0bfbff99cf6?cpf_cliente=87056135153&indice=0&total=200";

                //HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
                //httpWebRequest.Method = "GET";
                //httpWebRequest.Accept = "application/json";
                //httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                //httpWebRequest.Headers.Add("Authorization", Authorization);
                //httpWebRequest.KeepAlive = true;
                //httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                //httpWebRequest.CookieContainer = CookieContainer;
                //HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();

                //JObject detail = JObject.Parse(json2);

                //JArray data = JArray.Parse(detail["data"].ToString());

                ////string data2 = ToString(data);

                //if (data.Count <= 0)
                //{
                //    break;
                //}

                SgrProviderParam sgrProviderParam = new SgrProviderParam();
                sgrProviderParam.ClienteUltimaAtualizacao = dataInicio;

                IEnumerable clientes = sgrProvider.List<SgrCliente>(sgrProviderParam);

                //foreach (JObject cliente in data)
                foreach (SgrCliente cliente in clientes)
                {
                    //if (DateTime.Now.Hour == 4)
                    //{
                    //    break;
                    //}

                    // Caso o servico nao esteja rodando
                    if (!IsRunning)
                    {
                        break;
                    }

                    //try
                    //{
                    //string nome = ToString(cliente["nome_cliente"]);
                    string nome = cliente.NomeCliente;
                    SgrEmail[] emails = cliente.Emails;
                    SgrTelefone[] phones = cliente.Telefones;

                    //string cpfCnpj = StringClear(cliente["cpf_cliente"]);
                    string cpfCnpj = StringClear(cliente.CpfCliente);

                    //string situacao = ToString(cliente["situacao"]["descricao"]);
                    string situacao = cliente.Situacao.Descricao;

                    string cep = cliente.Endereco.Cep;
                    string logradouro = cliente.Endereco.Logradouro;
                    string numero = cliente.Endereco.Numero;
                    string complemento = cliente.Endereco.Complemento;
                    string bairro = cliente.Endereco.Bairro;
                    string cidade = cliente.Endereco.Cidade;
                    string uf = cliente.Endereco.Uf;

                    int tipoPessoa = StringClear(cpfCnpj).Length == 14 ? 2 : 1; // Pessoa Fisica

                    //if (nome.Equals("VERONI JASCZICZIN"))
                    //{
                    //    string aqui = null;
                    //}

                    //bool found = false;
                    //ProviderResult providerResult = null;

                    //string filter = $"cpfCnpj eq '{cpfCnpj}'";

                    ////Delay(10);

                    //JObject parans = null;

                    //HttpClient client = new HttpClient();
                    //HttpRequestMessage
                    //httpRequestMessage = new HttpRequestMessage
                    //{
                    //    Method = HttpMethod.Get,
                    //    RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={movideskProvider.Token}&$filter={filter2}"),
                    //    Content = new StringContent("", null, "application/json")
                    //    //null
                    //};

                    //var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                    //var responseInfo = response1.GetAwaiter().GetResult();

                    //string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                    //if (!responseInfo.IsSuccessStatusCode)
                    //{
                    //    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    //    {
                    //        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    //    }
                    //    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                    //    {
                    //        result1 = null;
                    //    }
                    //    else
                    //    {
                    //        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                    //    }
                    //}
                    MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();
                    moviDeskProviderParam.Filter = $"cpfCnpj eq '{cpfCnpj}'";

                    ProviderResult providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

                    if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                    {
                        throw new GeneralException(providerResult.Message);
                    }

                    //found = !result1.Equals("[]");

                    ////Log($"{cpfCnpj} {nome} {situacao} Cadastrado: {found}");

                    ////Não estou usando porque faltam campos
                    ////SgrProviderParam sgrProviderParam = new SgrProviderParam();
                    ////sgrProviderParam.ClienteUltimaAtualizacao = DateTime.Now;

                    ////List<SgrCliente> clientes = sgrProvider.List<SgrCliente>(sgrProviderParam).ToList();

                    ////if (!situacao.Equals("ATIVO"))
                    ////{
                    ////    string aq = "";
                    ////}

                    //MovideskPerson person = new MovideskPerson();
                    MovideskPerson person = (MovideskPerson)providerResult.Detail;

                    bool found = person != null;

                    if (!found)
                    {
                        //
                        // Se pessoa fisica é obrigatório colocar um relacionamento tipo empresa
                        //
                        string idEmpresa = null;

                        //string situacao = ToString(cliente["situacao"]["descricao"]);

                        if (tipoPessoa == 1)
                        {
                            person = new MovideskPerson();
                            person.BusinessName = nome;
                            person.PersonType = 2; // 1 - Fisica, 2 - Juridica,
                            person.ProfileType = 2;
                            person.IsActive = true;// !situacao.Equals("CANCELADO") ? true : false;

                            providerResult = movideskProvider.Post(person);

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                ////Delay(60);

                                //providerResult = movideskProvider.Post(person);

                                //if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                                //{
                                //    throw new Exception(providerResult.Message);
                                //}

                                throw new GeneralException(providerResult.Message);
                            }

                            person = (MovideskPerson)providerResult.Detail;

                            found = person != null;

                            if(!found)
                            {
                                throw new GeneralException("Falha ao criar o relacionamento quando é uma pessoa fisica");
                            }

                            idEmpresa = person.Id;

                            //Delay(30);
                        }

                        person = new MovideskPerson();
                        person.CpfCnpj = cpfCnpj;
                        //person.BusinessName = UrlEncode(nome);
                        person.BusinessName = nome;
                        person.PersonType = tipoPessoa; // 1 - Fisica, 2 - Juridica,
                        person.ProfileType = 2;
                        person.IsActive = !situacao.Equals("CANCELADO") ? true : false;

                        //object[] enderecos = new object[]{new JObject(
                        //                    new JProperty("addressType", "Comercial"),
                        //                    new JProperty("country", "BRASIL"),
                        //                    new JProperty("postalCode", cliente["endereco"]["cep"]),// cliente.EnderecoClienteCep),
                        //                    new JProperty("state", cliente["endereco"]["uf"]),//cliente.EnderecoClienteEstado),
                        //                    new JProperty("district", cliente["endereco"]["bairro"]),//cliente.EnderecoClienteBairro),
                        //                    new JProperty("city", cliente["endereco"]["cidade"]),//cliente.EnderecoClienteCidade),
                        //                    new JProperty("street", cliente["endereco"]["logradouro"]), //cliente.EnderecoClienteLogradouro),
                        //                    new JProperty("number", cliente["endereco"]["numero"]),//cliente.EnderecoClienteNumero),
                        //                    new JProperty("complement", cliente["endereco"]["complemento"]),
                        //                    //new JProperty("reference", ""),
                        //                    new JProperty("isDefault", true),
                        //                    new JProperty("countryId", 1058)
                        //                    //new JProperty("representative", new JObject(new JProperty("code", codRepresentante))),
                        //                    //new JProperty("customer", new JObject(new JProperty("code", NumberUtilities.parseInt(dadosCliente["code"]))))
                        //                    )};

                        //person.Addresses = enderecos;

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

                        //JArray emails2 = JArray.Parse(ToString(cliente["email"]));

                        //object[] moviDeskEmails = null;

                        ////foreach (JObject email in emails2)
                        //{
                        //    moviDeskEmails = new object[]{new JObject(
                        //        new JProperty("emailType", "Pessoal"),
                        //        new JProperty("email", email.Email),
                        //        new JProperty("isDefault", true)
                        //    )};
                        //}

                        //person.Emails = emails;
                        List<MovideskEmail> movideskEmails = new List<MovideskEmail>();
                        
                        foreach(SgrEmail email in emails)
                        {
                            MovideskEmail movideskEmail = new MovideskEmail();
                            movideskEmail.EmailType = "Pessoal";
                            movideskEmail.Email = email.Email;
                            //movideskEmail.IsDefault = true;

                            movideskEmails.Add(movideskEmail);
                        }

                        person.Emails = movideskEmails.ToArray();

                        //JArray phones = JArray.Parse(ToString(cliente["telefone"]));

                        //object[] contacts = null;

                        //foreach (JObject fone in phones)
                        //{
                        //    contacts = new object[]{new JObject(
                        //                            new JProperty("contactType", "Telefone celular"),
                        //                            new JProperty("contact", fone["contato"]),
                        //                            new JProperty("isDefault", true)
                        //                            )};
                        //}

                        //person.Contacts = contacts;

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
                            //Delay(20);

                            //providerResult = movideskProvider.Post(person);

                            //if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            //{
                            //    throw new Exception(providerResult.Message);
                            //}

                            throw new Exception(providerResult.Message);
                        }

                        // Log($"NOVO CADASTRO: {person.CpfCnpj} {nome}");
                    }
                    //else
                    //{
                    //    //string idCliente = Substring(result1, "id\":\"", "\"").Trim();

                    //    ////timeUsuario = Substring(result1, "\"teams\":[\"", "\"").Trim();

                    //    //person.BusinessName = nome;
                    //    //person.Id = idCliente;

                    //    //providerResult = movideskProvider.Put(person);

                    //    //if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                    //    //{
                    //    //    Delay(20);

                    //    //    providerResult = movideskProvider.Post(person);

                    //    //    if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                    //    //    {
                    //    //        throw new Exception(providerResult.Message);
                    //    //    }
                    //    //}
                    //}

                    //}
                    //catch (Exception e)
                    //{
                    //    Delay(60);

                    //    Log($"{StringClear(cliente["cpf_cliente"])} - {e.Message}");
                    //}

                    //indice++;

                    //Delay(60);
                }

                //} while (true);
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("MonitorarClienteSgrParaMovidesk finalizado");

            Delay(60);
        }




































        



        public class Mesalidade
        {
            public decimal Valor { get; set; }
            public string Descricao { get; set; }
            public int CodGrupoMensalidade { get; set; }
        }








//JToken customFieldValue = entityCustomFields.SelectToken("$.[?(@.id == 'CF_0WGqoGSKC9zK2qnP')]", true);
List<EntityCustomFields> entityCustomFields = new List<EntityCustomFields>(contact.EntityCustomFields);
EntityCustomFields customField = entityCustomFields.Find(e => e.Id == "CF_0WGqoGSKC9zK2qnP");



ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                        //
                        //Gravar o pedido 
                        //
                        //HttpWebRequest 
                        request = (HttpWebRequest)WebRequest.Create($"https://api.movidesk.com/public/v1/tickets?token=0a06763b-a892-4d3e-94b5-4e4ec942cb86");
                        request.Method = "POST";
                        request.ContentType = "application/json";
                        request.KeepAlive = false;
                        request.ServicePoint.ConnectionLimit = 10000;

                        JObject note = new JObject(
                                       new JProperty("type", "1"),
                                       new JProperty("subject", "teste"),
                                       new JProperty("createdBy", new JObject(new JProperty("id", "938628769"))),
                                       new JProperty("clients", new JArray(new JObject(new JProperty("id", "ca943566-44f2-4241-")))),
                                       new JProperty("actions", new JArray(new JObject(new JProperty("type", 1),
                                                                            new JProperty("description", "teste 3")))));

                        var call = new
                        {
                            //call = "LancarRecebimento",
                            //app_key = OMIE_APP_KEY,
                            //app_secret = OMIE_APP_SECRET,
                            param = new[] { note }
                        };

                        //string 
                            json = JsonConvert.SerializeObject(call, Formatting.Indented);

                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(json);
                        }

                        //HttpWebResponse 
                            response = (HttpWebResponse)request.GetResponse();

                        json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        JObject obj = JObject.Parse(json);

                        //ProviderResult result = new ProviderResult();


                           string parans = ToJson(new
                        {
                            type = 1,
                            subject = "teste",
                            createdDate = "2023-05-17T02:23:49.6430422",
                            createdBy = "{\"id\" = \"1375876216\"}",
                            clients = "[{\"id\": \"1315650525\"}]",
                            actions = "[\r\n        {\r\n            \"type\" : 1,\r\n            \"description\": \"Isso é um Teste via API312321 321 3123 1 3\\n\"\r\n        }\r\n    ]"

                        });

                        parans = "{\r\n  \"type\": \"1\",\r\n  \"subject\": \"\",\r\n  \"createdBy\": {\r\n        \"id\": \"938628769\"\r\n    },\r\n  \"clients\": [\r\n    {\r\n      \"id\": \"ca943566-44f2-4241-\"\r\n    }\r\n  ],\r\n  \"actions\": [\r\n    {\r\n      \"type\": 1,\r\n      \"description\": \"teste 2\"\r\n    }\r\n  ]\r\n}";



                        
        //  Sempre que ocorrer um agendamento no SGR deve-se enviá-lo ao MOVIDESK como TICKET
        //      - Solicitante = Cliente
        //      - Responsável = Pessoa que abriu o agendamento
        private void MonitorarAgendamentosAbertosSgrParaMovidesk1()
        {
            Log("MonitorarAgendamentoSgrParaMovidesk iniciado...");

            try
            {
                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

                //SgrProviderParam filter = new SgrProviderParam();
                //filter.ClienteUltimaAtualizacao = DateTime.Today.AddDays(-1);
                //IEnumerable agendamentos = sgrProvider.List<SgrAgendamento>(filter);

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

                JObject jObj = JObject.Parse(json);

                string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
                string Authorization = jObj["Headers"]["Authorization"].ToString();

                //JObject resultSgr = ((SgrProvider)sgrProvider).BuscaSgr($"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio=2023-05-01&data_cadastro_fim=2023-05-17",
                //                       XAuthToken,
                //                       Authorization,
                //                       CookieContainer);

                //string agendamentos2 = resultSgr[0].ToString();

                //JObject detail = JObject.Parse(json2);
                //JArray data = JArray.Parse(detail["data"].ToString());

                //foreach (JObject mensalidade in data)
                //{
                //}

                string url = "buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio=2023-05-01&data_cadastro_fim=2023-05-17";

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
                httpWebRequest.Headers.Add("Authorization", Authorization);
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.CookieContainer = CookieContainer;
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();

                //string agendamentos = json2[0].ToString();

                JObject detail = JObject.Parse(json2);
                JArray data = JArray.Parse(detail["data"].ToString());

                foreach (JObject mensalidade in data)
                {

                    //Gravar ticket no movedesk

                    MovideskTicket ticket = new MovideskTicket();

                    MovideskClient movideskClient = new MovideskClient();
                    movideskClient.Id = 1315650525;

                    ticket.Clients = new MovideskClient[] { movideskClient };

                    //MovideskAction movideskAction = new MovideskAction();
                    //movideskAction.Description = "Teste";

                    //ticket.Actions = new Action[] { movideskAction };

                    ////acão
                    //Action[] actions = new Action[]{new JObject(
                    //                    new JProperty("Description", "Teste")
                    //                    )};

                    //ticket.Actions = actions;

                    ticket.Type = 1;
                    ticket.Subject = $"{mensalidade["contratante"]} - SERVIÇO: {mensalidade["servico"]["descricao"]}";


                    ////Contatos
                    //object[] clientes = new MovideskClient[]{new JObject(
                    //                    new JProperty("Id", 1315650525)
                    //                    )};

                    //ticket.Clients = clientes;

                    MovideskAction movideskActions = new MovideskAction();
                    movideskActions.Description = "teste";

                    ProviderResult providerResult = movideskProvider.Post(ticket);

                    if (providerResult.Status == 500)
                    {
                        throw new Exception(providerResult.Message);
                    }


                    providerResult = movideskProvider.Post(ticket);

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

            Log("MonitorarAgendamentoSgrParaMovidesk finalizado");
        }


        //
        //Backup
        //

        08/08/2023

         //SgrProviderParam filter = new SgrProviderParam();
                //filter.ClienteUltimaAtualizacao = DateTime.Now.AddDays(-3).AddHours(3).AddMinutes(20);

                //string dataAtualizacao = DateTime.Now.AddDays(-3).AddHours(3).AddMinutes(20).ToString("dd-MM-yyyy HH:mm:ss");

                //IEnumerable clientes = sgrProvider.List<SgrCliente>(filter);

                //foreach (SgrCliente cliente in clientes)
                //{
                //    try
                //    {
                //        string enderecoCep = cliente.EnderecoClienteCep;

                //        string nome = cliente.NomeCliente;
                //        string cpfCnpj = StringClear(cliente.CpfCliente);

                //        Log($"{cpfCnpj} {nome}");

                //        int tipoPessoa = StringClear(cpfCnpj).Length == 14 ? 2 : 1; // Pessoa Fisica

                //        if (!nome.Equals("ROSANGELA BRUNO"))
                //        {
                //            continue;
                //        }

                //        //// https://atendimento.movidesk.com/kb/article/189/
                //        ////Buscar antes
                //        //MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();
                //        //moviDeskProviderParam.Filter = $"cpfCnpj eq '{cpfCnpj}'";

                //        //ProviderResult providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

                //        //if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                //        //{
                //        //    throw new Exception(providerResult.Message);
                //        //}

                //        //MovideskPerson movideskPerson = (MovideskPerson)providerResult.Detail;

                //        //bool found = movideskPerson != null;

                //        bool found = false;
                //        ProviderResult providerResult = null;

                //        string filter2 = $"cpfCnpj eq '{cpfCnpj}'";

                //        //Delay(10);

                //        JObject parans = null;

                //        HttpClient client = new HttpClient();
                //        HttpRequestMessage
                //        httpRequestMessage = new HttpRequestMessage
                //        {
                //            Method = HttpMethod.Get,
                //            RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={movideskProvider.Token}&$filter={filter2}"),
                //            Content = new StringContent("", null, "application/json")
                //            //null
                //        };

                //        var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                //        var responseInfo = response1.GetAwaiter().GetResult();

                //        string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                //        if (!responseInfo.IsSuccessStatusCode)
                //        {
                //            if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //            {
                //                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                //            }
                //            else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                //            {
                //                result1 = null;
                //            }
                //            else
                //            {
                //                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                //            }
                //        }

                //        found = !result1.Equals("[]");

                //        if (!found)
                //        {
                //            MovideskPerson person = new MovideskPerson();

                //            //
                //            //Se pessoa fisica é obrigatório colocar um relacionamento tipo empresa
                //            //
                //            string idEmpresa = null;
                //            if (tipoPessoa == 1)
                //            {
                //                person.BusinessName = nome;
                //                person.PersonType = 2; // 1 - Fisica, 2 - Juridica,
                //                person.ProfileType = 2;
                //                person.IsActive = true;

                //                providerResult = movideskProvider.Post(person);

                //                if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                //                {
                //                    throw new Exception(providerResult.Message);
                //                }

                //                person = (MovideskPerson)providerResult.Detail;

                //                idEmpresa = person.Id;
                //            }

                //            person = new MovideskPerson();

                //            person.CpfCnpj = cpfCnpj;
                //            person.BusinessName = nome;
                //            person.PersonType = tipoPessoa; // 1 - Fisica, 2 - Juridica,
                //            person.ProfileType = 2;
                //            person.IsActive = true;

                //            object[] enderecos = new object[]{new JObject(
                //                            new JProperty("addressType", "Comercial"),
                //                            new JProperty("country", "BRASIL"),
                //                            new JProperty("postalCode", cliente.EnderecoClienteCep),
                //                            new JProperty("state", cliente.EnderecoClienteEstado),
                //                            new JProperty("district", cliente.EnderecoClienteBairro),
                //                            new JProperty("city", cliente.EnderecoClienteCidade),
                //                            new JProperty("street",cliente.EnderecoClienteLogradouro),
                //                            new JProperty("number", cliente.EnderecoClienteNumero),
                //                            //new JProperty("complement",""),
                //                            //new JProperty("reference", ""),
                //                            new JProperty("isDefault", true),
                //                            new JProperty("countryId", 1058)
                //                            //new JProperty("representative", new JObject(new JProperty("code", codRepresentante))),
                //                            //new JProperty("customer", new JObject(new JProperty("code", NumberUtilities.parseInt(dadosCliente["code"]))))
                //                            )};

                //            person.Addresses = enderecos;

                //            MovideskPersonRelationship relationships = new MovideskPersonRelationship();

                //            relationships.Id = idEmpresa;

                //            person.Relationships = new MovideskPersonRelationship[] { relationships };

                //            ////Contatos
                //            //object[] contatos = new object[]{new JObject(
                //            //                    new JProperty("contactType", "Telefone celular"),
                //            //                    new JProperty("contact", cliente.),
                //            //                    new JProperty("isDefault", true)
                //            //                    )};

                //            //person.Contacts = contatos;

                //            ////Emails
                //            //object[] emails = new object[]{new JObject(
                //            //                    new JProperty("contactType", "Telefone celular"),
                //            //                    new JProperty("email", cliente.),
                //            //                    new JProperty("isDefault", true)
                //            //                    )};

                //            //person.Emails = emails;


                //            providerResult = movideskProvider.Post(person);

                //            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                //            {
                //                throw new Exception(providerResult.Message);
                //            }
                //            Log($"NOVO CADASTRO: {person.CpfCnpj} {nome}");
                //        }

                //    }
                //    catch (Exception e)
                //    {
                //        Log($"{cliente.CpfCliente} - {e.Message}");
                //    }

                //    Delay(5);
                //}


                
        private void MonitorarClienteSgrParaMovidesk1()
        {
            Log("MonitorarClienteSgrParaMovidesk iniciado...");

            try
            {
                ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
                ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784"); //bd4a8514-9ab3-4536-b875-4ab4086509d8

                SgrProviderParam filter = new SgrProviderParam();
                filter.ClienteUltimaAtualizacao = DateTime.Now.AddDays(-3);

                //string dataAtualizacao = DateTime.Now.AddDays(-10).ToString("dd-MM-yyyy HH:mm:ss");

                IEnumerable clientes = sgrProvider.List<SgrCliente>(filter);

                foreach (SgrCliente cliente in clientes)
                {
                    try
                    {
                        string enderecoCep = cliente.EnderecoClienteCep;

                        string nome = cliente.NomeCliente;
                        string cpfCnpj = StringClear(cliente.CpfCliente);

                        Log($"{cpfCnpj} {nome}");

                        int tipoPessoa = StringClear(cpfCnpj).Length == 14 ? 2 : 1; // Pessoa Fisica

                        if (!nome.Equals("ROSANGELA BRUNO"))
                        {
                            continue;
                        }

                        //// https://atendimento.movidesk.com/kb/article/189/
                        ////Buscar antes
                        //MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();
                        //moviDeskProviderParam.Filter = $"cpfCnpj eq '{cpfCnpj}'";

                        //ProviderResult providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

                        //if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                        //{
                        //    throw new Exception(providerResult.Message);
                        //}

                        //MovideskPerson movideskPerson = (MovideskPerson)providerResult.Detail;

                        //bool found = movideskPerson != null;

                        bool found = false;
                        ProviderResult providerResult = null;

                        string filter2 = $"cpfCnpj eq '{cpfCnpj}'";

                        //Delay(10);

                        JObject parans = null;

                        HttpClient client = new HttpClient();
                        HttpRequestMessage
                        httpRequestMessage = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={movideskProvider.Token}&$filter={filter2}"),
                            Content = new StringContent("", null, "application/json")
                            //null
                        };

                        var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                        var responseInfo = response1.GetAwaiter().GetResult();

                        string result1 = responseInfo.Content.ReadAsStringAsync().Result;

                        if (!responseInfo.IsSuccessStatusCode)
                        {
                            if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                            }
                            else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                result1 = null;
                            }
                            else
                            {
                                throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
                            }
                        }

                        found = !result1.Equals("[]");

                        if (!found)
                        {
                            MovideskPerson person = new MovideskPerson();

                            //
                            //Se pessoa fisica é obrigatório colocar um relacionamento tipo empresa
                            //
                            string idEmpresa = null;
                            if (tipoPessoa == 1)
                            {
                                person.BusinessName = nome;
                                person.PersonType = 2; // 1 - Fisica, 2 - Juridica,
                                person.ProfileType = 2;
                                person.IsActive = true;

                                providerResult = movideskProvider.Post(person);

                                if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                                {
                                    throw new Exception(providerResult.Message);
                                }

                                person = (MovideskPerson)providerResult.Detail;

                                idEmpresa = person.Id;
                            }

                            person = new MovideskPerson();

                            person.CpfCnpj = cpfCnpj;
                            person.BusinessName = nome;
                            person.PersonType = tipoPessoa; // 1 - Fisica, 2 - Juridica,
                            person.ProfileType = 2;
                            person.IsActive = true;

                            object[] enderecos = new object[]{new JObject(
                                            new JProperty("addressType", "Comercial"),
                                            new JProperty("country", "BRASIL"),
                                            new JProperty("postalCode", cliente.EnderecoClienteCep),
                                            new JProperty("state", cliente.EnderecoClienteEstado),
                                            new JProperty("district", cliente.EnderecoClienteBairro),
                                            new JProperty("city", cliente.EnderecoClienteCidade),
                                            new JProperty("street",cliente.EnderecoClienteLogradouro),
                                            new JProperty("number", cliente.EnderecoClienteNumero),
                                            //new JProperty("complement",""),
                                            //new JProperty("reference", ""),
                                            new JProperty("isDefault", true),
                                            new JProperty("countryId", 1058)
                                            //new JProperty("representative", new JObject(new JProperty("code", codRepresentante))),
                                            //new JProperty("customer", new JObject(new JProperty("code", NumberUtilities.parseInt(dadosCliente["code"]))))
                                            )};

                            person.Addresses = enderecos;

                            MovideskPersonRelationship relationships = new MovideskPersonRelationship();

                            relationships.Id = idEmpresa;

                            person.Relationships = new MovideskPersonRelationship[] { relationships };

                            ////Contatos
                            //object[] contatos = new object[]{new JObject(
                            //                    new JProperty("contactType", "Telefone celular"),
                            //                    new JProperty("contact", cliente.),
                            //                    new JProperty("isDefault", true)
                            //                    )};

                            //person.Contacts = contatos;

                            ////Emails
                            //object[] emails = new object[]{new JObject(
                            //                    new JProperty("contactType", "Telefone celular"),
                            //                    new JProperty("email", cliente.),
                            //                    new JProperty("isDefault", true)
                            //                    )};

                            //person.Emails = emails;


                            providerResult = movideskProvider.Post(person);

                            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
                            {
                                throw new Exception(providerResult.Message);
                            }
                            Log($"NOVO CADASTRO: {person.CpfCnpj} {nome}");
                        }

                    }
                    catch (Exception e)
                    {
                        Log($"{cliente.CpfCliente} - {e.Message}");
                    }

                    Delay(5);
                }
            }
            catch (Exception e)
            {
                Log(e);
            }

            Log("MonitorarClienteSgrParaMovidesk finalizado");
        }


          //  Sempre que ocorrer um agendamento no SGR deve-se enviá-lo ao MOVIDESK como TICKET
        //      - Solicitante = Cliente
        //      - Responsável = Pessoa que abriu o agendamento
        //Migrar versão 6.0.0.5
        public void MonitorarAgendamentosAbertosSgrParaMovidesk()
        {
            //Log("MonitorarAgendamentoSgrParaMovidesk iniciado...");

            //try
            //{
            //    string dataInicio = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

            //    string dataFim = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

            //    ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
            //    ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

            //    string tokenMovidesk = movideskProvider.Token;

            //    CookieContainer CookieContainer = new CookieContainer();
            //    CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
            //        $"cliente={"3569"}&" +
            //        $"nome={"moskit"}&" +
            //        $"senha={"2SI7WG"}");

            //    request.Method = "POST";
            //    request.Accept = "application/json";
            //    request.CookieContainer = CookieContainer;

            //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            //    JObject jObj = JObject.Parse(json);

            //    string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
            //    string Authorization = jObj["Headers"]["Authorization"].ToString();

            //    string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}";

            //    //string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}&data_cadastro_fim={dataFim}";
            //    //long numeroAgendamento = 38895;
            //    //url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?cod_agendamento={numeroAgendamento}";

            //    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
            //    httpWebRequest.Method = "GET";
            //    httpWebRequest.Accept = "application/json";
            //    httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
            //    httpWebRequest.Headers.Add("Authorization", Authorization);
            //    httpWebRequest.KeepAlive = true;
            //    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            //    httpWebRequest.CookieContainer = CookieContainer;
            //    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //    string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();

            //    JObject detail = JObject.Parse(json2);
            //    JArray data = JArray.Parse(detail["data"].ToString());

            //    foreach (JObject agendamento in data)
            //    {
            //        string contratanteCliente = ToString(agendamento["contratante"]);

            //        string codAgendamento = ToString(agendamento["cod_agendamento"]);

            //        string subject = $"{ToString(agendamento["cod_agendamento"])} - {contratanteCliente}";

            //        try
            //        {
            //            ProviderResult result = new ProviderResult();

            //            string situacao = ToString(agendamento["situacao"]["descricao"]);

            //            DateTime dataInicial = ToDate($"{ToString(agendamento["data_inicial"])} {ToString(agendamento["horario_inicial"])}");
            //            DateTime dataFinal = ToDate($"{ToString(agendamento["data_final"])} {ToString(agendamento["horario_final"])}");

            //            string dataCriacao = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");

            //            if (dataInicial < DateTime.Now.AddHours(3))
            //            {
            //                dataCriacao = dataInicial.ToString("yyyy-MM-dd HH:mm:ss");
            //            }

            //            string status = "Novo";
            //            string baseStatus = "New";
            //            string justificativa = null;
            //            string dataResolvido = null;

            //            if (situacao.ToLower().Equals("cancelado"))
            //            {
            //                status = "Cancelado";
            //                baseStatus = "Canceled";
            //            }
            //            else if (situacao.ToLower().Equals("parado"))
            //            {
            //                status = "Aguardando";
            //                baseStatus = "Stopped";
            //                justificativa = "Retorno do cliente";
            //            }
            //            else if (situacao.ToLower().Equals("concluído"))
            //            {
            //                status = "Resolvido";
            //                baseStatus = "Resolved";

            //                dataResolvido = ToDate(ToString(agendamento["data_conclusao"])).ToString("yyyy-MM-dd HH:mm:ss"); //DateTime.Now.ToString();

            //                Log($"Data de conclusao: {ToString(agendamento["data_conclusao"])} - {dataResolvido}");

            //                if (ToDate(ToString(agendamento["data_conclusao"])) < ToDate(dataCriacao))
            //                {
            //                    dataResolvido = dataCriacao;// DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");

            //                    dataCriacao = dataResolvido;
            //                }

            //            }

            //            string observacao = ToString(agendamento["observacao"]);

            //            string descricao = ToString(agendamento["descricao"]);

            //            string usuario = ToString(agendamento["usuario_original"]["nome"]);

            //            string pontoVenda = ToString(agendamento["ponto"]["nome"]);

            //            string descServico = ToString(agendamento["servico"]["descricao"]);

            //            string servico = descServico;

            //            //switch (descServico.ToUpper())
            //            //{
            //            //    //case "INSTALACAO": servico = "Instalação"; break;
            //            //    case "MANUTENÇÃO": servico = "MANUTENÇÃO"; break;
            //            //}

            //            string dadosVeiculo = $"Veículo: " +
            //                $"{ToString(agendamento["modelo"])} -" +
            //                $"{ToString(agendamento["placa"])} - " +
            //                $"{ToString(agendamento["marca"])} - " +
            //                $"{ToString(agendamento["anofab"])} -" +
            //                $"{ToString(agendamento["anomod"])} -";

            //            string localInstalacao = ToString(agendamento["local_instalacao_modulo"]);

            //            var res = "";

            //            bool found = false;
            //            ProviderResult providerResult = null;

            //            string filter = $"businessName eq '{contratanteCliente}'";

            //            Delay(10);

            //            JObject parans = null;

            //            HttpClient client = new HttpClient();
            //            HttpRequestMessage
            //            httpRequestMessage = new HttpRequestMessage
            //            {
            //                Method = HttpMethod.Get,
            //                RequestUri = new Uri($"https://api.movidesk.com/public/v1/persons?token={tokenMovidesk}&$filter={filter}"),
            //                Content = new StringContent("", null, "application/json")
            //                //null
            //            };

            //            var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

            //            var responseInfo = response1.GetAwaiter().GetResult();

            //            string result1 = responseInfo.Content.ReadAsStringAsync().Result;

            //            if (!responseInfo.IsSuccessStatusCode)
            //            {
            //                if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
            //                {
            //                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
            //                }
            //                else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
            //                {
            //                    result1 = null;
            //                }
            //                else
            //                {
            //                    throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
            //                }
            //            }

            //            found = !result1.Equals("[]");

            //            //
            //            //Alguns clientes estão com erro no retorno do campo data de criação
            //            //
            //            MoviDeskProviderParam moviDeskProviderParam = new MoviDeskProviderParam();

            //            moviDeskProviderParam.Filter = $"businessName eq '{contratanteCliente}'";

            //            //providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

            //            //if (providerResult.Status == 500)
            //            //{
            //            //    Log($"Contratante não cadastrado {contratanteCliente}");
            //            //    throw new Exception(providerResult.Message);
            //            //}

            //            //MovideskPerson movideskPerson = (MovideskPerson)providerResult.Detail;

            //            //found = movideskPerson != null;

            //            if (!found)
            //            {
            //                Log($"Cliente não cadastrado {contratanteCliente}");
            //            }

            //            //
            //            //Se encontrar o cliente cadastrado
            //            //
            //            if (found)
            //            {
            //                string idCliente = Substring(result1, "id\":\"", "\"").Trim();

            //                //
            //                //Buscar agente 
            //                //
            //                string idUsuario = "df562909-3e5d-4406-"; //df562909-3e5d-4406-

            //                string timeUsuario = "Administrativo Suporte";

            //                if (pontoVenda.ToUpper().Equals("MARINGA"))
            //                {
            //                    idUsuario = "1844aa69-53d0-4cbf-";

            //                    timeUsuario = "Atendimento N1 Maringá";
            //                }

            //                moviDeskProviderParam = new MoviDeskProviderParam();
            //                moviDeskProviderParam.Filter = $"businessName eq '{usuario}'";

            //                providerResult = movideskProvider.Get<MovideskPerson>(moviDeskProviderParam);

            //                if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
            //                {
            //                    throw new Exception(providerResult.Message);
            //                }

            //                MovideskPerson movideskUser = (MovideskPerson)providerResult.Detail;

            //                found = movideskUser != null;

            //                if (found)
            //                {
            //                    if ((movideskUser.ProfileType == 1 || movideskUser.ProfileType == 3) && movideskUser.Teams.Length > 0)
            //                    {
            //                        idUsuario = movideskUser.Id;

            //                        timeUsuario = movideskUser.Teams[0].ToString();
            //                    }
            //                }

            //                //
            //                //Consultar o serviço para pegar o id
            //                //
            //                string idServico = null;

            //                providerResult = GetHttpClient(
            //                  $"https://api.movidesk.com/public/v1/services?token=78113255-a9e4-4824-b651-95f664927d9f&$filter=name eq '{servico}'&$select=name,id",
            //                  null,
            //                  HttpMethod.Get
            //                );

            //                if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
            //                {
            //                    throw new Exception(providerResult.Message);
            //                }

            //                res = providerResult.Message.ToString().Replace("[", "").Replace("]", "").Replace("},", "};");

            //                if (!IsNullOrEmpty(res))
            //                {
            //                    string[] serv = res.Split(";");

            //                    foreach (string ret in serv)
            //                    {
            //                        JObject servicos = JObject.Parse(ret);
            //                        idServico = ToString(servicos["id"]);

            //                        break;
            //                    }
            //                }

            //                //
            //                //Buscar o ticket se já foi cadastrado
            //                //
            //                filter = $"subject eq '{subject}'";

            //                string campos = "id,subject,createdDate";

            //                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //                bool foundTicket = false;
            //                JObject agendament = null;

            //                providerResult = GetHttpClient(
            //                  $"https://api.movidesk.com/public/v1/tickets?token={tokenMovidesk}&$filter={filter}&$select={campos}",
            //                  null,
            //                  HttpMethod.Get
            //                );

            //                if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
            //                {
            //                    throw new Exception(providerResult.Message);
            //                }

            //                res = providerResult.Message.ToString().Replace("[", "").Replace("]", "").Replace("},", "};");

            //                if (!IsNullOrEmpty(res))
            //                {
            //                    string[] agend = res.Split(";");

            //                    foreach (string ret in agend)
            //                    {
            //                        agendament = JObject.Parse(ret);
            //                        foundTicket = true;
            //                    }
            //                }

            //                if (!foundTicket)
            //                {
            //                    parans = new JObject(
            //                               new JProperty("type", "2"),
            //                               new JProperty("status", status),
            //                               new JProperty("createdDate", dataCriacao), //VERIFICAR A DATA DE CONCLUSÃO
            //                               new JProperty("justification", justificativa),
            //                               new JProperty("baseStatus", baseStatus),
            //                               new JProperty("resolvedIn", dataResolvido),
            //                               //new JProperty("closedIn", dataConcluido)
            //                               new JProperty("subject", subject),
            //                               new JProperty("category", "Solicitação de serviço"),
            //                               new JProperty("serviceFirstLevelId", idServico), // 792031
            //                               new JProperty("urgency", "Média"),
            //                               new JProperty("ownerTeam", timeUsuario),
            //                               new JProperty("createdBy", new JObject(new JProperty("id", idUsuario))), //"938628769"

            //                               new JProperty("clients", new JArray(new JObject(new JProperty("id", idCliente)))), //"ca943566-44f2-4241-"
            //                               new JProperty("actions", new JArray(new JObject(new JProperty("type", 1),
            //                                                                   new JProperty("description", $"{descricao} \n\n {observacao} \n\n{dadosVeiculo} \n\nPonto de Venda: {pontoVenda} \n\n Data Agendamento Inicio: {dataInicial} \\n\\n Data Agendamento Final: {dataFinal}"))
            //                                                                   )),
            //                               new JProperty("owner", new JObject(new JProperty("id", idUsuario)
            //                                                     )));

            //                    //HttpClient
            //                    client = new HttpClient();
            //                    //HttpRequestMessage
            //                    httpRequestMessage = new HttpRequestMessage
            //                    {
            //                        Method = HttpMethod.Post,
            //                        RequestUri = new Uri($"https://api.movidesk.com/public/v1/tickets?token={tokenMovidesk}"),
            //                        Content = new StringContent(parans.ToString(), null, "application/json")
            //                    };
            //                }

            //                //var 
            //                response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

            //                //var
            //                responseInfo = response1.GetAwaiter().GetResult();

            //                //string
            //                result1 = responseInfo.Content.ReadAsStringAsync().Result;

            //                if (!responseInfo.IsSuccessStatusCode)
            //                {
            //                    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
            //                    {
            //                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
            //                    }
            //                    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
            //                    {
            //                        result1 = null;
            //                    }
            //                    else
            //                    {
            //                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
            //                    }
            //                }

            //                Log($"{subject}");

            //            }

            //        }
            //        catch (Exception e)
            //        {
            //            Log($"Falha no envio: {subject}");

            //            Log(e);
            //        }
            //    }

            //}
            //catch (Exception e)
            //{
            //    Log(e);
            //}

            //Log("MonitorarAgendamentoSgrParaMovidesk finalizado");

        }

        //
        //Monitorar todos os agendamento no SGR e ir atualizando o status dos tickets no MOVIDESK 
        //Migrar versão 6.0.0.5
        private void MonitorarAgendamentosFechadosSgrParaMovidesk()
        {
            //Log("MonitorarAgendamentosFechadosSgrParaMovidesk iniciado...");

            //try
            //{
            //    string dataInicio = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

            //    string dataFim = DateTime.Today.ToString("yyyy-MM-dd");

            //    ProviderBase sgrProvider = GetProvider<SgrProvider>("334998e8-7ca5-44db-8270-10d0c5c3f835");
            //    ProviderBase movideskProvider = GetProvider<MoviDeskProvider>("304998e8-7ca5-44db-8270-10d0c5c3f784");

            //    string tokenMovidesk = movideskProvider.Token;

            //    CookieContainer CookieContainer = new CookieContainer();
            //    CookieContainer.GetCookies(new Uri("http://sgr.hinova.com.br"));

            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/headers_authorization?" +
            //        $"cliente={"3569"}&" +
            //        $"nome={"moskit"}&" +
            //        $"senha={"2SI7WG"}");

            //    request.Method = "POST";
            //    request.Accept = "application/json";
            //    request.CookieContainer = CookieContainer;

            //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            //    JObject jObj = JObject.Parse(json);

            //    string XAuthToken = jObj["Headers"]["X-Auth-Token"].ToString();
            //    string Authorization = jObj["Headers"]["Authorization"].ToString();

            //    //string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_cadastro_inicio={dataInicio}";

            //    string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_conclusao_inicio={dataInicio}";


            //    //string url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?data_inicial_inicio={dataInicio}";

            //    //string 
            //    //url = $"buscar_agendamento/688472668f481b3efbddb0bfbff99cf6?cod_agendamento=37793";

            //    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/servicos/" + url);
            //    httpWebRequest.Method = "GET";
            //    httpWebRequest.Accept = "application/json";
            //    httpWebRequest.Headers.Add("X-Access-Token", XAuthToken);
            //    httpWebRequest.Headers.Add("Authorization", Authorization);
            //    httpWebRequest.KeepAlive = true;
            //    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            //    httpWebRequest.CookieContainer = CookieContainer;
            //    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //    string json2 = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();

            //    JObject detail = JObject.Parse(json2);
            //    JArray data = JArray.Parse(detail["data"].ToString());

            //    foreach (JObject agendamento in data)
            //    {
            //        try
            //        {
            //            string situacao = ToString(agendamento["situacao"]["descricao"]);

            //            string contratanteCliente = ToString(agendamento["contratante"]);

            //            string codAgendamento = ToString(agendamento["cod_agendamento"]);

            //            //string subject = $"{codAgendamento} - {contratanteCliente}(TESTE VSI)";

            //            string subject = $"{ToString(agendamento["cod_agendamento"])} - {contratanteCliente}";

            //            //
            //            //Situação do ticket
            //            //
            //            //New, InAttendance,Stopped,Canceled,Resolved,Closed
            //            string status = null;
            //            string justificativa = null;
            //            string baseStatus = "New";
            //            string dataResolvido = null;
            //            string dataConcluido = null;

            //            if (situacao.ToLower().Equals("aberto"))
            //            {
            //                continue;
            //            }

            //            //switch (situacao.ToLower())
            //            //{
            //            //    case "cancelado": status = "Cancelado", baseStatus = "Canceled"; break;
            //            //}
            //            if (situacao.ToLower().Equals("cancelado"))
            //            {
            //                status = "Cancelado";
            //                baseStatus = "Canceled";
            //            }
            //            else if (situacao.ToLower().Equals("parado"))
            //            {
            //                status = "Aguardando";
            //                baseStatus = "Stopped";
            //                justificativa = "Retorno do cliente";
            //            }
            //            else if (situacao.ToLower().Equals("concluído"))
            //            {
            //                //status = "Fechado";
            //                //baseStatus = "Closed";
            //                ////justificativa = "Retorno do cliente";
            //                //dataConcluido = ToDate(ToString(agendamento["data_conclusao"]) + " 00:00:00").ToString(); //DateTime.Now.ToString();

            //                status = "Resolvido";
            //                baseStatus = "Resolved";
            //                //justificativa = "Retorno do cliente";
            //                //dataConcluido = ToDate(ToString(agendamento["data_conclusao"]) + " 00:00:00").ToString(); //DateTime.Now.ToString();

            //                dataResolvido = ToDate(ToString(agendamento["data_conclusao"])).ToString("yyyy-MM-dd HH:mm:ss"); //DateTime.Now.ToString();

            //            }
            //            else
            //            {
            //                //Resolvido - Resolved
            //                //Em atendimento - InAttendance
            //                //
            //                //No SGR - Aguardando agendamento, Agendado

            //                Log($"{subject} -  {situacao}");
            //                continue;
            //            }

            //            //
            //            //Buscar o ticket se já foi cadastrado
            //            //
            //            string filter = $"subject eq '{subject}'";

            //            string campos = "id,subject,createdDate,status,baseStatus,justification,origin,createdDate,resolvedIn,closedIn";

            //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //            bool foundTicket = false;
            //            JObject movideskTicket = null;

            //            ProviderResult providerResult = GetHttpClient(
            //              $"https://api.movidesk.com/public/v1/tickets?token={tokenMovidesk}&$filter={filter}&$select={campos}",
            //              null,
            //              HttpMethod.Get
            //            );

            //            if (providerResult.Status == ProviderResultEnum.INTERNAL_SERVER_ERROR)
            //            {
            //                throw new Exception(providerResult.Message);
            //            }

            //            string res = providerResult.Message.ToString().Replace("[", "").Replace("]", "").Replace("},", "};");

            //            if (!IsNullOrEmpty(res))
            //            {
            //                string[] agend = res.Split(";");

            //                foreach (string ret in agend)
            //                {
            //                    movideskTicket = JObject.Parse(ret);

            //                    string statusMovidesk = ToString(movideskTicket["baseStatus"]);

            //                    if (!statusMovidesk.ToLower().Equals("resolved") && !statusMovidesk.ToLower().Equals("closed") && !situacao.ToLower().Equals("aberto"))
            //                    {
            //                        foundTicket = true;
            //                    }

            //                    if (statusMovidesk.ToLower().Equals("canceled") && situacao.ToLower().Equals("cancelado"))
            //                    {
            //                        foundTicket = false;
            //                    }
            //                }
            //            }

            //            JObject parans = null;
            //            HttpClient client = null;
            //            HttpRequestMessage httpRequestMessage = null;

            //            if (foundTicket)
            //            {
            //                parans = new JObject(
            //                          new JProperty("status", status),
            //                          new JProperty("justification", justificativa),
            //                          new JProperty("baseStatus", baseStatus),
            //                          new JProperty("resolvedIn", dataResolvido),
            //                          new JProperty("closedIn", dataConcluido)

            //                          );

            //                client = new HttpClient();
            //                httpRequestMessage = new HttpRequestMessage
            //                {
            //                    Method = HttpMethod.Patch,
            //                    RequestUri = new Uri($"https://api.movidesk.com/public/v1/tickets?id={ToString(movideskTicket["id"])}&token={tokenMovidesk}"),
            //                    Content = new StringContent(parans.ToString(), null, "application/json")
            //                };

            //                var response1 = client.SendAsync(httpRequestMessage).ConfigureAwait(false);

            //                var responseInfo = response1.GetAwaiter().GetResult();

            //                string result1 = responseInfo.Content.ReadAsStringAsync().Result;

            //                if (!responseInfo.IsSuccessStatusCode)
            //                {
            //                    if (responseInfo.StatusCode == System.Net.HttpStatusCode.BadRequest)
            //                    {
            //                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
            //                    }
            //                    else if (responseInfo.StatusCode == System.Net.HttpStatusCode.NotFound)
            //                    {
            //                        result1 = null;
            //                    }
            //                    else
            //                    {
            //                        throw new HttpRequestException($"{result1}", null, responseInfo.StatusCode);
            //                    }
            //                }

            //                Log($"Finalizou {subject}");
            //            }
            //            else
            //            {
            //                Log($"{subject} Situação: {situacao} Data conclusão: {dataResolvido}");
            //            }

            //        }
            //        catch (Exception e)
            //        {
            //            Log(e.Message);
            //        }

            //        Delay(5);

            //    }
            //}
            //catch (Exception e)
            //{
            //    Log(e);
            //}

            //Log("MonitorarAgendamentosFechadosSgrParaMovidesk finalizado");

        }