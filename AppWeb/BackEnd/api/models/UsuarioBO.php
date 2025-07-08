<?php

class UsuarioBO extends UsuarioModel {

    public function getAutoIncrement($store, $table) {
        return CustomUtilities::getAutoIncrement($store, $table);
    }

    public function validate() {
        return parent::validate();
    }

}
