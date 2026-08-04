using ECommercePlatform.Modules.Payment.Application.Abstractions;

namespace ECommercePlatform.Modules.Payment.Infrastructure.Gateways;

/// <summary>
/// Fake stand-in for a real payment gateway (Stripe, iyzico, ...). This exists so the rest of the
/// codebase — Payment.Domain, Payment.Application, PaymentsController, and every test — can be
/// written and verified end to end against a REAL-SHAPED contract (<see cref="IPaymentGateway"/>)
/// without an actual gateway account. When iyzico is integrated later, this class is the ONLY one
/// that gets replaced; nothing that depends on <see cref="IPaymentGateway"/> needs to change.
///
/// Test-card convention (deliberately mirrors how real test-mode gateways work — e.g. Stripe's
/// well-known "4242...4242 always succeeds, ...0002 always declines" test cards — so swapping in a
/// real gateway later means swapping *which* magic numbers trigger which outcome, not changing how
/// the rest of the system reasons about payments):
///
///   - Card numbers ending in "0000"  -> declined ("Kart reddedildi (mock).")
///   - Anything else                  -> approved
///
/// This is intentionally the simplest possible rule — just enough to exercise both the success and
/// decline paths in ProcessPaymentCommandHandler and its tests.
/// </summary>
internal sealed class MockPaymentGateway : IPaymentGateway
{
    private const string DeclinedCardSuffix = "0000";

    public Task<ChargeResult> ChargeAsync(decimal amount, string cardNumber, CancellationToken cancellationToken)
    {
        var result = cardNumber.EndsWith(DeclinedCardSuffix, StringComparison.Ordinal)
            ? new ChargeResult(IsSuccessful: false, FailureReason: "Kart reddedildi (mock).")
            : new ChargeResult(IsSuccessful: true, FailureReason: null);

        return Task.FromResult(result);
    }
}
