using do_an_epu.DTOs.VnPay;

namespace do_an_epu.Services.Interface
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, PaymentInformationModel model);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
