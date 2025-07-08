<?php

/**
 * @Controller=empresa
 * @Description=Mantem informações sobre as filiais do sistema
 */
class EmpresaController extends ApiBaseController {

    public function __construct($data, $model) {
        parent::__construct($data, $model);
    }
    
    /**
     * @POST=/empresa
     * @Description=Inclue uma nova empresa
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
        }

    }

    /**
     * @PUT=/empresa
     * @Description=Altera uma empresa
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
        }

    }

    /**
     * @DELETE=/empresa
     * @Description=Apaga uma empresa
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
        }

    }

    /**
     * @GET=/empresa/find
     * @Description=Busca por uma empresa
     */
    public function find() {
        
        $ret = ManagerBO::find($this->Model);

        if($ret == null) {
            $this->Response->Code = 500;
            $this->Response->Message = "Registro não encontrado";
            
        } else {
            $this->Response->Content = $ret;
        }
    }

    /**
     * @GET=/empresa
     * @Description=Lista todas as empresas
     */
    public function enum() {

//        $filter = $this->Request['filter'];
//        $order = $this->Request['order'];
//        
//        $expression = "SELECT * ";
//        $expression .= "FROM " . ManagerBO::getTableName($this->Model);
//        $expression .= " WHERE 1=1 ";
//
//        if (!StringUtilities::isEmptyOrNull($filter)) {
//            $expression .= " AND " . $filter;
//        }
//
//        if (!StringUtilities::isEmptyOrNull($order)) {
//            $expression .= " ORDER BY " . $order;
//        }
//        
//        $ret = ManagerBO::getListBySelect($expression);
        
        $ret = ManagerBO::getList($this->Model, null);
        
        if(!is_array($ret)) {
            $this->Response->Code = 500;
            $this->Response->Message = "Registro não encontrado";
            
        } else {
            $this->Response->Content = $ret;
        }
    }
}
