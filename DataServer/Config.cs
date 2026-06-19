namespace DataServer;

public class KestrelEndpoint {
    public string Url { get; set; } = "";
    public string Protocols { get; set; } = "Http1";
    public bool UseH2C { get; set; } = false;
    public KestrelCertificate? Certificate { get; set; }
}

public class KestrelCertificate {
    public string Path { get; set; } = "";
    public string Password { get; set; } = "";
}
public class ApplicationConfiguration
{

}
