using System.Collections.Generic;
using System.Linq;



namespace TravelBooking.Domain.Common;

public abstract class ValueObject  //---Deger Nesnesi (Value Object) Temel Sinifi

{
    protected abstract IEnumerable<object> GetEqualityComponents();         //---Esitlik Karsilastirmasi Icin Bilesenleri Saglayan Soyut Metot

    public override bool Equals(object? obj)                                //---Nesnelerin Esitligini Karsilastiran Metot
    {
        if (obj == null || obj.GetType() != GetType())
            return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    public override int GetHashCode()                                       //---Nesnenin Hash Kodunu Donduren Metot
    {
        return GetEqualityComponents()

           .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

}
