<?php

class EventoBO extends EventoModel {

    public function getAutoIncrement($store, $table) {
        return CustomUtilities::getAutoIncrement($store, $table);
    }

    public function validate() {
        return parent::validate();
    }

}
