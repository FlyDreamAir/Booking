namespace FlyDreamAir.Services;

public class CardService
{
    private readonly string _defaultCardName;
    private readonly string _defaultCardNumber;
    private readonly string _defaultCardExpiration;
    private readonly string _defaultCardCvv;

    public CardService(IConfiguration configuration)
    {
        _defaultCardName = configuration["Payment:Name"] ?? "NGUYEN VAN THIEU";
        _defaultCardNumber = configuration["Payment:CardNumber"] ?? "4242 4242 4242 4242";
        _defaultCardExpiration = configuration["Payment:Expiration"] ?? "04/75";
        _defaultCardCvv = configuration["Payment:CVV"] ?? "304";
    }

    public Task<bool> ValidateAsync(
        string cardName,
        string cardNumber,
        string cardExpiration,
        string cardCvv
    )
    {
        if (cardName.ToUpperInvariant() != _defaultCardName
            || cardNumber != _defaultCardNumber
            || cardExpiration != _defaultCardExpiration
            || cardCvv != _defaultCardCvv)
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
