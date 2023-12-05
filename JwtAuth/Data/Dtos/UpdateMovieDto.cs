using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Data.Dtos
{
    public class UpdateMovieDto
    {
        [Required(ErrorMessage = "Title missing")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Genre missing")]
        public string Genre { get; set; }
        [Required(ErrorMessage = "Time missing")]
        [Range(1, 500, ErrorMessage = "Time must be between 1 and 500")]
        public int Time { get; set; }
    }
}
