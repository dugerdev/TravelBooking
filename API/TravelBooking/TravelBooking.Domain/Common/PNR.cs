using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TravelBooking.Domain.Common;

//---PNR (Passenger Name Record) icin Value Object---//
public class PNR : ValueObject
{
    private static readonly Regex PnrRegex = new(
        @"^[A-Z0-9]{6}$",
        RegexOptions.Compiled);

    public string Value { get; private set; } = string.Empty;                 //---PNR degeri (6 karakterlik alfanumerik kod)---//

    protected PNR() { }                                                        //---EF Core icin parameterless constructor---//

    public PNR(string pnr)                                                     //---PNR olusturan constructor---//
    {
        if (string.IsNullOrWhiteSpace(pnr))
            throw new ArgumentException("PNR bos olamaz.", nameof(pnr));

        var normalizedPnr = pnr.Trim().ToUpperInvariant();

        if (!PnrRegex.IsMatch(normalizedPnr))
            throw new ArgumentException("PNR 6 karakterlik alfanumerik bir kod olmalidir.", nameof(pnr));

        Value = normalizedPnr;
    }

    //---Rastgele PNR olusturan static metot---//
    public static PNR Generate()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var pnrChars = new char[6];
        
        for (int i = 0; i < 6; i++)
        {
            pnrChars[i] = chars[random.Next(chars.Length)];
        }

        return new PNR(new string(pnrChars));
    }

    protected override IEnumerable<object> GetEqualityComponents()             //---Esitlik karsilastirmasi icin bilesenler---//
    {
        yield return Value;
    }

    public override string ToString() => Value;                                //---PNR'i string olarak donduren metot---//

    //---String'den PNR'a implicit donusum---//
    public static implicit operator string(PNR pnr) => pnr.Value;
}

