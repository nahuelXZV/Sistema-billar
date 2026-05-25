using Domain.DTOs.Contact;

namespace WebClient.Models.Contact;

public class ClienteViewModel : MainViewModel
{
    public ClienteDTO Cliente { get; set; }

    public ClienteViewModel() : base() { }
}
