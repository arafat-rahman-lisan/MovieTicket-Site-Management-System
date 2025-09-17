// File: Data/Base/IEntityBase.cs
namespace Movie_Site_Management_System.Data.Base
{
    /// <summary>
    /// Marker interface for domain entities. 
    /// (We are not enforcing an Id property here because your models have different PK names:
    /// TheatreId, HallId, SeatTypeId (short), etc.)
    /// </summary>
    public interface IEntityBase
    {
    }
}
