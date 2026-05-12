namespace _02.CLIMOV.Servicio
{
    public interface IRestService
    {
        Task<double> ConvertirAsync(string type, double value);
        Task<bool> ValidarConexionAsync();
    }
}