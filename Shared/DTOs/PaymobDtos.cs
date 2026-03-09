namespace ELProject.Domain.DTOs
{
    public class PaymobIntentRequest
    {
        public long Amount { get; set; } 
        public string Currency { get; set; } = "EGP";
        public List<int> Payment_methods { get; set; } = [];
        public BillingData Billing_data { get; set; } = new BillingData();
        public string Special_reference { get; set; } = string.Empty; 
    }

    public class BillingData
    {
        public string First_name { get; set; } = "NA";
        public string Last_name { get; set; } = "NA";
        public string Email { get; set; } = "NA";
        public string Phone_number { get; set; } = "NA";
        public string Apartment { get; set; } = "NA";
        public string Floor { get; set; } = "NA";
        public string Street { get; set; } = "NA";
        public string Building { get; set; } = "NA";
        public string Shipping_method { get; set; } = "NA";
        public string City { get; set; } = "NA";
        public string Country { get; set; } = "EG";
        public string State { get; set; } = "NA";
    }


}