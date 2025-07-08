using System;
using System.Data;

namespace Sync.Custom
{
    public class UsuarioBO : UsuarioVO
    {
        //public override Object Insert()
        //{
        //    if (this.SYSAQ_CODIGO < 1)
        //    {
        //        this.SYSAQ_CODIGO = Worker.GetInstance().GetNumerator(this.SYSAQ_FILIAL_COD, this.GetType().Name);
        //    }

        //    Object ret = Worker.GetInstance().DataAccessManager.Insert(this);

        //    return ret;
        //}

        //public override Object Update()
        //{
        //    return Worker.GetInstance().DataAccessManager.Update(this);
        //}

        //public UsuarioBO Find(int param1, String param2)
        //{
        //    return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_ID", "'" + param2 + "'" } });
        //}

        //public UsuarioBO FindByNumero(int param1, String param2, String param3)
        //{
        //    return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_NUMERO", "'" + param2 + "'" } });
        //}

        ////public DataTable GetList(int param1, String param2)
        ////{
        ////    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_CODIGO", "'" + param2 + "'" } });
        ////}

        //public DataTable GetList(int param1, bool param2)
        //{
        //    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAQ_FILIAL_COD", param1 }, { "SYSAQ_INTEGRADO", param2 } });
        //}
    }
}
