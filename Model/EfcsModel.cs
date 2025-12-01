using System;
using System.Security.Cryptography;
using System.Text;


namespace hsinchugas_efcs_api.Model
{
    //ALL
    public class ALL<TBody>
    {
        public FUN FUN { get; set; }
        public DOCDATA<TBody> DOCDATA { get; set; }

        public SEC SEC { get; set; }
    }


    //FUN
    public class FUN
    {
        public string FUNNAME { get; set; }
        public string VER { get; set; }

    }

    //DOCDATA
    public class DOCDATA<TBody>
    {
        public HEAD HEAD { get; set; }
        public TBody BODY { get; set; }
    }

    //HEAD
    public class HEAD
    {
        public string SRC_ID { get; set; }
        public string DEST_ID { get; set; }
        public string TXN_DATETIME { get; set; }
        public string TXN_NO { get; set; }
        public string PRS_CODE { get; set; }
        public string BILLER_ID { get; set; }
        public string BTYPECLASS { get; set; }
        public string ICCHK_CODE { get; set; }
        public string? ICCHK_CODE_DESC { get; set; }
    }

    //SEC
    public class SEC
    {
        public string DIG { get; set; }
        public string MAC { get; set; }

    }











    //B207Q
    public class BillerDataQueryRq
    {
        public string QUERY_TYPE { get; set; }
        public string QUERY_DATA1 { get; set; }
        public string? QUERY_DATA2 { get; set; }
        public string? QUERY_DATA3 { get; set; }
        public string? QUERY_DATA4 { get; set; }
        public string? QUERY_DATA5 { get; set; }

    }

    //B207S
    public class BillerDataQueryRs
    {


    }



}
