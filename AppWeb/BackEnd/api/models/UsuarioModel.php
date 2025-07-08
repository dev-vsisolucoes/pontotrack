<?php

/**
 * @TableName=sysaq
 */
class UsuarioModel extends ValueObjectBase {

    /**
     * @ColumnName=SYSAQ_ID
     * @Description=Id
     * @Type=varchar
     * @Size=36
     * @IsRequired
     * @IsKey
     * @IsUUID     
     */
    public $id;

    /**
     * @ColumnName=SYSAQ_FILIAL_COD
     * @Description=Filial
     * @Type=int
     * @Size=11
     * @IsStoreField
     */
    public $filial;
    
    /**
     * @ColumnName=SYSAQ_CODIGO
     * @Description=Número
     * @Type=varchar
     * @Size=10
     * @IsRequired
     * @IsAutoIncrement
     */
    public $codigo;

    /**
     * @ColumnName=SYSAQ_NUMERO
     * @Description=Número
     * @Type=varchar
     * @Size=10
     * @IsRequired
     */
    public $numero;

    /**
     * @ColumnName=SYSAQ_USUARIO
     * @Description=Nome
     * @Type=varchar
     * @Size=50
     * @IsRequired
     */
    public $usuario;
    
     /**
     * @ColumnName=SYSAQ_AGENTE_ID
     * @Description=Id do agente
     * @Type=varchar
     * @Size=45
     */
    public $agenteId;

    /**
     * @ColumnName=SYSAQ_SENHA
     * @Description=Senha
     * @Type=varchar
     * @Size=50
     */
    public $senha;
    
     /**
     * @ColumnName=SYSAQ_FUNIL
     * @Description=Funil
     * @Type=varchar
     * @Size=50
     */
    public $funil;

    /**
     * @ColumnName=SYSAQ_TIPO
     * @Description=Tipo
     * @Type=varchar
     * @Size=2     
     */
    public $tipo;

    /**
     * @ColumnName=SYSAQ_PARCEIRO_ID
     * @Description=Parceiro
     * @Type=varchar
     * @Size=36
     */
    public $parceiroId;

    /**
     * @ColumnName=SYSAQ_PERFIL_ID
     * @Description=Perfil
     * @Type=varchar
     * @Size=36
     */
    public $perfilId;

    /**
     * @ColumnName=SYSAQ_AMBIENTE
     * @Description=Ambiente
     * @Type=text
     * @Size=5000
     */
    public $ambiente;

    /**
     * @ColumnName=SYSAQ_INATIVO
     * @Description=Inativo
     * @Type=tinyint
     * @Size=1
     */
    public $inativo;
    
    /**
     * @ColumnName=SYSAQ_PARTICIPA_ROLETA
     * @Description=Participa da rolota
     * @Type=tinyint
     * @Size=1
     */
    public $participaRoleta;
    
    /**
     * @ColumnName=SYSAQ_EMAIL
     * @Description=Email
     * @Type=varchar
     * @Size=100
     */
    public $email;
    
    /**
     * @ColumnName=SYSAQ_EQUIPE_NAME
     * @Description=Nome da equipe
     * @Type=varchar
     * @Size=100
     */
    public $equipe;

}
