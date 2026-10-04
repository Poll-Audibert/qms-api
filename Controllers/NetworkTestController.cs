using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkTestController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            try
            {
                using var client = new TcpClient();

                var connection = client.ConnectAsync(
                    "db71501.public.databaseasp.net",
                    1433
                );

                var completed = await Task.WhenAny(
                    connection,
                    Task.Delay(10000)
                );

                if (completed != connection)
                {
                    return Ok(new
                    {
                        dns = true,
                        tcp = false,
                        message = "TCP connection timed out"
                    });
                }

                await connection;

                return Ok(new
                {
                    dns = true,
                    tcp = true,
                    message = "TCP connection successful"
                });
            }
            catch (SocketException ex)
            {
                return Ok(new
                {
                    dns = true,
                    tcp = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    tcp = false,
                    message = ex.Message
                });
            }
        }
    }
}