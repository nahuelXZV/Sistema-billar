namespace Domain.DTOs.Shared;

public class SelectOptionDTO<T>
{
    public T Value { get; set; }
    public string Label { get; set; }
}
