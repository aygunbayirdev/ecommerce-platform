namespace ECommercePlatform.Modules.Payment.Application.Abstractions;

/// <summary>Result of a single charge attempt against a payment gateway.</summary>
public sealed record ChargeResult(bool IsSuccessful, string? FailureReason);

/// <summary>
/// The "port" in a port/adapter (hexagonal) architecture: everything in this module — the domain
/// model, ProcessPaymentCommandHandler, the controller, the tests — is written against this
/// interface, never against a concrete payment provider's SDK/API directly. This is the exact same
/// pattern as <c>IPasswordHasher</c>/<c>ITokenGenerator</c> in the Identity module: Application
/// defines the contract it needs, Infrastructure supplies a concrete implementation.
///
/// Today the only implementation is <c>MockPaymentGateway</c> (Payment.Infrastructure), which fakes
/// success/decline based on the card number instead of calling a real provider — Stripe doesn't
/// support Turkey as a business country, so real gateway integration is deferred to a later,
/// hands-on task where iyzico (a Turkish payment provider) will be plugged in. Swapping providers
/// later means writing ONE new class — `IyzicoPaymentGateway : IPaymentGateway` — and changing ONE
/// line of DI registration in `PaymentModule.cs`. Nothing else in this module needs to change,
/// which is the entire point of depending on an abstraction here instead of a concrete SDK type.
/// </summary>
public interface IPaymentGateway
{
    Task<ChargeResult> ChargeAsync(decimal amount, string cardNumber, CancellationToken cancellationToken);
}
