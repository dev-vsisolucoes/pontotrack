using Khronus.Framework.DataAccess;
using Khronus.Framework.DataAccess.ORM;
using System;

namespace Sync.Custom
{
    [Table("sysaq")]
    public class UsuarioVO : BusinessObjectBase
    {
        [Field]
        [IsKey]
        [IsUUID]
        public string SYSAQ_ID { get; set; }

        [Field]
        public int SYSAQ_FILIAL_COD { get; set; }

        [Field]
        public int SYSAQ_CODIGO { get; set; }

        [Field]
        public string SYSAQ_NUMERO { get; set; }

        [Field]
        public string SYSAQ_USUARIO { get; set; }

        [Field]
        public string SYSAQ_EMAIL { get; set; }

        [Field]
        public long SYSAQ_EQUIPE_ID { get; set; }

        //[FieldAttr]
        //public int SYSAQ_AGENCIA { get; set; }

        [Field]
        public string SYSAQ_EQUIPE_NAME { get; set; }

        [Field]
        public bool SYSAQ_PARTICIPA_ROLETA { get; set; }

        //[Field]
        //public bool SYSAQ_USADO_ROLETA { get; set; }

        //[Field]
        //public string SYSAQ_LOCALIZACAO { get; set; }

        [Field]
        public bool SYSAQ_INATIVO { get; set; }

        [Field]
        public string SYSAQ_AGENTE_ID { get; set; }

    }
}
