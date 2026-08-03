namespace ECommercePlatform.Modules.Inventory.Domain;

/// <summary>
/// Only Inbound is exercised for now (stock received). Reserved/Released/Committed will be added
/// when the Order module needs them — adding an enum member later needs no migration.
/// </summary>
public enum StockMovementType
{
    Inbound = 0,
}
