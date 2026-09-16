namespace SmartMetroService.Application.Models;

public class AamarPayRequest
{
    public string store_id { get; set; }
    public string tran_id { get; set; }
    public string success_url { get; set; }
    public string fail_url { get; set; }
    public string cancel_url { get; set; }
    public string amount { get; set; }
    public string currency { get; set; } = "BDT";
    public string signature_key { get; set; }
    public string desc { get; set; }
    public string cus_name { get; set; }
    public string cus_email { get; set; }
    public string cus_add1 { get; set; }
    public string cus_add2 { get; set; }
    public string cus_city { get; set; } = "Dhaka";
    public string cus_state { get; set; } = "Dhaka";
    public string cus_postcode { get; set; } = "1206";
    public string cus_country { get; set; } = "Bangladesh";
    public string cus_phone { get; set; }
    public string type { get; set; } = "json";
}
