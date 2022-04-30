// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using BackendTest.Dtos;
// using BackendTest.Repository;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
//
// namespace BackendTest.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class SearchController : BaseController
//     {
//         private readonly IMovieRepo _movieRepo;
//
//         public SearchController(IMovieRepo movieRepo)
//         {
//             _movieRepo = movieRepo;
//         }
//         
//         [HttpGet]
//         public async Task<IActionResult> SearchMovies([FromBody] SearchParamsDto searchParams)
//         {
//             try
//             {
//                 var movies = await _movieRepo.FindMovies(searchParams);
//
//                 return Ok(movies);
//             }
//             catch (Exception exception)
//             {
//                 return Problem(exception.Message);
//             }
//         }
//     }
// }
