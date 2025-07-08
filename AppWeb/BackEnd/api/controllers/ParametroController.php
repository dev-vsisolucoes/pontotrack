<?php

/**
 * @Controller=parametro
 * @Description=Mantem informações sobre as configurações do sistema
 */
class ParametroController extends ApiBaseController {

    public function __construct($data, $model) {
        parent::__construct($data, $model);
    }

    /**
     * @POST=/parametro
     * @Description=Incluir um novo parâmetro
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
     * @PUT=/parametro
     * @Description=Altera um parâmetro
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
     * @DELETE=/parametro
     * @Description=Excluir um parâmetro
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
     * @GET=/parametro/find
     * @Description=Busca por um parâmetro
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
     * @GET=/parametro
     * @Description=Lista todos os parâmetro
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
        $expression .= " WHERE 1=1 ";

        //
        // filtro
        //
        $filter = $this->Request['filter'];

        if (!StringUtilities::isEmptyOrNull($filter)) {

            //$expression .= " AND " . $filter;

            $fields = "CONVERT(CONCAT_WS(SYSAH_FILIAL_COD, SYSAH_CODIGO, SYSAH_NOME, SYSAH_VALOR, SYSAH_DESCRICAO), CHAR)";

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

            $fields = "CONVERT(CONCAT_WS(SYSAH_FILIAL_COD, SYSAH_CODIGO, SYSAH_NOME, SYSAH_VALOR, SYSAH_DESCRICAO), CHAR)";

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
}

