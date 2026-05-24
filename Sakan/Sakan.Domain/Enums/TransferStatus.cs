namespace Sakan.Domain.Enums
{
    public enum TransferStatus
    {
        Initiated = 1,
        DocumentsUploaded,
        LegalReview,
        PaymentHeld,
        Signed,
        Registered,
        Completed,
        Cancelled
    }
}
