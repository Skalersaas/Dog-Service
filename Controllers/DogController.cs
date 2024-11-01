using Codebridge.Data;
using Codebridge.Helpers;
using Codebridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Codebridge.Controllers
{
    [EnableRateLimiting("SlidingWindow")]
    public class DogController(ApplicationContext context) : ControllerBase
    {
        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return Ok(new { response = "Dogshouseservice.Version1.0.1" });
        }
        [HttpGet("dogs")]
        public ActionResult GetDogs(string attribute = "id", string order = "asc", int pageNumber = -1, int pageSize = -1)
        {
            attribute = attribute.ToLower();
            order = order.ToLower();

            var validAttributes = new string[] { "id", "name", "color", "weight", "taillength" };
            
            if (!validAttributes.Contains(attribute))
                return BadRequest(new { response = "Wrong attribute" });

            if (order != "asc" && order != "desc")
                return BadRequest(new { response = "Wrong order" });

            var orderedQuery = attribute switch
            {
                "id" => Sorter.OrderBy(context.Dogs, d => d.Id, order),
                "name" => Sorter.OrderBy(context.Dogs, d => d.Name, order),
                "color" => Sorter.OrderBy(context.Dogs, d => d.Color, order),
                "weight" => Sorter.OrderBy(context.Dogs, d => d.Weight, order),
                "taillength" => Sorter.OrderBy(context.Dogs, d => d.TailLength, order),
                _ => throw new NotSupportedException()
            };
            if (pageNumber > 0 && pageSize > 0)
                return Ok(orderedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize));

            return Ok(orderedQuery);
        }
        [HttpPost("dog")]
        public async Task<ActionResult> CreateDog([FromBody] Dog dog)
        {
            if (dog == null)
                return BadRequest(new { response = "Wrong json object was sent" });
            context.Add(dog);
            try
            {
                await context.SaveChangesAsync();
            }
            catch
            {
                return Conflict(new { response = "Wrong data sent" });
            }
            return Ok(dog);
        }

    }
}
