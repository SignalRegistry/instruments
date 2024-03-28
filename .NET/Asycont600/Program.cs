using System.Net.Sockets;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Text;



String deviceIpAddress = "10.0.0.20";
Int32  devicePort      = 4000;

Dictionary<String, String> moves = new()
{
  { "abs", "MoveAbs" },
  { "inc", "MoveInc" }
};

Dictionary<String, String> axes = new()
{
  { "x", "1" },
  { "y", "2" }
};

Dictionary<String, String> axesSafeAcceleration = new()
{
  { "x", "0.05" },
  { "y", "0.1" }
};

Dictionary<String, String> axesSafeSpeed = new()
{
  { "x", "0.1" },
  { "y", "0.2" }
};

// TcpClient deviceClient;
// NetworkStream deviceStream;

try
{

	// // Prefer a using declaration to ensure the instance is Disposed later.
	// TcpClient deviceClient = new TcpClient(deviceIpAddress, devicePort);

	// // Get a client stream for reading and writing.
	// NetworkStream deviceStream = deviceClient.GetStream();

	var builder = WebApplication.CreateBuilder(args);
	var app = builder.Build();

	app.MapGet("/", () => {
		// // Buffer to store the response bytes.
		// Byte[] data = new Byte[256];

		// // String to store the response ASCII representation.
		// String responseData = String.Empty;

		// // Read the first batch of the TcpServer response bytes.
		// Int32 bytes = deviceStream.Read(data, 0, data.Length);
		// responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
		return "Hello World!"; 
	});
	app.MapPost("/", () => "Hello World!");

	app.MapPut("/move/{move}/{axis}/{pos}", (String axis, String move, String pos) => {

		if (!axes.TryGetValue(axis, out string? value))
		{
			return "Axis not found.";
		}

		XmlDocument doc = new XmlDocument();
		XmlElement el = doc.CreateElement("command");

		el.SetAttribute("name", moves[move]);
		el.SetAttribute("axis", value);
		el.SetAttribute("Acceleration", axesSafeAcceleration[axis]);
		el.SetAttribute("Deceleration", axesSafeAcceleration[axis]);
		el.SetAttribute("Velocity", axesSafeSpeed[axis]);
		el.SetAttribute("Direction", "Auto");
		el.SetAttribute("Position", pos);

		doc.AppendChild(el);
		Console.WriteLine(doc.OuterXml.ToString());

		//deviceStream.Write(Encoding.ASCII.GetBytes(doc.OuterXml.ToString()));

		return doc.OuterXml.ToString(); 
	} );

	app.MapDelete("/", () => "Hello World!");

	app.Run();

	//deviceStream.Close();
	//deviceClient.Close();

	// Console.WriteLine("Connection Established.");
}
catch (ArgumentNullException e)
{
	Console.WriteLine("ArgumentNullException: {0}", e);
}
catch (SocketException e)
{
	Console.WriteLine("SocketException: {0}", e);
}

