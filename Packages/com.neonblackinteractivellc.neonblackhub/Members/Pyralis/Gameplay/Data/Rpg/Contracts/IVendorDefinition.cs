namespace NeonBlack.Gameplay.Data.Rpg
{
    public interface IVendorDefinition
    {
        string VendorId { get; }
        string DisplayName { get; }
        VendorOffer[] Offers { get; }
        bool TryGetOffer(string offerId, out VendorOffer offer);
    }
}
