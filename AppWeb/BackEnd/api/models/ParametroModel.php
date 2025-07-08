<?php

/**
 * @TableName=sysah
 */
class ParametroModel extends ValueObjectBase {

    /**
     * @ColumnName=SYSAH_ID
     * @Description=Id
     * @Type=varchar
     * @Size=36
     * @IsRequired
     * @IsKey
     * @IsUUID
     */
    public $id;

    /**
     * @ColumnName=SYSAH_FILIAL_COD
     * @Description=Id
     * @Type=varchar
     * @Size=10
     * @IsStoreField
     */
    public $filial;

    /**
     * @ColumnName=SYSAH_CODIGO
     * @Description=Número
     * @Type=int
     * @Size=11
     * @IsRequired
     * @IsAutoIncrement 
     */
    public $codigo;

    /**
     * @ColumnName=SYSAH_NOME
     * @Description=Nome
     * @Type=varchar
     * @Size=50
     * @IsRequired
     */
    public $nome;

    /**
     * @ColumnName=SYSAH_DESCRICAO
     * @Description=Descrição
     * @Type=varchar
     * @Size=255
     */
    public $descricao;

    /**
     * @ColumnName=SYSAH_VALOR
     * @Description=Valor
     * @Type=text
     * @Size=8000
     */
    public $valor;

}
