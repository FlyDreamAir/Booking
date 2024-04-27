namespace FlyDreamAir.Services;

public class CardService
{
    public Task<bool> ValidateAsync(
        string cardName,
        string cardNumber,
        string cardExpiration,
        string cardCvv
    )
    {
        if (cardName.ToUpperInvariant() != "NGUYEN VAN THIEU"
            || cardNumber != "4242 4242 4242 4242"
            || cardExpiration != "04/75"
            || cardCvv != "304")
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task ChargeAsync(
        string cardName,
        string cardNumber,
        string cardExpiration,
        string cardCvv,
        decimal amount
    )
    {
        return Task.CompletedTask;
    }

    public Task RefundAsync(
        string cardName,
        string cardNumber,
        decimal amount
    )
    {
        return Task.CompletedTask;
    }
}
