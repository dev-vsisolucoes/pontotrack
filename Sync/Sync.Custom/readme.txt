                IEnumerable<ParametroBO> parametroBOList = BusinessObjectManager.GetListByFilter<ParametroBO>("SYSAH_FILIAL_COD = 0");

                foreach (ParametroBO parametroBO in parametroBOList)
                {
                    String param = parametroBO.SYSAH_NOME;
                    String value = parametroBO.SYSAH_VALOR;

                    switch (param)
                    {
                        case "PARAM_0001":
                            moskitApiKey = value;

                            break;

                            //case "PARAM_0002":
                            //    wooCommerceApiKey = value;

                            //    break;

                            //case "PARAM_0003":
                            //    wooCommerceApiSecret = value;

                            //    break;

                            //case "PARAM_0004":
                            //    blingTimeBefore = NumberUtilities.parseInt(value);

                            //    break;
                    }
                }





        ////
        //// Parametros customizados do sistema
        ////
        //private string moskitApiKey;
        //private string wooCommerceApiKey;
        //private string wooCommerceApiSecret;
        //private int blingTimeBefore;
        //private string valorSubstring;
        
        
        
        
        //public int RoletaUsuario(string equipe, bool isFone)
        //{
        //    int usuarioId = 0;

        //    String sql = "SELECT ";
        //    sql += "SYSAQ_ID, SYSAQ_NUMERO ";
        //    sql += "FROM sysaq ";
        //    sql += "WHERE SYSAQ_INATIVO = 0 AND SYSAQ_USADO_ROLETA = 0 ";
        //    sql += "AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ";

        //    if (isFone)
        //    {
        //        sql += "AND SYSAQ_GERENTE = 1 ";
        //    }
        //    else
        //    {
        //        //sql += "AND SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_GERENTE = 0 ";
        //        sql += "AND SYSAQ_PARTICIPA_ROLETA = 1 ";
        //    }

        //    sql += "ORDER BY SYSAQ_NUMERO";

        //    DataTable usersDataTable = DataAccessManager.ExecuteDataTable(sql, null);

        //    if (usersDataTable.Rows.Count != 0)
        //    {
        //        DataRow row = usersDataTable.Rows[0];

        //        string id = StringUtilities.ToString(row["SYSAQ_ID"]);
        //        usuarioId = NumberUtilities.parseInt(usersDataTable.Rows[0]["SYSAQ_NUMERO"]);

        //        Object ret = DataAccessManager.ExecuteNonQuery($"UPDATE sysaq SET SYSAQ_USADO_ROLETA = 1 WHERE SYSAQ_ID = '{id}'", null);

        //        //
        //        // Se algo der errado, retornamos 0, 
        //        // para indicar que nao encontramos um usuario na rotela
        //        //
        //        if (ret != null)
        //        {
        //            usuarioId = 0;
        //        }

        //        //
        //        // Depois de ter pego o proximo usuario da roleta, 
        //        // vejo se não tem mais nenhum disponivel, se nao tiver, volto todos para SYSAQ_USADO_ROLETA = 0
        //        // para começar a roleta novamente
        //        //
        //        sql = "SELECT ";
        //        sql += "COUNT(SYSAQ_ID) AS QTD ";
        //        sql += "FROM sysaq ";
        //        //sql += "WHERE SYSAQ_INATIVO = 0 AND SYSAQ_USADO_ROLETA = 0 AND SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ";
        //        sql += "WHERE SYSAQ_INATIVO = 0 AND SYSAQ_USADO_ROLETA = 0 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ";

        //        if (isFone)
        //        {
        //            sql += "AND SYSAQ_GERENTE = 1 ";
        //        }
        //        else
        //        {
        //            //sql += "AND SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_GERENTE = 0 ";
        //            sql += "AND SYSAQ_PARTICIPA_ROLETA = 1 ";
        //        }

        //        int qtd = NumberUtilities.parseInt(DataAccessManager.ExecuteScalar(sql, null));

        //        if (qtd == 0)
        //        {
        //            sql = "UPDATE sysaq ";
        //            sql += "SET SYSAQ_USADO_ROLETA = 0 ";
        //            //sql += "FROM sysaq ";
        //            sql += "WHERE SYSAQ_INATIVO = 0 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ";

        //            if (isFone)
        //            {
        //                sql += "AND SYSAQ_GERENTE = 1 ";
        //            }
        //            else
        //            {
        //                //sql += "AND SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_GERENTE = 0 ";
        //                sql += "AND SYSAQ_PARTICIPA_ROLETA = 1 ";
        //            }

        //            //ret = DataAccessManager.ExecuteNonQuery($"UPDATE sysaq SET SYSAQ_USADO_ROLETA = 0 WHERE SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ", null);

        //            ret = DataAccessManager.ExecuteNonQuery(sql, null);

        //            //
        //            // Se algo der errado, retornamos 0, 
        //            // para indicar que nao encontramos um usuario na rotela
        //            //
        //            if (ret != null)
        //            {
        //                usuarioId = 0;
        //            }
        //        }
        //    }

        //    return usuarioId;
        //}

































                                    //
                                    // Tratar o nome do arquivo a ser criado
                                    //
                                    string folder = lojaBO.USUAM_NOME;
                                    folder = folder.Replace(" ", "_");
                                    folder = folder.Replace("/", "_");
                                    folder = folder.Replace("-", "_");
                                    folder = folder.ToLower();





                            //
                            //
                            //
                            List<Khronus.Framework.Integration.WooCommerce.Entities.Image> images = new List<Khronus.Framework.Integration.WooCommerce.Entities.Image>();

                            foreach (Imagem imagem in produto.Imagem)
                            {
                                try
                                {
                                    string path = Path.Combine(Application.StartupPath, "tmp");

                                    if(!Directory.Exists(path))
                                    {
                                        Directory.CreateDirectory(path);
                                    }

                                    //
                                    // Tratar o nome do arquivo a ser criado
                                    //
                                    string folder = lojaBO.USUAM_NOME;
                                    folder = folder.Replace(" ", "_");
                                    folder = folder.Replace("/", "_");
                                    folder = folder.Replace("-", "_");
                                    folder = folder.ToLower();

                                    string filename = new FileInfo(Path.GetTempFileName()).Name.Replace(".tmp", ".png").ToLower();

                                    string file = Path.Combine(path, filename);

                                    if (imagem.TipoArmazenamento.Equals("interno"))
                                    {
                                        //
                                        // Download da imagem do bling
                                        //
                                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imagem.Link);
                                        //request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.Default.GetBytes(client.ApiKey + ":" + client.ApiSecret))}");
                                        request.Method = "GET";
                                        //request.ContentType = "image/png";

                                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                        //response.ContentType
                                        //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                                        //byte[] data = Encoding.UTF8.GetBytes(json);

                                        //using(FileStream fileStream = new FileStream(Path.Combine(@"c:\temp\", filename), FileMode.CreateNew))
                                        //{
                                        //    fileStream.Write(data, 0, data.Length);
                                        //}

                                        using (Stream stream = response.GetResponseStream())
                                        {
                                            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                                            img.Save(file);

                                            //stream.Close();
                                        }

                                        //RestClient
                                        ////
                                        //// Upload da imagem no servidor VSI 
                                        ////      
                                        //string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                                        //string contentType = request.ContentType; // "image/png";

                                        ////request = (HttpWebRequest)WebRequest.Create($"https://www.vsisolucoes.com.br/clientes/mude/{folder}/{filename}");
                                        ////request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.Default.GetBytes(client.ApiKey + ":" + client.ApiSecret))}");
                                        //request = (HttpWebRequest)WebRequest.Create($"http://localhost/mude/api/integracao/upload/image");
                                        //request.Method = "POST";
                                        //request.ContentType = contentType;
                                        //request.KeepAlive = true;
                                        //request.ContentType = "multipart/form-data; boundary=" + boundary;

                                        //using (var stream = request.GetRequestStream())
                                        //{
                                        //    //
                                        //    //
                                        //    //
                                        //    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";

                                        //    string header = string.Format(headerTemplate, "image", file, contentType);

                                        //    byte[] data = System.Text.Encoding.UTF8.GetBytes(header);

                                        //    stream.Write(data, 0, data.Length);

                                        //    //
                                        //    //
                                        //    //
                                        //    FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                                        //    byte[] buffer = new byte[4096];
                                        //    int bytesRead = 0;
                                        //    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                        //    {
                                        //        stream.Write(buffer, 0, bytesRead);
                                        //    }
                                        //    fileStream.Close();

                                        //    //
                                        //    //
                                        //    //
                                        //    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                                        //    stream.Write(trailer, 0, trailer.Length);
                                        //    //rs.Close();
                                        //}

                                        //response = (HttpWebResponse)request.GetResponse();

                                        //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                                        //
                                        // Upload da imagem no servidor VSI - VERSAO 2
                                        //  
                                        var restClient = new RestClient("http://localhost/mude/api/integracao/upload/image");
                                        restClient.Timeout = -1;
                                        
                                        var restRequest = new RestRequest(Method.POST);
                                        restRequest.AddHeader("Cookie", "PHPSESSID=aofejlg2lv1ssnngdc59rh6to2; XDEBUG_SESSION=netbeans-xdebug");
                                        //restRequest.AddFile("image", "jzIA5TGb-/tmp19c7.png");
                                        restRequest.AddFile("image", file);

                                        IRestResponse restResponse = restClient.Execute(restRequest);
                                        Console.WriteLine(restResponse.Content);
                                    }

                                    Image image = new Image();
                                    //image.Src = imagem.Link;
                                    ////image.Src = new Uri("D:\\Drivers\\Particular\\Dropbox\\Condominio\\Fotos\\Photos\\20210207_122931.jpg");
                                    //image.Src = new Uri("http://orgbling.s3.amazonaws.com/8d1083b0f7f323df7e2a0d0367f576b1/ce690d309be51c737434f0cae6cee833?AWSAccessKeyId=AKIATCLMSGFX4G7QTFVD&Expires=1637090140&Signature=wdaWxXVF15ATW6%2B2ItBwSvjuD6U%3D");
                                    ////image.Src = new Uri("https://vsisolucoes.com.br/clientes/mude/img/1.jpg");
                                    image.Src = new Uri("https://vsisolucoes.com.br/clientes/mude/tmp/{}");

                                    images.Add(image);
                                }
                                catch (Exception e)
                                {

                                }
                            }










        public Worker()
        {
            //try
            //{
            //    AppName = Assembly.GetExecutingAssembly().GetName().Name;

            //    //LogFileName = Application.StartupPath + "\\" + AppName + ".log";
            //    LogFileName = Application.StartupPath + "\\sync.log";

            //    LogApp("inicializado...");

            //    //
            //    //
            //    //
            //    String server = "mysql.vsisolucoes.com.br";
            //    String port = "3306";
            //    String database = "vsisolucoe149";
            //    String user = "vsisolucoe149";
            //    String password = "V123456";
            //    String options = "charset=utf8";
            //    String connection = "server=" + server + ";port=" + port + ";database=" + database + ";Uid=" + user + ";Pwd=" + password + ";" + options;

            //    //worker.DataAccessManager = DataAccessManager.GetInstance();
            //    DataAccessManager.ConnectionProvider = ConnectionProviders.MYSQL;
            //    DataAccessManager.ConnectionString = connection;
            //}
            //catch (Exception ex)
            //{
            //    LogApp(ex);
            //}
        }

        //public static Worker GetInstance()
        //{
        //    if (worker == null)
        //    {
        //        worker = new Worker();
        //    }

        //    return worker;
        //}






        public void Sync13()
        {
            Object ret = null;

            if (ThreadIsRunning1)
            {
                return;
            }

            ThreadIsRunning1 = true;

            LogApp("Sync1 iniciado...");

            try
            {
                //
                // Apenas lojas ativas
                //
                DataTable dataTable = new LojaBO().GetList(0, false);

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    string id = StringUtilities.ToString(dataRow["USUAM_ID"]);

                    LojaBO lojaBO = new LojaBO().Find(0, id);

                    List<Product> produtosWoocomerceList = new List<Product>();

                    //
                    // Loja com erp BLING
                    //
                    if (lojaBO.USUAM_ERP_CODIGO == 1)
                    {
                        BlingClient blingClient = new BlingClient();
                        blingClient.Token = lojaBO.USUAM_ERP_BLING_TOKEN;

                        ProdutoService produtoService = new ProdutoService(blingClient);

                        string filter = null;

                        // O teste do paramentro vem aqui
                        if (1 == 2)
                        {
                            //"dataInclusao[26/10/2021 16:00:00 TO 26/10/2021 23:59:59];dataAlteracao[26/10/2021 16:00:00 TO 26/10/2021 23:59:59]"
                            filter =
                                //"dataInclusao[" +
                                //    DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm:ss") + " TO " +
                                //    DateTime.Today.ToString("dd/MM/yyyy") + " 23:59:59];" +
                                "dataAlteracao[" +
                                    DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm:ss") + " TO " +
                                    DateTime.Today.ToString("dd/MM/yyyy") + " 23:59:59]";
                        }

                        IEnumerable<ValueObject> produtos = produtoService.GetList(filter);
                        //IEnumerable<ValueObject> produtos = produtoService.GetList();

                        foreach (Produto produto in produtos)
                        {
                            // Produto no woocommerce
                            Product product = new Product();

                            product.Name = produto.Descricao;
                            product.ShortDescription = produto.DescricaoCurta;
                            product.Description = produto.DescricaoComplementar;
                            product.Type = "simple";
                            product.RegularPrice = produto.Preco;
                            product.CatalogVisibility = "visible";
                            product.Vendor = NumberUtilities.parseInt(lojaBO.USUAM_ERP_WOO_LOJA_CODIGO);
                            product.Sku = produto.Id;
                            product.StockQuantity = produto.EstoqueAtual;

                            produtosWoocomerceList.Add(product);
                        }
                    }
                    //
                    // ContaAzul
                    // 
                    else if (lojaBO.USUAM_ERP_CODIGO == 2)
                    {

                    }
                    //
                    // Omie
                    // 
                    else if (lojaBO.USUAM_ERP_CODIGO == 3)
                    {

                    }

                    //
                    // Cria o Evento                    //
                    if (produtosWoocomerceList.Count > 0)
                    {
                        foreach (Product produto in produtosWoocomerceList)
                        {
                            //JObject evento = new JObject(
                            //        //new JProperty("Campanhas", new JObject(
                            //        new JProperty("Name", produto.Name),
                            //        new JProperty("ShortDescription", produto.ShortDescription),
                            //        new JProperty("Type", produto.Type),
                            //        new JProperty("RegularPrice", produto.RegularPrice),
                            //        new JProperty("CatalogVisibility", produto.CatalogVisibility),
                            //        new JProperty("Vendor", produto.Vendor),
                            //        new JProperty("Sku", produto.Sku),
                            //        new JProperty("StockQuantity", produto.StockQuantity)
                            ////))
                            //);
                            string evento = JObject.FromObject(produto).ToString();

                            //String hash = CryptoUtilities.GetMD5Hash(evento.ToString());

                            IntegracaoBO integracaoBO = new IntegracaoBO().FindByEvento(0, 1, evento);

                            if (integracaoBO == null)
                            {
                                integracaoBO = new IntegracaoBO();
                                integracaoBO.USUAN_FILIAL = 0;
                                integracaoBO.USUAN_TIPO = 1;
                                integracaoBO.USUAN_DATA = DateTime.Now.AddDays(0);
                                integracaoBO.USUAN_EVENTO = evento.ToString();
                                //integracaoBO.USUAN_IDENTIFICACAO = email;
                                integracaoBO.USUAN_STATUS = "00";
                                //integracaoBO.USUAN_HASH = hash;

                                ret = integracaoBO.Insert();

                                if (ret != null)
                                {
                                    //break;
                                }
                            }
                            else
                            {
                                string eventoAtualiza = integracaoBO.USUAN_EVENTO.ToString();

                                if (eventoAtualiza.Equals(evento.ToString()))
                                {
                                    //integracaoBO = new IntegracaoBO();
                                    integracaoBO.USUAN_FILIAL = 0;
                                    integracaoBO.USUAN_TIPO = 1;
                                    integracaoBO.USUAN_DATA = DateTime.Now.AddDays(0);
                                    integracaoBO.USUAN_EVENTO = evento.ToString();
                                    //integracaoBO.USUAN_IDENTIFICACAO = email;
                                    integracaoBO.USUAN_STATUS = "00";
                                    //integracaoBO.USUAN_HASH = hash;

                                    ret = integracaoBO.Update();

                                    if (ret != null)
                                    {
                                        //break;
                                    }
                                }

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp("Sync1 finalizado...");

            ThreadIsRunning1 = false;
        }

        public void Sync12()
        {
            Object ret = null;

            if (ThreadIsRunning2)
            {
                return;
            }

            ThreadIsRunning2 = true;

            LogApp("Sync1 iniciado...");

            try
            {
                //
                // Apenas lojas ativas
                //
                DataTable dataTable = new LojaBO().GetList(0, false);

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    string id = StringUtilities.ToString(dataRow["USUAM_ID"]);

                    LojaBO lojaBO = new LojaBO().Find(0, id);

                    List<Product> produtosWoocomerceList = new List<Product>();

                    //
                    // Loja com erp BLING
                    //
                    if (lojaBO.USUAM_ERP_CODIGO == 1)
                    {
                        BlingClient blingClient = new BlingClient();
                        blingClient.Token = lojaBO.USUAM_ERP_BLING_TOKEN;

                        ProdutoService produtoService = new ProdutoService(blingClient);

                        //string filter = null;

                        //// O teste do paramentro vem aqui
                        //if (1 == 2) {
                        //    //"dataInclusao[26/10/2021 16:00:00 TO 26/10/2021 23:59:59];dataAlteracao[26/10/2021 16:00:00 TO 26/10/2021 23:59:59]"
                        //    filter = 
                        //        //"dataInclusao[" +
                        //        //    DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm:ss") + " TO " +
                        //        //    DateTime.Today.ToString("dd/MM/yyyy") + " 23:59:59];" +
                        //        "dataAlteracao[" +
                        //            DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm:ss") + " TO " +
                        //            DateTime.Today.ToString("dd/MM/yyyy") + " 23:59:59]";
                        //}

                        //IEnumerable<ValueObject> produtos = produtoService.GetList(filter);
                        IEnumerable<ValueObject> produtos = produtoService.GetList();

                        foreach (Produto produto in produtos)
                        {
                            // Produto no woocommerce
                            Product product = new Product();

                            product.Name = produto.Descricao;
                            product.ShortDescription = produto.DescricaoCurta;
                            product.Description = produto.DescricaoComplementar;
                            product.Type = "simple";
                            product.RegularPrice = produto.Preco;
                            product.CatalogVisibility = "visible";
                            product.Vendor = NumberUtilities.parseInt(lojaBO.USUAM_ERP_WOO_LOJA_CODIGO);
                            product.Sku = produto.Id;
                            product.StockQuantity = produto.EstoqueAtual;

                            produtosWoocomerceList.Add(product);
                        }
                    }
                    //
                    // ContaAzul
                    // 
                    else if (lojaBO.USUAM_ERP_CODIGO == 2)
                    {

                    }
                    //
                    // Omie
                    // 
                    else if (lojaBO.USUAM_ERP_CODIGO == 3)
                    {

                    }

                    //
                    // Cria o produto do woocommerce
                    //
                    if (produtosWoocomerceList.Count > 0)
                    {
                        WooCommerceClient client = new WooCommerceClient();
                        client.ApiUri = "https://inspireemude.com.br/";
                        //client.ApiKey = "ck_4ce27d0799b402493d696de65ed74e46c137b27b"; // pegar de parametros
                        //client.ApiSecret = "cs_b6cf9b6cf8c805ed50c7a409360688f9af0546c5"; // pegar de parametros
                        client.ApiKey = "ck_4ce27d0799b402493d696de65ed74e46c137b27b"; // pegar de parametros
                        client.ApiSecret = "cs_b6cf9b6cf8c805ed50c7a409360688f9af0546c5"; // pegar de parametros

                        foreach (Product produto in produtosWoocomerceList)
                        {
                            //
                            // Criacao do produto
                            //
                            ProductService productService = new ProductService(client);

                            //Product product = (Product)productService.GetBySku(produto.Sku);

                            //bool found = true;

                            //if (product == null)
                            //{
                            //    found = false;

                            //    product = new Product();
                            //}

                            //product.Name = produto.Name;
                            //product.ShortDescription = produto.ShortDescription;
                            //product.Description = produto.Description;
                            //product.Type = produto.Type;
                            //product.RegularPrice = produto.RegularPrice;
                            //product.CatalogVisibility = produto.CatalogVisibility;
                            //product.Vendor = produto.Vendor;
                            //product.Sku = produto.Sku;
                            //product.StockQuantity = produto.StockQuantity;

                            //ServiceReturn resp = null;

                            //if (found)
                            //{
                            //    resp = productService.Put(product);
                            //}
                            //else
                            //{
                            //    resp = productService.Post(product);
                            //}

                            //if (resp.Status != 200)
                            //{
                            //    throw new KException(resp.Message.ToString());
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp("Sync1 finalizado...");

            ThreadIsRunning2 = false;
        }

        public void Sync4()
        {
            Object ret = null;

            if (ThreadIsRunning4)
            {
                return;
            }

            ThreadIsRunning4 = true;

            LogApp("Sync4 iniciado...");

            try
            {

                DataTable dataTable = new IntegracaoBO().GetList(0, 0, "00");

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    //
                    // Parse do evento
                    //
                    string id = StringUtilities.ToString(dataRow["USUAN_ID"]);

                    IntegracaoBO integracaoBO = new IntegracaoBO().Find(0, id);

                    JObject row = null;

                    try
                    {
                        row = JObject.Parse(StringUtilities.ToString(integracaoBO.USUAN_EVENTO));
                    }
                    catch (Exception)
                    {

                    }

                    if (row == null)
                    {
                        continue;
                    }

                    //
                    // Recupera os dados do produto via evento
                    //

                    string name = StringUtilities.ToString(row["event"]["descricao"]);
                    Decimal preco = NumberUtilities.parseDecimal(row["event"]["valor_unitario"]);
                    string unidade = StringUtilities.ToString(row["event"]["unidade"]);
                    string descricao = StringUtilities.ToString(row["event"]["descr_detalhada"]);

                    //
                    // Grava no WooCommerce
                    //
                    WooCommerceClient client = new WooCommerceClient();
                    client.ApiUri = "https://inspireemude.com.br/";
                    client.ApiKey = "ck_4ce27d0799b402493d696de65ed74e46c137b27b"; // pegar de parametros
                    client.ApiSecret = "cs_b6cf9b6cf8c805ed50c7a409360688f9af0546c5"; // pegar de parametros

                    //
                    // Criacao do produto
                    //
                    Product product = new Product();
                    product.Name = "Premium Quality";
                    product.Type = "simple";
                    product.RegularPrice = "21.99";
                    product.Description = "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vestibulum tortor quam, feugiat vitae, ultricies eget, tempor sit amet, ante. Donec eu libero sit amet quam egestas semper. Aenean ultricies mi vitae est. Mauris placerat eleifend leo.";
                    product.ShortDescription = "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.";

                    ProductService productService = new ProductService(client);

                    ServiceReturn resp = productService.Post(product);

                    string status = "00";
                    string message = resp.Message.ToString();
                    object eventReturn = null;

                    if (resp.Status == 200)
                    {
                        status = "10";
                        message = "Processado com sucesso";
                        eventReturn = resp.Message;

                        if (resp.Status == 500)
                        {
                            status = "20";
                            message = "Falha ao processar o evento";
                            eventReturn = resp.Message;
                        }
                    }

                    ret = integracaoBO.UpdateFields(new string[] { "USUAN_STATUS", "USUAN_STATUS_MOTIVO", "USUAN_EVENTO_RETORNO" }, new object[] { status, message, eventReturn });

                }
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp("Sync4 finalizado...");

            ThreadIsRunning4 = false;
        }

        private ServiceReturn criaPedidoLogista(JObject row, LojaBO loja)
        {
            ServiceReturn resp = null;

            try
            {
                //if (loja.USUAM_LOJA.Equals("BLING"))
                //{
                //    BlingClient blingClient = new BlingClient();
                //    blingClient.Token = loja.USUAM_API_KEY;

                //    PedidoService pedidoService = new PedidoService(blingClient);

                //    //
                //    //Cliente
                //    //
                //    JObject clienteRow = JObject.Parse(StringUtilities.ToString(row["billing"]));
                //    string cpf = StringUtilities.ToString(clienteRow["cpf"]);
                //    string cnpj = StringUtilities.ToString(clienteRow["cnpj"]);

                //    Cliente cliente = new Cliente();
                //    cliente.Nome = StringUtilities.ToString(clienteRow["first_name"]) + " " + StringUtilities.ToString(clienteRow["last_name"]);
                //    cliente.TipoPessoa = StringUtilities.ToString(clienteRow["persontype"]);
                //    cliente.CpfCnpj = String.IsNullOrEmpty(cpf) ? cnpj : cpf;
                //    cliente.Email = StringUtilities.ToString(clienteRow["email"]);
                //    cliente.Fone = StringUtilities.ToString(clienteRow["phone"]);
                //    cliente.Endereco = StringUtilities.ToString(clienteRow["address_1"]);
                //    cliente.Numero = StringUtilities.ToString(clienteRow["number"]);
                //    cliente.Complemento = StringUtilities.ToString(clienteRow["persontype"]);
                //    cliente.Bairro = StringUtilities.ToString(clienteRow["neighborhood"]);
                //    cliente.Cidade = StringUtilities.ToString(clienteRow["city"]);
                //    cliente.Uf = StringUtilities.ToString(clienteRow["state"]);
                //    cliente.Cep = StringUtilities.ToString(clienteRow["postcode"]);
                //    cliente.Ie = StringUtilities.ToString(clienteRow["ie"]);

                //    //
                //    //Transporte
                //    //
                //    //JObject transporteRow = JObject.Parse(StringUtilities.ToString(row["billing"]));

                //    Transporte transporte = new Transporte();
                //    transporte.TipoFrete = "FOB";
                //    transporte.Transportadora = "Teste";
                //    transporte.Volumes = null;
                //    transporte.DadosEtiqueta = null;
                //    transporte.ServicoCorreios = null;

                //    //
                //    //Pedido
                //    //
                //    Pedido pedido = new Pedido();
                //    pedido.Loja = loja.USUAM_CODIGO;
                //    pedido.VlrFrete = NumberUtilities.parseFloat(row["shipping_total"]);
                //    pedido.VlrDesconto = NumberUtilities.parseFloat(row["discount_total"]);
                //    pedido.Cliente = cliente;
                //    pedido.Transporte = transporte;

                //    //Parcelas
                //    //
                //    Parcelas parcelas = new Parcelas();

                //    Parcela parcela = new Parcela();
                //    parcela.Data = StringUtilities.ToString(row["date_created"]);
                //    parcela.Vlr = StringUtilities.ToString(row["total"]);

                //    parcelas.Parcela.Add(parcela);

                //    pedido.Parcelas = parcelas;

                //    //
                //    //Adicionar Itens
                //    //
                //    JArray itensRow = (JArray)row["line_items"];

                //    Itens itens = new Itens();

                //    foreach (JObject it in itensRow)
                //    {
                //        Item item = new Item();
                //        item.Codigo = StringUtilities.ToString(it["id"]);
                //        item.Descricao = StringUtilities.ToString(it["name"]);
                //        item.VlrUnit = StringUtilities.ToString(it["subtotal"]);
                //        item.Qtde = StringUtilities.ToString(it["quantity"]);
                //        item.Un = "UN";

                //        itens.Item.Add(item);
                //    }

                //    foreach (JObject it in itensRow)
                //    {
                //        Item item = new Item();
                //        item.Codigo = StringUtilities.ToString(it["id"]);
                //        item.Descricao = StringUtilities.ToString(it["name"]);
                //        item.VlrUnit = StringUtilities.ToString(it["subtotal"]);
                //        item.Qtde = StringUtilities.ToString(it["quantity"]);
                //        item.Un = "UN";

                //        itens.Item.Add(item);
                //    }

                //    pedido.Itens = itens;

                //    resp = pedidoService.Post(pedido);
                //}
                //else if (loja.USUAM_LOJA.Equals("CONTAAZUL"))
                //{
                //    resp.Status = 500;
                //}
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            return resp;
        }

















        //private int roletaUsuario(string equipe)
        //{
        //    int usuarioId = 0;

        //    String sql = "SELECT ";
        //    sql += "SYSAQ_ID, SYSAQ_NUMERO ";
        //    sql += "FROM sysaq ";
        //    sql += "WHERE SYSAQ_INATIVO = 0 AND SYSAQ_USADO_ROLETA = 0 AND SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ";
        //    sql += "ORDER BY SYSAQ_NUMERO";

        //    DataTable usersDataTable = DataAccessManager.ExecuteDataTable(sql, null);

        //    if (usersDataTable.Rows.Count != 0)
        //    {
        //        DataRow row = usersDataTable.Rows[0];

        //        string id = StringUtilities.ToString(row["SYSAQ_ID"]);
        //        usuarioId = NumberUtilities.parseInt(usersDataTable.Rows[0]["SYSAQ_NUMERO"]);

        //        Object ret = DataAccessManager.ExecuteNonQuery($"UPDATE sysaq SET SYSAQ_USADO_ROLETA = 1 WHERE SYSAQ_ID = '{id}'", null);

        //        //
        //        // Se algo der errado, retornamos 0, 
        //        // para indicar que nao encontramos um usuario na rotela
        //        //
        //        if (ret != null)
        //        {
        //            usuarioId = 0;
        //        }

        //        //
        //        // Depois de ter pego o proximo usuario da roleta, 
        //        // vejo se não tem mais nenhum disponivel, se nao tiver, volto todos para SYSAQ_USADO_ROLETA = 0
        //        // para começar a roleta novamente
        //        //
        //        sql = "SELECT ";
        //        sql += "COUNT(SYSAQ_ID) AS QTD ";
        //        sql += "FROM sysaq ";
        //        sql += "WHERE SYSAQ_INATIVO = 0 AND SYSAQ_USADO_ROLETA = 0 AND SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ";

        //        int qtd = NumberUtilities.parseInt(DataAccessManager.ExecuteScalar(sql, null));

        //        if (qtd == 0)
        //        {
        //            sql = "UPDATE sysaq";
        //            sql += "SET SYSAQ_USADO_ROLETA = 0 ";
        //            sql += "FROM sysaq ";
        //            sql += "WHERE SYSAQ_INATIVO = 0 AND SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ";

        //            ret = DataAccessManager.ExecuteNonQuery($"UPDATE sysaq SET SYSAQ_USADO_ROLETA = 0 WHERE SYSAQ_PARTICIPA_ROLETA = 1 AND SYSAQ_EQUIPE_NAME LIKE '%" + equipe + "%' ", null);

        //            //
        //            // Se algo der errado, retornamos 0, 
        //            // para indicar que nao encontramos um usuario na rotela
        //            //
        //            if (ret != null)
        //            {
        //                usuarioId = 0;
        //            }
        //        }
        //    }

        //    return usuarioId;
        //}




                int filial = 0;

                byte[] bytes = null;
                HttpWebRequest request = null;
                HttpWebResponse response = null;

                int quantity = 50;
                int start = 0;
                int registro = 1;

                //
                // Busca usuário
                //

                do
                {
                    JArray users = (JArray)SendCommand($"https://api.moskitcrm.com/v2/users?start={start + registro}&quantity={quantity}", "GET", null, moskitApiKey);

                    if (users.Count == 0)
                    {
                        registro = 1;
                    }

                    foreach (JObject user in users)
                    {
                        if (!IsRunning)
                        {
                            return;
                        }

                        String usuarioId = StringUtilities.ToString(user["id"]);
                        String usuario = StringUtilities.ToString(user["name"]);
                        string email = StringUtilities.ToString(user["username"]);

                        int equipeId = NumberUtilities.parseInt(user["team"]["id"]);

                        JObject equipe = (JObject)SendCommand($"https://api.moskitcrm.com/v2/teams/{equipeId}", "GET", null, moskitApiKey);

                        String equipeName = StringUtilities.ToString(equipe["name"]);

                        UsuarioBO usuarioBO = new UsuarioBO().FindByNumero(0, usuarioId, null);

                        if (usuarioBO != null)
                        {
                            ret = usuarioBO.UpdateFields(new String[] { "SYSAQ_USUARIO", "SYSAQ_EMAIL" }, new Object[] { usuario, email });

                            if (ret != null)
                            {
                                break;
                            }
                        }
                        else
                        {
                            usuarioBO = new UsuarioBO();
                            usuarioBO.SYSAQ_FILIAL_COD = 0;
                            usuarioBO.SYSAQ_CODIGO = 0;
                            usuarioBO.SYSAQ_NUMERO = usuarioId;
                            usuarioBO.SYSAQ_USUARIO = usuario;
                            usuarioBO.SYSAQ_EQUIPE_ID = equipeId;
                            usuarioBO.SYSAQ_EQUIPE_NAME = equipeName;
                            usuarioBO.SYSAQ_EMAIL = email;
                            usuarioBO.SYSAQ_PARTICIPA_ROLETA = false;
                            usuarioBO.SYSAQ_USADO_ROLETA = false;
                            usuarioBO.SYSAQ_INATIVO = false;

                            ret = usuarioBO.Insert();
                        }

                        if (ret != null)
                        {
                            break;
                        }

                        registro++;
                    }

                    if (ret != null)
                    {
                        throw new Exception(StringUtilities.ToString(ret));
                    }


                } while (true);








        public void Sync3()
        {
            Object ret = null;

            //moskitApiKeyV1 = "55e17ecf-eddc-4a6e-bc4e-b06df4488afe";   //Teste Moskit wlbmaciel
            //moskitApiKeyV2 = "b2dd8a0e-a4a6-47d1-8306-00658b85d7fe";   //Teste Moskit wlbmaciel

            //int stageCheflera = 162127; // Filial 0
            //int stageSrEspeto = 161981; // Filial 1

            //stageSrEspeto = 131427; //Teste Moskit wlbmaciel

            if (ThreadIsRunning3)
            {
                return;
            }

            ThreadIsRunning3 = true;

            LogApp("Sync3 iniciado...");

            try
            {
                for (int filial = 0; filial <= 1; filial++)
                {
                    DataTable dataTable = new ContatoBO().GetList(filial, "00");

                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        try
                        {
                            String id = StringUtilities.ToString(dataRow["GERAN_ID"]);
                            String nome = StringUtilities.ToString(dataRow["GERAN_NOME"]);
                            String email = StringUtilities.ToString(dataRow["GERAN_EMAIL"]);
                            String telefone = StringUtilities.ToString(dataRow["GERAN_FONE"]);

                            String dataAtual = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString("f0");
                            long dataInt = NumberUtilities.parseLong(dataAtual);

                            //nome = "Robson";
                            String nomeNegocio = "" + (filial == 0 ? "CH " : "SE ") + DateTime.Today.ToString("dd.MM.yy") + " " + nome;

                            String status1 = "10";


                            //String celular = "55" + StringUtilities.ToString(dataRow["GERAN_CELULAR"]);
                            //celular = StringUtilities.Clear(celular);
                            //celular = celular.Replace(" ", "");


                            // Criacao do negocio e contato no moskit

                            //
                            // Grava no MOSKIT, empresa, contato e negocio
                            //

                            //
                            // Busca o contato por email
                            //
                            var param = JArray.Parse("[{\"field\": \"emails\",\"expression\": \"like\", \"values\": [\"" + email + "\" ]}]");

                            JArray results = (JArray)SendCommand("https://api.moskitcrm.com/v2/contacts/search", "POST", param, moskitApiKeyV2);

                            if (results.Count == 0)
                            {
                                int idUser = roletaUsuario();

                                //idUser = 42966; //Teste

                                //
                                // Contato
                                //
                                JObject contact = new JObject(
                                    new JProperty("entity", "Contact"),
                                    new JProperty("name", nomeNegocio),
                                    new JProperty("emails", new JArray(new JObject(
                                        new JProperty("entity", "Email"),
                                        new JProperty("address", email),
                                            new JProperty("type", new JObject(
                                                new JProperty("entity", "EmailType"),
                                                new JProperty("name", "Comercial")
                                            ))
                                        ))
                                    ),
                                    new JProperty("primaryEmail", new JObject(
                                        new JProperty("entity", "Email"),
                                        new JProperty("address", email),
                                        new JProperty("type", new JObject(
                                            new JProperty("entity", "EmailType"),
                                            new JProperty("name", "Comercial")
                                        ))
                                    )),
                                    new JProperty("phones", new JArray(
                                        new JObject(
                                            new JProperty("entity", "Phone"),
                                            new JProperty("number", telefone),
                                            new JProperty("type", new JObject(
                                                new JProperty("entity", "PhoneType"),
                                                new JProperty("name", "Celular")
                                            ))
                                        ))
                                    ),
                                    new JProperty("primaryPhone", new JObject(
                                        new JProperty("entity", "Phone"),
                                        new JProperty("number", telefone),
                                        new JProperty("type", new JObject(
                                            new JProperty("entity", "PhoneType"),
                                            new JProperty("name", "Celular")
                                        ))
                                    )),
                                    //new JProperty("customFieldValues", new JArray(
                                    //     new JObject(
                                    //        new JProperty("entity", "CustomFieldValue"),
                                    //        new JProperty("value", cpf),
                                    //        new JProperty("customField", new JObject(
                                    //            new JProperty("id", "164972"),
                                    //            new JProperty("entity", "CustomField")
                                    //        ))
                                    //    )
                                    //)),
                                    new JProperty("responsible", new JObject(
                                        new JProperty("id", idUser),
                                        new JProperty("entity", "User")
                                    ))
                                );

                                contact = (JObject)SendCommand("https://api.moskitcrm.com/v1/contacts/", "POST", contact, moskitApiKeyV1);

                                //
                                // Cria Negocio
                                //

                                JObject deal = new JObject(
                                    new JProperty("entity", "Deal"),
                                    new JProperty("name", nomeNegocio),
                                    new JProperty("previsionCloseDate", (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString("f0")),
                                    new JProperty("status", "OPEN"),

                                    new JProperty("stage", new JObject(
                                        new JProperty("entity", "Stage"),
                                        new JProperty("id", filial == 0 ? stageCheflera : stageSrEspeto)
                                    )),
                                    new JProperty("contacts", new JArray(
                                        new JObject(
                                            new JProperty("entity", "Contact"),
                                            new JProperty("id", contact["id"])
                                        ))
                                    ),
                                    //new JProperty("customFieldValues", new JArray(
                                    //    new JObject(
                                    //    new JProperty("entity", "CustomFieldValue"),
                                    //    new JProperty("value", curso),
                                    //    new JProperty("customField", new JObject(
                                    //        new JProperty("id", "163103"),
                                    //        new JProperty("entity", "CustomField")
                                    //    ))
                                    //    )                                  

                                    //)),
                                    new JProperty("responsible", new JObject(
                                        new JProperty("entity", "User"),
                                        new JProperty("id", idUser)
                                    ))
                                );

                                deal = (JObject)SendCommand("https://api.moskitcrm.com/v1/deals/", "POST", deal, moskitApiKeyV1);

                            }
                            else
                            {
                                status1 = "20";
                            }
                            //else
                            //{
                            //    //int idUser = roletaUsuario();

                            //    //
                            //    // Contato
                            //    //
                            //    JObject contact = (JObject)results[0];


                            //    //Atualiza contato

                            //    contact["name"] = nome;
                            //    if (contact["phones"] != null && ((JArray)contact["phones"]).Count > 0)
                            //    {
                            //        contact["primaryPhone"]["number"] = telefone;
                            //    }
                            //    else
                            //    {
                            //        if (contact["phones"] == null)
                            //        {
                            //            contact.Add(new JProperty("phones"));
                            //        }

                            //        ((JArray)contact["phones"]).Add(new JObject(
                            //                new JProperty("entity", "Phone"),
                            //                new JProperty("number", telefone),
                            //                new JProperty("type", new JObject(
                            //                    new JProperty("entity", "PhoneType"),
                            //                    new JProperty("name", "Celular")
                            //                ))
                            //            ));
                            //    }

                            //    SendCommand("https://api.moskitcrm.com/v1/contacts/" + contact["id"], "PUT", contact, moskitApiKeyV1);

                            //    //
                            //    // Busca Negocio
                            //    //
                            //    if (((JArray)contact["deals"]).Count != 0)
                            //    {
                            //        JObject deal = null;
                            //        String funilName = null;
                            //        int idNegocio = 0;

                            //        foreach (JObject item in ((JArray)contact["deals"]))
                            //        {
                            //            idNegocio = NumberUtilities.parseInt(item["id"]);
                            //            deal = (JObject)SendCommand("https://api.moskitcrm.com/v1/deals/" + idNegocio, "GET", null, moskitApiKeyV1);

                            //            deal["name"] = nomeNegocio;
                            //            deal["stage"]["id"] = filial == 0 ? stageCheflera : stageSrEspeto;
                            //            SendCommand("https://api.moskitcrm.com/v1/deals/" + idNegocio, "PUT", deal, moskitApiKeyV1);

                            //        }
                            //    }
                            //}

                            //
                            //
                            //
                            ContatoBO contatoBO = new ContatoBO().Find(filial, id);

                            if (contatoBO != null)
                            {
                                ret = contatoBO.UpdateFields(new String[] { "GERAN_STATUS" }, new Object[] { status1 });

                                if (ret != null)
                                {
                                    break;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            LogApp(ex);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp("Sync3 finalizado...");

            ThreadIsRunning3 = false;
        }

        public void Sync4()
        {
            Object ret = null;
            DataTable dataTable = null;
            String sql = "";

            //moskitApiKeyV1 = "55e17ecf-eddc-4a6e-bc4e-b06df4488afe";
            //moskitApiKeyV2 = "b2dd8a0e-a4a6-47d1-8306-00658b85d7fe";

            if (ThreadIsRunning4)
            {
                return;
            }

            ThreadIsRunning4 = true;

            LogApp("SyncUsuario iniciado...");

            try
            {
                int filial = 0;

                byte[] bytes = null;
                HttpWebRequest request = null;
                HttpWebResponse response = null;

                //
                // Busca usuário
                //
                request = (HttpWebRequest)WebRequest.Create("https://api.moskitcrm.com/v1/users/?limit=100");
                request.Headers.Add("apikey", moskitApiKeyV1);
                request.Method = "GET";

                response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    //throw new Exception(response.StatusDescription);
                }

                var result = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());

                var results = (JArray)result["results"];

                foreach (var res in results)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    String usuarioId = StringUtilities.ToString(res["id"]);
                    String usuario = StringUtilities.ToString(res["name"]);

                    UsuarioBO usuarioBO = new UsuarioBO().FindByNumero(0, usuarioId, null);

                    if (usuarioBO != null)
                    {
                        ret = usuarioBO.UpdateFields(new String[] { "SYSAQ_USUARIO" }, new Object[] { usuario });

                        if (ret != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        usuarioBO = new UsuarioBO();
                        usuarioBO.SYSAQ_FILIAL_COD = 0;
                        usuarioBO.SYSAQ_CODIGO = 0;
                        usuarioBO.SYSAQ_NUMERO = usuarioId;
                        usuarioBO.SYSAQ_USUARIO = usuario;
                        usuarioBO.SYSAQ_INATIVO = false;
                        usuarioBO.SYSAQ_PARTICIPA_ROLETA = false;

                        ret = usuarioBO.Insert();
                    }


                    if (ret != null)
                    {
                        break;
                    }
                }

                if (ret != null)
                {
                    throw new Exception(StringUtilities.ToString(ret));
                }
            }
            catch (Exception ex)
            {
                LogApp(ex);
            }

            LogApp("SyncUsuario finalizado...");

            ThreadIsRunning4 = false;
        }














                GoogleManager manager = GoogleManager.GetInstance();
                manager.Credential = Application.StartupPath + "\\credentials.json";

                Sheets sheets = new Sheets();
                //sheets.SpreadSheetId = "1xJCuvFZj1UK1EDrU6HqmP_zZtx56nM1GIMIRfyt49Rw"; 1xJCuvFZj1UK1EDrU6HqmP_zZtx56nM1GIMIRfyt49Rw
                sheets.SpreadSheetId = googleSpreadSheetId; // "1xJCuvFZj1UK1EDrU6HqmP_zZtx56nM1GIMIRfyt49Rw";

                var values1 = sheets.GetList("Página1!A1:O99999999");

                foreach (var row in values1)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    //
                    // Gravação no banco de dados
                    //

                    String first_name = StringUtilities.ToString(row[12]);
                    String phone_number = StringUtilities.ToString(row[13]);
                    String email = StringUtilities.ToString(row[14]);

                    ContatoBO contatoBO = new ContatoBO().FindByEmail(0, email, null);

                    if (contatoBO == null)
                    {
                        contatoBO = new ContatoBO();
                        contatoBO.GERAN_FILIAL = 0;
                        contatoBO.GERAN_CODIGO = 0;

                        contatoBO.GERAN_NOME = first_name;
                        contatoBO.GERAN_EMAIL = email;
                        contatoBO.GERAN_FONE = phone_number;
                        contatoBO.GERAN_STATUS = "00";

                        ret = contatoBO.Insert();
                    }
                }




                
        private int roletaUsuario()
        {

            DataTable usersDataTable = null;

            if (users.Count == 0)
            {
                String sql = "SELECT ";
                sql += "SYSAQ_NUMERO, SYSAQ_USUARIO, SYSAQ_PARTICIPA_ROLETA ";
                sql += "FROM sysaq "; //WHERE SYSAQ_NUMERO=1 AND SYSAQ_INATIVO=0
                sql += "ORDER BY SYSAQ_NUMERO DESC ";

                usersDataTable = DataAccessManager.GetInstance().ExecuteReader(sql, null);

                if (users.Count == 0)
                {
                    foreach (DataRow row in usersDataTable.Rows)
                    {
                        if (BooleanUtilities.parse(row["SYSAQ_PARTICIPA_ROLETA"]))
                        {
                            users.Push(NumberUtilities.parseInt(row["SYSAQ_NUMERO"]));
                        }
                    }
                }

                //if (userAnhanguera.Count == 0)
                //{
                //    foreach (DataRow row in usersDataTable.Rows)
                //    {
                //        if (BooleanUtilities.parse(row["SYSAQ_FUNIL_ANHANG"]))
                //        {
                //            userAnhanguera.Push(NumberUtilities.parseInt(row["SYSAQ_NUMERO"]));
                //        }
                //    }
                //}
            }

            //
            // Caso o usuario tenha sido informado na planilha, utilizamos ele, caso contrario pego da lista de usuarios
            //
            //int idUser = 0;

            //foreach (DataRow row in usersDataTable.Rows)
            //{
            //    if (StringUtilities.ToString(row["SYSAQ_USUARIO"]).Equals(responsavel))
            //    {
            //        idUser = NumberUtilities.parseInt(row["SYSAQ_NUMERO"]);

            //        break;
            //    }
            //}

            //if (idUser == 0)
            //{
            ////if (StringUtilities.toString(IES).Equals("Cruzeiro do Sul", StringComparison.OrdinalIgnoreCase))
            //if (funil.Equals("Cruzeiro do Sul", StringComparison.OrdinalIgnoreCase))
            //{
            //    idUser = userCruzeiro.Pop();
            //}
            ////else if (StringUtilities.toString(IES).Equals("Anhanguera", StringComparison.OrdinalIgnoreCase))
            //else if (funil.Equals("Anhanguera", StringComparison.OrdinalIgnoreCase))
            //{
            //    idUser = userAnhanguera.Pop();
            //}
            //}

            return users.Pop();
        }








                //public void Sync1()
        //{
        //    Object ret = null;

        //    if (ThreadIsRunning1)
        //    {
        //        return;
        //    }

        //    ThreadIsRunning1 = true;

        //    LogApp("Sync1 iniciado...");

        //    try
        //    {
        //        MoskitClient moskitClient = new MoskitClient();
        //        moskitClient.Token = moskitApiKey;

        //        DealService dealService = new DealService(moskitClient);

        //        ContactService contactService = new ContactService(moskitClient);

        //        //
        //        // Vamos monitorar todos os leads novos em aberto, caso nao ocorra uma interacao com o lead 
        //        // em 3 horas iremos trocar de responsavel utilizando a planilha de Cidade x Responsavel
        //        // caso nao encontre um responsavel em Cidade x Reponsavel iremos distribuir entre a equipe digital
        //        //

        //        DataTable dataTable = new IntegracaoBO().GetList(0, "00");

        //        foreach (DataRow dataRow in dataTable.Rows)
        //        {
        //            string id = StringUtilities.ToString(dataRow["USUAN_ID"]);

        //            IntegracaoBO integracaoBO = new IntegracaoBO().Find(0, id);

        //            JObject eventRaw = JObject.Parse(StringUtilities.ToString(dataRow["USUAN_EVENTO"]));

        //            JObject dealData = JObject.Parse(StringUtilities.ToString(eventRaw["data"]));

        //            int idDeal = NumberUtilities.parseInt(dealData["id"]);

        //            //
        //            // Busco o lead no moskit
        //            //
        //            //Deal deal = (Deal)dealService.Get(idDeal);
        //            dealData = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idDeal}", "GET", null, moskitApiKey);

        //            //
        //            // Vamos verificar quanto tempo esta inativo o lead
        //            // Para isso vamos checar se existe alguma atividade criada e se ela esta atrasa a mais de 3hs
        //            // Caso não tenha uma atividade verifica o proprio lead
        //            //
        //            //String idActivity = "";
        //            DateTime dataAtual = DateTime.Now;
        //            DateTime? dataHoraInteracao = DateUtilities.Parse(dealData["nextActivityPending"]);

        //            if (dataHoraInteracao.HasValue)
        //            {
        //                dataHoraInteracao = DateUtilities.Parse(dealData["lastInteraction"]);
        //            }

        //            int tempoDecorrido = (int)(dataAtual - dataHoraInteracao.Value).TotalHours;

        //            //
        //            // Se ja passaram mais de 3 hrs desde a ultima interacao vamos 
        //            // alterar o responsavel do lead
        //            //
        //            if (tempoDecorrido >= 0)
        //            {

        //                //
        //                // Busca campos customizados Cidade e Estado para seu usado na rotela
        //                //
        //                var customFields = (JArray)dealData["entityCustomFields"];

        //                String cidadeDeal = "";

        //                String estadoDeal = "";

        //                String cidadeEstadoDeal = "";

        //                foreach (var field in (customFields))
        //                {
        //                    String optionsFieldId = StringUtilities.ToString(field["id"]);

        //                    if (optionsFieldId.Equals("CF_0WGqoEiKC9ae2mnP"))
        //                    {
        //                        cidadeDeal = StringUtilities.ToString(field["textValue"]);
        //                    }
        //                    else if (optionsFieldId.Equals("CF_oJZmP1iKCGQxRDgv"))
        //                    {
        //                        estadoDeal = StringUtilities.ToString(field["textValue"]);
        //                    }

        //                }

        //                cidadeEstadoDeal = cidadeDeal + " - " + estadoDeal;

        //                string status = "10";
        //                string motivo = "Processado com sucesso";

        //                string equipe = "";

        //                int usuarioId = roletaUsuario(null, cidadeEstadoDeal);

        //                if (usuarioId != 0)
        //                {     
        //                    List<Object> contactsList = new List<Object>(dealData["contacts"]);

        //                    int idContact = 0;

        //                    foreach (JObject item in contactsList)
        //                    {
        //                        idContact = NumberUtilities.parseInt(item["id"]);

        //                        JObject contact = (JObject)SendCommand($"https://api.moskitcrm.com/v2/contacts/{idContact}", "GET", null, moskitApiKey);

        //                        contact["dateCreated"] = DateUtilities.Parse(contact["dateCreated"]).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();

        //                        if (dealData["lastAssignedDate"] != null)
        //                        {
        //                            contact["lastAssignedDate"] = DateUtilities.Parse(contact["lastAssignedDate"]).AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                        }
        //                        if (dealData["lastInteraction"] != null)
        //                        {
        //                            contact["lastInteraction"] = DateUtilities.Parse(contact["lastInteraction"]).AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                        }

        //                        contact["responsible"]["id"] = usuarioId;

        //                        contact = (JObject)SendCommand($"https://api.moskitcrm.com/v2/contacts/{idContact}", "PUT", contact, moskitApiKey);
        //                    }

        //                    //
        //                    // Alteramos o responsavel do negocio
        //                    //                           
        //                    dealData["dateCreated"] = DateUtilities.Parse(dealData["dateCreated"]).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();

        //                    if (dealData["lastAssignedDate"] != null)
        //                    {
        //                        dealData["lastAssignedDate"] = DateUtilities.Parse(dealData["lastAssignedDate"]).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                    }
        //                    if (dealData["lastInteraction"] != null)
        //                    {
        //                        dealData["lastInteraction"] = DateUtilities.Parse(dealData["lastInteraction"]).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                    }
        //                    if (dealData["lastChangeStage"] != null)
        //                    {
        //                        dealData["lastChangeStage"] = DateUtilities.Parse(dealData["lastChangeStage"]).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                    }
        //                    if (dealData["lastActivityDone"] != null)
        //                    {
        //                        dealData["lastActivityDone"] = DateUtilities.Parse(dealData["lastActivityDone"]).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                    }

        //                    dealData["responsible"]["id"] = usuarioId;

        //                    dealData = (JObject)SendCommand($"https://api.moskitcrm.com/v2/deals/{idDeal}", "PUT", dealData, moskitApiKey);


        //                    //
        //                    // Caso exista uma atividade pendende devemos mudar o responsavel dela
        //                    // e caso não exista devemos cria-la
        //                    //
        //                    string activity = StringUtilities.ToString(dealData["activities"]);
        //                    //activity = activity.Replace("[","").Replace("]","");

        //                    if (!String.IsNullOrEmpty(activity) || dealData["activities"] != null)
        //                    {
        //                        int index = JArray.Parse(activity).Count - 1;

        //                        //int qtd = JArray.Parse(dealData["activities"].ToString()).Count - 1;
        //                        int idActivity = NumberUtilities.parseInt(dealData["activities"][index]["id"]);

        //                        JObject activities = (JObject)SendCommand($"https://api.moskitcrm.com/v2/activities/{idActivity}", "GET", null, moskitApiKey);

        //                        if (dealData["doneDate"] != null)
        //                        {
        //                            activities["dateCreated"] = DateUtilities.Parse(activities["dateCreated"]).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                            activities["dueDate"] = DateUtilities.Parse(activities["dueDate"]).AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();

        //                            //if (dealData["doneDate"] != null)
        //                            //{
        //                            //    activities["doneDate"] = DateUtilities.Parse(activities["doneDate"]).AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                            //}
        //                            activities["responsible"]["id"] = usuarioId;

        //                            activities = (JObject)SendCommand($"https://api.moskitcrm.com/v2/activities/{idActivity}", "PUT", activities, moskitApiKey);
        //                        }
        //                    }
        //                    else
        //                    {

        //                        String dataCreated = dataAtual.ToString("yyyy-MM-ddTHH:mm:ss") + ".000" + DateUtilities.getUTC();
        //                        //
        //                        // Cria a nova atividade caso ela não exista no lead
        //                        //
        //                        JObject activities = new JObject(
        //                        new JProperty("title", StringUtilities.ToString(dealData["name"])),
        //                        new JProperty("dateCreated", dataCreated),
        //                        new JProperty("dueDate", dataCreated),
        //                        new JProperty("deals", new JArray(
        //                            new JObject(
        //                                new JProperty("entity", "Deal"),
        //                                new JProperty("id", idDeal)
        //                            ))
        //                        ),
        //                        new JProperty("type", new JObject(
        //                            new JProperty("id", 79343)
        //                        )),
        //                        new JProperty("responsible", new JObject(
        //                            new JProperty("id", usuarioId)
        //                        )),
        //                        new JProperty("createdBy", new JObject(
        //                            new JProperty("id", usuarioId)
        //                        ))
        //                        );

        //                        if (idContact > 0)
        //                        {
        //                            activities.Add(new JProperty("contacts"));
        //                            ((JArray)activities["contacts"]).Add(new JObject(
        //                                   new JProperty("id", idContact)
        //                           ));
        //                        }

        //                        activities = (JObject)SendCommand("https://api.moskitcrm.com/v2/activities/", "POST", activities, moskitApiKey);

        //                    }

        //                    status = "10";
        //                    motivo = "Processado com sucesso "; // + serviceReturn.Message.ToString();

        //                }
        //                else
        //                {
        //                    status = "00";
        //                    motivo = $"Falha ao processar o evento, usuario: {usuarioId}, equipe: {equipe} ";
        //                }

        //                ret = integracaoBO.UpdateFields(new string[] { "USUAN_STATUS", "USUAN_STATUS_MOTIVO" }, new object[] { status, motivo });

        //                if (ret != null)
        //                {
        //                    throw new KException(ret.ToString());
        //                }

        //                //
        //                // Caso tenha chego ate aqui com o status 00, algo deu errado,
        //                // vamos voltar o usuario da roleta
        //                //
        //                if (status.Equals("00"))
        //                {
        //                    ret = DataAccessManager.ExecuteNonQuery($"UPDATE sysaq SET SYSAQ_USADO_ROLETA = 0 WHERE SYSAQ_NUMERO = {usuarioId}", null);

        //                    if (ret != null)
        //                    {
        //                        throw new KException(ret.ToString());
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogApp(ex);
        //    }

        //    LogApp("Sync1 finalizado...");

        //    ThreadIsRunning1 = false;
        //}










                        //int filial = 0;

                //byte[] bytes = null;
                //HttpWebRequest request = null;
                //HttpWebResponse response = null;

                //int quantity = 50;
                //int start = -1;
                //int registro = 1;

                ////
                //// Busca usuário
                ////
                ////moskitApiKey = "b2dd8a0e-a4a6-47d1-8306-00658b85d7fe";
                //do
                //{
                //    JArray users = (JArray)SendCommand($"https://api.moskitcrm.com/v2/users?start={start + registro}&quantity={quantity}", "GET", null, moskitApiKey);

                //    if (users.Count == 0)
                //    {
                //        registro = 1;
                //    }

                //    foreach (JObject user in users)
                //    {
                //        if (!IsRunning)
                //        {
                //            return;
                //        }

                //        String usuarioId = StringUtilities.ToString(user["id"]);
                //        String usuario = StringUtilities.ToString(user["name"]);
                //        string email = StringUtilities.ToString(user["username"]);
                //        string active = StringUtilities.ToString(user["active"]);

                //        int equipeId = NumberUtilities.parseInt(user["team"]["id"]);

                //        JObject equipe = (JObject)SendCommand($"https://api.moskitcrm.com/v2/teams/{equipeId}", "GET", null, moskitApiKey);

                //        String equipeName = StringUtilities.ToString(equipe["name"]);

                //        UsuarioBO usuarioBO = new UsuarioBO().FindByNumero(0, usuarioId, null);

                //        if (usuarioBO != null)
                //        {
                //            ret = usuarioBO.UpdateFields(new String[] { "SYSAQ_USUARIO", "SYSAQ_EMAIL", "SYSAQ_INATIVO" }, new Object[] { usuario, email, !Convert.ToBoolean(active) });

                //            if (ret != null)
                //            {
                //                break;
                //            }
                //        }
                //        else
                //        {
                //            usuarioBO = new UsuarioBO();
                //            usuarioBO.SYSAQ_FILIAL_COD = 0;
                //            usuarioBO.SYSAQ_CODIGO = 0;
                //            usuarioBO.SYSAQ_NUMERO = usuarioId;
                //            usuarioBO.SYSAQ_USUARIO = usuario;
                //            usuarioBO.SYSAQ_EQUIPE_ID = equipeId;
                //            usuarioBO.SYSAQ_EQUIPE_NAME = equipeName;
                //            usuarioBO.SYSAQ_EMAIL = email;
                //            usuarioBO.SYSAQ_PARTICIPA_ROLETA = false;
                //            usuarioBO.SYSAQ_USADO_ROLETA = false;
                //            usuarioBO.SYSAQ_INATIVO = !Convert.ToBoolean(active);
                //            usuarioBO.SYSAQ_INTEGRADO = false;


                //            //if (active.Equals("True"))
                //            //{
                //            //    usuarioBO.SYSAQ_INATIVO = false;
                //            //}

                //            ret = usuarioBO.Insert();
                //        }

                //        if (ret != null)
                //        {
                //            break;
                //        }

                //        registro++;
                //    }

                //    if (ret != null)
                //    {
                //        throw new Exception(StringUtilities.ToString(ret));
                //    }

                //} while (true);












                        //public ServiceReturn Put(ValueObject vo)
        //{
        //    ServiceReturn ret = new ServiceReturn();

        //    ////try
        //    ////{
        //    ////    Contact contact = (Contact)vo;

        //    //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //    ////    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{client.ApiUri}contacts/{contact.Id}");
        //    ////    request.Headers.Add("apikey", client.Token);
        //    ////    request.Method = "PUT";
        //    ////    request.ContentType = "application/json";

        //    ////    //MessageBox.Show(request.RequestUri.ToString());
        //    ////    //MessageBox.Show(request.Method);
        //    ////    //MessageBox.Show(request.ContentType);

        //    ////    String json = JsonConvert.SerializeObject(contact);

        //    ////    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        //    ////    {
        //    ////        streamWriter.Write(json);
        //    ////    }

        //    ////    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        //    ////    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
        //    ////    {
        //    ////        throw new Exception(response.StatusDescription);
        //    ////    }

        //    ////    json = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //    ////}
        //    ////catch (WebException e)
        //    ////{
        //    ////    if (e.Response != null)
        //    ////    {
        //    ////        using (var errorResponse = (HttpWebResponse)e.Response)
        //    ////        {
        //    ////            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
        //    ////            {
        //    ////                String resp = reader.ReadToEnd();

        //    ////                JObject error = JObject.Parse(resp);

        //    ////                ret.Status = 500;
        //    ////                ret.Message = StringUtilities.ToString(error["errors"][0]);
        //    ////            }
        //    ////        }
        //    ////    }
        //    ////}

        //    return ret;
        //}

        //public ServiceReturn Delete(object vo)
        //{
        //    ServiceReturn ret = new ServiceReturn();

        //    return ret;
        //}

        //public ValueObject Get(object id)
        //{
        //    Group group = null;

        //    //try
        //    //{


        //    //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{client.ApiUri}contacts/{id}");
        //    //    request.Headers.Add("apikey", client.Token);
        //    //    request.Method = "GET";
        //    //    request.ContentType = "application/json";

        //    //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        //    //    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
        //    //    {
        //    //        throw new Exception(response.StatusDescription);
        //    //    }

        //    //    String json = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //    //    contact = JsonConvert.DeserializeObject<Contact>(json);
        //    //}
        //    //catch (WebException e)
        //    //{
        //    //    if (e.Response != null)
        //    //    {
        //    //        using (var errorResponse = (HttpWebResponse)e.Response)
        //    //        {
        //    //            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
        //    //            {
        //    //                String resp = reader.ReadToEnd();

        //    //                JObject error = JObject.Parse(resp);
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    return group;
        //}

        //public IEnumerable<ValueObject> GetList()
        //{
        //    //String json = null;

        //    ////try
        //    ////{
        //    //int start = -1;
        //    //int quantity = 50;
        //    //int record = 1;

        //    //do
        //    //{
        //        //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //    //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{client.ApiUri}users?start={start + record}&quantity={quantity}");
        //    //    request.Headers.Add("apikey", client.Token);
        //    //    request.Method = "GET";
        //    //    request.ContentType = "application/json";

        //    //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        //    //    //if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
        //    //    //{
        //    //    //    throw new Exception(response.StatusDescription);
        //    //    //}

        //    //    json = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //    //    //contact = JsonConvert.DeserializeObject<Contact>(json);

        //    //    JArray users = JArray.Parse(json);

        //    //    foreach (JObject obj in users)
        //    //    {
        //    //        Thread.Sleep(1000);

        //    //        User user = obj.ToObject<User>();

        //    //        TeamService teamService = new TeamService(this.client);
        //    //        //user.Team.Id
        //    //        user.Team = (Team)teamService.Get(user.Team.Id);

        //    //        record++;

        //    //        yield return user;
        //    //    }

        //    //    if (users.Count == 0)
        //    //    {
        //    //        break;
        //    //    }

        //    //} while (true);
        //    ////}
        //    ////catch (WebException e)
        //    ////{
        //    ////    serviceReturn.Status = 500;

        //    ////    if (e.Response != null)
        //    ////    {
        //    ////        using (var errorResponse = (HttpWebResponse)e.Response)
        //    ////        {
        //    ////            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
        //    ////            {
        //    ////                String resp = reader.ReadToEnd();

        //    ////                serviceReturn.Message = resp;
        //    ////            }
        //    ////        }
        //    ////    }
        //    ////    else
        //    ////    {

        //    ////    }
        //    ////}

        //    ////
        //    ////
        //    ////

        //    throw new NotImplementedException();
        //}

        ////public ServiceReturn Active(object mail)
        ////{
        ////    ServiceReturn serviceReturn = new ServiceReturn();

        ////    try
        ////    {
        ////        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        ////        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{client.ApiUri}v2/vendedores/desbloquear?email={mail}");
        ////        request.Headers.Add("token_exact", client.Token);
        ////        request.Method = "PUT";
        ////        //request.ContentType = "application/json";

        ////        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        ////        string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

        ////        serviceReturn.Message = json;
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        String error = e.StackTrace;

        ////        serviceReturn.Status = 500;
        ////        serviceReturn.Message = error;

        ////        if (e is WebException)
        ////        {
        ////            if (((WebException)e).Response != null)
        ////            {
        ////                using (var errorResponse = (HttpWebResponse)((WebException)e).Response)
        ////                {
        ////                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
        ////                    {
        ////                        String resp = reader.ReadToEnd();

        ////                        serviceReturn.Message = resp;
        ////                    }
        ////                }
        ////            }
        ////        }
        ////    }

        ////    return serviceReturn;
        ////}

        ////public ServiceReturn Inactive(object mail)
        ////{
        ////    ServiceReturn serviceReturn = new ServiceReturn();

        ////    try
        ////    {
        ////        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        ////        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{client.ApiUri}v2/vendedores/bloquear?email={mail}");
        ////        request.Headers.Add("token_exact", client.Token);
        ////        request.Method = "PUT";
        ////        //request.ContentType = "application/json";

        ////        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        ////        string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

        ////        serviceReturn.Message = json;
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        String error = e.StackTrace;

        ////        serviceReturn.Status = 500;
        ////        serviceReturn.Message = error;

        ////        if (e is WebException)
        ////        {
        ////            if (((WebException)e).Response != null)
        ////            {
        ////                using (var errorResponse = (HttpWebResponse)((WebException)e).Response)
        ////                {
        ////                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
        ////                    {
        ////                        String resp = reader.ReadToEnd();

        ////                        serviceReturn.Message = resp;
        ////                    }
        ////                }
        ////            }
        ////        }
        ////    }

        ////    return serviceReturn;
        ////}


         //foreach (var message in messages)
                                        //{
                                        //if (!string.IsNullOrEmpty(mensagem))
                                        //{
                                        //    mensagem = mensagem + "\n" + StringUtilities.ToString(message["message"]);
                                        //}
                                        //else
                                        //{
                                        //    mensagem = StringUtilities.ToString(message["message"]);
                                        //}

                                        //DateTime? dataConversa = DateUtilities.parse(message["date"], "dd/MM/yyyy HH:mm:ss");
                                        //string fromName = StringUtilities.ToString(message["fromName"]);

                                        //var result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                            //var url = "https://pontotrack.ipsolutiontelecom.com.br:5001/external/getAtendimentos";

                            //var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                            //httpRequest.Method = "POST";

                            //httpRequest.Accept = "application/json";
                            //httpRequest.Headers["Authorization"] = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6NDcsImxvZ2luIjoiaXBzb2x1dGlvbiIsImVtYWlsIjoiIiwiY2hhbm5lbCI6e30sImlhdCI6MTY0NDQ0OTQwMCwiZXhwIjoxNjc1OTg1NDAwfQ.r4bfbt-D-T79HuCYT1Dkt512VbzieLEdnSMclDwUA50";
                            ////httpRequest.ContentType = "application/json";

                            //var data = @"{
                            //                ""number"": ""5543999444182"",
                            //                ""instanceid"": ""12""
                            //                }";

                            //using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                            //{
                            //    streamWriter.Write(data);
                            //}

                            //var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                            //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            //{
                            //    var results = streamReader.ReadToEnd();

                            //    //JArray de = (JArray)results["atendimentos"][0];

                            //    foreach (var result in results)
                            //    {
                            //        string agente = "";
                            //    }

                            //    ////JArray atendimentos = (JArray)result["atendimentos"];
                            //    //string customFields = StringUtilities.ToString(result["atendimentos"]);

                            //    //string agente = StringUtilities.ToString(customFields.SelectToken("[?(@.id == 'CF_rpGmBPioCRO4AqeR')]")["textValue"]);
                            //}



                            //Console.WriteLine(httpResponse.StatusCode);