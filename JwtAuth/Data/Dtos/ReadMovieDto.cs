using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Data.Dtos
{
    public class ReadMovieDto
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Time { get; set; }
        public DateTime ConsultedAt { get; set; } = DateTime.Now;
    }
}
