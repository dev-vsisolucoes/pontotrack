using System;
using System.Data;

namespace Sync.Custom
{
    public class ParametroBO : ParametroVO
    {
        //public override Object Insert()
        //{
        //    if (this.SYSAH_CODIGO < 1)
        //    {
        //        this.SYSAH_CODIGO = Worker.GetInstance().GetNumerator(this.SYSAH_FILIAL_COD, this.GetType().Name);
        //    }

        //    //Object ret = Worker.GetInstance().DataAccessManager.Insert(this);

        //    return DataAccessManager.Insert(this);
        //}

        //public override Object Update()
        //{
        //    //return Worker.GetInstance().DataAccessManager.Update(this);
        //    return DataAccessManager.Update(this);
        //}

        //public UsuarioBO Find(int param1, String param2)
        //{
        //    //return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_ID", "'" + param2 + "'" } });
        //    return (UsuarioBO)DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_ID", "'" + param2 + "'" } });
        //}

        //public UsuarioBO FindByCodigo(int param1, int param2)
        //{
        //    //return (UsuarioBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_NUMERO", "" + param2 + "" } });
        //    return (UsuarioBO)DataAccessManager.Find(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_NUMERO", "" + param2 + "" } });
        //}

        //public DataTable GetList(int param1)
        //{
        //    //return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 } });
        //    return DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 } });
        //}

        //public DataTable GetList(int param1, String param2)
        //{
        //    //return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_CODIGO", "'" + param2 + "'" } });
        //    return DataAccessManager.GetList(this, new Object[,] { { "SYSAH_FILIAL_COD", param1 }, { "SYSAH_CODIGO", "'" + param2 + "'" } });
        //}
    }
}
