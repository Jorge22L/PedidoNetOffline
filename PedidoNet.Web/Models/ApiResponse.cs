namespace PedidoNet.Web.Models
{
    // @TODO: Revisar si puede ser compartido
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
