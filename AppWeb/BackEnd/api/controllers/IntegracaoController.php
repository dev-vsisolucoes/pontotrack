<?php

/**
 * @Controller=integracao
 * @Description=
 */
class IntegracaoController extends ApiBaseController {

    public function __construct($data, $model) {
        parent::__construct($data, $model);
    }
    
    /**
     * @POST=/integracao/negocio
     * @Description=
     */
    public function webhook1() {

//        ManagerBO::beginTransaction();

        $evento = $this->Request["raw"];

//        $obj = json_decode($evento);

        $integracaoBO = new IntegracaoBO();
//        $integracaoBO->id = null;
//        $integracaoBO->filial = 0;
//        $integracaoBO->codigo = 0;
        $integracaoBO->tipo = 0;
        $integracaoBO->data = DateUtilities::getDateTime();
        $integracaoBO->evento = $evento;
        $integracaoBO->status = "00";
        $integracaoBO->statusMotivo = "Aguardando processamento";

        $ret = ManagerBO::add($integracaoBO);

        if ($ret != null) {
            $this->Response->Code = 500;
            $this->Response->Message = $ret;
//
//            ManagerBO::rollBackTransaction();
            
        } else {
//            ManagerBO::commitTransaction();
        }
        
    }   
    
    /**
     * @POST=/integracao/fase-alterada
     * @Description=
     */
    public function webhook2() {
        $evento = $this->Request["raw"];

        $integracaoBO = new IntegracaoBO();
        $integracaoBO->tipo = 1;
        $integracaoBO->data = DateUtilities::getDateTime();
        $integracaoBO->evento = $evento;
        $integracaoBO->status = "00";
        $integracaoBO->statusMotivo = "Aguardando processamento";
        
        $ret = ManagerBO::add($integracaoBO);

        if ($ret != null) {
            $this->Response->Code = 500;
            $this->Response->Message = $ret;
            
        } else {
        }
        
    }
   
}

