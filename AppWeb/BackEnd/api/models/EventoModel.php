<?php

/**
 * @TableName=usuan
 */
class EventoModel extends ValueObjectBase {

    /**
     * @ColumnName=USUAN_ID
     * @Description=Id
     * @Type=varchar
     * @Size=36
     * @IsRequired
     * @IsKey
     * @IsUUID
     */
    public $id;

    /**
     * @ColumnName=USUAN_FILIAL
     * @Description=Filial
     * @Type=int
     * @Size=11
     */
    public $filial;
    
    /**
     * @ColumnName=USUAN_CODIGO
     * @Description=Código
     * @Type=int
     * @Size=11
     * @IsRequired
     * @IsAutoIncrement 
     */
    public $codigo;
    
    /**
     * @ColumnName=USUAN_TIPO
     * @Description=Tipo
     * @Type=int
     * @Size=11
     */
    public $tipo;

    /**
     * @ColumnName=USUAN_DATA
     * @Description=Data
     * @Type=datetime
     */
    public $data;    
    
    /**
     * @ColumnName=USUAN_EVENTO
     * @Description=Evento
     * @Type=text
     */
    public $evento;  
    
    /**
     * @ColumnName=USUAN_STATUS
     * @Description=Status
     * @Type=varchar
     * @Size=2
     */
    public $status;
    
    /**
     * @ColumnName=USUAN_STATUS_MOTIVO
     * @Description=Status Motivo
     * @Type=text
     */
    public $statusMotivo;     
}
    
