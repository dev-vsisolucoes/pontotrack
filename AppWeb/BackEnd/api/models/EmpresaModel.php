<?php

/**
 * @TableName=sysaa
 */
class EmpresaModel extends ValueObjectBase {

    /**
     * @ColumnName=SYSAA_ID
     * @Description=
     * @Type=int
     * @Size=11
     * @IsRequired
     * @IsKey
     * @IsAutoIncrement  
     */
    public $SYSAA_ID;

    /**
     * @ColumnName=SYSAA_APELIDO
     * @Description=
     * @Type=varchar
     * @Size=50
     * @IsRequired
     */
    public $SYSAA_APELIDO;

    /**
     * @ColumnName=SYSAA_TIPO_PESSOA
     * @Description=
     * @Type=varchar
     * @Size=1
     * @IsRequired
     */
    public $SYSAA_TIPO_PESSOA;

    /**
     * @ColumnName=SYSAA_NOME_FANTASIA
     * @Description=
     * @Type=varchar
     * @Size=255
     * @IsRequired
     */
    public $SYSAA_NOME_FANTASIA;

    /**
     * @ColumnName=SYSAA_RAZAO_SOCIAL
     * @Description=
     * @Type=varchar
     * @Size=255
     * @IsRequired
     */
    public $SYSAA_RAZAO_SOCIAL;

    /**
     * @ColumnName=SYSAA_CNPJ_CPF
     * @Description=
     * @Type=varchar
     * @Size=50
     * @IsRequired
     */
    public $SYSAA_CNPJ_CPF;

    /**
     * @ColumnName=SYSAA_IE_RG
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_IE_RG;

    /**
     * @ColumnName=SYSAA_CNAE
     * @Description=
     * @Type=varchar
     * @Size=10
     */
    public $SYSAA_CNAE;

    /**
     * @ColumnName=SYSAA_CRT
     * @Description=
     * @Type=int
     * @Size=1
     */
    public $SYSAA_CRT;

    /**
     * @ColumnName=SYSAA_IM
     * @Description=
     * @Type=varchar
     * @Size=10
     */
    public $SYSAA_IM;  

    /**
     * @ColumnName=SYSAA_PERC_CRED_ICMSSN
     * @Description=
     * @Type=float
     * @Size=20
     */
    public $SYSAA_PERC_CRED_ICMSSN;    

    /**
     * @ColumnName=SYSAA_CEP
     * @Description=
     * @Type=varchar
     * @Size=10
     */
    public $SYSAA_CEP;

    /**
     * @ColumnName=SYSAA_ENDERECO
     * @Description=
     * @Type=varchar
     * @Size=255
     */
    public $SYSAA_ENDERECO;

    /**
     * @ColumnName=SYSAA_NUMERO
     * @Description=
     * @Type=varchar
     * @Size=10
     */
    public $SYSAA_NUMERO;

    /**
     * @ColumnName=SYSAA_COMPLEMENTO
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_COMPLEMENTO;

    /**
     * @ColumnName=SYSAA_BAIRRO
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_BAIRRO;

    /**
     * @ColumnName=SYSAA_CIDADE_COD
     * @Description=
     * @Type=varchar
     * @Size=10
     */
    public $SYSAA_CIDADE_COD;

    /**
     * @ColumnName=SYSAA_CIDADE
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_CIDADE;

    /**
     * @ColumnName=SYSAA_UF_COD
     * @Description=
     * @Type=varchar
     * @Size=2
     */
    public $SYSAA_UF_COD;

    /**
     * @ColumnName=SYSAA_UF
     * @Description=
     * @Type=varchar
     * @Size=2
     */
    public $SYSAA_UF;

    /**
     * @ColumnName=SYSAA_PAIS_COD
     * @Description=
     * @Type=varchar
     * @Size=10
     */
    public $SYSAA_PAIS_COD;

    /**
     * @ColumnName=SYSAA_PAIS
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_PAIS;

    /**
     * @ColumnName=SYSAA_EMAIL
     * @Description=
     * @Type=varchar
     * @Size=100
     */
    public $SYSAA_EMAIL;

    /**
     * @ColumnName=SYSAA_FONE1
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_FONE1;

    /**
     * @ColumnName=SYSAA_FONE2
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_FONE2;

    /**
     * @ColumnName=SYSAA_CELULAR
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_CELULAR;    

    /**
     * @ColumnName=SYSAA_SITE
     * @Description=
     * @Type=varchar
     * @Size=50
     */
    public $SYSAA_SITE;

    /**
     * @ColumnName=SYSAA_LOGOTIPO
     * @Description=
     * @Type=varchar
     * @Size=50
     */     
    public $SYSAA_LOGOTIPO;

}
