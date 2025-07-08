<?php

/**
 * @Controller=usuario
 * @Description=Mantem informações sobre os usuários do sistema
 */
class UsuarioController extends ApiBaseController {

    public function __construct($data, $model) {
        parent::__construct($data, $model);
    }

    /**
     * @POST=/usuario
     * @Description=
     */
    public function add() {

        ManagerBO::beginTransaction();

        $ret = ManagerBO::add($this->Model);

        if ($ret == null) {
            
        }

        if ($ret != null) {
            $this->Response->Code = 500;
            $this->Response->Message = $ret;

            ManagerBO::rollBackTransaction();
        } else {
            ManagerBO::commitTransaction();

            $this->Response->Message = "Ação executada com sucesso";
        }
    }

    /**
     * @PUT=/usuario
     * @Description=
     */
    public function update() {

        ManagerBO::beginTransaction();

        $ret = ManagerBO::update($this->Model);

        if ($ret == null) {
            
        }

        if ($ret != null) {
            $this->Response->Code = 500;
            $this->Response->Message = $ret;

            ManagerBO::rollBackTransaction();
        } else {
            ManagerBO::commitTransaction();

            $this->Response->Message = "Ação executada com sucesso";
        }
    }

    /**
     * @DELETE=/usuario
     * @Description=
     */
    public function delete() {

        ManagerBO::beginTransaction();

        $ret = ManagerBO::delete($this->Model);

        if ($ret == null) {
            
        }

        if ($ret != null) {
            $this->Response->Code = 500;
            $this->Response->Message = $ret;

            ManagerBO::rollBackTransaction();
        } else {
            ManagerBO::commitTransaction();

            $this->Response->Message = "Ação executada com sucesso";
        }
    }

    /**
     * @GET=/usuario/find
     * @Description=
     */
    public function find() {

        $ret = ManagerBO::find($this->Model);

        if ($ret == null) {
            $this->Response->Code = 500;
            $this->Response->Message = "Registro não encontrado";
            
        } else {
            $this->Response->Content = $ret;
        }
    }

    /**
     * @GET=/usuario
     * @Description=
     */
    public function enum() {

       //
        // Parte 1, obtenho a paginacao
        //
        $limit = $this->Request['limit'];

        if ($limit == null) {
            $limit = 9999999;
        }

        $page = $this->Request['page'];

        if ($page == null) {
            $page = 0;
        }

        if ($page < 0) {
            $page = 0;
        }

        $page = $page * $limit;

        $expression = "SELECT COUNT(*) AS NUM_RECORDS ";
        $expression .= "FROM " . ManagerBO::getTableName($this->Model);
//        $expression .= " INNER JOIN esao on PRAT_PRODUTO_COD =  ESAO_COD_PRO ";
        $expression .= " WHERE 1=1 ";

        //
        // filtro
        //
        $filter = $this->Request['filter'];

        if (!StringUtilities::isEmptyOrNull($filter)) {

            //$expression .= " AND " . $filter;

            $fields = "CONVERT(CONCAT_WS(SYSAQ_FILIAL_COD, SYSAQ_CODIGO, SYSAQ_NUMERO, SYSAQ_AGENTE_ID, SYSAQ_EQUIPE_NAME, SYSAQ_USUARIO, SYSAQ_EMAIL,SYSAQ_SENHA), CHAR)";

            $searchText = explode(" ", $filter);

            $filter = "";

            foreach ($searchText as $text) {
                $filter .= $fields . " LIKE '%" . $text . "%' AND ";
            }

            $filter = substr($filter, 0, strlen($filter) - 5);

            $expression .= " AND " . $filter;
        }

        $ret = ManagerBO::getListBySelect($this->Model, $expression);

        $totalRows = $ret[0]["NUM_RECORDS"];

        $totalPages = ceil($totalRows / $limit);

        //
        // Parte 2, a consulta em si
        //

        $expression = "SELECT * ";
        $expression .= "FROM " . ManagerBO::getTableName($this->Model);
//        $expression .= " INNER JOIN esao on PRAT_PRODUTO_COD =  ESAO_COD_PRO ";
        $expression .= " WHERE 1=1 ";

        //
        // filtro
        //
        $filter = $this->Request['filter'];

        if (!StringUtilities::isEmptyOrNull($filter)) {

            //$expression .= " AND " . $filter;

            if ($totalPages < $page) {
                $page = 0;
            }

            $fields = "CONVERT(CONCAT_WS(SYSAQ_FILIAL_COD, SYSAQ_CODIGO, SYSAQ_NUMERO, SYSAQ_AGENTE_ID, SYSAQ_EQUIPE_NAME, SYSAQ_USUARIO,SYSAQ_EMAIL, SYSAQ_SENHA), CHAR)";

            $searchText = explode(" ", $filter);

            $filter = "";

            foreach ($searchText as $text) {
                $filter .= $fields . " LIKE '%" . $text . "%' AND ";
            }

            $filter = substr($filter, 0, strlen($filter) - 5);

            $expression .= " AND " . $filter;
        }

        $order = $this->Request['order'];

        if (!StringUtilities::isEmptyOrNull($order)) {
            $expression .= " ORDER BY " . $order;
        }

        $expression .= " LIMIT $page, $limit";

        $ret = ManagerBO::getListBySelect($this->Model, $expression);

        if (!is_array($ret)) {
            $this->Response->Code = 500;
            $this->Response->Message = "Registro não encontrado";
            
        } else {
            $this->Response->Content = array("totalRows" => $totalRows, "totalPages" => $totalPages, "rows" => $ret);
        }
    }

    /**
     * @POST=/usuario/login
     * @Description=
     */
    public function login() {

        $env = $this->Request["modulo"];
        $filial = $this->Request["empresa"];

        $ret = ManagerBO::findByExpression($this->Model, "SYSAQ_USUARIO='" . $this->Model->usuario . "' AND SYSAQ_SENHA='" . $this->Model->senha . "' AND SYSAQ_FILIAL_COD='" . $filial . "'");

        if ($ret == null) {
            $this->Response->Code = 500;
            $this->Response->Message = "Usuário ou senha invalidos";
        } else if ($ret->inativo === "1") {
            $this->Response->Code = 500;
            $this->Response->Message = "Usuário informado está inativo";
        } else if ($ret->ambiente != null && stripos($ret->ambiente, $env) === false) {
            $this->Response->Code = 500;
            $this->Response->Message = "Usuário sem permissão para acessar o módulo";
        } else {
            $_SESSION["STORE"] = $filial;
            $_SESSION["USER"] = serialize($ret);
        }
    }

    /**
     * @GET=/usuario/logout
     * @Description=
     */
    public function logout() {
        session_destroy();
    }

}

