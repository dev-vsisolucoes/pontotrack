<?php

class CustomUtilities {

    public static function getAutoIncrement($store, $tableName) {
        
        if($store == null) {
            $store = 0;
        }
        
        $command = "SELECT SYSAB_VALOR FROM sysab WHERE SYSAB_FILIAL=:SYSAB_FILIAL AND SYSAB_CODIGO=:SYSAB_CODIGO";

        $statement = ManagerConnection::getInstance()->prepare($command);

        $statement->bindValue(":SYSAB_FILIAL", $store);
        $statement->bindValue(":SYSAB_CODIGO", $tableName);

        $statement->execute();

        $row = $statement->fetchAll(PDO::FETCH_ASSOC);

        $ret = 1;

        if (count($row) == 0) {

            $command = "INSERT INTO sysab(SYSAB_FILIAL, SYSAB_CODIGO, SYSAB_VALOR) VALUES(:SYSAB_FILIAL, :SYSAB_CODIGO, :SYSAB_VALOR)";
        } else {

            $ret = $row[0]["SYSAB_VALOR"] + 1;

            $command = "UPDATE sysab SET SYSAB_VALOR=:SYSAB_VALOR WHERE SYSAB_FILIAL=:SYSAB_FILIAL AND SYSAB_CODIGO=:SYSAB_CODIGO";
        }

        $statement = ManagerConnection::getInstance()->prepare($command);

        $statement->bindValue(":SYSAB_FILIAL", $store);
        $statement->bindValue(":SYSAB_CODIGO", $tableName);
        $statement->bindValue(":SYSAB_VALOR", $ret);

        $statement->execute();

        return $ret;
    }

    public static function getParameterValue($parameterName) {

        $ret = null;

        $parameterBO = ManagerBO::findByExpression(new ParametroBO(), "SYSAH_NOME='$parameterName'");

        if ($parameterBO == null) {
            
        } else {
            $ret = $parameterBO->SYSAH_VALOR;
        }

        return $ret;
    }

}
