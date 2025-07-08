using Khronus.Framework.DataAccess;
using Khronus.Framework.DataAccess.ORM;
using System;

namespace Sync.Custom
{
    [Table("usuan")]
    public class EventoVO : BusinessObjectBase
    {
        [Field]
        [IsKey]
        [IsUUID]
        public String USUAN_ID { get; set; }

        [Field]
        public int USUAN_FILIAL { get; set; }

        [Field]
        public int USUAN_CODIGO { get; set; }

        [Field]
        public int USUAN_TIPO { get; set; }

        [Field]
        public DateTime USUAN_DATA { get; set; }

        [Field]
        public String USUAN_EVENTO { get; set; }

        [Field]
        public String USUAN_STATUS { get; set; }

        [Field]
        public String USUAN_STATUS_MOTIVO { get; set; }

        [Field]
        public String USUAN_EVENTO_RETORNO { get; set; }
    }
}
