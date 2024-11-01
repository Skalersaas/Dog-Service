# Dog-Service
 Api for controlling dogs information

## Features
1. Rate Limitter: No more than 10 requests per second
1. Pagination and/or Sorting: You can sort by any attribute in any order and retrieve them
1. Creating: You can add any dog you want, but its info should be valid:
	1. Name should not be empty or null
	1. Weight should be positive
	1. Tail Length should be positive

## Technologies used
1. ASP.NET, Entity  Framework
1. SwaggerUI
1. PostgresSql

## Packages Installed
1. Microsoft.EntityFrameworkCore
1. Npgsql.EntityFrameworkCore.PostgreSQL
1. Swashbuckle.AspNetCore

## Testing
4 unit tests were created
1. Ping -> pass if server is working
1. Create -> pass if all test cases were processed correctly
1. Get -> pass if retrieved data from the server was correct according to sorting and pagination
1. RateLimit -> pass if only 10 of selected requests per second were processed

### Before using
1. Set your Connection String in appsettings.json

That's all, just run the app!!
