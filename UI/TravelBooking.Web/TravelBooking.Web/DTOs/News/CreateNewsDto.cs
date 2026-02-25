using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.DTOs.News;

public class CreateNewsDto
{
    [Required(ErrorMessage = "Baslik gereklidir")]
    [StringLength(200, ErrorMessage = "Baslik en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ozet gereklidir")]
    [StringLength(500, ErrorMessage = "Ozet en fazla 500 karakter olabilir")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Icerik gereklidir")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori gereklidir")]
    [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yayin tarihi gereklidir")]
    [DataType(DataType.DateTime)]
    public DateTime PublishDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Yazar gereklidir")]
    [StringLength(100, ErrorMessage = "Yazar adi en fazla 100 karakter olabilir")]
    public string Author { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();

    public bool IsPublished { get; set; }
}
