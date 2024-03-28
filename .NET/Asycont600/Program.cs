using System.Net.Sockets;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Text;



String deviceIpAddress = "10.0.0.20";
Int32  devicePort      = 4000;

Dictionary<String, String> moves = new()
{
  { "abs", "MoveAbs" },
  { "rel", "MoveRel" }
};

Dictionary<String, String> axes = new()
{
  { "x", "1" },
  { "y", "2" },
  { "z", "3" },
  { "pol", "4" },
  { "slide", "5" },
  { "azimuth", "6" },
};

Dictionary<String, String> axesSafeAcceleration = new()
{
  { "x", "0.05" },
  { "y", "0.1" },
  { "z", "0.1" },
  { "pol", "10" },
  { "slide", "0.01" },
  { "azimuth", "1" },
};

Dictionary<String, String> axesSafeSpeed = new()
{
  { "x", "0.1" },
  { "y", "0.2" },
  { "z", "0.02" },
  { "pol", "10" },
  { "slide", "0.01" },
  { "azimuth", "1" },
};

// TcpClient deviceClient;
// NetworkStream netStream;

try
{

	// Prefer a using declaration to ensure the instance is Disposed later.
	TcpClient deviceClient = new(deviceIpAddress, devicePort);

	// Get a client stream for reading and writing.
	NetworkStream netStream = deviceClient.GetStream();
  // deviceClient.ReceiveTimeout = 1000;

	var builder = WebApplication.CreateBuilder(args);
	var app = builder.Build();

	app.MapGet("/", () => {
		// // Buffer to store the response bytes.
		// Byte[] data = new Byte[256];

		// // String to store the response ASCII representation.
		// String responseData = String.Empty;

		// // Read the first batch of the TcpServer response bytes.
		// Int32 bytes = netStream.Read(data, 0, data.Length);
		// responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
		return "Hello World!"; 
	});
	app.MapPost("/", () => "Hello World!");

  app.MapPut("/move/{move}/{axis}/{pos}", (String axis, String move, String pos) =>
  {

    if (!axes.TryGetValue(axis, out string? value))
    {
      return "Axis not found.";
    }

    XmlDocument doc = new XmlDocument();

    if (move == "relative")
    {
      XmlElement state   = doc.CreateElement("state");
      XmlElement section = doc.CreateElement("section");
      XmlElement query   = doc.CreateElement("query");
      query.SetAttribute("name", "Axis Position");
      section.SetAttribute("name", "Axis " + axes[axis]);
      section.AppendChild(query);
      state.AppendChild(section);
      doc.AppendChild(state);
      Console.WriteLine(doc.OuterXml.ToString());
      netStream.Write(Encoding.ASCII.GetBytes(doc.OuterXml.ToString()));

      Byte[] buffer     = new Byte[1024];
      int bytesReceived = netStream.Read(buffer);
      string readData   = Encoding.UTF8.GetString(buffer.AsSpan(0, bytesReceived));
      Console.WriteLine($"{readData}");

      doc.RemoveAll();
      doc.LoadXml(readData);
      XmlNode entry = doc.SelectSingleNode("//state//section//entry") ?? doc.CreateElement("error");
      if (entry.Attributes is not null && entry.Attributes["v1"] is not null &&  entry.Attributes["v1"].InnerText is not null) {
        // Console.WriteLine(Convert.ToSingle(entry.Attributes["v1"].InnerText));
        Single newPos = Convert.ToSingle(entry.Attributes["v1"].InnerText) + Convert.ToSingle(pos);
        pos = Convert.ToString(newPos); 
        // Console.WriteLine($"New position {pos}");
      }

    }

    doc.RemoveAll();
    XmlElement el = doc.CreateElement("command");

    el.SetAttribute("name", "MoveAbs");
    el.SetAttribute("axis", value);
    el.SetAttribute("Acceleration", axesSafeAcceleration[axis]);
    el.SetAttribute("Deceleration", axesSafeAcceleration[axis]);
    el.SetAttribute("Velocity", axesSafeSpeed[axis]);
    el.SetAttribute("Direction", "Auto");
    el.SetAttribute("Position", pos);

    doc.AppendChild(el);
    Console.WriteLine(doc.OuterXml.ToString());

    netStream.Write(Encoding.ASCII.GetBytes(doc.OuterXml.ToString()));

    return doc.OuterXml.ToString();
  });

	app.MapDelete("/", () => "Hello World!");

	app.Run();
}
catch (ArgumentNullException e)
{
	Console.WriteLine("ArgumentNullException: {0}", e);
}
catch (SocketException e)
{
	Console.WriteLine("SocketException: {0}", e);
}

