using Khronus.Framework.DataAccess;
using Khronus.Framework.DataAccess.ORM;
using System;

namespace Sync.Custom
{
    [Table("usuam")]
    public class LojaVO : BusinessObjectBase
    {
        [Field]
        [IsKey]
        [IsUUID]
        public String USUAM_ID { get; set; }

        [Field]
        public int USUAM_FILIAL { get; set; }

        [Field]
        public int USUAM_CODIGO { get; set; }

        [Field]
        public string USUAM_NOME { get; set; }

        [Field]
        public string USUAM_ERP_WOO_LOJA_CODIGO { get; set; }

        [Field]
        public string USUAM_ERP_WOO_CLASSE_ENTREG { get; set; }

        [Field]
        public int USUAM_ERP_CODIGO { get; set; }

        [Field]
        public int USUAM_ERP_BLING_LOJA_CODIGO { get; set; }

        [Field]
        public string USUAM_ERP_BLING_TOKEN { get; set; }

        [Field]
        public bool USUAM_ATUALIZA_PRODUTOS { get; set; }

        [Field]
        public bool USUAM_INATIVO { get; set; }
    }
}
