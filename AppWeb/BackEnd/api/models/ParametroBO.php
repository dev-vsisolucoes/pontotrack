<?php

class ParametroBO extends ParametroModel {

    public function getAutoIncrement($store, $table) {
        return CustomUtilities::getAutoIncrement($store, $table);
    }

    public function validate() {
        return parent::validate();
    }

}
