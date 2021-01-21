using Assignment2.Models;

namespace Assignment2.ViewModels
{
    public class AtmTransferModel
    {
        public int AccountNumber { get; set; }
        public int DestinationAccountNumber { get; set; }
        public Account Account { get; set; }
        public Account DestinationAccount { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
    }
}
