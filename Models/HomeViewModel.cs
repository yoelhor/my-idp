namespace my_idp.Models;

public class HomeViewModel
{
    public string? email { get; set; }
    public string? name { get; set; }
    public string? given_name { get; set; }
    public string? family_name { get; set; }
    public string? phone_number { get; set; }
    public string? locality { get; set; }
    public string? country { get; set; }

    public string? state { get; set; }
    public string? scope { get; set; }
    public string? client_id { get; set; }
    public string? redirect_uri { get; set; }
}
