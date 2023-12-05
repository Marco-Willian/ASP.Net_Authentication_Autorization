using AutoMapper;
using JwtAuth.Data;
using JwtAuth.Data.Dtos;
using JwtAuth.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers;

// recevies a request from the client and returns a response
[ApiController]
[Route("[controller]")]
public class MovieController : ControllerBase
{
    //We using Mapper because we dont want to expose our entities to the client

    private MovieContext _context;
    private IMapper _mapper;

    public MovieController(MovieContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    //Exemple of documentation

    /// <summary>
    /// Add a movie to the database
    /// </summary>
    /// <param name="movie">Objet with nedeed filds to creat a movie</param>
    /// <returns>IActionResult</returns>
    /// <reponse code="201">Returns the newly created item</reponse>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AddMovie([FromBody] CreateMovieDto movie)
    {
        Movie movieToMap = _mapper.Map<Movie>(movie);
        _context.Movies.Add(movieToMap);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetMovieById), new { id = movieToMap.Id }, movieToMap);
    }

    // return with pagination

    [HttpGet]
    public IEnumerable<ReadMovieDto> GetMovies([FromQuery] int skip = 0, [FromQuery] int take = 25)
    {
        return _mapper.Map<List<ReadMovieDto>>(_context.Movies.Skip(skip).Take(take));
    }

    // return a single movie by id

    [HttpGet("{id}")]
    public IActionResult GetMovieById(int id)
    {
        var filmeToGet = _context.Movies.FirstOrDefault(movie => movie.Id == id);
        if (filmeToGet == null)
        {
            return NotFound();
        }
        var movieToReturn = _mapper.Map<UpdateMovieDto>(filmeToGet);
        return Ok(movieToReturn);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateMovie(int id, [FromBody] UpdateMovieDto movie)
    {
        var movieToUpdate = _context.Movies.FirstOrDefault(movie => movie.Id == id);
        if (movieToUpdate == null)
        {
            return NotFound();
        }
        _mapper.Map(movie, movieToUpdate);
        _context.SaveChanges();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public IActionResult PatchMovie(int id, [FromBody] JsonPatchDocument<UpdateMovieDto> patchMovie)
    {
        var movieToUpdate = _context.Movies.FirstOrDefault(movie => movie.Id == id);
        if (movieToUpdate == null)
        {
            return NotFound();
        }
        var movieToPatch = _mapper.Map<UpdateMovieDto>(movieToUpdate);
        patchMovie.ApplyTo(movieToPatch, ModelState);
        if (!TryValidateModel(movieToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _mapper.Map(movieToPatch, movieToUpdate);
        _context.SaveChanges();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public IActionResult DeleteMovie(int id)
    {
        var movieToDelete = _context.Movies.FirstOrDefault(movie => movie.Id == id);
        if (movieToDelete == null)
        {
            return NotFound();
        }
        _context.Movies.Remove(movieToDelete);
        _context.SaveChanges();
        return NoContent();
    }
}
