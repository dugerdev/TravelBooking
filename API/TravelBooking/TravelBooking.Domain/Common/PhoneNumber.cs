using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TravelBooking.Domain.Common;

//---Telefon numarasi icin Value Object---//
public class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; private set; } = string.Empty;                 //---Telefon numarasi degeri---//

    protected PhoneNumber() { }                                                //---EF Core icin parameterless constructor---//

    public PhoneNumber(string phoneNumber)                                     //---Telefon numarasi olusturan constructor---//
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Telefon numarasi bos olamaz.", nameof(phoneNumber));

        //---Bosluk, tire ve parantez karakterlerini temizle---//
        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (!PhoneRegex.IsMatch(cleaned))
            throw new ArgumentException("Gecersiz telefon numarasi formati.", nameof(phoneNumber));

        Value = cleaned;
    }

    protected override IEnumerable<object> GetEqualityComponents()             //---Esitlik karsilastirmasi icin bilesenler---//
    {
        yield return Value;
    }

    public override string ToString() => Value;                                //---Telefon numarasini string olarak donduren metot---//

    //---String'den PhoneNumber'a implicit donusum---//
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}

