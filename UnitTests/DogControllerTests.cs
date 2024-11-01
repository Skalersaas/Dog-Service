using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace UnitTests
{
    [TestClass]
    public class DogControllerTests
    {
        private const string api = "https://localhost:7184";
        [TestMethod]
        public async Task Ping_Test()
        {
            HttpClient client= new HttpClient();
            string endpointUrl = api + "/ping";

            HttpResponseMessage response = await client.GetAsync(endpointUrl);
            string responseContent = await response.Content.ReadAsStringAsync();

            JsonDocument jsonDoc = JsonDocument.Parse(responseContent);
            JsonElement root = jsonDoc.RootElement;

            var resp = root.GetProperty("response").GetString();
            

            Assert.IsInstanceOfType(resp, typeof(string));
            Assert.IsTrue(resp == "Dogshouseservice.Version1.0.1");
        }
        [TestMethod]
        public async Task GetDogs_Test()
        {
            string endpointUrl = api + "/dogs";
            string[] endpoints =
            {
                endpointUrl,
                endpointUrl + "?attribute=id&order=asc&pageNumber=-1&pageSize=-1",            // 1
                endpointUrl + "?attribute=name&order=desc&pageNumber=2&pageSize=3",           // 2
                endpointUrl + "?attribute=color&order=adssdsc&pageNumber=2&pageSize=3",       // 3
                endpointUrl + "?attribute=tailLength&order=asc&pageNumber=-1&pageSize=3",     // 4
                endpointUrl + "?attribute=weight&order=desc&pageNumber=4&pageSize=-1",        // 5
                endpointUrl + "?attribute=weasdasdasight&order=desc&pageNumber=4&pageSize=-1",// 6
            };
            HttpClient client = new HttpClient();
            int fullCount = 0;
            for (int i = 0; i < endpoints.Length; i++)
            {
                HttpResponseMessage response = await client.GetAsync(endpoints[i]);
                string responseContent = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDoc = JsonDocument.Parse(responseContent);
                JsonElement root = jsonDoc.RootElement;

                if (i == 0)
                {
                    Assert.IsTrue(root.ValueKind == JsonValueKind.Array);
                    fullCount = root.GetArrayLength();
                }
                else if (i == 1)
                {
                    Assert.IsTrue(root.ValueKind == JsonValueKind.Array);
                    Assert.IsTrue(fullCount == root.GetArrayLength());
                    if (fullCount >= 2)
                        Assert.IsTrue(root[0].GetProperty("id").GetInt32() < root[1].GetProperty("id").GetInt32());
                }
                else if (i == 2)
                {
                    Assert.IsTrue(root.ValueKind == JsonValueKind.Array);
                    Assert.IsTrue(root.GetArrayLength() == 3 || fullCount < 2 * 3);
                    if (root.GetArrayLength() >= 2)
                        Assert.IsTrue(root[0].GetProperty("name").GetString()
                            .CompareTo(root[1].GetProperty("name").GetString()) > 0);
                }
                else if (i == 3)
                {
                    Assert.IsTrue(root.GetProperty("response").GetString() == "Wrong order");
                }
                else if (i == 4)
                {
                    Console.WriteLine(root.ToString());
                    Assert.IsTrue(root.ValueKind == JsonValueKind.Array);
                    Assert.IsTrue(fullCount == root.GetArrayLength());
                    if (root.GetArrayLength() >= 2)
                        Assert.IsTrue(root[0].GetProperty("tailLength").GetInt32()
                            .CompareTo(root[1].GetProperty("tailLength").GetInt32()) <= 0);
                }
                else if (i == 5)
                {
                    Assert.IsTrue(root.ValueKind == JsonValueKind.Array);
                    Assert.IsTrue(root.GetArrayLength() == fullCount);
                }
                else
                {
                    Assert.IsTrue(root.GetProperty("response").GetString() == "Wrong attribute");
                }
            }
        }
        [TestMethod]
        public async Task CreateDog_Test()
        {
            string name = "Max 2"; // This should be changed every time!
            string endpointUrl = api + "/dog";
            HttpClient client = new HttpClient();


            HttpResponseMessage response = await client.PostAsync(endpointUrl, new StringContent(
                JsonConvert.SerializeObject(new {  color = "black", weight = 23, tailLength = 32 }),
                Encoding.UTF8,
                "application/json"));
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);

            JsonDocument jsonDoc = JsonDocument.Parse(responseContent);
            JsonElement root = jsonDoc.RootElement;
            Assert.IsTrue(root.GetProperty("response").ToString() == "Null dog or empty name");


            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(new {name = name, color = "black", weight = 23, tailLength = 32} ),
                Encoding.UTF8,
                "application/json");
            response = await client.PostAsync(endpointUrl, jsonContent);
            responseContent = await response.Content.ReadAsStringAsync();

            jsonDoc = JsonDocument.Parse(responseContent);
            root = jsonDoc.RootElement;
            Console.WriteLine(root.ToString());
            Assert.IsTrue(root.GetProperty("name").ToString() == name);
            Assert.IsTrue(root.GetProperty("color").ToString() == "black");
            Assert.IsTrue(root.GetProperty("weight").GetInt32() == 23);
            Assert.IsTrue(root.GetProperty("tailLength").GetInt32() == 32);
            
            
            
            jsonContent = new StringContent(
                JsonConvert.SerializeObject(new { name = name, color = "black", weight = 23, tailLength = 32 }),
                Encoding.UTF8,
                "application/json");
            response = await client.PostAsync(endpointUrl, jsonContent);
            responseContent = await response.Content.ReadAsStringAsync();

            jsonDoc = JsonDocument.Parse(responseContent);
            root = jsonDoc.RootElement;
      
            Assert.IsTrue(root.GetProperty("response").ToString() == "Wrong data sent");
        }
        [TestMethod]
        public async Task RateLimit_Test()
        {
            var tasks = new List<Task>();
            var client = new HttpClient();
            string endpointUrl = api + "/ping";
            int errors = 0, count = 5, permitLimit = 10;

            using var semaphore = new SemaphoreSlim(permitLimit);

            for (int i = 0; i < count; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); 
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(endpointUrl);
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (responseContent == "Too many requests")
                            Interlocked.Increment(ref errors);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            Assert.IsTrue(count <= permitLimit || errors == count - 10);
        }
    }
}