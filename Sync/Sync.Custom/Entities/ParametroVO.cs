using Khronus.Framework.DataAccess.ORM;
using System;

namespace Sync.Custom
{
    [Table("sysah")]
    public class ParametroVO : BusinessObjectBase
    {
        [Field]
        [IsKey]
        [IsUUID]
        public String SYSAH_ID { get; set; }

        [Field]
        public int SYSAH_FILIAL_COD { get; set; }

        [Field]
        public int SYSAH_CODIGO { get; set; }

        [Field]
        public String SYSAH_NOME { get; set; }

        [Field]
        public String SYSAH_VALOR { get; set; }

        [Field]
        public String SYSAH_DESCRICAO { get; set; }
    }
}
