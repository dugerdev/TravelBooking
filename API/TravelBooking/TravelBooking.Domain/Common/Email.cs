using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TravelBooking.Domain.Common;

//---E-posta adresi icin Value Object---//
public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; private set; } = string.Empty;                 //---E-posta adresi degeri---//

    protected Email() { }                                                      //---EF Core icin parameterless constructor---//

    public Email(string email)                                                 //---E-posta olusturan constructor---//
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-posta adresi bos olamaz.", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Gecersiz e-posta adresi formati.", nameof(email));

        Value = email.Trim().ToLowerInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()             //---Esitlik karsilastirmasi icin bilesenler---//
    {
        yield return Value;
    }

    public override string ToString() => Value;                                //---E-posta adresini string olarak donduren metot---//

    //---String'den Email'e implicit donusum---//
    public static implicit operator string(Email email) => email.Value;
}

