using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using TravelBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(TravelBookingDbContext context)
    {
        // Airports
        if (!await context.Airports.AnyAsync())
        {
            var airports = new List<Airport>
            {
                // Turkiye - Istanbul
                new Airport("IST", "Istanbul", "Turkiye", "Istanbul Havalimani"),
                new Airport("SAW", "Istanbul", "Turkiye", "Sabiha Gokcen Havalimani"),
                
                // Turkiye - Ankara
                new Airport("ESB", "Ankara", "Turkiye", "Esenboga Havalimani"),
                
                // Turkiye - Izmir
                new Airport("ADB", "Izmir", "Turkiye", "Adnan Menderes Havalimani"),
                
                // Turkiye - Antalya
                new Airport("AYT", "Antalya", "Turkiye", "Antalya Havalimani"),
                new Airport("GZP", "Antalya", "Turkiye", "Gazipasa-Alanya Havalimani"),
                
                // Turkiye - Diger
                new Airport("BJV", "Bodrum", "Turkiye", "Milas-Bodrum Havalimani"),
                new Airport("DLM", "Dalaman", "Turkiye", "Dalaman Havalimani"),
                new Airport("TZX", "Trabzon", "Turkiye", "Trabzon Havalimani"),
                new Airport("ASR", "Kayseri", "Turkiye", "Kayseri Havalimani"),
                
                // Avrupa - Londra (6 havalimani)
                new Airport("LHR", "Londra", "Ingiltere", "London Heathrow Airport"),
                new Airport("LGW", "Londra", "Ingiltere", "London Gatwick Airport"),
                new Airport("STN", "Londra", "Ingiltere", "London Stansted Airport"),
                new Airport("LTN", "Londra", "Ingiltere", "London Luton Airport"),
                new Airport("LCY", "Londra", "Ingiltere", "London City Airport"),
                new Airport("SEN", "Londra", "Ingiltere", "London Southend Airport"),
                
                // Avrupa - Paris (3 havalimani)
                new Airport("CDG", "Paris", "Fransa", "Charles de Gaulle Airport"),
                new Airport("ORY", "Paris", "Fransa", "Paris Orly Airport"),
                new Airport("BVA", "Paris", "Fransa", "Paris Beauvais Airport"),
                
                // Avrupa - Diger
                new Airport("FRA", "Frankfurt", "Almanya", "Frankfurt Airport"),
                new Airport("MUC", "Munih", "Almanya", "Munich Airport"),
                new Airport("TXL", "Berlin", "Almanya", "Berlin Tegel Airport"),
                new Airport("SXF", "Berlin", "Almanya", "Berlin Schonefeld Airport"),
                new Airport("AMS", "Amsterdam", "Hollanda", "Amsterdam Schiphol Airport"),
                new Airport("FCO", "Roma", "Italya", "Leonardo da Vinci-Fiumicino Airport"),
                new Airport("CIA", "Roma", "Italya", "Rome Ciampino Airport"),
                new Airport("MAD", "Madrid", "Ispanya", "Adolfo Suárez Madrid-Barajas Airport"),
                new Airport("BCN", "Barselona", "Ispanya", "Barcelona-El Prat Airport"),
                new Airport("VIE", "Viyana", "Avusturya", "Vienna International Airport"),
                new Airport("ZRH", "Zurih", "Isvicre", "Zurich Airport"),
                
                // Orta Dogu - Dubai (2 havalimani)
                new Airport("DXB", "Dubai", "BAE", "Dubai International Airport"),
                new Airport("DWC", "Dubai", "BAE", "Al Maktoum International Airport"),
                
                // Orta Dogu - Diger
                new Airport("DOH", "Doha", "Katar", "Hamad International Airport"),
                new Airport("CAI", "Kahire", "Misir", "Cairo International Airport"),
                new Airport("SPX", "Kahire", "Misir", "Sphinx International Airport"),
                new Airport("JED", "Cidde", "Suudi Arabistan", "King Abdulaziz International Airport"),
                new Airport("RUH", "Riyad", "Suudi Arabistan", "King Khalid International Airport"),
                
                // Amerika - New York (3 havalimani)
                new Airport("JFK", "New York", "ABD", "John F. Kennedy International Airport"),
                new Airport("LGA", "New York", "ABD", "LaGuardia Airport"),
                new Airport("EWR", "New York", "ABD", "Newark Liberty International Airport"),
                
                // Amerika - Los Angeles (5 havalimani)
                new Airport("LAX", "Los Angeles", "ABD", "Los Angeles International Airport"),
                new Airport("BUR", "Los Angeles", "ABD", "Hollywood Burbank Airport"),
                new Airport("ONT", "Los Angeles", "ABD", "Ontario International Airport"),
                new Airport("SNA", "Los Angeles", "ABD", "John Wayne Airport"),
                new Airport("LGB", "Los Angeles", "ABD", "Long Beach Airport"),
                
                // Amerika - Chicago (2 havalimani)
                new Airport("ORD", "Chicago", "ABD", "O'Hare International Airport"),
                new Airport("MDW", "Chicago", "ABD", "Chicago Midway International Airport"),
                
                // Amerika - Diger
                new Airport("MIA", "Miami", "ABD", "Miami International Airport"),
                new Airport("SFO", "San Francisco", "ABD", "San Francisco International Airport"),
                new Airport("SEA", "Seattle", "ABD", "Seattle-Tacoma International Airport"),
                new Airport("YYZ", "Toronto", "Kanada", "Toronto Pearson International Airport"),
                
                // Asya - Tokyo (2 havalimani)
                new Airport("NRT", "Tokyo", "Japonya", "Narita International Airport"),
                new Airport("HND", "Tokyo", "Japonya", "Tokyo Haneda Airport"),
                
                // Asya - Pekin (2 havalimani)
                new Airport("PEK", "Pekin", "Cin", "Beijing Capital International Airport"),
                new Airport("PKX", "Pekin", "Cin", "Beijing Daxing International Airport"),
                
                // Asya - Sangay (2 havalimani)
                new Airport("PVG", "Sangay", "Cin", "Shanghai Pudong International Airport"),
                new Airport("SHA", "Sangay", "Cin", "Shanghai Hongqiao International Airport"),
                
                // Asya - Diger
                new Airport("ICN", "Seul", "Guney Kore", "Incheon International Airport"),
                new Airport("GMP", "Seul", "Guney Kore", "Gimpo International Airport"),
                new Airport("SIN", "Singapur", "Singapur", "Singapore Changi Airport"),
                new Airport("BKK", "Bangkok", "Tayland", "Suvarnabhumi Airport"),
                new Airport("DMK", "Bangkok", "Tayland", "Don Mueang International Airport"),
                new Airport("HKG", "Hong Kong", "Hong Kong", "Hong Kong International Airport"),
                new Airport("KUL", "Kuala Lumpur", "Malezya", "Kuala Lumpur International Airport"),
                new Airport("DEL", "Delhi", "Hindistan", "Indira Gandhi International Airport"),
                // Maldivler
                new Airport("MLE", "Malé", "Maldivler", "Velana International Airport")
            };

            await context.Airports.AddRangeAsync(airports);
            await context.SaveChangesAsync();
        }
        
        // Hotels
        if (!await context.Hotels.AnyAsync())
        {
            var hotels = new List<Hotel>([])
            {
                new Hotel("Grand Plaza Hotel", "Istanbul", "Turkiye", "Taksim Meydani No:5", 5, new Money(1500, Currency.TRY), "/images/hotels/grand-plaza.jpg", "Luks ve konforlu bir konaklama deneyimi", true, true, true, true),
                new Hotel("Seaside Resort", "Antalya", "Turkiye", "Lara Plaji Yolu No:25", 4, new Money(1200, Currency.TRY), "/images/hotels/seaside-resort.jpg", "Deniz manzarali tatil koyu", true, true, true, true),
                new Hotel("City Center Hotel", "Ankara", "Turkiye", "Kizilay Caddesi No:42", 3, new Money(800, Currency.TRY), "/images/hotels/city-center.jpg", "Sehir merkezinde ekonomik konaklama", true, false, false, true)
            };

            // Rating ve ReviewCount'u ayarla
            hotels[0].UpdateRating(4.8, 245);
            hotels[1].UpdateRating(4.5, 189);
            hotels[2].UpdateRating(4.2, 156);

            await context.Hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();

            // Her otele ornek odalar ekle (oda bilgisi olmazsa Detail'de "oda bilgisi bulunmuyor" hatasi alinir)
            var roomCurrency = Currency.USD.ToString();
            foreach (var hotel in hotels)
            {
                var hotelRooms = new List<Room>
                {
                    new Room(hotel.Id, "Standart Oda", 30m, 2, "Standart konforlu oda", new List<string>(), roomCurrency),
                    new Room(hotel.Id, "Deluxe Oda", 40m, 3, "Genis deluxe oda", new List<string> { "Minibar", "Jakuzi" }, roomCurrency),
                    new Room(hotel.Id, "Suite", 60m, 4, "Suite oda, oturma alani", new List<string> { "Oturma alani", "Minibar" }, roomCurrency)
                };
                await context.Rooms.AddRangeAsync(hotelRooms);
            }
            await context.SaveChangesAsync();
        }

        // Oteller var ama hic oda yoksa (eski seed veya manuel eklenen oteller) her otele ornek oda ekle
        if (await context.Hotels.AnyAsync() && !await context.Rooms.AnyAsync())
        {
            var existingHotels = await context.Hotels.Where(h => !h.IsDeleted).ToListAsync();
            var roomCurrency = Currency.USD.ToString();
            foreach (var hotel in existingHotels)
            {
                var hotelRooms = new List<Room>
                {
                    new Room(hotel.Id, "Standart Oda", 30m, 2, "Standart konforlu oda", new List<string>(), roomCurrency),
                    new Room(hotel.Id, "Deluxe Oda", 40m, 3, "Genis deluxe oda", new List<string> { "Minibar", "Jakuzi" }, roomCurrency),
                    new Room(hotel.Id, "Suite", 60m, 4, "Suite oda, oturma alani", new List<string> { "Oturma alani", "Minibar" }, roomCurrency)
                };
                await context.Rooms.AddRangeAsync(hotelRooms);
            }
            await context.SaveChangesAsync();
        }

        // Odasi olmayan her otele 3 oda ekle (her API baslangicinda calisir, idempotent)
        var hotelsWithoutRooms = await context.Hotels
            .Where(h => !h.IsDeleted)
            .Where(h => !context.Rooms.Any(r => r.HotelId == h.Id && !r.IsDeleted))
            .ToListAsync();
        if (hotelsWithoutRooms.Count > 0)
        {
            var roomCurrency = Currency.USD.ToString();
            foreach (var hotel in hotelsWithoutRooms)
            {
                var hotelRooms = new List<Room>
                {
                    new Room(hotel.Id, "Standart Oda", 30m, 2, "Standart konforlu oda", new List<string>(), roomCurrency),
                    new Room(hotel.Id, "Deluxe Oda", 40m, 3, "Genis deluxe oda", new List<string> { "Minibar", "Jakuzi" }, roomCurrency),
                    new Room(hotel.Id, "Suite", 60m, 4, "Suite oda, oturma alani", new List<string> { "Oturma alani", "Minibar" }, roomCurrency)
                };
                await context.Rooms.AddRangeAsync(hotelRooms);
            }
            await context.SaveChangesAsync();
        }

        // Cars
        if (!await context.Cars.AnyAsync())
        {
            var cars = new List<Car>([])
            {
                new Car("Toyota", "Corolla", "Sedan", 2023, "Benzin", "Otomatik", 5, 4, new Money(350, Currency.TRY), "/images/cars/toyota-corolla.jpg", "Istanbul", true, true),
                new Car("Volkswagen", "Passat", "Sedan", 2023, "Dizel", "Otomatik", 5, 4, new Money(450, Currency.TRY), "/images/cars/vw-passat.jpg", "Ankara", true, true),
                new Car("Renault", "Clio", "Kompakt", 2022, "Benzin", "Manuel", 5, 4, new Money(250, Currency.TRY), "/images/cars/renault-clio.jpg", "Izmir", true, false)
            };

            // Rating ve ReviewCount'u ayarla
            cars[0].UpdateRating(4.6, 128);
            cars[1].UpdateRating(4.7, 95);
            cars[2].UpdateRating(4.3, 87);

            await context.Cars.AddRangeAsync(cars);
            await context.SaveChangesAsync();
        }

        // Tours
        if (!await context.Tours.AnyAsync())
        {
            var tours = new List<Tour>([])
            {
                new Tour("Kapadokya Turu", "Kapadokya", 3, new Money(2500, Currency.TRY), "/images/tours/cappadocia.jpg", "Kapadokya'nin essiz peribacalarini ve yeralti sehirlerini kesfedin", "Kolay", 15, ["Balon Turu", "Yeralti Sehirleri", "Goreme Acik Hava Muzesi"], ["Ulasim", "Konaklama", "Sabah Kahvaltisi", "Rehber"]),
                new Tour("Pamukkale & Hierapolis", "Denizli", 2, new Money(1800, Currency.TRY), "/images/tours/pamukkale.jpg", "Beyaz cennet Pamukkale ve antik Hierapolis sehri turu", "Kolay", 20, ["Pamukkale Travertenleri", "Hierapolis Antik Kenti", "Termal Havuzlar"], ["Ulasim", "Ogle Yemegi", "Muze Girisleri", "Rehber"]),
                new Tour("Likya Yolu Trekking", "Antalya - Fethiye", 5, new Money(4500, Currency.TRY), "/images/tours/lycian-way.jpg", "Turkiye'nin en unlu trekking rotasi Likya Yolu'nda macera", "Orta", 12, ["Olimpos Antik Kenti", "Chimaera Yanardag", "Deniz Manzarasi"], ["Trekking Ekipmani", "Konaklama", "3 Ogun Yemek", "Profesyonel Rehber"])
            };

            // Rating ve ReviewCount'u ayarla
            tours[0].UpdateRating(4.9, 312);
            tours[1].UpdateRating(4.7, 198);
            tours[2].UpdateRating(4.8, 145);

            await context.Tours.AddRangeAsync(tours);
            await context.SaveChangesAsync();
        }

        // News
        if (!await context.News.AnyAsync())
        {
            var news = new List<NewsArticle>([])
            {
                new NewsArticle("2024 Yilinin En Populer Tatil Destinasyonlari", "Bu yil tatilcilerin en cok tercih ettigi destinasyonlar belli oldu", "2024 yilinda tatilcilerin en cok ilgi gosterdigi destinasyonlar arasinda Kapadokya, Pamukkale ve Ege kiyilari on plana cikti. Ozellikle bahar aylarinda bu bolgelere olan talep yuzde 40 artti.", "Seyahat", DateTime.Now.AddDays(-5), "Ayse Yilmaz", "/images/news/popular-destinations.jpg", ["Tatil", "Destinasyon", "Turizm"]),
                new NewsArticle("Havayolu Sirketleri Yaz Tarifesini Acikladi", "Yaz sezonunda yeni rotalar ve artan sefer sayilari", "Turk havayolu sirketleri 2024 yaz sezonunda 25 yeni rota ekliyor. Ozellikle Avrupa ve Orta Dogu destinasyonlarina sefer sayisi artirildi.", "Havacilik", DateTime.Now.AddDays(-3), "Mehmet Kaya", "/images/news/airline-schedule.jpg", ["Havayolu", "Yaz Sezonu", "Ucus"]),
                new NewsArticle("Arac Kiralama Sektorunde Yeni Trendler", "Elektrikli arac kiralama talebi artiyor", "Turkiye'de arac kiralama sektorunde elektrikli ve hibrit araclara olan talep son bir yilda ikiye katlandi. Sirketler filosunu genisletiyor.", "Otomotiv", DateTime.Now.AddDays(-1), "Zeynep Demir", "/images/news/car-rental-trends.jpg", ["Arac Kiralama", "Elektrikli Arac", "Trend"])
            };

            // Tum haberleri yayinla
            foreach (var article in news)
            {
                article.Publish();
            }

            await context.News.AddRangeAsync(news);
            await context.SaveChangesAsync();
        }
    }
}
