namespace ParkingApi.Domain.Dtos.Common;

public class ServiceResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ServiceResponse<T> Ok(T data, string message = "Operación exitosa") =>
        new() { Success = true, Message = message, Data = data };

    public static ServiceResponse<T> Fail(string message, T? data = default) =>
        new() { Success = false, Message = message, Data = data };
}
