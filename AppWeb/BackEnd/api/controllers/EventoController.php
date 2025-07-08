<?php

/**
 * @Controller=evento
 * @Description=
 */
class EventoController extends ApiBaseController {

    public function __construct($data, $model) {
        parent::__construct($data, $model);
    }

    /**
     * @POST=/evento/negocio-ganho
     * @Description=
     */
    public function webhook1() {

        ManagerBO::beginTransaction();

        $evento = $this->Request["raw"];

//        $obj = json_decode($evento);

        $eventoBO = new EventoBO();
        $eventoBO->tipo = 0;
        $eventoBO->data = DateUtilities::getDateTime();
        $eventoBO->evento = $evento;
        $eventoBO->status = "00";
        $eventoBO->statusMotivo = "Aguardando processamento";

        $ret = ManagerBO::add($eventoBO);

        if ($ret != null) {
            $this->Response->Code = 500;
            $this->Response->Message = $ret;

            ManagerBO::rollBackTransaction();
        } else {
            ManagerBO::commitTransaction();
        }
    }

    /**
     * @POST=/evento/whats
     * @Description=
     */
    public function webhook2() {

        //ManagerBO::beginTransaction();

        //$evento = $this->Request["raw"];

        //$eventoBO = new EventoBO();
        //$eventoBO->tipo = 1;
        //$eventoBO->data = DateUtilities::getDateTime();
        //$eventoBO->evento = $evento;
        //$eventoBO->status = "00";
        //$eventoBO->statusMotivo = "Aguardando processamento";

        //$ret = ManagerBO::add($eventoBO);
		//
        //if ($ret != null) {
        //    $this->Response->Code = 500;
        //    $this->Response->Message = $ret;
		//
        //    ManagerBO::rollBackTransaction();
        //} else {
        //    ManagerBO::commitTransaction();
        //}
    }    
}