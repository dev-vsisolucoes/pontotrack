using System;
using System.Data;

namespace Sync.Custom
{
    public class EventoBO : EventoVO
    {
        //public override Object Insert()
        //{
        //    if(this.USUAN_CODIGO < 1)
        //    {
        //        this.USUAN_CODIGO = Worker.GetInstance().GetNumerator(this.USUAN_FILIAL, this.GetType().Name);
        //    }            

        //    Object ret = DataAccessManager.Insert(this);

        //    return ret;
        //}

        //public override Object Update()
        //{
        //    return Worker.GetInstance().DataAccessManager.Update(this);
        //}

        //public IntegracaoBO Find(int param1, String param2)
        //{
        //    return (IntegracaoBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_ID", "'" + param2 + "'" } });
        //}

        //public IntegracaoBO FindByNumero(int param1, String param2)
        //{
        //    return (IntegracaoBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_NUMERO", "'" + param2 + "'" } });
        //}

        //public IntegracaoBO FindByEvento(int param1, int param2, String param3)
        //{
        //    return (IntegracaoBO)Worker.GetInstance().DataAccessManager.Find(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_TIPO", param2 }, { "USUAN_EVENTO", "'" + param2 + "'" } });
        //}


        //public DataTable GetList(int param1, String param2)
        //{
        //    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_STATUS", "'" + param2 + "'" } });
        //}

        //public DataTable GetList(int param1, int param2, String param3)
        //{
        //    return Worker.GetInstance().DataAccessManager.GetList(this, new Object[,] { { "USUAN_FILIAL", param1 }, { "USUAN_TIPO", param2 }, { "USUAN_STATUS", "'" + param3 + "'" } });
        //}
    }
}
